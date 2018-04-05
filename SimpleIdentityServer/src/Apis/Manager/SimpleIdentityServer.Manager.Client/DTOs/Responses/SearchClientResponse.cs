using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace SimpleIdentityServer.Manager.Client.DTOs.Responses
{
    public class SearchClientResponse : BaseResponse
    {
        public SearchClientResponse()
        {

        }

        public SearchClientResponse(ErrorResponse error)
        {
            Error = error;
            ContainsError = true;
        }

        public IEnumerable<OpenIdClientResponse> Content { get; set; }
        public int StartIndex { get; set; }
        public int NbResults { get; set; }

        public static SearchClientResponse ToSearchClientResponse(JObject jObj)
        {
            if (jObj == null)
            {
                throw new ArgumentNullException(nameof(jObj));
            }

            var result = new SearchClientResponse();
            JToken jCount;
            JToken jStartIndex;
            JToken jContent;
            if (jObj.TryGetValue(Constants.SearchClientResponseNames.Count, out jCount))
            {
                int count;
                if (int.TryParse(jCount.ToString(), out count))
                {
                    result.NbResults = count;
                }
            }

            if (jObj.TryGetValue(Constants.SearchClientResponseNames.StartIndex, out jStartIndex))
            {
                int startIndex;
                if (int.TryParse(jStartIndex.ToString(), out startIndex))
                {
                    result.StartIndex = startIndex;
                }

            }

            if (jObj.TryGetValue(Constants.SearchClientResponseNames.Content, out jContent))
            {
                var jArrContent = jContent as JArray;
                var clients = new List<OpenIdClientResponse>();
                foreach (JObject o in jArrContent)
                {
                    clients.Add(OpenIdClientResponse.ToClient(o));
                }

                result.Content = clients;
            }

            return result;
        }
    }
}
