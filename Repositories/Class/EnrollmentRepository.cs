using Microsoft.EntityFrameworkCore;
using OnlineLearningPlatform.Context;
using OnlineLearningPlatform.Models;
using OnlineLearningPlatform.Repositories.Interface;

namespace OnlineLearningPlatform.Repositories.Class;

public class EnrollmentRepository : GenericRepository<Enrollment>, IEnrollmentRepository {
    public EnrollmentRepository(ApplicationDbContext context)
        : base(context) { 
    }

    public async Task<List<string>> GetUserCoursesByUserID(string userID)
    {
        // Retrieve enrollments with course details where the StudentId matches
        var courses = await _context.Enrollments
            .Include(e => e.Course) // Include the related Course entity
            .Where(e => e.StudentId == userID)
            .Select(e => e.Course.Name) // Select only the course name
            .ToListAsync();

        return courses;
    }

}