using Microsoft.AspNetCore.Mvc;
using RestX.API.Models.DTOs.Request;
using RestX.API.Services.Interfaces;

namespace RestX.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DishController : ControllerBase
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

        /// <summary>
        /// Lấy danh sách tất cả món ăn
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>Danh sách món ăn</returns>
        [HttpGet]
        public async Task<IActionResult> GetAllDishes(CancellationToken cancellationToken)
        {
            try
            {
                var dishes = await dishManagementService.GetDishesAsync();
                return Ok(new { success = true, data = dishes });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { success = false, message = "Unauthorized access" });
            }
            catch (Exception ex)
            {
                this.exceptionHandler.RaiseException(ex, "An error occurred while loading dishes.");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Tạo mới món ăn
        /// </summary>
        /// <param name="request">Thông tin món ăn</param>
        /// <returns>Kết quả tạo món ăn</returns>
        [HttpPost]
        public async Task<IActionResult> CreateDish([FromForm] DishRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, message = "Invalid data", errors = ModelState });

                var resultDishId = await dishService.UpsertDishAsync(request);

                if (resultDishId == null)
                    return BadRequest(new { success = false, message = "Operation failed." });

                return CreatedAtAction(nameof(GetDishById), new { id = resultDishId }, 
                    new { success = true, message = "Dish created successfully!", dishId = resultDishId });
            }
            catch (Exception ex)
            {
                this.exceptionHandler.RaiseException(ex, "An error occurred while creating the dish.");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Cập nhật món ăn
        /// </summary>
        /// <param name="id">ID món ăn</param>
        /// <param name="request">Thông tin cập nhật</param>
        /// <returns>Kết quả cập nhật</returns>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateDish(int id, [FromForm] DishRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, message = "Invalid data", errors = ModelState });

                request.Id = id;
                var resultDishId = await dishService.UpsertDishAsync(request);

                if (resultDishId == null)
                    return NotFound(new { success = false, message = "Dish not found" });

                return Ok(new { success = true, message = "Dish updated successfully!", dishId = resultDishId });
            }
            catch (Exception ex)
            {
                this.exceptionHandler.RaiseException(ex, "An error occurred while updating the dish.");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Xóa món ăn
        /// </summary>
        /// <param name="id">ID món ăn cần xóa</param>
        /// <returns>Kết quả xóa</returns>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteDish(int id)
        {
            try
            {
                await dishService.DeleteDishAsync(id);
                return Ok(new { success = true, message = "Dish deleted successfully!" });
            }
            catch (Exception ex)
            {
                this.exceptionHandler.RaiseException(ex, "An error occurred while deleting the dish.");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Lấy thông tin chi tiết món ăn theo ID
        /// </summary>
        /// <param name="id">ID món ăn</param>
        /// <returns>Thông tin chi tiết món ăn</returns>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetDishById(int id)
        {
            try
            {
                var dishViewModel = await dishService.GetDishViewModelByIdAsync(id);
                if (dishViewModel == null)
                    return NotFound(new { success = false, message = "Dish not found" });

                return Ok(new { success = true, data = dishViewModel });
            }
            catch (Exception ex)
            {
                this.exceptionHandler.RaiseException(ex, "An error occurred while loading dish detail.");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }
    }
}