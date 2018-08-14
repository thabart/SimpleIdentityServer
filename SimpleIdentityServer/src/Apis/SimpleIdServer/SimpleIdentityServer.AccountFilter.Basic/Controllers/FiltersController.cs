using Microsoft.AspNetCore.Mvc;
using SimpleIdentityServer.AccountFilter.Basic.Common.Requests;
using SimpleIdentityServer.AccountFilter.Basic.Repositories;
using System.Threading.Tasks;

namespace SimpleIdentityServer.AccountFilter.Basic.Controllers
{
    [Route("filters")]
    public class FilterController : Controller
    {
        private readonly IFilterRepository _filterRepository;

        public FilterController(IFilterRepository filterRepository)
        {
            _filterRepository = filterRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetFilters()
        {
            return null;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetFilter(string id)
        {
            return null;
        }

        [HttpPost]
        public async Task<IActionResult> AddFilter([FromBody] AddFilterRequest addFilterRequest)
        {
            _filterRepository.GetAll();
            return null;
        } 

        [HttpPut]
        public async Task<IActionResult> UpdateFilter()
        {
            return null;
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteFilter()
        {
            return null;
        }
    }
}
