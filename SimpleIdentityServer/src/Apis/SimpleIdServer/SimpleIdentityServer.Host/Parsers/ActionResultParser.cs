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

using Microsoft.AspNetCore.Routing;
using SimpleIdentityServer.Core.Results;

namespace SimpleIdentityServer.Host.Parsers
{
    public interface IActionResultParser
    {
        ActionInformation GetControllerAndActionFromRedirectionActionResult(ActionResult actionResult);
        RouteValueDictionary GetRedirectionParameters(ActionResult actionResult);
    }

    public class ActionResultParser : IActionResultParser
    {
        private readonly IRedirectInstructionParser _redirectInstructionParser;

        public ActionResultParser(IRedirectInstructionParser redirectInstructionParser)
        {
            _redirectInstructionParser = redirectInstructionParser;
        }

        public ActionInformation GetControllerAndActionFromRedirectionActionResult(ActionResult actionResult)
        {
            if (actionResult.Type != TypeActionResult.RedirectToAction 
                || actionResult.RedirectInstruction == null)
            {
                return null;
            }

            return _redirectInstructionParser.GetActionInformation(actionResult.RedirectInstruction);
        }

        public RouteValueDictionary GetRedirectionParameters(ActionResult actionResult)
        {
            if (actionResult.Type != TypeActionResult.RedirectToAction &&
                actionResult.Type != TypeActionResult.RedirectToCallBackUrl ||
                actionResult.RedirectInstruction == null)
            {
                return null;
            }

            return _redirectInstructionParser.GetRouteValueDictionary(actionResult.RedirectInstruction);
        }
    }
}