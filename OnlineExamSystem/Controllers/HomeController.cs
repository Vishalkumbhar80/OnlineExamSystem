using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using OnlineExamSystem.Common;
using OnlineExamSystem.DAL;
using OnlineExamSystem.Models;

namespace OnlineExamSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ExamConfigDAL _repository;
        private object repository;
        private readonly SessionManager _session;





        public HomeController(ILogger<HomeController> logger, SessionManager session)
        {
            _logger = logger;
            _repository = new ExamConfigDAL(); // ✅ Initialize repository

            _session = session;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult Config()
        {
            return View();
        }
        public IActionResult CreateExam()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ExamConfig model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ExamTypes = Enum.GetValues(typeof(ExamType)).Cast<ExamType>();
                return View(model);
            }

            ExamConfigDAL examdd = new ExamConfigDAL();
            examdd.InsertExamConfig(model);

           

            return View("Config");
        }

        [HttpGet]
        public IActionResult GetExamConfigs()
        {
            ExamConfigDAL examdd = new ExamConfigDAL();
            var examConfigs = examdd.GetExamConfigs(); // returns List<ExamConfig>
            return Json(examConfigs);
        }


        public IActionResult EditExam(int id)
        {
            ExamConfig exam = _repository.GetExamById(id, _session.UserId); // ✅ Works now
            if (exam == null)
                return NotFound();

            return View(exam);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditExam(ExamConfig model)
        {
            if (!ModelState.IsValid)
                return View(model);

            _repository.UpdateExam(model); // call repository update method
            return RedirectToAction("Config");
        }

        [HttpPost]
        public IActionResult DeleteExam(int id)
        {
            if (id <= 0) return BadRequest();

            try
            {
                _repository.DeleteExam(id); // implement this in your DAL
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                // log ex
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }



    }
}
