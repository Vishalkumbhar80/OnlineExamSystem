using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using OnlineExamSystem.Common;
using OnlineExamSystem.DAL;
using OnlineExamSystem.Models;

namespace OnlineExamSystem.Controllers
{
    public class AdminController : Controller
    {
        private readonly UserDAL _userDal = new UserDAL();
        private readonly SessionManager _session;

        public AdminController(SessionManager session)
        {
            _session = session;
        }

        private bool IsAdmin() => _session.Role == "Admin";

        public IActionResult ManageUsers()
        {
            if (!IsAdmin()) return RedirectToAction("Index", "Home");

            var users = _userDal.GetAllUsers();
            return View(users);
        }

        [HttpGet]
        public IActionResult EditUser(int? id)
        {
            if (!IsAdmin()) return RedirectToAction("Index", "Home");

            var rolesDt = _userDal.GetAllRoles();
            var roles = rolesDt.AsEnumerable().Select(r => new SelectListItem
            {
                Value = r["RoleId"].ToString(),
                Text = r["RoleName"].ToString()
            }).ToList();

            if (!id.HasValue)
            {
                // Create new user
                ViewBag.Roles = roles;
                return View(new UserModel());
            }

            var (user, userRoles) = _userDal.GetUserById(id.Value);
            ViewBag.Roles = roles;
            ViewBag.UserRoles = userRoles;
            user.SelectedRoleIds = userRoles;
            return View(user);
        }

        [HttpPost]
        public IActionResult EditUser(UserModel model, int? SelectedRoleId)
        {
            if (!IsAdmin()) return RedirectToAction("Index", "Home");

            if (model.UserId == 0)
            {
                // create
                _userDal.CreateUser(model, SelectedRoleId);
                TempData["SuccessMessage"] = "User created successfully.";
            }
            else
            {
                _userDal.UpdateUser(model);
                if (SelectedRoleId.HasValue)
                {
                    _userDal.AssignRole(model.UserId, SelectedRoleId.Value);
                }
                TempData["SuccessMessage"] = "User updated successfully.";
            }

            return RedirectToAction("ManageUsers");
        }

        [HttpPost]
        public IActionResult DeleteUser(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Index", "Home");

            _userDal.DeleteUser(id);
            TempData["SuccessMessage"] = "User deleted.";
            return RedirectToAction("ManageUsers");
        }

        [HttpPost]
        public IActionResult ToggleUserStatus(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Index", "Home");

            _userDal.ToggleUserStatus(id);
            TempData["SuccessMessage"] = "User status changed.";
            return RedirectToAction("ManageUsers");
        }


    }
}
