using RestX.UI.Models.ViewModels;

namespace RestX.UI.Services.Interfaces
{
    public interface IHomeUIService
    {
        /// <summary>
        /// Lấy trang chủ của nhà hàng
        /// </summary>
        /// <param name="ownerId">ID của chủ nhà hàng.</param>
        /// <param name="tableId">ID của bàn khách đang ngồi.</param>
        /// <returns></returns>
        Task<HomeViewModel?> GetHomeViewsAsync(Guid ownerId, int tableId);
    }
}
