using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using System.Text.RegularExpressions;

namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Hubs
{
    public class NotificationHub : Hub
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public NotificationHub(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public override async Task OnConnectedAsync()
        {
            if (Context.User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(Context.User);
                await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{user.Id}");

                // Update user's last seen
                user.LastLoginAt = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            if (Context.User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(Context.User);
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"User_{user.Id}");
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task JoinClassRoom(string classId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Class_{classId}");
        }

        public async Task LeaveClassRoom(string classId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Class_{classId}");
        }

        public async Task SendMessageToClass(string classId, string message)
        {
            await Clients.Group($"Class_{classId}").SendAsync("ReceiveClassMessage", Context.User.Identity.Name, message);
        }

        public async Task SendNotificationToUser(string userId, string message)
        {
            await Clients.User(userId).SendAsync("ReceiveNotification", message);
        }

    }
}
