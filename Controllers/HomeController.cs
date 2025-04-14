using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MAppBnB.Models;
using MAppBnB.Data;
using Microsoft.EntityFrameworkCore;

namespace MAppBnB.Controllers
{
    // The HomeController handles requests for the main public pages of the site.
    public class HomeController : Controller
    {
        private readonly MappBnBContext _context;

        // Constructor that injects the application's database context
        public HomeController(MappBnBContext context)
        {
            _context = context;
        }

        // The Index action is the main homepage.
        // It retrieves the list of accommodations from the database and passes it to the view using ViewBag.
        public async Task<IActionResult> Index()
        {
            ViewBag.AccommodationList = await _context.Accommodation.ToListAsync();
            return View();
        }

        // Displays the booking page (view only, no logic yet)
        public IActionResult Booking()
        {
            return View();
        }

        // Displays the privacy policy page
        public IActionResult Privacy()
        {
            return View();
        }

        // Handles errors and displays the error page.
        // ResponseCache attributes prevent caching of error responses.
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
