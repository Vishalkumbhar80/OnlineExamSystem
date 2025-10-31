using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using OnlineExamSystem.Common;
using OnlineExamSystem.DAL;
using OnlineExamSystem.Models;

namespace OnlineExamSystem.Controllers
{
    public class LoginController : Controller
    {

        private readonly LoginDAL _loginDal = new LoginDAL();
        private readonly SessionManager _sessionManager;

        public LoginController(SessionManager sessionManager)
        {
            _sessionManager = sessionManager;
        }


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
                return View("UserRagister",model);
            }

            LoginDAL login1 = new LoginDAL();
            login1.InsertUserRagister(model);



            return View("UserLogin");
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

                // map to SessionData and store

                user.RoleList = loginDAL.GetUserRole(user.UserId);

                _sessionManager.Data = new SessionData
                {
                    UserId = user.UserId,
                    FullName = $"{user.FirstName} {user.LastName}".Trim(),
                    Email = user.Email,
                    LastLogin = DateTime.UtcNow,
                    UserRoles = user.RoleList
                };

                // Successful login
                // You can set session or authentication cookie here

                // ✅ Successful login
                // Save complex object (like your user model) to session using JSON serialization
                HttpContext.Session.SetString("UserDetails", JsonConvert.SerializeObject(user));


                if(user.RoleList != null)
                {
                    // Set default role in session (e.g., first role)
                    _sessionManager.Role = user.RoleList.First().RoleName;
                    HttpContext.Session.SetString("UserRole", user.RoleList.First().RoleName);
                }
                else
                {
                    _sessionManager.Role = "User";
                    HttpContext.Session.SetString("UserRole", "User"); // default role
                }

                    //TempData["LoginSuccess"] = "Login successful! Welcome back.";
                return RedirectToAction("Index", "Home");
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

        public IActionResult SwitchRole(string role)
        {
            if (!string.IsNullOrEmpty(role))
            {
                // Store the selected role in session
                _sessionManager.Role = role;
                //HttpContext.Session.SetString("UserRole", role);
            }

            // Redirect back to home (or wherever you want)
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Logout()
        {
            // Clear all session data
            HttpContext.Session.Clear();

            // Optionally clear authentication cookie if you use it
            // await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            TempData["LogoutMessage"] = "You have been successfully logged out.";

            // Redirect to login page
            return RedirectToAction("UserLogin", "Login");
        }
    }
}
