using DomainLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Specifications
{
    public class TicketExistenceSpecification : BaseSpecifications<Ticket>
    {
        public TicketExistenceSpecification(int eventId, int seatId)
            : base(t => t.EventId == eventId && t.SeatId == seatId)
        {
        }
    }
}
