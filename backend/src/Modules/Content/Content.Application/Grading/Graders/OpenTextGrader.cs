// OpenTextGrader.cs

using Content.Domain.Enums;
using Content.Domain.ValueObjects.Answers;
using Content.Domain.ValueObjects.Blocks;

namespace Content.Application.Grading.Graders;

// Grader class: рассчитывает баллы и корректность ответа для своего типа интерактивного блока.
public class OpenTextGrader : IBlockGrader
{
    public LessonBlockType SupportedType => LessonBlockType.OpenText;

    // Сравнивает ответ студента с эталоном блока и возвращает баллы, статус и фидбек.
    public GradeResult Grade(LessonBlockData data, LessonBlockAnswer answer, LessonBlockSettings settings)
    {
        var _ = (OpenTextBlockData)data;
        var __ = (OpenTextAnswer)answer;
        return new GradeResult(0, settings.Points, false, NeedsReview: true);
    }
}
