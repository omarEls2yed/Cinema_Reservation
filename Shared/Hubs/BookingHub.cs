using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.SignalR;
namespace Shared.Hubs
{
    public class BookingHub : Hub
    {
        public async Task JoinEventGroup(string eventId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Event_{eventId}");
        }
    }
}
