using Content.Domain.Enums;
using Content.Domain.ValueObjects.Answers;
using Content.Domain.ValueObjects.Blocks;

namespace Content.Application.Grading.Graders;

public class WordBankGrader : IBlockGrader
{
    public LessonBlockType SupportedType => LessonBlockType.WordBank;

    public GradeResult Grade(LessonBlockData data, LessonBlockAnswer answer, LessonBlockSettings settings)
    {
        var d = (WordBankBlockData)data;
        var a = (WordBankAnswer)answer;

        var totalGaps = d.Sentences.Sum(s => s.CorrectAnswers.Count);
        if (totalGaps == 0)
            return new GradeResult(0, settings.Points, false, false);

        var correct = 0;
        foreach (var sentence in d.Sentences)
        {
            var sentenceAnswer = a.Responses.FirstOrDefault(r => r.SentenceId == sentence.Id);
            for (var i = 0; i < sentence.CorrectAnswers.Count; i++)
            {
                var expected = sentence.CorrectAnswers[i];
                var given = sentenceAnswer is not null && i < sentenceAnswer.Answers.Count ? sentenceAnswer.Answers[i] : null;
                if (given is not null && string.Equals(given.Trim(), expected.Trim(), StringComparison.OrdinalIgnoreCase))
                    correct++;
            }
        }

        var fraction = (decimal)correct / totalGaps;
        var score = Math.Round(settings.Points * fraction, 2);
        return new GradeResult(score, settings.Points, fraction == 1m, false);
    }
}
