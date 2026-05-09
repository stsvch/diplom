using EduPlatform.Shared.Application.Contracts;

namespace Payments.Infrastructure.Services;

/// <summary>
/// Заглушка по умолчанию: возвращает пустой список. Используется, если
/// модуль Scheduling ещё не зарегистрировал реальную реализацию
/// <see cref="ICompletedBookingReadService"/>.
/// </summary>
internal sealed class NoCompletedBookingsReadService : ICompletedBookingReadService
{
    public Task<IReadOnlyList<CompletedBookingInfo>> GetCompletedBetweenAsync(
        string studentId,
        DateTime periodStart,
        DateTime periodEnd,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<CompletedBookingInfo>>(Array.Empty<CompletedBookingInfo>());
    }
}
