using BusBooking.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;


namespace BusBooking.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketsController : ControllerBase
    {
        private readonly BusBookingContext _context;

        public TicketsController(BusBookingContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Ticket>>> GetTickets()
        {
            var bookings = await _context.BookSeats.ToListAsync();
            return Ok(bookings);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Ticket>> GetTicket(int id)
        {
            var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.TicketId == id);

            if (ticket == null)
            {
                return NotFound();
            }

            return ticket;
        }

        [HttpPost]
        public async Task<ActionResult<Ticket>> PostTicket(Ticket ticket)
        {
            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTicket", new { id = ticket.TicketId }, ticket);
        }
    }

}
