using SimpleIdentityServer.Uma.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Uma.Core.Stores
{
    public interface ITicketStore
    {
        Task<bool> AddAsync(IEnumerable<Ticket> tickets);
        Task<bool> AddAsync(Ticket ticket);
        Task<bool> RemoveAsync(string ticketId);
        Task<Ticket> GetAsync(string ticketId);
    }
}
