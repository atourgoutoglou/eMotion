using eMotion.Data;
using eMotion.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eMotion.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            var activities = await _context.StudentActivities.ToListAsync();

            var mostCommonEmotion = activities
                .Where(a => !string.IsNullOrWhiteSpace(a.EmotionState))
                .GroupBy(a => a.EmotionState)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefault() ?? "-";

            var recentActivities = activities
                .OrderByDescending(a => a.CreatedAt)
                .Take(5)
                .Select(a =>
                {
                    var chapter = a.ChapterName switch
                    {
                        "Chapter1" => "Κεφάλαιο 1",
                        "Chapter2" => "Κεφάλαιο 2",
                        "Chapter3" => "Κεφάλαιο 3",
                        "Chapter4" => "Κεφάλαιο 4",
                        _ => a.ChapterName
                    };

                    var emotion = a.EmotionState switch
                    {
                        "Neutral" => "Ουδέτερο",
                        "Confident" => "Σίγουρος/η",
                        "Anxious" => "Αγχωμένος/η",
                        "Frustrated" => "Απογοητευμένος/η",
                        "Tired" => "Κουρασμένος/η",
                        "Idle" => "Αδρανής",
                        _ => a.EmotionState
                    };

                    return $"{a.CreatedAt:dd/MM/yyyy HH:mm} - {chapter} - {emotion}";
                })
                .ToList();

            var rows = new List<AdminUserRowViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                rows.Add(new AdminUserRowViewModel
                {
                    UserId = user.Id,
                    Email = user.Email ?? "",
                    UserName = user.UserName ?? "",
                    Role = roles.FirstOrDefault() ?? "Χωρίς ρόλο"
                });
            }

            var model = new AdminDashboardViewModel
            {
                TotalUsers = rows.Count,
                StudentsCount = rows.Count(x => x.Role == "Student"),
                TeachersCount = rows.Count(x => x.Role == "Teacher"),
                AdminsCount = rows.Count(x => x.Role == "Admin"),
                TotalActivities = activities.Count,
                TotalMessages = await _context.TeacherMessages.CountAsync(),

                MostCommonEmotion = mostCommonEmotion switch
                {
                    "Neutral" => "Ουδέτερο",
                    "Confident" => "Σίγουρος/η",
                    "Anxious" => "Αγχωμένος/η",
                    "Frustrated" => "Απογοητευμένος/η",
                    "Tired" => "Κουρασμένος/η",
                    "Idle" => "Αδρανής",
                    _ => mostCommonEmotion
                },

                RecentActivities = recentActivities,

                Users = rows
                    .OrderBy(x => x.Role)
                    .ThenBy(x => x.Email)
                    .ToList()
            };

            return View(model);
        }

        [HttpGet]
        public IActionResult CreateUser()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(string email, string password, string role)
        {
            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(role))
            {
                ModelState.AddModelError("", "Συμπλήρωσε όλα τα πεδία.");
                return View();
            }

            var allowedRoles = new[] { "Student", "Teacher", "Admin" };

            if (!allowedRoles.Contains(role))
            {
                ModelState.AddModelError("", "Μη έγκυρος ρόλος.");
                return View();
            }

            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new IdentityRole(role));
            }

            var user = new IdentityUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return View();
            }

            await _userManager.AddToRoleAsync(user, role);

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeRole(string userId, string newRole)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(newRole))
                return RedirectToAction("Index");

            var allowedRoles = new[] { "Student", "Teacher", "Admin" };

            if (!allowedRoles.Contains(newRole))
                return RedirectToAction("Index");

            if (!await _roleManager.RoleExistsAsync(newRole))
            {
                await _roleManager.CreateAsync(new IdentityRole(newRole));
            }

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return RedirectToAction("Index");

            var currentRoles = await _userManager.GetRolesAsync(user);

            if (currentRoles.Any())
            {
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
            }

            await _userManager.AddToRoleAsync(user, newRole);

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return RedirectToAction("Index");

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return RedirectToAction("Index");

            var activities = _context.StudentActivities
                .Where(x => x.UserId == userId);

            _context.StudentActivities.RemoveRange(activities);

            var messagesAsStudent = _context.TeacherMessages
                .Where(x => x.StudentId == userId);

            _context.TeacherMessages.RemoveRange(messagesAsStudent);

            var messagesAsTeacher = _context.TeacherMessages
                .Where(x => x.TeacherId == userId);

            _context.TeacherMessages.RemoveRange(messagesAsTeacher);

            await _context.SaveChangesAsync();

            await _userManager.DeleteAsync(user);

            return RedirectToAction("Index");
        }
    }
}