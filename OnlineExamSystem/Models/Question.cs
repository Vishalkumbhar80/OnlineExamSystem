using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace OnlineExamSystem.Models
{
    public class QuestionOption
    {
        public int Id { get; set; }
        public string OptionText { get; set; }
        public bool IsCorrect { get; set; }
        public int QuestionId { get; set; }
    }

    public class Question
    {
        public int Id { get; set; }
        public int ExamConfigId { get; set; }
        public string Title { get; set; }
        public decimal Marks { get; set; } = 1;
        public bool IsMultipleAnswer { get; set; } = false;
        public List<QuestionOption> Options { get; set; } = new List<QuestionOption>();
        public List<int> SelectedOptionIds { get; set; } = new List<int>();

    }

    public class SelectExamViewModel
    {
        public int SelectedExamId { get; set; }
        public List<SelectListItem> Exams { get; set; } = new List<SelectListItem>();
    }

    public class ExamTakeViewModel
    {
        public ExamConfig Exam { get; set; }
        public List<Question> Questions { get; set; } = new List<Question>();
    }

    public class AnswerSubmission
    {
        public int QuestionId { get; set; }
        public List<int> SelectedOptionIds { get; set; } = new();
    }


    public class ExamResultQuestion
    {
        public string QuestionText { get; set; }
        public string SelectedOption { get; set; }
        public string CorrectOption { get; set; }
        public decimal Marks { get; set; }
        public bool IsCorrect { get; set; }
    }

    public class ExamResultViewModel
    {
        public string ExamTitle { get; set; }
        public decimal TotalMarks { get; set; }
        public decimal ObtainedMarks { get; set; }

        public string Result { get; set; }
        public List<ExamResultQuestion> Questions { get; set; } = new List<ExamResultQuestion>();
    }

    public class UserModel
    {
        public int UserId { get; set; }

        [Required]
        public string FirstName { get; set; }

        public string LastName { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        public List<SelectListItem> GenderList { get; set; } = new List<SelectListItem>();

        public string Gender { get; set; }

        // Use string for phone numbers
        [Required]
        [Phone] // uses built-in phone validation (may be lenient); combine with RegularExpression for stricter rules
        [StringLength(15, ErrorMessage = "Phone number can't be longer than 15 digits.")]
        [RegularExpression(@"^\+?\d{7,15}$", ErrorMessage = "Enter a valid phone number (7–15 digits, optional leading +).")]
        public string PhoneNumber { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }

    public class LoginModel
    {
        public string UserEmail { get; set; }
        public string Password { get; set; }
    }


}
