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
                { "@Password", user.Password   }
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
                UserId = Convert.ToInt32(row["Id"]),
                FirstName = Convert.ToString(row["FirstName"].ToString()),
                LastName = Convert.ToString(row["LastName"].ToString()),
                Email = Convert.ToString(row["Email"].ToString()),
                DateOfBirth = Convert.ToDateTime(row["DateOfBirth"]),
                Gender =Convert.ToString( row["Gender"].ToString()),

                PhoneNumber = Convert.ToString(row["PhoneNumber"].ToString()),
                Password = Convert.ToString(row["Password"].ToString()),



            };

            

        }   
    }
}
