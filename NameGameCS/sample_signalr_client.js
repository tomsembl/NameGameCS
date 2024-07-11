    const connection = new signalR.HubConnectionBuilder().withUrl("/SignalRHub").build();

    connection.on("ReceiveMessage", function (user, message) {
        console.log(`${user}: ${message}`);
    });

    newConnewCon.onclose(() => {
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


    // Example of sending a message
    document.getElementById("sendButton").addEventListener("click", function (event) {
        var user = document.getElementById("userInput").value;
        var message = document.getElementById("messageInput").value;
        connection.invoke("SendMessage", user, message).catch(function (err) {
            return console.error(err.toString());
        });
        event.preventDefault();
    });
     