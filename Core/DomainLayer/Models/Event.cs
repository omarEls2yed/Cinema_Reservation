using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.Models
{
    public class Event
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public DateTime EventDate { get; set; }

        public bool IsActive { get; set; }

        public int VenueId { get; set; }
        public Venue Venue { get; set; }

        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    }
}
