using TravelBooking.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Stripe;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using Microsoft.AspNetCore;
// using System.Configuration;

var builder = WebApplication.CreateBuilder(args);
//read Values from appsettings.json
var secretKeyValue = builder.Configuration["SecretKey"];
// Debug.WriteLine("SecretKey:" + secretKeyValue);
StripeConfiguration.ApiKey = secretKeyValue;

// Add services to the container.
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();

builder.Services.AddDbContext<TravelBookingContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string not found."));
});


builder.Services.AddIdentity<IdentityUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<TravelBookingContext>()
    .AddTokenProvider<DataProtectorTokenProvider<IdentityUser>>(TokenOptions.DefaultProvider)
    .AddRoles<IdentityRole>();

builder.Services
    .AddAuthentication()
    .AddCookie()
    .AddGoogle(o =>
    {
        o.ClientId = "41243224755-9etbgr6l5asi9vvh2h4rmjqg4b618crg.apps.googleusercontent.com";
        o.ClientSecret = "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";
        // o.ClientId = "373602987509-ju7nf1ln5m18mhef8k43m4aehmaeu2ad.apps.googleusercontent.com";
        // o.ClientSecret = "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";
        o.SignInScheme = IdentityConstants.ExternalScheme;
        // o.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;;
    });

builder.Services.Configure<IdentityOptions>(options =>
{
    // Password settings.
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 8;
    options.Password.RequiredUniqueChars = 1;
    // Lockout settings.
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(25);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
    // User settings.
    options.User.AllowedUserNameCharacters =
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true;
    // Allow user with EmailConfirmed value 0/false to log in
    options.SignIn.RequireConfirmedAccount = true;
});

builder.Services.ConfigureApplicationCookie(options =>
{
    // Cookie settings.
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(25);
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

builder.Services.AddControllersWithViews();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

using (var scope = app.Services.CreateAsyncScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<TravelBookingContext>();

    context.Database.EnsureCreated();
    //context.Database.Migrate();

    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    IdentitySeedData.InitializeAsync(userManager, roleManager);
}

app.UseWebSockets();

app.Run();

