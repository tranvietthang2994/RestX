    using System.Diagnostics;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using RestX.WebApp.Models;
    using RestX.WebApp.Models.ViewModels;
    using RestX.WebApp.Services.Interfaces;

    namespace RestX.WebApp.Controllers
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
