using Microsoft.AspNetCore.Mvc;
using RestX.WebApp.Services.Interfaces;
using RestX.WebApp.Helper;

namespace RestX.WebApp.Controllers
{
    [Route("Category")]
    public class CategoryController : Controller
    {
        private readonly ICategoryService categoryService;
        private readonly IExceptionHandler exceptionHandler;

        public CategoryController(
            ICategoryService categoryService,
            IExceptionHandler exceptionHandler)
        {
            this.categoryService = categoryService;
            this.exceptionHandler = exceptionHandler;
        }

        [HttpGet("Index")]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                var categories = await categoryService.GetCategoriesAsync();
                var data = categories.Select(c => new { id = c.Id, name = c.Name }).ToList();
                return Json(new { success = true, data });
            }
            catch (Exception ex)
            {
                this.exceptionHandler.RaiseException(ex, "An error occurred while loading categories.");
                return Json(new { success = false, message = "Failed to load categories." });
            }
        }

        [HttpPost("Create")]
        public async Task<IActionResult> CreateCategory([FromBody] Services.DataTransferObjects.Category request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Name))
                    return Json(new { success = false, message = "Category name is required." });

                var categoryId = await categoryService.CreateCategoryAsync(request);

                return Json(new { success = true, message = "Category created successfully!", categoryId });
            }
            catch (InvalidOperationException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                this.exceptionHandler.RaiseException(ex, "An error occurred while creating category.");
                return Json(new { success = false, message = "An error occurred while creating category." });
            }
        }
    }
}