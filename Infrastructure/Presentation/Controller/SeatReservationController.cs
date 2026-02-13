using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using ServiceAbstraction;
using Shared.DataTransferObjects.SeatDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Presentation.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReservationController(IServiceManager _serviceManager)
        : ControllerBase
    {
        [HttpPost("lock")]
        public async Task<ActionResult> LockSeat([FromBody] LockSeatRequestDTO request)
        {
            bool success = await _serviceManager.SeatReservationService.LockSeatAsync(request);
            if (success) return Ok(new { Message = "Seat locked successfully." });
            return Conflict(new { Message = "Seat is already taken or unavailable." });
        }

        [HttpPost("unlock")]
        public async Task<ActionResult> UnlockSeat([FromBody] UnlockSeatRequestDTO request)
        {
            bool success = await _serviceManager.SeatReservationService.UnlockSeatAsync(request);
            if (success) return Ok(new { Message = "Seat unlocked." });
            return BadRequest(new { Message = "Could not unlock seat (maybe you don't own the lock)." });
        }

        [HttpPost("book")]
        public async Task<ActionResult> BookTicket([FromBody] BookTicketRequestDTO request)
        {
            var result = await _serviceManager.SeatReservationService.BookSeatAsync(request);
            if (result == "Success") return Ok(new { Message = "Booking confirmed! Ticket created." });
            return BadRequest(new { Message = result });
        }
    }
}
