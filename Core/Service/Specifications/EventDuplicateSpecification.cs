using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DomainLayer.Models;

namespace Service.Specifications
{
    public class EventDuplicateSpecification : BaseSpecifications<Event>
    {
        public EventDuplicateSpecification(string name, int venueId, DateTime date)
            : base(e => e.VenueId == venueId && e.EventDate == date && e.Name == name && e.EventDate > DateTime.Now)
        {
            ApplyPagination(1,0);
        }
    }
}
