using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using RestX.WebApp.Models;
using RestX.WebApp.Services.Interfaces;

namespace RestX.WebApp.Controllers
{
    public class LoginController : BaseController
    {
        private readonly ILoginService loginService;

        public LoginController(ILoginService loginService, IExceptionHandler exceptionHandler) : base(exceptionHandler)
        {
            this.loginService = loginService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(string username, string password, CancellationToken cancellationToken)
        {
            try
            {
                var account = await loginService.GetAccountByUsernameAsync(username, password, cancellationToken);

                if (account == null)
                {
                    ViewData["Error"] = "Invalid username or password.";
                    return View();
                }

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, account.Username),
                    new Claim(ClaimTypes.Role, account.Role),
                    new Claim("AccountId", account.Id.ToString())
                };

                if (account.StaffId.HasValue)
                {
                    claims.Add(new Claim("StaffId", account.StaffId.Value.ToString()));

                    // Add OwnerId claim for Staff
                    if (account.Staff != null && account.Staff.OwnerId.HasValue)
                    {
                        claims.Add(new Claim("OwnerId", account.Staff.OwnerId.Value.ToString()));
                    }
                }
                else if (account.OwnerId.HasValue)
                {
                    claims.Add(new Claim("OwnerId", account.OwnerId.Value.ToString()));
                }

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme, 
                    new ClaimsPrincipal(claimsIdentity));

                if (account.Role == "Staff")
                {
                    return RedirectToAction("StatusTable", "Staff");
                }
                else if (account.Role == "Owner")
                {
                    return RedirectToAction("DashBoard", "Owner");
                }


                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                this.exceptionHandler.RaiseException(ex, "An error occurred during login.");
                return this.BadRequest("An unexpected error occurred. Please try again later.");
            }
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Login");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
