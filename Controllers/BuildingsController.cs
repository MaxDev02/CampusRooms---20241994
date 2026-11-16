using CampusRooms.Data;
using CampusRooms.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CampusRooms.Controllers
{
    public class BuildingsController : Controller
    {
        private readonly AppDbContext _db;

        public BuildingsController(AppDbContext db)
        {
            _db = db;
        }

        // GET /buildings/2: one building and its rooms. /buildings/99 is a 404.
        [HttpGet("buildings/{id:int}")]
        public IActionResult Details(int id)
        {
            Building? building = _db.Buildings
                .Include(b => b.Rooms.OrderBy(room => room.Code))
                .FirstOrDefault(b => b.Id == id);
            if (building == null)
            {
                return NotFound();
            }
            ViewData["Title"] = building.Name;
            return View(building);
        }
    }
}
