using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelBooking.Data;
using TravelBooking.Models;

namespace TravelBooking.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    private readonly TravelBookingContext _context;

    public HomeController(ILogger<HomeController> logger, TravelBookingContext context)
    {
        _logger = logger;
        _context = context;
    }

    public IActionResult Index()
    {
        var userName = User.Identity.Name; // userName is email
        var count = _context.PlannedFlights.Include("User").Where(u => u.User.UserName == userName).ToList().Count.ToString();
        HttpContext.Session.SetString("Count", count);
        ViewBag.count = HttpContext.Session.GetString("Count");
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
