using Exam.Domain.Enums;
using Exam.Domain.Interfaces;
using Exam.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Exam.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IUserService _userService;
        private readonly IVenueService _venueService;
        private readonly IInvigilatorAvailabilityService _invigilatorAvailabilityService;
        private readonly IAllocationService _allocationService;

        public AdminController(IUserService userService, IVenueService venueService, IInvigilatorAvailabilityService invigilatorAvailabilityService, IAllocationService allocationService)
        {
            _userService = userService;
            _venueService = venueService;
            _invigilatorAvailabilityService = invigilatorAvailabilityService;
            _allocationService = allocationService;
        }

        /*--------------------------------------
                      U S E R
        --------------------------------------*/

        // Load all
        private async Task LoadAllUsersForView()
        {
            var allUsersResponse = await _userService.GetAll();
            ViewBag.UsersList = (allUsersResponse.Data ?? Enumerable.Empty<User>()).ToList();
        }

        // Email Validation
        private static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address.Equals(email, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }


        // GET
        [HttpGet("admin/users")]
        public async Task<IActionResult> Users(
            string? userId,
            string? name,
            string? email,
            string? phoneNo,
            Guid? id,
            string? active)
        {
            try
            {
                // --- Parse Id filter ---
                Guid? idFilter = null;
                if (!string.IsNullOrWhiteSpace(userId) && Guid.TryParse(userId, out var idParsed))
                    idFilter = idParsed;

                // --- Parse Active filter ---
                bool? activeFilter = null;
                if (!string.IsNullOrWhiteSpace(active))
                {
                    if (bool.TryParse(active, out var activeBool))
                    {
                        activeFilter = activeBool;
                    }
                    else if (active == "1") activeFilter = true;
                    else if (active == "0") activeFilter = false;
                }

                // --- Call service with correct param order ---
                var allUsersResponse = await _userService.GetAll(
                    name,
                    idFilter,
                    email,
                    phoneNo,
                    activeFilter
                );

                if (!allUsersResponse.Success)
                {
                    TempData["ErrorMessage"] = allUsersResponse.Message;
                    return RedirectToAction("Users");
                }

                var usersList = (allUsersResponse.Data ?? Enumerable.Empty<User>()).ToList();

                User? selectedUser = null;
                if (id.HasValue)
                {
                    var getUserResponse = await _userService.GetById(id.Value);
                    if (getUserResponse.Success)
                        selectedUser = getUserResponse.Data;
                }

                // Pass data to view
                ViewBag.UsersList = usersList;
                ViewData["UserId"] = userId;
                ViewData["Name"] = name;
                ViewData["Email"] = email;
                ViewData["Phone"] = phoneNo;
                ViewData["Active"] = active;

                ModelState.Clear();
                return View(selectedUser ?? new User());
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An unexpected error occurred while loading users.";
                return RedirectToAction("Users");
            }
        }

        // POST - Create and Update
        [HttpPost("admin/users")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Users(User model)
        {
            try
            {
                if (!IsValidEmail(model.Email))
                    ModelState.AddModelError(nameof(model.Email), "Please enter a valid email address.");

                if (model.Id == Guid.Empty)
                {
                    if (string.IsNullOrWhiteSpace(model.Password))
                        ModelState.AddModelError(nameof(model.Password), "Password is required.");

                    if (model.Password?.Length < 8)
                        ModelState.AddModelError(nameof(model.Password), "Password must be at least 8 characters long.");

                }
                else
                {
                    ModelState.Remove(nameof(model.Password));
                    model.Password = string.Empty;
                }

                if (!ModelState.IsValid)
                {
                    await LoadAllUsersForView();
                    return View(model);
                }

                if (model.Id == Guid.Empty)
                {
                    // CREATE
                    var createResponse = await _userService.Create(model);
                    if (!createResponse.Success)
                    {
                        TempData["ErrorMessage"] = createResponse.Message;
                        await LoadAllUsersForView();
                        return View(model);
                    }

                    TempData["SuccessMessage"] = "User created successfully.";
                }
                else
                {
                    // UPDATE
                    var updateResponse = await _userService.Update(model);
                    if (!updateResponse.Success)
                    {
                        TempData["ErrorMessage"] = updateResponse.Message;
                        await LoadAllUsersForView();
                        return View(model);
                    }

                    TempData["SuccessMessage"] = "User updated successfully.";
                }

                return RedirectToAction("Users");
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An unexpected error occurred while saving user.";
                return RedirectToAction("Users");
            }
        }

        // DELETE
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("admin/users/delete/{id}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            try
            {
                var deleteResponse = await _userService.Delete(id);

                if (!deleteResponse.Success)
                {
                    ModelState.AddModelError("", deleteResponse.Message);
                    await LoadAllUsersForView();
                    return View("Users", new User());
                }

                TempData["SuccessMessage"] = "User deleted successfully.";
                return RedirectToAction("Users");
            }
            catch
            {
                TempData["ErrorMessage"] = "An unexpected error occurred while deleting user.";
                return RedirectToAction("Users");
            }
        }

        // TOGGLE ACTIVE STATUS
        [HttpPost("admin/users/toggle-active")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActiveStatus(Guid userId, bool isActive)
        {
            try
            {
                var userResp = await _userService.GetById(userId);
                if (!userResp.Success || userResp.Data is null)
                    return Json(new { success = false, message = "User not found." });

                var user = userResp.Data;
                user.Active = isActive;

                var updateResp = await _userService.Update(user);
                if (!updateResp.Success)
                    return Json(new { success = false, message = updateResp.Message });

                return Json(new { success = true, message = "Status updated successfully.", active = user.Active });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        /*--------------------------------------
                     V E N U E
        --------------------------------------*/
        // Load all
        private async Task LoadAllVenuesForView()
        {
            var allVenues = await _venueService.GetAll();
            ViewBag.venueList = (allVenues.Data ?? Enumerable.Empty<Venue>()).ToList();
        }

        // GET
        [HttpGet("admin/venue")]
        public async Task<IActionResult> Venue(
            string? name,
            string? hallNumber,
            Guid? id)
        {
            try
            {
                var allVenue = await _venueService.GetAll(
                    name, hallNumber
                );

                if (!allVenue.Success)
                {
                    TempData["ErrorMessage"] = allVenue.Message;
                    return RedirectToAction("Venue");
                }

                var venueList = (allVenue.Data ?? Enumerable.Empty<Venue>()).ToList();

                Venue? selectedVenue = null;
                if (id.HasValue)
                {
                    var getVenueResponse = await _venueService.GetById(id.Value);
                    if (getVenueResponse.Success)
                        selectedVenue = getVenueResponse.Data;
                }

                // Pass data to view
                ViewBag.VenueList = venueList;
                ViewData["Name"] = name;
                ViewData["HallNumber"] = hallNumber;

                ModelState.Clear();
                return View(selectedVenue ?? new Venue());
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An unexpected error occurred while loading venue.";
                return RedirectToAction("Venue");
            }
        }

        [HttpPost("admin/venue")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Venue(Venue model)
        {
            try
            {
                if (model.Capacity <= 1)
                    ModelState.AddModelError(nameof(model.Capacity), "Capacity must be greater than 1.");

                if (!ModelState.IsValid)
                {
                    await LoadAllVenuesForView();
                    return View("Venue", model);
                }

                if (model.Id == Guid.Empty)
                {
                    // CREATE
                    var createResponse = await _venueService.Create(model);
                    if (!createResponse.Success)
                    {
                        TempData["ErrorMessage"] = createResponse.Message;
                        await LoadAllVenuesForView();
                        return View("Venue", model);
                    }

                    TempData["SuccessMessage"] = "Venue created successfully.";
                }
                else
                {
                    // UPDATE
                    var updateResponse = await _venueService.Update(model);
                    if (!updateResponse.Success)
                    {
                        TempData["ErrorMessage"] = updateResponse.Message;
                        await LoadAllVenuesForView();
                        return View("Venue", model);
                    }

                    TempData["SuccessMessage"] = "Venue updated successfully.";
                }

                return RedirectToAction("Venue");
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An unexpected error occurred while saving venue.";
                return RedirectToAction("Venue");
            }
        }


        // DELETE
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("admin/venue/delete/{id}")]
        public async Task<IActionResult> DeleteVenue(Guid id)
        {
            try
            {
                var deleteResponse = await _venueService.Delete(id);

                if (!deleteResponse.Success)
                {
                    ModelState.AddModelError("", deleteResponse.Message);
                    await LoadAllVenuesForView();
                    return View("Venue", new Venue());
                }

                TempData["SuccessMessage"] = "Venue deleted successfully.";
                return RedirectToAction("Venue");
            }
            catch
            {
                TempData["ErrorMessage"] = "An unexpected error occurred while deleting venue.";
                return RedirectToAction("Venue");
            }
        }

        /*----------------------------------------------------
           I N V I G I L A T O R   A V A I L A B I L I T Y
        ----------------------------------------------------*/
        // GET
        [HttpGet("admin/invigilator")]
        public async Task<IActionResult> Invigilator(
            string? invigilatorName,
            string? dayOfWeek)
        {
            try
            {
                DayOfWeekEnum? dayEnum = null;

                if (!string.IsNullOrWhiteSpace(dayOfWeek) && Enum.TryParse<DayOfWeekEnum>(dayOfWeek, true, out var parsedDay))
                {
                    dayEnum = parsedDay;
                }

                var allAvailabilty = await _invigilatorAvailabilityService.GetAll(
                    invigilatorName, dayEnum
                );

                if (!allAvailabilty.Success)
                {
                    TempData["ErrorMessage"] = allAvailabilty.Message;
                    return RedirectToAction("Invigilator");
                }

                var availabilityList = (allAvailabilty.Data ?? Enumerable.Empty<InvigilatorAvailability>()).ToList();

                ViewBag.AvailabilityList = availabilityList;
                ViewData["InvigilatorName"] = invigilatorName;
                ViewData["DayOfWeek"] = dayOfWeek;

                ModelState.Clear();
                return View();
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An unexpected error occurred while loading invigilators.";
                return RedirectToAction("Invigilator");
            }
        }

        // -------------------------------------------------
        //               A L L O C A T I O N S
        // -------------------------------------------------

        // Load all
        private async Task LoadAllAllocationForView()
        {
            var allAllocationsResponse = await _allocationService.GetAll();
            ViewBag.AllocationList = (allAllocationsResponse.Data ?? Enumerable.Empty<Allocation>()).ToList();
        }

        // GET: Allocation List
        [HttpGet("admin/allocation")]
        public async Task<IActionResult> Allocation(
            string? venueName,
            string? hallNumber,
            string? invigilatorName,
            string? invigilatorEmail,
            DateTime? date,
            AllocationStatus? status)
        {
            try
            {
                var response = await _allocationService.GetAll(venueName, hallNumber, invigilatorName, invigilatorEmail, date, status);

                if (!response.Success)
                {
                    TempData["ErrorMessage"] = response.Message;
                    return View(new List<Allocation>());
                }

                var allocationList = (response.Data ?? Enumerable.Empty<Allocation>()).ToList();

                ViewBag.AllocationList = allocationList;
                ViewBag.VenueName = venueName;
                ViewBag.HallNumber = hallNumber;
                ViewBag.InvigilatorName = invigilatorName;
                ViewBag.InvigilatorEmail = invigilatorEmail;
                ViewBag.Date = date?.ToString("yyyy-MM-dd");
                ViewBag.Status = status;

                ModelState.Clear();
                return View();
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An unexpected error occurred while loading allocations.";
                return RedirectToAction("Allocation");
            }
        }

        // DELETE
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("admin/users/allocation/{id}")]
        public async Task<IActionResult> DeleteAllocation(Guid id)
        {
            try
            {
                var deleteResponse = await _allocationService.Delete(id);

                if (!deleteResponse.Success)
                {
                    ModelState.AddModelError("", deleteResponse.Message);
                    await LoadAllAllocationForView();
                    return View("Allocation", new Allocation());
                }

                return RedirectToAction("Allocation");
            }
            catch
            {
                TempData["ErrorMessage"] = "An unexpected error occurred while deleting allocation.";
                return RedirectToAction("Allocation");
            }
        }

        // ---------------------------------------------------------
        // GET: Admin Allocation Management 
        // ---------------------------------------------------------
        [HttpGet("admin/allocation/manage")]
        public async Task<IActionResult> ManageAllocation(string? date)
        {
            try
            {
                DateTime? selectedDate = null;
                if (!string.IsNullOrWhiteSpace(date) && DateTime.TryParse(date, out var parsed))
                    selectedDate = parsed.Date;

                List<Allocation> allocationsForDate = new();
                if (selectedDate.HasValue)
                {
                    var resp = await _allocationService.GetAll(
                        venueName: null,
                        hallNumber: null,
                        invigilatorName: null,
                        invigilatorEmail: null,
                        date: selectedDate.Value,
                        status: null
                    );

                    if (!resp.Success)
                    {
                        TempData["ErrorMessage"] = resp.Message;
                    }
                    else
                    {
                        allocationsForDate = resp.Data?.ToList() ?? new List<Allocation>();
                    }
                }

                ViewBag.SelectedDate = selectedDate?.ToString("yyyy-MM-dd");
                ViewBag.Allocations = allocationsForDate;

                return View();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An unexpected error occurred while loading allocations.";
                return View();
            }
        }

        // ---------------------------------------------------------
        // POST: Generate Draft for a date
        // ---------------------------------------------------------
        [HttpPost("admin/allocation/generate")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateAllocation(string date)
        {
            try
            {
                if (!DateTime.TryParse(date, out var parsed))
                {
                    TempData["ErrorMessage"] = "Please choose a valid date.";
                    return RedirectToAction("ManageAllocation", new { date });
                }

                var resp = await _allocationService.GenerateDraft(parsed.Date);
                if (!resp.Success)
                {
                    TempData["ErrorMessage"] = resp.Message;
                    return RedirectToAction("ManageAllocation", new { date = parsed.ToString("yyyy-MM-dd") });
                }

                TempData["SuccessMessage"] = "Draft allocations generated.";
                return RedirectToAction("ManageAllocation", new { date = parsed.ToString("yyyy-MM-dd") });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An unexpected error occurred while loading allocations.";
                return View();
            }
        }

        // ---------------------------------------------------------
        // POST: Approve all Draft for a date
        // ---------------------------------------------------------
        [HttpPost("admin/allocation/approve")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveAllocation(string date)
        {
            try
            {
                if (!DateTime.TryParse(date, out var parsed))
                {
                    TempData["ErrorMessage"] = "Please choose a valid date.";
                    return RedirectToAction("ManageAllocation", new { date });
                }

                var resp = await _allocationService.ApproveDraft(parsed.Date);
                if (!resp.Success)
                {
                    TempData["ErrorMessage"] = resp.Message;
                    return RedirectToAction("ManageAllocation", new { date = parsed.ToString("yyyy-MM-dd") });
                }

                TempData["SuccessMessage"] = "Draft allocations approved.";
                return RedirectToAction("ManageAllocation", new { date = parsed.ToString("yyyy-MM-dd") });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An unexpected error occurred while loading allocations.";
                return View();
            }
        }

        // ---------------------------------------------------------
        // POST: Publish all Approved for a date
        // ---------------------------------------------------------
        [HttpPost("admin/allocation/publish")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PublishAllocation(string date)
        {
            try
            {
                if (!DateTime.TryParse(date, out var parsed))
                {
                    TempData["ErrorMessage"] = "Please choose a valid date.";
                    return RedirectToAction("ManageAllocation", new { date });
                }

                var resp = await _allocationService.Publish(parsed.Date);
                if (!resp.Success)
                {
                    TempData["ErrorMessage"] = resp.Message;
                    return RedirectToAction("ManageAllocation", new { date = parsed.ToString("yyyy-MM-dd") });
                }

                TempData["SuccessMessage"] = "Allocations published.";
                return RedirectToAction("ManageAllocation", new { date = parsed.ToString("yyyy-MM-dd") });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An unexpected error occurred while loading allocations.";
                return View();
            }
        }
    }
}
