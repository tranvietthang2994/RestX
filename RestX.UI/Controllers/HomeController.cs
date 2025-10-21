using Microsoft.AspNetCore.Mvc;
using RestX.UI.Models.ViewModels;
using RestX.UI.Services.Interfaces;
using System.Diagnostics;

namespace RestX.UI.Controllers
{
    public class HomeController : Controller
    {
        private readonly IMenuUIService _menuService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(IMenuUIService menuService, ILogger<HomeController> logger)
        {
            _menuService = menuService;
            _logger = logger;
        }

        /// <summary>
        /// Hiển thị trang chủ của nhà hàng
        /// </summary>
        /// <param name="ownerId">ID của chủ nhà hàng.</param>
        /// <param name="tableId">ID của bàn khách đang ngồi.</param>
        /// <returns></returns>
        [HttpGet]
        [Route("Home/Index/{ownerId:guid}/{tableId:int}")]
        public async Task<IActionResult> Index(Guid ownerId, int tableId)
        {
            try
            {
                _logger.LogInformation("Loading home page for owner: {OwnerId}, table: {TableId}", ownerId, tableId);
                
                var message = TempData["Message"]?.ToString();
                if (!string.IsNullOrEmpty(message))
                {
                    ViewBag.Message = message;
                }

                // Store restaurant context in session for later use
                HttpContext.Session.SetString("OwnerId", ownerId.ToString());
                HttpContext.Session.SetString("TableId", tableId.ToString());

                // Get menu for the home page
                var menu = await _menuService.GetMenuAsync(ownerId, tableId);
                
                if (menu == null || !string.IsNullOrEmpty(menu.ErrorMessage))
                {
                    _logger.LogWarning("Failed to load menu for owner: {OwnerId}, table: {TableId}", ownerId, tableId);
                    return View("Error", new ErrorViewModel 
                    { 
                        Message = menu?.ErrorMessage ?? "Unable to load restaurant menu",
                        StatusCode = 404
                    });
                }

                // Set ViewBag properties for the view
                ViewBag.OwnerId = ownerId;
                ViewBag.TableId = tableId;

                // Check if customer is logged in
                var customerId = HttpContext.Session.GetString("CustomerId");
                ViewBag.IsLoggedIn = !string.IsNullOrEmpty(customerId);
                
                if (!string.IsNullOrEmpty(customerId))
                {
                    ViewBag.CustomerName = HttpContext.Session.GetString("CustomerName");
                    ViewBag.CustomerPhone = HttpContext.Session.GetString("CustomerPhone");
                }

                return View(menu);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading home page for owner: {OwnerId}, table: {TableId}", ownerId, tableId);
                return View("Error", new ErrorViewModel 
                { 
                    Message = "An error occurred while loading the page",
                    RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Default home page redirect to menu selection
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            // If no parameters provided, redirect to a restaurant selection or show default page
            return View("SelectRestaurant");
        }

        /// <summary>
        /// Hiển thị thông tin riêng tư của người dùng.
        /// </summary>
        /// <returns></returns>
        public IActionResult Privacy()
        {
            return View();
        }

        /// <summary>
        /// Hiển thị trang lỗi với thông tin chi tiết về lỗi.
        /// </summary>
        /// <returns></returns>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel 
            { 
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                Message = "An unexpected error occurred"
            });
        }

        /// <summary>
        /// Xử lý tình trạng khi bàn đã được đặt hoặc có vấn đề.
        /// </summary>
        /// <returns></returns>
        public IActionResult TableNotAvailable()
        {
            return View();
        }
    }
}