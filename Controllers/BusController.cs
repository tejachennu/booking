using BusBooking.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace BusBooking.Server.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class BusController : ControllerBase
    {
        private readonly BusBookingContext _context;

        public BusController(BusBookingContext context)
        {
            _context = context;
        }

        public class Bus
        {
            public int Id { get; set; }
            public string BusName { get; set; }
            public string BusNumber { get; set; }
            public string OwnerName { get; set; }
            public string BusType { get; set; }
            public string BusCompany { get; set; }
            public int UserId { get; set; }
            public ICollection<BusImage> BusImages { get; set; }
        }

        public class BusesDto
        {
            public string BusName { get; set; }
            public string BusNumber { get; set; }
            public string OwnerName { get; set; }
            public string BusType { get; set; }
            public string BusCompany { get; set; }
            public int? UserId { get; set; }
        }


        public class BusImage
        {
            public int Id { get; set; }
            public string ImageUrl { get; set; }
            public int BusId { get; set; }
            public Bus Bus { get; set; }
        }


        // POST: api/bus/PostBusWithImages
        [HttpPost("PostBusWithImages")]
        public async Task<ActionResult<BusBooking.Server.Models.Bus>> PostBusWithImages([FromForm] BusesDto busDto, [FromForm] List<IFormFile> images)
        {
            var user = await _context.TblUsers.FindAsync(busDto.UserId);
            if (user == null)
            {
                return BadRequest("User does not exist.");
            }

            var bus = new BusBooking.Server.Models.Bus
            {
                BusName = busDto.BusName,
                BusNumber = busDto.BusNumber,
                OwnerName = busDto.OwnerName,
                BusType = busDto.BusType,
                BusCompany = busDto.BusCompany,
                UserId = busDto.UserId.Value
            };

            _context.Buses.Add(bus);
            await _context.SaveChangesAsync();

            if (images != null && images.Count > 0)
            {
                string wwwRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                string uploadFolder = Path.Combine(wwwRootPath, "Images");

                foreach (var file in images)
                {
                    if (file.Length > 0)
                    {
                        var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                        var filePath = Path.Combine(uploadFolder, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        var fileUrl = Path.Combine("/Images", fileName);
                        _context.BusImages.Add(new BusBooking.Server.Models.BusImage { BusId = bus.Id, ImageUrl = fileUrl });
                    }
                }
                await _context.SaveChangesAsync();
            }

            return CreatedAtAction(nameof(GetBusById), new { id = bus.Id }, bus);
        }

        // GET: api/bus/GetBusesByUserId/{userId}
        [HttpGet("GetBusesByUserId/{userId}")]
        public async Task<ActionResult<IEnumerable<BusBooking.Server.Models.Bus>>> GetBusesByUserId(int userId)
        {
            var buses = await _context.Buses
                                       .Include(b => b.BusImages)
                                       .Where(b => b.UserId == userId)
                                       .ToListAsync();

            if (buses == null || buses.Count == 0)
            {
                return NotFound("No buses found for this user.");
            }

            return Ok(buses);
        }

        // GET: api/bus/GetBusById/{id}
        [HttpGet("GetBusById/{id}")]
        public async Task<ActionResult<BusBooking.Server.Models.Bus>> GetBusById(int id)
        {
            var bus = await _context.Buses.Include(b => b.BusImages).FirstOrDefaultAsync(b => b.Id == id);
            if (bus == null)
            {
                return NotFound();
            }
            return bus;
        }

        // GET: api/bus/GetBuses
        [HttpGet("GetBuses")]
        public async Task<ActionResult<IEnumerable<BusBooking.Server.Models.Bus>>> GetBuses()
        {
            return await _context.Buses.Include(b => b.BusImages).ToListAsync();
        }

        // PUT: api/bus/PutBus/{id}
        [HttpPut("PutBus/{id}")]
        public async Task<IActionResult> PutBus(int id)
        {
            var form = Request.Form;

            // Retrieve the busDto data from the form
            var busDto = new BusesDto
            {
                UserId = int.Parse(form["UserId"]),
                BusName = form["BusName"],
                BusNumber = form["BusNumber"],
                OwnerName = form["OwnerName"],
                BusType = form["BusType"],
                BusCompany = form["BusCompany"]
            };

            // Retrieve the bus from the database
            var bus = await _context.Buses.FindAsync(id);
            if (bus == null)
            {
                return NotFound();
            }

            // Update bus properties
            bus.BusName = busDto.BusName;
            bus.BusNumber = busDto.BusNumber;
            bus.OwnerName = busDto.OwnerName;
            bus.BusType = busDto.BusType;
            bus.BusCompany = busDto.BusCompany;

            _context.Entry(bus).State = EntityState.Modified;

            // Handle file uploads and image replacement
            if (form.Files != null && form.Files.Count > 0)
            {
                string wwwRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                string uploadFolder = Path.Combine(wwwRootPath, "Images");

                // Step 1: Remove existing images from the database and file system
                var existingImages = await _context.BusImages.Where(img => img.BusId == id).ToListAsync();
                foreach (var image in existingImages)
                {
                    // Remove the image from the file system
                    var imagePath = Path.Combine(wwwRootPath, image.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }

                    // Remove the image record from the database
                    _context.BusImages.Remove(image);
                }

                // Save changes to remove old images
                await _context.SaveChangesAsync();

                // Step 2: Add new images
                foreach (var file in form.Files)
                {
                    if (file.Length > 0)
                    {
                        var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                        var filePath = Path.Combine(uploadFolder, fileName);

                        // Save the file
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        // Save file URL in the database
                        var fileUrl = Path.Combine("/Images", fileName);
                        _context.BusImages.Add(new BusBooking.Server.Models.BusImage { BusId = bus.Id, ImageUrl = fileUrl });
                    }
                }

                // Save changes to add new images
                await _context.SaveChangesAsync();
            }

            // Save bus data
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/bus/DeleteBus/{id}
        [HttpDelete("DeleteBus/{id}")]
        public async Task<IActionResult> DeleteBus(int id)
        {
            var bus = await _context.Buses.FindAsync(id);
            if (bus == null)
            {
                return NotFound();
            }

            _context.Buses.Remove(bus);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}