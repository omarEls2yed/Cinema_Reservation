using DomainLayer.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1.Ocsp;
using ServiceAbstraction;
using Shared;
using Shared.DataTransferObjects.EventDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Presentation.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController(IServiceManager _serviceManager) : ControllerBase
    {
        [HttpGet("futureEvents")]
        public async Task<ActionResult<PaginatedStruct<EventInformationDTO>>> GetFutureEvents([FromQuery] EventQueryStruct query)
        {
            var result = await _serviceManager.EventService.GetFutureEventsAsync(query);
            return Ok(result);
        }

        [HttpPost("create")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Create([FromBody] CreateEventRequestDTO request)
        {
            if (request == null) return BadRequest("Invalid Data");
            var result = await _serviceManager.EventService.AddFutureEventAsync(request);
            if (result) return Ok(new { Message = "Event Created Successfully" });
            return StatusCode(500, new { Message = "Error creating event" });
        }

        [HttpPut("update")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Update([FromBody] UpdateEventRequestDTO request)
        {
            if (request == null) return BadRequest("Invalid Data");
            var result = await _serviceManager.EventService.UpdateEvent(request);
            if (result) return Ok(new { Message = "Event Updated Successfully" });
            throw new EventNotFoundException(request.Id);
        }
        [HttpDelete("delete/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Delete(int id)
        {
            var result = await _serviceManager.EventService.DeleteEventAsync(id);
            if (result) return Ok(new { Message = "Event Deleted Successfully" });
            throw new EventNotFoundException(id);
        }
    }
}

