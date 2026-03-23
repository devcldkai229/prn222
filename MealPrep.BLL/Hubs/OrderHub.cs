using Microsoft.AspNetCore.SignalR;

namespace MealPrep.BLL.Hubs;

/// <summary>
/// Real-time hub for order-related events.
/// - Admin receives new order notifications & status changes
/// - Users receive order status updates for their own orders
/// - Shippers receive assignment notifications
/// </summary>
public class OrderHub : Hub
{
    /// <summary>
    /// Admins join the "Admins" group on connection so they receive
    /// admin-only broadcasts (new orders, status changes overview).
    /// </summary>
    public async Task JoinAdminGroup()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "Admins");
    }

    /// <summary>
    /// Users join a group keyed by their UserId so they receive
    /// personal order updates.
    /// </summary>
    public async Task JoinUserGroup(string userId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");
    }

    /// <summary>
    /// Shippers join a group keyed by their ShipperId.
    /// </summary>
    public async Task JoinShipperGroup(string shipperId)
    {
        var group = $"Shipper_{shipperId}".ToLowerInvariant();
        await Groups.AddToGroupAsync(Context.ConnectionId, group);
    }
}
