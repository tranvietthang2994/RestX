using Microsoft.AspNetCore.Mvc;
using RestX.API.Services.Interfaces;

namespace RestX.API.Controllers
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
