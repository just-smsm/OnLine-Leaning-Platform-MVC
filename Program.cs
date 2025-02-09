using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using OnlineLearningPlatform.Context;
using OnlineLearningPlatform.Context.Identity;
using OnlineLearningPlatform.Handlers;
using OnlineLearningPlatform.Mapping;
using OnlineLearningPlatform.Repositories;
using OnlineLearningPlatform.Requirements;
using Stripe;

namespace OnlineLearningPlatform;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Configure Identity Password Options
        builder.Services.Configure<IdentityOptions>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequiredLength = 8;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireLowercase = false;
            options.Password.RequiredUniqueChars = 6;

            // Lockout settings (optional)
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;

            // User settings
            options.User.RequireUniqueEmail = true;
        });

        // Add services to the container.
        builder.Services.AddControllersWithViews();

        // Inject the ApplicationDbContext
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
        });

        // Identity Configurations
        builder.Services
            .AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        // Configure Cookie Authentication
        builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.LoginPath = "/Account/Login"; // Redirect to login if not authenticated
                options.AccessDeniedPath = "/Account/AccessDenied"; // Redirect if access is denied
                options.ExpireTimeSpan = TimeSpan.FromDays(14); // Authentication cookie expiration
                options.SlidingExpiration = true; // Refresh expiration time upon use
            });

        // Register Unit of Work and Repositories
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Configure AutoMapper
        builder.Services.AddAutoMapper(typeof(MappingProfile));

        // Configure Stripe for payments
        StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

        // Add Authorization Policies
        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("EnrolledInCourse", policy =>
                policy.Requirements.Add(new EnrolledInCourseRequirement()));
        });

        // Register Authorization Handlers
        builder.Services.AddScoped<IAuthorizationHandler, EnrolledInCourseHandler>();

        // Build the application
        var app = builder.Build();

        // Seed Data (if applicable)
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            await SeedData.Initialize(services);
        }

        // Configure HTTP Request Pipeline
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts(); // Enforce HTTPS in production
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        // Enable Authentication and Authorization Middleware
        app.UseAuthentication();
        app.UseAuthorization();

        // Configure Default Route
        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}"
        );

        // Run the application
        app.Run();
    }
}
