namespace OnlineExamSystem.Models
{
    public class QuestionOption
    {
        public int Id { get; set; }
        public string OptionText { get; set; }
        public bool IsCorrect { get; set; }
    }

    public class Question
    {
        public int Id { get; set; }
        public int ExamConfigId { get; set; }
        public string Title { get; set; }
        public decimal Marks { get; set; } = 1;
        public bool IsMultipleAnswer { get; set; } = false;
        public List<QuestionOption> Options { get; set; } = new List<QuestionOption>();
    }

}
