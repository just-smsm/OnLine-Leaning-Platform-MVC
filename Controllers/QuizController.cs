using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineLearningPlatform.Context;
using OnlineLearningPlatform.Context.Identity;
using OnlineLearningPlatform.Models;
using OnlineLearningPlatform.Repositories;
using OnlineLearningPlatform.ViewModels;

namespace OnlineLearningPlatform.Controllers;

[Authorize(Roles = "Admin, Student")]
public class QuizController : BaseController
{
    private readonly IUnitOfWork _db;
    private readonly IMapper _mapper;
    IAuthorizationService _authorizationService;
    private readonly ApplicationDbContext dbContext;
    private readonly UserManager<ApplicationUser> _userManager;

    public QuizController(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        UserManager<ApplicationUser> userManager,
        IAuthorizationService authorizationService,ApplicationDbContext dbContext
    )
        : base(userManager)
    {
        _db = unitOfWork;
        _mapper = mapper;
        _userManager = userManager;
        _authorizationService = authorizationService;
        this.dbContext = dbContext;
    }

    public IActionResult Index()
    {
        var quizzes = _db.Quizzes.Get();
        var quizViewModels = _mapper.Map<IEnumerable<QuizViewModel>>(quizzes);

        // get courses
        var courses = _db.Courses.Get();
        ViewData["CoursesViewModel"] = _mapper.Map<IEnumerable<CourseViewModel>>(courses);

        ViewData["QuizViewModel"] = new QuizViewModel();

        return View(quizViewModels);
    }

    // GET: Quiz/Details/5
    [HttpGet]
    public IActionResult Details(int id)
    {
        try
        {
            // Fetch the quiz by its ID
            var quiz = _db.Quizzes.GetByID(id);
            if (quiz == null)
            {
                return RedirectToAction("Error", new { message = "Quiz not found." });
            }

            // Check if user is authorized to access the quiz
            var authorizationResult = _authorizationService
                .AuthorizeAsync(User, quiz.CourseId, "EnrolledInCourse")
                .Result;

            if (!authorizationResult.Succeeded && !User.IsInRole("Admin"))
            {
                return RedirectToAction("Create", "Enrollment", new { courseId = quiz.CourseId });
            }

            // Fetch and shuffle questions for the quiz
            var questions = _db.QuizQuestions.Get(q => q.QuizId == id).ToList();
            var shuffledQuestions = questions.OrderBy(q => Guid.NewGuid()).ToList();
            ViewData["Questions"] = _mapper.Map<IEnumerable<QuizQuestionViewModel>>(shuffledQuestions);

            // Fetch and shuffle answers for each question
            var answerViewModels = new List<QuizAnswerViewModel>();
            foreach (var question in shuffledQuestions)
            {
                var answers = _db.QuizAnswers.Get(a => a.QuestionId == question.Id).ToList();
                var shuffledAnswers = answers.OrderBy(a => Guid.NewGuid()).ToList();
                answerViewModels.AddRange(_mapper.Map<IEnumerable<QuizAnswerViewModel>>(shuffledAnswers));
            }
            ViewData["Answers"] = answerViewModels;

            // Map the quiz to its ViewModel and pass it to the view
            var quizViewModel = _mapper.Map<QuizViewModel>(quiz);
            return View(quizViewModel);
        }
        catch (Exception ex)
        {
           
            return RedirectToAction("Error", new { message = "An error occurred while loading the quiz details." });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Details(int id, Dictionary<int, int> Answers)
    {
        try
        {
            // Validate input
            if (Answers == null || !Answers.Any())
            {
                return RedirectToAction("Error", new { message = "No answers submitted." });
            }

            // Fetch quiz by ID
            var quiz = _db.Quizzes.GetByID(id);
            if (quiz == null)
            {
                return RedirectToAction("Error", new { message = "Quiz not found." });
            }

            // Validate MinPassScore
            if (quiz.MinPassScore == null || quiz.MinPassScore > 100 || quiz.MinPassScore < 0)
            {
                return RedirectToAction("Error", new { message = "Quiz minimum passing score is not valid." });
            }

            // Fetch questions for the quiz
            var questions = _db.QuizQuestions.Get(q => q.QuizId == id).ToList();
            if (!questions.Any())
            {
                return RedirectToAction("Error", new { message = "No questions found for this quiz." });
            }

            // Get user ID
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Error", new { message = "User is not authenticated." });
            }

            // Calculate correct answers
            int correctAnswers = 0;
            foreach (var question in questions)
            {
                if (Answers.TryGetValue(question.Id, out int selectedAnswerId))
                {
                    var correctAnswer = _db.QuizAnswers.Get(a => a.QuestionId == question.Id && a.IsCorrect).FirstOrDefault();
                    if (correctAnswer != null && correctAnswer.Id == selectedAnswerId)
                    {
                        correctAnswers++;
                    }
                }
            }

            // Calculate score percentage
            double percentage = Math.Round((double)correctAnswers / questions.Count * 100, 2);

            // Determine if the student passed
            bool isPassed = percentage >= quiz.MinPassScore;

            // Record attempt
            var attempt = new StudentQuizAttempt
            {
                StudentId = userId,
                QuizId = id,
                AttemptDatetime = DateTime.Now,
                ScoreAchieved = (int)percentage // Use the percentage score
            };

            // Save the attempt to the database
            _db.StudentQuizAttempts.Insert(attempt);
            _db.SaveChanges();

            // Fetch the saved attempt using the async method
            var result = await _db.StudentQuizAttempts.GetBYIncrementId(attempt.StudentId, attempt.QuizId, attempt.Id);

            if (result == null)
            {
                return RedirectToAction("Error", new { message = "Quiz attempt could not be found." });
            }

            // Redirect based on result
            return isPassed
                ? RedirectToAction("Passed", new { id = result.Id }) // Use the auto-incremental Id
                : RedirectToAction("Failed", new { id = result.Id }); // Use the auto-incremental Id
        }
        catch (Exception ex)
        {
            return RedirectToAction("Error", new { message = "An error occurred while submitting your quiz attempt." });
        }
    }
    [HttpGet]
    [Authorize(Roles = "Student")] // Ensure only students can access this action
    public async Task<IActionResult> UserQuizzes()
    {
        try
        {
            // Get the authenticated user's ID
            var studentId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(studentId))
            {
                ViewBag.Error = "User not authenticated.";
                return View("Error");
            }

            // Fetch quizzes for the authenticated student
            var quizzes = await dbContext.StudentQuizAttempts
                .Where(q => q.StudentId == studentId)
                .Select(q => new UserQuizViewModel
                {
                    QuizId = q.QuizId,
                    QuizName = q.Quiz.Name, // Assuming navigation property Quiz
                    ScoreAchieved = q.ScoreAchieved,
                    AttemptDate = q.AttemptDatetime
                })
                .ToListAsync();

            // If no quizzes are found, return an empty view
            if (!quizzes.Any())
            {
                ViewBag.Message = "No quizzes found.";
                return View(new List<UserQuizViewModel>());
            }

            // Return the quizzes to the view
            return View(quizzes);
        }
        catch (Exception ex)
        {
            // Log the exception
            Console.WriteLine($"Error fetching quizzes: {ex.Message}");
            ViewBag.Error = "An error occurred while retrieving quizzes.";
            return View("Error");
        }
    }

