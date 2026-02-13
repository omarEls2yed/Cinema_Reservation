using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DataTransferObjects.EventDTO
{
    public class CreateEventRequestDTO
    {
        public string Name { get; set; }
        public DateTime EventDate { get; set; }
        public int VenueId { get; set; }
    }
}
