using Microsoft.EntityFrameworkCore;
using OnlineLearningPlatform.Context;
using OnlineLearningPlatform.Models;
using OnlineLearningPlatform.Repositories.Interface;

namespace OnlineLearningPlatform.Repositories.Class;

public class StudentQuizAttemptRepository
    : GenericRepository<StudentQuizAttempt>,
        IStudentQuizAttemptRepository
{
    public StudentQuizAttemptRepository(ApplicationDbContext context)
        : base(context) { }

    public async Task<StudentQuizAttempt?> GetBYIncrementId(string StudentId, int QuizId, int Id)
    {
        return await _context.StudentQuizAttempts
            .FirstOrDefaultAsync(s => s.StudentId == StudentId && s.QuizId == QuizId && s.Id == Id);
    }

    public async Task<ICollection<object>> GetStudentQuizzesWithScores(string studentId)
    {
        var result = await _context.StudentQuizAttempts
            .Where(q => q.StudentId == studentId)
            .Select(q => new
            {
                QuizId = q.QuizId,
                QuizName = q.Quiz.Name, // Assuming there's a navigation property `Quiz` with a `Name`
                ScoreAchieved = q.ScoreAchieved,
                AttemptDate = q.AttemptDatetime
            })
            .ToListAsync();

        return result.Cast<object>().ToList(); // Cast to ICollection<object> if needed
    }


}
