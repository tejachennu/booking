using BusBooking.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace BusBooking.Server.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class JourneysController : ControllerBase
    {
        private readonly BusBookingContext _context;

        public JourneysController(BusBookingContext context)
        {
            _context = context;
        }

        public class JourneyDto
        {
            public int? BusId { get; set; }
            public int? RouteId { get; set; }
            public int? UserId { get; set; }
            public string? DepartureDate { get; set; }
            public string? DepartureTime { get; set; }
            public string? ArrivalDate { get; set; }
            public string? ArrivalTime { get; set; }
            public string? Duration { get; set; }
            public decimal? Rating { get; set; }
            public string? Reviews { get; set; }
            public decimal? Price { get; set; }
            public string? BusNumber { get; set; }
            public string? DriverName { get; set; }
            public string? DriverPhone { get; set; }
        }

        // GET: api/Journeys
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Journey>>> GetJourneys()
        {
            return await _context.Journeys.ToListAsync();
        }

        // GET: api/Journeys/{journeyId}
        [HttpGet("{journeyId:int}")]
        public async Task<ActionResult<Journey>> GetJourneyByIdAsync(int journeyId)
        {
            var journey = await _context.Journeys
                .Include(j => j.Route) // Include the Route entity
                .Include(j => j.Bus)   // Include the Bus entity
                .ThenInclude(b => b.BusImages) // Include related BusImages
                .FirstOrDefaultAsync(j => j.JourneyId == journeyId);

            if (journey == null)
            {
                return NotFound();
            }

            return Ok(journey);
        }

        // GET: api/Journeys/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<Journey>>> GetJourneysByUserId(int userId)
        {
            var journeys = await _context.Journeys
                .Where(j => j.UserId == userId)
                .ToListAsync();

            if (journeys == null || !journeys.Any())
            {
                return NotFound("No journeys found for the specified user.");
            }

            return journeys;
        }

        // GET: api/Journeys/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Journey>> GetJourney(int id)
        {
            var journey = await _context.Journeys.FindAsync(id);

            if (journey == null)
            {
                return NotFound();
            }

            return journey;
        }

        // POST: api/Journeys
        [HttpPost]
        public async Task<ActionResult<Journey>> PostJourney([FromBody] JourneyDto journeyDto)
        {
            var journey = new Journey
            {
                UserId = journeyDto.UserId,
                BusId = journeyDto.BusId.Value,
                RouteId = journeyDto.RouteId.Value,
                DepartureDate = journeyDto.DepartureDate,
                DepartureTime = journeyDto.DepartureTime,
                ArrivalDate = journeyDto.ArrivalDate,
                ArrivalTime = journeyDto.ArrivalTime,
                Duration = journeyDto.Duration,
                Rating = journeyDto.Rating ?? 0,
                Reviews = journeyDto.Reviews,
                Price = journeyDto.Price ?? 0,
                BusNumber = journeyDto.BusNumber,
                DriverName = journeyDto.DriverName,
                DriverPhone = journeyDto.DriverPhone
            };

            _context.Journeys.Add(journey);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetJourney), new { id = journey.JourneyId }, journey);
        }

        // PUT: api/Journeys/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutJourney(int id, [FromBody] JourneyDto journeyDto)
        {
            var journey = await _context.Journeys.FindAsync(id);
            if (journey == null)
            {
                return NotFound();
            }

            journey.UserId = journeyDto.UserId;
            journey.BusId = journeyDto.BusId.Value;
            journey.RouteId = journeyDto.RouteId.Value;
            journey.DepartureDate = journeyDto.DepartureDate;
            journey.DepartureTime = journeyDto.DepartureTime;
            journey.ArrivalDate = journeyDto.ArrivalDate;
            journey.ArrivalTime = journeyDto.ArrivalTime;
            journey.Duration = journeyDto.Duration;
            journey.Rating = journeyDto.Rating ?? journey.Rating;
            journey.Reviews = journeyDto.Reviews;
            journey.Price = journeyDto.Price ?? journey.Price;
            journey.BusNumber = journeyDto.BusNumber;
            journey.DriverName = journeyDto.DriverName;
            journey.DriverPhone = journeyDto.DriverPhone;

            _context.Entry(journey).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!JourneyExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // GET: api/Journeys/available-bus/{fromLocation}/{toLocation}/{date}
        [HttpGet("available-bus/{fromLocation}/{toLocation}/{date}")]
        public async Task<ActionResult<IEnumerable<Journey>>> GetAvailableJourneys(string fromLocation, string toLocation, string date)
        {
            DateTime departureDate;

            // Validate the date format
            if (!DateTime.TryParse(date, out departureDate))
            {
                return BadRequest("Invalid date format. Please use a valid date (yyyy-MM-dd).");
            }

            var availableJourneys = await _context.Journeys
                .Include(j => j.Route) // Include the related Route entity
                .Include(j => j.Bus)   // Include the related Bus entity
                .Where(j => j.Route.DepartureCity == fromLocation
                            && j.Route.ArrivalCity == toLocation
                            && j.DepartureDate == departureDate.ToString("yyyy-MM-dd"))
                .ToListAsync();

            if (availableJourneys == null || !availableJourneys.Any())
            {
                return NotFound($"No journeys found from {fromLocation} to {toLocation} on {date}.");
            }

            return Ok(availableJourneys);
        }


        // DELETE: api/Journeys/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteJourney(int id)
        {
            var journey = await _context.Journeys.FindAsync(id);
            if (journey == null)
            {
                return NotFound();
            }

            _context.Journeys.Remove(journey);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool JourneyExists(int id)
        {
            return _context.Journeys.Any(e => e.JourneyId == id);
        }

    }

}