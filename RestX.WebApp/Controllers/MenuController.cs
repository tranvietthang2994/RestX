using Microsoft.AspNetCore.Mvc;
using RestX.WebApp.Services.Interfaces;

namespace RestX.WebApp.Controllers
{
    public class MenuController : BaseController
    {
        private readonly IMenuService menuService;

        public MenuController(IMenuService menuService, IExceptionHandler exceptionHandler) : base(exceptionHandler)
        {
            this.menuService = menuService;
        }

        [HttpGet]
        [Route("Menu/Index/{ownerId:guid}/{tableId:int}")]
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var model = await menuService.GetMenuViewModelAsync(cancellationToken);
            return View(model);
        }
    }
}
