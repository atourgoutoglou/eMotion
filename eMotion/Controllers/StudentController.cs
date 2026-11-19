using eMotion.Data;
using eMotion.Models;
using eMotion.Models.ViewModels;
using eMotion.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace eMotion.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmotionEngineService _emotionEngine;
        private readonly UserManager<IdentityUser> _userManager;

        private static readonly List<string> AllChapters = new()
        {
            "Chapter1",
            "Chapter2",
            "Chapter3",
            "Chapter4"
        };

        public StudentController(ApplicationDbContext context, IEmotionEngineService emotionEngine, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _emotionEngine = emotionEngine;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return View(new StudentProgressViewModel());
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return View(new StudentProgressViewModel());
            }

            var model = await BuildStudentProgressViewModelAsync(userId);

            var teacherMessages = await _context.TeacherMessages
                .Where(m => m.StudentId == userId)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();

            ViewBag.TeacherMessages = teacherMessages;

            var loginActivity = await _context.StudentLoginActivities
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (loginActivity == null)
            {
                loginActivity = new StudentLoginActivity
                {
                    UserId = userId,
                    LastLoginAt = DateTime.Now
                };

                _context.StudentLoginActivities.Add(loginActivity);
            }
            else
            {
                loginActivity.LastLoginAt = DateTime.Now;
            }

            await _context.SaveChangesAsync();

            return View(model);
        }

        public IActionResult Chapter1() => View();
        public IActionResult Chapter2() => View();
        public IActionResult Chapter3() => View();
        public IActionResult Chapter4() => View();

        [HttpPost]
        public async Task<IActionResult> SaveActivity([FromBody] StudentActivity activity)
        {
            if (activity == null)
                return BadRequest();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            activity.UserId = userId;
            activity.CreatedAt = DateTime.Now;

            var analysis = _emotionEngine.Analyze(activity);
            activity.EmotionState = analysis.Emotion;

            if (string.IsNullOrWhiteSpace(activity.ActivityType))
            {
                activity.ActivityType = "Unknown";
            }

            _context.StudentActivities.Add(activity);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                emotion = analysis.Emotion,
                title = analysis.Title,
                message = analysis.Message,
                confidence = analysis.Confidence,
                difficultyLevel = analysis.DifficultyLevel,
                supportAction = analysis.SupportAction,
                uiMode = analysis.UiMode,
                reasons = analysis.Reasons,
                recommendations = analysis.Recommendations
            });
        }

        public async Task<IActionResult> Progress()
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return View(new StudentProgressViewModel());
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return View(new StudentProgressViewModel());
            }

            var activities = await _context.StudentActivities
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            var model = BuildStudentProgressViewModel(activities);

            var emotionDistribution = activities
                .GroupBy(a =>
                    string.IsNullOrWhiteSpace(a.EmotionState)
                    ? "Neutral"
                    : a.EmotionState.Trim())
                .Select(g => new
                {
                    Emotion = g.Key,
                    Count = g.Count()
                })
                .ToList();

            ViewBag.EmotionDistribution = emotionDistribution;

            return View(model);
        }

        private async Task<StudentProgressViewModel> BuildStudentProgressViewModelAsync(string userId)
        {
            var activities = await _context.StudentActivities
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            return BuildStudentProgressViewModel(activities);
        }


        private StudentProgressViewModel BuildStudentProgressViewModel(List<StudentActivity> activities)
        {
            var chapterProgressList = AllChapters
                .Select(chapter => BuildChapterProgress(activities, chapter))
                .ToList();

            int overallCompletion = chapterProgressList.Any()
                ? (int)Math.Round(chapterProgressList.Average(c => c.CompletionPercentage))
                : 0;

            var recentGrades = activities
                .Take(5)
                .Select(a => new RecentGradeViewModel
                {
                    ChapterName = a.ChapterName,
                    Score = a.Score,
                    CreatedAt = a.CreatedAt
                })
                .ToList();

            return new StudentProgressViewModel
            {
                OverallCompletionPercentage = overallCompletion,

                Chapters = chapterProgressList,

                RecentGrades = recentGrades,

                HasCompletedAllChapters =
                    chapterProgressList.All(c => c.Completed),

                CompletedChaptersCount =
                    chapterProgressList.Count(c => c.Completed),

                TotalChaptersCount =
                    chapterProgressList.Count,

                TotalAttempts =
                    chapterProgressList.Sum(c => c.Attempts),

                DominantEmotion =
                    activities
                        .Where(a => !string.IsNullOrWhiteSpace(a.EmotionState))
                        .GroupBy(a => a.EmotionState)
                        .OrderByDescending(g => g.Count())
                        .Select(g => g.Key)
                        .FirstOrDefault() ?? "Neutral",

                EmotionSummary =
                    activities
                        .Where(a => !string.IsNullOrWhiteSpace(a.EmotionState))
                        .GroupBy(a => a.EmotionState)
                        .Select(g => new EmotionSummaryViewModel
                        {
                            Emotion = g.Key,
                            Count = g.Count()
                        })
                        .ToList(),

                CompletionRecommendations =
                    BuildCompletionRecommendations(
                    chapterProgressList,
                    activities
                        .Where(a => !string.IsNullOrWhiteSpace(a.EmotionState))
                        .GroupBy(a => a.EmotionState)
                        .OrderByDescending(g => g.Count())
                        .Select(g => g.Key)
                        .FirstOrDefault() ?? "Neutral"
                    )
            };
        }

        private ChapterProgressViewModel BuildChapterProgress(List<StudentActivity> activities, string chapterName)
        {
            var chapterActivities = activities
                .Where(a => a.ChapterName == chapterName)
                .ToList();

            if (!chapterActivities.Any())
            {
                return new ChapterProgressViewModel
                {
                    ChapterName = chapterName,
                    CompletionPercentage = 0,
                    Completed = false,
                    LastScore = 0,
                    BestScore = 0,
                    Attempts = 0,
                    EmotionState = "-"
                };
            }

            var steps = chapterActivities
                .Where(a => !string.IsNullOrWhiteSpace(a.ActivityType))
                .Select(a => a.ActivityType.Trim())
                .Distinct()
                .ToList();

            bool theoryDone = steps.Contains("Theory");
            bool bookDone = steps.Contains("Book");
            bool quizDone = steps.Contains("Quiz");

            int completedSteps = 0;

            if (theoryDone) completedSteps++;
            if (bookDone) completedSteps++;
            if (quizDone) completedSteps++;

            int percentage = (int)Math.Round((double)completedSteps / 3 * 100);

            var quizActivities = chapterActivities
                .Where(a => a.ActivityType == "Quiz")
                .OrderByDescending(a => a.CreatedAt)
                .ToList();

            var lastQuiz = quizActivities.FirstOrDefault();

            return new ChapterProgressViewModel
            {
                ChapterName = chapterName,
                CompletionPercentage = percentage,
                Completed = theoryDone && bookDone && quizDone,

                LastScore = lastQuiz?.Score ?? 0,
                BestScore = quizActivities.Any() ? quizActivities.Max(a => a.Score) : 0,
                Attempts = quizActivities.Count,
                EmotionState = lastQuiz?.EmotionState ?? "-"
            };
        }


        [HttpPost]
        public IActionResult AnalyzeLive([FromBody] StudentActivity activity)
        {
            if (activity == null)
                return BadRequest();

            var analysis = _emotionEngine.Analyze(activity);

            return Ok(new
            {
                emotion = analysis.Emotion,
                title = analysis.Title,
                message = analysis.Message,
                confidence = analysis.Confidence,
                difficultyLevel = analysis.DifficultyLevel,
                supportAction = analysis.SupportAction,
                uiMode = analysis.UiMode,
                reasons = analysis.Reasons,
                recommendations = analysis.Recommendations
            });
        }

        public async Task<IActionResult> Completion()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return View(new StudentProgressViewModel());

            var model = await BuildStudentProgressViewModelAsync(userId);

            return View(model);
        }

        private List<CompletionRecommendationViewModel> BuildCompletionRecommendations(List<ChapterProgressViewModel> chapters,string dominantEmotion)
        {
            var recommendations = new List<CompletionRecommendationViewModel>();

            var weakerChapters = chapters
                .Where(c => c.CompletionPercentage > 0 && c.CompletionPercentage < 70)
                .OrderBy(c => c.CompletionPercentage)
                .Take(2)
                .Select(c => c.ChapterName)
                .ToList();

            if (weakerChapters.Any())
            {
                recommendations.Add(new CompletionRecommendationViewModel
                {
                    Title = "Πρόταση επανάληψης",
                    Text = $"Καλό είναι να ξαναδείς: {string.Join(", ", weakerChapters)}."
                });
            }
            else
            {
                recommendations.Add(new CompletionRecommendationViewModel
                {
                    Title = "Επόμενο βήμα",
                    Text = "Έχεις ολοκληρώσει όλα τα κεφάλαια με καλή συνολική εικόνα."
                });
            }

            switch (dominantEmotion?.Trim().ToLowerInvariant())
            {
                case "frustrated":
                    recommendations.Add(new CompletionRecommendationViewModel
                    {
                        Title = "Συναισθηματική υποστήριξη",
                        Text = "Παρατηρήθηκαν στιγμές απογοήτευσης. Προτείνεται επανάληψη με μικρότερα βήματα."
                    });
                    break;

                case "tired":
                    recommendations.Add(new CompletionRecommendationViewModel
                    {
                        Title = "Ρυθμός μελέτης",
                        Text = "Εντοπίστηκαν σημάδια κούρασης. Προτείνεται διάλειμμα και επιστροφή αργότερα."
                    });
                    break;

                case "confident":
                    recommendations.Add(new CompletionRecommendationViewModel
                    {
                        Title = "Ενίσχυση μαθησιακής πορείας",
                        Text = "Η εικόνα δείχνει αυτοπεποίθηση. Μπορείς να δοκιμάσεις πιο απαιτητικές δραστηριότητες."
                    });
                    break;

                default:
                    recommendations.Add(new CompletionRecommendationViewModel
                    {
                        Title = "Συνέχιση μάθησης",
                        Text = "Συνέχισε με στοχευμένη επανάληψη και παρακολούθηση της προόδου σου."
                    });
                    break;
            }

            return recommendations;
        }

        public async Task<IActionResult> MyQuestions()
        {
            var user = await _userManager.GetUserAsync(User);

            var questions = await _context.StudentQuestions
                .Where(q => q.StudentId == user.Id)
                .OrderByDescending(q => q.CreatedAt)
                .ToListAsync();

            return View(questions);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendQuestion(string chapter, string message)
        {
            var user = await _userManager.GetUserAsync(User);

            if (string.IsNullOrWhiteSpace(chapter) ||
                string.IsNullOrWhiteSpace(message))
            {
                TempData["Error"] = "Συμπλήρωσε όλα τα πεδία.";
                return RedirectToAction("MyQuestions");
            }

            var question = new StudentQuestion
            {
                StudentId = user.Id,
                StudentName = user.UserName,
                Chapter = chapter,
                Message = message,
                CreatedAt = DateTime.Now
            };

            _context.StudentQuestions.Add(question);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Η απορία στάλθηκε επιτυχώς.";

            return RedirectToAction("MyQuestions");
        }
    }


}