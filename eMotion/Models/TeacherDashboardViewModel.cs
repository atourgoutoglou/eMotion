using System.Collections.Generic;

namespace eMotion.Models.ViewModels
{
    public class TeacherDashboardStudentRowViewModel
    {
        public string StudentId { get; set; } = "";
        public string StudentName { get; set; } = "";
        public string CurrentChapter { get; set; } = "";
        public int ProgressPercentage { get; set; }
        public string BestScoreDisplay { get; set; } = "—";
        public string LastEmotion { get; set; } = "-";
        public int TotalAttempts { get; set; }
    }

    public class TeacherDashboardViewModel
    {
        public int ActiveStudentsCount { get; set; }
        public int AverageClassProgress { get; set; }
        public int ActiveChaptersCount { get; set; }

        public List<TeacherDashboardStudentRowViewModel> Students { get; set; } = new();
    }
}