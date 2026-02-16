using DomainLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Specifications
{
    public class SeatRowExistsSpecification : BaseSpecifications<Seat>
    {
        public SeatRowExistsSpecification(int venueId, int row)
            : base(s => s.VenueId == venueId && s.Row == row)
        {
            ApplyPagination(1,0);
        }
    }
}
