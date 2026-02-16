using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

        [Required(ErrorMessage = "Base price is required.")]
        [Range(100.0, 9999.9, ErrorMessage = "Base price cannot exceed 4 digits and 1 decimal place (max 9999.9) and cant be smaller than 100.0.")]
        public decimal BasePrice { get; set; }
    }
}
