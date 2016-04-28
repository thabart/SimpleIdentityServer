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

using SimpleIdentityServer.Uma.Core.Errors;
using SimpleIdentityServer.Uma.Core.Exceptions;
using SimpleIdentityServer.Uma.Core.Extensions;
using SimpleIdentityServer.Uma.Core.Repositories;
using SimpleIdentityServer.Uma.Core.Responses;
using System;
using System.Collections.Generic;

namespace SimpleIdentityServer.Uma.Core.Api.IntrospectionController.Actions
{
    public interface IGetIntrospectAction
    {
        IntrospectionResponse Execute(string rpt);
    }

    internal class GetIntrospectAction : IGetIntrospectAction
    {
        private readonly IRptRepository _rptRepository;

        private readonly ITicketRepository _ticketRepository;

        #region Constructor

        public GetIntrospectAction(
            IRptRepository rptRepository,
            ITicketRepository ticketRepository)
        {
            _rptRepository = rptRepository;
            _ticketRepository = ticketRepository;
        }

        #endregion

        #region Public methods

        public IntrospectionResponse Execute(string rpt)
        {
            if (string.IsNullOrWhiteSpace(rpt))
            {
                throw new ArgumentNullException(nameof(rpt));
            }

            var rptInformation = _rptRepository.GetRpt(rpt);
            if (rptInformation == null)
            {
                throw new BaseUmaException(ErrorCodes.InvalidRpt,
                    string.Format(ErrorDescriptions.TheRptDoesntExist, rpt));
            }

            var ticket = _ticketRepository.GetTicketById(rptInformation.TicketId);
            if (ticket == null)
            {
                throw new BaseUmaException(ErrorCodes.InternalError,
                    string.Format(ErrorDescriptions.TheTicketDoesntExist, rptInformation.TicketId));
            }

            var result = new IntrospectionResponse
            {
                Expiration = rptInformation.ExpirationDateTime.ConvertToUnixTimestamp(),
                IssuedAt = rptInformation.CreateDateTime.ConvertToUnixTimestamp()
            };

            if (rptInformation.ExpirationDateTime < DateTime.UtcNow ||
                ticket.ExpirationDateTime < DateTime.UtcNow)
            {
                result.IsActive = false;
                return result;
            }

            result.Permissions = new List<PermissionResponse>
            {
                new PermissionResponse
                {
                    ResourceSetId = rptInformation.ResourceSetId,
                    Scopes = ticket.Scopes,
                    Expiration = ticket.ExpirationDateTime.ConvertToUnixTimestamp()
                }
            };

            result.IsActive = true;
            return result;
        }

        #endregion
    }
}
