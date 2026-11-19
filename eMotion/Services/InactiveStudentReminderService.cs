using eMotion.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace eMotion.Services
{
    public class InactiveStudentReminderService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public InactiveStudentReminderService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _scopeFactory.CreateScope();

                var context = scope.ServiceProvider
                    .GetRequiredService<ApplicationDbContext>();

                var userManager = scope.ServiceProvider
                    .GetRequiredService<UserManager<IdentityUser>>();

                var emailService = scope.ServiceProvider
                    .GetRequiredService<EmailService>();

                var users = await userManager.Users.ToListAsync();

                foreach (var user in users)
                {
                    if (!await userManager.IsInRoleAsync(user, "Student"))
                        continue;

                    var activities = await context.StudentActivities
                        .Where(a => a.UserId == user.Id)
                        .OrderByDescending(a => a.CreatedAt)
                        .ToListAsync();

                    var latestActivity = activities.FirstOrDefault();

                    if (latestActivity == null)
                        continue;

                    var daysInactive =
                        (DateTime.Now - latestActivity.CreatedAt).TotalDays;

                    if (daysInactive >= 1 && !latestActivity.ReminderSent)
                    {
                        var body = $@"
                            <div style='font-family: Arial, sans-serif; line-height: 1.6; color: #1f2937;'>

                                <h2 style='color: #4f46e5;'>
                                    Συνέχισε τη μαθησιακή σου πορεία στο eMotion
                                </h2>

                            <p>
                                Παρατηρήσαμε ότι έχει περάσει λίγος χρόνος από την τελευταία σου
                                δραστηριότητα στην πλατφόρμα.
                            </p>

                            <p>
                                Μπορείς να επιστρέψεις όποτε είσαι έτοιμος/η, να συνεχίσεις από το
                                σημείο που σταμάτησες και να ολοκληρώσεις σταδιακά το επόμενο κεφάλαιο.
                            </p>

                            <p>
                                Η πρόοδός σου αποθηκεύεται αυτόματα, ώστε να μπορείς να συνεχίσεις
                                με τον δικό σου ρυθμό.
                            </p>

                            <br/>

                            <p>
                            <strong>Η ομάδα του eMotion</strong>
                            </p>

                            </div>
                            ";

                        await emailService.SendEmailAsync(
                            user.Email!,
                            "Συνέχισε τη μαθησιακή σου πορεία",
                            body
                        );

                        await emailService.SendEmailAsync(
                            user.Email!,
                            "Συνέχισε το μάθημά σου",
                            body
                        );
                        latestActivity.ReminderSent = true;

                        await context.SaveChangesAsync();
                    }
                }

                await Task.Delay(TimeSpan.FromHours(6), stoppingToken);
            }
        }
    }
}