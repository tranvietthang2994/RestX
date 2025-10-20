using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace RestX.AdminWeb.Controllers
{
    using Microsoft.AspNetCore.Authorization;

    [Authorize]
    [Route("content")]
    public class ContentController : Controller
    {
        //private ICountriesJob countriesJob;
        //private IExceptionHandler exceptionHandler;
        public ContentController()
        {
            //this.countriesJob = countriesJob;
            //this.exceptionHandler = exceptionHandler;
        }

        // GET: /<controller>/
        public IActionResult Index()
        {
            return View();
        }

    }
}
