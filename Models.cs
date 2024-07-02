using System;

namespace NameGameCS.Models {

    public class User {
        public int user_id { get; set; }
        public string username { get; set; }
        public string last_ip { get; set; }
        public DateTime first_login { get; set; }
        public DateTime last_login { get; set; }
    }

    public class UserInstance {
        public int user_inst_id { get; set; }
        public int user_id { get; set; }
        public string username { get; set; }
        public int team_id { get; set; }
        public int game_id { get; set; }
        public int order { get; set; }
    }

    public class Game {
        public int game_id { get; set; }
        public string game_name { get; set; }
        public bool is_active { get; set; }
        public int stage { get; set; }
        public int round { get; set; }
        public int number_teams { get; set; }
        public int number_names { get; set; }
        public int time_limit_sec { get; set; }
        public bool round1 { get; set; }
        public bool round2 { get; set; }
        public bool round3 { get; set; }
        public bool round4 { get; set; }
        public DateTime date_created { get; set; }
    }

    public class Team {
        public int team_id { get; set; }
        public int game_id { get; set; }
        public string team_name { get; set; }
        public int order { get; set; }
    }

    public class Name {
        public int name_id { get; set; }
        public int game_id { get; set; }
        public int user_inst_id { get; set; }
        public string name { get; set; }
    }

    public class DefaultName {
        public int name_id { get; set; }
        public string name { get; set; }
    }

    public class Turn {
        public int turn_id { get; set; }
        public int user_inst_id { get; set; }
        public int game_id { get; set; }
        public int round { get; set; }
        public DateTime time_start { get; set; }
        public DateTime time_finish { get; set; }
        public bool isActive { get; set; }
    }

    public class Answer {
        public int answer_id { get; set; }
        public int game_id { get; set; }
        public int team_id { get; set; }
        public int user_inst_id { get; set; }
        public int name_id { get; set; }
        public string name { get; set; }
        public bool success { get; set; }
        public int round { get; set; }
        public DateTime time_start { get; set; }
        public DateTime time_finish { get; set; }
    }

    public class Mp3Order {
        public int number_stops { get; set; }
        public int current_stop { get; set; }
        public int number_starts { get; set; }
        public int current_start { get; set; }
    }
}