
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StayBnb.Data;
using System.Security.Claims;
using System.Linq;

namespace StayBnb.Controllers
{
    [Authorize]
    public class HostController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HostController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Display bookings for properties owned by the logged-in host
        public async Task<IActionResult> Bookings()
        {
            var hostId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            var bookings = await _context.Bookings
                .Include(b => b.Property)
                .Include(b => b.Guest)
                .Where(b => b.Property != null &&
                            b.Property.HostId == hostId)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            return View(bookings);
        }

        // Approve a pending booking
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var hostId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            var booking = await _context.Bookings
                .Include(b => b.Property)
                .FirstOrDefaultAsync(b =>
                    b.Id == id &&
                    b.Property != null &&
                    b.Property.HostId == hostId);

            if (booking == null)
            {
                return NotFound();
            }

            if (booking.Status != "Pending")
            {
                TempData["HostMessage"] =
                    "This booking has already been processed.";

                return RedirectToAction(nameof(Bookings));
            }

            booking.Status = "Confirmed";

            await _context.SaveChangesAsync();

            TempData["HostMessage"] =
                "Booking approved successfully.";

            return RedirectToAction(nameof(Bookings));
        }

        // Reject a pending booking
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var hostId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            var booking = await _context.Bookings
                .Include(b => b.Property)
                .FirstOrDefaultAsync(b =>
                    b.Id == id &&
                    b.Property != null &&
                    b.Property.HostId == hostId);

            if (booking == null)
            {
                return NotFound();
            }

            if (booking.Status != "Pending")
            {
                TempData["HostMessage"] =
                    "This booking has already been processed.";

                return RedirectToAction(nameof(Bookings));
            }

            booking.Status = "Rejected";

            await _context.SaveChangesAsync();

            TempData["HostMessage"] =
                "Booking rejected successfully.";

            return RedirectToAction(nameof(Bookings));
        }


        // Display properties belonging to the logged-in host
        [Authorize(Roles = "Host")]
        public async Task<IActionResult> MyProperties()
        {
            var hostId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            var properties = await _context.Properties
                .Where(p => p.HostId == hostId)
                .OrderByDescending(p => p.Id)
                .ToListAsync();

            return View(properties);
        }
    }
}