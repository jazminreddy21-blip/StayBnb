
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StayBnb.Data;
using StayBnb.Models;
using System.Security.Claims;

namespace StayBnb.Controllers
{
    [Authorize(Roles = "Host")]
    public class CheckInController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CheckInController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Display the check-in instructions form
        [HttpGet]
        public async Task<IActionResult> Manage(int propertyId)
        {
            var hostId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            var property = await _context.Properties
                .FirstOrDefaultAsync(p =>
                    p.Id == propertyId &&
                    p.HostId == hostId);

            if (property == null)
            {
                return NotFound();
            }

            var instructions = await _context.CheckInProcesses
                .FirstOrDefaultAsync(c =>
                    c.PropertyId == propertyId);

            if (instructions == null)
            {
                instructions = new CheckInProcess
                {
                    PropertyId = propertyId
                };
            }

            ViewBag.PropertyTitle = property.Title;

            return View(instructions);
        }

        // Save new or updated check-in instructions
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Manage(
            CheckInProcess model)
        {
            var hostId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            var property = await _context.Properties
                .FirstOrDefaultAsync(p =>
                    p.Id == model.PropertyId &&
                    p.HostId == hostId);

            if (property == null)
            {
                return NotFound();
            }

            ModelState.Remove(nameof(CheckInProcess.Property));

            if (!ModelState.IsValid)
            {
                ViewBag.PropertyTitle = property.Title;
                return View(model);
            }

            var existing = await _context.CheckInProcesses
                .FirstOrDefaultAsync(c =>
                    c.PropertyId == model.PropertyId);

            if (existing == null)
            {
                model.UpdatedAt = DateTime.UtcNow;
                _context.CheckInProcesses.Add(model);
            }
            else
            {
                existing.Instructions = model.Instructions;
                existing.RequiredDocuments = model.RequiredDocuments;
                existing.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Check-in instructions saved successfully.";

            return RedirectToAction(
                "Manage",
                new { propertyId = model.PropertyId });
        }
    }
}