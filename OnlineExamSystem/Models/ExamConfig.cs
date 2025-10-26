using System.ComponentModel.DataAnnotations;

namespace OnlineExamSystem.Models
{
  


    public enum ExamType
    {
        [Display(Name = "Online")]
        Online,
        [Display(Name = "Offline")]
        Offline,
        [Display(Name = "Hybrid")]
        Hybrid
    }

    public class ExamSection
    {
        public int Id { get; set; } // EF Core PK
        public string Title { get; set; }
        public int QuestionCount { get; set; }
        public int DurationMinutes { get; set; } // minutes for the section
        public decimal MarksPerQuestion { get; set; }
        public decimal NegativeMarksPerQuestion { get; set; }
    }

    public class ExamConfig
    {
        public int Id { get; set; }

        [Required, StringLength(200)]
        public string Title { get; set; }

        [DataType(DataType.Date)]
        public DateTime? ExamStartDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? ExamEndDate { get; set; }

        [DataType(DataType.Time)]
        public TimeSpan? StartTime { get; set; }

        [Range(1, 10000)]
        public int DurationMinutes { get; set; } // total duration

        [Required]
        public ExamType Type { get; set; }

        [Range(0, 1000)]
        public int TotalQuestions { get; set; }

        [Range(0, 10000)]
        public decimal TotalMarks { get; set; }

        [Range(0, 100)]
        public decimal PassingMarks { get; set; }

        public bool IsNegativeMarking { get; set; }

        // Navigation
        public List<ExamSection> Sections { get; set; } = new List<ExamSection>();

        [DataType(DataType.MultilineText)]
        public string Instructions { get; set; }
        public DateTime CreatedAt { get; internal set; }
        public int? CreatedBy { get; internal set; }
    }
}
