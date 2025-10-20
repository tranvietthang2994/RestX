using Microsoft.AspNetCore.Mvc;
using RestX.WebApp.Helper;
using RestX.WebApp.Models;
using RestX.WebApp.Models.ViewModels;
using RestX.WebApp.Services.Interfaces;
using RestX.WebApp.Services.Services;
using System.Diagnostics;

namespace RestX.WebApp.Controllers
{
    [Route("Table")]
    public class TableController : BaseController
    {
        private readonly ITableService tableService;
        private readonly IHttpContextAccessor httpContextAccessor;
        public TableController(ITableService tableService,
                               IHttpContextAccessor httpContextAccessor,
                               IExceptionHandler exceptionHandler) : base(exceptionHandler)
        {
            this.tableService = tableService;
            this.httpContextAccessor = httpContextAccessor;
        }

        [HttpGet("createqr")]
        public async Task<IActionResult> TableQrCode(CancellationToken cancellationToken)
        {
            try
            {
                var user = Helper.UserHelper.HttpContextAccessor.HttpContext?.User;
                var ownerIdClaim = user?.FindFirst("OwnerId");
                var ownerIdString = ownerIdClaim.ToString();
                ownerIdString = ownerIdString.Substring(ownerIdString.IndexOf(": ") + 1);
                var ownerId = Guid.Parse(ownerIdString);


                var model = await tableService.GetTablesByOwnerIdAsync(ownerId);
                return View(model);
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Index", "Login");
            }
            catch (Exception ex)
            {
                this.exceptionHandler.RaiseException(ex, "An error occurred while loading dishes management.");
                return View("Error");
            }
        }

        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var tables = await tableService.GetAllTablesAsync();
                ViewBag.TableStatuses = await tableService.GetTableStatus();
                return View(tables);
            }
            catch (Exception ex)
            {
                this.exceptionHandler.RaiseException(ex, "An error occurred while loading tables.");
                return View("Error");
            }
        }

        [HttpGet("Detail/{id:int}")]
        public async Task<IActionResult> TableDetail(int id)
        {
            var table = await tableService.GetTableViewModelByIdAsync(id);
            if (table == null)
                return Json(new { success = false, message = "Table not found." });
            return Json(new { success = true, data = table });
        }


        [HttpPost("Upsert")]
        public async Task<IActionResult> UpsertTable([FromForm] TableViewModel model)
        {
            try
            {
                var resultId = await tableService.UpsertTableAsync(model);
                if (resultId == null)
                    return Json(new { success = false, message = "Operation failed." });
                
                string operation = model.Id != 0 ? "updated" : "created";
                return Json(new { success = true, message = $"Table {operation} successfully!", tableId = resultId });
            }
            catch (Exception ex)
            {
                this.exceptionHandler.RaiseException(ex, "An error occurred while saving the table.");
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpDelete("Delete/{id:int}")]
        public async Task<IActionResult> DeleteTable(int id)
        {
            try
            {
                var result = await tableService.DeleteTableAsync(id);
                if (!result)
                    return Json(new { success = false, message = "Table not found." });
                return Json(new { success = true, message = "Table deleted successfully!" });
            }
            catch (Exception ex)
            {
                this.exceptionHandler.RaiseException(ex, "An error occurred while deleting the table.");
                return Json(new { success = false, message = ex.Message });
            }
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
