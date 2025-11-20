using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestX.UI.Models.ApiModels;
using RestX.UI.Models.ViewModels;
using RestX.UI.Services.Interfaces;
using System.Text.Json;

namespace RestX.UI.Controllers
{
    //[Authorize(Roles = "Owner,Staff")]
    [Route("Table")]
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

        [HttpGet("")]
        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            try
            {
                _logger.LogInformation("Loading tables management page");

                TableListViewModel response = new TableListViewModel
                {
                    Tables = new List<TableApiModel>()
                };

                // Get data from API
                var responseString = await _apiService.GetStringAsync("api/Table/owner");
                _logger.LogInformation("API Response: {Response}", responseString);

                if (!string.IsNullOrEmpty(responseString))
                {
                    try
                    {
                        // Parse the API response which has structure: { "success": true, "data": [...] }
                        var apiResponseWrapper = JsonSerializer.Deserialize<ApiResponse<JsonElement[]>>(responseString, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                        if (apiResponseWrapper?.Success == true && apiResponseWrapper.Data != null)
                        {
                            // Convert the data array to TableApiModel list
                            var tables = new List<TableApiModel>();

                            foreach (var tableElement in apiResponseWrapper.Data)
                            {
                                var table = new TableApiModel
                                {
                                    Id = tableElement.TryGetProperty("id", out var idProp) ? idProp.GetInt32() : 0,
                                    TableNumber = tableElement.TryGetProperty("tableNumber", out var numProp) ? numProp.GetInt32() : 0,
                                    TableStatusId = tableElement.TryGetProperty("tableStatusId", out var statusIdProp) ? statusIdProp.GetInt32() : 0,
                                    OwnerId = tableElement.TryGetProperty("ownerId", out var ownerProp) &&
                                             Guid.TryParse(ownerProp.GetString(), out var ownerId) ? ownerId : Guid.Empty,
                                    IsActive = tableElement.TryGetProperty("isActive", out var activeProp) && activeProp.GetBoolean(),
                                    QrCodeUrl = tableElement.TryGetProperty("qrcode", out var qrProp) ? qrProp.GetString() : "",
                                    CreatedDate = tableElement.TryGetProperty("createdDate", out var createdProp) &&
                                                 DateTime.TryParse(createdProp.GetString(), out var created) ? created : null,
                                    ModifiedDate = tableElement.TryGetProperty("modifiedDate", out var modifiedProp) &&
                                                  DateTime.TryParse(modifiedProp.GetString(), out var modified) ? modified : null,
                                    // Map status based on tableStatusId
                                    Status = GetStatusNameFromId(tableElement.TryGetProperty("tableStatusId", out var sIdProp) ? sIdProp.GetInt32() : 1)
                                };
                                tables.Add(table);
                            }

                            if (tables.Any())
                            {
                                response = new TableListViewModel
                                {
                                    Tables = tables,
                                    TotalTables = tables.Count,
                                    AvailableTables = tables.Count(t => t.TableStatusId == 1),
                                    OccupiedTables = tables.Count(t => t.TableStatusId == 2),
                                    Success = true
                                };
                                _logger.LogInformation("Using API data with {Count} tables", response.Tables.Count);
                            }
                            else
                            {
                                _logger.LogWarning("API returned empty tables");
                            }
                        }
                        else
                        {
                            _logger.LogWarning("API response unsuccessful or no data");
                        }
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogError(ex, "Failed to deserialize API response");
                    }
                }
                else
                {
                    _logger.LogWarning("API returned empty response");
                }

                // Set ViewBag cho table statuses
                ViewBag.TableStatuses = await GetTableStatusesAsync();

                _logger.LogInformation("Final response: Tables={Count}, TableStatuses={StatusCount}",
                    response.Tables.Count,
                    ((List<TableStatusViewModel>)ViewBag.TableStatuses).Count);

                return View("~/Views/Management/Table/Index.cshtml", response);
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

        private string GetStatusNameFromId(int statusId)
        {
            return statusId switch
            {
                1 => "Available",
                2 => "Occupied",
                3 => "Reserved",
                4 => "Maintenance",
                _ => "Available"
            };
        }

        // ===== CRUD METHODS SỬ DỤNG IApiService =====

        /// <summary>
        /// Upsert table - sử dụng IApiService
        /// </summary>
        /// <param name="model">Table data từ form</param>
        /// <returns></returns>
        [HttpPost("Upsert")]
        //[Authorize(Roles = "Owner")]
        public async Task<IActionResult> UpsertTable([FromForm] TableUpsertModel model)
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

                // Determine if create or update
                if (model.Id.HasValue && model.Id.Value > 0)
                {
                    // Update existing table
                    var updateData = new
                    {
                        Id = model.Id.Value,
                        TableNumber = model.TableNumber,
                        TableStatusId = model.TableStatusId,
                        IsActive = model.IsActive,
                        Qrcode = model.Qrcode
                    };

                    var response = await _apiService.PutAsync<object, object>($"api/table/{model.Id.Value}", updateData);

                    if (response != null)
                    {
                        return Json(new { success = true, message = "Table updated successfully!" });
                    }
                }
                else
                {
                    // Create new table
                    var createData = new
                    {
                        TableNumber = model.TableNumber,
                        TableStatusId = model.TableStatusId,
                        IsActive = model.IsActive,
                        OwnerId = ownerId
                    };

                    var response = await _apiService.PostAsync<object, object>("api/table", createData);

                    if (response != null)
                    {
                        return Json(new { success = true, message = "Table added successfully!" });
                    }
                }

                return Json(new { success = false, message = "Failed to save table" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error upserting table: {TableNumber}", model.TableNumber);
                return Json(new { success = false, message = "An error occurred while saving table" });
            }
        }

        /// <summary>
        /// Get table details - sử dụng IApiService
        /// </summary>
        /// <param name="id">Table ID</param>
        /// <returns></returns>
        [HttpGet("Detail/{id:int}")]
        public async Task<IActionResult> TableDetail(int id)
        {
            try
            {
                var response = await _apiService.GetAsync<TableApiModel>($"api/table/{id}");

                if (response == null)
                {
                    return Json(new { success = false, message = "Table not found" });
                }

                return Json(new { success = true, data = response });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting table details for ID: {TableId}", id);
                return Json(new { success = false, message = "An error occurred while loading table details" });
            }
        }

        /// <summary>
        /// Delete table - sử dụng IApiService
        /// </summary>
        /// <param name="id">Table ID</param>
        /// <returns></returns>
        [HttpDelete("Delete/{id:int}")]
        //[Authorize(Roles = "Owner")]
        public async Task<IActionResult> DeleteTable(int id)
        {
            try
            {
                var success = await _apiService.DeleteAsync($"api/table/{id}");

                if (success)
                {
                    return Json(new { success = true, message = "Table has been deleted." });
                }

                return Json(new { success = false, message = "Failed to delete table" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting table: {TableId}", id);
                return Json(new { success = false, message = "An error occurred while deleting table" });
            }
        }

        /// <summary>
        /// Get table statuses - sử dụng IApiService
        /// </summary>
        /// <returns></returns>
        [HttpGet("Statuses")]
        public async Task<IActionResult> GetTableStatuses()
        {
            try
            {
                var statuses = await GetTableStatusesAsync();
                return Json(new { success = true, data = statuses });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting table statuses");
                return Json(new { success = false, message = "An error occurred while loading table statuses" });
            }
        }

        // ===== GIỮ NGUYÊN CÁC METHOD KHÁC =====

        /// <summary>
        /// Get all tables as JSON
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetTables")]
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
        [HttpGet("GetTable/{tableId:int}")]
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
        /// Update table status
        /// </summary>
        /// <param name="tableId">Table ID</param>
        /// <param name="status">New status</param>
        /// <returns></returns>
        [HttpPost("UpdateStatus")]
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
        [HttpPost("GenerateQrCode")]
        public async Task<IActionResult> GenerateQrCode(int tableId)
        {
            try
            {
                var currentUser = await _authService.GetCurrentUserAsync();
                var ownerId = currentUser?.OwnerId ?? currentUser?.Id;

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
        /// Table QR code display page for individual table
        /// </summary>
        /// <param name="tableId">Table ID</param>
        /// <returns></returns>
        [HttpGet("QrCode/{tableId:int}")]
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

        /// <summary>
        /// Error handling
        /// </summary>
        /// <returns></returns>
        public IActionResult Error()
        {
            return View("Error", new ErrorViewModel
            {
                Message = "An unexpected error occurred"
            });
        }

        #region Private Methods

        /// <summary>
        /// Get table statuses using API
        /// </summary>
        /// <returns></returns>
        private async Task<List<TableStatusViewModel>> GetTableStatusesAsync()
{
    try
    {
        // First, try to get the actual table statuses from the API
        var response = await _apiService.GetStringAsync("api/table/statuses");

        if (!string.IsNullOrEmpty(response))
        {
            _logger.LogInformation("Table statuses API response: {Response}", response);

            try
            {
                // Try to deserialize as ApiResponse wrapper first
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<object>>>(response, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (apiResponse?.Success == true && apiResponse.Data != null)
                {
                    var statuses = new List<TableStatusViewModel>();
                    
                    foreach (var item in apiResponse.Data)
                    {
                        if (item is JsonElement element)
                        {
                            var status = new TableStatusViewModel
                            {
                                Id = element.TryGetProperty("id", out var idProp) ? idProp.GetInt32() : 0,
                                TableNumber = 0, // This might not be relevant for status types
                                TableStatus = new TableStatusDetailViewModel
                                {
                                    Id = element.TryGetProperty("id", out var statusIdProp) ? statusIdProp.GetInt32() : 0,
                                    Name = element.TryGetProperty("name", out var nameProp) ? nameProp.GetString() : "Unknown"
                                }
                            };
                            statuses.Add(status);
                        }
                    }

                    if (statuses.Any())
                    {
                        _logger.LogInformation("Successfully parsed table statuses, count: {Count}", statuses.Count);
                        return statuses;
                    }
                }

                // Fallback: Try direct array parsing
                var jsonDocument = JsonDocument.Parse(response);
                var statusList = new List<TableStatusViewModel>();

                if (jsonDocument.RootElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var element in jsonDocument.RootElement.EnumerateArray())
                    {
                        var status = new TableStatusViewModel
                        {
                            Id = element.TryGetProperty("id", out var idProp) ? idProp.GetInt32() : 0,
                            TableNumber = 0,
                            TableStatus = new TableStatusDetailViewModel
                            {
                                Id = element.TryGetProperty("id", out var statusIdProp) ? statusIdProp.GetInt32() : 0,
                                Name = element.TryGetProperty("name", out var nameProp) ? nameProp.GetString() : "Unknown"
                            }
                        };
                        statusList.Add(status);
                    }

                    if (statusList.Any())
                    {
                        _logger.LogInformation("Successfully parsed table statuses manually, count: {Count}", statusList.Count);
                        return statusList;
                    }
                }
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse table statuses response");
            }
        }
        else
        {
            _logger.LogWarning("Table statuses API returned empty response");
        }

        return GetDefaultTableStatuses();
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error getting table statuses from API");
        return GetDefaultTableStatuses();
    }
}

private static List<TableStatusViewModel> GetDefaultTableStatuses()
{
    return new List<TableStatusViewModel>
    {
        new() { 
            Id = 1, 
            TableNumber = 0, 
            TableStatus = new TableStatusDetailViewModel { Id = 1, Name = "Available" }
        },
        new() { 
            Id = 2, 
            TableNumber = 0, 
            TableStatus = new TableStatusDetailViewModel { Id = 2, Name = "Occupied" }
        },
        new() { 
            Id = 3, 
            TableNumber = 0, 
            TableStatus = new TableStatusDetailViewModel { Id = 3, Name = "Reserved" }
        },
        new() { 
            Id = 4, 
            TableNumber = 0, 
            TableStatus = new TableStatusDetailViewModel { Id = 4, Name = "Maintenance" }
        }
    };
}

        /// <summary>
        /// Get tables from API
        /// </summary>
        /// <returns></returns>
        private async Task<List<TableApiModel>> GetTablesAsync()
        {
            try
            {
                var currentUser = await _authService.GetCurrentUserAsync();
                var ownerId = currentUser?.OwnerId ?? currentUser?.Id;

                if (ownerId == null)
                {
                    _logger.LogWarning("No owner context found for tables");
                    return new List<TableApiModel>();
                }

                var response = await _apiService.GetAsync<TableListViewModel>("api/table/owner");
                return response?.Tables ?? new List<TableApiModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tables from API");
                return new List<TableApiModel>();
            }
        }

        #endregion
    }
    // ===== VIEWMODELS =====

    /// <summary>
    /// Form model cho Table Upsert operation
    /// </summary>
    public class TableUpsertModel
    {
        public int? Id { get; set; }
        public int TableNumber { get; set; }
        public int TableStatusId { get; set; }
        public bool IsActive { get; set; } = true;
        public string? Qrcode { get; set; }
    }

    /// <summary>
    /// ViewModel cho QR Code page
    /// </summary>
    public class TableItemViewModel
    {
        public int Id { get; set; }
        public int TableNumber { get; set; }
        public string QrCode { get; set; } = string.Empty;
        public Guid OwnerId { get; set; }
    }
}
    // 