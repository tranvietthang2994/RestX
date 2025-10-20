using RestX.API.Data.Repository.Interfaces;
using RestX.API.Models.ViewModels;
using RestX.API.Services.Interfaces;

namespace RestX.API.Services.Implementations
{
    public class HomeService : BaseService, IHomeService
    {
        private readonly IOwnerService ownerService;
        private readonly ITableService tableService;

        public HomeService(IOwnerService ownerService, ITableService tableService, IRepository repo, IHttpContextAccessor httpContextAccessor) : base(repo, httpContextAccessor)
        {
            this.ownerService = ownerService;
            this.tableService = tableService;
        }

        public async Task<HomeViewModel> GetHomeViewsAsync(CancellationToken cancellationToken = default)
        {
            var owner = await ownerService.GetOwnerByIdAsync(OwnerId);
            var table = await tableService.GetTableByIdAsync(TableId, cancellationToken);

            var homeViewModel = new HomeViewModel
            {
                OwnerId = owner.Id,
                TableId = table.Id,
                Name = owner.Name ?? string.Empty,
                Address = owner.Address ?? string.Empty,
                FileName = owner.File?.Name ?? "Defaul",
                FileUrl = owner.File?.Url ?? "/images/default.png",
                TableNumber = table.TableNumber
            };

            return homeViewModel;
        }
    }
}
