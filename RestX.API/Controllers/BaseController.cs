using Microsoft.AspNetCore.Mvc;
using RestX.API.Services.Interfaces;

namespace RestX.API.Controllers
{
    public class BaseController : Controller
        {
            public readonly IExceptionHandler exceptionHandler;

            public BaseController(IExceptionHandler exceptionHandler)
            {
                this.exceptionHandler = exceptionHandler;
            }
        }
    }
