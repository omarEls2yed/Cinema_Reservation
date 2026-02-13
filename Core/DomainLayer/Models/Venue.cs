using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.Models
{
    public class Venue
    {
        public int Id { get; set; }

        public string Name { get; set; } 

        public string Location { get; set; }
        
        public ICollection<Seat>Seats { get; set; } = new List<Seat>();

        public ICollection<Event> Events { get; set; } = new List<Event>();
    }
}
