using Content.Domain.Enums;
using Content.Domain.ValueObjects.Answers;
using Content.Domain.ValueObjects.Blocks;

namespace Content.Application.Grading.Graders;

public class SingleChoiceGrader : IBlockGrader
{
    public LessonBlockType SupportedType => LessonBlockType.SingleChoice;

    public GradeResult Grade(LessonBlockData data, LessonBlockAnswer answer, LessonBlockSettings settings)
    {
        var d = (SingleChoiceBlockData)data;
        var a = (SingleChoiceAnswer)answer;

        var correct = d.Options.FirstOrDefault(o => o.IsCorrect);
        var isCorrect = correct is not null && correct.Id == a.SelectedOptionId;
        var score = isCorrect ? settings.Points : 0m;
        return new GradeResult(score, settings.Points, isCorrect, false);
    }
}
