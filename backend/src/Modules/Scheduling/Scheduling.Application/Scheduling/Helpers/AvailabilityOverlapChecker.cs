// AvailabilityOverlapChecker.cs

using Scheduling.Domain.Entities;
using Scheduling.Domain.Enums;

namespace Scheduling.Application.Scheduling.Helpers;

/// <summary>
/// Проверяет пересечения временных интервалов между правилами расписания учителя.
/// Учитывает три комбинации: Recurring vs Recurring, OneOff vs OneOff, Recurring vs OneOff.
/// </summary>
public static class AvailabilityOverlapChecker
{
    /// Возвращает первое пересекающееся правило (если есть). null = конфликта нет.
    public static TeacherAvailability? FindOverlap(
        IEnumerable<TeacherAvailability> existing,
        AvailabilityKind kind,
        DayOfWeek? dayOfWeek,
        DateOnly? specificDate,
        TimeOnly startTime,
        TimeOnly endTime,
        DateOnly validFrom,
        DateOnly? validUntil,
        Guid? excludeId = null)
    {
        foreach (var rule in existing)
        {
            if (excludeId.HasValue && rule.Id == excludeId.Value) continue;
            if (!rule.IsActive) continue;

            if (!TimeIntervalsOverlap(startTime, endTime, rule.StartTime, rule.EndTime))
                continue;

            if (Conflicts(kind, dayOfWeek, specificDate, validFrom, validUntil, rule))
                return rule;
        }
        return null;
    }

    private static bool Conflicts(
        AvailabilityKind kind,
        DayOfWeek? dayOfWeek,
        DateOnly? specificDate,
        DateOnly validFrom,
        DateOnly? validUntil,
        TeacherAvailability other)
    {
        // Recurring vs Recurring: пересечение только если день недели совпадает И периоды действия пересекаются.
        if (kind == AvailabilityKind.Recurring && other.Kind == AvailabilityKind.Recurring)
        {
            if (dayOfWeek != other.DayOfWeek) return false;
            return DateRangesOverlap(validFrom, validUntil, other.ValidFrom, other.ValidUntil);
        }

        // OneOff vs OneOff: только если дата та же.
        if (kind == AvailabilityKind.OneOff && other.Kind == AvailabilityKind.OneOff)
        {
            return specificDate.HasValue && specificDate == other.SpecificDate;
        }

        // Recurring vs OneOff: OneOff попадает в период действия Recurring,
        // и день недели OneOff совпадает с DayOfWeek Recurring.
        if (kind == AvailabilityKind.Recurring && other.Kind == AvailabilityKind.OneOff)
        {
            return RecurringContainsOneOff(dayOfWeek, validFrom, validUntil, other.SpecificDate);
        }
        if (kind == AvailabilityKind.OneOff && other.Kind == AvailabilityKind.Recurring)
        {
            return RecurringContainsOneOff(other.DayOfWeek, other.ValidFrom, other.ValidUntil, specificDate);
        }

        return false;
    }

    private static bool RecurringContainsOneOff(
        DayOfWeek? recurringDay,
        DateOnly recurringFrom,
        DateOnly? recurringUntil,
        DateOnly? oneOffDate)
    {
        if (!recurringDay.HasValue || !oneOffDate.HasValue) return false;
        if (recurringDay.Value != oneOffDate.Value.DayOfWeek) return false;
        if (oneOffDate.Value < recurringFrom) return false;
        if (recurringUntil.HasValue && oneOffDate.Value > recurringUntil.Value) return false;
        return true;
    }

    private static bool TimeIntervalsOverlap(TimeOnly aStart, TimeOnly aEnd, TimeOnly bStart, TimeOnly bEnd)
    {
        return aStart < bEnd && bStart < aEnd;
    }

    private static bool DateRangesOverlap(DateOnly aFrom, DateOnly? aUntil, DateOnly bFrom, DateOnly? bUntil)
    {
        if (aUntil.HasValue && aUntil.Value < bFrom) return false;
        if (bUntil.HasValue && bUntil.Value < aFrom) return false;
        return true;
    }
}
