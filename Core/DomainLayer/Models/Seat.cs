using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.Models
{
    public class Seat
    {
        public int Id { get; set; }

        public int Row { get; set; }  
        
        public int Number { get; set; }  
        
        public string Class { get; set; }

        public int VenueId { get; set; }
        public Venue Venue { get; set; }

        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    }
}
