

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Google;


namespace TravelBooking.Controllers
{
    public class GoogleLoginController: Controller
    {
        private UserManager<IdentityUser> userManager;
        private SignInManager<IdentityUser> signInManager;
        public GoogleLoginController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
    {
        this.userManager = userManager;
        this.signInManager = signInManager;
    }
    
   [AllowAnonymous]
   public IActionResult Login()
    {
         string redirectUrl = Url.Action("GoogleResponse","GoogleLogin");
         var properties = signInManager.ConfigureExternalAuthenticationProperties("Google", redirectUrl);
         return new ChallengeResult("Google", properties);      
    }
     
  
      [AllowAnonymous]
        public async Task<IActionResult> GoogleResponse()
        {
         
            ExternalLoginInfo info = await signInManager.GetExternalLoginInfoAsync();
            if (info == null){
                @TempData["loginFailed"] = "login failed";
                return RedirectToAction("Login", "Account");
            }
 
            var result = await signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, false);
            string[] userInfo = { info.Principal.FindFirst(ClaimTypes.Name).Value, info.Principal.FindFirst(ClaimTypes.Email).Value };
            if (result.Succeeded){
            //    @TempData["loginFailed"] = "login succeed";
                return RedirectToAction("Index", "Home");
            }
            else
            {
                IdentityUser user = new IdentityUser
                {
                    Email = info.Principal.FindFirst(ClaimTypes.Email).Value,
                    UserName = info.Principal.FindFirst(ClaimTypes.Email).Value,
                    EmailConfirmed = true
                };
 
                IdentityResult identResult = await userManager.CreateAsync(user);
               
                if (identResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "User");
                    identResult = await userManager.AddLoginAsync(user, info);
                    if (identResult.Succeeded)
                    {
                        await signInManager.SignInAsync(user, false);
                        //   @TempData["loginFailed"] = "user registerd and logged in";
                          return RedirectToAction("Index", "Home");
                        // return RedirectToAction("Index", "Home");
                    }
                }
               return RedirectToAction("Login", "Account");
            }
        }

    }
}