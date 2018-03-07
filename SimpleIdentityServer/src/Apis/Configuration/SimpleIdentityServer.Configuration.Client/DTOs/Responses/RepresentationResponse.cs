using System;
using System.Runtime.Serialization;

namespace SimpleIdentityServer.Configuration.Client.DTOs.Responses
{
    [DataContract]
    public class RepresentationResponse
    {
        [DataMember(Name = Constants.RepresentationResponseNames.Key)]
        public string Key { get; set; }

        [DataMember(Name = Constants.RepresentationResponseNames.AbsoluteExpiration)]
        public DateTimeOffset? AbsoluteExpiration { get; set; }

        [DataMember(Name = Constants.RepresentationResponseNames.SlidingExpiration)]
        public TimeSpan? SlidingExpiration { get; set; }

        [DataMember(Name = Constants.RepresentationResponseNames.Etag)]
        public string Etag { get; set; }

        [DataMember(Name = Constants.RepresentationResponseNames.DateTime)]
        public DateTime DateTime { get; set; }
    }
}
