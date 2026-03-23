using Microsoft.AspNetCore.SignalR;

namespace MealPrep.BLL.Hubs;

/// <summary>
/// Real-time hub for admin dashboard KPI updates.
/// When orders/payments/subscriptions change, the dashboard
/// auto-refreshes without page reload.
/// </summary>
public class DashboardHub : Hub
{
    public async Task JoinAdminDashboard()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "AdminDashboard");
    }

    public async Task JoinUserDashboard(string userId)
    {
        var groupName = $"UserDashboard_{userId}".ToLowerInvariant();
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
    }
}
