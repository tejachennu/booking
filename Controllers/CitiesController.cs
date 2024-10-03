using BusBooking.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace BusBooking.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CitiesController : ControllerBase
    {
        public readonly BusBookingContext _context;
        
        public CitiesController(BusBookingContext context)
        {
            _context = context;
        }

        // GET: api/cities
        [HttpGet]
        public async Task<ActionResult<IEnumerable<City>>> GetCities()
        {
            return await _context.Cities.ToListAsync();
        }

        // GET: api/cities/5
        [HttpGet("{id}")]
        public async Task<ActionResult<City>> GetCity(int id)
        {
            var city = await _context.Cities.FindAsync(id);

            if (city == null)
            {
                return NotFound();
            }

            return city;
        }
        public partial class CitiesDto
        {
          

            public string? City1 { get; set; }
        }


        // POST: api/cities
        [HttpPost]
        public async Task<ActionResult<City>> PostCity(CitiesDto cityDto)
        {
            if (cityDto == null)
            {
                return BadRequest();
            }

            // Map DTO to Entity
            var city = new City
            {
                City1 = cityDto.City1 // Map the City1 property
            };

            _context.Cities.Add(city);
            await _context.SaveChangesAsync();

            // Return the created city with its generated ID
            return CreatedAtAction(nameof(GetCity), new { id = city.Id }, city);
        }


        // PUT: api/cities/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCity(int id, City city)
        {
            if (id != city.Id)
            {
                return BadRequest();
            }

            _context.Entry(city).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CityExists(id))
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

        // DELETE: api/cities/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCity(int id)
        {
            var city = await _context.Cities.FindAsync(id);
            if (city == null)
            {
                return NotFound();
            }

            _context.Cities.Remove(city);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CityExists(int id)
        {
            return _context.Cities.Any(e => e.Id == id);
        }

    }
}
