// UserStatsDto.cs
namespace Auth.Application.Queries.GetDashboardStats;

// Сводка для админского дашборда: общее число пользователей, разбивка по ролям, блокировки, неподтверждённые email и новые за 7 дней.
public class UserStatsDto
{
    public int Total { get; set; }
    public int Students { get; set; }
    public int Teachers { get; set; }
    public int Admins { get; set; }
    public int Blocked { get; set; }
    public int UnconfirmedEmail { get; set; }
    public int NewLast7Days { get; set; }
}
