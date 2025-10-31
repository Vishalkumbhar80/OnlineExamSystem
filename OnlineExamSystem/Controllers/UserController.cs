using System.Reflection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using OnlineExamSystem.Common;
using OnlineExamSystem.DAL;
using OnlineExamSystem.Models;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;

namespace OnlineExamSystem.Controllers
{
    public class UserController : Controller
    {

        private readonly SessionManager _sessionManager;
        // Fix for CS1061: Replace the type of `_userDal` from `object` to `UserDAL` to match the expected type.

        private readonly UserDAL _userDal; // Correct type

        
        public UserController(SessionManager sessionManager)
        {

            _userDal = new UserDAL(); // Correct initialization
            _sessionManager = sessionManager;
        }

        [HttpGet]
        public IActionResult SelectExam()
        {
            var vm = new SelectExamViewModel();

            var dal = new ExamConfigDAL();
            var exams = dal.GetExamConfigs(); // returns List<ExamConfig>

            vm.Exams = exams.Select(e => new SelectListItem
            {
                Value = e.Id.ToString(),
                Text = e.Title + (e.ExamStartDate != null ? " (" + e.ExamStartDate?.ToString("yyyy-MM-dd") + ")" : "")
            }).ToList();

            // optional: add a placeholder
            vm.Exams.Insert(0, new SelectListItem { Value = "", Text = "-- Select an exam --", Selected = true });

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SelectExam(SelectExamViewModel model)
        {
            // repopulate the exam list if needed on invalid
            if (!ModelState.IsValid)
            {
                var dal = new ExamConfigDAL();
                var exams = dal.GetExamConfigs();
                model.Exams = exams.Select(e => new SelectListItem
                {
                    Value = e.Id.ToString(),
                    Text = e.Title
                }).ToList();
                model.Exams.Insert(0, new SelectListItem { Value = "", Text = "-- Select an exam --" });
                return View(model);
            }

            return RedirectToAction("Instructions", new { examId = model.SelectedExamId });
        }



        [HttpGet]
        public IActionResult Instructions(int examId)
        {
            if (examId <= 0) return BadRequest();

            var dal = new ExamConfigDAL();
            var exam = dal.GetExamById(examId, _sessionManager.UserId);
            if (exam == null) return NotFound();

            // Optional: check exam availability window
            var now = DateTime.UtcNow; // or DateTime.Now if you're using local time
            if (exam.ExamStartDate.HasValue && now < exam.ExamStartDate.Value)
            {
                ViewBag.ExamNotStarted = true;
            }
            if (exam.ExamEndDate.HasValue && now > exam.ExamEndDate.Value)
            {
                ViewBag.ExamExpired = true;
            }

            return View(exam); // Views/User/Instructions.cshtml
        }

        // POST: /User/StartExam
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult StartExam(int examId)
        {
            if (examId <= 0) return BadRequest();

            var dal = new ExamConfigDAL();
            var exam = dal.GetExamById(examId, _sessionManager.UserId);
            if (exam == null) return NotFound();

            // Optionally re-check exam availability server-side
            var now = DateTime.UtcNow;
            if (exam.ExamStartDate.HasValue && now < exam.ExamStartDate.Value)
                return BadRequest("Exam has not started yet.");
            if (exam.ExamEndDate.HasValue && now > exam.ExamEndDate.Value)
                return BadRequest("Exam has already ended.");

            // OPTIONAL: create an attempt record to track user progress
            // var attemptId = new ExamAttemptDAL().CreateAttempt(UserId, examId);

            // Redirect to your exam runner — update controller/action as required
            return RedirectToAction("TakeExam", "User", new { examId = examId /*, attemptId = attemptId*/ });
        }


        public IActionResult TakeExam(int examId)
        {
            var dal = new UserDAL();
            int userId = 1; // however you get logged-in user
            var model = dal.GetExamTakeData(examId, userId);
            return View(model);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Submit(int examId, List<AnswerSubmission> answers)
        {

            int userId = _sessionManager.UserId;
            var dal = new UserDAL();
            if (answers != null && answers.Count > 0)
            {
                // Save answers and get submissionId
                int submissionId = dal.SaveExamSubmission(examId, userId, answers);
            }
            // Fetch result
            var examResult = dal.GetExamResult(examId, userId);
            ViewBag.IsSubmitted = true;

            // ✅ Generate certificate if passed
            if (examResult.Result == "Pass")
            {
                string certPath = GenerateCertificate(examResult, userId);
                ViewBag.CertificatePath = certPath; // Pass to ThankYou view
            }

            // Pass result to ThankYou view
            return View("ThankYou", examResult);
        }

        // -----------------------------------------------------------------
        // 🔹 Certificate Generator Method
        private string GenerateCertificate(ExamResultViewModel examResult, int userId)
        {
            string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "certificates");
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            string fileName = $"Certificate_User{userId}_Exam{examResult.ExamTitle}.pdf";
            string filePath = Path.Combine(folderPath, fileName);

            using (PdfDocument document = new PdfDocument())
            {
                document.Info.Title = "Certificate of Achievement";
                var page = document.AddPage();
                var gfx = XGraphics.FromPdfPage(page);

                // Background
                gfx.DrawRectangle(XBrushes.White, 0, 0, page.Width, page.Height);

                // Fonts
                var titleFont = new XFont("Times New Roman", 30, XFontStyle.Bold);
                var nameFont = new XFont("Times New Roman", 24, XFontStyle.BoldItalic);
                var normalFont = new XFont("Times New Roman", 16, XFontStyle.Regular);
                var smallFont = new XFont("Times New Roman", 12, XFontStyle.Italic);

                // Title
                gfx.DrawString("Certificate of Achievement", titleFont, XBrushes.DarkBlue,
                    new XRect(0, 80, page.Width, 50), XStringFormats.TopCenter);

                // Subtitle
                gfx.DrawString("This is to certify that", normalFont, XBrushes.Black,
                    new XRect(0, 160, page.Width, 30), XStringFormats.TopCenter);

                // Candidate Name
                gfx.DrawString($"{_sessionManager.FullName}", nameFont, XBrushes.DarkRed,
                    new XRect(0, 200, page.Width, 40), XStringFormats.TopCenter);

                // Exam info
                gfx.DrawString($"has successfully passed the exam \"{examResult.ExamTitle}\"", normalFont, XBrushes.Black,
                    new XRect(0, 260, page.Width, 30), XStringFormats.TopCenter);

                gfx.DrawString($"Score: {examResult.ObtainedMarks} / {examResult.TotalMarks}", normalFont, XBrushes.Black,
                    new XRect(0, 300, page.Width, 30), XStringFormats.TopCenter);

                gfx.DrawString($"Passing Marks: {examResult.PassingMarks}", normalFont, XBrushes.Black,
                    new XRect(0, 330, page.Width, 30), XStringFormats.TopCenter);

                // Footer
                gfx.DrawString("Authorized Signature", smallFont, XBrushes.Black,
                    new XRect(50, page.Height - 100, 200, 20), XStringFormats.TopLeft);

                gfx.DrawString($"Date: {DateTime.Now:dd MMM yyyy}", smallFont, XBrushes.Black,
                    new XRect(page.Width - 250, page.Height - 100, 200, 20), XStringFormats.TopRight);

                document.Save(filePath);
            }

            // Return relative path for UI download link
            return $"/certificates/{fileName}";
        }
        public IActionResult ResultData()
        {
            var dal = new UserDAL();
            var results = dal.GetResultsData(_sessionManager.UserId);


            return View(results);
        }
        public IActionResult DownloadCertificate(int examId)
        {

            var dal = new UserDAL();
            // Fetch result
            var examResult = dal.GetExamResult(examId, _sessionManager.UserId);
            string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "certificates");
            string fileName1 = $"Certificate_User{_sessionManager.UserId}_Exam{examId}.pdf";

            string fileName = $"Certificate_User{_sessionManager.UserId}_Exam{examResult.ExamTitle}.pdf";
            string filePath = Path.Combine(folderPath, fileName);

            if (!System.IO.File.Exists(filePath))
                return NotFound("Certificate not found.");

            var bytes = System.IO.File.ReadAllBytes(filePath);
            return File(bytes, "application/pdf", fileName);
        }

