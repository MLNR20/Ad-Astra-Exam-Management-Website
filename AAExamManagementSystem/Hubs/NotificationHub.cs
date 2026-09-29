using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace AAExamManagementSystem.Hubs;

[Authorize]
public class NotificationHub : Hub
{
}
