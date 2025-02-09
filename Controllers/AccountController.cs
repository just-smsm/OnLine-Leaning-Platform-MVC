using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlineLearningPlatform.Context.Identity;
using OnlineLearningPlatform.Repositories;
using OnlineLearningPlatform.ViewModels.Account;
using static System.Net.WebRequestMethods;

namespace OnlineLearningPlatform.Controllers;

public class AccountController : BaseController
{
    private readonly IMapper _mapper;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    

    public AccountController(
        SignInManager<ApplicationUser> signInManager,
        IMapper mapper,
        UserManager<ApplicationUser> userManager
    ) : base(userManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _mapper = mapper;
    }

    // GET: /Account/Register
    [AllowAnonymous]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }
        return View();
    }

    // POST: /Account/Register
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Check if the email is already registered
        var existingUser = await _userManager.FindByEmailAsync(model.Email);
        if (existingUser != null)
        {
            ModelState.AddModelError("Email", "This email is already registered.");
            return View(model);
        }

        // Create a new user instance
        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FirstName = model.FirstName,
            LastName = model.LastName
        };

        // Attempt to create the user
        var result = await _userManager.CreateAsync(user, model.Password);
        if (result.Succeeded)
        {
            // Assign the default role
            await _userManager.AddToRoleAsync(user, "Student");

            // Sign in the user
            await _signInManager.SignInAsync(user, isPersistent: false);

            return RedirectToAction("Index", "Home");
        }

        // Add any errors from identity to the ModelState
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(model);
    }

    // GET: /Account/Login
    [AllowAnonymous]
    public IActionResult Login()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        if (!EmailExists(model.Email))
        {
            ModelState.AddModelError(string.Empty, "This email is not registered.");
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(
            model.Email,
            model.Password,
            model.RememberMe,
            lockoutOnFailure: false
        );

        if (result.Succeeded)
        {
            Console.WriteLine("no error");
            return RedirectToAction("Index", "Home"); // No `await` needed here
        }

        if (result.IsLockedOut)
        {
            return View("Lockout");
        }

        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        return View(model);
    }

    // POST: /Account/Logout
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            await _signInManager.SignOutAsync();
        }

        return RedirectToAction("Index", "Home");
    }

    // GET: /Account/Profile
    [Authorize]
    public async Task<IActionResult> Profile()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Index", "Home");
        }

        var model = new ProfileViewModel
        {
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            ProfilePictureBase64 = user.ProfilePictureUrl != null
                ? Convert.ToBase64String(user.ProfilePictureUrl)
                : null
        };

        return View(model);
    }


    // POST: /Account/Profile
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(ProfileViewModel model, IFormFile? profilePictureUrl)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Index", "Home");

        if (profilePictureUrl != null)
        {
            using var memoryStream = new MemoryStream();
            await profilePictureUrl.CopyToAsync(memoryStream);

            if (memoryStream.Length < 2097152) // 2 MB limit
            {
                user.ProfilePictureUrl = memoryStream.ToArray();
            }
            else
            {
                ModelState.AddModelError("File", "The file is too large. Please upload a file smaller than 2 MB.");
                return View(model);
            }
        }

        user.FirstName = model.FirstName;
        user.LastName = model.LastName;

        var result = await _userManager.UpdateAsync(user);
        if (result.Succeeded)
        {
            return RedirectToAction("Index", "Home");
        }

        AddErrors(result);
        return View(model);
    }

    // GET: /Account/AccessDenied
    [AllowAnonymous]
    public IActionResult AccessDenied()
    {
        return View();
    }

    #region Helpers

    private bool EmailExists(string email)
    {
        return _userManager.Users.Any(u => u.Email == email);
    }

    private void AddErrors(IdentityResult result)
    {
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
    }

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction("Index", "Home");
    }

    #endregion
}
