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
    public class ProfileController : Controller
    {
        private readonly TravelBookingContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        public ProfileController(UserManager<IdentityUser> userManager, TravelBookingContext context)
        {
            this._userManager = userManager;
            _context = context;
        }

        //GET: Users/Details/id
        public async Task<IActionResult> Index(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            ViewBag.count = HttpContext.Session.GetString("Count");
            return View(user);
        }

        //GET: Users/Edit/id
        public async Task<IActionResult> Edit(string? id)
        {
            var userName = _userManager.GetUserAsync(User).Result.UserName;
            var currentUser = _context.Users.Where(u => u.UserName == userName).FirstOrDefault();

            if(currentUser.Id != id){
                return RedirectToAction("AccessDenied", "Account");
            }
            // var currentUser = await _userManager.FindByIdAsync(id);
            if (currentUser == null)
            {
                return NotFound();
            }
            IdentityVM user = new IdentityVM() { Id = currentUser.Id, UserName = currentUser.UserName, Email = currentUser.Email, PhoneNumber = currentUser.PhoneNumber };
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string? id, [Bind("Id,UserName,Email,PhoneNumber")] IdentityVM user)
        {
            try
            {
                if (!ModelState.IsValid) return View(user);

                var currentUser = await _userManager.FindByIdAsync(id);
                if (currentUser == null) return View(currentUser);

                currentUser.UserName = user.UserName;
                currentUser.Email = user.Email;
                currentUser.PhoneNumber = user.PhoneNumber;

                var tryUpdate = await _userManager.UpdateAsync(currentUser);
                if (tryUpdate.Succeeded)
                {
                    TempData["profileflash"] = "Updated";
                    return RedirectToAction(nameof(Index));
                }
                return View(currentUser);
            }
            catch (Exception ex)
            {
                return NotFound();
            }
        }

        public async Task<IActionResult> CustomerList(string searchString)
        {
            if (_context.Users == null) return Problem("No users found.");

            // FIXME: should according to "role != admin"
            var adminEmail = "@admin.com";
            var customers = await _context.Users.Where(c => !(c.Email.Contains(adminEmail))).ToListAsync();

            if (!String.IsNullOrEmpty(searchString))
            {
                customers = await _context.Users.Where(c => c.UserName.ToLower().Contains(searchString.ToLower()) || c.Email.ToLower().Contains(searchString.ToLower())).ToListAsync();
                if (customers.Count <= 0)
                {
                    TempData["notfound"] = "No Customers Found!";
                }
            }
            return View(customers);
        }


        private bool UserExists(string id)
        {
            return (_context.Users?.Any(e => e.Id == id)).GetValueOrDefault();
        }

    }
}