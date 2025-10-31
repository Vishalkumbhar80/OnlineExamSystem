using System.Reflection;
using OnlineExamSystem.Common;
using OnlineExamSystem.Models;
using System.Data;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace OnlineExamSystem.DAL
{
    public class LoginDAL
    {
       

        public void InsertUserRagister(UserModel user)
        {
            var parameters = new Dictionary<string, object>
            {
                { "@FirstName", user.FirstName},
                { "@LastName", user.LastName},
                { "@Email", user.Email},
                { "@DateOfBirth", user.DateOfBirth },
                { "@Gender", user.Gender },
                { "@PhoneNumber",user.PhoneNumber },
                { "@Password", user.Password   },
               { "@IsActive",user.IsActive }
            };



            object newUserId = SqlHelper.ExecuteStoredProcedure(
               storedProcedureName: "usp_InsertUserDetails",
               parameters: parameters
           );

        }

        public void InsertUserLogin(UserModel user)
        {
            var parameters = new Dictionary<string, object>
            {
                { "@Email", user.Email},
                { "@Password", user.Password   }
            };
        }

        public UserModel GetUserLogin(LoginModel user)
        {
            var parameters = new Dictionary<string, object>
            {
                { "@Email", user.UserEmail},
                { "@Password", user.Password   }
            };
            DataTable dt = SqlHelper.ExecuteStoredProcedureSelect(
               storedProcedureName: "usp_GetUserLogin",
               parameters: parameters);

            if (dt.Rows.Count == 0) return null;

            DataRow row = dt.Rows[0];
            return new UserModel
            {
                UserId = row["Id"] != DBNull.Value ? Convert.ToInt32(row["Id"]) : 0,
                FirstName = row["FirstName"] != DBNull.Value ? row["FirstName"].ToString() : string.Empty,
                LastName = row["LastName"] != DBNull.Value ? row["LastName"].ToString() : string.Empty,
                Email = row["Email"] != DBNull.Value ? row["Email"].ToString() : string.Empty,
                DateOfBirth = row["DateOfBirth"] != DBNull.Value ? Convert.ToDateTime(row["DateOfBirth"]) : (DateTime?)null,
                Gender = row["Gender"] != DBNull.Value ? row["Gender"].ToString() : string.Empty,
                PhoneNumber = row["PhoneNumber"] != DBNull.Value ? row["PhoneNumber"].ToString() : string.Empty,
                Password = row["Password"] != DBNull.Value ? row["Password"].ToString() : string.Empty
            };

        }
        public List<RoleModel> GetUserRole(int UserId)
        {
            var parameters = new Dictionary<string, object>
            {
                { "@UserId", UserId},
            };
            DataTable dt = SqlHelper.ExecuteStoredProcedureSelect(
               storedProcedureName: "usp_GetUserRole",
               parameters: parameters);

            if (dt.Rows.Count == 0) return null;

            var list = new List<RoleModel>();

            foreach (DataRow row in dt.Rows)
            {
                list.Add(new RoleModel
                {
                    RoleId = Convert.ToInt32(row["RoleId"]),
                    RoleName = Convert.ToString(row["RoleName"].ToString()),
                });
            }

            return list;
        }
    }
}
