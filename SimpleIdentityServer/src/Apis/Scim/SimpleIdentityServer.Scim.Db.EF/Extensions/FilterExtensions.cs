using SimpleIdentityServer.Scim.Core.Parsers;
using SimpleIdentityServer.Scim.Db.EF.Models;
using System.Collections.Generic;

namespace SimpleIdentityServer.Scim.Db.EF.Extensions
{
    public static class FilterExtensions
    {
        public static void Evaluate(this Filter filter, IEnumerable<Representation> representations)
        {
            if (representations == null)
            {

            }

            filter.Expression.Evaluate(representations);
        }

        public static void Evaluate(this Expression expression, IEnumerable<Representation> representations)
        {
            // TH : CONTINUE TO GENERATE THE TREE VIEW.
        }

        public static void Evaluate(this CompAttributeExpression compAttributeExpression, IEnumerable<Representation> representations)
        {
            var resourceParameter = System.Linq.Expressions.Expression.Parameter(typeof(Core.Models.Representation), "r");
            var resourceAttributes = System.Linq.Expressions.Expression.PropertyOrField(resourceParameter, "Attributes");
            string s = "";
        }
    }
}
