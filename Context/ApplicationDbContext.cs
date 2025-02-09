using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OnlineLearningPlatform.Context.Identity;
using OnlineLearningPlatform.Models;
using OnlineLearningPlatform.ModelsConfiguration;
using System.Reflection.Emit;

namespace OnlineLearningPlatform.Context;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser> {
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public ApplicationDbContext() { }

    public DbSet<Course> Courses { get; set; }
    public DbSet<Category> Categories  { get; set; }
    public DbSet<Module> Modules { get; set; }
    public DbSet<Lesson> Lessons { get; set; }
    public DbSet<Quiz> Quizzes { get; set; }
    public DbSet<QuizQuestion> QuizQuestions { get; set; }
    public DbSet<QuizAnswer> QuizAnswers { get; set; }
    public DbSet<StudentQuizAttempt> StudentQuizAttempts { get; set; }
    public DbSet<Enrollment> Enrollments { get; set; }
    public DbSet<StudentLesson> StudentLessons { get; set; }
    public DbSet<Payment> Payments { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
        base.OnConfiguring(optionsBuilder);

        var connectionString = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build()
            .GetConnectionString("DefaultConnection");

        optionsBuilder.UseSqlServer(connectionString);
    }

    protected override void OnModelCreating(ModelBuilder builder) {
        base.OnModelCreating(builder);

        builder.ApplyConfiguration(new CourseConfiguration());
        builder.ApplyConfiguration(new ModuleConfiguration());
        builder.ApplyConfiguration(new LessonConfiguration());
        builder.ApplyConfiguration(new QuizConfiguration());
        builder.ApplyConfiguration(new QuizQuestionConfiguration());
        builder.ApplyConfiguration(new QuizAnswerConfiguration());
        builder.ApplyConfiguration(new StudentQuizAttemptConfiguration());
        builder.ApplyConfiguration(new EnrollmentConfiguration());
        builder.ApplyConfiguration(new StudentLessonConfiguration());

        // convert the profile picture to base64 string
        builder
            .Entity<ApplicationUser>()
            .Property(e => e.ProfilePictureUrl)
            .HasConversion(
                v => v == null ? null : Convert.ToBase64String(v),
                v => v == null ? null : Convert.FromBase64String(v)
            );
        builder.Entity<StudentQuizAttempt>(entity =>
        {
            
            entity.Property(e => e.Id)
                  .ValueGeneratedOnAdd(); // Configure auto-increment (identity)
        });
    }
}