using CampusRooms.Data;
using CampusRooms.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CampusRooms.Controllers
{
    // The rooms pages: list, details, add, edit, delete.
    public class RoomsController : Controller
    {
        private readonly AppDbContext _db;

        public RoomsController(AppDbContext db)
        {
            _db = db;
        }

        // Fills the building dropdown (A to Z) and selects the given building
        private void LoadBuildings(int? selectedId)
        {
            List<Building> buildings = _db.Buildings.OrderBy(building => building.Name).ToList();
            ViewBag.Buildings = new SelectList(buildings, "Id", "Name", selectedId);
        }

        // GET /rooms: every room, sorted by code, each with its building (one query)
        [HttpGet("rooms")]
        public IActionResult Index()
        {
            ViewData["Title"] = "Rooms";
            List<Room> rooms = _db.Rooms
                .Include(room => room.Building)
                .OrderBy(room => room.Code)
                .ToList();
            ViewBag.Count = rooms.Count;
            return View(rooms);
        }

        // GET /rooms/seats/30: rooms with at least 30 seats, largest first.
        // The SELECT is written by hand; {min} goes in as a parameter, not pasted into the text.
        [HttpGet("rooms/seats/{min:int}")]
        public IActionResult Seats(int min)
        {
            ViewData["Title"] = $"Rooms with {min}+ seats";
            List<Room> rooms = _db.Rooms
                .FromSql($"SELECT * FROM dbo.Rooms WHERE Seats >= {min}")
                .Include(room => room.Building)
                .OrderByDescending(room => room.Seats)
                .ThenBy(room => room.Code)
                .ToList();
            ViewBag.Min = min;
            return View(rooms);
        }

        // GET /rooms/2: one room. /rooms/99 is a 404.
        [HttpGet("rooms/{id:int}")]
        public IActionResult Details(int id)
        {
            Room? room = _db.Rooms
                .Include(r => r.Building)
                .FirstOrDefault(r => r.Id == id);
            if (room == null)
            {
                return NotFound();
            }
            ViewData["Title"] = room.Code;
            return View(room);
        }

        // GET /rooms/add: an empty form
        [Authorize(Policy = "CanManageRooms")]
        [HttpGet("rooms/add")]
        public IActionResult Add()
        {
            ViewData["Title"] = "Add a room";
            LoadBuildings(null);
            return View(new Room());
        }

        // POST /rooms/add: save the new room, or show the form again with the messages
        [Authorize(Policy = "CanManageRooms")]
        [HttpPost("rooms/add")]
        [ValidateAntiForgeryToken]
        public IActionResult Add(Room new_room)
        {
            if (new_room.BuildingId != null && !_db.Buildings.Any(b => b.Id == new_room.BuildingId))
            {
                ModelState.AddModelError("BuildingId", "Pick a building");
            }

            if (!ModelState.IsValid)
            {
                ViewData["Title"] = "Add a room";
                LoadBuildings(new_room.BuildingId);
                return View(new_room);
            }

            _db.Rooms.Add(new_room);
            try
            {
                _db.SaveChanges();
            }
            catch (DbUpdateException)
            {
                // The unique index on Code said no
                ModelState.AddModelError("Code", "Another room already has that code");
                ViewData["Title"] = "Add a room";
                LoadBuildings(new_room.BuildingId);
                return View(new_room);
            }

            TempData["Message"] = $"{new_room.Code} {new_room.Name} was added";
            return RedirectToAction("Index");
        }

        // GET /rooms/edit/2: the form, filled in. /rooms/edit/99 is a 404.
        [Authorize(Policy = "CanManageRooms")]
        [HttpGet("rooms/edit/{id:int}")]
        public IActionResult Edit(int id)
        {
            Room? room = _db.Rooms.Find(id);
            if (room == null)
            {
                return NotFound();
            }
            ViewData["Title"] = "Edit a room";
            LoadBuildings(room.BuildingId);
            return View(room);
        }

        // POST /rooms/edit/2: save the changes to the same row
        [Authorize(Policy = "CanManageRooms")]
        [HttpPost("rooms/edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Room changed_room)
        {
            if (changed_room.BuildingId != null && !_db.Buildings.Any(b => b.Id == changed_room.BuildingId))
            {
                ModelState.AddModelError("BuildingId", "Pick a building");
            }

            if (!ModelState.IsValid)
            {
                ViewData["Title"] = "Edit a room";
                LoadBuildings(changed_room.BuildingId);
                return View(changed_room);
            }

            Room? room = _db.Rooms.Find(id);
            if (room == null)
            {
                TempData["Message"] = "That room was deleted before you saved";
                return RedirectToAction("Index");
            }
            room.Code = changed_room.Code;
            room.Name = changed_room.Name;
            room.Seats = changed_room.Seats;
            room.HasProjector = changed_room.HasProjector;
            room.BuildingId = changed_room.BuildingId;

            try
            {
                _db.SaveChanges();
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("Code", "Another room already has that code");
                ViewData["Title"] = "Edit a room";
                LoadBuildings(changed_room.BuildingId);
                return View(changed_room);
            }

            TempData["Message"] = $"Saved {room.Code}";
            return RedirectToAction("Index");
        }

        // GET /rooms/delete/2: ask first. Nothing is deleted yet.
        [Authorize(Roles = "Admin")]
        [HttpGet("rooms/delete/{id:int}")]
        public IActionResult Delete(int id)
        {
            Room? room = _db.Rooms.Find(id);
            if (room == null)
            {
                return NotFound();
            }
            ViewData["Title"] = "Delete a room";
            return View(room);
        }

        // POST /rooms/delete/2: the Yes button. Now the row goes.
        [Authorize(Roles = "Admin")]
        [HttpPost("rooms/delete/{id:int}")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            Room? room = _db.Rooms.Find(id);
            if (room == null)
            {
                TempData["Message"] = "That room was already deleted";
                return RedirectToAction("Index");
            }
            _db.Rooms.Remove(room);
            _db.SaveChanges();
            TempData["Message"] = $"{room.Code} {room.Name} was deleted";
            return RedirectToAction("Index");
        }
    }
}
