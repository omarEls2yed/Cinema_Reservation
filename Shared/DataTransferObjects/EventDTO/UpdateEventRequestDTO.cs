using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

        [Required(ErrorMessage = "Base price is required.")]
        [Range(100.0, 9999.9, ErrorMessage = "Base price cannot exceed 4 digits and 1 decimal place (max 9999.9) and cant be smaller than 100.0.")]
        public decimal BasePrice { get; set; }
    }
}
