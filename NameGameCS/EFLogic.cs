using System;
using NameGameCS.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace NameGameCS {

    public class EFLogic {
        private readonly NameGameDbContext _dbContext;
        private readonly Random _random = new Random();

        public EFLogic(
            NameGameDbContext dbContext
        ) {
            _dbContext = dbContext;
        }

        public User getUserFromCookie(HttpRequest Request) {
            User user = new User();
            try {
                user.last_ip = Request.HttpContext.Connection.LocalIpAddress.MapToIPv4().ToString();
                user.user_id = int.Parse(Request.Cookies["user_id"]);
                user.username = Request.Cookies["username"];
            } catch { }
            return user;
        }

        public async Task<Game?> getGameFromCookie(HttpRequest Request) {
            Game game = new Game();
            int game_id;
            bool wasFound = int.TryParse(Request.Cookies["game_id"], out game_id);
            if (!wasFound) {
                return null;
            }
            return await GetGame(game_id);
        }

        public async Task<User> SignIn(User user) {
            User? dbUser = await _dbContext.Users.FirstOrDefaultAsync(x => x.user_id == user.user_id);
            if (dbUser == null) {
                dbUser = new User {
                    username = user.username ?? RandomNumberGenerator.GetHexString(5),
                    created = DateTime.Now,
                    last_login = DateTime.Now,
                    last_ip = user.last_ip,
                };
                dbUser = (await _dbContext.Users.AddAsync(dbUser)).Entity;
            } else {
                _dbContext.Update(dbUser);
                dbUser.last_login = DateTime.Now;
            }

            await _dbContext.SaveChangesAsync();
            return dbUser;
        }


        public async Task<Game> CreateGame(IFormCollection form) {
            Game game = new Game();
            game.game_name = form["game_name"];
            game.time_limit_sec = int.Parse(form["time_limit"]);
            game.number_teams = int.Parse(form["number_teams"]);
            game.number_names = int.Parse(form["number_names"]);
            game.round1 = form["round1"] == "on";
            game.round2 = form["round2"] == "on";
            game.round3 = form["round3"] == "on";
            game.round4 = form["round4"] == "on";
            game = (await _dbContext.Games.AddAsync(game)).Entity;
            await _dbContext.SaveChangesAsync();

            List<Team> teams = Enumerable.Range(0, game.number_teams).Select(i => {
                Team team = new Team();
                team.team_name = $"Team {RandomNumberGenerator.GetHexString(5)}";
                team.game_id = game.game_id;
                team.order = i;
                return team;
            }).ToList();
            await _dbContext.Teams.AddRangeAsync(teams);
            await _dbContext.SaveChangesAsync();

            game.current_team_id = teams[int.Max(0, _random.Next(teams.Count))].team_id;

            await _dbContext.SaveChangesAsync();
            return game;
        }


        public async Task CreateTeams(Game game) {
            List<Team> teams = new List<Team>();
            for (int i = 0; i < game.number_teams; i++) {
                Team team = new Team();
                team.team_name = $"Team {RandomNumberGenerator.GetHexString(5)}";
                team.game_id = game.game_id;
                team.order = i;
            }
            game = (await _dbContext.Games.AddAsync(game)).Entity;
        }

        public async Task<List<Game>> GetJoinableGamesByUser(User user) {
            var joinableGames = await _dbContext.Games
                .Where(g => g.is_active &&
                            (g.stage == (int)GameStage.lobby ||
                             _dbContext.UserInstances.Any(ui => ui.user_id == user.user_id && ui.game_id == g.game_id)))
                .OrderByDescending(g => g.game_id)
                .ToListAsync();
            return joinableGames;
        }

        public async Task<UserInstance> GetOrAddUserInstance(User user, Game game) {
            UserInstance userInstance = await _dbContext.UserInstances.FirstOrDefaultAsync(x => x.user_id == user.user_id && x.game_id == game.game_id);
            if (userInstance == null) {
                userInstance = new UserInstance {
                    user_id = user.user_id,
                    username = user.username,
                    game_id = game.game_id,
                };
                userInstance = (await _dbContext.UserInstances.AddAsync(userInstance)).Entity;
                await _dbContext.SaveChangesAsync();
            }
            return userInstance;
        }

        public async Task AddNames(UserInstance userInstance, List<Name> names) {
            IEnumerable<Name> newNames = names.Select(name => new Name {
                user_inst_id = userInstance.user_inst_id,
                game_id = userInstance.game_id,
                name = name.name,
            });
            await _dbContext.Names.AddRangeAsync(newNames);
            await _dbContext.SaveChangesAsync();
        }

        //GetRandomDefaultName
        public async Task<DefaultName> GetRandomDefaultName(Game game) {
            DefaultName name = await _dbContext.DefaultNames
                .Skip(_random.Next(_dbContext.DefaultNames.Count()))
                .FirstOrDefaultAsync();
            return name;
        }

        public async Task<Dictionary<int, int[]>> GetTeamMembers(Game game) {
            Dictionary<int, int[]> teamMembers = new Dictionary<int, int[]>();
            List<Team> teams = await _dbContext.Teams.Where(x => x.game_id == game.game_id).ToListAsync();
            foreach (Team team in teams) {
                int[] members = await _dbContext.UserInstances.Where(x => x.game_id == game.game_id && x.team_id == team.team_id).Select(x => x.user_id).ToArrayAsync();
                teamMembers[team.team_id] = members;
            }
            return teamMembers;
        }

        public async Task<Dictionary<int, string>> GetPlayers(Game game) {
            Dictionary<int, string> players = new Dictionary<int, string>();
            List<UserInstance> userInstances = await _dbContext.UserInstances.Where(x => x.game_id == game.game_id).ToListAsync();
            foreach (UserInstance userInstance in userInstances) {
                players[userInstance.user_id] = userInstance.username;
            }
            return players;
        }

        public async Task UpdateUsername(User user, UserInstance userInstance, string newUsername) { 
            _dbContext.Update(user);
            _dbContext.Update(userInstance);
            user.username = newUsername;
            userInstance.username = newUsername;
            await _dbContext.SaveChangesAsync();
        }
        public async Task DeleteGame(Game game) {
            _dbContext.Update(game);
            game.is_active = false;
            await _dbContext.SaveChangesAsync();
        }

        public async Task KickUserInstance(UserInstance userInstance) { 
            _dbContext.Remove(userInstance);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<Game> GetGame(int game_id) {
            return await _dbContext.Games.FirstOrDefaultAsync(x => x.game_id == game_id);
        }

        public async Task<IEnumerable<Team>> GetTeams(Game game) {
            return _dbContext.Teams.Where(x => x.game_id == game.game_id);
        }

        public async Task AdvanceGame(Game game, int stage) {
            _dbContext.Update(game);
            game.stage = stage;
            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<UserInstance>> GetWaitingOn(Game game) {
            return _dbContext.UserInstances
                .Where(ui => ui.game_id == game.game_id && !_dbContext.Names.Any(n => n.game_id == game.game_id && n.user_inst_id == ui.user_inst_id))
                .ToList();
        }

        public async Task<bool> GetUserHasSubmittedNames(UserInstance userInstance) {
            return _dbContext.Names
                .Where(n => n.user_inst_id == userInstance.user_inst_id)
                .Count() > 0;
        }

        public async Task ChangeTeamName(Team team, string newTeamName) {
            _dbContext.Update(team);
            team.team_name = newTeamName;
            await _dbContext.SaveChangesAsync();
        }

        public async Task PlayerTeamChange(Team team, UserInstance userInstance) {
            _dbContext.Update(userInstance);
            userInstance.team_id = team.team_id;
            await _dbContext.SaveChangesAsync();
        }

        public async Task InitPlayerTurnOrder(Game game) {
            IEnumerable<Team> teams = _dbContext.Teams.Where(x => x.game_id == game.game_id);
            _dbContext.UpdateRange(teams);
            foreach (Team team in teams) {
                List<UserInstance> userInstances = await _dbContext.UserInstances.Where(x => x.team_id == team.team_id).ToListAsync();
                for (int i = 0; i < userInstances.Count; i++) {
                    UserInstance userInstance = userInstances[i];
                    _dbContext.Update(userInstance);
                    userInstance.order = i;
                }
                team.current_user_inst_id = userInstances[int.Max(0, _random.Next(userInstances.Count - 1))].user_inst_id;
            }
            await _dbContext.SaveChangesAsync();
        }

        public async Task AdvanceTurnOrder(Game game) {
            IEnumerable<Team> teams = _dbContext.Teams.Where(x => x.game_id == game.game_id);
            Team currentTeam = teams.First(x => x.team_id == game.current_team_id);
            Team nextTeam = teams.First(x => x.order == ((currentTeam.order + 1) % teams.Count()));
            _dbContext.Update(game);
            game.current_team_id = nextTeam.team_id;
            IEnumerable<UserInstance> userInstances = _dbContext.UserInstances.Where(x => x.team_id == nextTeam.team_id);
            UserInstance currentUserInstance = userInstances.First(x => x.user_inst_id == nextTeam.current_user_inst_id);
            UserInstance nextUserInstance = userInstances.First(x => x.order == ((currentUserInstance.order + 1) % userInstances.Count()));
            nextTeam.current_user_inst_id = nextUserInstance.user_inst_id;
            await _dbContext.SaveChangesAsync();
        }

        public async Task<CurrentTurn> GetCurrentTurn(Game game) {
            CurrentTurn currentTurn = new CurrentTurn();
            Team team = await _dbContext.Teams.FirstAsync(x => x.team_id == game.current_team_id);
            currentTurn.team_id = team.team_id;
            currentTurn.user_id = (await _dbContext.UserInstances.FirstAsync(x => x.user_inst_id == team.current_user_inst_id)).user_id;
            return currentTurn;
        }

        public async Task<TurnOrder> GetTurnOrder(Game game) {
            TurnOrder turnOrder = new TurnOrder();
            turnOrder.user_ids = new List<int>();
            List<Team> teams = await _dbContext.Teams.Where(x => x.game_id == game.game_id).OrderBy(x => x.order).ToListAsync();
            List<UserInstance> userInstances = await _dbContext.UserInstances.Where(x => x.game_id == game.game_id).OrderBy(x => x.order).ToListAsync();
            Team team = teams.First(x => x.team_id == game.current_team_id);
            UserInstance userInstance = userInstances.First(x => x.user_inst_id == team.current_user_inst_id);
            for (int i = 0; i < userInstances.Count; i++) {
                turnOrder.user_ids.Add(userInstance.user_id);
                int nextOrder = (userInstance.order + 1) % userInstances.Where(x => x.team_id == team.team_id).Count();
                team.current_user_inst_id = userInstances.First(x => x.team_id == team.team_id && x.order == nextOrder).user_inst_id;
                team = teams.First(x => x.order == ((team.order + 1) % teams.Count()));
                userInstance = userInstances.First(x => x.user_inst_id == team.current_user_inst_id);
            }
            return turnOrder;
        }

        public async Task RandomShuffleTeams(Game game) {
            List<UserInstance> userInstances = await _dbContext.UserInstances.Where(x => x.game_id == game.game_id).ToListAsync();
            List<Team> teams = await _dbContext.Teams.Where(x => x.game_id == game.game_id).ToListAsync();
            userInstances = userInstances.OrderBy(x => _random.Next()).ToList();
            _dbContext.UpdateRange(userInstances);
            int teamIndex = 0;
            foreach (UserInstance userInstance in userInstances) {
                Team team = teams[teamIndex];
                await PlayerTeamChange(team, userInstance);
                teamIndex = (teamIndex + 1) % teams.Count;
            }
        }

        public async Task<Scores> GetScores(Game game) {
            Scores scores = new Scores();

            var userScores = await _dbContext.UserInstances
                .Where(ui => ui.game_id == game.game_id)
                .GroupJoin(
                    _dbContext.Answers.Where(a => a.success),
                    ui => ui.user_inst_id,
                    a => a.user_inst_id,
                    (ui, answers) => new { ui.username, count = answers.Count() }
                )
                .OrderByDescending(x => x.count)
                .ToListAsync();

            var teamScores = await _dbContext.Teams
                .Where(t => t.game_id == game.game_id)
                .GroupJoin(
                    _dbContext.Answers.Where(a => a.success),
                    t => t.team_id,
                    a => a.team_id,
                    (t, answers) => new { t.team_name, count = answers.Count() }
                )
                .OrderByDescending(x => x.count)
                .ToListAsync();

            scores.teams = teamScores.Select(x => x.team_name);
            scores.teamScores = teamScores.Select(x => x.count);
            scores.players = userScores.Select(x => x.username);
            scores.playerScores = userScores.Select(x => x.count);

            return scores;
        }


        public async Task AdvanceRound(Game game) {
            _dbContext.Update(game);
            List<bool> enabledRounds = new List<bool> { game.round1, game.round2, game.round3, game.round4 };
            List<int> rounds = new List<int>();
            for (int i = 0; i < 4; i++) {
                if (enabledRounds[i]) {
                    rounds.Add(i + 1);
                }
            }
            int nextIndex = rounds.IndexOf(game.round) + 1;
            if (nextIndex >= rounds.Count) {
                game.stage = 5;
                game.is_active = false;
            } else {
                game.round = rounds[nextIndex];
            }
            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<Name>> GetNames(Game game) {
            return await _dbContext.Names
                .Where(n => n.game_id == game.game_id && _dbContext.Answers.Where(a => a.name_id == n.name_id && a.success && a.round == game.round).Count() == 0)
                .ToListAsync();
        }
        public async Task<Name> GetNameById(int name_id) {
            return await _dbContext.Names.FirstOrDefaultAsync(x => x.name_id == name_id);
        }

        public async Task InitAnswer(Game game, User user, Name name) {
            UserInstance userInstance = await _dbContext.UserInstances.FirstAsync(x => x.user_id == user.user_id && x.game_id == game.game_id);
            Answer answer = new Answer {
                game_id = game.game_id,
                user_inst_id = userInstance.user_inst_id,
                name_id = name.name_id,
                name = name.name,
                team_id = userInstance.team_id,
                round = game.round,
                time_start = DateTime.Now,
            };
            await _dbContext.Answers.AddAsync(answer);
            await _dbContext.SaveChangesAsync();
        }

        public async Task ScoreAnswer(Game game, User user, Name name, bool success) {
            UserInstance userInstance = await _dbContext.UserInstances.FirstAsync(x => x.user_id == user.user_id && x.game_id == game.game_id);
            Answer answer = await _dbContext.Answers.OrderByDescending(x => x.answer_id).FirstAsync(x => x.user_inst_id == userInstance.user_inst_id && x.name_id == name.name_id && x.round == game.round);
            _dbContext.Update(answer);
            answer.success = success;
            answer.time_finish = DateTime.Now;
            await _dbContext.SaveChangesAsync();
        }

        public async Task<Mp3Order> GetMp3Order() {
            Mp3Order mp3Order = await _dbContext.Mp3Order.FirstAsync();
            _dbContext.Update(mp3Order);
            mp3Order.current_stop = (mp3Order.current_stop + 1) % mp3Order.number_stops;
            mp3Order.current_start = (mp3Order.current_start + 1) % mp3Order.number_starts;
            await _dbContext.SaveChangesAsync();
            return mp3Order;
        }

        public async Task AddTurn(Game game, User user) {
            UserInstance userInstance = await _dbContext.UserInstances.FirstAsync(x => x.user_id == user.user_id && x.game_id == game.game_id);
            DateTime now = DateTime.Now;
            Turn turn = new Turn {
                user_inst_id = userInstance.user_inst_id,
                game_id = game.game_id,
                round = game.round,
                time_start = now,
                time_finish = now.AddSeconds(game.time_limit_sec),
                isActive = true,
            };
            _dbContext.Add(turn);
            await _dbContext.SaveChangesAsync();
        }

        public async Task EndTurn(Game game, User user) {
            UserInstance userInstance = await _dbContext.UserInstances.FirstAsync(x => x.user_id == user.user_id && x.game_id == game.game_id);
            Turn turn = await _dbContext.Turns.FirstOrDefaultAsync(x => x.user_inst_id == userInstance.user_inst_id && x.game_id == game.game_id && x.isActive);
            if (turn == null) {
                //logger.LogError($"No active turn found for user {user.username} in game {game.game_id}");
                return;
            }
            _dbContext.Update(turn);
            turn.isActive = false;
            turn.time_finish = DateTime.Now;
            await _dbContext.SaveChangesAsync();
        }

        public async Task<int> GetLastRound(Game game) {
            if (game.round4) { return 4; }
            if (game.round3) { return 3; }
            if (game.round2) { return 2; }
            if (game.round1) { return 1; }
            return 0;
        }

        public async Task<List<User>> GetUsers() { 
            return await _dbContext.Users.OrderByDescending(x=>x.last_login).Take(50).ToListAsync();
        }

        public async Task<User> GetUser(int user_id) {
            return await _dbContext.Users.FirstOrDefaultAsync(x => x.user_id == user_id);
        }

        public async Task<Team> GetTeam(int team_id) {
            return await _dbContext.Teams.FirstOrDefaultAsync(x => x.team_id == team_id);
        }

        //GetNameGameViewModel
        public async Task<NameGameViewModel> GetNameGameViewModel(Game game, User user) {
            NameGameViewModel model = new NameGameViewModel();
            model.User = user;
            model.Game = game;
            model.UserInstance = await GetOrAddUserInstance(user, game);
            model.Teams = (await GetTeams(game)).ToList();
            model.Players = await GetPlayers(game);
            model.TeamMembers = await GetTeamMembers(game);
            model.CurrentTurn = await GetCurrentTurn(game);
            model.TurnOrder = await GetTurnOrder(game);
            return model;
        }
    }
    /*
        public class EFLogicFactory {
            private readonly IServiceProvider _serviceProvider;

            public EFLogicFactory(IServiceProvider serviceProvider) {
                _serviceProvider = serviceProvider;
            }

            public EFLogic CreateEFLogic() {
                return new EFLogic(_serviceProvider.GetRequiredService<NameGameDbContext>());
            }
        }*/
}

