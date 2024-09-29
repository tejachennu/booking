using BusBooking.Server.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using Microsoft.EntityFrameworkCore;

namespace BusBooking.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookSeatsController : ControllerBase
    {
        private readonly BusBookingContext _context;

        public class BookSeatsDto
        {
            public int JourneyId { get; set; }
            public decimal TotalCost { get; set; }
            public string Email { get; set; }
            public string Phone { get; set; }
            public string PickupPoint { get; set; }
            public string DropPoint { get; set; }
            public List<SeatDto> Seats { get; set; }
        }

        public class SeatDto
        {
            public string SeatNumber { get; set; }
            public string Name { get; set; }
            public string Gender { get; set; }
        }

        private readonly RazorpayService _razorpayService;


        public BookSeatsController(BusBookingContext context, RazorpayService razorpayService)
        {
            _context = context;
            _razorpayService = razorpayService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<BookSeat>>> GetBookings()
        {
            return await _context.BookSeats.Include(b => b.Tickets).ToListAsync();
        }

        // GET: api/Journeys/Tickets/{journeyId}
        [HttpGet("Tickets/{journeyId}")]
        public async Task<ActionResult<IEnumerable<Ticket>>> GetTicketsByJourneyId(int journeyId)
        {
            try
            {
                // Find all bookings for the given journey
                var bookedSeats = await _context.BookSeats
                    .Where(b => b.JourneyId == journeyId && b.Status == "booked")
                    .Include(b => b.Tickets) // Include related tickets
                    .ToListAsync();

                if (bookedSeats == null || !bookedSeats.Any())
                {
                    return NotFound("No bookings found for the given journey ID.");
                }

                // Extract all tickets from the booked seats
                var tickets = bookedSeats.SelectMany(b => b.Tickets).ToList();

                if (tickets == null || !tickets.Any())
                {
                    return NotFound("No tickets found for the given journey.");
                }

                // Return the tickets for the journey
                return Ok(tickets);
            }
            catch (Exception ex)
            {
                // Handle any errors
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        [HttpGet("{journeyId}")]
        public async Task<ActionResult<IEnumerable<BookSeat>>> GetBookings(int journeyId)
        {
            var bookings = await _context.BookSeats
                                          .Include(b => b.Tickets)
                                          .Where(b => b.JourneyId == journeyId)
                                          .ToListAsync();

            if (bookings == null || !bookings.Any())
            {
                return NotFound();
            }

            return Ok(bookings);
        }


        // GET: api/Journeys/User/{userId}
        [HttpGet("User/{userId}")]
        public async Task<ActionResult<IEnumerable<Journey>>> GetJourneysByUserId(int userId)
        {
            try
            {
                // Fetch all journeys for the specific user
                var journeys = await _context.Journeys
                    .Where(j => j.UserId == userId)
                    .ToListAsync();

                if (journeys == null || !journeys.Any())
                {
                    return NotFound("No journeys found for the given user.");
                }

                return Ok(journeys);
            }
            catch (Exception ex)
            {
                // Handle any errors
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        // GET: api/BookSeats/Order/{orderId}
        [HttpGet("Order/{orderId}")]
        public async Task<ActionResult<BookSeat>> GetBookingByOrderId(string orderId)
        {
            try
            {
                // Find the booking with the specified RazorpayOrderId and Status == 'booked'
                var booking = await _context.BookSeats
                    .Include(b => b.Tickets) // Include tickets related to the booking
                    .FirstOrDefaultAsync(b => b.RazorpayOrderId == orderId && b.Status == "booked");

                if (booking == null)
                {
                    return NotFound("No booking found with the given order ID and status 'booked'.");
                }

                // Fetch the Journey and its related Route based on JourneyId
                var journey = await _context.Journeys
                    .Include(j => j.Route) // Include the Route data
                    .FirstOrDefaultAsync(j => j.JourneyId == booking.JourneyId);

                if (journey == null)
                {
                    return NotFound("No journey found for the given booking.");
                }

                // Construct the response with both booking and journey details
                var response = new
                {
                    Booking = booking,
                    Journey = journey
                };

                return Ok(response); // Return the booking and journey details
            }
            catch (Exception ex)
            {
                // Handle any errors
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        // GET: api/BookSeats/Order/{orderId}
        [HttpGet("Pnr/{PnrId}")]
        public async Task<ActionResult<BookSeat>> GetBookingByPnr(string PnrId)
        {
            try
            {
                // Find the booking with the specified RazorpayOrderId and Status == 'booked'
                var booking = await _context.BookSeats
                    .Include(b => b.Tickets) // Include tickets related to the booking
                    .FirstOrDefaultAsync(b => b.Pnr == PnrId && b.Status == "booked");

                if (booking == null)
                {
                    return NotFound("No booking found with the given order ID and status 'booked'.");
                }

                // Fetch the Journey and its related Route based on JourneyId
                var journey = await _context.Journeys
                    .Include(j => j.Route) // Include the Route data
                    .FirstOrDefaultAsync(j => j.JourneyId == booking.JourneyId);

                if (journey == null)
                {
                    return NotFound("No journey found for the given booking.");
                }

                // Construct the response with both booking and journey details
                var response = new
                {
                    Booking = booking,
                    Journey = journey
                };

                return Ok(response); // Return the booking and journey details
            }
            catch (Exception ex)
            {
                // Handle any errors
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }




        [HttpPost]
        public async Task<ActionResult<BookSeat>> CreateBooking([FromBody] BookSeatsDto bookingDto)
        {
            if (bookingDto == null || !bookingDto.Seats.Any())
            {
                return BadRequest("Booking data is incomplete or no seats selected.");
            }
            try
            {
                var order = _razorpayService.CreateOrder(bookingDto.TotalCost, "INR");
                // Create a new booking object
                var booking = new BookSeat
                {
                    RazorpayOrderId= order["id"].ToString(),
                    Status="pending",
                    JourneyId = bookingDto.JourneyId,
                    TotalCost = bookingDto.TotalCost,
                    Email = bookingDto.Email,
                    Phone = bookingDto.Phone,
                    PickupPoint = bookingDto.PickupPoint,
                    DropPoint = bookingDto.DropPoint,
                    Tickets = bookingDto.Seats.Select(seat => new Ticket
                    {
                        RazorpayOrderId = order["id"].ToString(),
                        SeatNumber = seat.SeatNumber.ToString(),
                        Name = seat.Name,
                        Gender = seat.Gender
                    }).ToList()
                };

                // Add the booking to the database
                _context.BookSeats.Add(booking);
                await _context.SaveChangesAsync();

                return Ok(new { orderId = order["id"].ToString(), amount = order["amount"], currency = order["currency"] });

            }
            catch (Exception ex)
            {
                // Log the exception for debugging (you might want to log it to a file or monitoring system)
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


    }
}
