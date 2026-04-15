using DomainLayer.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using ServiceAbstraction;
using Shared;
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
    public class SeatController(IServiceManager _serviceManager, IOutputCacheStore _cache) : ControllerBase
    {
        [HttpGet("Get-all-seats-for-event")]
        public async Task<IActionResult> GetAllSeatsAsync([FromQuery] SeatQueryStruct query)
        {
            if (query.EventId <= 0)
                return BadRequest("You must provide an EventId (SessionId) to check seat availability.");
            var result = await _serviceManager.SeatService.GetAllSeatsAsync(query);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [OutputCache(Duration = 60, Tags = ["Seats"])]
        public async Task<IActionResult> GetSeatByIdAsync(int id)
        {
            var result = await _serviceManager.SeatService.GetSeatByIdAsync(id);
            if (result == null) throw new SeatNotFoundException(id);
            return Ok(result);
        }

        [HttpPost("Create-row-of-seats")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateSeatRow([FromBody] CreateSeatRowRequestDTO request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            bool success = await _serviceManager.SeatService.CreateSeatRowAsync(request);
            if (!success)
                return BadRequest("Failed. This row might already exist.");

            await _cache.EvictByTagAsync("Seats", cancellationToken);

            return StatusCode(201, new { Message = $"Created {request.SeatCount} seats for Row {request.Row}." });
        }

        [HttpDelete("Delete-seat-from-Venue")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteSeat([FromBody] DeleteSeatRequestDTO request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            bool success = await _serviceManager.SeatService.DeleteSeatAsync(request);
            if (!success) throw new SeatNotFoundException(request.Number);

            await _cache.EvictByTagAsync("Seats", cancellationToken);

            return NoContent();
        }
    }
}
