using Microsoft.AspNetCore.Mvc;
using RestX.WebApp.Models;
using RestX.WebApp.Services.Interfaces;
using System.Diagnostics;
using RestX.WebApp.Models.ViewModels;
using RestX.WebApp.Helper;

namespace RestX.WebApp.Controllers
{
    public class OwnerController : BaseController
    {
        private readonly IDashboardService dashboardService;
        private readonly IOwnerService ownerService;

        public OwnerController(
            IDashboardService dashboardService,
            IOwnerService ownerService,
            IExceptionHandler exceptionHandler) : base(exceptionHandler)
        {
            this.dashboardService = dashboardService;
            this.ownerService = ownerService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var vm = await ownerService.GetOwnerProfileViewModelAsync();
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Index(OwnerProfileViewModel vm)
        {
            var (success, passwordMessage) = await ownerService.UpdateOwnerProfileAsync(vm);

            if (success)
            {
                return Json(new { success = true, message = passwordMessage ?? "Profile updated successfully!" });
            }
            else
            {
                return Json(new { success = false, message = passwordMessage ?? "Update failed!" });
            }
        }

        public async Task<IActionResult> DashBoard(CancellationToken cancellationToken)
        {
            try
            {
                var model = await dashboardService.GetDashboardViewModelAsync(cancellationToken);
                return View(model);
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Index", "Login");
            }
            catch (Exception ex)
            {
                this.exceptionHandler.RaiseException(ex, "An error occurred while loading the dashboard.");
                return View("Error");
            }
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}