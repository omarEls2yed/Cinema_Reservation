using DomainLayer.Models;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Specifications
{
    public class SeatTicketCountSpecification : BaseSpecifications<Seat>
    {
        public SeatTicketCountSpecification(SeatQueryStruct queryStruct) : base(seat =>
            (!queryStruct.SeatId.HasValue || seat.Id == queryStruct.SeatId) &&
            (!queryStruct.VenueId.HasValue || seat.VenueId == queryStruct.VenueId) &&
            (!queryStruct.SeatRow.HasValue || seat.Row == queryStruct.SeatRow) &&
            (!queryStruct.SeatNumber.HasValue || seat.Number == queryStruct.SeatNumber) &&
            (string.IsNullOrEmpty(queryStruct.Class) || seat.Class == queryStruct.Class))
        {
          
        }
    }
}
