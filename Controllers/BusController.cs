//using BusBooking.Server.Models;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Threading.Tasks;
//using Microsoft.AspNetCore.Http;
//using System.Text.Json;
//using System;
//using Microsoft.Extensions.Logging;
//using Microsoft.Extensions.Hosting;
//using Newtonsoft.Json;

//namespace BusBooking.Server.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class BusController : ControllerBase
//    {
//        private readonly BusBookingContext _context;
//        private readonly ILogger<BusController> _logger;


//        public BusController(BusBookingContext context, ILogger<BusController> logger)
//        {
//            _context = context;
//            _logger = logger;
//        }

//        public class DeckDto
//        {
//            public string DeckType { get; set; } = null!;
//            public List<SeatDto> Seats { get; set; } = new List<SeatDto>();
//        }

//        public class SeatDto
//        {
//            public string SeatNumber { get; set; } = null!;
//            public string SeatType { get; set; } = null!;
//            public string Berth { get; set; } = null!;
//            public string Column { get; set; } = null!;
//            public string Row { get; set; } = null!;
//        }

//        public class BusDetailsDto
//        {
//            public string BusName { get; set; } = null!;
//            public string BusNumber { get; set; } = null!;
//            public string OwnerName { get; set; } = null!;
//            public string BusType { get; set; } = null!;
//            public string BusCompany { get; set; } = null!;
//            public List<DeckDto> Decks { get; set; } = new List<DeckDto>();
//        }

//        public class BusesDto
//        {
//            public int Id { get; set; }

//            public string BusName { get; set; } = null!;

//            public string BusNumber { get; set; } = null!;

//            public int? UserId { get; set; }

//            public string? OwnerName { get; set; }

//            public string? BusType { get; set; }

//            public string? BusCompany { get; set; }
//        }

//        // DTO Classes
//        [HttpGet]
//        public async Task<ActionResult<IEnumerable<Bus>>> GetBuses()
//        {
//            return await _context.Buses.ToListAsync();
//        }

//        [HttpGet("GetBusesByUserId/{userId}")]
//        public async Task<ActionResult<IEnumerable<Bus>>> GetBusesByUserId(int userId)
//        {
//            var buses = await _context.Buses
//                .Where(b => b.UserId == userId) 
//                .ToListAsync();

//            // Check if any buses were found
//            if (buses == null || !buses.Any())
//            {
//                return NotFound(); 
//            }

//            return Ok(buses); 
//        }

//        [HttpPost("PostBusWithImages")]
//        public async Task<ActionResult<Bus>> PostBusWithImages([FromForm] BusesDto busDto, [FromForm] List<IFormFile> images)
//        {
//            // Find the user to associate with the bus
//            var user = await _context.TblUsers.FindAsync(busDto.UserId);
//            if (user == null)
//            {
//                return BadRequest("User does not exist.");
//            }

//            // Map the DTO to the Bus entity
//            var bus = new Bus
//            {
//                BusName = busDto.BusName,
//                BusNumber = busDto.BusNumber,
//                OwnerName = busDto.OwnerName,
//                BusType = busDto.BusType,
//                BusCompany = busDto.BusCompany,
//                UserId = busDto.UserId.Value // assuming UserId is non-null due to previous check
//            };

//            _context.Buses.Add(bus);
//            await _context.SaveChangesAsync();

//            // Handle image uploads
//            if (images != null && images.Count > 0)
//            {
//                // Define the folder path inside the wwwroot folder
//                string wwwRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
//                string uploadFolder = Path.Combine(wwwRootPath, "Images");

//                var uploadedFiles = new List<string>();

//                foreach (var file in images)
//                {
//                    if (file.Length > 0)
//                    {
//                        // Ensure the file has a unique name
//                        var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
//                        var filePath = Path.Combine(uploadFolder, fileName);

//                        using (var stream = new FileStream(filePath, FileMode.Create))
//                        {
//                            await file.CopyToAsync(stream);
//                        }

//                        var fileUrl = Path.Combine("/Images", fileName); // Update the path as needed
//                        uploadedFiles.Add(fileUrl);

