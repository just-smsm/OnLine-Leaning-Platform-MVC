using OnlineLearningPlatform.Models;

namespace OnlineLearningPlatform.Repositories.Interface;

public interface ICategoryRepository : IGenericRepository<Category> {
    public Task<Category> GetCategoryByID(int id);
}