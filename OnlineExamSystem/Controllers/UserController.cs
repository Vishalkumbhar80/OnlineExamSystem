using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using OnlineExamSystem.DAL;
using OnlineExamSystem.Models;

namespace OnlineExamSystem.Controllers
{
    public class UserController : Controller
    {
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
            var exam = dal.GetExamById(examId);
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
            var exam = dal.GetExamById(examId);
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

        //[HttpGet]
        //public IActionResult TakeExam(int examId)
        //{
        //    if (examId <= 0) return BadRequest();

        //    var examDal = new ExamConfigDAL();
        //    var exam = examDal.GetExamById(examId);
        //    if (exam == null) return NotFound();

        //    // Load questions and their options
        //    var qDal = new QuestionDAL();
        //    var questions = qDal.GetQuestionsByExamId(examId);
        //    foreach(var que in questions)
        //    {
        //        que.Options = qDal.GetOptionsByQuestionId(que.Id);
        //    }
        //    // For each question, load options if not included by GetQuestionsByExamId
        //    // (Assume GetQuestionsByExamId loads options; if not, call GetOptionsByQuestionId inside loop)
        //    // Build a simple view model
        //    var vm = new ExamTakeViewModel
        //    {
        //        Exam = exam,
        //        Questions = questions
        //    };

        //    return View(vm);
        //}

        public IActionResult TakeExam(int examId)
        {
            var dal = new UserDAL();
            int userId = 1; // however you get logged-in user
            var model = dal.GetExamTakeData(examId, userId);
            return View(model);
        }

        // POST: /Exam/Submit
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public IActionResult Submit(int examId, string answersJson)
        //{
        //    if (examId <= 0) return BadRequest("Invalid exam id.");
        //    if (string.IsNullOrWhiteSpace(answersJson)) return BadRequest("No answers submitted.");

        //    try
        //    {
        //        // Save submission (DAL should parse JSON or accept structured objects)
        //        var dal = new UserDAL(); // create a DAL to handle exam submissions; implement below or adapt
        //        int userId = 0; // get from auth if you have users: e.g. int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value)
        //        var submissionId = dal.SaveExamSubmission(examId, userId, answersJson);

        //        // Optionally calculate score here or queue scoring
        //        return Json(new { success = true, submissionId = submissionId });
        //    }
        //    catch (Exception ex)
        //    {
        //        // log ex
        //        return StatusCode(500, new { success = false, message = ex.Message });
        //    }
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public IActionResult Submit(int examId, List<AnswerSubmission> answers)
        //{
        //    var dal = new UserDAL();
        //    var result = dal.SaveExamSubmission(examId, 1, answers);
        //    return RedirectToAction("Result", new { examId });
        //}


        private int GetCurrentUserId() => 1; // Replace with real auth logic

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Submit(int examId, List<AnswerSubmission> answers)
        {

            int userId = GetCurrentUserId();
            var dal = new UserDAL();
            if (answers != null && answers.Count > 0)
            {
                // Save answers and get submissionId
                int submissionId = dal.SaveExamSubmission(examId, userId, answers);
            }
            // Fetch result
            var examResult = dal.GetExamResult(examId, userId);

            // Pass result to ThankYou view
            return View("ThankYou", examResult);
        }


    }

}

