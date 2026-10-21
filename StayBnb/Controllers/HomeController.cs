using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StayBnb.Data;
using StayBnb.Models;
using System.Diagnostics;

namespace StayBnb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(
            string? city,
            DateTime? checkIn,
            DateTime? checkOut,
            int guests = 2)
        {
            var properties = _context.Properties
                .Where(p => p.IsActive);

            if (!string.IsNullOrWhiteSpace(city))
            {
                properties = properties.Where(p =>
                    p.City.Contains(city));
            }

            var availableProperties = await properties
                .Include(p => p.Images)
                .OrderBy(p => p.Title)
                .ToListAsync();

            ViewBag.City = city;
            ViewBag.CheckIn = checkIn;
            ViewBag.CheckOut = checkOut;
            ViewBag.Guests = guests;

            return View(availableProperties);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(
            Duration = 0,
            Location = ResponseCacheLocation.None,
            NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id
                    ?? HttpContext.TraceIdentifier
            });
        }
    }
}