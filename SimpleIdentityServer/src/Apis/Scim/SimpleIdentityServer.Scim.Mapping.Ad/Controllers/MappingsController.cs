using Microsoft.AspNetCore.Mvc;
using SimpleIdentityServer.Common.Dtos.Responses;
using SimpleIdentityServer.Scim.Mapping.Ad.Common.DTOs.Requests;
using SimpleIdentityServer.Scim.Mapping.Ad.Extensions;
using SimpleIdentityServer.Scim.Mapping.Ad.Stores;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Scim.Mapping.Ad.Controllers
{
    [Route(Constants.RouteNames.MappingsController)]
    public class MappingsController : Controller
    {
        private readonly IMappingStore _mappingStore;

        public MappingsController(IMappingStore mappingStore)
        {
            _mappingStore = mappingStore;
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] AddMappingRequest addMappingRequest)
        {
            if (addMappingRequest == null)
            {
                return GetError(ErrorCodes.InvalidRequest, ErrorDescriptions.NoRequest, HttpStatusCode.BadRequest);
            }

            var parameter = addMappingRequest.ToModel();
            var mapping = await _mappingStore.GetMapping(addMappingRequest.AttributeId);
            if(mapping != null)
            {
                return GetError(ErrorCodes.InvalidRequest, ErrorDescriptions.MappingAlreadyAssigned, HttpStatusCode.BadRequest);
            }

            if(!await _mappingStore.AddMapping(addMappingRequest.ToModel()))
            {
                return GetError(ErrorCodes.InternalError, ErrorDescriptions.CannotInsertMapping, HttpStatusCode.InternalServerError);
            }

            return new NoContentResult();
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var adMappings = await _mappingStore.GetAll();
            return new OkObjectResult(adMappings.Select(m => m.ToDto()));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            if(string.IsNullOrWhiteSpace(id))
            {
                return GetError(ErrorCodes.InvalidRequest, string.Format(ErrorDescriptions.MissingParameter, "id"));
            }

            var result = await _mappingStore.GetMapping(id);
            if(result == null)
            {
                return GetError(ErrorCodes.InvalidRequest, ErrorDescriptions.MappingDoesntExist, HttpStatusCode.NotFound);
            }

            return new OkObjectResult(result.ToDto());
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Remove(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return GetError(ErrorCodes.InvalidRequest, string.Format(ErrorDescriptions.MissingParameter, "id"));
            }

            var result = await _mappingStore.GetMapping(id);
            if (result == null)
            {
                return GetError(ErrorCodes.InvalidRequest, ErrorDescriptions.MappingDoesntExist, HttpStatusCode.NotFound);
            }

            if(!await _mappingStore.Remove(id))
            {
                return GetError(ErrorCodes.InternalError, ErrorDescriptions.CannotDeleteMapping, HttpStatusCode.InternalServerError);
            }

            return new NoContentResult();
        }

        private static IActionResult GetError(string code, string message, HttpStatusCode httpStatusCode = HttpStatusCode.InternalServerError)
        {
            var error = new ErrorResponse
            {
                Error = code,
                ErrorDescription = message
            };
            return new JsonResult(error)
            {
                StatusCode = (int)httpStatusCode
            };
        }
    }
}