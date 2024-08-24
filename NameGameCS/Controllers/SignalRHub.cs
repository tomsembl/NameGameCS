using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using NameGameCS.Models;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;

namespace NameGameCS {
    public class SignalRHub : Hub {
        private readonly OutboundSignalR _outboundSignalR;
        private readonly EFLogic _efLogic;
        public SignalRHub(
            EFLogic efLogic,
            OutboundSignalR outboundSignalR
            ) {
            _efLogic = efLogic;
            _outboundSignalR = outboundSignalR;
        }
        public IClientProxy? getGroup(string groupName) {
            IClientProxy group;
            try {
                group = Clients.Group(groupName);
            } catch (NullReferenceException e) {
                return null;
            }
            return group;
        }

        public void log(string s) {
            Console.WriteLine(s);
        }

        public async Task Message(string user, string message) {
            await Clients.Group("users").SendAsync("message", user, message);
        }

        public async Task GetJoinableGames() {
            User user = _efLogic.getUserFromCookie(Context.GetHttpContext().Request);
            List<Game> games = await _efLogic.GetJoinableGamesByUser(user);
            string json = JsonConvert.SerializeObject(games);
            await Clients.Caller.SendAsync("GetJoinableGames", "GetJoinableGames()", json);
        }

        public async Task EmitWaitingOn() {
            HttpContext? httpContext = Context.GetHttpContext();
            User user = _efLogic.getUserFromCookie(httpContext.Request);
            Game game = await _efLogic.getGameFromCookie(httpContext.Request);
            List<UserInstance> players = await _efLogic.GetWaitingOn(game);
            string json = JsonConvert.SerializeObject(players);
            await Clients.Caller.SendAsync("EmitWaitingOn", "EmitWaitingOn()", json);
        }
        public async Task<string> GetRandomDefaultName() {
            Game game = await _efLogic.getGameFromCookie(Context.GetHttpContext().Request);
            DefaultName randomName = await _efLogic.GetRandomDefaultName(game);
            return randomName.name;
        }

        public async Task EmitNextName() {
            HttpContext? httpContext = Context.GetHttpContext();
            User user = _efLogic.getUserFromCookie(httpContext.Request);
            Game game = await _efLogic.getGameFromCookie(httpContext.Request); 
            await _outboundSignalR.EmitNextName(game, user);
        }

        public async Task EmitScores() {
            HttpContext? httpContext = Context.GetHttpContext();
            User user = _efLogic.getUserFromCookie(httpContext.Request);
            Game game = await _efLogic.getGameFromCookie(httpContext.Request);
            Scores scores = await _efLogic.GetScores(game);
            string json = JsonConvert.SerializeObject(scores);
            await Clients.Caller.SendAsync("EmitScores", "EmitScores()", json);
        }

        public async Task EmitMp3Order() {
            Mp3Order mp3Order = await _efLogic.GetMp3Order();
            string json = JsonConvert.SerializeObject(mp3Order);
            await Clients.Caller.SendAsync("EmitMp3Order", "EmitMp3Order()", json);
        }

        public async Task ReSyncState() {
            HttpContext? httpContext = Context.GetHttpContext();
            User user = _efLogic.getUserFromCookie(httpContext.Request);
            Game game = await _efLogic.getGameFromCookie(httpContext.Request);
            NameGameViewModel model = await _efLogic.GetNameGameViewModel(game, user);
            string json = JsonConvert.SerializeObject(model);
            await Clients.Caller.SendAsync("ReSyncState", "ReSyncState()", json);
        }

        public async override Task OnConnectedAsync() {
            try { 
                HttpContext? httpContext = Context.GetHttpContext();
                User user = _efLogic.getUserFromCookie(httpContext.Request);
                Game? game = await _efLogic.getGameFromCookie(httpContext.Request);
                string gameGroup = $"game{(game is null ? "_join" : game.game_id)}";
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user{user.user_id}");
                await Groups.AddToGroupAsync(Context.ConnectionId, gameGroup);
                await Clients.Group(gameGroup).SendAsync("message", "SignalrHub", $"{user.username} connected");
            } catch (Exception e) {
                log(e.Message);
            }
        }
        
        public async override Task OnDisconnectedAsync(Exception? e) {
            HttpContext? httpContext = Context.GetHttpContext();
            User user = _efLogic.getUserFromCookie(httpContext.Request);
            Game? game = await _efLogic.getGameFromCookie(httpContext.Request);
            string gameGroup = $"game{(game is null ? "_join" : game.game_id)}";
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user{user.user_id}");
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, gameGroup);
            await Clients.Group(gameGroup).SendAsync("message", "SignalrHub", $"{user.username} disconnected");
        }
    }
}
