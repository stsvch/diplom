using Content.Domain.Enums;
using Content.Domain.ValueObjects.Answers;
using Content.Domain.ValueObjects.Blocks;

namespace Content.Application.Grading.Graders;

public class MatchingGrader : IBlockGrader
{
    public LessonBlockType SupportedType => LessonBlockType.Matching;

    public GradeResult Grade(LessonBlockData data, LessonBlockAnswer answer, LessonBlockSettings settings)
    {
        var d = (MatchingBlockData)data;
        var a = (MatchingAnswer)answer;

        if (d.CorrectPairs.Count == 0)
            return new GradeResult(0, settings.Points, false, false);

        var correct = 0;
        foreach (var pair in d.CorrectPairs)
        {
            var match = a.Pairs.FirstOrDefault(p => p.LeftId == pair.LeftId);
            if (match is not null && match.RightId == pair.RightId) correct++;
        }

        var fraction = (decimal)correct / d.CorrectPairs.Count;
        var score = Math.Round(settings.Points * fraction, 2);
        return new GradeResult(score, settings.Points, fraction == 1m, false);
    }
}
