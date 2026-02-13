using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DataTransferObjects.EventDTO
{
    public class UpdateEventRequestDTO
    {
        public int Id { get; set; }

        public string NewName { get; set; }

        public DateTime NewEventDate { get; set; }

        public int NewVenueId { get; set; }
    }
}
