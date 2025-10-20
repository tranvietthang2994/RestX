using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestX.WebApp.Models;
using RestX.WebApp.Services.Interfaces;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;

namespace RestX.WebApp.Controllers
{
    public class HomeController : BaseController
    {
        private readonly IHomeService _homeService;

        public HomeController(IHomeService homeService, IExceptionHandler exceptionHandler)
            : base(exceptionHandler)
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
        [Route("Home/Index/{ownerId:guid}/{tableId:int}")]
        public async Task<IActionResult> Index(Guid ownerId, int tableId, CancellationToken cancellationToken)
        {
            var message = TempData["Message"];
            if (message != null)
                ViewBag.Message = message.ToString();

            var claims = new List<Claim>();
            try
            {
                claims.Add(new Claim("OwnerId", ownerId.ToString()));

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity));

                // Lấy thông tin trang chủ không cần kiểm tra session
                var viewModel = await _homeService.GetHomeViewsAsync(cancellationToken);
                if (viewModel == null)
                {
                    return NotFound("Không tìm thấy thông tin nhà hàng.");
                }

                // Truyền thông tin ownerId và tableId vào ViewBag để sử dụng trong view
                ViewBag.OwnerId = ownerId;
                ViewBag.TableId = tableId;

                // Kiểm tra trạng thái đăng nhập để hiển thị thông tin phù hợp
                var isLoggedIn = !string.IsNullOrEmpty(HttpContext.Session.GetString("AuthCustomerId"));
                ViewBag.IsLoggedIn = isLoggedIn;

                if (isLoggedIn)
                {
                    ViewBag.CustomerName = HttpContext.Session.GetString("AuthCustomerName");
                    ViewBag.CustomerPhone = HttpContext.Session.GetString("AuthCustomerPhone");
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                this.exceptionHandler.RaiseException(ex, $"Lỗi khi tải trang chủ cho OwnerId: {ownerId}");
                return BadRequest("Đã có lỗi xảy ra. Vui lòng thử lại sau.");
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}