//                        var busImage = new BusImage
//                        {
//                            BusId = bus.Id,
//                            ImageUrl = fileUrl
//                        };

//                        _context.BusImages.Add(busImage);
//                    }
//                }
//                await _context.SaveChangesAsync();
//            }

//            return CreatedAtAction(nameof(GetBusById), new { id = bus.Id }, bus);
//        }


//        // POST: api/Buses/PostBus
//        [HttpPost("PostBus")]
//        public async Task<ActionResult<Bus>> PostBus([FromForm] BusesDto busDto)
//        {
//            // Find the user to associate with the bus
//            var user = await _context.TblUsers.FindAsync(busDto.UserId);
//            if (user == null)
//            {
//                return BadRequest("User does not exist.");
//            }

//            // Map the DTO to the Bus entity
//            var bus = new Bus
//            {
//                BusName = busDto.BusName,
//                BusNumber = busDto.BusNumber,
//                OwnerName = busDto.OwnerName,
//                BusType = busDto.BusType,
//                BusCompany = busDto.BusCompany,
//                UserId = busDto.UserId.Value // assuming UserId is non-null due to previous check
//            };

//            _context.Buses.Add(bus);
//            await _context.SaveChangesAsync();

//            return CreatedAtAction(nameof(GetBusById), new { id = bus.Id }, bus);
//        }

//        [HttpPut("PutBus/{id}")]
//        public async Task<ActionResult<Bus>> PutBus(int id, [FromForm] BusesDto busDto)
//        {
//            // Check if the bus exists
//            var existingBus = await _context.Buses.FindAsync(id);
//            if (existingBus == null)
//            {
//                return NotFound("Bus not found.");
//            }

//            // Find the user to associate with the bus
//            var user = await _context.TblUsers.FindAsync(busDto.UserId);
//            if (user == null)
//            {
//                return BadRequest("User does not exist.");
//            }

//            // Update the existing bus
//            existingBus.BusName = busDto.BusName;
//            existingBus.BusNumber = busDto.BusNumber;
//            existingBus.OwnerName = busDto.OwnerName;
//            existingBus.BusType = busDto.BusType;
//            existingBus.BusCompany = busDto.BusCompany;
//            existingBus.UserId = busDto.UserId.Value; // assuming UserId is non-null due to previous check

//            _context.Buses.Update(existingBus);
//            await _context.SaveChangesAsync();

//            return Ok(existingBus);
//        }

//        [HttpDelete("DeleteBus/{id}")]
//        public async Task<ActionResult> DeleteBus(int id)
//        {
//            // Find the bus by ID
//            var bus = await _context.Buses.FindAsync(id);
//            if (bus == null)
//            {
//                return NotFound("Bus not found.");
//            }

//            // Remove the bus from the context
//            _context.Buses.Remove(bus);
//            await _context.SaveChangesAsync();

//            return NoContent(); // Return 204 No Content on successful deletion
//        }

//        // POST: api/bus/upload-images
//        [HttpPost("upload-images")]
//        public async Task<IActionResult> UploadImages([FromForm] int busId, [FromForm] List<IFormFile> images)
//        {
//            if (images == null || images.Count == 0)
//            {
//                return BadRequest("No images uploaded.");
//            }

//            // Define the folder path inside the wwwroot folder
//            string wwwRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
//            string uploadFolder = Path.Combine(wwwRootPath, "Images");

//            var uploadedFiles = new List<string>();

//            foreach (var file in images)
//            {
//                if (file.Length > 0)
//                {
//                    // Ensure the file has a unique name
//                    var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
//                    var filePath = Path.Combine(uploadFolder, fileName);

//                    using (var stream = new FileStream(filePath, FileMode.Create))
//                    {
//                        await file.CopyToAsync(stream);
//                    }

//                    var fileUrl = Path.Combine("/Images", busId.ToString(), fileName);
//                    uploadedFiles.Add(fileUrl);

//                    var busImage = new BusImage
//                    {
//                        BusId = busId,
//                        ImageUrl = fileUrl
//                    };

