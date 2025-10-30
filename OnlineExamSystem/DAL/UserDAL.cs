using System.Data;
using Microsoft.AspNetCore.Mvc;
using OnlineExamSystem.Common;
using OnlineExamSystem.Models;

namespace OnlineExamSystem.DAL
{
    public class UserDAL
    {
        public int SaveExamSubmission(int examId, int userId)
        {

            var parameters = new Dictionary<string, object>
            {
                { "@ExamId", examId },
                { "@UserId", userId },
                //{ "@AnswersJson", answersJson }
            };
            var dt = SqlHelper.ExecuteStoredProcedure("usp_SaveExamSubmission", parameters);

            return 0;


        }

        public int SaveExamSubmission(int examId, int userId, List<AnswerSubmission> answers)
        {
            int rowsAffected = 0;

            foreach (var answer in answers)
            {
                foreach (var optionId in answer.SelectedOptionIds)
                {
                    var parameters = new Dictionary<string, object>
            {
                { "@ExamId", examId },
                { "@UserId", userId },
                { "@QuestionId", answer.QuestionId },
                { "@OptionId", optionId }
            };

                    // You can reuse your helper but call a new stored procedure that inserts a single record
                    SqlHelper.ExecuteStoredProcedure("usp_SaveExamAnswer", parameters);
                    rowsAffected++;
                }
            }

            return rowsAffected;
        }


        public ExamTakeViewModel GetExamTakeData(int examId, int userId)
        {

            var parameters = new Dictionary<string, object>
            {
                { "@ExamId", examId },
                { "@UserId", userId },
                //{ "@AnswersJson", answersJson }
            };
            var ds = SqlHelper.ExecuteDataset("usp_GetExamTakeData", parameters);

            var exam = ds.Tables[0].AsEnumerable().Select(r => new ExamConfig
            {
                Id = r.Field<int>("Id"),
                Title = r.Field<string>("Title"),
                DurationMinutes = r.Field<int>("DurationMinutes"),
                Instructions = r.Field<string>("Instructions")
            }).FirstOrDefault();

            var questions = ds.Tables[1].AsEnumerable().Select(q => new Question
            {
                Id = q.Field<int>("Id"),
                ExamConfigId = q.Field<int>("ExamConfigId"),
                Title = q.Field<string>("Title"),
                Marks = q.Field<decimal>("Marks"),
                IsMultipleAnswer = q.Field<bool>("IsMultipleAnswer"),
                Options = new List<QuestionOption>()
            }).ToList();

            var options = ds.Tables[2].AsEnumerable().Select(o => new QuestionOption
            {
                Id = o.Field<int>("Id"),
                OptionText = o.Field<string>("OptionText"),
                IsCorrect = o.Field<bool>("IsCorrect"),
                QuestionId = o.Field<int>("QuestionId")
            }).ToList();

            var answers = ds.Tables[3].AsEnumerable().Select(a => new
            {
                QuestionId = a.Field<int>("QuestionId"),
                OptionId = a.Field<int>("OptionId")
            }).ToList();

            // attach options
            foreach (var q in questions)
                q.Options = options.Where(o => o.QuestionId == q.Id).ToList();

            // attach existing user answers
            foreach (var q in questions)
            {
                q.SelectedOptionIds = answers
                    .Where(a => a.QuestionId == q.Id)
                    .Select(a => a.OptionId)
                    .ToList();
            }

            return new ExamTakeViewModel
            {
                Exam = exam,
                Questions = questions
            };
        }

        public ExamResultViewModel GetExamResult(int examId, int userId)
        {
            var parameters = new Dictionary<string, object>
            {
                { "@ExamId", examId },
                { "@UserId", userId },
                //{ "@AnswersJson", answersJson }
            };
            var ds = SqlHelper.ExecuteDataset("usp_GetExamResult", parameters);

            var examTitle = ds.Tables[0].Rows[0]["ExamTitle"].ToString();
            var totalMarks = Convert.ToDecimal(ds.Tables[0].Rows[0]["TotalMarks"]);
            var obtainedMarks = Convert.ToDecimal(ds.Tables[0].Rows[0]["ObtainedMarks"]);
            var result = Convert.ToString(ds.Tables[0].Rows[0]["Result"]);

            var questions = ds.Tables[1].AsEnumerable().Select(static q => new ExamResultQuestion
            {
                QuestionText = q.Field<string>("Title"),
                SelectedOption = q.Field<string>("SelectedOption"),
                CorrectOption = q.Field<string>("CorrectOption"),
                Marks = q.Field<decimal>("Marks"),

                //IsCorrect = q.Field<bool>("IsCorrect")
            }).ToList();

            return new ExamResultViewModel
            {
                ExamTitle = examTitle,
                TotalMarks = totalMarks,
                ObtainedMarks = obtainedMarks,
                Questions = questions,
                Result =result
            };
        }




    }
}
