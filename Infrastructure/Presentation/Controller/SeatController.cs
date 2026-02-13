using DomainLayer.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Org.BouncyCastle.Asn1.Ocsp;
using ServiceAbstraction;
using Shared;
using Shared.DataTransferObjects.SeatDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Presentation.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class SeatController(IServiceManager _serviceManager) : ControllerBase
    {
        [HttpGet]
        [OutputCache(Duration = 10, VaryByQueryKeys = ["EventId", "DesiredPageIndx", "VenueId"], Tags = ["Seats"])]
        public async Task<IActionResult> GetAllSeatsAsync([FromQuery] SeatQueryStruct query)
        {
            if (query.EventId <= 0)
                return BadRequest("You must provide an EventId (SessionId) to check seat availability.");
            var result = await _serviceManager.SeatService.GetAllSeatsAsync(query);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [OutputCache(Duration = 2, Tags = ["Seats"])]
        public async Task<IActionResult> GetSeatByIdAsync(int id)
        {
            var result = await _serviceManager.SeatService.GetSeatByIdAsync(id);
            if (result == null) throw new SeatNotFoundException(id);
            return Ok(result);
        }

        [HttpPost("row")]
        [Authorize(Roles = "Admin")] 
        public async Task<IActionResult> CreateSeatRow([FromBody] CreateSeatRowRequestDTO request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            bool success = await _serviceManager.SeatService.CreateSeatRowAsync(request);
            if (!success)
                return BadRequest("Failed. This row might already exist.");
            return StatusCode(201, new { Message = $"Created {request.SeatCount} seats for Row {request.Row}." });
        }

        [HttpDelete]
        [Authorize(Roles = "Admin")] 
        public async Task<IActionResult> DeleteSeat([FromBody] DeleteSeatRequestDTO request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            bool success = await _serviceManager.SeatService.DeleteSeatAsync(request);
            if (!success) throw new SeatNotFoundException(request.Number);
            return NoContent(); 
        }
    }
}