    public IActionResult Passed(int id)
    {
        var result = _db
            .StudentQuizAttempts.Get(st =>
                st.Id == id && st.StudentId == _userManager.GetUserId(User)
            )
            .ElementAt(0);
        return View(result);
    }

    public IActionResult Failed(int id)
    {
        var attempts = _db
            .StudentQuizAttempts.Get(st =>
                st.Id == id && st.StudentId == _userManager.GetUserId(User)
            )
            .ToList();

        if (attempts.Count == 0)
        {
            // Handle the case where no attempts are found
            // Redirect to an appropriate page or display an error message
            return RedirectToAction("Error", "Home", new { message = "No attempts found for this quiz." });
        }

        var result = attempts.First(); // Safely access the first element
        return View(result);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CreateQuiz(QuizViewModel model)
    {
        if (ModelState.IsValid)
        {
            var quiz = _mapper.Map<Quiz>(model);
            _db.Quizzes.Insert(quiz);
            _db.SaveChanges();
        }
        return RedirectToAction("Index");
    }

    // GET: Quiz/Edit/5
    [HttpGet]
    public IActionResult Edit(int? id)
    {
        if (id == null)
            return NotFound();

        var quiz = _db.Quizzes.GetByID(id);
        if (quiz == null)
            return NotFound();

        var quizViewModel = _mapper.Map<QuizViewModel>(quiz);

        // get courses related to each quiz
        GetCourses();

        return View(quizViewModel);
    }

    // POST: Course/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, QuizViewModel quizViewModel)
    {
        if (id != quizViewModel.Id)
            return NotFound();
        var course = _db.Courses.GetByID(quizViewModel.CourseId);

        if (ModelState.IsValid)
        {
            var quiz = _mapper.Map<Quiz>(quizViewModel);
            // get course by id
            quiz.Course = course;
            _db.Quizzes.Update(quiz);
            _db.SaveChanges();
            return RedirectToAction("Index");
        }

        return View(quizViewModel);
    }

    [HttpGet]
    public IActionResult Delete(int id)
    {
        var quiz = _db.Quizzes.GetByID(id);
        var quizViewModel = _mapper.Map<QuizViewModel>(quiz);

        // get courses related to each quiz
        GetCourses();

        return View(quizViewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ConfirmDelete(int id)
    {
        var quiz = _db.Quizzes.GetByID(id);
        _db.Quizzes.Delete(quiz);
        _db.SaveChanges();

        // get related questions
        foreach (var item in _db.QuizQuestions.Get(q => q.QuizId == id))
        {
            _db.QuizQuestions.Delete(item);
            _db.SaveChanges();

            // get related answers
            foreach (var answer in _db.QuizAnswers.Get(a => a.QuestionId == item.Id))
            {
                _db.QuizAnswers.Delete(answer);
                _db.SaveChanges();
            }
        }

        return RedirectToAction("Index");
    }

    private void GetCourses()
    {
        var courses = _db.Courses.Get();
        ViewData["CourseList"] = _mapper.Map<IEnumerable<CourseViewModel>>(courses);
    }
}
