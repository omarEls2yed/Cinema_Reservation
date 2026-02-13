using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DataTransferObjects.SeatDTO
{
    public class CreateSeatRowRequestDTO
    {
        [Required(ErrorMessage = "You must choose the type of the chairs in this row (VIP, Standard)")]
        public string Class { get; set; }

        [Required(ErrorMessage = "You must choose the venue id")]
        public int VenueId { get; set; }

        [Required(ErrorMessage = "Row number is required.")]
        [Range(1, 100, ErrorMessage = "The row number is illogical.")]
        public int Row { get; set; }

        [Required(ErrorMessage = "Seat count in this row is required.")]
        [Range(1, 50, ErrorMessage = "The number of chairs must be between 1 and 50 at a time.")]
        public int SeatCount { get; set; }
    }
}
