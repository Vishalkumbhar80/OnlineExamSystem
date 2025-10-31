using System.Data;
using System.Linq;

using Microsoft.AspNetCore.Mvc;
using OnlineExamSystem.Common;
using OnlineExamSystem.Models;
using static System.Formats.Asn1.AsnWriter;

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
            var passingMarks = Convert.ToInt32(ds.Tables[0].Rows[0]["PassingMarks"]);
            



            var questions = ds.Tables[1].AsEnumerable().Select(static q => new ExamResultQuestion
            {
                QuestionText = q.Field<string>("Title"),
                SelectedOption = q.Field<string>("SelectedOption"),
                CorrectOption = q.Field<string>("CorrectOption"),
                Marks = q.Field<decimal>("Marks"),
                IsCorrect = q.Field<int>("IsCorrect") == 1 ? true : false,
                QuestionStatus = q.Field<string>("QuestionStatus")
            }).ToList();

            return new ExamResultViewModel
            {
                ExamConfigId = examId,
                ExamTitle = examTitle,
                TotalMarks = totalMarks,
                ObtainedMarks = obtainedMarks,
                Questions = questions,
                Result = result,
                PassingMarks = passingMarks,
                

            };
        }

        public List<UserModel> GetAllUsers()
        {
            var dt = SqlHelper.ExecuteStoredProcedureSelect("usp_GetAllUsers");
            var list = new List<UserModel>();

            foreach (DataRow r in dt.Rows)
            {
                list.Add(new UserModel
                {
                    UserId = Convert.ToInt32(r["UserId"]),
                    FirstName = r["FirstName"]?.ToString(),
                    LastName = r["LastName"]?.ToString(),
                    Email = r["Email"]?.ToString(),
                    PhoneNumber = r["PhoneNumber"]?.ToString(),
                    Gender = r["Gender"]?.ToString(),
                    DateOfBirth = r["DateOfBirth"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(r["DateOfBirth"]),
                    IsActive = r["IsActive"] != DBNull.Value && Convert.ToBoolean(r["IsActive"]),
                    Role = r["Role"]?.ToString()

                    // RoleName now holds "Admin,User" or single role depending on SP
                });
            }
            return list;
        }
        public (UserModel user, List<int> roleIds) GetUserById(int userId)
        {
            var ds = SqlHelper.ExecuteDataset("usp_GetUserById", new Dictionary<string, object> { { "@UserId", userId } });

            UserModel user = null;
            var roleIds = new List<int>();

            if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                var r = ds.Tables[0].Rows[0];
                user = new UserModel
                {
                    UserId = Convert.ToInt32(r["UserId"]),
                    FirstName = r["FirstName"].ToString(),
                    LastName = r["LastName"].ToString(),
                    Email = r["Email"].ToString(),
                    PhoneNumber = r["PhoneNumber"]?.ToString(),
                    Gender = r["Gender"]?.ToString(),
                    DateOfBirth = r["DateOfBirth"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(r["DateOfBirth"]),
                    IsActive = r["IsActive"] != DBNull.Value && Convert.ToBoolean(r["IsActive"])
                };
            }

            if (ds.Tables.Count > 1)
            {
                foreach (DataRow rr in ds.Tables[1].Rows)
                {
                    roleIds.Add(Convert.ToInt32(rr["RoleId"]));
                }
            }

            return (user, roleIds);
        }


        public DataTable GetAllRoles()
        {
            return SqlHelper.ExecuteStoredProcedureSelect("usp_GetAllRoles");
        }

        public int CreateUser(UserModel model, int? roleId)
        {
            var param = new Dictionary<string, object>
            {
                {"@Email", model.Email},
                {"@Password", model.Password ?? "Temp@123"}, // hash it in real app
                {"@FirstName", model.FirstName},
                {"@LastName", model.LastName},
                {"@PhoneNumber", model.PhoneNumber},
                {"@Gender", model.Gender},
                {"@DateOfBirth", model.DateOfBirth == DateTime.MinValue ? (object)DBNull.Value : model.DateOfBirth},
                {"@RoleIds",string.Join(",", model.SelectedRoleIds ?? new List<int>()) },
            };

            var result = SqlHelper.ExecuteStoredProcedure("usp_CreateUser", param, outputParamName: null);
            // usp_CreateUser returns NewUserId in result set; since your helper doesn't return SELECT result, you may alter the SP to output; or re-query.
            return 1; // or fetch newly created ID as needed
        }

        public void UpdateUser(UserModel model)
        {
            var param = new Dictionary<string, object>
            {
                {"@UserId", model.UserId},
                {"@Email", model.Email},
                {"@FirstName", model.FirstName},
                {"@LastName", model.LastName},
                {"@PhoneNumber", model.PhoneNumber ?? (object)DBNull.Value},
                {"@Gender", model.Gender ?? (object)DBNull.Value},
                {"@DateOfBirth", model.DateOfBirth == DateTime.MinValue ? (object)DBNull.Value : model.DateOfBirth},
                {"@IsActive", model.IsActive},
                {"@RoleIds",string.Join(",", model.SelectedRoleIds ?? new List<int>()) },
                { "@UpdatedBy", 1 }
            };

            SqlHelper.ExecuteStoredProcedure("usp_UpdateUser", param);
        }

        public void AssignRole(int userId, int roleId)
        {
            var param = new Dictionary<string, object> { { "@UserId", userId }, { "@RoleId", roleId } };
            SqlHelper.ExecuteStoredProcedure("usp_AssignRole", param);
        }

        public void DeleteUser(int userId)
        {
            var param = new Dictionary<string, object> { { "@UserId", userId } };
            SqlHelper.ExecuteStoredProcedure("usp_DeleteUser", param);
        }

        public void ToggleUserStatus(int userId)
        {
            var param = new Dictionary<string, object> { { "@UserId", userId } };
            SqlHelper.ExecuteStoredProcedure("usp_ToggleUserStatus", param);
        }


        public List<ResultModel> GetResultsData(int userId)
        {
            var parameter = new Dictionary<string, object>
            {
                { "UserId", userId },
            };

            DataTable dt = SqlHelper.ExecuteStoredProcedureSelect("usp_GetResultData", parameter);

            var results = new List<ResultModel>();
            foreach (DataRow row in dt.Rows)
            {
                results.Add(new ResultModel()
                {
                    ExamConfigId = Convert.ToInt32(row["ExamConfigId"]),
                    ExamTitle = Convert.ToString(row["ExamTitle"]) ?? "",
                    TotalMarks = row["TotalMarks"] != DBNull.Value ? Convert.ToInt32(row["TotalMarks"]) : 0,
                    ObtainedMarks = row["ObtainedMarks"] != DBNull.Value ? Convert.ToInt32(row["ObtainedMarks"]) : 0,
                    DateExamTaken = Convert.ToDateTime(row["DateExamTaken"]),
                    Percentage = Convert.ToDecimal(row["Percentage"]),
                    Result = Convert.ToString(row["Result"]) ?? "",
                });
            }
            return results;
        }

        public UserModel GetUserDetailsById(int userId)
        {
            var parameter = new Dictionary<string, object>
            {
                { "UserId", userId },
            };

            DataTable dt = SqlHelper.ExecuteStoredProcedureSelect("usp_GetUserById", parameter);
            var users = new UserModel();
            foreach (DataRow row in dt.Rows)
            {
                users = new UserModel()
                {
                    UserId = Convert.ToInt32(row["UserId"]),
                    FirstName = Convert.ToString(row["FirstName"]) ?? "",
                    LastName = Convert.ToString(row["LastName"]) ?? "",
                    Email = Convert.ToString(row["Email"]) ?? "",
                    PhoneNumber = Convert.ToString(row["PhoneNumber"]) ?? "",
                    DateOfBirth = Convert.ToDateTime(row["DateOfBirth"]),
                    Gender = Convert.ToString(row["Gender"]),
                    

                };
            }
            return users;
        }


        public void UpdateUserProfile(UserModel user)
        {
            var param = new Dictionary<string, object>
            {
                {"@UserId", user.UserId},
                {"@Email", user.Email},
                {"@FirstName", user.FirstName},
                {"@LastName", user.LastName},
                {"@PhoneNumber", user.PhoneNumber ?? (object)DBNull.Value},
                {"@Gender", user.Gender ?? (object)DBNull.Value},
                {"@DateOfBirth", user.DateOfBirth ?? (object)DBNull.Value},
                
            };

            SqlHelper.ExecuteStoredProcedure("usp_UpdateUserDetails", param);

        }
    }
}
