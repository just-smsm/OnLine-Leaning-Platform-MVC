using OnlineLearningPlatform.Models;

namespace OnlineLearningPlatform.Repositories.Interface;

public interface IEnrollmentRepository : IGenericRepository<Enrollment> {
    public  Task<List<string>> GetUserCoursesByUserID(string userID);
}