using Content.Domain.Enums;
using Content.Domain.ValueObjects.Answers;
using Content.Domain.ValueObjects.Blocks;

namespace Content.Application.Grading.Graders;

public class FillGapGrader : IBlockGrader
{
    public LessonBlockType SupportedType => LessonBlockType.FillGap;

    public GradeResult Grade(LessonBlockData data, LessonBlockAnswer answer, LessonBlockSettings settings)
    {
        var d = (FillGapBlockData)data;
        var a = (FillGapAnswer)answer;

        var totalGaps = d.Sentences.Sum(s => s.Gaps.Count);
        if (totalGaps == 0)
            return new GradeResult(0, settings.Points, false, false);

        var correct = 0;
        foreach (var sentence in d.Sentences)
        {
            var sentenceAnswer = a.Responses.FirstOrDefault(r => r.SentenceId == sentence.Id);
            foreach (var gap in sentence.Gaps)
            {
                var value = sentenceAnswer?.Gaps.FirstOrDefault(g => g.GapId == gap.Id)?.Value ?? string.Empty;
                if (IsMatch(value, gap.CorrectAnswers, gap.CaseSensitive))
                    correct++;
            }
        }

        var fraction = (decimal)correct / totalGaps;
        var score = Math.Round(settings.Points * fraction, 2);
        return new GradeResult(score, settings.Points, fraction == 1m, false);
    }

    private static bool IsMatch(string value, IEnumerable<string> acceptable, bool caseSensitive)
    {
        var v = value.Trim();
        var comparison = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
        return acceptable.Any(a => string.Equals(a.Trim(), v, comparison));
    }
}
