using Content.Domain.Enums;
using Content.Domain.ValueObjects.Answers;
using Content.Domain.ValueObjects.Blocks;

namespace Content.Application.Grading;

public class BlockGraderRegistry : IBlockGraderRegistry
{
    private readonly Dictionary<LessonBlockType, IBlockGrader> _graders;

    public BlockGraderRegistry(IEnumerable<IBlockGrader> graders)
    {
        _graders = graders.ToDictionary(g => g.SupportedType);
    }

    public GradeResult Grade(LessonBlockType type, LessonBlockData data, LessonBlockAnswer answer, LessonBlockSettings settings)
    {
        if (!_graders.TryGetValue(type, out var grader))
            throw new InvalidOperationException($"Нет проверщика для типа блока {type}.");

        return grader.Grade(data, answer, settings);
    }
}
