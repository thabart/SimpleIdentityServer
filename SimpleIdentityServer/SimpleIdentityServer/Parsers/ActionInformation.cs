using System.Web.Routing;

namespace SimpleIdentityServer.Api.Parsers
{
    public class ActionInformation
    {
        public ActionInformation()
        {
        }

        public ActionInformation(string controllerName, string actionName)
        {
            ControllerName = controllerName;
            ActionName = actionName;
            IsCallBackUrl = false;
        }

        public string ControllerName { get; set; }

        public string ActionName { get; set; }

        public bool IsCallBackUrl { get; set; }

        public RouteValueDictionary RouteValueDictionary { get; set; }
    }
}