namespace eMotion.Models.ViewModels
{
    public class ChapterProgressViewModel
    {
        public string ChapterName { get; set; } = "";
        public int BestScore { get; set; }
        public int LastScore { get; set; }
        public string LastEmotion { get; set; } = "";
        public int Attempts { get; set; }
        public bool Completed { get; set; }
        public int CompletionPercentage { get; set; }
        public string EmotionState { get; set; } = "-";
    }

    public class RecentGradeViewModel
    {
        public string ChapterName { get; set; } = "";
        public int Score { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class StudentProgressViewModel
    {
        public int OverallCompletionPercentage { get; set; }
        public List<ChapterProgressViewModel> Chapters { get; set; } = new();
        public List<RecentGradeViewModel> RecentGrades { get; set; } = new();

        public bool HasCompletedAllChapters { get; set; }
        public int CompletedChaptersCount { get; set; }
        public int TotalChaptersCount { get; set; }
        public int TotalAttempts { get; set; }
        public string DominantEmotion { get; set; } = "Neutral";
        public List<EmotionSummaryViewModel> EmotionSummary { get; set; } = new();
        public List<CompletionRecommendationViewModel> CompletionRecommendations { get; set; } = new();
    }

    public class CompletionRecommendationViewModel
    {
        public string Title { get; set; } = "";
        public string Text { get; set; } = "";
    }

    public class EmotionSummaryViewModel
    {
        public string Emotion { get; set; } = "";
        public int Count { get; set; }
    }
}