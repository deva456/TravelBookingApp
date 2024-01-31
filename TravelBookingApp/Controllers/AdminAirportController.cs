using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelBooking.Data;
using TravelBooking.Models;
using TravelBooking.Data.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Azure.Storage.Blobs;

namespace TravelBooking.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminAirportController : Controller
    {
        private readonly TravelBookingContext _context;
        private readonly string _storageConnectionString;
        private readonly string _storageContainerName;
        private readonly BlobContainerClient _client;
        private readonly ILogger<AdminAirportController> _logger;
        private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment;

        public AdminAirportController(IConfiguration configuration, TravelBookingContext context, ILogger<AdminAirportController> logger)
        {
            _context = context;
            _storageConnectionString = configuration.GetValue<string>("BlobConnectionString");
            _storageContainerName = configuration.GetValue<string>("BlobContainerName");
            _logger = logger;
            _client = new BlobContainerClient(_storageConnectionString, _storageContainerName);
        }

        // GET: AdminAirport
        public async Task<IActionResult> Index()
        {
            return _context.Airports != null ?
                       View(await _context.Airports.ToListAsync()) :
                       Problem("Entity set 'TravelBookingContext.Airports'  is null.");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(string searchWord)
        {
            if (searchWord == null)
            {
                return _context.Airports != null ?
                           View(await _context.Airports.ToListAsync()) :
                           Problem("Entity set 'TravelBookingContext.Airports'  is null.");
            }
            else
            {
                searchWord = searchWord.ToLower();
                List<Airport> searcheRes = await _context.Airports.Where(a => a.Name.ToLower().Contains(searchWord) || a.Code.ToLower().Contains(searchWord)).ToListAsync();
                if (searcheRes.Count == 0)
                {
                    TempData["AirportOption"] = "No search results";
                }
                return View(searcheRes);
            }
        }
        // GET: AdminAirport/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            try
            {
                var airport = await _context.Airports
                    .FirstOrDefaultAsync(m => m.Id == id);
                if (airport == null)
                {
                    TempData["airport not exist"] = $"Airport {id} does not exist";
                    return View();
                }
                airport.ImageUrl = _client.Uri.ToString() + "/" + airport.ImageUrl;

                // Replace with the latitude and longitude of the location you want to center the map on
                int latitudedegreePosition = airport.Latitude.IndexOf('°');
                string latitudeStr = airport.Latitude.Substring(0, latitudedegreePosition);
                double lat = Convert.ToDouble(latitudeStr);

                int longitudedegreePosition = airport.Longitude.IndexOf('°');
                string longitudeStr = airport.Longitude.Substring(0, longitudedegreePosition);
                double lng = Convert.ToDouble(longitudeStr);
                ViewData["APIKey"] = "AIzaSyCb_kdxjG-fDsyKhxs66L3XnNZwDzREMkw";
                ViewData["Latitude"] = lat;
                ViewData["Longitude"] = lng;
                return View(airport);
            }
            catch (SystemException ex)
            {
                TempData["AirportOption"] = $"{ex.Message}";
                return View();
            }
        }

        // GET: AdminAirport/Create
        public IActionResult Create()
        {
            return View(new NewAirportVM());
        }

        // POST: AdminAirport/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(NewAirportVM newAirport)
        {
            try
            {
                Airport ifAirportName = await _context.Airports.FirstOrDefaultAsync(a => a.Name == newAirport.Name);
                if (ifAirportName != null)
                {
                    TempData["AirportCreateOption"] = $"Airport {newAirport.Name} has already existed";
                    return View(newAirport);
                }

                Airport ifAirportCode = await _context.Airports.FirstOrDefaultAsync(a => a.Code == newAirport.Code);
                if (ifAirportCode != null)
                {
                    TempData["AirportCreateOption"] = $"Airport {newAirport.Code} has already existed";
                    return View(newAirport);
                }

                if (!ModelState.IsValid)
                {
                    return View(newAirport);
                }
                string fileName = newAirport.LogoImage.FileName.Trim();
                // Create a BlobClient using the Blob storage connection string
                var blobClient = new BlobClient(_storageConnectionString, _storageContainerName, fileName);

                // Upload the image data to the Blob storage
                using (var stream = newAirport.LogoImage.OpenReadStream())
                {
                    await blobClient.UploadAsync(stream);
                }
                Airport airport = new Airport()
                {
                    Code = newAirport.Code,
                    Name = newAirport.Name,
                    Latitude = $"{newAirport.LatitudeValue.ToString()}°{newAirport.LatitudeDirection}",
                    Longitude = $"{newAirport.LongitudeValue.ToString()}°{newAirport.LongitudeDirection}",
                    ImageUrl = newAirport.LogoImage.FileName
                };
                _context.Airports.Add(airport);
                await _context.SaveChangesAsync();
                TempData["AirportOption"] = $"{airport.Name} has been created successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (SystemException ex)
            {
                TempData["AirportCreateOption"] = $"{ex.Message}";
                return View();
            }
        }

        // GET: AdminAirport/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var airport = await _context.Airports.FirstOrDefaultAsync(f => f.Id == id);
                int latitudedegreePosition = airport.Latitude.IndexOf('°');
                string latitudeStr = airport.Latitude.Substring(0, latitudedegreePosition);
                double latitudeValue = Convert.ToDouble(latitudeStr);
                int longitudedegreePosition = airport.Longitude.IndexOf('°');
                string longitudeStr = airport.Longitude.Substring(0, longitudedegreePosition);
                double longitudeValue = Convert.ToDouble(longitudeStr);
                string blobUrl = _client.Uri.ToString();
                EditAirportVM newAirport = new EditAirportVM
                {
                    Id = airport.Id,
                    Code = airport.Code,
                    Name = airport.Name,
                    LatitudeValue = latitudeValue,
                    LongitudeValue = longitudeValue,
                    LogoImage = null,
                    ImageUrl = blobUrl + "/" + airport.ImageUrl
                };

                return View(newAirport);
            }
            catch (SystemException ex)
            {
                TempData["AirportEditOption"] = $"{ex.Message}";
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditAirportVM editAirport, int id)
        {
            if (!ModelState.IsValid)
            {
                return View(editAirport);
            }
            try
            {
                var airport = _context.Airports.FirstOrDefault(a => a.Id == id);
                List<Airport> airportsList = _context.Airports.ToList<Airport>();
                airportsList.Remove(airport);
                // if Number exists
                Airport ifAirportName = airportsList.Find(a => a.Name == editAirport.Name);
                if (ifAirportName != null)
                {
                    TempData["AirportEditOption"] = $"Airport {editAirport.Name} has already existed";
                    return View(editAirport);
                }

                Airport ifAirportCode = airportsList.Find(a => a.Code == editAirport.Code);
                if (ifAirportCode != null)
                {
                    TempData["AirportEditOption"] = $"Airport {editAirport.Code} has already existed";
                    return View(editAirport);
                }
             
                // Create a BlobClient using the Blob storage connection string
                if (editAirport.LogoImage != null)
                {
                    string fileName = editAirport.LogoImage.FileName.Trim();
                    //var blobClient = new BlobClient( _storageConnectionString,  _storageContainerName, newAirport.LogoImage.FileName);
                    // BlobContainerClient client = new BlobContainerClient(_storageConnectionString, _storageContainerName);
                    BlobClient file = _client.GetBlobClient(airport.ImageUrl);

                    await file.DeleteAsync();

                    BlobClient newfile = _client.GetBlobClient(fileName);
                    // Upload the image data to the Blob storage

                    // Upload the image data to the Blob storage
                    using (var stream = editAirport.LogoImage.OpenReadStream())
                    {
                        await newfile.UploadAsync(stream);
                    }


                    //    var image = new BlobDto
                    //           {
                    //             FileName = newAirport.LogoImage.FileName,
                    //             ContentType = newAirport.LogoImage.ContentType,
                    //             URL = client.Uri.ToString()
                    //           };
                    airport.ImageUrl = fileName;
                }
                airport.Code = editAirport.Code;
                airport.Name = editAirport.Name;
                airport.Latitude = $"{editAirport.LatitudeValue.ToString()}°{editAirport.LatitudeDirection}";
                airport.Longitude = $"{editAirport.LongitudeValue.ToString()}°{editAirport.LongitudeDirection}";

                _context.Airports.Update(airport);
                await _context.SaveChangesAsync();
                TempData["AirportOption"] = $"{airport.Name} has been Edited successfully";
                return RedirectToAction(nameof(Index));
            }


            catch (SystemException ex)
            {
                TempData["AirportEditOption"] = $"{ex.Message}";
                return View();
            }
        }


        // GET: AdminAirport/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            try
            {
                var airport = await _context.Airports
                    .FirstOrDefaultAsync(m => m.Id == id);
                if (airport == null)
                {
                    return NotFound();
                }
                airport.ImageUrl = _client.Uri.ToString() + "/" + airport.ImageUrl;
                return View(airport);
            }
            catch (SystemException ex)
            {
                TempData["AirportDeleteOption"] = $"{ex.Message}";
                return View();
            }
        }

        // POST: AdminAirport/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var airport = await _context.Airports.FirstOrDefaultAsync(a => a.Id == id);
                if (airport != null)
                {
                    //var blobClient = new BlobClient( _storageConnectionString,  _storageContainerName, newAirport.LogoImage.FileName);

                    Flight flightOrigin = await _context.Flights.FirstOrDefaultAsync(f => f.Origin.Name == airport.Name);
                    Flight flightDestination = await _context.Flights.FirstOrDefaultAsync(f => f.Destination.Name == airport.Name);

                    if (flightOrigin != null || flightDestination != null)
                    {
                        TempData["AirportDeleteOption"] = "There are flights related to this airport, can not delete it";
                        airport.ImageUrl = _client.Uri.ToString() + "/" + airport.ImageUrl;
                        return View(airport);
                    }

                    BlobClient file = _client.GetBlobClient(airport.ImageUrl);

                    await file.DeleteAsync();

                    _context.Airports.Remove(airport);
                    TempData["AirportOption"] = $"Airport {airport.Name} has been deleted successfully";
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }

                TempData["AirportDeleteOption"] = $"Airport {id} does not exist";
                return View();

            }
            catch (SystemException ex)
            {
                TempData["AirportDeleteOption"] = $"{ex.Message}";
                return View();
            }
        }

    }
}




