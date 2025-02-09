using OnlineLearningPlatform.Models;

namespace OnlineLearningPlatform.Repositories.Interface;

public interface IStudentQuizAttemptRepository : IGenericRepository<StudentQuizAttempt> {
    public  Task<StudentQuizAttempt?> GetBYIncrementId(string StudentId, int QuizId, int Id);
    public Task<ICollection<object>> GetStudentQuizzesWithScores(string studentId);
}