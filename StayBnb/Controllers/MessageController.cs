
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StayBnb.Data;
using StayBnb.Models;
using System.Security.Claims;

namespace StayBnb.Controllers
{
    [Authorize]
    public class MessageController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MessageController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Display messages received or sent by the logged-in user.
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            var messages = await _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .Where(m =>
                    m.SenderId == userId ||
                    m.ReceiverId == userId)
                .OrderByDescending(m => m.SentAt)
                .ToListAsync();

            return View(messages);
        }

        // Display the conversation with another user.
        [HttpGet]
        public async Task<IActionResult> Conversation(string userId)
        {
            var currentUserId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId) ||
                userId == currentUserId)
            {
                return BadRequest();
            }

            var isAllowed = await CanMessageAsync(
                currentUserId!, userId);

            if (!isAllowed)
            {
                return Forbid();
            }

            var messages = await _context.Messages
                .Where(m =>
                    (m.SenderId == currentUserId &&
                     m.ReceiverId == userId) ||
                    (m.SenderId == userId &&
                     m.ReceiverId == currentUserId))
                .OrderBy(m => m.SentAt)
                .ToListAsync();

            var unreadMessages = await _context.Messages
                .Where(m =>
                    m.SenderId == userId &&
                    m.ReceiverId == currentUserId &&
                    !m.IsRead)
                .ToListAsync();

            foreach (var message in unreadMessages)
            {
                message.IsRead = true;
            }

            if (unreadMessages.Count > 0)
            {
                await _context.SaveChangesAsync();
            }

            ViewBag.OtherUserId = userId;

            ViewBag.OtherUserName = await _context.Users
    .Where(u => u.Id == userId)
    .Select(u => u.UserName)
    .FirstOrDefaultAsync() ?? "StayBnb member";

            return View(messages);
        }

        // Send a message or reply.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Send(
            string receiverId,
            string content)
        {
            var senderId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(senderId))
            {
                return Challenge();
            }

            if (string.IsNullOrWhiteSpace(receiverId) ||
                receiverId == senderId)
            {
                return BadRequest();
            }

            if (string.IsNullOrWhiteSpace(content) ||
                content.Trim().Length > 2000)
            {
                TempData["MessageError"] =
                    "Enter a message of 1 to 2000 characters.";

                return RedirectToAction(
                    "Conversation",
                    new { userId = receiverId });
            }

            var isAllowed = await CanMessageAsync(
                senderId, receiverId);

            if (!isAllowed)
            {
                return Forbid();
            }

            var message = new Message
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                Content = content.Trim(),
                SentAt = DateTime.UtcNow,
                IsRead = false
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            return RedirectToAction(
                "Conversation",
                new { userId = receiverId });
        }

        // Allow messages only between the guest and host
        // of a confirmed booking.
        private async Task<bool> CanMessageAsync(
            string firstUserId,
            string secondUserId)
        {
            return await _context.Bookings
                .AnyAsync(b =>
                    b.Status == "Confirmed" &&
                    b.Property != null &&
                    (
                        (b.GuestId == firstUserId &&
                         b.Property.HostId == secondUserId)
                        ||
                        (b.GuestId == secondUserId &&
                         b.Property.HostId == firstUserId)
                    ));
        }
    }
}