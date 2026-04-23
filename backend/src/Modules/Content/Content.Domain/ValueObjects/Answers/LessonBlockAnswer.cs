using System.Text.Json.Serialization;
using Content.Domain.Enums;

namespace Content.Domain.ValueObjects.Answers;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(SingleChoiceAnswer), nameof(LessonBlockType.SingleChoice))]
[JsonDerivedType(typeof(MultipleChoiceAnswer), nameof(LessonBlockType.MultipleChoice))]
[JsonDerivedType(typeof(TrueFalseAnswer), nameof(LessonBlockType.TrueFalse))]
[JsonDerivedType(typeof(FillGapAnswer), nameof(LessonBlockType.FillGap))]
[JsonDerivedType(typeof(DropdownAnswer), nameof(LessonBlockType.Dropdown))]
[JsonDerivedType(typeof(WordBankAnswer), nameof(LessonBlockType.WordBank))]
[JsonDerivedType(typeof(ReorderAnswer), nameof(LessonBlockType.Reorder))]
[JsonDerivedType(typeof(MatchingAnswer), nameof(LessonBlockType.Matching))]
[JsonDerivedType(typeof(OpenTextAnswer), nameof(LessonBlockType.OpenText))]
[JsonDerivedType(typeof(CodeExerciseAnswer), nameof(LessonBlockType.CodeExercise))]
public abstract class LessonBlockAnswer
{
    [JsonIgnore]
    public abstract LessonBlockType Type { get; }
}
