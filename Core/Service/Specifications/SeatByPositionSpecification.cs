using DomainLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Specifications
{
    public class SeatByPositionSpecification : BaseSpecifications<Seat>
    {
        public SeatByPositionSpecification(int venueId, int row, int number)
            : base(s => s.VenueId == venueId && s.Row == row && s.Number == number)
        {
            AddIncludeExpression(s => s.Tickets);
        }
    }
}
