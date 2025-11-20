using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestX.UI.Models.ViewModels;
using RestX.UI.Services.Interfaces;

namespace RestX.UI.Controllers
{
    [Authorize(Roles = "Owner,Staff")]
    public class TableController : Controller
    {
        private readonly IApiService _apiService;
        private readonly IAuthService _authService;
        private readonly ILogger<TableController> _logger;

        public TableController(
            IApiService apiService,
            IAuthService authService,
            ILogger<TableController> logger)
        {
            _apiService = apiService;
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Tables management page
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                _logger.LogInformation("Loading tables management page");
                
                var tables = await GetTablesAsync();
                
                var model = new TableManagementViewModel
                {
                    Tables = tables,
                    TotalTables = tables.Count,
                    AvailableTables = tables.Count(t => t.Status == "Available"),
                    OccupiedTables = tables.Count(t => t.Status == "Occupied")
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading tables management page");
                return View("Error", new ErrorViewModel 
                { 
                    Message = "An error occurred while loading tables"
                });
            }
        }

        /// <summary>
        /// Get all tables as JSON
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetTables()
        {
            try
            {
                var tables = await GetTablesAsync();
                return Json(new { success = true, data = tables });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tables");
                return Json(new { success = false, message = "An error occurred while loading tables" });
            }
        }

        /// <summary>
        /// Get table by ID
        /// </summary>
        /// <param name="tableId">Table ID</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetTable(int tableId)
        {
            try
            {
                var response = await _apiService.GetAsync<object>($"api/table/{tableId}");
                
                if (response != null)
                {
                    return Json(new { success = true, data = response });
                }
                
                return Json(new { success = false, message = "Table not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting table by ID: {TableId}", tableId);
                return Json(new { success = false, message = "An error occurred while loading table data" });
            }
        }

        /// <summary>
        /// Create new table
        /// </summary>
        /// <param name="model">Table data</param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> CreateTable(TableViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return Json(new { success = false, message = string.Join(", ", errors) });
                }

                var currentUser = await _authService.GetCurrentUserAsync();
                var ownerId = currentUser?.OwnerId ?? currentUser?.Id;

                var createData = new
                {
                    Name = model.Name,
                    Status = "Available",
                    QrCode = model.QrCode,
                    IsActive = model.IsActive ?? true,
                    OwnerId = ownerId
                };

                var response = await _apiService.PostAsync<object, object>("api/table", createData);
                
                if (response != null)
                {
                    return Json(new { success = true, message = "Table created successfully" });
                }
                
                return Json(new { success = false, message = "Failed to create table" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating table: {TableName}", model.Name);
                return Json(new { success = false, message = "An error occurred while creating the table" });
            }
        }

        /// <summary>
        /// Update table
        /// </summary>
        /// <param name="model">Updated table data</param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> UpdateTable(TableViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return Json(new { success = false, message = string.Join(", ", errors) });
                }

                var updateData = new
                {
                    Id = model.Id,
                    Name = model.Name,
                    Status = model.Status,
                    QrCode = model.QrCode,
                    IsActive = model.IsActive
                };

                var response = await _apiService.PutAsync<object, object>($"api/table/{model.Id}", updateData);
                
                if (response != null)
                {
                    return Json(new { success = true, message = "Table updated successfully" });
                }
                
                return Json(new { success = false, message = "Failed to update table" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating table ID: {TableId}", model.Id);
                return Json(new { success = false, message = "An error occurred while updating the table" });
            }
        }

        /// <summary>
        /// Delete table
        /// </summary>
        /// <param name="tableId">Table ID</param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> DeleteTable(int tableId)
        {
            try
            {
                var success = await _apiService.DeleteAsync($"api/table/{tableId}");
                
                if (success)
                {
                    return Json(new { success = true, message = "Table deleted successfully" });
                }
                
                return Json(new { success = false, message = "Failed to delete table" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting table: {TableId}", tableId);
                return Json(new { success = false, message = "An error occurred while deleting the table" });
            }
        }

        /// <summary>
        /// Update table status
        /// </summary>
        /// <param name="tableId">Table ID</param>
        /// <param name="status">New status</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> UpdateTableStatus(int tableId, string status)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(status))
                {
                    return Json(new { success = false, message = "Status is required" });
                }

                var updateData = new
                {
                    TableId = tableId,
                    Status = status
                };

                var response = await _apiService.PutAsync<object, object>("api/table/status", updateData);
                
                if (response != null)
                {
                    return Json(new { success = true, message = "Table status updated successfully" });
                }
                
                return Json(new { success = false, message = "Failed to update table status" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating table status: {TableId} -> {Status}", tableId, status);
                return Json(new { success = false, message = "An error occurred while updating table status" });
            }
        }

        /// <summary>
        /// Generate QR code for table
        /// </summary>
        /// <param name="tableId">Table ID</param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> GenerateQrCode(int tableId)
        {
            try
            {
                var currentUser = await _authService.GetCurrentUserAsync();
                var ownerId = currentUser?.OwnerId ?? currentUser?.Id;

                // Generate QR code URL (this would point to the customer menu)
                var qrCodeUrl = $"{Request.Scheme}://{Request.Host}/Home/Index/{ownerId}/{tableId}";
                
                var updateData = new
                {
                    TableId = tableId,
                    QrCode = qrCodeUrl
                };

                var response = await _apiService.PutAsync<object, object>("api/table/qrcode", updateData);
                
                if (response != null)
                {
                    return Json(new { success = true, message = "QR code generated successfully", qrCode = qrCodeUrl });
                }
                
                return Json(new { success = false, message = "Failed to generate QR code" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating QR code for table: {TableId}", tableId);
                return Json(new { success = false, message = "An error occurred while generating QR code" });
            }
        }

        /// <summary>
        /// Table QR code display page
        /// </summary>
        /// <param name="tableId">Table ID</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> QrCode(int tableId)
        {
            try
            {
                var response = await _apiService.GetAsync<object>($"api/table/{tableId}");
                
                if (response == null)
                {
                    return View("Error", new ErrorViewModel 
                    { 
                        Message = "Table not found",
                        StatusCode = 404
                    });
                }

                return View(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading QR code page for table: {TableId}", tableId);
                return View("Error", new ErrorViewModel 
                { 
                    Message = "An error occurred while loading QR code"
                });
            }
        }

        #region Private Methods

        /// <summary>
        /// Get tables from API
        /// </summary>
        /// <returns></returns>
        private async Task<List<TableViewModel>> GetTablesAsync()
        {
            try
            {
                var currentUser = await _authService.GetCurrentUserAsync();
                var ownerId = currentUser?.OwnerId ?? currentUser?.Id;
                
                if (ownerId == null)
                {
                    _logger.LogWarning("No owner context found for tables");
                    return new List<TableViewModel>();
                }

                var response = await _apiService.GetAsync<object>($"api/table/owner/{ownerId}");
                
                // This is a simplified version - you would need proper mapping
                // For now, return empty list as we don't have the full API model structure
                return new List<TableViewModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tables from API");
                return new List<TableViewModel>();
            }
        }

        #endregion
    }

    // Additional ViewModels for Table management
    public class TableViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = "Available";
        public string QrCode { get; set; } = string.Empty;
        public bool? IsActive { get; set; } = true;
        public Guid OwnerId { get; set; }
    }

    public class TableManagementViewModel
    {
        public List<TableViewModel> Tables { get; set; } = new();
        public TableViewModel NewTable { get; set; } = new();
        public int TotalTables { get; set; }
        public int AvailableTables { get; set; }
        public int OccupiedTables { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