//                    _context.BusImages.Add(busImage);
//                }
//            }
//            await _context.SaveChangesAsync();

//            return Ok(new { message = "Images uploaded successfully", files = uploadedFiles });
//        }


//        [HttpGet("{id}")]
//        public async Task<ActionResult<BusDetailsDto>> GetBusById(int id)
//        {
//            // Fetch the bus by its ID along with decks and seats
//            var bus = await _context.Buses
//                .Include(b => b.Decks)
//                .ThenInclude(d => d.Seats) // Include seats within decks
//                .Where(b => b.Id == id)
//                .FirstOrDefaultAsync();

//            if (bus == null)
//            {
//                return NotFound();
//            }

//            // Create a DTO to shape the data as needed for the client
//            var busDetails = new BusDetailsDto
//            {
//                BusName = bus.BusName,
//                BusNumber = bus.BusNumber,
//                OwnerName = bus.OwnerName,
//                BusType = bus.BusType,
//                BusCompany = bus.BusCompany,
//                Decks = bus.Decks.Select(deck => new DeckDto
//                {
//                    DeckType = deck.DeckType,
//                    Seats = deck.Seats.Select(seat => new SeatDto
//                    {
//                        SeatNumber = seat.SeatNumber,
//                        SeatType = seat.SeatType,
//                        Berth = seat.Berth,
//                        Column = seat.Col,
//                        Row = seat.Row
//                    }).ToList()
//                }).ToList()
//            };

//            return Ok(busDetails);
//        }



//        [Route("add-decks-seats")]
//        [HttpPost]
//        public async Task<ActionResult> AddDecksAndSeats()
//        {
//            // Retrieve form data
//            var form = Request.Form;

//            // Get the BusId from the form (assuming it's an integer)
//            if (!int.TryParse(form["BusId"], out int busId))
//            {
//                return BadRequest("Invalid BusId.");
//            }

//            // Get the Decks JSON string from the form
//            var deckDtosJson = form["Decks"];
//            if (string.IsNullOrEmpty(deckDtosJson))
//            {
//                return BadRequest("Decks data is missing.");
//            }

//            // Deserialize the Decks JSON string to a DeckDto[] array
//            var deckDtoArray = JsonConvert.DeserializeObject<DeckDto[]>(deckDtosJson);

//            // Find the bus
//            var bus = await _context.Buses.FindAsync(busId);
//            if (bus == null)
//            {
//                return NotFound("Bus not found.");
//            }

//            // Process each deck and its seats
//            foreach (var deckDto in deckDtoArray)
//            {
//                var deck = new Deck
//                {
//                    DeckType = deckDto.DeckType,
//                    BusId = busId
//                };

//                _context.Decks.Add(deck);
//                await _context.SaveChangesAsync(); // Save to get the deck ID

//                foreach (var seatDto in deckDto.Seats)
//                    {
//                    var seat = new Seat
//                    {
//                        SeatNumber = seatDto.SeatNumber,
//                        SeatType = seatDto.SeatType,
//                        Berth = seatDto.Berth,
//                        DeckId = deck.Id,
//                        Col = seatDto.Column,
//                        Row = seatDto.Row,
//                    };

//                    _context.Seats.Add(seat);
//                }
//            }

//            await _context.SaveChangesAsync();
//            return Ok(new { message = "Decks and seats added successfully." });

//            }
//    }
//}

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
        public async Task<IActionResult> PutBus(int id, [FromForm] BusesDto busDto, [FromForm] List<IFormFile> images)
        {
            if (id != busDto.UserId)
            {
                return BadRequest();
            }

            var bus = await _context.Buses.FindAsync(id);
            if (bus == null)
            {
                return NotFound();
            }

            bus.BusName = busDto.BusName;
            bus.BusNumber = busDto.BusNumber;
            bus.OwnerName = busDto.OwnerName;
            bus.BusType = busDto.BusType;
            bus.BusCompany = busDto.BusCompany;

            _context.Entry(bus).State = EntityState.Modified;
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