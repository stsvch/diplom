namespace Content.Domain.Enums;

public enum LessonBlockStatus
{
    /// <summary>
    /// Блок создан как пустой/частично заполненный черновик. Допустимо отсутствие обязательных данных.
    /// </summary>
    Draft = 0,

    /// <summary>
    /// Блок прошёл валидацию и готов к показу студентам.
    /// </summary>
    Ready = 1,

    /// <summary>
    /// Блок принудительно помечен сломанным (например, удалён ресурс, на который ссылался).
    /// </summary>
    Invalid = 2
}
