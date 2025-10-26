using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using OnlineExamSystem.DAL;
using OnlineExamSystem.Models;

namespace OnlineExamSystem.Controllers
{
    public class QuestionController : Controller
    {
        private object model;

        public IActionResult ConfigQuestion(int examId)
        {
            // Get all questions for this exam
            QuestionDAL dal = new QuestionDAL();
            var questions = dal.GetQuestionsByExamId(examId);

            ViewBag.ExamId = examId;
            return View(questions);
        }

        public IActionResult CreateQuestions(int examid)
        {
            var model = new Question { ExamConfigId = examid };
            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateQuestions(Question model)
        {
            // server-side validation
            if (string.IsNullOrWhiteSpace(model.Title))
                ModelState.AddModelError(nameof(model.Title), "Title is required.");

            if (model.Options == null || model.Options.Count == 0)
                ModelState.AddModelError("", "Please add at least one option.");

            if (!ModelState.IsValid)
                return View(model);

            if (!model.Options.Exists(o => o.IsCorrect))
            {
                ModelState.AddModelError("", "Please mark at least one option as correct.");
                return View(model);
            }

            // ensure option texts non-empty
            for (int i = 0; i < model.Options.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(model.Options[i].OptionText))
                    ModelState.AddModelError($"Options[{i}].OptionText", "Option text required.");
            }
            if (!ModelState.IsValid) return View(model);

            var dal = new QuestionDAL();
            int newId = dal.InsertQuestionWithOptions(model);

            var questions = dal.GetQuestionsByExamId(model.ExamConfigId);

            ViewBag.ExamId = model.ExamConfigId;
            return RedirectToAction("ConfigQuestion", new { examId = model.ExamConfigId });
        }

        [HttpPost]
        public IActionResult DeleteQuestion(int id)
        {
            if (id <= 0) return BadRequest();

            try
            {
                var dal = new QuestionDAL();
                dal.DeleteQuestion(id);
                return Json(new { success = true, message = "Question deleted" });
            }
            catch (Exception ex)
            {
                // log ex
                return StatusCode(500, new { success = false, message = ex.Message });
            }


        }

        // GET: Edit question
        [HttpGet]
        public IActionResult EditQuestion(int id)
        {
            var dal = new QuestionDAL();
            var question = dal.GetQuestionById(id);
            if (question == null) return NotFound();

            // Correct call to get options
            question.Options = dal.GetOptionsByQuestionId(id);

            return View(question);
        }

        // POST: Update question
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditQuestionDetail(Question model)
        {
            if (string.IsNullOrWhiteSpace(model.Title))
                ModelState.AddModelError(nameof(model.Title), "Title is required.");

            if (model.Options == null || model.Options.Count == 0)
                ModelState.AddModelError("", "Please add at least one option.");

            if (!model.IsMultipleAnswer && model.Options.Count(o => o.IsCorrect) > 1)
                ModelState.AddModelError("", "Multiple correct answers selected, but question is single-answer type.");

            for (int i = 0; i < (model.Options?.Count ?? 0); i++)
            {
                if (string.IsNullOrWhiteSpace(model.Options[i].OptionText))
                    ModelState.AddModelError($"Options[{i}].OptionText", "Option text is required.");
            }

            if (!ModelState.IsValid)
            {
                // reload options if missing
                var dal = new QuestionDAL();
                if (model.Options == null || !model.Options.Any())
                    model.Options = dal.GetOptionsByQuestionId(model.Id);

                return View(model);
            }

            try
            {
                var dal = new QuestionDAL();
                dal.UpdateQuestionWithOptions(model);
                return RedirectToAction("ConfigQuestion", new { examId = model.ExamConfigId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Update failed: " + ex.Message);
                return View(model);
            }
        }
    }

}

