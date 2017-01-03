#region copyright
// Copyright 2017 Habart Thierry
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

using Newtonsoft.Json;
using SimpleIdentityServer.Uma.Common.DTOs;
using SimpleIdentityServer.Uma.Core.Errors;
using SimpleIdentityServer.Uma.Core.Exceptions;
using SimpleIdentityServer.Uma.Core.Extensions;
using SimpleIdentityServer.Uma.Core.Repositories;
using SimpleIdentityServer.Uma.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Uma.Core.Api.IntrospectionController.Actions
{
    public interface IGetIntrospectAction
    {
        Task<IntrospectionResponse> Execute(string rpt);
        Task<IEnumerable<IntrospectionResponse>> Execute(IEnumerable<string> rpts);
    }

    internal class GetIntrospectAction : IGetIntrospectAction
    {
        private readonly IRptRepository _rptRepository;
        private readonly ITicketRepository _ticketRepository;
        private readonly IUmaServerEventSource _umaServerEventSource;

        public GetIntrospectAction(
            IRptRepository rptRepository,
            ITicketRepository ticketRepository,
            IUmaServerEventSource umaServerEventSource)
        {
            _rptRepository = rptRepository;
            _ticketRepository = ticketRepository;
            _umaServerEventSource = umaServerEventSource;
        }

        public async Task<IntrospectionResponse> Execute(string rpt)
        {
            if (string.IsNullOrWhiteSpace(rpt))
            {
                throw new ArgumentNullException(nameof(rpt));
            }

            return (await Execute(new[] { rpt })).First();
        }

        public async Task<IEnumerable<IntrospectionResponse>> Execute(IEnumerable<string> rpts)
        {
            if (rpts == null || !rpts.Any())
            {
                throw new ArgumentNullException(nameof(rpts));
            }

            var concatenatedRpts = string.Join(",", rpts);
            _umaServerEventSource.StartToIntrospect(concatenatedRpts);
            var rptsInformation = await _rptRepository.Get(rpts);
            if (rptsInformation == null || !rptsInformation.Any())
            {
                throw new BaseUmaException(ErrorCodes.InvalidRpt,
                    string.Format(ErrorDescriptions.TheRptsDontExist, concatenatedRpts));
            }

            var tickets = await _ticketRepository.Get(rptsInformation.Select(r => r.TicketId));
            if (tickets == null || !tickets.Any() || tickets.Count() != rptsInformation.Count())
            {
                throw new BaseUmaException(ErrorCodes.InternalError,
                    ErrorDescriptions.AtLeastOneTicketDoesntExist);
            }

            var result = new List<IntrospectionResponse>();
            foreach(var rptInformation in rptsInformation)
            {
                var record = new IntrospectionResponse
                {
                    Expiration = rptInformation.ExpirationDateTime.ConvertToUnixTimestamp(),
                    IssuedAt = rptInformation.CreateDateTime.ConvertToUnixTimestamp()
                };

                var ticket = tickets.First(t => t.Id == rptInformation.TicketId);
                if (rptInformation.ExpirationDateTime < DateTime.UtcNow ||
                    ticket.ExpirationDateTime < DateTime.UtcNow)
                {
                    _umaServerEventSource.RptHasExpired(rptInformation.Value);
                    record.IsActive = false;
                }
                else
                {
                    record.Permissions = new List<PermissionResponse>
                    {
                        new PermissionResponse
                        {
                            ResourceSetId = rptInformation.ResourceSetId,
                            Scopes = ticket.Scopes,
                            Expiration = ticket.ExpirationDateTime.ConvertToUnixTimestamp()
                        }
                    };

                    record.IsActive = true;
                }

                result.Add(record);
            }

            _umaServerEventSource.EndIntrospection(JsonConvert.SerializeObject(result));
            return result;
        }
    }
}
