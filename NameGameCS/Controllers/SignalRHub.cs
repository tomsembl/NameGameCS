using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.SignalR;
using NameGameCS.Models;
using System.Threading.Tasks;

namespace NameGameCS {
    public class SignalRHub : Hub {
        public SignalRHub() { }

        public IClientProxy? getGroup(string groupName) {
            IClientProxy group;
            try { 
                group = Clients.Group(groupName); 
            } catch (NullReferenceException e) {
                return null;
            }
            return group;
        }

        public async Task Message(string user, string message) {
            await Clients.Group("users").SendAsync("message", user, message);
        }

        public async Task RefreshJoinGame(string user, string message) {
            IClientProxy group = getGroup("join_game");
            if (group == null) return;
            await group.SendAsync("refresh_join_game", user, message);
        }

        public async override Task OnConnectedAsync() {
            HttpContext? httpContext = Context.GetHttpContext();
            string? username = httpContext.Request.Cookies["username"];
            int? user_id = int.Parse(httpContext.Request.Cookies["user_id"]);
            int? game_id = int.Parse(httpContext.Request.Cookies["game_id"]);
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user{user_id}");
            await Groups.AddToGroupAsync(Context.ConnectionId, $"game{game_id}");
            await Clients.Group($"game{game_id}").SendAsync("ReceiveMessage", "SignalrHub", $"{username} connected");
        }
        
        public async override Task OnDisconnectedAsync(Exception? e) {
            HttpContext? httpContext = Context.GetHttpContext();
            string? username = httpContext.Request.Cookies["username"];
            int? user_id = int.Parse(httpContext.Request.Cookies["user_id"]);
            int? game_id = int.Parse(httpContext.Request.Cookies["game_id"]);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user{user_id}");
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"game{game_id}");
            await Clients.Group($"game{game_id}").SendAsync("ReceiveMessage", "SignalrHub", $"{username} disconnected");
        }
    }
}
