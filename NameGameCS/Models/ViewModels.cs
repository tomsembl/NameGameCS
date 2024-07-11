namespace NameGameCS.Models {
    public class JoinGameViewModel {
        public IEnumerable<Game> Games;
        public User User;
    }

    public class LobbyViewModel {
        public User User;
        public Game Game;
        public UserInstance UserInstance;
        public Dictionary<int,string> Players;
    }

    public class WriteNamesViewModel {
        public Game Game;
    }

    public class PickTeamsViewModel {
        public User User;
        public Game Game;
        public UserInstance UserInstance;
        public Dictionary<int, string> Players;
        public Dictionary<int, int[]> TeamMembers;
        public List<Team> Teams;
    }

    public class ChangeUserViewModel {
        public User User;
        public IEnumerable<User> Users;
    }

    public class GraphsViewModel {
        public Game Game;
        public User User;
        public Scores Scores;
    }

    public class NameGameViewModel {
        public User User;
        public Game Game;
        public UserInstance UserInstance;
        public Dictionary<int, string> Players;
        public Dictionary<int, int[]> TeamMembers;
        public List<Team> Teams;
        public CurrentTurn CurrentTurn;
        public TurnOrder TurnOrder;
    }
}
