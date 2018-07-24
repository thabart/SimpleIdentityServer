using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Xunit;

namespace SimpleIdentityServer.AccountFilter.Basic.Tests
{
    public class AccountFilterFixture
    {
        [Fact]
        public void When_Pass_Null_Parameter_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            var resourceOwnerFilter = new AccountFilter(new AccountFilterBasicOptions());

            // ACT & ASSERT
            Assert.Throws<ArgumentNullException>(() => resourceOwnerFilter.Check(null));
        }

        [Fact]
        public void When_Claim_Doesnt_Exist_Then_Error_Is_Returned()
        {
            // ARRANGE
            var resourceOwnerFilter = new AccountFilter(new AccountFilterBasicOptions
            {
                Rules = new List<FilterRule>
                {
                    new FilterRule
                    {
                        Comparisons = new List<FilterComparison>
                        {
                            new FilterComparison
                            {
                                ClaimKey = "key",
                                ClaimValue = "val",
                                Operation = ComparisonOperations.Equal
                            }
                        }
                    }
                }
            });

            // ACT
            var result = resourceOwnerFilter.Check(new List<Claim>
            {
                new Claim("keyv", "valv")
            });

            // ASSERTS
            Assert.NotNull(result);
            Assert.False(result.IsValid);
            Assert.True(result.AccountFilterRules.Count() == 1);
            Assert.Equal("the claim 'key' doesn't exist", result.AccountFilterRules.First().ErrorMessages.First());
        }

        [Fact]
        public void When_Filter_Claim_Value_Equal_To_Val_Is_Wrong_Then_Error_Is_Returned()
        {
            // ARRANGE
            var resourceOwnerFilter = new AccountFilter(new AccountFilterBasicOptions
            {
                Rules = new List<FilterRule>
                {
                    new FilterRule
                    {
                        Comparisons = new List<FilterComparison>
                        {
                            new FilterComparison
                            {
                                ClaimKey = "key",
                                ClaimValue = "val",
                                Operation = ComparisonOperations.Equal
                            }
                        }
                    }
                }
            });

            // ACT
            var result = resourceOwnerFilter.Check(new List<Claim>
            {
                new Claim("key", "valv")
            });

            // ASSERTS
            Assert.NotNull(result);
            Assert.False(result.IsValid);
            Assert.True(result.AccountFilterRules.Count() == 1);
            Assert.Equal("the filter claims['key'] == 'val' is wrong", result.AccountFilterRules.First().ErrorMessages.First());
        }

        [Fact]
        public void When_Filter_Claim_Value_Not_Equal_To_Val_Is_Wrong_Then_Error_Is_Returned()
        {
            // ARRANGE
            var resourceOwnerFilter = new AccountFilter(new AccountFilterBasicOptions
            {
                Rules = new List<FilterRule>
                {
                    new FilterRule
                    {
                        Comparisons = new List<FilterComparison>
                        {
                            new FilterComparison
                            {
                                ClaimKey = "key",
                                ClaimValue = "val",
                                Operation = ComparisonOperations.NotEqual
                            }
                        }
                    }
                }
            });

            // ACT
            var result = resourceOwnerFilter.Check(new List<Claim>
            {
                new Claim("key", "val")
            });

            // ASSERTS
            Assert.NotNull(result);
            Assert.False(result.IsValid);
            Assert.True(result.AccountFilterRules.Count() == 1);
            Assert.Equal("the filter claims['key'] != 'val' is wrong", result.AccountFilterRules.First().ErrorMessages.First());
        }

        [Fact]
        public void When_Filter_Claim_Value_Doesnt_Match_Regular_Expression_Is_Wrong_Then_Error_Is_Returned()
        {
            // ARRANGE
            var resourceOwnerFilter = new AccountFilter(new AccountFilterBasicOptions
            {
                Rules = new List<FilterRule>
                {
                    new FilterRule
                    {
                        Comparisons = new List<FilterComparison>
                        {
                            new FilterComparison
                            {
                                ClaimKey = "key",
                                ClaimValue = "^[0-9]{1}$",
                                Operation = ComparisonOperations.RegularExpression
                            }
                        }
                    }
                }
            });

            // ACT
            var result = resourceOwnerFilter.Check(new List<Claim>
            {
                new Claim("key", "111")
            });

            // ASSERTS
            Assert.NotNull(result);
            Assert.False(result.IsValid);
            Assert.True(result.AccountFilterRules.Count() == 1);
            Assert.Equal("the filter claims['key'] match regular expression ^[0-9]{1}$ is wrong", result.AccountFilterRules.First().ErrorMessages.First());
        }

        [Fact]
        public void When_Filter_Claim_Value_Equal_To_Val_Is_Correct_Then_True_Is_Returned()
        {
            // ARRANGE
            var resourceOwnerFilter = new AccountFilter(new AccountFilterBasicOptions
            {
                Rules = new List<FilterRule>
                {
                    new FilterRule
                    {
                        Comparisons = new List<FilterComparison>
                        {
                            new FilterComparison
                            {
                                ClaimKey = "key",
                                ClaimValue = "val",
                                Operation = ComparisonOperations.Equal
                            }
                        }
                    }
                }
            });

            // ACT
            var result = resourceOwnerFilter.Check(new List<Claim>
            {
                new Claim("key", "val")
            });

            // ASSERTS
            Assert.NotNull(result);
            Assert.True(result.IsValid);
        }

        [Fact]
        public void When_Filter_Claim_Value_Equal_To_Val_Is_Correct_And_Filter_Claim_Value_Different_To_Val_Is_Incorrect_Then_True_Is_Returned()
        {
            // ARRANGE
            var resourceOwnerFilter = new AccountFilter(new AccountFilterBasicOptions
            {
                Rules = new List<FilterRule>
                {
                    new FilterRule
                    {
                        Comparisons = new List<FilterComparison>
                        {
                            new FilterComparison
                            {
                                ClaimKey = "key",
                                ClaimValue = "val",
                                Operation = ComparisonOperations.Equal
                            }
                        }
                    },
                    new FilterRule
                    {
                        Comparisons = new List<FilterComparison>
                        {
                            new FilterComparison
                            {
                                ClaimKey = "key",
                                ClaimValue = "val",
                                Operation = ComparisonOperations.NotEqual
                            }
                        }
                    }
                }
            });

            // ACT
            var result = resourceOwnerFilter.Check(new List<Claim>
            {
                new Claim("key", "val")
            });

            // ASSERTS
            Assert.NotNull(result);
            Assert.True(result.IsValid);
        }
    }
}
