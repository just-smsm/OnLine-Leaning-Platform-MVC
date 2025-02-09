using OnlineLearningPlatform.Models;

namespace OnlineLearningPlatform.Repositories.Interface;

public interface ICourseRepository : IGenericRepository<Course> { 
    public Task <ICollection<Course>> GetAllAsync ();
    public Task <ICollection <Course>> GetAsyncByCategoryId (int categoryId);
    
}