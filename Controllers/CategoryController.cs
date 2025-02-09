using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlineLearningPlatform.Context.Identity;
using OnlineLearningPlatform.Models;
using OnlineLearningPlatform.Repositories;
using OnlineLearningPlatform.ViewModels;

namespace OnlineLearningPlatform.Controllers;

[Authorize(Roles = "Admin")]
public class CategoryController : BaseController
{
    private readonly IUnitOfWork _db;
    private readonly IMapper _mapper;

    public CategoryController(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        UserManager<ApplicationUser> userManager
    )
        : base(userManager)
    {
        _db = unitOfWork;
        _mapper = mapper;
    }

    // GET: Category
    // GET: Category
    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        // Fetch categories and map them to ViewModels
        var categories =  _db.Categories.Get(); // Ensure this is async
        var categoryViewModels = _mapper.Map<IEnumerable<CategoryViewModel>>(categories);

        // Fetch courses and pass them to the view through ViewData
        var courses =  _db.Courses.Get(); // Ensure this is async
        var courseViewModels = _mapper.Map<IEnumerable<CourseViewModel>>(courses);

        ViewData["CoursesList"] = courseViewModels;

        return View(categoryViewModels);
    }


    
    // GET: Category/Details/5
    public IActionResult Details(int? id)
    {
        if (id == null)
            return NotFound();

        var category = _db.Categories.GetByID(id);
        if (category == null)
            return NotFound();

        var categoryViewModel = _mapper.Map<CategoryViewModel>(category);
        return View(categoryViewModel);
    }

    // GET: Category/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Category/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(CategoryViewModel categoryViewModel)
    {
        if (ModelState.IsValid)
        {
            var category = _mapper.Map<Category>(categoryViewModel);
            _db.Categories.Insert(category);
            _db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        return View(categoryViewModel);
    }

    // GET: Category/Edit/5
    public IActionResult Edit(int? id)
    {
        if (id == null)
            return NotFound();

        var category = _db.Categories.GetByID(id);
        if (category == null)
            return NotFound();

        var categoryViewModel = _mapper.Map<CategoryViewModel>(category);
        return View(categoryViewModel);
    }

    // POST: Category/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, CategoryViewModel categoryViewModel)
    {
        if (id != categoryViewModel.Id)
            return NotFound();

        if (ModelState.IsValid)
        {
            var category = _mapper.Map<Category>(categoryViewModel);
            _db.Categories.Update(category);
            _db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        return View(categoryViewModel);
    }

    // GET: Category/Delete/5
    public IActionResult Delete(int? id)
    {
        if (id == null)
            return NotFound();

        var category = _db.Categories.GetByID(id);
        if (category == null)
            return NotFound();

        var categoryViewModel = _mapper.Map<CategoryViewModel>(category);
        return View(categoryViewModel);
    }

    // POST: Category/Delete/5
    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int id)
    {
        var category = _db.Categories.GetByID(id);
        _db.Categories.Delete(category);
        _db.SaveChanges();
        return RedirectToAction(nameof(Index));
    }
}
