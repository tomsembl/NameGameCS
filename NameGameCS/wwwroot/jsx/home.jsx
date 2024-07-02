function App() {
    //State
    const [state, setState] = React.useState({
        test: "1"
    })

    //SignalR
    const signalRCon = React.useRef()
    React.useEffect(() => {
        const connection = new signalR.HubConnectionBuilder().withUrl("/SignalRHub").build();

        connection.on("ReceiveMessage", function (user, message) {
            console.log(`${user}: ${message}`);
        });

        connection.start().catch(function (err) {
            return console.error(err.toString());
        });

        signalRCon.current = connection

    }, [])

    //Logic

    //React Components

    //React Render
    return (
        <div>
            {state.test}
        </div>
    )
}
ReactDOM.render(<App />, document.querySelector("#app"));