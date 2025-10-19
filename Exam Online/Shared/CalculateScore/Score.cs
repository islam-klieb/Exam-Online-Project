using Exam_Online_API.Domain.Entities;
using Exam_Online_API.Shared.Submission;

namespace Exam_Online_API.Shared.CalculateScore
{
    public static class Score
    {
        public static ScoreResult CalculateScore(List<Question> questions, List<AnswerSubmission> answers)
        {
            var correctAnswers = 0;
            var totalQuestions = questions.Count;

            foreach (var question in questions)
            {
                var userAnswer = answers.FirstOrDefault(a => a.QuestionId == question.Id);
                if (userAnswer == null) continue;

                var correctChoiceIds = question.Choices
                    .Where(c => c.IsCorrect)
                    .Select(c => c.Id)
                    .OrderBy(id => id)
                    .ToList();

                var selectedChoiceIds = userAnswer.SelectedChoiceIds
                    .OrderBy(id => id)
                    .ToList();

                var isCorrect = correctChoiceIds.SequenceEqual(selectedChoiceIds);

                if (isCorrect)
                {
                    correctAnswers++;
                }
            }

            var score = totalQuestions > 0 ? (int)((correctAnswers / (double)totalQuestions) * 100) : 0;

            return new ScoreResult
            {
                Score = score,
                CorrectAnswers = correctAnswers,
                IncorrectAnswers = totalQuestions - correctAnswers,
                TotalQuestions = totalQuestions
            };
        }
    }
}
