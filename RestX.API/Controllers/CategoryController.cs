using Microsoft.AspNetCore.Mvc;
using RestX.API.Models.DTOs.Request;
using RestX.API.Services.Interfaces;
using AutoMapper;

namespace RestX.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService categoryService;
        private readonly IExceptionHandler exceptionHandler;
        private readonly IMapper mapper;

        public CategoryController(
            ICategoryService categoryService,
            IExceptionHandler exceptionHandler,
            IMapper mapper)
        {
            this.categoryService = categoryService;
            this.exceptionHandler = exceptionHandler;
            this.mapper = mapper;
        }

        /// <summary>
        /// Lấy danh sách tất cả danh mục
        /// </summary>
        /// <returns>Danh sách danh mục</returns>
        [HttpGet]
        public async Task<IActionResult> GetAllCategories()
        {
            try
            {
                var categories = await categoryService.GetCategoriesAsync();
                var categoriesDto = mapper.Map<List<Models.DTOs.Response.CategoryDto>>(categories);
                
                return Ok(new { success = true, data = categoriesDto });
            }
            catch (Exception ex)
            {
                this.exceptionHandler.RaiseException(ex, "An error occurred while loading categories.");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Lấy thông tin chi tiết danh mục theo ID
        /// </summary>
        /// <param name="id">ID của danh mục</param>
        /// <returns>Thông tin chi tiết danh mục</returns>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetCategoryById(int id)
        {
            try
            {
                var categories = await categoryService.GetCategoriesAsync();
                var category = categories.FirstOrDefault(c => c.Id == id);
                
                if (category == null)
                    return NotFound(new { success = false, message = "Category not found" });

                var categoryDto = mapper.Map<Models.DTOs.Response.CategoryDto>(category);
                return Ok(new { success = true, data = categoryDto });
            }
            catch (Exception ex)
            {
                this.exceptionHandler.RaiseException(ex, "An error occurred while loading category details.");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Tạo mới danh mục
        /// </summary>
        /// <param name="request">Thông tin danh mục mới</param>
        /// <returns>Kết quả tạo danh mục</returns>
        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] CategoryRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, message = "Invalid data", errors = ModelState });

                if (string.IsNullOrWhiteSpace(request.Name))
                    return BadRequest(new { success = false, message = "Category name is required." });

                var categoryId = await categoryService.CreateCategoryAsync(request);

                return CreatedAtAction(nameof(GetCategoryById), new { id = categoryId }, 
                    new { success = true, message = "Category created successfully!", categoryId });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                this.exceptionHandler.RaiseException(ex, "An error occurred while creating category.");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Cập nhật danh mục
        /// </summary>
        /// <param name="id">ID của danh mục cần cập nhật</param>
        /// <param name="request">Thông tin cập nhật</param>
        /// <returns>Kết quả cập nhật</returns>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] CategoryRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, message = "Invalid data", errors = ModelState });

                // Note: Cần implement UpdateCategoryAsync trong ICategoryService
                // var result = await categoryService.UpdateCategoryAsync(id, request);
                
                return Ok(new { success = true, message = "Update method not implemented yet" });
            }
            catch (Exception ex)
            {
                this.exceptionHandler.RaiseException(ex, "An error occurred while updating category.");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Xóa danh mục
        /// </summary>
        /// <param name="id">ID của danh mục cần xóa</param>
        /// <returns>Kết quả xóa</returns>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            try
            {
                // Note: Cần implement DeleteCategoryAsync trong ICategoryService
                // await categoryService.DeleteCategoryAsync(id);
                
                return Ok(new { success = true, message = "Delete method not implemented yet" });
            }
            catch (Exception ex)
            {
                this.exceptionHandler.RaiseException(ex, "An error occurred while deleting category.");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }
    }
}
