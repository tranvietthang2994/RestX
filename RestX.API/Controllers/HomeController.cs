using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestX.API.Services.Interfaces;

namespace RestX.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class HomeController : ControllerBase
    {

        private readonly IHomeService _homeService;

        public HomeController(IHomeService homeService)
        {
            _homeService = homeService;
        }

        /// <summary>
        /// Hiển thị trang chủ của nhà hàng
        /// </summary>
        /// <param name="ownerId">ID của chủ nhà hàng.</param>
        /// <param name="tableId">ID của bàn khách đang ngồi.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("Index/{ownerId:guid}/{tableId:int}")]
        public async Task<IActionResult> Index(Guid ownerId, int tableId, CancellationToken cancellationToken = default)
        {
            try
            {

                var viewModel = await _homeService.GetHomeViewsAsync(cancellationToken);
                if (viewModel == null)
                {
                    return NotFound("Không tìm thấy thông tin nhà hàng.");
                }

                return Ok(new { success = true, data = viewModel });

            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }
    }
}
