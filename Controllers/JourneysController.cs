using BusBooking.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

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
        public TimeOnly? DepartureTime { get; set; }
        public DateOnly? ArrivalDate { get; set; }
        public TimeOnly? ArrivalTime { get; set; }
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
        if (!journeyDto.BusId.HasValue ||
            !journeyDto.RouteId.HasValue ||
            !journeyDto.DepartureDate.HasValue ||
            !journeyDto.DepartureTime.HasValue)
        {
            return BadRequest("Missing required fields.");
        }

        var journey = new Journey
        {
            UserId = journeyDto.UserId,
            BusId = journeyDto.BusId.Value,
            RouteId = journeyDto.RouteId.Value,
            DepartureDate = journeyDto.DepartureDate.Value,
            DepartureTime = journeyDto.DepartureTime.Value,
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
        if (id != journeyDto.BusId)
        {
            return BadRequest();
        }

        var journey = await _context.Journeys.FindAsync(id);
        if (journey == null)
        {
            return NotFound();
        }

        journey.UserId = journeyDto.UserId;
        journey.BusId = journeyDto.BusId.Value;
        journey.RouteId = journeyDto.RouteId.Value;
        journey.DepartureDate = journeyDto.DepartureDate.Value;
        journey.DepartureTime = journeyDto.DepartureTime.Value;
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
