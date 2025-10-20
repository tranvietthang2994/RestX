using Microsoft.AspNetCore.Mvc;
using RestX.API.Services.Interfaces;

namespace RestX.API.Controllers
{
    [ApiController]
    public abstract class BaseController : ControllerBase
    {
        protected readonly IExceptionHandler exceptionHandler;

        protected BaseController(IExceptionHandler exceptionHandler)
        {
            this.exceptionHandler = exceptionHandler;
        }
    }
}
