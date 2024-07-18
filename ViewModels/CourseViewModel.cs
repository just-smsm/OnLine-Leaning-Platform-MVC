using OnlineLearningPlatform.Models;

namespace OnlineLearningPlatform.ViewModels;

public class CourseViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; } = 0;
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsProgressLimited { get; set; } = false;
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
}
