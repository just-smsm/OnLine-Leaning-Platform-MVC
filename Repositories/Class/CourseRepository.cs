using Microsoft.EntityFrameworkCore;
using OnlineLearningPlatform.Context;
using OnlineLearningPlatform.Models;
using OnlineLearningPlatform.Repositories.Interface;

namespace OnlineLearningPlatform.Repositories.Class;

public class CourseRepository : GenericRepository<Course>, ICourseRepository {
    public CourseRepository(ApplicationDbContext context)
        : base(context) { }

    public async Task<ICollection<Course>> GetAllAsync()
    {
        return _context.Courses.Include(c=>c.Category).ToList();
    }

    public async Task<ICollection<Course>> GetAsyncByCategoryId(int categoryId)
    {
        return await _context.Courses
            .Where(c => c.CategoryId == categoryId)
            .ToListAsync(); // Use ToListAsync for asynchronous execution
    }

}