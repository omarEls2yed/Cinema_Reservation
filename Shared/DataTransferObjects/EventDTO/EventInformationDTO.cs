using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DataTransferObjects.EventDTO
{
    public class EventInformationDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }        
        public DateTime EventDate { get; set; } 

        public int VenueId { get; set; }
        public string VenueName { get; set; }  
        public string VenueLocation { get; set; }

        public bool IsActive { get; set; }
    }
}
