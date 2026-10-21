
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StayBnb.Data;
using StayBnb.Models;

namespace StayBnb.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    public class SuperAdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public SuperAdminController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Dashboard()
        {
            ViewBag.TotalUsers = await _userManager.Users.CountAsync();

            ViewBag.TotalProperties = await _context.Properties.CountAsync();

            ViewBag.PendingProperties = await _context.Properties
                .CountAsync(p => !p.IsActive);

            ViewBag.TotalBookings = await _context.Bookings.CountAsync();

            return View();
        }


        public async Task<IActionResult> ManageAdmins()
        {
            var users = await _userManager.Users
                .OrderBy(u => u.Email)
                .ToListAsync();

            return View(users);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PromoteToAdmin(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return BadRequest();
            }

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            if (await _userManager.IsInRoleAsync(user, "SuperAdmin"))
            {
                TempData["AdminMessage"] = "SuperAdmin accounts cannot be changed here.";
                return RedirectToAction(nameof(ManageAdmins));
            }

            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                TempData["AdminMessage"] = "This user is already an Admin.";
                return RedirectToAction(nameof(ManageAdmins));
            }

            var result = await _userManager.AddToRoleAsync(user, "Admin");

            if (result.Succeeded)
            {
                TempData["AdminMessage"] = $"{user.Email} has been promoted to Admin.";
            }
            else
            {
                TempData["AdminMessage"] = string.Join(
                    " ", result.Errors.Select(e => e.Description));
            }

            return RedirectToAction(nameof(ManageAdmins));
        }
    }
}