using System.Text.Json.Serialization;
using Content.Domain.Enums;

namespace Content.Domain.ValueObjects.Blocks;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(TextBlockData), nameof(LessonBlockType.Text))]
[JsonDerivedType(typeof(VideoBlockData), nameof(LessonBlockType.Video))]
[JsonDerivedType(typeof(AudioBlockData), nameof(LessonBlockType.Audio))]
[JsonDerivedType(typeof(ImageBlockData), nameof(LessonBlockType.Image))]
[JsonDerivedType(typeof(BannerBlockData), nameof(LessonBlockType.Banner))]
[JsonDerivedType(typeof(FileBlockData), nameof(LessonBlockType.File))]
[JsonDerivedType(typeof(SingleChoiceBlockData), nameof(LessonBlockType.SingleChoice))]
[JsonDerivedType(typeof(MultipleChoiceBlockData), nameof(LessonBlockType.MultipleChoice))]
[JsonDerivedType(typeof(TrueFalseBlockData), nameof(LessonBlockType.TrueFalse))]
[JsonDerivedType(typeof(FillGapBlockData), nameof(LessonBlockType.FillGap))]
[JsonDerivedType(typeof(DropdownBlockData), nameof(LessonBlockType.Dropdown))]
[JsonDerivedType(typeof(WordBankBlockData), nameof(LessonBlockType.WordBank))]
[JsonDerivedType(typeof(ReorderBlockData), nameof(LessonBlockType.Reorder))]
[JsonDerivedType(typeof(MatchingBlockData), nameof(LessonBlockType.Matching))]
[JsonDerivedType(typeof(OpenTextBlockData), nameof(LessonBlockType.OpenText))]
[JsonDerivedType(typeof(CodeExerciseBlockData), nameof(LessonBlockType.CodeExercise))]
[JsonDerivedType(typeof(QuizBlockData), nameof(LessonBlockType.Quiz))]
[JsonDerivedType(typeof(AssignmentBlockData), nameof(LessonBlockType.Assignment))]
public abstract class LessonBlockData
{
    [JsonIgnore]
    public abstract LessonBlockType Type { get; }
}
