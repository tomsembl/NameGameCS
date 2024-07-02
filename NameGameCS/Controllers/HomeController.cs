using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using NameGameCS.Models;
using NameGameCS.Filters;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;

namespace NameGameCS {
    [ServiceFilter(typeof(UserAuthenticationFilter))]
    public class HomeController : Controller {
        private readonly NameGameDbContext _dbContext;
        private readonly DBLogic _dbLogic;
        private readonly SignalRHub _signalRHub;

        public HomeController(
            NameGameDbContext dbContext,
            SignalRHub signalRHub,
            DBLogic dbLogic
        ) {
            _dbContext = dbContext;
            _dbLogic = dbLogic;
            _signalRHub = signalRHub;
        }

        public void log(string s) {
            Console.WriteLine(s);
        }
        public void log(Exception e) {
            Console.WriteLine(e.Message);
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

        [HttpGet]
        [SkipUserAuthentication]
        public async Task<IActionResult> Home() {
            User user = getUserFromCookie(Request);
            user = await _dbLogic.SignIn(user: user);
            Response.Cookies.Append("user_id", $"{user.user_id}");
            Response.Cookies.Append("username", $"{user.username}");
            return View("home");
        }

        [HttpGet]
        [Route("/create_game")]
        public async Task<IActionResult> create_game() {
            return View("create-game");
        }

        [HttpPost]
        [Route("/add_game")]
        public async Task<IActionResult> add_game() {
            var formData = await Request.ReadFormAsync();
            Game game = await _dbLogic.CreateGame(formData);
            await _dbLogic.CreateTeams(game);
            await _signalRHub.RefreshJoinGame("add_game()","");
            return Redirect("/join_game");
        }

        [HttpGet]
        [Route("/join_game")]
        [Route("/join_game/{game_id:int}")]
        public async Task<IActionResult> join_game(int? game_id) {
            if (game_id != null) {
                Game game = await _dbContext.Games.FirstOrDefaultAsync(x => x.game_id == game_id);
                try {
                    string endpoint = new Dictionary<int, string> {
                        {1, "lobby"},
                        {2, "write_names"},
                        {3, "pick_teams"},
                        {4, "name_game"},
                        {5, "graphs"},
                    }[game.stage];
                    return Redirect($"/{endpoint}/{game.game_id}");
                } catch (Exception e) {
                    log(e);
                    return Redirect("/join_game");
                }
            }
            User user = getUserFromCookie(Request);
            JoinGameViewModel model = new JoinGameViewModel();
            model.User = user;
            model.Games = await _dbLogic.GetJoinableGamesByUser(user);
            Response.Cookies.Append("game_id", "join_game");
            return View("join-game", model);
        }

        [HttpGet]
        [Route("/lobby/{game_id:int}")]
        public async Task<IActionResult> lobby(int game_id) {
            User user = getUserFromCookie(Request);
            Game game = await _dbContext.Games.FirstOrDefaultAsync(x => x.game_id == game_id);
            if (game == null) return Redirect("/join_game");
            UserInstance userInstance = await _dbLogic.GetOrAddUserInstance(user, game);
            LobbyViewModel model = new LobbyViewModel();
            return View("lobby", model);
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
