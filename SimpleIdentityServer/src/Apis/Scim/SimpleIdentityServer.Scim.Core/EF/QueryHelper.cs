using SimpleIdentityServer.Scim.Common.Models;
using SimpleIdentityServer.Scim.Core.EF.Extensions;
using SimpleIdentityServer.Scim.Core.EF.Helpers;
using SimpleIdentityServer.Scim.Core.Parsers;
using System;
using System.Collections.Generic;
using System.Linq;
using Model = SimpleIdentityServer.Scim.Core.EF.Models;

namespace SimpleIdentityServer.Scim.Core.EF
{
    public static class QueryHelper
    {
        public static IQueryable<Model.Representation> SearchRepresentations(IQueryable<Model.Representation> reprs, string resourceType, SearchParameter searchParameter, out int totalResults)
        {
            if (string.IsNullOrWhiteSpace(resourceType))
            {
                throw new ArgumentNullException(nameof(resourceType));
            }

            if (searchParameter == null)
            {
                throw new ArgumentNullException(nameof(searchParameter));
            }

            IQueryable<Model.Representation> representations = reprs
                .Where(r => r.ResourceType == resourceType);
            if (searchParameter.Filter != null)
            {
                var lambdaExpression = searchParameter.Filter.EvaluateFilter(representations);
                representations = (IQueryable<Model.Representation>)lambdaExpression.Compile().DynamicInvoke(representations);
            }

            totalResults = representations.Count();
            representations = representations.Skip(searchParameter.StartIndex);
            representations = representations.Take(searchParameter.Count);
            return representations;
        }

        public static IQueryable<Model.RepresentationAttribute> SearchValues(IQueryable<Model.Representation> reprs, IQueryable<Model.RepresentationAttribute> reprsAttrs, string resourceType, Filter filter)
        {
            IQueryable<Model.Representation> representations = reprs.Where(r => r.ResourceType == resourceType);
            IQueryable<Model.RepresentationAttribute> representationAttributes = reprsAttrs;
            var lambdaExpression = filter.EvaluateSelection(representations, representationAttributes);
            var res = (IQueryable<Model.RepresentationAttribute>)lambdaExpression.Compile().DynamicInvoke(representations);
            return res;
        }
    }
}
