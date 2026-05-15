// ReorderGrader.cs

using Content.Domain.Enums;
using Content.Domain.ValueObjects.Answers;
using Content.Domain.ValueObjects.Blocks;

namespace Content.Application.Grading.Graders;

// Grader class: рассчитывает баллы и корректность ответа для своего типа интерактивного блока.
public class ReorderGrader : IBlockGrader
{
    public LessonBlockType SupportedType => LessonBlockType.Reorder;

    // Сравнивает ответ студента с эталоном блока и возвращает баллы, статус и фидбек.
    public GradeResult Grade(LessonBlockData data, LessonBlockAnswer answer, LessonBlockSettings settings)
    {
        var d = (ReorderBlockData)data;
        var a = (ReorderAnswer)answer;

        if (d.CorrectOrder.Count == 0)
            return new GradeResult(0, settings.Points, false, false);

        if (d.AllOrNothing)
        {
            var match = d.CorrectOrder.SequenceEqual(a.Order);
            return new GradeResult(match ? settings.Points : 0m, settings.Points, match, false);
        }

        var correct = 0;
        for (var i = 0; i < d.CorrectOrder.Count && i < a.Order.Count; i++)
        {
            if (d.CorrectOrder[i] == a.Order[i]) correct++;
        }

        var fraction = (decimal)correct / d.CorrectOrder.Count;
        var score = Math.Round(settings.Points * fraction, 2);
        return new GradeResult(score, settings.Points, fraction == 1m, false);
    }
}
