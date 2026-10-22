using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StayBnb.Data;
using Microsoft.AspNetCore.Identity;
using StayBnb.Models;

namespace StayBnb.Controllers
{
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Dashboard()
        {
            var pendingCount = await _context.Properties
                .CountAsync(p => !p.IsActive);

            ViewBag.PendingCount = pendingCount;

            return View();
        }

        public async Task<IActionResult> HostApplications()
        {
            var applications = await _context.HostApplications
                .Include(a => a.User)
                .Include(a => a.Property)
                .Where(a => a.Status == "Pending")
                .OrderByDescending(a => a.SubmittedAt)
                .ToListAsync();

            return View(applications);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveHostApplication(int id)
        {
            var application = await _context.HostApplications
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (application == null)
            {
                TempData["AdminMessage"] =
                    "The host application could not be found.";

                return RedirectToAction(nameof(HostApplications));
            }

            if (application.Status != "Pending")
            {
                TempData["AdminMessage"] =
                    $"This application has already been processed. Current status: {application.Status}.";

                return RedirectToAction(nameof(HostApplications));
            }

            if (application.User == null)
            {
                TempData["AdminMessage"] =
                    "Approval failed because the applicant's account could not be found.";

                return RedirectToAction(nameof(HostApplications));
            }

            // Check whether the user already has the Host role.
            var alreadyHost = await _userManager.IsInRoleAsync(
                application.User, "Host");

            if (!alreadyHost)
            {
                var result = await _userManager.AddToRoleAsync(
                    application.User, "Host");

                if (!result.Succeeded)
                {
                    var errors = string.Join(
                        "; ",
                        result.Errors.Select(e => e.Description));

                    TempData["AdminMessage"] =
                        $"Could not assign the Host role: {errors}";

                    return RedirectToAction(nameof(HostApplications));
                }
            }

            application.Status = "Approved";
            application.ReviewedAt = DateTime.UtcNow;
            application.ReviewNotes = "Host application approved.";

            await _context.SaveChangesAsync();

            TempData["AdminMessage"] =
                "Host application approved successfully. The applicant now has Host access.";

            return RedirectToAction(nameof(HostApplications));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectHostApplication(
    int id, string? reviewNotes)
        {
            var application = await _context.HostApplications
                .FirstOrDefaultAsync(a => a.Id == id);

            if (application == null)
            {
                return NotFound();
            }

            if (application.Status != "Pending")
            {
                return RedirectToAction(nameof(HostApplications));
            }

            application.Status = "Rejected";
            application.ReviewedAt = DateTime.UtcNow;
            application.ReviewNotes = string.IsNullOrWhiteSpace(reviewNotes)
                ? "Host application rejected."
                : reviewNotes.Trim();

            await _context.SaveChangesAsync();

            TempData["AdminMessage"] =
                "Host application rejected successfully.";

            return RedirectToAction(nameof(HostApplications));
        }


        public async Task<IActionResult> Properties()
        {
            var properties = await _context.Properties
                .Include(p => p.Images)
                .Where(p => !p.IsActive)
                .OrderByDescending(p => p.Id)
                .ToListAsync();

            return View(properties);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var property = await _context.Properties
                .FirstOrDefaultAsync(p => p.Id == id);

            if (property == null)
            {
                return NotFound();
            }

            if (!property.IsActive)
            {
                property.IsActive = true;
                await _context.SaveChangesAsync();

                TempData["AdminMessage"] =
                    "Property approved successfully.";
            }

            return RedirectToAction(nameof(Properties));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var property = await _context.Properties
                .FirstOrDefaultAsync(p => p.Id == id);

            if (property == null)
            {
                return NotFound();
            }

            _context.Properties.Remove(property);
            await _context.SaveChangesAsync();

            TempData["AdminMessage"] =
                "Property rejected and removed successfully.";

            return RedirectToAction(nameof(Properties));
        }

    }
}