document.addEventListener("DOMContentLoaded", function () {
    var connection = new signalR.HubConnectionBuilder()
        .withUrl("/chatHub")
        .build();

    connection.on("ReceiveMessage", function (message) {
        var li = document.createElement("li");
        document.getElementById("messagesList").appendChild(li);
        li.textContent = `Bot: ${message}`;
    });

    connection.start().then(function () {
        console.log("SignalR Connected");

        document.getElementById("sendButton").addEventListener("click", function (event) {
            var message = document.getElementById("inputText").value;

            if (!message) {
                alert("Message cannot be empty");
                return;
            }

            var connectionId = connection.connectionId; 

            var formattedMessage = `${connectionId}:${message}`;

            var li = document.createElement("li");
            document.getElementById("messagesList").appendChild(li);
            li.textContent = `User: ${message}`;

            connection.invoke("SendMessageToQueue", formattedMessage).catch(function (err) {
                return console.error(err.toString());
            });

            document.getElementById("inputText").value = "";

            event.preventDefault();
        });
    }).catch(function (err) {
        return console.error(err.toString());
    });
});
