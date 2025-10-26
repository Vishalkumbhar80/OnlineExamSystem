using System.Data;
using OnlineExamSystem.Common;
using OnlineExamSystem.Models;

namespace OnlineExamSystem.DAL
{
    public class ExamConfigDAL
    {
        public void InsertExamConfig(ExamConfig model)
        {
            var parameters = new Dictionary<string, object>
{
    { "@Title", model.Title },
    { "@ExamStartDate", model.ExamStartDate },
    { "@ExamEndDate", model.ExamEndDate },
    { "@DurationMinutes", model.DurationMinutes },
    { "@TotalQuestions", model.TotalQuestions },
    { "@TotalMarks", model.TotalMarks },
    { "@PassingMarks", model.PassingMarks },
    { "@IsNegativeMarking", model.IsNegativeMarking },
    { "@Instructions", model.Instructions },

};

            object newUserId = SqlHelper.ExecuteStoredProcedure(
                storedProcedureName: "usp_InsertExamConfig",
                parameters: parameters,
                outputParamName: ""
            );
        }

        public List<ExamConfig> GetExamConfigs()
        {
            // Call your synchronous SQL helper
            DataTable dt = SqlHelper.ExecuteStoredProcedureSelect("usp_GetExamConfigs");

            var list = new List<ExamConfig>();

            foreach (DataRow row in dt.Rows)
            {
                list.Add(new ExamConfig
                {
                    Id = Convert.ToInt32(row["Id"]),
                    Title = row["Title"].ToString(),
                    ExamStartDate = row["ExamStartDate"] as DateTime?,
                    ExamEndDate = row["ExamEndDate"] as DateTime?,
                    DurationMinutes = Convert.ToInt32(row["DurationMinutes"]),
                    TotalQuestions = Convert.ToInt32(row["TotalQuestions"]),
                    TotalMarks = Convert.ToDecimal(row["TotalMarks"]),
                    PassingMarks = Convert.ToDecimal(row["PassingMarks"]),
                    IsNegativeMarking = Convert.ToBoolean(row["IsNegativeMarking"]),
                    Instructions = row["Instructions"].ToString(),
                    CreatedAt = Convert.ToDateTime(row["CreatedAt"]),
                    CreatedBy = row["CreatedBy"] as int?
                });
            }

            return list;
        }

        public ExamConfig GetExamById(int id)
        {
            var parameters = new Dictionary<string, object> { { "@Id", id } };
            DataTable dt = SqlHelper.ExecuteStoredProcedureSelect("usp_GetExamById", parameters);

            if (dt.Rows.Count == 0) return null;

            DataRow row = dt.Rows[0];
            return new ExamConfig
            {
                Id = Convert.ToInt32(row["Id"]),
                Title = row["Title"].ToString(),
                ExamStartDate = row["ExamStartDate"] as DateTime?,
                ExamEndDate = row["ExamEndDate"] as DateTime?,
                DurationMinutes = Convert.ToInt32(row["DurationMinutes"]),
                TotalQuestions = Convert.ToInt32(row["TotalQuestions"]),
                TotalMarks = Convert.ToDecimal(row["TotalMarks"]),
                PassingMarks = Convert.ToDecimal(row["PassingMarks"]),
                IsNegativeMarking = Convert.ToBoolean(row["IsNegativeMarking"]),
                Instructions = row["Instructions"].ToString(),
                CreatedAt = Convert.ToDateTime(row["CreatedAt"]),
                CreatedBy = row["CreatedBy"] as int?
            };
        }


        public void UpdateExam(ExamConfig model)
        {
            var parameters = new Dictionary<string, object>
    {
        { "@Id", model.Id },
        { "@Title", model.Title },
        { "@ExamStartDate", model.ExamStartDate },
        { "@ExamEndDate", model.ExamEndDate },
        { "@DurationMinutes", model.DurationMinutes },
        { "@TotalQuestions", model.TotalQuestions },
        { "@TotalMarks", model.TotalMarks },
        { "@PassingMarks", model.PassingMarks },
        { "@IsNegativeMarking", model.IsNegativeMarking },
        { "@Instructions", model.Instructions }
    };

            SqlHelper.ExecuteStoredProcedure("usp_UpdateExamConfig", parameters);
        }

        public void DeleteExam(int id)
        {
            var parameters = new Dictionary<string, object> { { "@Id", id } };
            SqlHelper.ExecuteStoredProcedure("usp_DeleteExam", parameters);
        }



    }
}

