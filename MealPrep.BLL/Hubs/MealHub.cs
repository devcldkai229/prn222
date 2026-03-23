using Microsoft.AspNetCore.SignalR;

namespace MealPrep.BLL.Hubs;

/// <summary>
/// Real-time hub for meal feedback/rating events.
/// When a user submits a rating, other users viewing the same meal
/// see the updated average rating live.
/// </summary>
public class MealHub : Hub
{
}
