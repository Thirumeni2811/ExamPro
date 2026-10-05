using Exam.Domain.Enums;
using Exam.Domain.Interfaces;
using Exam.Domain.Models;
using Exam.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Exam.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserService _userService;

        public AccountController(IUserService userService)
        {
            _userService = userService;
        }

        /*--------------------------------------
                       A D M I N
        --------------------------------------*/

        // GET
        [AllowAnonymous]
        [HttpGet]
        [Route("admin-login")]
        public IActionResult AdminLogin()
        {
            return View();
        }

        // LOGIN
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("admin-login")]
        public async Task<IActionResult> AdminLogin(LoginViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var response = await _userService.LoginAdmin(model.Email, model.Password);

                if (!response.Success || response.Data == default)
                {
                    ModelState.AddModelError("", response.Message ?? "Login failed.");
                    return View(model);
                }

                Response.Cookies.Append("Token", response.Data.Token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true, 
                    SameSite = SameSiteMode.Strict,
                });

                return RedirectToAction("Users", "Admin");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error: {ex.Message}");
                return View(model);
            }
        }

        // LOGOUT
        [AllowAnonymous]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("Token");
            return RedirectToAction("Index", "Home");
        }

        /*--------------------------------------
                      U S E R
       --------------------------------------*/

        // GET
        [AllowAnonymous]
        [HttpGet]
        [Route("user-login")]
        public IActionResult UserLogin()
        {
            return View();
        }

        // LOGIN
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("user-login")]
        public async Task<IActionResult> UserLogin(LoginViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var response = await _userService.LoginInvigilator(model.Email, model.Password);

                if (!response.Success || response.Data == default)
                {
                    ModelState.AddModelError("", response.Message ?? "Login failed.");
                    return View(model);
                }

                Response.Cookies.Append("Token", response.Data.Token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                });

                return RedirectToAction("Availability", "Invigilator");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error: {ex.Message}");
                return View(model);
            }
        }

        // GET - UPDATE
        [AllowAnonymous]
        [HttpGet]
        [Route("update-profile")]
        public IActionResult Update()
        {
            return View();
        }
    }

}
