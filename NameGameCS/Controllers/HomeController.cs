using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Primitives;
using NameGameCS.Models;
using NameGameCS.Filters;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace NameGameCS {
    [ServiceFilter(typeof(UserAuthenticationFilter))]
    public class HomeController : Controller {
        //private readonly NameGameDbContext _dbContext;
        private readonly EFLogic _efLogic;
        private readonly IHubContext<SignalRHub> _hubContext;
        private readonly OutboundSignalR _outboundSignalR;
        private readonly CookieOptions _cookieOptions = new CookieOptions() { MaxAge = TimeSpan.FromDays(400) };

        public HomeController(
            //NameGameDbContext dbContext,
            EFLogic efLogic,
            IHubContext<SignalRHub> hubContext,
            OutboundSignalR outboundSignalR
        ) {
            //_dbContext = dbContext;
            _efLogic = efLogic;
            _hubContext = hubContext;
            _outboundSignalR = outboundSignalR;
        }

        public void log(string s) {
            Console.WriteLine(s);
        }
        public void log(Exception e) {
            Console.WriteLine(e.Message);
        }

        [HttpGet]
        [SkipUserAuthentication]
        public async Task<IActionResult> Home() {
            User user = _efLogic.getUserFromCookie(Request);
            user = await _efLogic.SignIn(user: user);
            Response.Cookies.Append("user_id", $"{user.user_id}", _cookieOptions);
            Response.Cookies.Append("username", $"{user.username}", _cookieOptions);
            return View("home");
        }

        [HttpGet]
        [Route("/create_game")]
        public async Task<IActionResult> create_game() {
            return View("create-game");
        }

        [HttpGet]
        [Route("/join_game")]
        [Route("/join_game/{game_id:int}")]
        public async Task<IActionResult> join_game(int? game_id) {
            if (game_id != null) {
                Game game = await _efLogic.GetGame(game_id.Value);
                try {
                    string endpoint = new Dictionary<int, string> {
                        {1, "lobby"},
                        {2, "write_names"},
                        {3, "pick_teams"},
                        {4, "name_game"},
                        {5, "graphs"},
                    }[game.stage];
                    Response.Cookies.Append("game_id", game.game_id.ToString(), _cookieOptions);
                    return Redirect($"/{endpoint}/{game.game_id}");
                } catch (Exception e) {
                    log(e);
                    return Redirect("/join_game");
                }
            }
            User user = _efLogic.getUserFromCookie(Request);
            JoinGameViewModel model = new JoinGameViewModel();
            model.User = user;
            model.Games = await _efLogic.GetJoinableGamesByUser(user);
            Response.Cookies.Append("game_id", "_join", _cookieOptions);
            return View("join-game", model);
        }

        [HttpGet]
        [Route("/lobby/{game_id:int}")]
        public async Task<IActionResult> lobby(int game_id) {
            LobbyViewModel model = new LobbyViewModel();
            model.User = _efLogic.getUserFromCookie(Request); ;
            model.Game = await _efLogic.GetGame(game_id);
            if (model.Game == null) return Redirect("/join_game");
            model.UserInstance = await _efLogic.GetOrAddUserInstance(model.User, model.Game);
            _outboundSignalR.EmitPlayers("lobby()", model.Game);
            model.Players = await _efLogic.GetPlayers(model.Game);
            Response.Cookies.Append("game_id", $"{game_id}", _cookieOptions);
            return View("lobby", model);
        }

        [HttpGet]
        [Route("/write_names/{game_id:int}")]
        public async Task<IActionResult> write_names(int game_id) {
            WriteNamesViewModel model = new WriteNamesViewModel();
            model.Game = await _efLogic.GetGame(game_id);
            if (model.Game == null) return Redirect("/join_game");
            return View("write-names", model);
        }

        [HttpGet]
        [Route("/pick_teams/{game_id:int}")]
        public async Task<IActionResult> pick_teams(int game_id) {
            PickTeamsViewModel model = new PickTeamsViewModel();
            model.User = _efLogic.getUserFromCookie(Request);
            model.Game = await _efLogic.GetGame(game_id);
            if (model.Game == null) return Redirect("/join_game");
            model.UserInstance = await _efLogic.GetOrAddUserInstance(model.User, model.Game);
            model.Teams = (await _efLogic.GetTeams(model.Game)).ToList();
            model.Players = await _efLogic.GetPlayers(model.Game);
            model.TeamMembers = await _efLogic.GetTeamMembers(model.Game);
            return View("pick-teams", model);
        }

        [HttpGet]
        [Route("/other")]
        public IActionResult other() {
            return View("other");
        }

        [HttpGet]
        [Route("/readme")]
        public IActionResult readme() {
            return View("readme");
        }

        [HttpGet]
        [Route("/change_user")]
        public async Task<IActionResult> change_user() {
            ChangeUserViewModel model = new ChangeUserViewModel();
            model.User = _efLogic.getUserFromCookie(Request);
            model.Users = await _efLogic.GetUsers();
            return View("change-user", model);
        }

        [HttpGet]
        [Route("/graphs/{game_id:int}")]
        public async Task<IActionResult> graphs(int game_id) {
            GraphsViewModel model = new GraphsViewModel();
            model.User = _efLogic.getUserFromCookie(Request);
            model.Game = await _efLogic.GetGame(game_id);
            if (model.Game == null) return Redirect("/join_game");
            model.Scores = await _efLogic.GetScores(model.Game);
            Response.Cookies.Append("game_id", $"{game_id}", _cookieOptions);
            return View("graphs", model);
        }

        [HttpGet]
        [Route("/name_game/{game_id:int}")]
        public async Task<IActionResult> name_game(int game_id) {
            NameGameViewModel model = new NameGameViewModel();
            model.User = _efLogic.getUserFromCookie(Request);
            model.Game = await _efLogic.GetGame(game_id);
            if (model.Game == null) return Redirect("/join_game");
            model.UserInstance = await _efLogic.GetOrAddUserInstance(model.User, model.Game);
            model.Teams = (await _efLogic.GetTeams(model.Game)).ToList();
            model.Players = await _efLogic.GetPlayers(model.Game);
            model.TeamMembers = await _efLogic.GetTeamMembers(model.Game);
            model.CurrentTurn = await _efLogic.GetCurrentTurn(model.Game);
            model.TurnOrder = await _efLogic.GetTurnOrder(model.Game);
            return View("name-game", model);
        }

        [HttpPost]
        [Route("/add_game")]
        public async Task<IActionResult> add_game() {
            var formData = await Request.ReadFormAsync();
            Game game = await _efLogic.CreateGame(formData);
            await _efLogic.CreateTeams(game);
            await _outboundSignalR.EmitGamesListChanged("add_game()", "");
            return Redirect("/join_game");
        }

        [HttpPost]
        [Route("/delete_game")]
        public async Task<IActionResult> delete_game(int game_id) {
            User user = _efLogic.getUserFromCookie(Request);
            Game game = await _efLogic.GetGame(game_id);
            await _efLogic.DeleteGame(game);
            await _outboundSignalR.EmitKickGroup(user.username, $"game{game_id}");
            await _outboundSignalR.EmitGamesListChanged("delete_game()", "");
            return new StatusCodeResult(200);
        }

        [HttpPost]
        [Route("/kick_user")]
        public async Task<IActionResult> kick_user(int user_id) {
            Game game = await _efLogic.getGameFromCookie(Request);
            User user = _efLogic.getUserFromCookie(Request);
            User kickUser = await _efLogic.GetUser(user_id);
            UserInstance userInstance = await _efLogic.GetOrAddUserInstance(kickUser, game);
            await _efLogic.KickUserInstance(userInstance);
            await _outboundSignalR.EmitKickGroup(user.username, $"user{user_id}");
            await _outboundSignalR.EmitPlayers("kick_user()", game);
            return new StatusCodeResult(200);
        }

        [HttpPost]
        [Route("/username_change")]
        public async Task<IActionResult> username_change(string newUsername) {
            User user = _efLogic.getUserFromCookie(Request);
            Game game = await _efLogic.getGameFromCookie(Request);
            UserInstance userInstance = await _efLogic.GetOrAddUserInstance(user, game);
            await _efLogic.UpdateUsername(user, userInstance, newUsername);
            Response.Cookies.Append("username", $"{newUsername}", _cookieOptions);
            await _outboundSignalR.EmitPlayers("username_change()", game);
            return new StatusCodeResult(200);
        }

        [HttpPost]
        [Route("/advance_game")]
        public async Task<IActionResult> advance_game(int stage) {
            User user = _efLogic.getUserFromCookie(Request);
            Game game = await _efLogic.getGameFromCookie(Request);
            await _efLogic.AdvanceGame(game, stage);
            if (stage == (int)GameStage.name_game) {
                await _efLogic.InitPlayerTurnOrder(game);
            }
            await _outboundSignalR.EmitAdvanceGame(user.username, $"game{game.game_id}");
            return new StatusCodeResult(200);
        }

        [HttpPost]
        [Route("/submit_names")]
        public async Task<IActionResult> submit_names() {
            string json = await new StreamReader(Request.Body).ReadToEndAsync();
            List<Name> names = JsonConvert.DeserializeObject<List<Name>>(json);
            User user = _efLogic.getUserFromCookie(Request);
            Game game = await _efLogic.getGameFromCookie(Request);
            UserInstance userInstance = await _efLogic.GetOrAddUserInstance(user, game);
            if (await _efLogic.GetUserHasSubmittedNames(userInstance)) {
                return new ContentResult { Content = $"You've already submitted names", StatusCode = 400, ContentType = "text/plain" };
            }
            await _efLogic.AddNames(userInstance, names);
            await _outboundSignalR.EmitWaitingOn("submit_names()", game);
            return new ContentResult { Content = "", StatusCode = 200, ContentType = "text/plain" };
        }

        [HttpPost]
        [Route("/team_name_change")]
        public async Task<IActionResult> team_name_change(int teamId, string newTeamName) {
            User user = _efLogic.getUserFromCookie(Request);
            Game game = await _efLogic.getGameFromCookie(Request);
            Team team = await _efLogic.GetTeam(teamId);
            await _efLogic.ChangeTeamName(team, newTeamName);
            await _outboundSignalR.EmitTeams("team_name_change()", game);
            return new ContentResult { Content = "", StatusCode = 200, ContentType = "text/plain" };
        }

        [HttpPost]
        [Route("/player_team_change")]
        public async Task<IActionResult> player_team_change(int teamId, int userId) {
            Game game = await _efLogic.getGameFromCookie(Request);
            User user = await _efLogic.GetUser(userId);
            UserInstance userInstance = await _efLogic.GetOrAddUserInstance(user, game);
            Team team = await _efLogic.GetTeam(teamId);
            await _efLogic.PlayerTeamChange(team, userInstance);
            await _outboundSignalR.EmitTeamMembers("player_team_change()", game);
            return new StatusCodeResult(200);
        }

        [HttpPost]
        [Route("/random_shuffle_teams")]
        public async Task<IActionResult> random_shuffle_teams() {
            Game game = await _efLogic.getGameFromCookie(Request);
            await _efLogic.RandomShuffleTeams(game);
            await _outboundSignalR.EmitTeamMembers("random_shuffle_teams()", game);
            return new StatusCodeResult(200);
        }

        /*[HttpPost]
        [Route("/advance_round")]
        public async Task<IActionResult> advance_round() {
            Game game = await _efLogic.getGameFromCookie(Request);
            await _efLogic.AdvanceRound(game);
            await _outboundSignalR.EmitCurrentRound("advance_round()", game);
            return new StatusCodeResult(200);
        }*/

        [HttpPost]
        [Route("/next_name")]
        public async Task<IActionResult> next_name() {
            User user = _efLogic.getUserFromCookie(Request);
            Game game = await _efLogic.getGameFromCookie(Request);
            await _outboundSignalR.EmitNextName(game, user);
            return new StatusCodeResult(200);
        }

        [HttpPost]
        [Route("/score_answer")]
        public async Task<IActionResult> score_answer(int name_id, bool is_success, bool is_skip) {
            User user = _efLogic.getUserFromCookie(Request);
            Game game = await _efLogic.getGameFromCookie(Request);
            Name name = await _efLogic.GetNameById(name_id);
            Console.WriteLine($"score_answer: {name.name} {is_success}");
            await _efLogic.ScoreAnswer(game, user, name, is_success);
            await _outboundSignalR.EmitScoreAnswer("score_answer", game, user, name, is_success, is_skip);
            return new StatusCodeResult(200);
        }

        [HttpPost]
        [Route("/start_timer")]
        public async Task<IActionResult> start_timer() {
            User user = _efLogic.getUserFromCookie(Request);
            Game game = await _efLogic.getGameFromCookie(Request);
            await _efLogic.AddTurn(game, user);
            await _outboundSignalR.EmitStartTimer("next_name()", game);
            return new StatusCodeResult(200);
        }

        [HttpPost]
        [Route("/stop_timer")]
        public async Task<IActionResult> stop_timer() {
            User user = _efLogic.getUserFromCookie(Request);
            Game game = await _efLogic.getGameFromCookie(Request);
            await _efLogic.EndTurn(game, user);
            await _outboundSignalR.EmitStopTimer("stop_timer()", game);
            return new StatusCodeResult(200);
        }

        [HttpPost]
        [Route("/advance_turn")]
        public async Task<IActionResult> advance_turn() {
            Game game = await _efLogic.getGameFromCookie(Request);
            await _outboundSignalR.AdvanceTurnOrder("advance_turn()", game);
            return new StatusCodeResult(200);
        }
        /*
                public IActionResult Privacy() {
                    return View();
                }

                [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
                public IActionResult Error() {
                    return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
                }*/
    }
}
