using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Shared;
using Shared.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Consumers
{
    public class BookingNotificationConsumer : IConsumer<BookingCompletedEvent>
    {
        private readonly IHubContext<BookingHub> _hubContext;
        public BookingNotificationConsumer(IHubContext<BookingHub> hubContext)
        {
            _hubContext = hubContext;
        }
        public async Task Consume(ConsumeContext<BookingCompletedEvent> context)
        {
            var msg = context.Message;
            await _hubContext.Clients.
                User(msg.UserId.ToString()).
                SendAsync("Booking result", new {
                    bookingId = msg.BookingId,
                    isSuccess = msg.IsSuccess,
                    ticketCode = msg.TicketCode,
                    errorMessage = msg.ErrorMessage
                });
        }
    }
}
