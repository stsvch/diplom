namespace Content.Domain.Enums;

public enum LessonBlockType
{
    Text = 0,
    Video = 1,
    Audio = 2,
    Image = 3,
    Banner = 4,
    File = 5,

    SingleChoice = 10,
    MultipleChoice = 11,
    TrueFalse = 12,
    FillGap = 13,
    Dropdown = 14,
    WordBank = 15,
    Reorder = 16,
    Matching = 17,

    OpenText = 20,
    CodeExercise = 21,

    Quiz = 30,
    Assignment = 31
}
