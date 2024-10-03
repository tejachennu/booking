using BusBooking.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using static BusBooking.Server.Controllers.BookSeatsController;


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
            public DateOnly? DepartureDate { get; set; }
            public string? DepartureTime { get; set; }
            public DateOnly? ArrivalDate { get; set; }
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

        [HttpGet("getbuses")]
        public async Task<ActionResult<IEnumerable<Journey>>> GetJourneysAfterCurrentDateAsync()
        {
            var currentDate = DateOnly.FromDateTime(DateTime.Now); // Convert current DateTime to DateOnly

            var journeys = await _context.Journeys
                .Include(j => j.Route) // Include the Route entity
                .Include(j => j.Bus)   // Include the Bus entity
                .ThenInclude(b => b.BusImages) // Include related BusImages
                .Where(j => j.DepartureDate > currentDate) // Filter journeys after the current date
                .GroupBy(j => j.BusId) // Group by BusId to remove duplicates
                .Select(g => g.First()) // Select the first entry in each group (unique BusId)
                .ToListAsync(); // Return the list of matching journeys

            if (journeys == null || journeys.Count == 0)
            {
                return NotFound();
            }

            return Ok(journeys);
        }





        //// GET: api/Journeys/user/{userId}
        //[HttpGet("user/{userId}")]
        //public async Task<ActionResult<IEnumerable<Journey>>> GetJourneysByUserId(int userId)
        //{
        //    var journeys = await _context.Journeys
        //        .Where(j => j.UserId == userId)
        //        .ToListAsync();

        //    if (journeys == null || !journeys.Any())
        //    {
        //        return NotFound("No journeys found for the specified user.");
        //    }

        //    return journeys;
        //}

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<Journey>>> GetJourneysByUserId(
            int userId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? startDate = null,
            [FromQuery] string? endDate = null)
        {


            // Validate pageNumber and pageSize
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 10;

            try
            {
                // Fetch the journeys for the specific user
                var query = _context.Journeys.AsQueryable()
                    .Where(j => j.UserId == userId); // Filter by user ID

                // Apply start date filtering if provided
                if (!string.IsNullOrEmpty(startDate))
                {
                    if (DateOnly.TryParseExact(startDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly startDateParsed))
                    {
                        query = query.Where(j => j.DepartureDate.HasValue && j.DepartureDate.Value >= startDateParsed);
                    }
                    else
                    {
                        return BadRequest("Invalid start date format. Use 'yyyy-MM-dd'.");
                    }
                }

                // Apply end date filtering if provided
                if (!string.IsNullOrEmpty(endDate))
                {
                    if (DateOnly.TryParseExact(endDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly endDateParsed))
                    {
                        query = query.Where(j => j.DepartureDate.HasValue && j.DepartureDate.Value <= endDateParsed);
                    }
                    else
                    {
                        return BadRequest("Invalid end date format. Use 'yyyy-MM-dd'.");
                    }
                }

                // Count the total number of journeys (before pagination)
                var totalJourneys = await query.CountAsync();

                // Apply pagination
                var journeys = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                if (journeys == null || !journeys.Any())
                {
                    return NotFound("No journeys found for the given user.");
                }

                // Create pagination metadata
                var paginationMetadata = new
                {
                    totalCount = totalJourneys,
                    pageSize,
                    currentPage = pageNumber,
                    totalPages = (int)Math.Ceiling(totalJourneys / (double)pageSize)
                };

                // Add pagination metadata to the response headers
                Response.Headers.Add("X-Pagination", Newtonsoft.Json.JsonConvert.SerializeObject(paginationMetadata));

                return Ok(journeys);
            }
            catch (Exception ex)
            {
                // Handle any unexpected errors
                Console.WriteLine($"Internal server error: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
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

        [HttpGet("available-bus/{fromLocation}/{toLocation}/{date}")]
        public async Task<ActionResult<IEnumerable<Journey>>> GetAvailableJourneys(string fromLocation, string toLocation, string date)
        {
            // Try to parse the input date
            if (!DateTime.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime departureDate))
            {
                return BadRequest("Invalid date format. Please use a valid date (yyyy-MM-dd).");
            }

            // Query available journeys by matching cities and departure date
            var availableJourneys = await _context.Journeys
                .Include(j => j.Route)  // Include the related Route entity
                .Include(j => j.Bus)    // Include the related Bus entity
                .Where(j => j.Route.DepartureCity == fromLocation
                            && j.Route.ArrivalCity == toLocation
                            && j.DepartureDate == DateOnly.FromDateTime(departureDate))  // Convert DateTime to DateOnly
                .ToListAsync();

            // Handle no results found
            if (availableJourneys == null || !availableJourneys.Any())
            {
                return NotFound($"No journeys found from {fromLocation} to {toLocation} on {date}.");
            }

            return Ok(availableJourneys);
        }


        [HttpGet("available-bus/{busId}")]
        public async Task<ActionResult<IEnumerable<Journey>>> GetAvailableJourneys(int busId)
        {
            // Get today's date
            var todayDate = DateOnly.FromDateTime(DateTime.Today);

            // Query available journeys by matching busId and departure dates on or after today
            var availableJourneys = await _context.Journeys
                .Include(j => j.Route)  // Include the related Route entity
                .Include(j => j.Bus)    // Include the related Bus entity
                .Where(j => j.BusId == busId && j.DepartureDate >= todayDate) // Filter by busId and dates on or after today
                .ToListAsync();

            // Handle no results found
            if (availableJourneys == null || !availableJourneys.Any())
            {
                return NotFound($"No journeys found for bus ID {busId} on or after {todayDate}.");
            }

            return Ok(availableJourneys); // Return the available journeys
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