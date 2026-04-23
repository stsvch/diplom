using Content.Domain.Enums;
using Content.Domain.ValueObjects.Answers;
using Content.Domain.ValueObjects.Blocks;

namespace Content.Application.Grading.Graders;

public class DropdownGrader : IBlockGrader
{
    public LessonBlockType SupportedType => LessonBlockType.Dropdown;

    public GradeResult Grade(LessonBlockData data, LessonBlockAnswer answer, LessonBlockSettings settings)
    {
        var d = (DropdownBlockData)data;
        var a = (DropdownAnswer)answer;

        var totalGaps = d.Sentences.Sum(s => s.Gaps.Count);
        if (totalGaps == 0)
            return new GradeResult(0, settings.Points, false, false);

        var correct = 0;
        foreach (var sentence in d.Sentences)
        {
            var sentenceAnswer = a.Responses.FirstOrDefault(r => r.SentenceId == sentence.Id);
            foreach (var gap in sentence.Gaps)
            {
                var value = sentenceAnswer?.Gaps.FirstOrDefault(g => g.GapId == gap.Id)?.Value;
                if (value == gap.Correct) correct++;
            }
        }

        var fraction = (decimal)correct / totalGaps;
        var score = Math.Round(settings.Points * fraction, 2);
        return new GradeResult(score, settings.Points, fraction == 1m, false);
    }
}
