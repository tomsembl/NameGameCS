function App() {
    var time_limit = @Html.Raw(Model.Game.time_limit_sec);
    const [state, setState] = React.useState({
        gameID: @Html.Raw(Model.Game.game_id),
        userID: @Html.Raw(Model.User.user_id),
        teamID: @Html.Raw(Model.UserInstance.team_id),
        myTurn: @Html.Raw(Model.User.user_id == Model.CurrentTurn.user_id),
        currentRound: @Html.Raw(Model.Game.round),
        currentPlayerID: @Html.Raw(Model.CurrentTurn.user_id),
        currentTeamID: @Html.Raw(Model.CurrentTurn.team_id),
        currentPlayer: @Html.Raw(Model.Players[Model.CurrentTurn.user_id]),
        players: @Html.Raw(Json.Serialize(Model.Players)),
        numPlayers: @Html.Raw(Model.Players.Count),
        teams: @Html.Raw(Json.Serialize(Model.Teams)),
        teammembers: @Html.Raw(Json.Serialize(Model.TeamMembers)),
        turnOrder: @Html.Raw(Json.Serialize(Model.TurnOrder)),
        turnStarted: false,
        doneButtonEnabled: true,
        whosUpVisible: true,
        startButtonEnabled: true,
        prevPhase: -1,
        phase: -1,
        toggleQueue: false,
    })
    const [countdown, setCountdown] = React.useState(time_limit)
    const [name, setName] = React.useState(null)
    const [previousName, setPreviousName] = React.useState(null)
    var current_name, previous_name, current_turn_user_id, current_team_id, teams, players, start_mp3, finish_mp3, teammembers, is_new_round, start_or_stop, stop_mp3, current_round
    const roundTexts = { 1: 'Round 1: \xa0Unlimited', 2: 'Round 2: \xa03 Words Max', 3: 'Round 3: \xa01 Word Only', 4: 'Round 4: \xa0Mime' }
    const nameRef = React.useRef()
    const con = React.useRef()
    const interval = React.useRef()
    const myTurnRef = React.useRef()
    const numPlayersRef = React.useRef()
    const audio1 = React.useRef(new Audio(`/static/start/0.mp3`))
    const audio2 = React.useRef(new Audio(`/static/stop/0.mp3`))
    const audio3 = React.useRef(new Audio(`/static/Ding2.mp3`))
    audio3.current.preservesPitch = false
    React.useEffect(() => { nameRef.current = name }, [name, state.myTurn])
    React.useEffect(() => { myTurnRef.current = state.myTurn }, [state.myTurn])
    React.useEffect(() => { numPlayersRef.current = state.numPlayers }, [state.numPlayers])

    const con = React.useRef()

    React.useEffect(() => {
        const newCon = new signalR.HubConnectionBuilder().withUrl("/SignalRHub").build();

        newCon.on("message", function (user, msg) {
            console.log(msg)
        });

        newCon.on('EmitTeams', function (user, msg) {
            setState(prevState => ({
                ...prevState,
                teams: JSON.parse(msg),
            }))
        })

        newCon.on('EmitTeamMembers', function (user, msg) {
            setState(prevState => ({
                ...prevState,
                teammembers: JSON.parse(msg),
            }))
        })

        newCon.on('EmitPlayers', function (user, msg) {
            const p = JSON.parse(msg)
            setState(prevState => ({
                ...prevState,
                players: p,
                numPlayers: p.length,
            }))
        })

        newCon.on('EmitCurrentTurn', function (user, msg) {
            const currentTurn = JSON.parse(msg)
            updateCurrentTurn(currentTurn.user_id, currentTurn.team_id)
        })

        newCon.on('EmitTurnOrder', function (user, msg) {
            setState(prevState => ({
                ...prevState,
                turnOrder: JSON.parse(msg),
            }))
        })

        newCon.on("EmitCurrentRound", function (user, msg) {
            location.href = `/graphs/${state.gameID}`
        })

        newCon.on('EmitAdvanceGame', function (user, msg) {
            location.href = `/join_game/${state.gameID}`;
        })

        newCon.on('EmitKickGroup', function (user, msg) {
            alert(msg)
            location.href = '/';
        })

        newCon.on('EmitNextName', function (user, msg) {
            const newName = JSON.parse(msg)
            setName(newName)
            nameRef.current = newName
        })

        newCon.on('EmitStartTimer', function (user, msg) {
            if (!myTurnRef.current) {
                console.log("this should only run for idle players", myTurnRef.current)
                startCountdown(false)
            }
        })

        newCon.on('EmitStopTimer', function (user, msg) {
            endCountdown()
        })

        newCon.on('EmitPreviousName', function (user, msg) {
            setPreviousName(msg)
            console.log
        })


        newCon.onclose(() => {
            console.log("Connection lost. Attempting to reconnect...");
            startConnection();
        });

        const startConnection = () => {
            newCon.start().then(() => {
                console.log("Connected");
            }).catch(err => {
                console.error('Error while establishing connection:', err);
                setTimeout(startConnection, 5000); // Retry after 5 seconds
            });
        };

        con.current = newCon;
        startConnection();

        return () => {
            if (con.current) {
                con.current.stop().then(() => console.log("Connection stopped."));
            }
        };

    }, [])

    function updateCurrentTurn(current_turn_user_id, current_team_id) {
        setState(prevState => {
            var current_player = prevState.players[current_turn_user_id]
            var my_turn = current_turn_user_id === prevState.userID
            var my_teams_turn = current_team_id === prevState.teamID
            var new_phase = getPhaseNumber(current_player, my_teams_turn, my_turn)
            return {
                ...prevState,
                currentPlayerID: current_turn_user_id,
                currentTeamID: current_team_id,
                currentPlayer: current_player,
                myTurn: my_turn,
                prevPhase: prevState.phase == new_phase ? prevState.prevPhase : prevState.phase, //this log is to handle the case of mulitple runs of this function, which would accidentally flush prevPhase
                phase: new_phase
            }
        })
    }

    function endCountdown() {
        clearInterval(interval.current)
        setCountdown(time_limit)
    }

    function score_answer(name_id, is_success) {
        fetch(`/score_answer?name_id=${name_id}&is_success=${is_success}`, { method: "POST" })
    }

    function getMp3s() {
        con.current.invoke("EmitMp3Order", (result) => {
            const mp3Order = JSON.parse(result)
            audio1.current = new Audio(`/static/${start}/${mp3Order.current_start}.mp3`)
            audio2.current = new Audio(`/static/${stop}/${mp3Order.current_stop}.mp3`)
        })
    }

    function your_turn() {
        getMp3s()
        endCountdown()
        setState(prevState => ({
            ...prevState,
            whosUpVisible: false,
            startButtonEnabled: true,
        }))
    }

    React.useEffect(() => {
        if (state.myTurn) {
            your_turn()
        }
    }, [state.myTurn])

    function decrementCountdown(is_player = true) {
        setCountdown((prev) => {
            if (prev === 0) {
                endCountdown();
                if (is_player) {
                    end_turn()
                }
                return null
            }
            return prev - 1;
        });
    }

    function startCountdown(is_player = true) {
        if (interval.current) { endCountdown() }//handles infinite loop issue
        interval.current = setInterval(() => {
            decrementCountdown(is_player);
        }, 1000);
    };

    function start() {
        setState(prevState => ({
            ...prevState,
            startButtonEnabled: false,
        }))
        audio1.current.addEventListener('ended', startAfterAudio, { capture: true, once: true });
        audio1.current.play()
    }

    const startAfterAudio = React.useCallback(() => {
        audio1.current.removeEventListener('ended', startAfterAudio, { capture: true, once: true })
        fetch(`/next_name`, { method: "POST" })
        fetch(`/start_timer`, { method: "POST" })
        startCountdown()
        setState(prevState => ({
            ...prevState,
            turnStarted: true,
            startButtonEnabled: true,
        }))
    })

    function ding() {
        const semitone = 1.0594630943592953
        audio3.current.pause(); audio3.current.currentTime = 0
        audio3.current.playbackRate = audio3.current.playbackRate > 4 ? 1 : audio3.current.playbackRate * semitone
        audio3.current.play()
    }

    function done() {
        ding()
        document.getElementById("done_button").blur()
        score_answer(nameRef.current.name_id, true)
        fetch(`/next_name`, { method: "POST" })
        setState(prevState => ({
            ...prevState,
            doneButtonEnabled: false,
        }))
        setTimeout(() => {
            setState(prevState => ({
                ...prevState,
                doneButtonEnabled: true,
            })
            )
        }, 500)
    }

    function skipName() {
        score_answer(nameRef.current.name_id, false)
        fetch(`/next_name`, { method: "POST" })
        audio3.current.play()
    }

    function end_turn(names_ran_out = false) {
        audio2.current.play()
        score_answer(nameRef.current.name_id, false)
        setState(prevState => ({
            ...prevState,
            turnStarted: false,
            myTurn: false,
        }))
        if (!names_ran_out) {
            fetch(`/advance_turn`, { method: "POST" })
        }
    }

    function concede(names_ran_out = false) {
        fetch(`/stop_timer`, { method: "POST" })
        endCountdown()
        end_turn(names_ran_out)
        if (!names_ran_out) { console.log("conceded") }
    }

    function getPhaseNumber(currentPlayer, myTeamsTurn, myTurn) {
        if (currentPlayer === null) { return -1 }
        return myTeamsTurn ? myTurn ? 0 : 1 : 2
    }

    function getTextForTheName() {
        if (state.turnStarted & name != null) {
            return name.name
        }
        switch (state.phase) {
            case 0: return "Your Turn"
            case 1: return state.currentPlayer + "'s Turn\nYou're Guessing"
            case 2: return state.currentPlayer + "'s Turn"
            default: return ""
        }
    }

    function phaseTranslate(phase) {
        switch (phase) {
            case 0: return "your"
            case 1: return "team"
            default: return "not"
        }
    }

    function getBackground() {
        var pp = state.prevPhase
        const p = state.phase
        if (pp == -1 & p == 2) {
            pp = 0
        }
        return `animate-${phaseTranslate(pp)}-${phaseTranslate(p)}`
    }

    const toggleQueue = (
        <span>
            <div class="flex-v center vh100">
                <label class="switch2 switch-heading">
                    <input type="checkbox" id='q' name="q" checked={state.toggleQueue} onClick={() => {
                        setState(prevState => ({
                            ...prevState,
                            toggleQueue: !prevState.toggleQueue,
                        }))
                    }} /><span class="slider2 round"></span>
                </label>
            </div>
        </span>
    )

    return (
        <body id="body" class={`lobby2 flex-v rainbow-gradient-bg ${getBackground()}`}>
            {!state.myTurn && <h1 id="heading" class="heading subheading3">THE NAME GAME</h1>}

            <div class="flex-v">
                <h1 id="round" class="heading subheading4">
                    {roundTexts[state.currentRound]}
                </h1>
            </div>

            <div class="flex-v">
                <h1 id="the_name" class={`heading subheading pad-10pct-v name-big${state.myTurn ? "-big" : ""}`}>{getTextForTheName()}</h1>
                {(!state.myTurn && previousName != null) && <h1 id="last_guessed_name" class="heading subheading4">{`Previous Name: ${previousName}`}</h1>}
                {state.myTurn && <button id="done_button" class={`button1 button3 button-green ${state.doneButtonEnabled ? "" : "correct"} ${state.turnStarted ? "" : "invisible"} mar-b20`} onClick={done} disabled={!state.doneButtonEnabled}>THEY GOT IT</button>}
            </div>

            <h1 class="heading subheading4 flex-h">
                <div class="flex-h center vw50">
                    ⏱
                    <span id="countdown">{countdown}</span>
                </div>
                <div class="flex-h center vw50">
                    More Info
                    {toggleQueue}
                </div>
            </h1>
            {state.toggleQueue && state.turnOrder && <div class="flex-h">
                <div class="flex-v-start vw50">
                    <h1 class="heading subheading4">Players</h1>
                    {state.turnOrder.map((player_id, i) => {
                        return (
                            <div class={`white text-centered option small-option slight-v-gradient p-0 ${i === 0 ? "my" : state.teammembers[state.currentTeamID].find(x => x === player_id) ? "team" : "not"}`} id="current_player">
                                {player_id === state.userID ? (<strong>You</strong>) : state.players[player_id]}
                            </div>
                        )
                    })}
                </div>
                <div class="flex-v-start vw50">
                    <h1 class="heading subheading4">My Team</h1>
                    {state.teammembers[state.teamID].map((teammate) => {
                        if (teammate !== state.userID) {
                            return (
                                <div class="white text-centered option small-option slight-v-gradient team p-0" id="current_player">
                                    {state.players[teammate]}
                                </div>
                            )
                        }
                    })}
                </div>
            </div>}
            {state.myTurn && <div class="flex-h mar-1em">
                <button id="start_button" class="button1 button3" onClick={start} disabled={!state.myTurn | state.turnStarted | !state.startButtonEnabled}>Start</button>
                <button id="concede_button" class="button1 button3" onClick={() => { concede(false) }} disabled={!state.myTurn | !state.turnStarted}>End</button>
            </div>}
            <div class="flex-h mar-1em">
                <button class="button2" onClick={() => { location.href = '/'; }}>Home</button>
                {state.myTurn && <button id="skip_button" class="button2" onClick={() => { skipName() }}>Skip</button>}
            </div>
        </body>
    )
}
ReactDOM.render(<App />, document.querySelector("#app"));