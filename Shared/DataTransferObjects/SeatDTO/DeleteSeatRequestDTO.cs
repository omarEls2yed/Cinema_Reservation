using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DataTransferObjects.SeatDTO
{
    public class DeleteSeatRequestDTO
    {
        [Required(ErrorMessage = "Venue ID is required.")]
        public int VenueId { get; set; }

        [Required(ErrorMessage = "Row number is required.")] 
        [Range(1, 100, ErrorMessage = "Row number must be between 1 and 100.")]
        public int Row { get; set; }

        [Required(ErrorMessage = "Seat number is required.")]
        [Range(1, 50, ErrorMessage = "Seat number must be between 1 and 50.")]
        public int Number { get; set; }

    }
}
