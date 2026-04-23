using Content.Domain.Enums;
using Content.Domain.ValueObjects.Answers;
using Content.Domain.ValueObjects.Blocks;

namespace Content.Application.Grading.Graders;

public class TrueFalseGrader : IBlockGrader
{
    public LessonBlockType SupportedType => LessonBlockType.TrueFalse;

    public GradeResult Grade(LessonBlockData data, LessonBlockAnswer answer, LessonBlockSettings settings)
    {
        var d = (TrueFalseBlockData)data;
        var a = (TrueFalseAnswer)answer;

        var total = d.Statements.Count;
        if (total == 0)
            return new GradeResult(0, settings.Points, false, false);

        var correctCount = 0;
        foreach (var stmt in d.Statements)
        {
            var response = a.Responses.FirstOrDefault(r => r.StatementId == stmt.Id);
            if (response is not null && response.Answer == stmt.IsTrue) correctCount++;
        }

        var fraction = (decimal)correctCount / total;
        var score = Math.Round(settings.Points * fraction, 2);
        return new GradeResult(score, settings.Points, fraction == 1m, false);
    }
}
