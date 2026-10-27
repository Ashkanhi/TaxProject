using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using TaxServiceAndApp.BI;
using TaxWebAppCore8.Models;

namespace TaxWebAppCore8.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;

        }

        public IActionResult Index()
        {
            if (NewTaxController.IsNewTax) return RedirectToAction("Dashboard", "NewTax");
            return View();
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
