using CampusRooms.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CampusRooms.Controllers
{
    // The Admin's page: every account, and a button to give the Staff role.
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;

        public UsersController(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        // GET /users
        [HttpGet("users")]
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Users";
            List<IdentityUser> users = await _userManager.Users.OrderBy(u => u.Email).ToListAsync();
            var staffIds = (await _userManager.GetUsersInRoleAsync("Staff")).Select(u => u.Id).ToHashSet();

            List<UserRow> rows = users
                .Select(u => new UserRow { Id = u.Id, Email = u.Email ?? u.UserName ?? "", IsStaff = staffIds.Contains(u.Id) })
                .ToList();
            return View(rows);
        }

        // POST /users/make-staff/{id}: it changes data, so it is a POST
        [HttpPost("users/make-staff/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MakeStaff(string id)
        {
            IdentityUser? user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["Message"] = "That account was not found";
                return RedirectToAction("Index");
            }

            if (!await _userManager.IsInRoleAsync(user, "Staff"))
            {
                await _userManager.AddToRoleAsync(user, "Staff");
            }
            TempData["Message"] = $"{user.Email} is now Staff";
            return RedirectToAction("Index");
        }
    }
}
