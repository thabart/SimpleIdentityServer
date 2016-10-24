#region copyright
// Copyright 2015 Habart Thierry
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using SimpleIdentityServer.Scim.Core.Models;
using SimpleIdentityServer.Scim.Core.Parsers;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SimpleIdentityServer.Scim.Core.Tests.Parsers
{
    public class FilterParserFixture
    {
        private IFilterParser _filterParser;

        #region Tests filter

        [Fact]
        public void When_Passing_Null_Or_Empty_Parameter_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACTS & ASSERTS
            Assert.Throws<ArgumentNullException>(() => _filterParser.Parse(null));
            Assert.Throws<ArgumentNullException>(() => _filterParser.Parse(string.Empty));
        }

        [Fact]
        public void When_Parsing_One_Attribute_Then_One_Attribute_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT 
            var result = _filterParser.Parse("name");

            // ASSERT
            Assert.NotNull(result);
            var filter = result as Filter;
            Assert.NotNull(filter);
            var attr = filter.Expression as AttributeExpression;
            Assert.NotNull(attr);
            Assert.NotNull(attr.Path);
            Assert.True(attr.Path.Name == "name");
        }

        [Fact]
        public void When_Parsing_Three_Attributes_Then_Three_Attributes_Are_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT 
            var result = _filterParser.Parse("name.firstName.firstLetter");

            // ASSERT
            Assert.NotNull(result);
            var filter = result as Filter;
            Assert.NotNull(filter);
            // Check name
            var attr = filter.Expression as AttributeExpression;
            Assert.NotNull(attr);
            Assert.NotNull(attr.Path);
            Assert.True(attr.Path.Name == "name");
            // Check firstName
            var firstName = attr.Path.Next;
            Assert.NotNull(firstName);
            Assert.NotNull(firstName.Name == "firstName");
            // Check firstLetter
            var firstLetter = firstName.Next;
            Assert.NotNull(firstLetter);
            Assert.NotNull(firstLetter.Name == "firstName");
        }

        [Fact]
        public void When_Parsing_Two_Attributes_And_Value_Filter_Then_Two_Attributes_With_Value_Filter_Are_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT 
            var result = _filterParser.Parse("name.firstName[firstLetter eq \"NO\"]");

            // ASSERT
            Assert.NotNull(result);
            var filter = result as Filter;
            Assert.NotNull(filter);
            // Check name
            var attr = filter.Expression as AttributeExpression;
            Assert.NotNull(attr);
            Assert.NotNull(attr.Path);
            Assert.True(attr.Path.Name == "name");
            // Check firstName
            var firstName = attr.Path.Next;
            Assert.NotNull(firstName);
            Assert.NotNull(firstName.Name == "firstName");
            // Check firstLetter
            var valueFilter = firstName.ValueFilter;
            Assert.NotNull(valueFilter);
            var compAttr = valueFilter.Expression as CompAttributeExpression;
            Assert.NotNull(compAttr);
            Assert.True(compAttr.Operator == ComparisonOperators.eq);
            Assert.True(compAttr.Path.Name == "firstLetter");
            Assert.True(compAttr.Value == "\"NO\"");
        }

        [Fact]
        public void When_Parsing_Two_Logical_Attributes_Then_Two_Attributes_Are_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT 
            var result = _filterParser.Parse("firstName eq thierry and lastName eq Habart");

            // ASSERT
            Assert.NotNull(result);
            var filter = result as Filter;
            Assert.NotNull(filter);
            var logicalAttr = filter.Expression as LogicalExpression;
            Assert.NotNull(logicalAttr);
            Assert.True(logicalAttr.Operator == LogicalOperators.and);
            var leftOperand = logicalAttr.AttributeLeft as CompAttributeExpression;
            var rightOperand = logicalAttr.AttributeRight as CompAttributeExpression;
            Assert.NotNull(leftOperand);
            Assert.NotNull(rightOperand);
            Assert.True(leftOperand.Operator == ComparisonOperators.eq);
            Assert.True(rightOperand.Operator == ComparisonOperators.eq);
            Assert.True(leftOperand.Path.Name == "firstName");
            Assert.True(rightOperand.Path.Name == "lastName");
        }

        [Fact]
        public void When_Filtering_Representation_With_Simple_Path_Then_Attribute_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            var representation = new Representation
            {
                Attributes = new[]
                {
                    new ComplexRepresentationAttribute(new SchemaAttributeResponse { Name = "name", Type = Constants.SchemaAttributeTypes.Complex })
                    {
                        Values = new [] {
                            new SingularRepresentationAttribute<string>(new SchemaAttributeResponse { Name = "firstName" }, "thierry")
                        }
                    }
                }
            };
            var result = _filterParser.Parse("name.firstName");

            // ACT 
            var attributes = result.Evaluate(representation);

            // ASSERTS
            Assert.NotNull(attributes);
            Assert.True(attributes.Count() == 1);
            Assert.True(attributes.First().SchemaAttribute.Name == "firstName");
        }

        [Fact]
        public void When_Filtering_Representation_By_FirstName_Equals_To_Thierry_Then_Attribute_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            var representation = new Representation
            {
                Attributes = new[]
                {
                    new ComplexRepresentationAttribute(new SchemaAttributeResponse { Name = "names", Type = Constants.SchemaAttributeTypes.Complex, MultiValued = true })
                    {
                        Values = new [] {
                            new SingularRepresentationAttribute<string>(new SchemaAttributeResponse { Name = "firstName", Type = Constants.SchemaAttributeTypes.String }, "thierry"),
                            new SingularRepresentationAttribute<string>(new SchemaAttributeResponse { Name = "firstName", Type = Constants.SchemaAttributeTypes.String }, "laetitia")
                        }
                    }
                }
            };
            var result = _filterParser.Parse("names[firstName eq thierry]");

            // ACT 
            var attributes = result.Evaluate(representation);

            // ASSERTS
            Assert.NotNull(attributes);
            var names = attributes.First() as ComplexRepresentationAttribute;
            Assert.True(names.SchemaAttribute.Name == "names");
            var name = names.Values;
            Assert.True(name.Count() == 1);
            Assert.True(name.First().SchemaAttribute.Name == "firstName");
        }
        
        [Fact]
        public void When_Filtering_Representation_By_FirstName_Not_Equals_To_Thierry_Then_Two_Attributes_Are_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            var representation = new Representation
            {
                Attributes = new[]
                {
                    new ComplexRepresentationAttribute(new SchemaAttributeResponse { Name = "names", Type = Constants.SchemaAttributeTypes.Complex, MultiValued = true })
                    {
                        Values = new [] {
                            new SingularRepresentationAttribute<string>(new SchemaAttributeResponse { Name = "firstName", Type = Constants.SchemaAttributeTypes.String }, "thierry"),
                            new SingularRepresentationAttribute<string>(new SchemaAttributeResponse { Name = "firstName", Type = Constants.SchemaAttributeTypes.String }, "thierry"),
                            new SingularRepresentationAttribute<string>(new SchemaAttributeResponse { Name = "firstName", Type = Constants.SchemaAttributeTypes.String }, "laetitia"),
                            new SingularRepresentationAttribute<string>(new SchemaAttributeResponse { Name = "firstName", Type = Constants.SchemaAttributeTypes.String }, "loki")
                        }
                    }
                }
            };
            var result = _filterParser.Parse("names[firstName ne thierry]");

            // ACT 
            var attributes = result.Evaluate(representation);

            // ASSERTS
            Assert.NotNull(attributes);
            var names = attributes.First() as ComplexRepresentationAttribute;
            Assert.True(names.SchemaAttribute.Name == "names");
            var name = names.Values;
            Assert.True(name.Count() == 2);
        }

        [Fact]
        public void When_Filtering_Representation_By_Name_Which_Doesnt_Contain_Thierry_Then_No_Attribute_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            var representation = new Representation
            {
                Attributes = new[]
                {
                    new SingularRepresentationAttribute<IEnumerable<string>>(new SchemaAttributeResponse { Name = "names", Type = Constants.SchemaAttributeTypes.String, MultiValued = true }, new []
                    {
                        "loki",
                        "loki",
                        "laetitia",
                        "loki"
                    })
                }
            };
            var result = _filterParser.Parse("names co thierry");

            // ACT 
            var attributes = result.Evaluate(representation);

            // ASSERTS
            Assert.NotNull(attributes);
            Assert.True(attributes.Count() == 0);
        }

        [Fact]
        public void When_Filtering_Representation_By_FirstName_Starts_With_Th_Then_Two_FirstNames_Are_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            var representation = new Representation
            {
                Attributes = new[]
                {
                    new ComplexRepresentationAttribute(new SchemaAttributeResponse { Name = "names", Type = Constants.SchemaAttributeTypes.Complex, MultiValued = true })
                    {
                        Values = new [] {
                            new SingularRepresentationAttribute<string>(new SchemaAttributeResponse { Name = "firstName", Type = Constants.SchemaAttributeTypes.String }, "thierry"),
                            new SingularRepresentationAttribute<string>(new SchemaAttributeResponse { Name = "firstName", Type = Constants.SchemaAttributeTypes.String }, "thRRR"),
                            new SingularRepresentationAttribute<string>(new SchemaAttributeResponse { Name = "firstName", Type = Constants.SchemaAttributeTypes.String }, "laetitia")
                        }
                    }
                }
            };
            var result = _filterParser.Parse("names[firstName sw th]");

            // ACT 
            var attributes = result.Evaluate(representation);

            // ASSERTS
            Assert.NotNull(attributes);
            Assert.True(attributes.Count() == 1);
            var complexAttr = attributes.First() as ComplexRepresentationAttribute;
            Assert.True(complexAttr.Values.Count() == 2);
        }

        [Fact]
        public void When_Filtering_Representation_By_Age_Less_Than_24_Then_One_Attribute_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            var representation = new Representation
            {
                Attributes = new[]
                {
                    new ComplexRepresentationAttribute(new SchemaAttributeResponse { Name = "persons", Type = Constants.SchemaAttributeTypes.Complex, MultiValued = true })
                    {
                        Values = new []
                        {
                            // 23 YO
                            new ComplexRepresentationAttribute(new SchemaAttributeResponse { Name = "person", Type = Constants.SchemaAttributeTypes.Complex })
                            {
                                Values = new []
                                {
                                    new ComplexRepresentationAttribute(new SchemaAttributeResponse { Name = "information", Type = Constants.SchemaAttributeTypes.Complex })
                                    {
                                        Values = new []
                                        {
                                            new SingularRepresentationAttribute<int>(new SchemaAttributeResponse { Name = "age", Type = Constants.SchemaAttributeTypes.Integer }, 23)
                                        }
                                    }
                                }
                            },
                            // 24 YO
                            new ComplexRepresentationAttribute(new SchemaAttributeResponse { Name = "person", Type = Constants.SchemaAttributeTypes.Complex })
                            {
                                Values = new []
                                {
                                    new ComplexRepresentationAttribute(new SchemaAttributeResponse { Name = "information", Type = Constants.SchemaAttributeTypes.Complex })
                                    {
                                        Values = new []
                                        {
                                            new SingularRepresentationAttribute<int>(new SchemaAttributeResponse { Name = "age", Type = Constants.SchemaAttributeTypes.Integer }, 24)
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };
            var result = _filterParser.Parse("persons[person.information.age lt 24]");

            // ACT 
            var attributes = result.Evaluate(representation);

            // ASSERTS
            Assert.NotNull(attributes);
            Assert.True(attributes.Count() == 1);
            var persons = attributes.First() as ComplexRepresentationAttribute;
            Assert.NotNull(persons);
            Assert.True(persons.SchemaAttribute.Name == "persons");
            var person = persons.Values.First() as ComplexRepresentationAttribute;
            Assert.NotNull(person);
            Assert.True(person.SchemaAttribute.Name == "person");
            var information = person.Values.First() as ComplexRepresentationAttribute;
            Assert.NotNull(information);
            Assert.True(information.SchemaAttribute.Name == "information");
            var age = information.Values.First() as SingularRepresentationAttribute<int>;
            Assert.NotNull(age);
            Assert.True(age.SchemaAttribute.Name == "age");
            Assert.True(age.Value == 23);
        }

        [Fact]
        public void When_Filtering_Representation_By_Existing_Adr_Then_Two_Attributes_Are_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            var representation = new Representation
            {
                Attributes = new[]
                {
                    new ComplexRepresentationAttribute(new SchemaAttributeResponse { Name = "persons", Type = Constants.SchemaAttributeTypes.Complex, MultiValued = true })
                    {
                        Values = new []
                        {
                            new ComplexRepresentationAttribute(new SchemaAttributeResponse { Name = "person", Type = Constants.SchemaAttributeTypes.Complex })
                            {
                                Values = new []
                                {
                                    new SingularRepresentationAttribute<string>(new SchemaAttributeResponse { Name = "adr", Type = Constants.SchemaAttributeTypes.String }, "adr1")
                                }
                            },
                            new ComplexRepresentationAttribute(new SchemaAttributeResponse { Name = "person", Type = Constants.SchemaAttributeTypes.Complex })
                            {
                                Values = new []
                                {
                                    new SingularRepresentationAttribute<string>(new SchemaAttributeResponse { Name = "adr", Type = Constants.SchemaAttributeTypes.String }, "adr2")
                                }
                            },
                            new ComplexRepresentationAttribute(new SchemaAttributeResponse { Name = "person", Type = Constants.SchemaAttributeTypes.Complex })
                            {
                                Values = null
                            },
                            new ComplexRepresentationAttribute(new SchemaAttributeResponse { Name = "person", Type = Constants.SchemaAttributeTypes.Complex })
                            {
                                Values = null
                            }
                        }
                    }
                }
            };
            var result = _filterParser.Parse("persons[person.adr pr]");

            // ACT 
            var attributes = result.Evaluate(representation);

            // ASSERTS
            Assert.NotNull(attributes);
            var persons = attributes.First() as ComplexRepresentationAttribute;
            Assert.NotNull(persons);
            var person = persons.Values;
            Assert.True(person.Count() == 2);
        }

        [Fact]
        public void When_Filtering_Representation_By_Existing_Adr_And_Pickup_Only_Addresses_Then_Two_Adrs_Are_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            var representation = new Representation
            {
                Attributes = new[]
                {
                    new ComplexRepresentationAttribute(new SchemaAttributeResponse { Name = "persons", Type = Constants.SchemaAttributeTypes.Complex, MultiValued = true })
                    {
                        Values = new []
                        {
                            new ComplexRepresentationAttribute(new SchemaAttributeResponse { Name = "person", Type = Constants.SchemaAttributeTypes.Complex })
                            {
                                Values = new []
                                {
                                    new SingularRepresentationAttribute<string>(new SchemaAttributeResponse { Name = "adr", Type = Constants.SchemaAttributeTypes.String }, "adr1")
                                }
                            },
                            new ComplexRepresentationAttribute(new SchemaAttributeResponse { Name = "person", Type = Constants.SchemaAttributeTypes.Complex })
                            {
                                Values = new []
                                {
                                    new SingularRepresentationAttribute<string>(new SchemaAttributeResponse { Name = "adr", Type = Constants.SchemaAttributeTypes.String }, "adr2")
                                }
                            },
                            new ComplexRepresentationAttribute(new SchemaAttributeResponse { Name = "person", Type = Constants.SchemaAttributeTypes.Complex })
                            {
                                Values = null
                            },
                            new ComplexRepresentationAttribute(new SchemaAttributeResponse { Name = "person", Type = Constants.SchemaAttributeTypes.Complex })
                            {
                                Values = null
                            }
                        }
                    }
                }
            };
            var result = _filterParser.Parse("persons[person.adr pr].person.adr");

            // ACT 
            var attributes = result.Evaluate(representation);

            // ASSERTS
            Assert.NotNull(attributes);
            Assert.True(attributes.Count() == 2);
            var firstAdr = attributes.First() as SingularRepresentationAttribute<string>;
            var secondAdr = attributes.ElementAt(1) as SingularRepresentationAttribute<string>;
            Assert.True(firstAdr.SchemaAttribute.Name == "adr");
            Assert.True(secondAdr.SchemaAttribute.Name == "adr");
        }

        [Fact]
        public void When_Filtering_Representation_By_FirstName_Equals_To_Thierry_Or_Lokit_Then_Two_Attributes_Are_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            var representation = new Representation
            {
                Attributes = new[]
                {
                    new ComplexRepresentationAttribute(new SchemaAttributeResponse { Name = "name", Type = Constants.SchemaAttributeTypes.Complex })
                    {
                        Values = new [] {
                            new SingularRepresentationAttribute<string>(new SchemaAttributeResponse { Name = "firstName", Type = Constants.SchemaAttributeTypes.String }, "thierry"),
                            new SingularRepresentationAttribute<string>(new SchemaAttributeResponse { Name = "lastName", Type = Constants.SchemaAttributeTypes.String }, "loki")
                        }
                    }
                }
            };
            var result = _filterParser.Parse("(name.firstName eq thierry) or (name.lastName eq lokit)");

            // ACT 
            var attributes = result.Evaluate(representation);

            // ASSERTS
            Assert.NotNull(attributes);
            Assert.True(attributes.Count() == 1);
        }

        [Fact]
        public void When_Filtering_Representation_By_FirstName_Equals_To_Thierry_And_Not_Equals_To_Lokit_Then_Attributes_Are_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            var representation = new Representation
            {
                Attributes = new[]
                {
                    new ComplexRepresentationAttribute(new SchemaAttributeResponse { Name = "name", Type = Constants.SchemaAttributeTypes.Complex })
                    {
                        Values = new [] {
                            new SingularRepresentationAttribute<string>(new SchemaAttributeResponse { Name = "firstName", Type = Constants.SchemaAttributeTypes.String }, "thierry"),
                            new SingularRepresentationAttribute<string>(new SchemaAttributeResponse { Name = "lastName", Type = Constants.SchemaAttributeTypes.String }, "loki")
                        }
                    }
                }
            };
            var result = _filterParser.Parse("(name.firstName eq thierry) and not (name.lastName eq lokit)");

            // ACT 
            var attributes = result.Evaluate(representation);

            // ASSERTS
            Assert.NotNull(attributes);
            Assert.True(attributes.Count() == 1);
        }

        [Fact]
        public void When_Filtering_Representation_By_FirstName_Not_Equals_To_Thierry_Then_Attributes_Are_Returned()
        {
            InitializeFakeObjects();
            var representation = new Representation
            {
                Attributes = new[]
                {
                    new ComplexRepresentationAttribute(new SchemaAttributeResponse { Name = "name", Type = Constants.SchemaAttributeTypes.Complex })
                    {
                        Values = new [] {
                            new SingularRepresentationAttribute<string>(new SchemaAttributeResponse { Name = "firstName", Type = Constants.SchemaAttributeTypes.String }, "laetitia"),
                            new SingularRepresentationAttribute<string>(new SchemaAttributeResponse { Name = "lastName", Type = Constants.SchemaAttributeTypes.String }, "loki")
                        }
                    }
                }
            };
            var result = _filterParser.Parse("not (name.firstName eq thierry) and not (name.lastName eq lokit)");

            // ACT 
            var attributes = result.Evaluate(representation);

            // ASSERTS
            Assert.NotNull(attributes);
            Assert.True(attributes.Count() == 1);
        }

        [Fact]
        public void When_Filtering_Representations_By_Complex_Filter_Then_Attributes_Are_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            var representation = new Representation
            {
                Attributes = new[]
                {
                    new ComplexRepresentationAttribute(new SchemaAttributeResponse { Name = "persons", Type = Constants.SchemaAttributeTypes.Complex, MultiValued = true })
                    {
                        Values = new []
                        {
                            new ComplexRepresentationAttribute(new SchemaAttributeResponse { Name = "person", Type = Constants.SchemaAttributeTypes.Complex })
                            {
                                Values = new RepresentationAttribute[]
                                {
                                    new ComplexRepresentationAttribute(new SchemaAttributeResponse { Name = "name", Type = Constants.SchemaAttributeTypes.Complex })
                                    {
                                        Values = new []
                                        {
                                            new SingularRepresentationAttribute<string>(new SchemaAttributeResponse { Name = "firstName", Type = Constants.SchemaAttributeTypes.String }, "laetitia"),
                                            new SingularRepresentationAttribute<string>(new SchemaAttributeResponse { Name = "lastName", Type = Constants.SchemaAttributeTypes.String }, "loki")
                                        }
                                    },
                                    new SingularRepresentationAttribute<DateTime>(new SchemaAttributeResponse { Name = "birthDate", Type = Constants.SchemaAttributeTypes.DateTime }, DateTime.Parse("2011-05-13T04:42:34Z"))
                                }
                            },
                            new ComplexRepresentationAttribute(new SchemaAttributeResponse { Name = "person", Type = Constants.SchemaAttributeTypes.Complex })
                            {
                                Values = new RepresentationAttribute[]
                                {
                                    new ComplexRepresentationAttribute(new SchemaAttributeResponse { Name = "name", Type = Constants.SchemaAttributeTypes.Complex })
                                    {
                                        Values = new []
                                        {
                                            new SingularRepresentationAttribute<string>(new SchemaAttributeResponse { Name = "firstName", Type = Constants.SchemaAttributeTypes.String }, "thierry"),
                                            new SingularRepresentationAttribute<string>(new SchemaAttributeResponse { Name = "lastName", Type = Constants.SchemaAttributeTypes.String }, "loki")
                                        }
                                    },
                                    new SingularRepresentationAttribute<DateTime>(new SchemaAttributeResponse { Name = "birthDate", Type = Constants.SchemaAttributeTypes.DateTime }, DateTime.Parse("2011-06-13T04:42:34Z"))
                                }
                            }
                        }
                    }
                }
            };
            var result = _filterParser.Parse("persons[person.name.lastName eq loki and person.birthDate le 2011-05-13T04:42:34Z].person.name.firstName");

            // ACT 
            var attributes = result.Evaluate(representation);

            // ASSERTS
            Assert.NotNull(attributes);
            Assert.True(attributes.Count() == 1);
            var firstName = attributes.First() as SingularRepresentationAttribute<string>;
            Assert.NotNull(firstName);
            Assert.True(firstName.Value == "laetitia");
        }

        #endregion

        #region Tests GetTarget

        [Fact]
        public void When_Passing_Null_Or_Empty_Parameter_To_GetTarget_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACTS  & ASSERTS
            Assert.Throws<ArgumentNullException>(() => _filterParser.GetTarget(null));
            Assert.Throws<ArgumentNullException>(() => _filterParser.GetTarget(string.Empty));
        }

        [Fact]
        public void When_Passing_Invalid_Path_To_GetTarget_Then_Empty_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACTS & ASSERTS
            Assert.True(_filterParser.GetTarget("not (name eq thierry)") == string.Empty);
            Assert.True(_filterParser.GetTarget("names[name eq thierry") == string.Empty);
        }

        [Fact]
        public void When_Parsing_SimpleFilter_Then_Target_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACTS & ASSERTS
            Assert.True(_filterParser.GetTarget("names[name eq thierry]") == "names");
            Assert.True(_filterParser.GetTarget("names") == "names");
            Assert.True(string.IsNullOrEmpty(_filterParser.GetTarget("title pr and userType eq Employee")));
            Assert.True(string.IsNullOrEmpty(_filterParser.GetTarget("persons[names co thierry] and userType eq Employee")));
        }

        [Fact]
        public void When_Parsing_ComplexFilter_Then_Target_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT
            var result = _filterParser.GetTarget("persons[names co thierry].adrs[main sw MainStreet]");

            // ASSERT
            Assert.True(result == "persons.adrs");
        }

        #endregion

        private void InitializeFakeObjects()
        {
            _filterParser = new FilterParser();
        }
    }
}
