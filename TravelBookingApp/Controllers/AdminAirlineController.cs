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
using Microsoft.AspNetCore.Authorization;

namespace TravelBooking.Controllers
{
    [Authorize(Roles="Admin")]
    public class AdminAirlineController : Controller
    {
        private readonly TravelBookingContext _context;

        public AdminAirlineController(TravelBookingContext context)
        {
            _context = context;
        }

        // GET: Flight
        public async Task<IActionResult> Index()
        {
            return _context.Airlines != null ?
                        View(await _context.Airlines.ToListAsync()) :
                        Problem("Entity set 'TravelBookingContext.Airlines' is null.");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(string searchWord)
        {
            if (searchWord == null)
            {
                return _context.Airlines != null ?
                            View(await _context.Airlines.ToListAsync()) :
                            Problem("Entity set 'TravelBookingContext.Airlines'  is null.");
            }
            else
            {
                searchWord = searchWord.ToLower();
                List<Airline> searcheRes = await _context.Airlines.Where(a => a.AirlineName.ToLower().Contains(searchWord)).ToListAsync();
                if (searcheRes.Count == 0)
                {
                    TempData["AirlineOption"] = "No search results";

                }
                return View(searcheRes);
            }
        }

        // GET: Airline/Create
        public IActionResult Create()
        {
            return View(new Airline());
        }

        // POST: Flight/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Airline airline)
        {
            try
            {
                Airline ifAirline = await _context.Airlines.FirstOrDefaultAsync(a => a.AirlineName == airline.AirlineName);

                if (ifAirline != null)
                {
                    TempData["AirlineCreateOption"] = $"Airline {airline.AirlineName} has already existed";
                    return View(airline);
                }
                if (!ModelState.IsValid)
                {
                    return View(airline);
                }
                _context.Airlines.Add(airline);
                await _context.SaveChangesAsync();
                TempData["AirlineOption"] = $"Airline {airline.AirlineName} has been created successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (SystemException ex)
            {
                TempData["AirlineCreateOption"] = $"{ex.Message}";
                return View();
            }
        }

        // GET: Flight/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                Airline airline = await _context.Airlines.FirstOrDefaultAsync(a => a.Id == id);
                return View(airline);
            }
            catch (SystemException ex)
            {
                TempData["AirlineEditOption"] = $"{ex.Message}";
                return View();
            }
        }

        // POST: Flight/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Airline airline )
        {  
            TempData["airline edit inError"] = "";
            try {
            
               var currentAirline= _context.Airlines.FirstOrDefault(a => a.AirlineName == airline.AirlineName);
                List<Airline> airlinesList = _context.Airlines.ToList<Airline>();
                airlinesList.Remove(airline);
                // if Number exists
               Airline ifAirline = airlinesList.Find(a => a.AirlineName == airline.AirlineName);
                if (ifAirline != null)
                {
                    TempData["AirlineEditOption"] = $"Airport Name {airline.AirlineName} has already existed";
                    return View(airline);
                }
                 
            if (ModelState.IsValid)
            {   
                _context.Airlines.Update(airline);
                await _context.SaveChangesAsync();
                TempData["airline edit success"] = $"Airline{airline.AirlineName} has been updated successfully";
                return RedirectToAction(nameof(Index));
        }
                return View(airline);
            }
            catch (SystemException ex)
            {
                TempData["AirlineEditOption"] = $"{ex.Message}";
                return View();
            }
        }

        // GET: Flight/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            try
            {

                var airline = await _context.Airlines
                    .FirstOrDefaultAsync(m => m.Id == id);
                if (airline == null)
                {
                    return NotFound();
                }

                return View(airline);
            }
            catch (SystemException ex)
            {
                TempData["AirlineDeleteOption"] = $"{ex.Message}";
                return View();
            }
        }

        // POST: Flight/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {

                var airline = await _context.Airlines.FindAsync(id);
                if (airline != null)
                {
                    Flight flight = await _context.Flights.FirstOrDefaultAsync(f => f.Airline.AirlineName == airline.AirlineName);

                    if (flight != null)
                    {
                        TempData["AirlineDeleteOption"] = "There are flights related to this airline, can not delete it";
                        return View(airline);
                    }

                }
                _context.Airlines.Remove(airline);
                await _context.SaveChangesAsync();
                TempData["AirlineOption"] = $"Airline {airline.AirlineName} has been deleted successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (SystemException ex)
            {
                TempData["AirlineDeleteOption"] = $"{ex.Message}";
                return View();
            }
        }

    }
}
