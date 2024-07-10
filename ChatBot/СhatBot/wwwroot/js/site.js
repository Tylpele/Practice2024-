document.addEventListener("DOMContentLoaded", function () {
    var connection = new signalR.HubConnectionBuilder()
        .withUrl("/chatHub")
        .build();

    connection.on("SwitchErrorScreens", function (toError) {
        if (toError) {
            window.location.href = "/Home/Error";
        } else if (window.location.href !== 'https://localhost:7045/') {
            window.location.href = "/";
        }
    });

    connection.on("ReceiveMessage", function (message) {
        var li = document.createElement("li");
        var timestamp = new Date().toLocaleTimeString();
        li.innerHTML = `<span class="message-text">Bot: ${message}</span><span class="timestamp">${timestamp}</span>`;
        document.getElementById("messagesList").appendChild(li);
        li.classList.add('bot-message');
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
            var timestamp = new Date().toLocaleTimeString();

            var li = document.createElement("li");
            li.innerHTML = `<span class="message-text">User: ${message}</span><span class="timestamp">${timestamp}</span>`;
            document.getElementById("messagesList").appendChild(li);
            li.classList.add('user-message');

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