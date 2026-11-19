using eMotion.Models;

namespace eMotion.Services
{
    public class EmotionAnalysisResult
    {
        public string Emotion { get; set; } = "Neutral";
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;

        public double Confidence { get; set; }

        public string DifficultyLevel { get; set; } = "Medium";

        public string SupportAction { get; set; } = "Continue";

        public string UiMode { get; set; } = "neutral";

        public List<string> Reasons { get; set; } = new();

        public List<string> Recommendations { get; set; } = new();
    }

    public interface IEmotionEngineService
    {
        EmotionAnalysisResult Analyze(StudentActivity activity);
    }

    public class EmotionEngineService : IEmotionEngineService
    {
        public EmotionAnalysisResult Analyze(StudentActivity activity)
        {
            var result = new EmotionAnalysisResult();

            int answeredQuestions =
                activity.CorrectAnswers + activity.WrongAnswers;

            double scoreRate =
                answeredQuestions > 0
                ? (double)activity.CorrectAnswers / answeredQuestions
                : 0;

            bool hasQuizData = answeredQuestions > 0;

            bool highScore =
                hasQuizData && scoreRate >= 0.80;

            bool mediumScore =
                hasQuizData &&
                scoreRate >= 0.50 &&
                scoreRate < 0.80;

            bool lowScore =
                hasQuizData &&
                scoreRate < 0.50;

            bool highIdle =
                activity.IdleTimeSeconds >= 90;

            bool longQuizTime =
                activity.QuizTimeSeconds > 120;

            bool veryLongQuizTime =
                activity.QuizTimeSeconds > 180;

            bool manyChanges =
                activity.AnswerChanges >= 4;

            bool someChanges =
                activity.AnswerChanges >= 2;

            bool manyWrongAnswers =
                activity.WrongAnswers >= 3;

            bool repeatedAttempts =
                activity.Attempts >= 2;

            bool longTheoryTime =
                activity.TheoryTimeSeconds > 500;

            var selfReported =
                activity.SelfReportedEmotion?.Trim();

            bool selfReportedFrustrated =
                selfReported == "Frustrated";

            bool selfReportedAnxious =
                selfReported == "Anxious";

            bool selfReportedTired =
                selfReported == "Tired";

            bool selfReportedConfident =
                selfReported == "Confident";

            if (answeredQuestions < 3 &&
                activity.IdleTimeSeconds < 90)
            {
                return new EmotionAnalysisResult
                {
                    Emotion = "Neutral",

                    Title = "Συλλογή δεδομένων",

                    Message =
                        "Δεν υπάρχουν ακόμη αρκετά δεδομένα για ασφαλή εκτίμηση της κατάστασης του μαθητή.",

                    Confidence = 0.40,

                    DifficultyLevel = "Medium",

                    SupportAction = "Continue",

                    UiMode = "neutral",

                    Reasons = new List<string>
                    {
                        "Μικρός αριθμός απαντήσεων μέχρι στιγμής"
                    },

                    Recommendations = new List<string>
                    {
                        "Συνέχιση της δραστηριότητας για καλύτερη εκτίμηση"
                    }
                };
            }

            if (highIdle)
            {
                result.Emotion = "Idle";

                result.Confidence = 0.95;

                result.Title = "Εντοπίστηκε αδράνεια";

                result.Message =
                    "Ο μαθητής έμεινε ανενεργός για αρκετή ώρα και προτείνεται σύντομη επανάληψη.";

                result.DifficultyLevel = "Easy";

                result.SupportAction = "ShowSummary";

                result.UiMode = "idle";

                result.Reasons.Add("Μεγάλος χρόνος αδράνειας");

                if (activity.MaterialClicks == 0)
                {
                    result.Reasons.Add(
                        "Περιορισμένη αλληλεπίδραση με το εκπαιδευτικό υλικό");
                }

                result.Recommendations.Add(
                    "Εμφάνιση σύντομης επανάληψης");

                result.Recommendations.Add(
                    "Συνέχιση της ενότητας από ένα πιο απλό σημείο");

                return result;
            }
            if (highScore &&
                activity.WrongAnswers <= 1 &&
                activity.Attempts <= 1)
            {
                result.Emotion = "Confident";

                result.Confidence =
                    selfReportedConfident ? 0.93 : 0.88;

                result.Title =
                    "Εντοπίστηκε υψηλή αυτοπεποίθηση";

                result.Message =
                    "Ο μαθητής φαίνεται να κατανοεί την ενότητα με άνεση και σταθερή απόδοση.";

                result.DifficultyLevel = "Hard";

                result.SupportAction = "UnlockChallenge";

                result.UiMode = "boost";

                result.Reasons.Add(
                    "Πολύ καλή επίδοση στις απαντήσεις");

                result.Reasons.Add(
                    "Ελάχιστα λάθη");

                if (activity.QuizTimeSeconds > 0 &&
                    activity.QuizTimeSeconds < 120)
                {
                    result.Reasons.Add(
                        "Γρήγορη και άνετη ολοκλήρωση του quiz");
                }

                if (selfReportedConfident)
                {
                    result.Reasons.Add(
                        "Η αυτοαναφορά επιβεβαιώνει την καλή επίδοση");
                }

                result.Recommendations.Add(
                    "Συνέχιση με το βασικό υλικό θεωρίας");

                result.Recommendations.Add(
                    "Εμφάνιση πιο απαιτητικής δραστηριότητας");

                result.Recommendations.Add(
                    "Ενεργοποίηση δραστηριοτήτων αυξημένης δυσκολίας");

                return result;
            }
            if (lowScore &&
                (manyWrongAnswers ||
                 repeatedAttempts ||
                 longQuizTime ||
                 selfReportedFrustrated))
            {
                result.Emotion = "Frustrated";

                result.Confidence =
                    selfReportedFrustrated ? 0.86 : 0.82;

                result.Title =
                    "Εντοπίστηκε δυσκολία κατανόησης της ενότητας";

                result.Message =
                    "Η επίδοση και οι προσπάθειες δείχνουν ότι ο μαθητής δυσκολεύεται στην ενότητα.";

                result.DifficultyLevel = "Easy";

                result.SupportAction =
                    "ShowScaffoldedTheory";

                result.UiMode = "support";

                if (manyWrongAnswers)
                {
                    result.Reasons.Add(
                        "Πολλές λανθασμένες απαντήσεις");
                }

                if (longQuizTime)
                {
                    result.Reasons.Add(
                        "Μεγάλος χρόνος ολοκλήρωσης");
                }

                if (repeatedAttempts)
                {
                    result.Reasons.Add(
                        "Πολλές προσπάθειες στο quiz");
                }

                if (selfReportedFrustrated)
                {
                    result.Reasons.Add(
                        "Η αυτοαναφορά ενισχύει την ένδειξη δυσκολίας");
                }

                result.Recommendations.Add(
                    "Εμφάνιση πιο απλής θεωρίας");

                result.Recommendations.Add(
                    "Πιο εύκολες ασκήσεις");

                result.Recommendations.Add(
                    "Καθοδήγηση βήμα προς βήμα");

                return result;
            }
            if ((mediumScore || highScore) &&
                (manyChanges ||
                 (selfReportedAnxious && (someChanges || longQuizTime))))
            {
                result.Emotion = "Anxious";

                result.Confidence =
                    selfReportedAnxious ? 0.82 : 0.75;

                result.Title =
                    "Εντοπίστηκε αβεβαιότητα";

                result.Message =
                    "Ο μαθητής δείχνει κατανόηση αλλά και δισταγμό στις επιλογές του.";

                result.DifficultyLevel = "Medium";

                result.SupportAction = "ShowHints";

                result.UiMode = "focus";

                if (manyChanges || someChanges)
                {
                    result.Reasons.Add(
                        "Αλλαγές στις απαντήσεις");
                }

                if (longQuizTime)
                {
                    result.Reasons.Add(
                        "Αυξημένος χρόνος στο quiz");
                }

                if (mediumScore)
                {
                    result.Reasons.Add(
                        "Σταθερή αλλά όχι απόλυτα σίγουρη επίδοση");
                }

                if (highScore)
                {
                    result.Reasons.Add(
                        "Καλή επίδοση με ενδείξεις δισταγμού");
                }

                if (selfReportedAnxious)
                {
                    result.Reasons.Add(
                        "Η αυτοαναφορά άγχους λήφθηκε υπόψη ως επιπλέον ένδειξη");
                }

                result.Recommendations.Add(
                    "Εμφάνιση μικρών βοηθειών");

                result.Recommendations.Add(
                    "Παροχή υποστηρικτικής θεωρίας");

                result.Recommendations.Add(
                    "Συνέχιση με επιπλέον υποστηρικτικές δραστηριότητες");

                return result;
            }
            if (longTheoryTime ||
                veryLongQuizTime ||
                (selfReportedTired && longQuizTime))
            {
                result.Emotion = "Tired";

                result.Confidence =
                    selfReportedTired ? 0.82 : 0.70;

                result.Title =
                    "Εντοπίστηκε πιθανή κόπωση";

                result.Message =
                    "Η μεγάλη διάρκεια μελέτης δείχνει ότι ο μαθητής ίσως χρειάζεται πιο ήπιο ρυθμό.";

                result.DifficultyLevel = "Easy";

                result.SupportAction =
                    "ShowBreakSuggestion";

                result.UiMode = "calm";

                if (longTheoryTime)
                {
                    result.Reasons.Add(
                        "Πολύωρη μελέτη θεωρίας");
                }

                if (veryLongQuizTime)
                {
                    result.Reasons.Add(
                        "Μεγάλη διάρκεια στο quiz");
                }

                if (selfReportedTired)
                {
                    result.Reasons.Add(
                        "Η αυτοαναφορά κόπωσης ενισχύει την εκτίμηση");
                }

                result.Recommendations.Add(
                    "Προτείνεται ένα μικρό διάλειμμα");

                result.Recommendations.Add(
                    "Σύντομη επανάληψη βασικών σημείων της θεωρίας");

                result.Recommendations.Add(
                    "Συνέχιση με πιο ήπιο ρυθμό");

                return result;
            }

            result.Emotion = "Neutral";

            result.Confidence =
                hasQuizData ? 0.60 : 0.40;

            result.Title =
                "Σταθερή μαθησιακή κατάσταση";

            result.Message =
                "Δεν εντοπίστηκε σημαντική μεταβολή στη μαθησιακή κατάσταση του μαθητή.";

            result.DifficultyLevel = "Medium";

            result.SupportAction = "Continue";

            result.UiMode = "neutral";

            result.Reasons.Add(
                "Σταθερή συμμετοχή και κανονική επίδοση");

            if (!string.IsNullOrWhiteSpace(selfReported))
            {
                result.Reasons.Add(
                    "Η αυτοαναφορά καταγράφηκε, αλλά δεν ήταν αρκετή μόνη της για αλλαγή κατάστασης");
            }

            result.Recommendations.Add(
                "Συνέχιση του μαθήματος χωρίς αλλαγές");

            return result;
        }
    }
}