using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StayBnb.Data;
using StayBnb.Models;
using System.Security.Claims;

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
        [Authorize(Roles = "Host")]
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

        // Approve a pending booking and notify the guest
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Host")]
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

            var notification = new Notification
            {
                UserId = booking.GuestId,
                Title = "Booking Confirmed",
                Content = $"Your booking for {booking.Property!.Title} has been approved by the host.",
                LinkUrl = "/Booking/MyBookings",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);

            await _context.SaveChangesAsync();

            TempData["HostMessage"] =
                "Booking approved successfully.";

            return RedirectToAction(nameof(Bookings));
        }

        // Reject a pending booking and notify the guest
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Host")]
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

            var notification = new Notification
            {
                UserId = booking.GuestId,
                Title = "Booking Update",
                Content = $"Your booking for {booking.Property!.Title} was rejected by the host.",
                LinkUrl = "/Booking/MyBookings",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);

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