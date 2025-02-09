using System.ComponentModel.DataAnnotations;

namespace OnlineLearningPlatform.ViewModels
{
    public class GetAllCourseViewModel
    {
        public int Id { get; set; }

        [Required][StringLength(100)] public string Name { get; set; }

        [Required][StringLength(1000)] public string Description { get; set; } 

        [Required] public decimal Price { get; set; } 

        [Required][StringLength(1000)] public string ImageUrl { get; set; } 

        [Required] public bool IsProgressLimited { get; set; }

        public string CategoryName { get; set; }

        public int CategoryId { get; set; }
    }
}
