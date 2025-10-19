namespace Exam_Online_API.Shared.CalculateScore
{
    public class ScoreResult
    {
        public int Score { get; set; }
        public int CorrectAnswers { get; set; }
        public int IncorrectAnswers { get; set; }
        public int TotalQuestions { get; set; }
    }
}
