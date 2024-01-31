using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TravelBooking.Data;
using TravelBooking.Data.ViewModels;
using TravelBooking.Models;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;


namespace TravelBooking.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminFlightController : Controller
    {
        private readonly TravelBookingContext _context;
        [BindProperty]
        public string Origin { get; set; }

        private readonly IAuthenticationService _authenticationService;

        [BindProperty]
        public string Destination { get; set; }

        public AdminFlightController(TravelBookingContext context)
        {
            _context = context;
        }

        // GET: Flight
        public async Task<IActionResult> Index()
        {
            return _context.Flights != null ?
                        View(await _context.Flights.Include("Origin").Include("Destination").Include("Airline").ToListAsync()) :
                        Problem("Entity set 'TravelBookingContext.Flights'  is null.");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(string searchWord)
        {
            List<Flight> searcheRes = new List<Flight>();

            if (searchWord == null)
            {
                return _context.Flights != null ?
                View(await _context.Flights.Include("Origin").Include("Destination").Include("Airline").ToListAsync()) :
                Problem("Entity set 'TravelBookingContext.Flights'  is null.");
            }
            else
            {
                searchWord = searchWord.ToLower();
                searcheRes = await _context.Flights.Include("Origin").Include("Destination").Include("Airline").Where(a => a.Origin.Name.ToLower().Contains(searchWord) || a.Destination.Name.ToLower().Contains(searchWord) || a.Number.ToLower().Contains(searchWord)).ToListAsync();
                if (searcheRes.Count == 0)
                {
                    TempData["FlightOption"] = "No search results";

                }
            }
            return View(searcheRes);
        }


        // GET: Flight/Details/5
        // public async Task<IActionResult> Details(int? id)
        // {
        //     if (id == null || _context.Flights == null)
        //     {
        //         return NotFound();
        //     }

        //     var flight = await _context.Flights
        //         .FirstOrDefaultAsync(m => m.Id == id);
        //     if (flight == null)
        //     {
        //         return NotFound();
        //     }

        //     return View(flight);
        // }

        // GET: Flight/Create
        public IActionResult Create()
        {
            List<Airline> airlineList = _context.Airlines.ToList(); // exception
            List<Airport> airportList = _context.Airports.ToList(); // exception
            if (airlineList.Count == 0)
            {
                TempData["FlightCreateOption"] = "Please add airline first";
            }
            if (airportList.Count == 0)
            {
                TempData["FlightCreateOption"] = "Please add airport first";
            }

            NewFlightVM newFlight = new NewFlightVM();
            newFlight.AirlineList = airlineList;
            newFlight.AirportList = airportList;
            return View(newFlight);
        }

        // POST: Flight/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(NewFlightVM newFlight)
        {
            try
            {
                List<Airline> airlineList = await _context.Airlines.ToListAsync(); // exception
                List<Airport> airportList = await _context.Airports.ToListAsync(); // exception

                newFlight.AirlineList = airlineList;
                newFlight.AirportList = airportList;
                Flight ifFlight = await _context.Flights.FirstOrDefaultAsync(f => f.Number == newFlight.Number);

                if (ifFlight != null)
                {
                    TempData["FlightCreateOption"] = $"Flight number {newFlight.Number} has already existed";
                    return View(newFlight);
                }

                if (!ModelState.IsValid)
                {
                    return View(newFlight);
                }
                if (newFlight.Origin == newFlight.Destination)
                {
                    TempData["FlightCreateOption"] = "Origin and destination can not be the same";
                    return View(newFlight);
                }
                Airport origin = await _context.Airports.FirstOrDefaultAsync(a => a.Id == newFlight.Origin);
                Airport destination = await _context.Airports.FirstOrDefaultAsync(a => a.Id == newFlight.Destination);
                Airline airline = await _context.Airlines.FirstOrDefaultAsync(a => a.Id == newFlight.Airline);

                var flight = new Flight()
                {
                    Airline = airline,
                    Number = newFlight.Number,
                    Origin = origin,
                    Destination = destination,
                    DepartureTime = newFlight.DepartureTime,
                    FlightDurationMinutes = newFlight.FlightDurationMinutes,
                    Price = newFlight.Price
                };
                await _context.Flights.AddAsync(flight);
                await _context.SaveChangesAsync();
                TempData["FlightOption"] = $"Flight {flight.Number} has been created successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (SystemException ex)
            {
                TempData["FlightCreateOption"] = $"{ex.Message}";
                return View();
            }
        }


        // GET: Flight/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                NewFlightVM newFlight = new NewFlightVM();
                List<Airline> airlineList = await _context.Airlines.ToListAsync();
                newFlight.AirlineList = airlineList;
                List<Airport> airportList = await _context.Airports.ToListAsync(); // exception
                newFlight.AirportList = airportList;
                var flight = await _context.Flights.Include("Origin").Include("Destination").FirstOrDefaultAsync(f => f.Id == id);

                newFlight.Id = flight.Id;
                newFlight.Airline = flight.Airline.Id;
                newFlight.Number = flight.Number;
                newFlight.Origin = flight.Origin.Id;
                newFlight.Destination = flight.Destination.Id;
                newFlight.DepartureTime = flight.DepartureTime;
                newFlight.FlightDurationMinutes = flight.FlightDurationMinutes;
                newFlight.Price = flight.Price;

                return View(newFlight);
            }
            catch (SystemException ex)
            {
                TempData["FlightEditOption"] = $"{ex.Message}";
                return View();
            }
        }

        // POST: Flight/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(NewFlightVM newFlight, int id)
        {
            try
            {
                List<Airline> airlineList = await _context.Airlines.ToListAsync();
                newFlight.AirlineList = airlineList;
                List<Airport> airportList = await _context.Airports.ToListAsync(); // exception
                newFlight.AirportList = airportList;
                if (ModelState.IsValid)
                {
                    if (newFlight.Origin == newFlight.Destination)
                    {
                        TempData["FlightEditOption"] = $"Origin and destination can not be the same";
                        return View(newFlight);
                    }
                    var flight = _context.Flights.FirstOrDefault(f => f.Id == id);
                    List<Flight> flightsList = _context.Flights.ToList<Flight>();
                    flightsList.Remove(flight);
                    // if Number exists
                    Flight ifFlight = flightsList.Find(f => f.Number == newFlight.Number);
                    if (ifFlight != null)
                    {
                        TempData["FlightEditOption"] = $"Flight number {newFlight.Number} has already existed";
                        return View(newFlight);
                    }

                    Airline airline = await _context.Airlines.FirstOrDefaultAsync(a => a.Id == newFlight.Airline);
                    Airport origin = await _context.Airports.FirstOrDefaultAsync(a => a.Id == newFlight.Origin);
                    Airport destination = await _context.Airports.FirstOrDefaultAsync(a => a.Id == newFlight.Destination);

                    flight.Airline = airline;
                    flight.Number = newFlight.Number;
                    flight.Origin = origin;
                    flight.Destination = destination;
                    flight.DepartureTime = newFlight.DepartureTime;
                    flight.FlightDurationMinutes = newFlight.FlightDurationMinutes;
                    flight.Price = newFlight.Price;
                    _context.Flights.Update(flight);
                    await _context.SaveChangesAsync();
                    TempData["FlightOption"] = $"Flight {flight.Number} has been updated successfully";
                    return RedirectToAction(nameof(Index));
                }
                return View(newFlight);
            }
            catch (SystemException ex)
            {
                TempData["FlightEditOption"] = $"{ex.Message}";
                return View();
            }
        }

        // GET: Flight/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            try
            {
                var flight = await _context.Flights.Include("Origin").Include("Destination").Include("Airline")
                    .FirstOrDefaultAsync(m => m.Id == id);
                if (flight == null)
                {
                    return NotFound();
                }
                return View(flight);
            }
            catch (SystemException ex)
            {
                TempData["FlightDeleteOption"] = $"{ex.Message}";
                return View();
            }
        }

        // POST: Flight/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var flight = await _context.Flights.Include("Origin").Include("Destination").Include("Airline").FirstOrDefaultAsync(m => m.Id == id);
                var purchasedFlight = await _context.PurchasedFlights.Include("Flight").FirstOrDefaultAsync(pf => pf.Flight.Id == id);
                if (purchasedFlight !=null){
                    TempData["FlightDeleteOption"] = "There are purchased flights related to this flight, can not delete it";
                    return View(flight);
                }
                if (flight != null)
                {
                    _context.Flights.Remove(flight);
                    await _context.SaveChangesAsync();
                    TempData["FlightOption"] = $"Flight {id} has been deleted successfully";
                    return RedirectToAction(nameof(Index));
                }
                TempData["FlightDeleteOption"] = $"Flight {id} does not exist";
                return View();
            }
            catch (SystemException ex)
            {
                TempData["FlightDeleteOption"] = $"{ex.Message}";
                return View();
            }

        }
    }
}
