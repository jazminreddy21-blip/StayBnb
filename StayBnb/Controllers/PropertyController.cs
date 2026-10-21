using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StayBnb.Data;
using StayBnb.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace StayBnb.Controllers
{
    public class PropertyController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public PropertyController(
    ApplicationDbContext context,
    IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // Display an active property's details
        public async Task<IActionResult> Details(int id)
        {
            var property = await _context.Properties
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p =>
                    p.Id == id && p.IsActive);

            if (property == null)
            {
                return NotFound();
            }

            var checkInInfo = await _context.CheckInProcesses
                .FirstOrDefaultAsync(c => c.PropertyId == id);

            ViewBag.RequiredDocuments = checkInInfo?.RequiredDocuments;

            return View(property);
        }
        // Display the property creation form
        [Authorize]
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // Save a new property listing
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
     [Bind("Title,Description,City,Address,PropertyType,PricePerNight,CleaningFee,ServiceFee")]
    Property property,
     IFormFile? propertyImage)
        {
            // Get the logged-in host's ID securely.
            ModelState.Remove(nameof(Property.HostId));

            var hostId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(hostId))
            {
                return Challenge();
            }

            // Validate the optional image before saving anything.
            string? extension = null;

            if (propertyImage != null && propertyImage.Length > 0)
            {
                extension = Path.GetExtension(propertyImage.FileName)
                    .ToLowerInvariant();

                var allowedExtensions = new[]
                {
            ".jpg", ".jpeg", ".png", ".webp"
        };

                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError(
                        "propertyImage",
                        "Please upload a JPG, JPEG, PNG, or WebP image.");
                }

                if (propertyImage.Length > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError(
                        "propertyImage",
                        "The picture must be 5 MB or smaller.");
                }
            }

            if (!ModelState.IsValid)
            {
                return View(property);
            }

            property.HostId = hostId;
            property.IsActive = false;

            // Save the property first to generate its database ID.
            _context.Properties.Add(property);
            await _context.SaveChangesAsync();

            // Save the optional image and link it to the property.
            if (propertyImage != null &&
                propertyImage.Length > 0 &&
                extension != null)
            {
                var uploadsFolder = Path.Combine(
                    _environment.WebRootPath,
                    "uploads",
                    "properties");

                Directory.CreateDirectory(uploadsFolder);

                // Generate a unique filename rather than using the
                // original filename supplied by the user.
                var fileName = $"{Guid.NewGuid():N}{extension}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(
                    filePath, FileMode.Create))
                {
                    await propertyImage.CopyToAsync(stream);
                }

                var image = new PropertyImage
                {
                    PropertyId = property.Id,
                    ImageUrl = $"/uploads/properties/{fileName}",
                    IsPrimary = true
                };

                _context.Set<PropertyImage>().Add(image);
                await _context.SaveChangesAsync();
            }

            var hostApplication = new HostApplication
            {
                UserId = hostId,
                PropertyId = property.Id,
                Status = "Pending",
                SubmittedAt = DateTime.UtcNow
            };

            _context.HostApplications.Add(hostApplication);
            await _context.SaveChangesAsync();

            TempData["HostMessage"] =
                "Your property and host application have been submitted for approval.";

            return RedirectToAction("Index", "Home");
        }
    }
}