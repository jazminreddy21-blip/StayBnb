using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StayBnb.Data;
using StayBnb.Models;
using System.Security.Claims;
using System.Linq;
using Microsoft.AspNetCore.Authorization;

namespace StayBnb.Controllers
{
    [Authorize]
    public class BookingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BookingController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Create(int propertyId)
        {
            var property = await _context.Properties
                .FirstOrDefaultAsync(p =>
                    p.Id == propertyId && p.IsActive);

            if (property == null)
            {
                return NotFound();
            }

            ViewBag.Property = property;

            return View();
        }
        
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            int propertyId,
            DateTime checkInDate,
            DateTime checkOutDate,
            int numberOfGuests)
        {
            var property = await _context.Properties
                .FirstOrDefaultAsync(p =>
                    p.Id == propertyId && p.IsActive);

            if (property == null)
            {
                return NotFound();
            }

            ViewBag.Property = property;

            if (checkInDate.Date < DateTime.Today)
            {
                ModelState.AddModelError(
                    "CheckInDate",
                    "Check-in cannot be in the past.");
            }

            if (checkOutDate.Date <= checkInDate.Date)
            {
                ModelState.AddModelError(
                    "CheckOutDate",
                    "Check-out must be after check-in.");
            }

            if (numberOfGuests < 1 || numberOfGuests > 20)
            {
                ModelState.AddModelError(
                    "NumberOfGuests",
                    "Guests must be between 1 and 20.");
            }

            if (!ModelState.IsValid)
            {
                return View();
            }

            var hasConflict = await _context.Bookings.AnyAsync(b =>
    b.PropertyId == property.Id &&
    (b.Status == "Pending" || b.Status == "Confirmed") &&
    checkInDate.Date < b.CheckOutDate &&
    checkOutDate.Date > b.CheckInDate);

            if (hasConflict)
            {
                ModelState.AddModelError(
                    "",
                    "This property already has a booking that overlaps those dates. Please choose different dates.");

                return View();
            }

            var nights = (checkOutDate.Date -
                          checkInDate.Date).Days;

            var totalPrice =
                (property.PricePerNight * nights)
                + property.CleaningFee
                + property.ServiceFee;

            var guestId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(guestId))
            {
                return Challenge();
            }

            var booking = new Booking
            {
                GuestId = guestId,
                PropertyId = property.Id,
                CheckInDate = checkInDate.Date,
                CheckOutDate = checkOutDate.Date,
                NumberOfGuests = numberOfGuests,
                TotalPrice = totalPrice,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            return View("Confirmation", booking);
        }

        [HttpGet]
        public async Task<IActionResult> CheckInInstructions(int id)
        {
            var guestId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var booking = await _context.Bookings
                .Include(b => b.Property)
                .FirstOrDefaultAsync(b =>
                    b.Id == id &&
                    b.GuestId == guestId &&
                    b.Status == "Confirmed");

            if (booking == null || booking.Property == null)
            {
                return NotFound();
            }

            var instructions = await _context.CheckInProcesses
                .FirstOrDefaultAsync(c =>
                    c.PropertyId == booking.PropertyId);

            if (instructions == null)
            {
                return NotFound("The host has not added check-in instructions yet.");
            }

            return View(instructions);
        }

        [HttpGet]
        public async Task<IActionResult> MyBookings()
        {
            var guestId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var bookings = await _context.Bookings
                .Include(b => b.Property)
                .Where(b => b.GuestId == guestId)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            return View(bookings);
        }
    }
}