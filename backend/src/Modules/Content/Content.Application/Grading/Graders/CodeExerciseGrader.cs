// CodeExerciseGrader.cs

using Content.Domain.Enums;
using Content.Domain.ValueObjects.Answers;
using Content.Domain.ValueObjects.Blocks;

namespace Content.Application.Grading.Graders;

// Grader class: рассчитывает баллы и корректность ответа для своего типа интерактивного блока.
public class CodeExerciseGrader : IBlockGrader
{
    public LessonBlockType SupportedType => LessonBlockType.CodeExercise;

    // Сравнивает ответ студента с эталоном блока и возвращает баллы, статус и фидбек.
    public GradeResult Grade(LessonBlockData data, LessonBlockAnswer answer, LessonBlockSettings settings)
    {
        var d = (CodeExerciseBlockData)data;
        var a = (CodeExerciseAnswer)answer;

        if (a.RunOutput is null || a.RunOutput.Count == 0)
            return new GradeResult(0, settings.Points, false, NeedsReview: true, "Код ещё не был выполнен.");

        var total = d.TestCases.Count;
        if (total == 0)
            return new GradeResult(0, settings.Points, false, false);

        var passed = a.RunOutput.Count(r => r.Passed);
        var fraction = (decimal)passed / total;
        var score = Math.Round(settings.Points * fraction, 2);
        return new GradeResult(score, settings.Points, fraction == 1m, false);
    }
}
