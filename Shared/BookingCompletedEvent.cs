using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public record BookingCompletedEvent
    {
        public Guid BookingId { get; init; }
        public string UserId { get; set; }
        public bool IsSuccess { get; init; }
        public string? TicketCode { get; init; }
        public string? ErrorMessage { get; init; }
    }
}
