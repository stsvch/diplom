// SingleChoiceGrader.cs

using Content.Domain.Enums;
using Content.Domain.ValueObjects.Answers;
using Content.Domain.ValueObjects.Blocks;

namespace Content.Application.Grading.Graders;

// Grader class: рассчитывает баллы и корректность ответа для своего типа интерактивного блока.
public class SingleChoiceGrader : IBlockGrader
{
    public LessonBlockType SupportedType => LessonBlockType.SingleChoice;

    // Сравнивает ответ студента с эталоном блока и возвращает баллы, статус и фидбек.
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
