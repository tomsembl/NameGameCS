using System;
using NameGameCS.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace NameGameCS {

    public class DBLogic {
        private NameGameDbContext _dbContext;

        public DBLogic(NameGameDbContext dbContext) {
            _dbContext = dbContext;
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
                            (g.stage == 1 ||
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
    }
}

