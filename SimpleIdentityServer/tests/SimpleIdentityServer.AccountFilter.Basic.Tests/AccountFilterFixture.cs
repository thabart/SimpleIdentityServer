using Moq;
using SimpleIdentityServer.AccountFilter.Basic.Aggregates;
using SimpleIdentityServer.AccountFilter.Basic.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.AccountFilter.Basic.Tests
{
    public class AccountFilterFixture
    {
        private Mock<IFilterRepository> _filterRepositoryStub;
        private IAccountFilter _accountFilter;

        [Fact]
        public async Task When_Pass_Null_Parameter_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            await Assert.ThrowsAsync<ArgumentNullException>(() => _accountFilter.Check(null)).ConfigureAwait(false);
        }

        [Fact]
        public async Task When_Claim_Doesnt_Exist_Then_Error_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            IEnumerable<FilterAggregate> filters = new List<FilterAggregate>
            {
                new FilterAggregate
                {
                    Rules = new List<FilterAggregateRule>
                    {
                        new FilterAggregateRule
                        {
                            ClaimKey = "key",
                            ClaimValue = "val",
                            Operation = ComparisonOperations.Equal
                        }
                    }
                }
            };
            _filterRepositoryStub.Setup(f => f.GetAll()).Returns(Task.FromResult(filters));

            // ACT
            var result = await _accountFilter.Check(new List<Claim>
            {
                new Claim("keyv", "valv")
            }).ConfigureAwait(false);

            // ASSERTS
            Assert.NotNull(result);
            Assert.False(result.IsValid);
            Assert.True(result.AccountFilterRules.Count() == 1);
            Assert.Equal("the claim 'key' doesn't exist", result.AccountFilterRules.First().ErrorMessages.First());
        }

        [Fact]
        public async Task When_Filter_Claim_Value_Equal_To_Val_Is_Wrong_Then_Error_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            IEnumerable<FilterAggregate> filters = new List<FilterAggregate>
            {
                new FilterAggregate
                {
                    Rules = new List<FilterAggregateRule>
                    {
                        new FilterAggregateRule
                        {
                            ClaimKey = "key",
                            ClaimValue = "val",
                            Operation = ComparisonOperations.Equal
                        }
                    }
                }
            };
            _filterRepositoryStub.Setup(f => f.GetAll()).Returns(Task.FromResult(filters));

            // ACT
            var result = await _accountFilter.Check(new List<Claim>
            {
                new Claim("key", "valv")
            }).ConfigureAwait(false);

            // ASSERTS
            Assert.NotNull(result);
            Assert.False(result.IsValid);
            Assert.True(result.AccountFilterRules.Count() == 1);
            Assert.Equal("the filter claims['key'] == 'val' is wrong", result.AccountFilterRules.First().ErrorMessages.First());
        }

        [Fact]
        public async Task When_Filter_Claim_Value_Not_Equal_To_Val_Is_Wrong_Then_Error_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            IEnumerable<FilterAggregate> filters = new List<FilterAggregate>
            {
                new FilterAggregate
                {
                    Rules = new List<FilterAggregateRule>
                    {
                        new FilterAggregateRule
                        {
                            ClaimKey = "key",
                            ClaimValue = "val",
                            Operation = ComparisonOperations.NotEqual
                        }
                    }
                }
            };
            _filterRepositoryStub.Setup(f => f.GetAll()).Returns(Task.FromResult(filters));

            // ACT
            var result = await _accountFilter.Check(new List<Claim>
            {
                new Claim("key", "val")
            }).ConfigureAwait(false);

            // ASSERTS
            Assert.NotNull(result);
            Assert.False(result.IsValid);
            Assert.True(result.AccountFilterRules.Count() == 1);
            Assert.Equal("the filter claims['key'] != 'val' is wrong", result.AccountFilterRules.First().ErrorMessages.First());
        }

        [Fact]
        public async Task When_Filter_Claim_Value_Doesnt_Match_Regular_Expression_Is_Wrong_Then_Error_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            IEnumerable<FilterAggregate> filters = new List<FilterAggregate>
            {
                new FilterAggregate
                {
                    Rules = new List<FilterAggregateRule>
                    {
                        new FilterAggregateRule
                        {
                            ClaimKey = "key",
                            ClaimValue = "^[0-9]{1}$",
                            Operation = ComparisonOperations.RegularExpression
                        }
                    }
                }
            };
            _filterRepositoryStub.Setup(f => f.GetAll()).Returns(Task.FromResult(filters));

            // ACT
            var result = await _accountFilter.Check(new List<Claim>
            {
                new Claim("key", "111")
            }).ConfigureAwait(false);

            // ASSERTS
            Assert.NotNull(result);
            Assert.False(result.IsValid);
            Assert.True(result.AccountFilterRules.Count() == 1);
            Assert.Equal("the filter claims['key'] match regular expression ^[0-9]{1}$ is wrong", result.AccountFilterRules.First().ErrorMessages.First());
        }

        [Fact]
        public async Task When_Filter_Claim_Value_Equal_To_Val_Is_Correct_Then_True_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            IEnumerable<FilterAggregate> filters = new List<FilterAggregate>
            {
                new FilterAggregate
                {
                    Rules = new List<FilterAggregateRule>
                    {
                        new FilterAggregateRule
                        {
                            ClaimKey = "key",
                            ClaimValue = "val",
                            Operation = ComparisonOperations.Equal
                        }
                    }
                }
            };
            _filterRepositoryStub.Setup(f => f.GetAll()).Returns(Task.FromResult(filters));

            // ACT
            var result = await _accountFilter.Check(new List<Claim>
            {
                new Claim("key", "val")
            }).ConfigureAwait(false);

            // ASSERTS
            Assert.NotNull(result);
            Assert.True(result.IsValid);
        }

        [Fact]
        public async Task When_Filter_Claim_Value_Equal_To_Val_Is_Correct_And_Filter_Claim_Value_Different_To_Val_Is_Incorrect_Then_True_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            IEnumerable<FilterAggregate> filters = new List<FilterAggregate>
            {
                new FilterAggregate
                {
                    Rules = new List<FilterAggregateRule>
                    {
                        new FilterAggregateRule
                        {
                            ClaimKey = "key",
                            ClaimValue = "val",
                            Operation = ComparisonOperations.Equal
                        }
                    }
                },
                new FilterAggregate
                {
                    Rules = new List<FilterAggregateRule>
                    {
                        new FilterAggregateRule
                        {
                            ClaimKey = "key",
                            ClaimValue = "val",
                            Operation = ComparisonOperations.NotEqual
                        }
                    }
                }
            };
            _filterRepositoryStub.Setup(f => f.GetAll()).Returns(Task.FromResult(filters));

            // ACT
            var result = await _accountFilter.Check(new List<Claim>
            {
                new Claim("key", "val")
            }).ConfigureAwait(false);

            // ASSERTS
            Assert.NotNull(result);
            Assert.True(result.IsValid);
        }

        private void InitializeFakeObjects()
        {
            _filterRepositoryStub = new Mock<IFilterRepository>();
            _accountFilter = new AccountFilter(_filterRepositoryStub.Object);
        }
    }
}
