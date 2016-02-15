using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SwaggerLibrary.Helpers
{
    public class UriParameter
    {
        public UriParameter(string name, object value)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; set; }

        public object Value { get; set; }
    }

    public static class UriExtensions
    {
        public static Uri BuildUri(this Uri target, object filter)
        {
            var uriParameters = BuildUriParameters(filter);
            var uriSegmentParameters = IdentifyUriSegmentParameters(target.OriginalString, uriParameters);
            var uriWithFilledUriSegments = FillUriSegmentParameters(target.OriginalString, uriSegmentParameters);

            var queryStringParameters = uriParameters.Except(uriSegmentParameters);

            var queryString = string.Join("&", queryStringParameters.Select(EnCodeQueryStringParameter));
            var separator = uriWithFilledUriSegments.Contains("?") ? "&" : "?";

            return new Uri(uriWithFilledUriSegments + separator + queryString, target.IsAbsoluteUri ? UriKind.Absolute : UriKind.Relative);
        }

        private static string FillUriSegmentParameters(string uriWithUriSegmentPlaceHolders, IEnumerable<UriParameter> parameters)
        {
            var uriWithFilledUriSegments = uriWithUriSegmentPlaceHolders;

            foreach (var parameter in parameters)
            {
                var evaluatedPlaceHolder = string.Format("{{{0}}}", parameter.Name);
                uriWithFilledUriSegments = Regex.Replace(uriWithFilledUriSegments, evaluatedPlaceHolder, parameter.Value.ToString(),
                    RegexOptions.IgnoreCase);
            }

            return uriWithFilledUriSegments;
        }

        private static IList<UriParameter> IdentifyUriSegmentParameters(string uriWithPlaceHolders, IEnumerable<UriParameter> uriParameters)
        {
            var uriSegmentParameters = new List<UriParameter>();

            foreach (var uriParameter in uriParameters)
            {
                var evaluatedPlaceHolder = string.Format("{{{0}}}", uriParameter.Name);
                if (uriWithPlaceHolders.ToUpper().Contains(evaluatedPlaceHolder.ToUpper()))
                {
                    uriSegmentParameters.Add(uriParameter);
                }
            }

            return uriSegmentParameters;
        }

        private static IList<UriParameter> BuildUriParameters(object parameters, params string[] includedProperties)
        {
            var type = parameters.GetType();
            var properties = type.GetProperties();

            var urlParameters = new List<UriParameter>();

            foreach (var property in properties)
            {
                bool isAllowed = includedProperties.Length == 0 ||
                                 (includedProperties.Length > 0 && includedProperties.Contains(property.Name));

                if (!isAllowed)
                    continue;

                var propType = property.PropertyType;
                var propertyValue = property.GetValue(parameters, null);

                if (propertyValue == null)
                    continue;

                if (propType.IsArray)
                {
                    var elementType = propType.GetElementType();
                    var array = (Array)propertyValue;

                    if (array.Length > 0 && elementType != null
                        && (elementType.IsPrimitive || elementType.IsValueType || elementType == typeof(string)))
                    {
                        foreach (var item in array)
                        {
                            urlParameters.Add(new UriParameter(property.Name, item));
                        }
                    }
                }
                else
                {
                    urlParameters.Add(new UriParameter(property.Name, propertyValue));
                }
            }

            return urlParameters;
        }
        private static string EnCodeQueryStringParameter(UriParameter parameter)
        {
            return string.Format("{0}={1}", HttpUtility.UrlEncode(parameter.Name), HttpUtility.UrlEncode(parameter.Value.ToString()));
        }
    }
}
