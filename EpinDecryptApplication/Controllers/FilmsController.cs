using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EpinDecryptApplication.Models;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace EpinDecryptApplication.Controllers
{
    public class FilmsController : Controller
    {
        // GET: /<controller>/
        public IActionResult Index()
        {
            SakilaContext context = HttpContext.RequestServices.GetService(typeof(EpinDecryptApplication.Models.SakilaContext)) as SakilaContext;

            return View(context.GetAllFilms());
        }
    }
}
