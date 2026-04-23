using Content.Domain.Enums;
using Content.Domain.ValueObjects.Answers;
using Content.Domain.ValueObjects.Blocks;

namespace Content.Application.Grading.Graders;

public class OpenTextGrader : IBlockGrader
{
    public LessonBlockType SupportedType => LessonBlockType.OpenText;

    public GradeResult Grade(LessonBlockData data, LessonBlockAnswer answer, LessonBlockSettings settings)
    {
        var _ = (OpenTextBlockData)data;
        var __ = (OpenTextAnswer)answer;
        return new GradeResult(0, settings.Points, false, NeedsReview: true);
    }
}
