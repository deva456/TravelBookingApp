using TravelBooking.Data;
using TravelBooking.Data.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.Security.Claims;



namespace TravelBooking.Controllers
{
    public class AccountController : Microsoft.AspNetCore.Mvc.Controller
    {
        private readonly TravelBookingContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, TravelBookingContext context)
        {
            this._userManager = userManager;
            this._signInManager = signInManager;
            _context = context;
        }

        public async Task<IActionResult> Users()
        {
            var users = await _context.Users.ToListAsync();
            return View(users);
        }

        public IActionResult Login() => View(new LoginVM());


        [Microsoft.AspNetCore.Mvc.HttpPost]
        public async Task<IActionResult> Login(LoginVM loginVM)
        {
            if (!ModelState.IsValid) return View(loginVM);

            var user = await _userManager.FindByEmailAsync(loginVM.Email);
            if (user != null)
            {
                if (user.EmailConfirmed == true)
                {
                    var passwordCheck = await _userManager.CheckPasswordAsync(user, loginVM.Password);
                    if (passwordCheck)
                    {
                        var result = await _signInManager.PasswordSignInAsync(user, loginVM.Password, false, false);
                        if (result.Succeeded)
                        {
                            return RedirectToAction("Index", "Home");
                        }
                    }
                    TempData["Error"] = "Wrong credentials. Please, try again!";
                    return View(loginVM);
                }
                TempData["Error"] = "Please go to your email address to confirm first.";
                return View(loginVM);
            }

            TempData["Error"] = "Wrong credentials. Please, try again!";
            return View(loginVM);
        }


        public IActionResult Register() => View(new RegisterVM());

        [Microsoft.AspNetCore.Mvc.HttpPost]
        public async Task<IActionResult> Register(RegisterVM registerVM)
        {
            if (!ModelState.IsValid) return View(registerVM);

            var user = await _userManager.FindByEmailAsync(registerVM.Email);
            if (user != null)
            {
                TempData["Error"] = "This email address is already in use";
                return View(registerVM);
            }

            var newUser = new IdentityUser() { Email = registerVM.Email, UserName = registerVM.UserName, PhoneNumber = registerVM.PhoneNumber };
            var newUserResponse = await _userManager.CreateAsync(newUser, registerVM.Password);

            if (newUserResponse.Succeeded)
            {
                // sign user to "User"
                var signToUser = await _userManager.AddToRoleAsync(newUser, "User");
                if (signToUser.Succeeded)
                {
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(newUser);
                    var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = newUser.Id, code = code }, protocol: HttpContext.Request.Scheme);

                    var body = $@"<p>Thank you for registering our booking system!</p>
                            <p>Your Username is: <br/>{newUser.UserName}</p>
                            <p>Please use your Email {newUser.Email} to Login.<br/></p>
                            <a href='{callbackUrl}'>
                            Please click here to confirm your email</a>";

                    // send confirmation email(GMAIL SMTP)
                    using (var smtp = new SmtpClient())
                    {
                        var message = new MailMessage();
                        var credential = new System.Net.NetworkCredential
                        {
                            UserName = "zhukris9@gmail.com",  // replace with valid value
                            Password = "jbzhodonlwicjoeg"  // replace with valid value (SMTP generated password)
                        };
                        smtp.Credentials = credential;
                        smtp.Host = "smtp.gmail.com";
                        smtp.Port = 587;
                        smtp.EnableSsl = true;
                        message.To.Add(credential.UserName); // replace with registered email (newUser.Email)
                        message.Subject = "Travel Booking System from ZGZ";
                        message.Body = body;
                        message.IsBodyHtml = true;
                        message.From = new MailAddress("example@gmail.com");
                        await smtp.SendMailAsync(message);
                    }
                    return View("RegisterCompleted");
                }
                else
                {
                    //FIXME: delete the user since role assignment failed, log the event, show error to user
                }
            }
            foreach (var error in newUserResponse.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(registerVM);

        }

        [Microsoft.AspNetCore.Mvc.HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            TempData["loginflash"] = "Logged out";
            return RedirectToAction("Login", "Account");
        }

        public IActionResult AccessDenied(string ReturnUrl)
        {
            return View();
        }


        [Microsoft.AspNetCore.Mvc.HttpGet("ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.ConfirmEmailAsync(user, code);
            if (result.Succeeded)
            {
                TempData["loginflash"] = "Email Confirmed";
                return RedirectToAction("Login", "Account");
            }

            return View("Error");
        }


    }
}
