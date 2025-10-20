using Microsoft.AspNetCore.Mvc;
using RestX.WebApp.Services.Interfaces;

namespace RestX.WebApp.Controllers
{
    [Route("Dish")]
    public class DishController : Controller
    {
        private readonly IDishService dishService;
        private readonly IDishManagementService dishManagementService;
        private readonly IExceptionHandler exceptionHandler;

        public DishController(
            IDishService dishService,
            IDishManagementService dishManagementService,
            IExceptionHandler exceptionHandler)
        {
            this.dishService = dishService;
            this.dishManagementService = dishManagementService;
            this.exceptionHandler = exceptionHandler;
        }

        [HttpGet("Index")]
        public async Task<IActionResult> DishesManagement(CancellationToken cancellationToken)
        {
            try
            {
                var model = await dishManagementService.GetDishesAsync();
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

        [HttpPost("Upsert/{id:int?}")]
        public async Task<IActionResult> UpsertDish([FromForm] Services.DataTransferObjects.Dish request, int? id = null)
        {
            try
            {
                var resultDishId = await dishService.UpsertDishAsync(request);

                if (resultDishId == null)
                    return Json(new { success = false, message = "Operation failed." });

                string operation = request.Id.HasValue && request.Id.Value > 0 ? "updated" : "created";
                return Json(new { success = true, message = $"Dish {operation} successfully!", dishId = resultDishId });
            }
            catch (Exception ex)
            {
                this.exceptionHandler.RaiseException(ex, "An error occurred while saving the dish.");
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpDelete("Delete/{id:int}")]
        public async Task<IActionResult> DeleteDish(int id)
        {
            try
            {
                await dishService.DeleteDishAsync(id);
                return Json(new { success = true, message = "Dish deleted successfully!" });
            }
            catch (Exception ex)
            {
                this.exceptionHandler.RaiseException(ex, "An error occurred while deleting the dish.");
                return Json(new { success = false, message = "An error occurred while deleting the dish." });
            }
        }

        [HttpGet("Detail/{id:int}")]
        public async Task<IActionResult> DishDetail(int id)
        {
            try
            {
                var dishViewModel = await dishService.GetDishViewModelByIdAsync(id);
                if (dishViewModel == null)
                    return Json(new { success = false, message = "Dish not found." });

                return Json(new { success = true, data = dishViewModel });
            }
            catch (Exception ex)
            {
                this.exceptionHandler.RaiseException(ex, "An error occurred while loading dish detail.");
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}