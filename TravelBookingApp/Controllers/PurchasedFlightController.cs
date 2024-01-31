using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TravelBooking.Data;
using TravelBooking.Data.ViewModels;
using TravelBooking.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace TravelBooking.Controllers
{
    [Authorize]
    public class PurchasedFlightController : Controller
    {
        private readonly TravelBookingContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        public PurchasedFlightController(UserManager<IdentityUser> userManager, TravelBookingContext context)
        {
            this._userManager = userManager;
            _context = context;
        }

        // Current user's PurchasedFlight history
        public async Task<IActionResult> Index(string? id, string searchString)
        {
            var userName = _userManager.GetUserAsync(User).Result.UserName;
            var currentUser = _context.Users.Where(u => u.UserName == userName).FirstOrDefault();

            if (currentUser.Id != id)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            if (_context.PurchasedFlights == null) return Problem("No purchases found.");

            var purchases = await _context.PurchasedFlights
                .Include(pf => pf.Flight).ThenInclude(f => f.Airline)
                .Include(pf => pf.Flight).ThenInclude(f => f.Origin)
                .Include(pf => pf.Flight).ThenInclude(f => f.Destination)
                .Include(pf => pf.Purchase)
                .Where(pf => pf.Purchase.User.Id == id).ToListAsync();

            if (!String.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();
                purchases = await _context.PurchasedFlights.Where(pf =>
                    pf.Flight.Number.ToLower().Contains(searchString) ||
                    pf.Flight.Airline.AirlineName.ToLower().Contains(searchString) ||
                    pf.Flight.Origin.Name.ToLower().Contains(searchString) ||
                    pf.Flight.Destination.Name.ToLower().Contains(searchString) ||
                    pf.Purchase.WhenPaid.ToString().Contains(searchString) ||
                    pf.DepartureDate.ToString().Contains(searchString)
                    )
                    .Where(pf => pf.Purchase.User.Id == id)
                    .ToListAsync();

                if (purchases.Count <= 0)
                {
                    TempData["notfound"] = "No related purchase Found!";
                }
            }
            ViewBag.count = HttpContext.Session.GetString("Count");
            return View(purchases);
        }

        // All PurchasedFlight 
        public async Task<IActionResult> AdminIndex(string searchString)
        {
            if (_context.PurchasedFlights == null) return Problem("No purchases found.");
            var purchases = await _context.PurchasedFlights
                .Include(pf => pf.Flight).ThenInclude(f => f.Airline)
                .Include(pf => pf.Flight).ThenInclude(f => f.Origin)
                .Include(pf => pf.Flight).ThenInclude(f => f.Destination)
                .Include(pf => pf.Purchase).ThenInclude(u => u.User)
                .ToListAsync();

            if (!String.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();
                purchases = await _context.PurchasedFlights.Where(pf =>
                    pf.Purchase.User.UserName.ToLower().Contains(searchString) ||
                    pf.Flight.Number.ToLower().Contains(searchString) ||
                    pf.Flight.Airline.AirlineName.ToLower().Contains(searchString) ||
                    pf.Flight.Origin.Name.ToLower().Contains(searchString) ||
                    pf.Flight.Destination.Name.ToLower().Contains(searchString) ||
                    pf.Purchase.WhenPaid.ToString().Contains(searchString) ||
                    pf.DepartureDate.ToString().Contains(searchString)
                    )
                    .ToListAsync();

                if (purchases.Count <= 0)
                {
                    TempData["notfound"] = "No related purchase Found!";
                }
            }
            return View(purchases);
        }

    }
}
