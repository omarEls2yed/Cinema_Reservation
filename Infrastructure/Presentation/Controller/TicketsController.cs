using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Persistence.Data;
using Service;

namespace Presentation.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketsController : ControllerBase
    {
        private readonly EventReservationDbcontext _context;
        private readonly TicketPdfService _pdfService;

        public TicketsController(EventReservationDbcontext context, TicketPdfService pdfService)
        {
            _context = context;
            _pdfService = pdfService;
        }

        [HttpGet("{id}/print")]
        public async Task<IActionResult> PrintTicket(int id)
        {
            var ticket = await _context.Tickets
                .Include(t => t.Event)
                .Include(t => t.Seat)
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id);

            if (ticket == null) return NotFound();

            var pdfBytes = _pdfService.GeneratePdf(ticket);

            return File(pdfBytes, "application/pdf");
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTicketById(int id)
        {
            var ticket = await _context.Tickets
                .Include(t => t.Event)
                .Include(t => t.Seat)
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id);

            if (ticket == null)
            {
                return NotFound(new { message = "Ticket not found." });
            }

            return Ok(ticket);
        }
    }
}
