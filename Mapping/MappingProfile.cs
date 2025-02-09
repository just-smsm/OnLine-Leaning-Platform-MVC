using AutoMapper;
using OnlineLearningPlatform.Models;
using OnlineLearningPlatform.ViewModels;

namespace OnlineLearningPlatform.Mapping;

public class MappingProfile : Profile {
    public MappingProfile() {
        CreateMap<Category, CategoryViewModel>().ReverseMap();
        CreateMap<Course, CourseViewModel>().ReverseMap();
        CreateMap<Enrollment, EnrollmentViewModel>().ReverseMap();
        CreateMap<Lesson, LessonViewModel>().ReverseMap();
        CreateMap<Module, ModuleViewModel>().ReverseMap();
        CreateMap<Quiz, QuizViewModel>().ReverseMap();
        CreateMap<QuizAnswer, QuizAnswerViewModel>().ReverseMap();
        CreateMap<QuizQuestion, QuizQuestionViewModel>().ReverseMap();
        CreateMap<StudentLesson, StudentLessonViewModel>().ReverseMap();
        CreateMap<StudentQuizAttempt, StudentQuizAttemptViewModel>().ReverseMap();
        
    }
}