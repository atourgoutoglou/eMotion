using eMotion.Data;
using eMotion.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using eMotion.Models;
using System.Security.Claims;

namespace eMotion.Controllers
{
    [Authorize(Roles = "Teacher")]
    public class TeacherController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public TeacherController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var allActivities = await _context.StudentActivities
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            var users = await _userManager.Users.ToListAsync();

            var groupedByStudent = allActivities
                .GroupBy(a => a.UserId)
                .ToList();

            var studentRows = new List<TeacherDashboardStudentRowViewModel>();

            foreach (var studentGroup in groupedByStudent)
            {
                var user = users.FirstOrDefault(u => u.Id == studentGroup.Key);

                var groupedByChapter = studentGroup
                    .GroupBy(x => x.ChapterName)
                    .Select(ch => new
                    {
                        ChapterName = ch.Key,
                        BestScore = ch.Max(x => x.Score),
                        LastActivity = ch.OrderByDescending(x => x.CreatedAt).First(),
                        Attempts = ch.Sum(x => x.Attempts > 0 ? x.Attempts : 1)
                    })
                    .ToList();

                var completedChapters = groupedByChapter.Count(x => x.Attempts > 0);
                var progressPercentage = (int)Math.Round((completedChapters / 4.0) * 100);

                var latestActivity = studentGroup
                    .OrderByDescending(x => x.CreatedAt)
                    .FirstOrDefault();

                var bestScoreOverall = studentGroup.Any()
                    ? studentGroup.Max(x => x.Score)
                    : 0;

                studentRows.Add(new TeacherDashboardStudentRowViewModel
                {
                    StudentName = user?.UserName ?? "Άγνωστος χρήστης",
                    CurrentChapter = latestActivity?.ChapterName ?? "-",
                    ProgressPercentage = progressPercentage,
                    BestScoreDisplay = bestScoreOverall > 0 ? $"{bestScoreOverall}/5" : "—",
                    LastEmotion = string.IsNullOrWhiteSpace(latestActivity?.EmotionState) ? "-" : latestActivity!.EmotionState,
                    StudentId = studentGroup.Key,
                    TotalAttempts = studentGroup.Sum(x => x.Attempts > 0 ? x.Attempts : 1)
                });
            }

            var activeStudentsCount = studentRows.Count;

            var averageClassProgress = studentRows.Any()
                ? (int)Math.Round(studentRows.Average(x => x.ProgressPercentage))
                : 0;

            var activeChaptersCount = allActivities
                .Select(x => x.ChapterName)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .Count();

            var vm = new TeacherDashboardViewModel
            {
                ActiveStudentsCount = activeStudentsCount,
                AverageClassProgress = averageClassProgress,
                ActiveChaptersCount = activeChaptersCount,
                Students = studentRows
                    .OrderByDescending(x => x.ProgressPercentage)
                    .ThenBy(x => x.StudentName)
                    .ToList()
            };

            return View(vm);
        }

        [HttpGet]
        public IActionResult SendMessage(string studentId)
        {
            ViewBag.StudentId = studentId;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage(string studentId, string message)
        {
            var teacherId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(studentId) || string.IsNullOrWhiteSpace(message))
                return RedirectToAction("Index");

            var teacherMessage = new TeacherMessage
            {
                StudentId = studentId,
                TeacherId = teacherId ?? "",
                Message = message.Trim(),
                CreatedAt = DateTime.Now
            };

            _context.TeacherMessages.Add(teacherMessage);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> StudentQuestions()
        {
            var questions = await _context.StudentQuestions
                .OrderByDescending(q => q.CreatedAt)
                .ToListAsync();

            return View(questions);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReplyToQuestion(int id, string teacherReply)
        {
            var question = await _context.StudentQuestions.FindAsync(id);

            if (question == null)
            {
                return NotFound();
            }

            question.TeacherReply = teacherReply;
            question.IsAnswered = true;
            question.AnsweredAt = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Η απάντηση στάλθηκε στον μαθητή.";

            return RedirectToAction("StudentQuestions");
        }
    }
}