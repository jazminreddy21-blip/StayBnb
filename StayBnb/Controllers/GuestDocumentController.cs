using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StayBnb.Data;
using StayBnb.Models;
using System.Security.Claims;

namespace StayBnb.Controllers
{
    [Authorize]
    public class GuestDocumentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public GuestDocumentController(
            ApplicationDbContext context,
            IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        [HttpGet]
        public async Task<IActionResult> Upload(int bookingId)
        {
            var guestId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            var booking = await _context.Bookings
                .Include(b => b.Property)
                .FirstOrDefaultAsync(b =>
                    b.Id == bookingId &&
                    b.GuestId == guestId);

            if (booking == null)
            {
                return NotFound();
            }

            ViewBag.BookingId = booking.Id;
            ViewBag.PropertyTitle = booking.Property?.Title;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(
            int bookingId,
            string documentType,
            IFormFile? file)
        {
            var guestId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            var booking = await _context.Bookings
                .Include(b => b.Property)
                .FirstOrDefaultAsync(b =>
                    b.Id == bookingId &&
                    b.GuestId == guestId);

            if (booking == null)
            {
                return NotFound();
            }

            ViewBag.BookingId = booking.Id;
            ViewBag.PropertyTitle = booking.Property?.Title;

            if (string.IsNullOrWhiteSpace(documentType))
            {
                ModelState.AddModelError(
                    nameof(documentType),
                    "Please select a document type.");
            }

            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError(
                    nameof(file),
                    "Please choose a file to upload.");
            }
            else
            {
                var extension = Path.GetExtension(file.FileName)
                    .ToLowerInvariant();

                var allowedExtensions = new[]
                {
                    ".pdf", ".jpg", ".jpeg", ".png"
                };

                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError(
                        nameof(file),
                        "Only PDF, JPG and PNG files are allowed.");
                }

                if (file.Length > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError(
                        nameof(file),
                        "The file must not exceed 5 MB.");
                }
            }

            if (!ModelState.IsValid)
            {
                return View();
            }

            var uploadFolder = Path.Combine(
                _environment.ContentRootPath,
                "App_Data",
                "GuestDocuments");

            Directory.CreateDirectory(uploadFolder);

            var savedFileName =
                $"{Guid.NewGuid()}{Path.GetExtension(file!.FileName).ToLowerInvariant()}";

            var filePath = Path.Combine(
                uploadFolder,
                savedFileName);

            using (var stream = new FileStream(
                filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var document = new GuestDocument
            {
                BookingId = booking.Id,
                GuestId = guestId!,
                DocumentType = documentType.Trim(),
                FilePath = savedFileName,
                Status = "Pending",
                UploadedAt = DateTime.UtcNow
            };

            _context.GuestDocuments.Add(document);
            await _context.SaveChangesAsync();

            TempData["DocumentMessage"] =
                "Your document was uploaded successfully and is awaiting verification.";

            return RedirectToAction(
                nameof(MyDocuments),
                new { bookingId = booking.Id });
        }

        [HttpGet]
        public async Task<IActionResult> MyDocuments(int bookingId)
        {
            var guestId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            var booking = await _context.Bookings
                .Include(b => b.Property)
                .FirstOrDefaultAsync(b =>
                    b.Id == bookingId &&
                    b.GuestId == guestId);

            if (booking == null)
            {
                return NotFound();
            }

            var documents = await _context.GuestDocuments
                .Where(d =>
                    d.BookingId == bookingId &&
                    d.GuestId == guestId)
                .OrderByDescending(d => d.UploadedAt)
                .ToListAsync();

            ViewBag.BookingId = booking.Id;
            ViewBag.PropertyTitle = booking.Property?.Title;

            return View(documents);
        }

        [HttpGet]
        [Authorize(Roles = "Host,Admin,SuperAdmin")]
        public async Task<IActionResult> Review()
        {
            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            var documentsQuery = _context.GuestDocuments
                .Include(d => d.Booking)
                    .ThenInclude(b => b!.Property)
                .Include(d => d.Guest)
                .AsQueryable();

            // Hosts may only review documents for their own properties.
            if (User.IsInRole("Host") &&
                !User.IsInRole("Admin") &&
                !User.IsInRole("SuperAdmin"))
            {
                documentsQuery = documentsQuery.Where(d =>
                    d.Booking != null &&
                    d.Booking.Property != null &&
                    d.Booking.Property.HostId == userId);
            }

            var documents = await documentsQuery
                .OrderByDescending(d => d.UploadedAt)
                .ToListAsync();

            return View(documents);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Host,Admin,SuperAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Host,Admin,SuperAdmin")]
        public async Task<IActionResult> Verify(
    int id,
    string status,
    string? verificationNotes)
        {
            if (status != "Verified" && status != "Rejected")
            {
                return BadRequest("Invalid document status.");
            }

            var document = await _context.GuestDocuments
                .Include(d => d.Booking)
                    .ThenInclude(b => b!.Property)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (document == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            bool isAdmin =
                User.IsInRole("Admin") ||
                User.IsInRole("SuperAdmin");

            bool ownsProperty =
                document.Booking?.Property?.HostId == userId;

            if (!isAdmin && !ownsProperty)
            {
                return Forbid();
            }

            document.Status = status;
            document.VerificationNotes =
                string.IsNullOrWhiteSpace(verificationNotes)
                    ? null
                    : verificationNotes.Trim();

            document.VerifiedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["DocumentReviewMessage"] =
                status == "Verified"
                    ? "The document has been successfully verified."
                    : "The document has been rejected.";

            return RedirectToAction(nameof(Review));
        }


        [HttpGet]
        [Authorize(Roles = "Host,Admin,SuperAdmin")]
        public async Task<IActionResult> ViewDocument(int id)
        {
            var document = await _context.GuestDocuments
                .Include(d => d.Booking)
                    .ThenInclude(b => b!.Property)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (document == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            bool isPrivileged =
                User.IsInRole("Admin") ||
                User.IsInRole("SuperAdmin");

            bool ownsProperty =
                document.Booking?.Property?.HostId == userId;

            if (!isPrivileged && !ownsProperty)
            {
                return Forbid();
            }

            var extension = Path.GetExtension(document.FilePath)
                .ToLowerInvariant();

            var contentType = extension switch
            {
                ".pdf" => "application/pdf",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                _ => "application/octet-stream"
            };

            var uploadFolder = Path.Combine(
                _environment.ContentRootPath,
                "App_Data",
                "GuestDocuments");

            var filePath = Path.Combine(
                uploadFolder,
                Path.GetFileName(document.FilePath));

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("The uploaded file could not be found.");
            }

            return PhysicalFile(
                filePath,
                contentType,
                enableRangeProcessing: true);
        }
    }
}