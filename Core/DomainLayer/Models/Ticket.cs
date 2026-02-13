using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.Models
{
    public class Ticket
    {
        public int Id { get; set; }

        public decimal Price { get; set; }

        public string TicketCode { get; set; }

        public int EventId { get; set; }
        public Event Event { get; set; }

        public int SeatId { get; set; }
        public Seat Seat { get; set; }

        public int UserId { get; set; }
    }
}
