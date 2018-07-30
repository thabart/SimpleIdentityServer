using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace SimpleBus.Signalr.Hubs
{
    public class BusHub : Hub
    {
        public async Task BroadCastMessages(string request)
        {
            await Clients.All.SendAsync("Event", request);
        }
    }
}