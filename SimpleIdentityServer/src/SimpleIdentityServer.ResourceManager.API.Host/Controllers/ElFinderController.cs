using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using SimpleIdentityServer.ResourceManager.API.Host.DTOs;
using System;

namespace SimpleIdentityServer.ResourceManager.API.Host.Controllers
{
    [Route(Constants.RouteNames.ElFinterController)]
    public class ElFinderController : Controller
    {
        [HttpPost]
        public IActionResult Index([FromBody] JObject json)
        {
            if (json == null)
            {
                throw new ArgumentNullException(nameof(json));
            }

            var deserializedParameter = ElFinderParameter.Deserialize(json);
            if (deserializedParameter.ErrorResponse != null)
            {
                return new OkObjectResult(deserializedParameter.ErrorResponse.GetJson());
            }

            var result = new JObject();
            var elFinderParameter = deserializedParameter.ElFinderParameter;
            if (elFinderParameter.Init)
            {
                result.Add(Constants.ElFinderResponseNames.Api, "2.1");
            }

            var files = new JArray();
            files.Add(FileDirectoryResponse.CreateRootFolder("Demo", "l1_Lw", "l1_").GetJson());
            var opts = new JObject();
            opts.Add("path", "Demo");
            opts.Add("disabled", new JArray(new[] { "chmod" }));
            opts.Add("separator", "/");
            result.Add(Constants.ElFinderResponseNames.UplMaxSize, "0");
            result.Add(Constants.ElFinderResponseNames.Cwd, FileDirectoryResponse.CreateRootFolder("Demo", "l1_Lw", "l1_").GetJson());
            result.Add(Constants.ElFinderResponseNames.Files, files);
            result.Add(Constants.ElFinderResponseNames.Options, opts);
            return new OkObjectResult(result);
        }
    }
}
