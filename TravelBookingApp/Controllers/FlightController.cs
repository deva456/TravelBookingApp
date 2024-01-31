using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Drawing;
using System.Linq;
using TravelBooking.Data;
using TravelBooking.Models;

namespace TravelBooking.Controllers
{
    public class FlightController : Controller
    {
        private readonly TravelBookingContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        // public DateTime departureDate { get; set; }

        public FlightController(TravelBookingContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            this._hostEnvironment = hostEnvironment;
        }

        // GET: Flights
        public async Task<IActionResult> Index()
        {
            var userName = User.Identity.Name; // userName is email
            ViewBag.count = HttpContext.Session.GetString("Count");
            return View();
        }

        // GET: Flights
        [HttpPost]
        public async Task<IActionResult> Index(string Origin, string Destination, DateTime DepartureDate)
        {
            //var data=_context.PlannedFlights.ToList();
            ViewBag.departureDate = DepartureDate;
            ViewBag.origin = Origin;
            ViewBag.destination = Destination;
            var userName = User.Identity.Name; // userName is email
            ViewBag.count = HttpContext.Session.GetString("Count");
            if (!string.IsNullOrEmpty(Origin) && !string.IsNullOrEmpty(Destination))
            {
                var flightList = await _context.Flights.Where(t => t.Origin.Name.Contains(Origin) && t.Destination.Name.Contains(Destination)).Include("Airline").Include("Origin").Include("Destination").ToListAsync();
                if (flightList.Count <= 0)
                {
                    TempData["FlightSearchMsg"] = "No Flights Found!";
                    return View();
                }
                return View(flightList);
            }
            else if (!string.IsNullOrEmpty(Origin))
            {
                var flightList = await _context.Flights.Where(t => t.Origin.Name.Contains(Origin)).Include("Airline").Include("Origin").Include("Destination").ToListAsync();
                TempData["FlightSearchMsg"] = "Please choose Destination";
                return View();
            }
            // return View(await _context.Flights.Where(t => t.Origin.Name.Contains(Origin)).Include("Airline").Include("Origin").Include("Destination").ToListAsync());
            else if (!string.IsNullOrEmpty(Destination))
            {
                var flightList = await _context.Flights.Where(t => t.Destination.Name.Contains(Destination)).Include("Airline").Include("Origin").Include("Destination").ToListAsync();
                TempData["FlightSearchMsg"] = "Please choose Origin";
                return View();
            }
                // return View(await _context.Flights.Where(t => t.Destination.Name.Contains(Destination)).Include("Airline").Include("Origin").Include("Destination").ToListAsync());
            else
                return View(await _context.Flights.Include("Airline").Include("Origin").Include("Destination").ToListAsync());
        }


        // GET: Flights/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var flight = await _context.Flights
                .FirstOrDefaultAsync(m => m.Id == id);
            if (flight == null)
            {
                return NotFound();
            }

            return View(flight);
        }

        private bool FlightExists(int id)
        {
            return _context.Flights.Any(e => e.Id == id);
        }

        public async Task<IActionResult> AddToCart(int Id, DateTime departureDate)
        {

            var flight = _context.Flights.FirstOrDefault(m => m.Id == Id);
            var userName = User.Identity.Name; // userName is email
            var user = _context.Users.Where(u => u.UserName == userName).FirstOrDefault(); // find user record
            var newPlannedFlight = new PlannedFlight { User = user, DepartureDate = departureDate, Flight = flight };
            _context.Add(newPlannedFlight);
            await _context.SaveChangesAsync();
            ViewBag.departureDate = departureDate;
            // ViewBag.count = _context.PlannedFlights.Include("User").Where(u => u.User.UserName == userName).ToList().Count.ToString();
            var count = _context.PlannedFlights.Include("User").Where(u => u.User.UserName == userName).ToList().Count.ToString();
            HttpContext.Session.SetString("Count", count);
            ViewBag.count = HttpContext.Session.GetString("Count");
            TempData["AddToCart"] = "Add flight to cart successfully";
            // return View("Index");
            return RedirectToAction(nameof(Index));
        }


    }
}
