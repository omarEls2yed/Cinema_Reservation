using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using DomainLayer.Models;
using Microsoft.EntityFrameworkCore;
using Shared;

namespace Service.Specifications
{
    public class SeatTicketSpecification : BaseSpecifications<Seat>
    {
        public SeatTicketSpecification(SeatQueryStruct queryStruct) : base(seat =>
            (!queryStruct.SeatId.HasValue || seat.Id == queryStruct.SeatId) &&
            (!queryStruct.VenueId.HasValue || seat.VenueId == queryStruct.VenueId) &&
            (!queryStruct.SeatRow.HasValue || seat.Row == queryStruct.SeatRow) &&
            (!queryStruct.SeatNumber.HasValue || seat.Number == queryStruct.SeatNumber) &&
            (string.IsNullOrEmpty(queryStruct.Class) || seat.Class == queryStruct.Class))
        {
            AddCustomQuery(query =>
            {
                 return query.Include(seat => seat.Tickets
                    .Where(ticket =>
                        ticket.EventId == queryStruct.EventId &&
                        ticket.Event.EventDate > DateTime.Now
                    ));
            });
            ApplyPagination(queryStruct.EachPageSize, queryStruct.DesiredPageIndx);
            AddOrderBy(seat => seat.Row); 
            AddThenBy(seat => seat.Number);
        }
    }
}
