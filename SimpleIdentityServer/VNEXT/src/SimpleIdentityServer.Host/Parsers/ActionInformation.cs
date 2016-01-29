using System.Web.Routing;

namespace SimpleIdentityServer.Host.Parsers
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
        }

        public string ControllerName { get; set; }

        public string ActionName { get; set; }

        public RouteValueDictionary RouteValueDictionary { get; set; }
    }
}