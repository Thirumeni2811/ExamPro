using Exam.Domain.Enums;
using Exam.Domain.Interfaces;
using Exam.Domain.Models;
using Exam.Service.Implementations;
using Exam.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Exam.Web.Controllers
{
    [Authorize(Roles = "Invigilator")]
    public class InvigilatorController : Controller
    {
        private readonly IInvigilatorAvailabilityService _invigilatorAvailabilityService;
        private readonly INotificationService _notificationService;

        public InvigilatorController(IInvigilatorAvailabilityService invigilatorAvailabilityService, INotificationService notificationService)
        {
            _invigilatorAvailabilityService = invigilatorAvailabilityService;
            _notificationService = notificationService;
        }

        private Guid? TryGetCurrentInvigilatorId()
        {
            var claim = User?.Claims?.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier || c.Type == "UserId")?.Value;
            if (Guid.TryParse(claim, out var id))
                return id;
            return null;
        }

        // GET
        [HttpGet("invigilator/availability")]
        public async Task<IActionResult> Availability()
        {
            var invigilatorId = TryGetCurrentInvigilatorId();
            if (!invigilatorId.HasValue)
            {
                TempData["ErrorMessage"] = "Session expired. Please log in again.";
                return RedirectToAction("UserLogin", "Account");
            }

            var listResp = await _invigilatorAvailabilityService.GetByInvigilator(invigilatorId.Value);
            var notifications = await _notificationService.GetByUser(invigilatorId.Value);

            if (!listResp.Success)
            {
                TempData["ErrorMessage"] = listResp.Message;
                return View(new List<InvigilatorAvailability>()); 
            }

            var model = new InvigilatorAvViewModel
            {
                Availabilities = listResp.Data?.ToList() ?? new List<InvigilatorAvailability>(),
                Notifications = notifications.Data?.ToList() ?? new List<Notification>()
            };

            return View(model);

        }

        // ADD
        [HttpPost("invigilator/add-availability")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAvailability(DateTime selectedDate)
        {
            var invigilatorId = TryGetCurrentInvigilatorId();
            if (!invigilatorId.HasValue)
            {
                TempData["ErrorMessage"] = "Session expired. Please log in again.";
                return RedirectToAction("UserLogin", "Account");
            }

            // Validate date
            if (selectedDate.Date < DateTime.Today)
            {
                TempData["ErrorMessage"] = "You cannot select a past date.";
                return RedirectToAction("Availability");
            }

            // Allow only this week (Sunday - Saturday)
            var today = DateTime.Today;
            var weekEnd = today.AddDays(DayOfWeek.Saturday - today.DayOfWeek);
            if (selectedDate.Date > weekEnd)
            {
                TempData["ErrorMessage"] = "You can select dates only within this week.";
                return RedirectToAction("Availability");
            }

            // Check if already selected
            var existing = await _invigilatorAvailabilityService.GetByInvigilator(invigilatorId.Value);
            if (existing.Data != null && existing.Data.Any(a => a.Date.Date == selectedDate.Date))
            {
                TempData["ErrorMessage"] = "You have already added this date.";
                return RedirectToAction("Availability");
            }

            // Create new availability
            var newAvailability = new InvigilatorAvailability
            {
                Id = Guid.NewGuid(),
                InvigilatorId = invigilatorId.Value,
                DayOfWeek = (DayOfWeekEnum)selectedDate.DayOfWeek,
                Date = selectedDate.Date,
                CreatedAt = DateTime.UtcNow
            };

            var response = await _invigilatorAvailabilityService.Create(newAvailability);

            if (!response.Success)
            {
                TempData["ErrorMessage"] = response.Message ?? "Failed to add availability.";
            }
            else
            {
                TempData["SuccessMessage"] = "Availability added successfully.";
            }

            return RedirectToAction("Availability");
        }

        // DELETE
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("invigilator/delete-availability/{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var deleteResponse = await _invigilatorAvailabilityService.Delete(id);

                if (!deleteResponse.Success)
                {
                    ModelState.AddModelError("", deleteResponse.Message);
                    return View("Availability", new InvigilatorAvailability());
                }

                TempData["SuccessMessage"] = "Availability deleted successfully.";
                return RedirectToAction("Availability");
            }
            catch
            {
                TempData["ErrorMessage"] = "An unexpected error occurred while deleting availability.";
                return RedirectToAction("Availability");
            }
        }
    }
}
