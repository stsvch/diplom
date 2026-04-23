using Content.Domain.Enums;
using Content.Domain.ValueObjects.Answers;
using Content.Domain.ValueObjects.Blocks;

namespace Content.Application.Grading.Graders;

public class MultipleChoiceGrader : IBlockGrader
{
    public LessonBlockType SupportedType => LessonBlockType.MultipleChoice;

    public GradeResult Grade(LessonBlockData data, LessonBlockAnswer answer, LessonBlockSettings settings)
    {
        var d = (MultipleChoiceBlockData)data;
        var a = (MultipleChoiceAnswer)answer;

        var correctIds = d.Options.Where(o => o.IsCorrect).Select(o => o.Id).ToHashSet();
        var selectedIds = a.SelectedOptionIds.ToHashSet();
        var total = d.Options.Count;

        if (total == 0)
            return new GradeResult(0, settings.Points, false, false);

        if (d.PartialCredit)
        {
            var matches = d.Options.Count(o => correctIds.Contains(o.Id) == selectedIds.Contains(o.Id));
            var fraction = (decimal)matches / total;
            var score = Math.Round(settings.Points * fraction, 2);
            var isCorrect = fraction == 1m;
            return new GradeResult(score, settings.Points, isCorrect, false);
        }

        var fullMatch = correctIds.SetEquals(selectedIds);
        return new GradeResult(fullMatch ? settings.Points : 0m, settings.Points, fullMatch, false);
    }
}
