using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlineLearningPlatform.Context.Identity;
using OnlineLearningPlatform.Models;
using OnlineLearningPlatform.Repositories;
using OnlineLearningPlatform.ViewModels;
using Stripe;
using Stripe.Checkout;

namespace OnlineLearningPlatform.Controllers;

[Authorize(Roles = "Admin,Student")]
public class EnrollmentController : BaseController {
    private readonly IUnitOfWork _db;
    private readonly IMapper _mapper;
    private readonly string _stripePublishableKey;
    private readonly UserManager<ApplicationUser> _userManager;

    public EnrollmentController(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration
    ) : base(userManager) {
        _db = unitOfWork;
        _mapper = mapper;
        _userManager = userManager;
        _stripePublishableKey = configuration["Stripe:PublishableKey"] ?? string.Empty;
    }

    // GET: Enrollment
    public IActionResult Index() {
        var userId = _userManager.GetUserId(User);
        var enrollments = _db.Enrollments.Get(e => e.StudentId == userId, includeProperties: "Course,Student");

        return View(enrollments);
    }

    [HttpGet]
    public async Task<IActionResult> GetUserCourses(string userID)
    {
        if (string.IsNullOrEmpty(userID))
        {
            // Handle invalid or missing user ID
            TempData["Error"] = "User ID is required.";
            return RedirectToAction("Error", "Home");
        }

        try
        {
            // Fetch user's courses from the service or database
            var courses = await _db.Enrollments.GetUserCoursesByUserID(userID);

            if (courses == null || !courses.Any())
            {
                TempData["Error"] = "No courses found for this user.";
                return RedirectToAction("Error", "Home");
            }

            // Return the courses to a view (assume a "UserCourses" view exists)
            return View("UserCourses", courses);
        }
        catch (Exception ex)
        {
            // Log the exception and redirect to a general error page
            TempData["Error"] = "An error occurred while retrieving courses.";
            TempData["ErrorDetails"] = ex.Message;
            return RedirectToAction("Error", "Home");
        }
    }


    // GET: Enrollment/Create
    [Authorize(Roles = "Student")]
    public IActionResult Create(int courseId) {
        var userId = _userManager.GetUserId(User);
        var existingEnrollment = _db.Enrollments.Get(e => e.CourseId == courseId && e.StudentId == userId).Any();

        if (existingEnrollment) {
            TempData["Message"] = "You are already enrolled in this course.";
            return RedirectToAction(nameof(Index));
        }

        var course = _db.Courses.GetByID(courseId);
        if (course == null) return NotFound();

        var paymentViewModel = new PaymentViewModel {
            CourseId = course.Id,
            CourseName = course.Name,
            Amount = course.Price,
            StripePublishableKey = _stripePublishableKey
        };

        return View("Payment", paymentViewModel);
    }

    // POST: Enrollment/ProcessPayment
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> ProcessPayment(PaymentViewModel model) {
        if (!ModelState.IsValid) return RedirectToAction(nameof(Index));

        var course = _db.Courses.GetByID(model.CourseId);
        if (course == null) return NotFound();

        var userId = _userManager.GetUserId(User) ?? string.Empty;
        var customerOptions = new CustomerCreateOptions {
            Email = User?.Identity?.Name,
            Metadata = new Dictionary<string, string> { { "UserId", userId } }
        };

        var customerService = new CustomerService();
        var customer = await customerService.CreateAsync(customerOptions);

        var sessionOptions = new SessionCreateOptions {
            PaymentMethodTypes = new List<string> { "card" },
            LineItems = new List<SessionLineItemOptions> {
                new() {
                    PriceData = new SessionLineItemPriceDataOptions {
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions {
                            Name = course.Name
                        },
                        UnitAmount = (long)(course.Price * 100) // Stripe expects amount in cents
                    },
                    Quantity = 1
                }
            },
            Mode = "payment",
            SuccessUrl = Url.Action("PaymentSuccess", "Enrollment", new { courseId = model.CourseId },
                Request.Scheme),
            CancelUrl = Url.Action("PaymentFailed", "Enrollment", null, Request.Scheme),
            Customer = customer.Id
        };

        var sessionService = new SessionService();
        var session = await sessionService.CreateAsync(sessionOptions);

        return Redirect(session.Url);
    }

    // GET: Enrollment/PaymentSuccess
    public IActionResult PaymentSuccess(int courseId) {
        var userId = _userManager.GetUserId(User) ?? string.Empty;

        // Add enrollment to the database after successful payment
        var enrollment = new Enrollment {
            CourseId = courseId,
            StudentId = userId,
            EnrollmentDatetime = DateTime.UtcNow
        };

        _db.Enrollments.Insert(enrollment);
        _db.SaveChanges();

        TempData["Message"] = "You have successfully enrolled in the course.";
        return RedirectToAction(nameof(Index));
    }

    // GET: Enrollment/PaymentFailed
    public IActionResult PaymentFailed() {
        TempData["Error"] = "Payment was unsuccessful. Please try again.";
        return View();
    }

    // GET: Enrollment/Delete/5
    [Authorize(Roles = "Admin")]
    public IActionResult Delete(int courseId, string studentId) {
        var enrollment = _db.Enrollments.Get(e => e.CourseId == courseId && e.StudentId == studentId)
            .FirstOrDefault();

        if (enrollment == null) return NotFound();

        var enrollmentViewModel = _mapper.Map<EnrollmentViewModel>(enrollment);
        return View(enrollmentViewModel);
    }

    // POST: Enrollment/Delete/5
    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public IActionResult DeleteConfirmed(int courseId, string studentId) {
        var enrollment = _db.Enrollments.Get(e => e.CourseId == courseId && e.StudentId == studentId)
            .FirstOrDefault();
        if (enrollment != null) {
            _db.Enrollments.Delete(enrollment);
            _db.SaveChanges();
        }

        return RedirectToAction(nameof(Index));
    }
}