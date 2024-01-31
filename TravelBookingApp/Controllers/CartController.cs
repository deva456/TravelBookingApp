using Microsoft.AspNetCore.Mvc;
using TravelBooking.Data;
using TravelBooking.Models;
using TravelBooking.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace TravelBooking.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly TravelBookingContext _context;

        public List<PlannedFlight> CartList;
        // public DateTime departureDate { get; set; }
        public CartController(TravelBookingContext context)
        {
            this._context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userName = User.Identity.Name; // userName is email
            var user = _context.Users.Where(u => u.UserName == userName).FirstOrDefault();
            ViewBag.Total = this._context.PlannedFlights.Include("Flight").Include("User").Where(u => u.User.UserName == userName).Sum(flight => (double)flight.Flight.Price);
            ViewBag.count = HttpContext.Session.GetString("Count");
            CartList = _context.PlannedFlights.Include("User").Include("Flight").Include("Flight.Airline").Include("Flight.Origin").Include("Flight.Destination").Where(u => u.User.UserName == userName).ToList();
            // ViewBag.PlannedFlights = CartList;
            return View(CartList);
        }

        private int IsExist(int Id)
        {
            List<Flight> cart = WorkingWithSession.GetObjectFromJson<List<Flight>>(HttpContext.Session, "cart");
            for (int i = 0; i < cart.Count; i++)
            {
                if (cart[i].Id.Equals(Id))
                {
                    return i;
                }
            }
            return -1;
        }

        public async Task<IActionResult> Buy(int Id, DateTime departureDate)
        {

            //    ProductModel productModel = new ProductModel();
            var flight = _context.Flights.FirstOrDefault(m => m.Id == Id);

            var userName = User.Identity.Name; // userName is email
            var user = _context.Users.Where(u => u.UserName == userName).FirstOrDefault(); // find user record
            var newPlannedFlight = new PlannedFlight { User = user, DepartureDate = departureDate, Flight = flight };
            _context.Add(newPlannedFlight);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Remove(int Id)
        {
            if (Id == null)
            {
                return NotFound();
            }

            var removeFlight = await _context.PlannedFlights
                .FirstOrDefaultAsync(m => m.Id == Id);
            if (removeFlight == null)
            {
                return NotFound();
            }

            _context.PlannedFlights.Remove(removeFlight);
            await _context.SaveChangesAsync();

            var userName = User.Identity.Name; // userName is email
            var count = _context.PlannedFlights.Include("User").Where(u => u.User.UserName == userName).ToList().Count.ToString();
            HttpContext.Session.SetString("Count", count);

            TempData["DeleteCartItem"] = "Removed planned flight in cart successfully";

            return RedirectToAction("Index");
        }
    }
}
