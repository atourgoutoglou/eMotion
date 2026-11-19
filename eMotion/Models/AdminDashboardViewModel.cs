using System.Collections.Generic;

namespace eMotion.Models.ViewModels
{
    public class AdminUserRowViewModel
    {
        public string UserId { get; set; } = "";
        public string Email { get; set; } = "";
        public string UserName { get; set; } = "";
        public string Role { get; set; } = "";
    }

    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int StudentsCount { get; set; }
        public int TeachersCount { get; set; }
        public int AdminsCount { get; set; }

        public int TotalActivities { get; set; }
        public int TotalMessages { get; set; }
        public string MostCommonEmotion { get; set; } = "-";
        public List<string> RecentActivities { get; set; } = new();

        public List<AdminUserRowViewModel> Users { get; set; } = new();
    }
}