using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestX.UI.Models.ViewModels;
using RestX.UI.Services.Interfaces;

namespace RestX.UI.Controllers
{
    //[Authorize(Roles = "Owner,Staff")]
    public class StaffController : Controller
    {
        private readonly IStaffService _tableService;

        public StaffController(IStaffService tableService)
        {
            _tableService = tableService;
        }

        [HttpGet]
        public async Task<IActionResult>StatusTable()
        {
            var tables = await _tableService.GetAllTablesAsync();
            Console.WriteLine($"Fetched tables: {tables?.Count()}");
            return View(tables ?? new List<TableStatusViewModel>());
        }
        [HttpGet]
        public async Task<IActionResult> CustomerRequests()
        {
            var customerRequests = await _tableService.GetAllCustomerRequestsAsync();
            return View(customerRequests); 
        }

        [HttpGet]
        public async Task<IActionResult> Menu()
        {
            var menu = await _tableService.GetMenuAsync();
            return View(menu);
        }


    }
}