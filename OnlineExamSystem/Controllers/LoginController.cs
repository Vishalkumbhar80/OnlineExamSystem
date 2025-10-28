using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using OnlineExamSystem.DAL;
using OnlineExamSystem.Models;

namespace OnlineExamSystem.Controllers
{
    public class LoginController : Controller
    {




        public IActionResult UserRagister()
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ExamTypes = Enum.GetValues(typeof(ExamType)).Cast<ExamType>();
                return View();
            }

            LoginDAL logiddd = new LoginDAL();

            UserModel userModel = new UserModel();
            userModel.GenderList = new List<SelectListItem>();
            userModel.GenderList.Insert(0, new SelectListItem { Value = "", Text = "-- Select Gender --", Selected = true });
            userModel.GenderList.Insert(0, new SelectListItem { Value = "Male", Text = "Male", Selected = false });
            userModel.GenderList.Insert(0, new SelectListItem { Value = "Female", Text = "Female", Selected = false });

            //logiddd.InsertUserRagister(userModel);

            return View(userModel);
        }

        [HttpPost]
        public IActionResult CreateRagsiter(UserModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(kvp => kvp.Value.Errors.Count > 0)
                    .Select(kvp => new
                    {
                        Key = kvp.Key,
                        Errors = kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                    });

                // e.g., log or inspect errors in debugger
                foreach (var e in errors)
                {
                    Console.WriteLine($"Field: {e.Key} -> {string.Join(", ", e.Errors)}");
                }

                ViewBag.ExamTypes = Enum.GetValues(typeof(ExamType)).Cast<ExamType>();
                return View(model);
            }

            LoginDAL login1 = new LoginDAL();
            login1.InsertUserRagister(model);



            return View("Login");
        }

        public IActionResult UserLogin()
        {
            return View();
        }
        [HttpPost]
        public IActionResult CreateLogin(LoginModel model)
        {
            LoginDAL loginDAL = new LoginDAL();
            var user = loginDAL.GetUserLogin(model);
            if (user != null)
            {
                // Successful login
                // You can set session or authentication cookie here

                // ✅ Successful login
                // Save complex object (like your user model) to session using JSON serialization
                HttpContext.Session.SetString("UserDetails", JsonConvert.SerializeObject(user));

                TempData["LoginSuccess"] = "Login successful! Welcome back.";
                return RedirectToAction("SelectExam", "User");
            }
            else
            {
                // Invalid credentials
                ViewBag.LoginError = "Invalid email or password. Please try again.";
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
                return View("UserLogin",model);
            }

            //var userJson = HttpContext.Session.GetString("UserDetails");
            //if (userJson != null)
            //{
            //    var user = JsonConvert.DeserializeObject<UserModel>(userJson);
            //    // now you can use user.FirstName, user.Email, etc.
            //}

        }
    }
}
