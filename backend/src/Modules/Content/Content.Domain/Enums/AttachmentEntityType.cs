// AttachmentEntityType.cs

namespace Content.Domain.Enums;

// Доменное перечисление enum: фиксирует допустимые состояния и режимы без строковых литералов.
public enum AttachmentEntityType
{
    LessonBlock,
    Assignment,
    AssignmentSubmission,
    Comment,
    Exercise,
    DictionaryWord,
    UserAvatar,
    CourseCover,
    ChatMessage
}
