using Microsoft.AspNetCore.SignalR;

namespace MealPrep.BLL.Hubs;

/// <summary>
/// Delivery Tracking Hub — real-time Order/OrderItem status updates.
/// Clients: Shipper (gửi sự kiện), Admin UI & User UI (nhận broadcast).
/// </summary>
public class OrderTrackingHub : Hub
{
    /// <summary>Admin joins to receive all tracking events</summary>
    public async Task JoinAdminGroup()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "Tracking_Admins");
    }

    /// <summary>User joins to receive updates for their orders</summary>
    public async Task JoinUserGroup(string userId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"Tracking_User_{userId}");
    }

    /// <summary>Shipper joins to receive assignments & confirmations</summary>
    public async Task JoinShipperGroup(string shipperId)
    {
        var groupName = $"Tracking_Shipper_{shipperId}".ToLowerInvariant();
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
    }
}
