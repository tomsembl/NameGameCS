using Microsoft.AspNetCore.SignalR;
using NameGameCS.Models;
using Newtonsoft.Json;
using System.Xml.Linq;

namespace NameGameCS {
    public class OutboundSignalR {
        private readonly IHubContext<SignalRHub> _hubContext;
        private readonly EFLogic _efLogic;
        public OutboundSignalR(
            IHubContext<SignalRHub> hubContext,
            EFLogic efLogic
            ) {
            _efLogic = efLogic;
            _hubContext = hubContext;
        }

        public IClientProxy? getGroup(string groupName) {
            IClientProxy group;
            try {
                group = _hubContext.Clients.Group(groupName);
            } catch (NullReferenceException e) {
                return null;
            }
            return group;
        }

        public async Task EmitGamesListChanged(string user, string message) {
            IClientProxy? group = getGroup("game_join");
            if (group == null) {
                return;
            }
            await group.SendAsync("EmitGamesListChanged", user, message);
        }

        public async Task EmitKickGroup(string byUser, string groupName) {
            IClientProxy? group = getGroup(groupName);
            string message = (groupName.StartsWith("game") ? "Game deleted" : "You were kicked") + $" by {byUser}";
            await group.SendAsync("EmitKickGroup", "KickGroup()", message);
        }

        public async Task EmitAdvanceGame(string byUser, string groupName) {
            IClientProxy? group = getGroup(groupName);
            string message = $"Game advanced by {byUser}";
            await group.SendAsync("message", "EmitAdvanceGame()", message);
            await group.SendAsync("EmitAdvanceGame", "EmitAdvanceGame()", message);
        }

        public async Task EmitPlayers(string user, Game game) {
            IClientProxy? group = getGroup($"game{game.game_id}");
            if (group == null) return;
            Dictionary<int, string> players = await _efLogic.GetPlayers(game);
            await group.SendAsync("EmitPlayers", user, JsonConvert.SerializeObject(players));
        }

        public async Task EmitTeamMembers(string user, Game game) {
            IClientProxy? group = getGroup($"game{game.game_id}");
            if (group == null) return;
            Dictionary<int, int[]> TeamMembers = await _efLogic.GetTeamMembers(game);
            await group.SendAsync("EmitTeamMembers", user, JsonConvert.SerializeObject(TeamMembers));
        }

        public async Task EmitTeams(string user, Game game) {
            IClientProxy? group = getGroup($"game{game.game_id}");
            if (group == null) return;
            IEnumerable<Team> teams = await _efLogic.GetTeams(game);
            await group.SendAsync("EmitTeams", user, JsonConvert.SerializeObject(teams));
        }

        public async Task EmitWaitingOn(string user, Game game) {
            IClientProxy? group = getGroup($"game{game.game_id}");
            if (group == null) return;
            List<UserInstance> players = await _efLogic.GetWaitingOn(game);
            string json = JsonConvert.SerializeObject(players);
            await group.SendAsync("EmitWaitingOn", user, json);
        }

        public async Task EmitScoreAnswer(string user_, Game game, User user, Name name, bool is_success, bool is_skip) { 
            IClientProxy? group = getGroup($"game{game.game_id}");
            if (group == null) return;
            if (is_success) {
                _ = EmitNextName(game, user);
                await group.SendAsync("EmitPreviousName", "EmitPreviousName()", name.name);
                return;
            }
            if (is_skip) {
                await EmitNextName(game, user);
                return;
            }
            await AdvanceTurnOrder("EmitScoreAnswer()", game);
        }

        public async Task AdvanceTurnOrder(string user, Game game) {
            IClientProxy? group = getGroup($"game{game.game_id}");
            if (group == null) return;
            await _efLogic.AdvanceTurnOrder(game);
            CurrentTurn currentTurn = await _efLogic.GetCurrentTurn(game);
            string currentTurnJson = JsonConvert.SerializeObject(currentTurn);
            TurnOrder turnOrder = await _efLogic.GetTurnOrder(game);
            string turnOrderJson = JsonConvert.SerializeObject(turnOrder.user_ids);
            await group.SendAsync("EmitCurrentTurn", user, currentTurnJson);
            await group.SendAsync("EmitTurnOrder", user, turnOrderJson);
        }

        public async Task EmitNextName(Game game, User user) {
            List<Name> names = await _efLogic.GetNames(game);
            if (names.Count == 0) {
                await _efLogic.AdvanceRound(game);
                await _efLogic.AdvanceTurnOrder(game);
                IClientProxy? gameGroup = getGroup($"game{game.game_id}");
                if (gameGroup == null) return;
                await gameGroup.SendAsync("EmitCurrentRound", "EmitNextName()", game.round.ToString());
                return;
            }
            Name name = names[new Random().Next(names.Count)];
            string json = JsonConvert.SerializeObject(name);
            IClientProxy? userGroup = getGroup($"user{user.user_id}");
            if (userGroup == null) return;
            await userGroup.SendAsync("EmitNextName", "EmitNextName()", json);
        }

        public async Task EmitStartTimer(string user_, Game game) {
            IClientProxy? group = getGroup($"game{game.game_id}");
            if (group == null) return;
            await group.SendAsync("EmitStartTimer", user_);
        }

        public async Task EmitStopTimer(string user_, Game game) {
            IClientProxy? group = getGroup($"game{game.game_id}");
            if (group == null) return;
            await group.SendAsync("EmitStopTimer", user_);
        }

        public async Task EmitTurnOrder(string user, Game game) {
            IClientProxy? group = getGroup($"game{game.game_id}");
            if (group == null) return;
            TurnOrder turnOrder = await _efLogic.GetTurnOrder(game);
            string json = JsonConvert.SerializeObject(turnOrder.user_ids);
            await group.SendAsync("EmitTurnOrder", user, json);
        }
    }
}