        public IActionResult ThankYou(int examConfigId)
        {
            var dal = new UserDAL();
            ViewBag.IsSubmitted = false;
            var examResult = dal.GetExamResult(examConfigId, _sessionManager.UserId);
            if (examResult.Result == "Pass")
            {
                string certPath = GenerateCertificate(examResult, _sessionManager.UserId);
                ViewBag.CertificatePath = certPath; // Pass to ThankYou view
            }
            return View(examResult);
        }

        public IActionResult EditProfile()
        {
            var dal = new UserDAL();
            var user = dal.GetUserDetailsById(_sessionManager.UserId);
            return View(user);
        }
         public IActionResult UserDetails()
        {
            var dal = new UserDAL();
            var user = dal.GetUserDetailsById(_sessionManager.UserId);
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ProfileEdit(UserModel model)
        {
            //if (!ModelState.IsValid)
            //{
            //    return View(model);
            //}

            try
            {
                _userDal.UpdateUserProfile(model);

                _sessionManager.Data = new SessionData
                {
                    UserId = model.UserId,
                    FullName = $"{model.FirstName} {model.LastName}".Trim(),
                    Email = model.Email,
                    LastLogin = DateTime.UtcNow
                };
                TempData["SuccessMessage"] = "Profile updated successfully!";
                return RedirectToAction("UserDetails", new { id = model.UserId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while updating the profile: " + ex.Message);
                return View("EditProfile", model);
            }
        }
    }
}






    

    


