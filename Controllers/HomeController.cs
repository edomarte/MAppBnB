using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MAppBnB.Models;
using MAppBnB.Data;
using Microsoft.EntityFrameworkCore;

namespace MAppBnB.Controllers;

public class HomeController : Controller
{
    private readonly MappBnBContext _context;

    public HomeController(MappBnBContext context)
    {
        _context = context;
    }

    /*private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }*/

    public async Task<IActionResult> Index()
    {
        ViewBag.AccommodationList = await _context.Accommodation.ToListAsync();
        return View();
    }

    public IActionResult Booking()
    {
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
