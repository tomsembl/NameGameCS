    const connection = new signalR.HubConnectionBuilder().withUrl("/SignalRHub").build();

    connection.on("ReceiveMessage", function (user, message) {
        console.log(`${user}: ${message}`);
    });

    connection.start().catch(function (err) {
        return console.error(err.toString());
    });

    // Example of sending a message
    document.getElementById("sendButton").addEventListener("click", function (event) {
        var user = document.getElementById("userInput").value;
        var message = document.getElementById("messageInput").value;
        connection.invoke("SendMessage", user, message).catch(function (err) {
            return console.error(err.toString());
        });
        event.preventDefault();
    });
     