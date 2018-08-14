using SimpleIdentityServer.AccountFilter.Basic.Client.Operations;
using SimpleIdentityServer.AccountFilter.Basic.Client.Results;
using SimpleIdentityServer.AccountFilter.Basic.Common.Requests;
using SimpleIdentityServer.Common.Client;
using System.Threading.Tasks;

namespace SimpleIdentityServer.AccountFilter.Basic.Client
{
    public interface IFilterClient
    {
        Task<AddFilterResult> Add(string requestUrl, AddFilterRequest addFilterRequest, string authorizationHeaderValue = null);
        Task<BaseResponse> Delete(string requestUrl, string filterId, string authorizationHeaderValue = null);
        Task<GetAllFiltersResult> GetAll(string requestUrl, string authorizationHeaderValue = null);
        Task<BaseResponse> Update(string requestUrl, UpdateFilterRequest updateFilterRequest, string authorizationHeaderValue = null);
        Task<GetFilterResult> Get(string requestUrl, string filterId, string authorizationHeaderValue = null);
    }

    internal sealed class FilterClient : IFilterClient
    {
        private readonly IAddFilterOperation _addFilterOperation;
        private readonly IDeleteFilterOperation _deleteFilterOperation;
        private readonly IGetAllFiltersOperation _getAllFiltersOperation;
        private readonly IUpdateFilterOperation _updateFilterOperation;
        private readonly IGetFilterOperation _getFilterOperation;

        public FilterClient(IAddFilterOperation addFilterOperation, IDeleteFilterOperation deleteFilterOperation, IGetAllFiltersOperation getAllFiltersOperation,
            IUpdateFilterOperation updateFilterOperation, IGetFilterOperation getFilterOperation)
        {
            _addFilterOperation = addFilterOperation;
            _deleteFilterOperation = deleteFilterOperation;
            _getAllFiltersOperation = getAllFiltersOperation;
            _updateFilterOperation = updateFilterOperation;
            _getFilterOperation = getFilterOperation;
        }

        public Task<AddFilterResult> Add(string requestUrl, AddFilterRequest addFilterRequest, string authorizationHeaderValue = null)
        {
            return _addFilterOperation.Execute(requestUrl, addFilterRequest, authorizationHeaderValue);
        }

        public Task<BaseResponse> Delete(string requestUrl, string filterId, string authorizationHeaderValue = null)
        {
            return _deleteFilterOperation.Execute(requestUrl, filterId, authorizationHeaderValue);
        }

        public Task<GetAllFiltersResult> GetAll(string requestUrl,string authorizationHeaderValue = null)
        {
            return _getAllFiltersOperation.Execute(requestUrl, authorizationHeaderValue);
        }

        public Task<BaseResponse> Update(string requestUrl, UpdateFilterRequest updateFilterRequest, string authorizationHeaderValue = null)
        {
            return _updateFilterOperation.Execute(requestUrl, updateFilterRequest, authorizationHeaderValue);
        }

        public Task<GetFilterResult> Get(string requestUrl, string filterId, string authorizationHeaderValue = null)
        {
            return _getFilterOperation.Execute(requestUrl, filterId, authorizationHeaderValue);
        }
    }
}
