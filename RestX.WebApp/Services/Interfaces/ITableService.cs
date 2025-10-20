using RestX.WebApp.Models;
using RestX.WebApp.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RestX.WebApp.Services.Interfaces
{
    public interface ITableService
    {
        public Task<List<TableStatus>> GetTableStatus();
        public Task<Table> GetTableByIdAsync(int id, CancellationToken cancellationToken = default);
        public Task<List<TableStatusViewModel>> GetAllTablesByOwnerIdAsync(Guid? guid, CancellationToken cancellationToken = default);
        public Task<List<TableStatusViewModel>> GetAllTablesByCurrentStaff(CancellationToken cancellationToken = default);
        public Task<TableStatusViewModel> UpdateTableStatusAsync(int tableId, int newStatusId);
        public Task<TableListViewModel> GetAllTablesAsync();
        Task<int?> UpsertTableAsync(TableViewModel model);
        public Task<bool> DeleteTableAsync(int id);
        public Task<TableViewModel> GetTableViewModelByIdAsync(int id);
        public Task<List<Table>> GetTablesByOwnerIdAsync(Guid guid);
    }
}
