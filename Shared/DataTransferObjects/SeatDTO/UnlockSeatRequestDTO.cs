using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DataTransferObjects.SeatDTO
{
    public class UnlockSeatRequestDTO
    {
        [Required(ErrorMessage = "Seat ID is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid Seat ID.")]
        public int SeatId { get; set; }

        [Required(ErrorMessage = "Event ID is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid Event ID.")]
        public int EventId { get; set; }

        [Required(ErrorMessage = "User ID is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid User ID.")]
        public int UserId { get; set; }
    }
}
