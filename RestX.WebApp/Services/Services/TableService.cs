using AutoMapper;
using QRCoder;
using RestX.WebApp.Helper;
using RestX.WebApp.Models;
using RestX.WebApp.Models.ViewModels;
using RestX.WebApp.Services.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using static QRCoder.PayloadGenerator;

namespace RestX.WebApp.Services.Services
{
    public class TableService : BaseService, ITableService
    {
        private readonly QRCodeGenerator qrGenerator;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IMapper mapper;

        public TableService(IRepository repo, IHttpContextAccessor httpContextAccessor, IMapper mapper, QRCodeGenerator qrGenerator)
            : base(repo, httpContextAccessor)
        {
            this.mapper = mapper;
            this.httpContextAccessor = httpContextAccessor;
            this.qrGenerator = qrGenerator;
        }

        public async Task<List<TableStatus>> GetTableStatus()
        {
            var statuses = await Repo.GetAsync<TableStatus>();
            return statuses.ToList();
        }

        public async Task<Table> GetTableByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await Repo.GetOneAsync<Table>(t => t.Id == id);
        }

        public async Task<TableViewModel> GetTableViewModelByIdAsync(int id)
        {
            return mapper.Map<TableViewModel>(GetAllTablesAsync());
        }

        public async Task<List<TableStatusViewModel>> GetAllTablesByOwnerIdAsync(Guid? guid, CancellationToken cancellationToken = default)
        {

            var tables = await Repo.GetAsync<Table>(
                filter: t => t.OwnerId == guid,
                orderBy: q => q.OrderBy(t => t.TableNumber),
                includeProperties: "TableStatus"
            );

            var tableViewModels = tables.Select(t => new TableStatusViewModel
            {
                Id = t.Id,
                TableNumber = t.TableNumber,
                TableStatus = t.TableStatus,
            });
            return tableViewModels.ToList();
        }

        public async Task<List<TableStatusViewModel>> GetAllTablesByCurrentStaff(CancellationToken cancellationToken = default)
        {
            var ownerId = UserHelper.GetCurrentOwnerId();

            var tables = await Repo.GetAsync<Table>(
                filter: t => t.OwnerId == ownerId,
                orderBy: q => q.OrderBy(t => t.TableNumber),
                includeProperties: "TableStatus"
            );

            var tableViewModels = tables.Select(t => new TableStatusViewModel
            {
                Id = t.Id,
                TableNumber = t.TableNumber,
                TableStatus = t.TableStatus,
            });
            return tableViewModels.ToList();
        }

        public async Task<TableStatusViewModel> UpdateTableStatusAsync(int tableId, int newStatusId)
        {
            var table = await Repo.GetByIdAsync<Table>(tableId);
            if (table == null)
            {
                return null;
            }

            table.TableStatusId = newStatusId;
            Repo.Update(table);
            await Repo.SaveAsync();

            var updatedTable = await Repo.GetOneAsync<Table>(
                filter: t => t.Id == tableId,
                includeProperties: "TableStatus"
            );

            return new TableStatusViewModel
            {
                Id = updatedTable.Id,
                TableNumber = updatedTable.TableNumber,
                TableStatus = updatedTable.TableStatus
            };
        }

        public async Task<TableListViewModel> GetAllTablesAsync()
        {
            var ownerId = UserHelper.GetCurrentOwnerId();
            var tables = await Repo.GetAsync<Table>(
                filter: t => t.OwnerId == ownerId,
                orderBy: q => q.OrderBy(t => t.TableNumber),
                includeProperties: "TableStatus"
            );
            var tableViewModels = tables.Select(t => mapper.Map<TableViewModel>(t)).ToList();
            return new TableListViewModel { Tables = tableViewModels };
        }

        public async Task<int?> UpsertTableAsync(TableViewModel model)
        {
            var ownerId = UserHelper.GetCurrentOwnerId();
            Table table;

            if (model.Id != 0)
            {
                table = await Repo.GetByIdAsync<Table>(model.Id);
                mapper.Map(model, table);
                table.OwnerId = ownerId;
                Repo.Update(table);
            }
            else
            {
                table = mapper.Map<Table>(model);
                table.OwnerId = ownerId;
                await Repo.CreateAsync(table);
                table.Qrcode = GenerateQRCodeURL(ownerId, table.Id, model.TableNumber);
                Repo.Update(table);
            }
            await Repo.SaveAsync();
            return table.Id;
        }

        public async Task<bool> DeleteTableAsync(int id)
        {
            var table = await Repo.GetByIdAsync<Table>(id);
            if (table == null) return false;
            Repo.Delete<Table>(id);
            await Repo.SaveAsync();
            return true;
        }

        public async Task<List<Table>> GetTablesByOwnerIdAsync(Guid guid)
        {
            var tables = await Repo.GetAsync<Table>(
                filter: t => t.OwnerId == guid
            );

            // Test create qr code under
            //Table table = tables.FirstOrDefault(t => t.Id == 5);
            //GenerateQRCodeURL(guid, table.Id, 5);

            return tables.ToList();
        }

        private string GenerateQRCodeURL(Guid ownerId, int tableId, int tableNum)
        {
            var schemeUrl = httpContextAccessor.HttpContext?.Request.Scheme;
            var hostUrl = httpContextAccessor.HttpContext?.Request.Host;
            string url = schemeUrl + "://" + hostUrl + "/Home/Index/" + ownerId + "/" + tableId;

            var qrCodeData = qrGenerator.CreateQrCode(new Url(url), QRCodeGenerator.ECCLevel.Q);

            return GeneratePng(qrCodeData, ownerId, tableNum);
        }

        private string GeneratePng(QRCodeData data, Guid ownerId, int tableNum)
        {
            using var qrCode = new PngByteQRCode(data);
            var qrCodeImage = qrCode.GetGraphic(20);

            using var qrStream = new MemoryStream(qrCodeImage); 
            var qrImage = Image.Load<Rgba32>(qrStream);

            string logoPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads\\Profile", "owner_avatar_" + ownerId + ".jpg");

            if (System.IO.File.Exists(logoPath))
            {
                var logo = Image.Load<Rgba32>(logoPath);
                int logoSize = qrImage.Width / 5;
                logo.Mutate(x => x.Resize(logoSize, logoSize));

                int x = (qrImage.Width - logoSize) / 2; 
                int y = (qrImage.Height - logoSize) / 2;
                qrImage.Mutate(ctx => ctx.DrawImage(logo, new Point(x, y), 1f));

                string fileName = $"Table{tableNum}_qrcode_{ownerId}.png";
                string saveFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads", "RestaurantQRCode");

                string filePath = Path.Combine(saveFolder, fileName);

                qrImage.Save(filePath);
                return saveFolder;
            }
            return string.Empty; 
        }
    }
}
