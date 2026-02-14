using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public class BookingMessage
    {
        public Guid BookingId { get; set; }
        public int EventId { get; set; }
        public int SeatId { get; set; }
        public int UserId { get; set; }
        public decimal TicketPrice { get; set; }
        public DateTime RequestTime { get; set; }
    }
}
