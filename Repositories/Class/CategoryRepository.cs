using Microsoft.EntityFrameworkCore;
using OnlineLearningPlatform.Context;
using OnlineLearningPlatform.Models;
using OnlineLearningPlatform.Repositories.Interface;

namespace OnlineLearningPlatform.Repositories.Class;

public class CategoryRepository : GenericRepository<Category>, ICategoryRepository {
    public CategoryRepository(ApplicationDbContext context)
        : base(context) { }

    public async Task<Category> GetCategoryByID(int id)
    {
        return await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
         
    }
}