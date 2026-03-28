using FluentValidation;
using Scheduling.Application.Scheduling.Commands.CreateSlot;

namespace Scheduling.Application.Scheduling.Validators;

public class CreateSlotCommandValidator : AbstractValidator<CreateSlotCommand>
{
    public CreateSlotCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Название занятия обязательно.")
            .MaximumLength(500).WithMessage("Название не должно превышать 500 символов.");

        RuleFor(x => x.StartTime)
            .LessThan(x => x.EndTime).WithMessage("Время начала должно быть раньше времени окончания.");

        RuleFor(x => x.EndTime)
            .GreaterThan(x => x.StartTime).WithMessage("Время окончания должно быть позже времени начала.");

        RuleFor(x => x.MaxStudents)
            .GreaterThan(0).WithMessage("Максимальное количество студентов должно быть больше 0.");
    }
}
