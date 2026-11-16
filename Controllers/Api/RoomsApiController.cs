using CampusRooms.Data;
using CampusRooms.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CampusRooms.Controllers.Api
{
    // JSON only, no views, so it inherits ControllerBase.
    [ApiController]
    [Route("api/rooms")]
    public class RoomsApiController : ControllerBase
    {
        private readonly AppDbContext _db;

        public RoomsApiController(AppDbContext db)
        {
            _db = db;
        }

        private static RoomDto ToDto(Room room)
        {
            return new RoomDto
            {
                Id = room.Id,
                Code = room.Code,
                Name = room.Name,
                Seats = room.Seats,
                HasProjector = room.HasProjector,
                BuildingId = room.BuildingId,
                BuildingName = room.Building?.Name
            };
        }

        // GET /api/rooms
        [HttpGet]
        public ActionResult<List<RoomDto>> GetAll()
        {
            List<Room> rooms = _db.Rooms.Include(r => r.Building).OrderBy(r => r.Code).ToList();
            return rooms.Select(ToDto).ToList();
        }

        // GET /api/rooms/3
        [HttpGet("{id:int}")]
        public ActionResult<RoomDto> GetById(int id)
        {
            Room? room = _db.Rooms.Include(r => r.Building).FirstOrDefault(r => r.Id == id);
            if (room == null)
            {
                return NotFound();
            }
            return ToDto(room);
        }

        // POST /api/rooms: 201 + Location, 400 (rules, [ApiController] does this), or 409 (duplicate code)
        [AllowAnonymous]
        [HttpPost]
        public ActionResult<RoomDto> Post(RoomInput input)
        {
            if (!_db.Buildings.Any(b => b.Id == input.BuildingId))
            {
                ModelState.AddModelError(nameof(input.BuildingId), "That building does not exist");
                return ValidationProblem(ModelState);
            }
            if (_db.Rooms.Any(r => r.Code == input.Code))
            {
                return Conflict(new { message = "Another room already has that code" });
            }

            var room = new Room
            {
                Code = input.Code,
                Name = input.Name,
                Seats = input.Seats,
                HasProjector = input.HasProjector,
                BuildingId = input.BuildingId
            };
            _db.Rooms.Add(room);
            try
            {
                _db.SaveChanges();
            }
            catch (DbUpdateException)
            {
                return Conflict(new { message = "Another room already has that code" });
            }

            _db.Entry(room).Reference(r => r.Building).Load();
            return CreatedAtAction(nameof(GetById), new { id = room.Id }, ToDto(room));
        }

        // PUT /api/rooms/2: 204, 404, 409 or 400
        [AllowAnonymous]
        [HttpPut("{id:int}")]
        public IActionResult Put(int id, RoomInput input)
        {
            Room? room = _db.Rooms.Find(id);
            if (room == null)
            {
                return NotFound();
            }
            if (!_db.Buildings.Any(b => b.Id == input.BuildingId))
            {
                ModelState.AddModelError(nameof(input.BuildingId), "That building does not exist");
                return ValidationProblem(ModelState);
            }
            if (_db.Rooms.Any(r => r.Code == input.Code && r.Id != id))
            {
                return Conflict(new { message = "Another room already has that code" });
            }

            room.Code = input.Code;
            room.Name = input.Name;
            room.Seats = input.Seats;
            room.HasProjector = input.HasProjector;
            room.BuildingId = input.BuildingId;
            try
            {
                _db.SaveChanges();
            }
            catch (DbUpdateException)
            {
                return Conflict(new { message = "Another room already has that code" });
            }
            return NoContent();
        }

        // DELETE /api/rooms/5: 204, or 404 if it is already gone. No "are you sure?" page.
        [AllowAnonymous]
        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            Room? room = _db.Rooms.Find(id);
            if (room == null)
            {
                return NotFound();
            }
            _db.Rooms.Remove(room);
            _db.SaveChanges();
            return NoContent();
        }
    }
}
