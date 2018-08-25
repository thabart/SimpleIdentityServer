using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleIdentityServer.Common.Dtos.Responses;
using SimpleIdentityServer.Scim.Mapping.Ad.Common.DTOs.Requests;
using SimpleIdentityServer.Scim.Mapping.Ad.Extensions;
using SimpleIdentityServer.Scim.Mapping.Ad.Models;
using SimpleIdentityServer.Scim.Mapping.Ad.Stores;
using System.Net;

namespace SimpleIdentityServer.Scim.Mapping.Ad.Controllers
{
    [Route(Constants.RouteNames.AdConfigurationController)]
    public class AdConfigurationController : Controller
    {
        private readonly IConfigurationStore _configurationStore;

        public AdConfigurationController(IConfigurationStore configurationStore)
        {
            _configurationStore = configurationStore;
        }

        [HttpGet]
        [Authorize("scim_manage")]
        public IActionResult Get()
        {
            var result = _configurationStore.GetConfiguration();
            if (result == null)
            {
                result = new AdConfiguration { IsEnabled = false };
            }

            return new OkObjectResult(result.ToDto());
        }

        [HttpPut]
        [Authorize("scim_manage")]
        public IActionResult Update([FromBody] UpdateAdConfigurationRequest updateAdConfigurationRequest)
        {
            if (updateAdConfigurationRequest == null)
            {
                return GetError(ErrorCodes.InvalidRequest, ErrorDescriptions.NoRequest, HttpStatusCode.BadRequest);
            }

            var request = updateAdConfigurationRequest.ToModel();
            if(!updateAdConfigurationRequest.IsEnabled)
            {
                _configurationStore.UpdateConfiguration(request);
                return new NoContentResult();
            }

            var actResult = Check(request);
            if (actResult != null)
            {
                return actResult;
            }

            return new NoContentResult();
        }

        private IActionResult Check(AdConfiguration adConfiguration)
        {
            if(string.IsNullOrWhiteSpace(adConfiguration.IpAdr))
            {
                return GetError(ErrorCodes.InvalidRequest, string.Format(ErrorDescriptions.MissingParameter, "ip_adr"), HttpStatusCode.BadRequest);
            }

            IPAddress ipAddress;
            if (!IPAddress.TryParse(adConfiguration.IpAdr, out ipAddress))
            {
                return GetError(ErrorCodes.InvalidRequest, ErrorDescriptions.NotValidIpAddress, HttpStatusCode.BadRequest);
            }

            if (adConfiguration.Port == default(int))
            {
                return GetError(ErrorCodes.InvalidRequest, string.Format(ErrorDescriptions.MissingParameter, "port"), HttpStatusCode.BadRequest);
            }

            if (string.IsNullOrWhiteSpace(adConfiguration.Username))
            {
                return GetError(ErrorCodes.InvalidRequest, string.Format(ErrorDescriptions.MissingParameter, "username"), HttpStatusCode.BadRequest);
            }

            if (string.IsNullOrWhiteSpace(adConfiguration.Password))
            {
                return GetError(ErrorCodes.InvalidRequest, string.Format(ErrorDescriptions.MissingParameter, "password"), HttpStatusCode.BadRequest);
            }

            if (string.IsNullOrWhiteSpace(adConfiguration.DistinguishedName))
            {
                return GetError(ErrorCodes.InvalidRequest, string.Format(ErrorDescriptions.MissingParameter, "distinguished_name"), HttpStatusCode.BadRequest);
            }

            if (string.IsNullOrWhiteSpace(adConfiguration.UserFilter))
            {
                return GetError(ErrorCodes.InvalidRequest, string.Format(ErrorDescriptions.MissingParameter, "user_filter"), HttpStatusCode.BadRequest);
            }

            if (string.IsNullOrWhiteSpace(adConfiguration.UserFilterClass))
            {
                return GetError(ErrorCodes.InvalidRequest, string.Format(ErrorDescriptions.MissingParameter, "user_filter_class"), HttpStatusCode.BadRequest);
            }

            using (var ldapHelper = new LdapHelper())
            {
                if (!ldapHelper.Connect(adConfiguration.IpAdr, adConfiguration.Port, adConfiguration.Username, adConfiguration.Password))
                {
                    return GetError(ErrorCodes.InvalidRequest, ErrorDescriptions.CannotContactTheAd, HttpStatusCode.BadRequest);
                }
            }

            return null;
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