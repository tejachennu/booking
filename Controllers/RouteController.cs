using BusBooking.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BusBooking.Server.Models;

namespace BusBooking.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RouteController : Controller
    {
        private readonly BusBookingContext _context;
        public RouteController( BusBookingContext busBooking) {
            _context = busBooking;
        }
        public partial class RoutesDto
        {

            public int? UserId { get; set; }

            public string? ArrivalCity { get; set; }

            public string? DepartureCity { get; set; }

            public decimal? Distance { get; set; }

            public string? Duration { get; set; }

            public string? Stops { get; set; }

        }

        // Create Route
        [HttpPost]
        public async Task<IActionResult> AddRoute([FromForm] Models.Route routeDto)
        {
            // Check if the routeDto is valid
            if (routeDto == null)
            {
                return BadRequest("Invalid route data.");
            }

            var route = new Models.Route
            {
                UserId = routeDto.UserId,
                ArrivalCity = routeDto.ArrivalCity,
                DepartureCity = routeDto.DepartureCity,
                Distance = routeDto.Distance,
                Duration = routeDto.Duration,
                Stops = routeDto.Stops
            };

            _context.Routes.Add(route);
            await _context.SaveChangesAsync();
            return Ok(route);
        }



        [HttpGet("GetRoutes")]
        public async Task<IActionResult> GetRoutes()
        {
            var routes = await _context.Routes.ToListAsync();
            return Ok(routes);
        }

        [HttpGet("GetRoutesByUserId/{userId}")]
        public async Task<ActionResult<BusBooking.Server.Models.Route>> GetBusesByUserId(int userId)
        {

            var buses = await _context.Routes
                                       .Include(b => b.User)
                                       .Where(b => b.UserId == userId)
                                       .ToListAsync();
            
            if (buses == null || buses.Count == 0)
            {
                return NotFound("No buses found for this user.");
            }

            return Ok(buses);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRoute(int id, [FromBody] RoutesDto routeDto)
        {
            // Validate routeDto
            if (routeDto == null)
            {
                return BadRequest("Route data is required.");
            }

            if (routeDto.Distance <= 0)
            {
                return BadRequest("Distance must be a positive number.");
            }

            var route = await _context.Routes.FindAsync(id);
            if (route == null)
            {
                return NotFound("Route not found.");
            }

            // Update properties
            route.ArrivalCity = routeDto.ArrivalCity;
            route.DepartureCity = routeDto.DepartureCity;
            route.Distance = routeDto.Distance;
            route.Duration = routeDto.Duration;
            route.Stops = routeDto.Stops;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict("A concurrency issue occurred. Please try again.");
            }

            return Ok(route); // Or return NoContent() if no response is required.
        }


        // Delete Route
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRoute(int id)
        {
            var route = await _context.Routes.FindAsync(id);
            if (route == null) return NotFound();

            _context.Routes.Remove(route);
            await _context.SaveChangesAsync();
            return Ok();
        }

    }
}
