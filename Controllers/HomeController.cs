using System.Diagnostics;
using System.Net;
using System.Net.Mail;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlineLearningPlatform.Context.Identity;
using OnlineLearningPlatform.Models;
using OnlineLearningPlatform.Repositories;
using OnlineLearningPlatform.ViewModels;

namespace OnlineLearningPlatform.Controllers;

public class HomeController : BaseController {
    private readonly IUnitOfWork _db;
    private readonly IMapper _mapper;

    public HomeController(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        UserManager<ApplicationUser> userManager
    )
        : base(userManager) {
        _db = unitOfWork;
        _mapper = mapper;
    }
    [AllowAnonymous]
    public IActionResult Index() {
        var courses = _db.Courses.Get();

        var courseViewModels = _mapper.Map<IEnumerable<CourseViewModel>>(courses);

        var categories = _db.Categories.Get();
        ViewData["Categories"] = _mapper.Map<IEnumerable<CategoryViewModel>>(categories);

        return View(courseViewModels);
    }

    [HttpPost]
    public async Task<IActionResult> SendEmail(
        string fullName,
        string email,
        string phone,
        string message
    ) {
        try {
            var smtpClient = new SmtpClient("smtp.office365.com") {
                Port = 587,
                Credentials = new NetworkCredential("mozekry.68@outlook.sa", ""),
                EnableSsl = true
            };

            var mailMessage = new MailMessage {
                From = new MailAddress("mozekry.68@outlook.sa"),
                Subject = $"New Contact Form Submission from {fullName}",
                Body =
                    $"<h4>New message from {fullName}</h4>"
                    + $"<p>Email: {email}</p>"
                    + $"<p>Phone: {phone}</p>"
                    + $"<p>Message: {message}</p>",
                IsBodyHtml = true
            };

            mailMessage.To.Add("mozekry.68@outlook.sa");

            await smtpClient.SendMailAsync(mailMessage);

            ViewBag.Message = "Email sent successfully!";
        }
        catch (Exception ex) {
            ViewBag.Message = $"Error: {ex.Message}";
        }

        return RedirectToAction("Index");
    }

    [HttpPost]
    public IActionResult Search(string query) {
        var results = _db.Courses.Get(x => x.Name.Contains(query));

        var courseViewModels = _mapper.Map<IEnumerable<CourseViewModel>>(results);

        var categories = _db.Categories.Get();
        ViewData["Categories"] = _mapper.Map<IEnumerable<CategoryViewModel>>(categories);

        return PartialView("_CourseListPartial", courseViewModels);
    }

    [HttpPost]
    public IActionResult GetCourses() {
        var results = _db.Courses.Get();

        var courseViewModels = _mapper.Map<IEnumerable<CourseViewModel>>(results);

        var categories = _db.Categories.Get();
        ViewData["Categories"] = _mapper.Map<IEnumerable<CategoryViewModel>>(categories);

        return PartialView("_CourseListPartial", courseViewModels);
    }

    [HttpPost]
    public IActionResult Filter(int id) {
        var results = _db.Courses.Get(x => x.CategoryId == id);

        var courseViewModels = _mapper.Map<IEnumerable<CourseViewModel>>(results);

        var categories = _db.Categories.Get();
        ViewData["Categories"] = _mapper.Map<IEnumerable<CategoryViewModel>>(categories);

        return PartialView("_CourseListPartial", courseViewModels);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() {
        return View(
            new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier }
        );
    }
}