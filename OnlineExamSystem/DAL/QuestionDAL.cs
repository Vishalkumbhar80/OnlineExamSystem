using System;
using System.Collections.Generic;
using System.Data;
using System.Text.Json;
using OnlineExamSystem.Common;
using OnlineExamSystem.Models;

namespace OnlineExamSystem.DAL
{
    public class QuestionDAL
    {
        // 1) Get all questions for an exam
        public List<Question> GetQuestionsByExamId(int examId)
        {
            var list = new List<Question>();
            var parameters = new Dictionary<string, object> { { "@ExamConfigId", examId } };
            DataTable dt = SqlHelper.ExecuteStoredProcedureSelect("usp_GetQuestionsByExamId", parameters);

            foreach (DataRow row in dt.Rows)
            {
                list.Add(new Question
                {
                    Id = Convert.ToInt32(row["Id"]),
                    ExamConfigId = Convert.ToInt32(row["ExamConfigId"]),
                    Title = row["Title"].ToString(),
                    Marks = row["Marks"] == DBNull.Value ? 1m : Convert.ToDecimal(row["Marks"]),
                    IsMultipleAnswer = row["IsMultipleAnswer"] == DBNull.Value ? false : Convert.ToBoolean(row["IsMultipleAnswer"])
                });
            }
            return list;
        }

        // 2) Get single question by id
        public Question GetQuestionById(int id)
        {
            var parameters = new Dictionary<string, object> { { "@Id", id } };
            DataTable dt = SqlHelper.ExecuteStoredProcedureSelect("usp_GetQuestionById", parameters);
            if (dt.Rows.Count == 0) return null;

            var row = dt.Rows[0];
            return new Question
            {
                Id = Convert.ToInt32(row["Id"]),
                ExamConfigId = Convert.ToInt32(row["ExamConfigId"]),
                Title = row["Title"].ToString(),
                Marks = row["Marks"] == DBNull.Value ? 1m : Convert.ToDecimal(row["Marks"]),
                IsMultipleAnswer = row["IsMultipleAnswer"] == DBNull.Value ? false : Convert.ToBoolean(row["IsMultipleAnswer"])
            };
        }

        // 3) Get all options for a question
        public List<QuestionOption> GetOptionsByQuestionId(int questionId)
        {
            var list = new List<QuestionOption>();
            var parameters = new Dictionary<string, object> { { "@QuestionId", questionId } };
            DataTable dt = SqlHelper.ExecuteStoredProcedureSelect("usp_GetOptionsByQuestionId", parameters);

            foreach (DataRow row in dt.Rows)
            {
                list.Add(new QuestionOption
                {
                    Id = Convert.ToInt32(row["Id"]),
                    OptionText = row["OptionText"].ToString(),
                    IsCorrect = row["IsCorrect"] == DBNull.Value ? false : Convert.ToBoolean(row["IsCorrect"])
                });
            }
            return list;
        }

        // 4) Insert question with options
        public int InsertQuestionWithOptions(Question model)
        {


            var parameters = new Dictionary<string, object>
            {
                {"@ExamConfigId",model.ExamConfigId },
                { "@Title",model.Title  },
                { "@Marks",model.Marks },
                { "@IsMultipleAnswer" ,model.IsMultipleAnswer }
            };
            DataTable dt = SqlHelper.ExecuteStoredProcedureSelect("usp_InsertQuestion", parameters);


            if (dt.Rows.Count > 0 && int.TryParse(dt.Rows[0]["NewQuestionId"].ToString(), out int newId))
            {
                if (model.Options != null && model.Options.Any())
                {
                    foreach (var option in model.Options)
                    {
                        var parameters1 = new Dictionary<string, object>
            {
                {"@QuestionId", newId },
                { "@OptionText",option.OptionText  },
                { "@IsCorrect",option.IsCorrect },

            };
                        var res = SqlHelper.ExecuteStoredProcedure("usp_InsertOptions", parameters1);
                    }

                }

                return newId;

            }

            return 0;
        }

        // 5) Update question with options
        public void UpdateQuestionWithOptions(Question model)
        {
            var payload = new
            {
                id = model.Id,
                examConfigId = model.ExamConfigId,
                title = model.Title,
                marks = model.Marks,
                isMultipleAnswer = model.IsMultipleAnswer,
                options = model.Options?.ConvertAll(o => new { optionText = o.OptionText, isCorrect = o.IsCorrect })
            };

            string json = JsonSerializer.Serialize(payload, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            SqlHelper.ExecuteStoredProcedure("usp_UpdateQuestionWithOptionsJson",
                new Dictionary<string, object> { { "@QuestionJson", json } });
        }

        // 6) Delete question
        public void DeleteQuestion(int id)
        {
            var parameters = new Dictionary<string, object> { { "@Id", id } };
            SqlHelper.ExecuteStoredProcedure("usp_DeleteQuestion", parameters);
        }
    }
}
