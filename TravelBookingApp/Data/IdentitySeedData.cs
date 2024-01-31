using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TravelBooking.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace TravelBooking.Data
{
    public class IdentitySeedData
    {
        public static async Task InitializeAsync(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            string[] roleNamesList = new string[] { "Admin", "User" };
            foreach (string roleName in roleNamesList)
            {
                if (!roleManager.RoleExistsAsync(roleName).Result)
                {
                    IdentityRole role = new IdentityRole();
                    role.Name = roleName;
                    IdentityResult result = roleManager.CreateAsync(role).Result;
                    // Warning: we ignore any errors that Create may return, they should be AT LEAST logged in!
                    foreach (IdentityError error in result.Errors)
                    {
                        // TODO: Log it!
                    }
                }
            }
            // CREATE admin -- Testing ONLY
            string adminEmail = "admin@admin.com";
            string adminUserName = "Admin";
            string adminPhoneNumber = "2345678901";
            string adminPass = "abcdefgA2@";
            if (userManager.FindByNameAsync(adminEmail).Result == null)
            {
                IdentityUser user = new IdentityUser();
                user.UserName = adminUserName;
                user.PhoneNumber = adminPhoneNumber;
                user.Email = adminEmail;
                user.EmailConfirmed = true;
                IdentityResult result = userManager.CreateAsync(user, adminPass).Result;

                if (result.Succeeded)
                {
                    IdentityResult result2 = userManager.AddToRoleAsync(user, "Admin").Result;
                    if (!result2.Succeeded)
                    {
                        //FIXME: log the error
                    }
                }
                else
                {
                    //FIXME: log the error
                }
            }
        }
    }
}