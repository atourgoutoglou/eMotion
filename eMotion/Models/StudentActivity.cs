using System;

namespace eMotion.Models
{
    public class StudentActivity
    {
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;

        public string ChapterName { get; set; } = string.Empty;

        public int Score { get; set; }

        public int CorrectAnswers { get; set; }

        public int WrongAnswers { get; set; }

        public int Attempts { get; set; }

        public int TheoryTimeSeconds { get; set; }

        public int QuizTimeSeconds { get; set; }

        public int AnswerChanges { get; set; }

        public int MaterialClicks { get; set; }

        public int IdleTimeSeconds { get; set; }

        public string ActivityType { get; set; } = "";

        public bool ReminderSent { get; set; }

        public string EmotionState { get; set; } = string.Empty;

        public string SelfReportedEmotion { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}