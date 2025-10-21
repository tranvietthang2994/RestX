using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestX.API.Models.ViewModels;
using RestX.API.Services.Interfaces;

namespace RestX.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Owner,Staff")] // Table management
    public class TableController : ControllerBase
    {
        private readonly ITableService tableService;
        private readonly IExceptionHandler exceptionHandler;

        public TableController(
            ITableService tableService,
            IExceptionHandler exceptionHandler)
        {
            this.tableService = tableService;
            this.exceptionHandler = exceptionHandler;
        }

        /// <summary>
        /// Lấy danh sách tất cả bàn
        /// </summary>
        /// <returns>Danh sách bàn</returns>
        [HttpGet]
        public async Task<IActionResult> GetAllTables()
        {
            try
            {
                var tables = await tableService.GetAllTablesAsync();
                return Ok(new { success = true, data = tables });
            }
            catch (Exception ex)
            {
                this.exceptionHandler.RaiseException(ex, "An error occurred while loading tables.");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Lấy danh sách bàn theo Owner ID
        /// </summary>
        /// <returns>Danh sách bàn của nhà hàng</returns>
        [HttpGet("owner")]
        public async Task<IActionResult> GetTablesByOwner()
        {
            try
            {
                // UserHelper sẽ lấy OwnerId từ JWT token
                var ownerId = RestX.API.Extensions.UserHelper.GetCurrentOwnerId();
                var tables = await tableService.GetTablesByOwnerIdAsync(ownerId);
                return Ok(new { success = true, data = tables });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { success = false, message = "Owner not authenticated" });
            }
            catch (Exception ex)
            {
                this.exceptionHandler.RaiseException(ex, "An error occurred while loading owner's tables.");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Lấy thông tin chi tiết bàn theo ID
        /// </summary>
        /// <param name="id">ID của bàn</param>
        /// <returns>Thông tin chi tiết bàn</returns>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetTableById(int id)
        {
            try
            {
                var table = await tableService.GetTableViewModelByIdAsync(id);
                if (table == null)
                    return NotFound(new { success = false, message = "Table not found" });

                return Ok(new { success = true, data = table });
            }
            catch (Exception ex)
            {
                this.exceptionHandler.RaiseException(ex, $"An error occurred while loading table {id}.");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Lấy trạng thái tất cả các bàn
        /// </summary>
        /// <returns>Danh sách trạng thái bàn</returns>
        [HttpGet("statuses")]
        public async Task<IActionResult> GetTableStatuses()
        {
            try
            {
                var tableStatuses = await tableService.GetTableStatus();
                return Ok(new { success = true, data = tableStatuses });
            }
            catch (Exception ex)
            {
                this.exceptionHandler.RaiseException(ex, "An error occurred while loading table statuses.");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Tạo mới bàn
        /// </summary>
        /// <param name="model">Thông tin bàn mới</param>
        /// <returns>Kết quả tạo bàn</returns>
        [HttpPost]
        public async Task<IActionResult> CreateTable([FromBody] TableViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, message = "Invalid data", errors = ModelState });

                // Ensure it's a new table
                model.Id = 0;
                
                var resultId = await tableService.UpsertTableAsync(model);
                if (resultId == null)
                    return BadRequest(new { success = false, message = "Operation failed" });

                return CreatedAtAction(nameof(GetTableById), new { id = resultId }, 
                    new { success = true, message = "Table created successfully!", tableId = resultId });
            }
            catch (Exception ex)
            {
                this.exceptionHandler.RaiseException(ex, "An error occurred while creating the table.");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Cập nhật thông tin bàn
        /// </summary>
        /// <param name="id">ID của bàn</param>
        /// <param name="model">Thông tin cập nhật</param>
        /// <returns>Kết quả cập nhật</returns>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateTable(int id, [FromBody] TableViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, message = "Invalid data", errors = ModelState });

                // Ensure the ID matches
                model.Id = id;
                
                var resultId = await tableService.UpsertTableAsync(model);
                if (resultId == null)
                    return NotFound(new { success = false, message = "Table not found" });

                return Ok(new { success = true, message = "Table updated successfully!", tableId = resultId });
            }
            catch (Exception ex)
            {
                this.exceptionHandler.RaiseException(ex, "An error occurred while updating the table.");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Xóa bàn
        /// </summary>
        /// <param name="id">ID của bàn</param>
        /// <returns>Kết quả xóa</returns>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteTable(int id)
        {
            try
            {
                var result = await tableService.DeleteTableAsync(id);
                if (!result)
                    return NotFound(new { success = false, message = "Table not found" });

                return Ok(new { success = true, message = "Table deleted successfully!" });
            }
            catch (Exception ex)
            {
                this.exceptionHandler.RaiseException(ex, "An error occurred while deleting the table.");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Tạo QR code cho các bàn của owner
        /// </summary>
        /// <returns>Danh sách bàn với QR codes</returns>
        [HttpGet("qr-codes")]
        public async Task<IActionResult> GetTableQrCodes()
        {
            try
            {
                var ownerId = RestX.API.Extensions.UserHelper.GetCurrentOwnerId();
                var tables = await tableService.GetTablesByOwnerIdAsync(ownerId);
                
                return Ok(new { 
                    success = true, 
                    data = tables,
                    message = "Table QR codes generated successfully" 
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { success = false, message = "Owner not authenticated" });
            }
            catch (Exception ex)
            {
                this.exceptionHandler.RaiseException(ex, "An error occurred while generating table QR codes.");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Cập nhật trạng thái bàn
        /// </summary>
        /// <param name="id">ID của bàn</param>
        /// <param name="request">Trạng thái mới</param>
        /// <returns>Kết quả cập nhật trạng thái</returns>
        [HttpPut("{id:int}/status")]
        public async Task<IActionResult> UpdateTableStatus(int id, [FromBody] UpdateTableStatusRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, message = "Invalid data", errors = ModelState });

                // TODO: Implement UpdateTableStatusAsync in ITableService
                return Ok(new { 
                    success = true, 
                    message = "UpdateTableStatusAsync method not implemented yet",
                    tableId = id,
                    newStatus = request.StatusId
                });
            }
            catch (Exception ex)
            {
                this.exceptionHandler.RaiseException(ex, $"An error occurred while updating table status {id}.");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Lấy lịch sử sử dụng bàn
        /// </summary>
        /// <param name="id">ID của bàn</param>
        /// <param name="from">Từ ngày (optional)</param>
        /// <param name="to">Đến ngày (optional)</param>
        /// <returns>Lịch sử sử dụng bàn</returns>
        [HttpGet("{id:int}/history")]
        public async Task<IActionResult> GetTableHistory(int id, [FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null)
        {
            try
            {
                // TODO: Implement GetTableHistoryAsync in ITableService
                return Ok(new { 
                    success = true, 
                    message = "GetTableHistoryAsync method not implemented yet",
                    tableId = id,
                    from = from,
                    to = to
                });
            }
            catch (Exception ex)
            {
                this.exceptionHandler.RaiseException(ex, $"An error occurred while loading table history {id}.");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }
    }

    /// <summary>
    /// Request model cho cập nhật trạng thái bàn
    /// </summary>
    public class UpdateTableStatusRequest
    {
        public int StatusId { get; set; }
        public string? Notes { get; set; }
    }
}
