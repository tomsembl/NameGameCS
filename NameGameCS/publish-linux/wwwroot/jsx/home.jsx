function App() {
    //State
    const [state, setState] = React.useState({
        test: "1"
    })

    //SignalR
    const con = React.useRef()
    React.useEffect(() => {
        const newCon = new signalR.HubConnectionBuilder().withUrl("/SignalRHub").build();

        newCon.on("ReceiveMessage", function (user, message) {
            console.log(`${user}: ${message}`);
        });

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