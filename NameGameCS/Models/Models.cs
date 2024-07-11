using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NameGameCS.Models {

    public class User {
        [Key]
        public int user_id { get; set; }
        [StringLength(40)]
        public string username { get; set; }
        public DateTime created { get; set; }
        public DateTime last_login { get; set; }
        [StringLength(30)]
        public string last_ip { get; set; }
    }



    public class UserInstance {
        [Key]
        public int user_inst_id { get; set; }
        [ForeignKey("User")]
        public int user_id { get; set; }
        [StringLength(40)]
        public required string username { get; set; }
        [ForeignKey("Team")]
        public int team_id { get; set; }
        [ForeignKey("Game")]
        public int game_id { get; set; }
        public int order { get; set; }
    }

    public class Game {
        [Key]
        public int game_id { get; set; }
        [StringLength(40)]
        public string game_name { get; set; }
        public bool is_active { get; set; } = true;
        public int stage { get; set; } = 1;
        public int round { get; set; } = 1;
        public int number_teams { get; set; }
        public int number_names { get; set; }
        public int time_limit_sec { get; set; }
        public bool round1 { get; set; }
        public bool round2 { get; set; }
        public bool round3 { get; set; }
        public bool round4 { get; set; }
        [ForeignKey("Team")]
        public int current_team_id { get; set; }
        public DateTime date_created { get; set; } = DateTime.Now;
    }

    public class Team {
        [Key]
        public int team_id { get; set; }
        [ForeignKey("Game")]
        public int game_id { get; set; }
        [StringLength(40)]
        public string team_name { get; set; }
        public int order { get; set; }
        [ForeignKey("UserInstance")]
        public int current_user_inst_id { get; set; }
    }

    public class Name {
        [Key]
        public int name_id { get; set; }
        [ForeignKey("Game")]
        public int game_id { get; set; }
        [ForeignKey("UserInstance")]
        public int user_inst_id { get; set; }
        [StringLength(40)]
        public required string name { get; set; }
    }

    public class DefaultName {
        [Key]
        public int default_name_id { get; set; }
        [StringLength(40)]
        public required string name { get; set; }
    }

    public class Turn {
        [Key]
        public int turn_id { get; set; }
        public int user_inst_id { get; set; }
        [ForeignKey("Game")]
        public int game_id { get; set; }
        public int round { get; set; }
        public DateTime time_start { get; set; }
        public DateTime time_finish { get; set; }
        public bool isActive { get; set; }
    }

    public class Answer {
        [Key]
        public int answer_id { get; set; }
        [ForeignKey("Game")]
        public int game_id { get; set; }
        [ForeignKey("Team")]
        public int team_id { get; set; }
        [ForeignKey("UserInstance")]
        public int user_inst_id { get; set; }
        [ForeignKey("Name")]
        public int name_id { get; set; }
        [StringLength(40)]
        public string name { get; set; }
        public bool success { get; set; }
        public int round { get; set; }
        public DateTime time_start { get; set; }
        public DateTime time_finish { get; set; }
    }

    public class Mp3Order {
        [Key]
        public int id { get; set; }
        public int number_stops { get; set; }
        public int current_stop { get; set; }
        public int number_starts { get; set; }
        public int current_start { get; set; }
    }

    public class Scores { 
        public IEnumerable<string> teams { get; set; }
        public IEnumerable<int> teamScores { get; set; }
        public IEnumerable<string> players { get; set; }
        public IEnumerable<int> playerScores { get; set; }
    }

    public enum GameStage {
        lobby = 1,
        write_names = 2,
        pick_teams = 3,
        name_game = 4,
        graphs = 5,
    }

    public class CurrentTurn { 
        public int team_id { get; set; }
        public int user_id { get; set; }
    }

    public class TurnOrder { 
        public List<int> user_ids { get; set; }
    }
}