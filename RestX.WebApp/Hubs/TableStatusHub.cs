using Microsoft.AspNetCore.SignalR;
using RestX.WebApp.Services.Interfaces;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace RestX.WebApp.Hubs
{
    public class TableStatusHub : Hub
    {
        private readonly ITableService tableService;
        private readonly ILogger<TableStatusHub> _logger;

        public TableStatusHub(ITableService tableService, ILogger<TableStatusHub> logger)
        {
            this.tableService = tableService;
            _logger = logger;
        }

        public async Task UpdateTableStatus(int tableId, int newStatusId)
        {
            try
            {
                _logger.LogInformation($"Updating table {tableId} to status {newStatusId}");
                
                var updatedTable = await tableService.UpdateTableStatusAsync(tableId, newStatusId);

                if (updatedTable != null)
                {
                    _logger.LogInformation($"Table updated successfully. Broadcasting to all clients. Table: {updatedTable.Id}, Status: {updatedTable.TableStatus?.Name}");
                    
                    // Create a simple DTO to avoid serialization issues
                    var updateData = new
                    {
                        id = updatedTable.Id,
                        tableNumber = updatedTable.TableNumber,
                        tableStatus = new
                        {
                            id = updatedTable.TableStatus.Id,
                            name = updatedTable.TableStatus.Name
                        }
                    };
                    
                    await Clients.All.SendAsync("ReceiveTableStatusUpdate", updateData);
                }
                else
                {
                    _logger.LogWarning($"Failed to update table {tableId}. UpdateTableStatusAsync returned null.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating table status for table {tableId}");
                throw;
            }
        }
    }
} 