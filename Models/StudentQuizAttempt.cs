using OnlineLearningPlatform.Context.Identity;

namespace OnlineLearningPlatform.Models;

public class StudentQuizAttempt {
    public int Id { get; set; }
    public string StudentId { get; set; } = default!;
    public int QuizId { get; set; }
    public DateTime AttemptDatetime { get; set; }
    public int ScoreAchieved { get; set; }
    public ApplicationUser Student { get; set; } = default!;
    public Quiz Quiz { get; set; } = default!;
}