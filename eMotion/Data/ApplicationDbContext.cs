using eMotion.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace eMotion.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<StudentActivity> StudentActivities { get; set; }

        public DbSet<TeacherMessage> TeacherMessages { get; set; }

        public DbSet<StudentLoginActivity> StudentLoginActivities { get; set; }

        public DbSet<StudentQuestion> StudentQuestions { get; set; }
    }
}
