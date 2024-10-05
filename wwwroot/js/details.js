const conversationId = "@Model.Id"; // Get the conversation ID from the model

// Connect to the SignalR hub
const connection = new signalR.HubConnectionBuilder().withUrl("/chathub").build();

// Function to add a message to the UI
function addMessage(user, message) {
    console.log("Test2");
    const messageContainer = document.getElementById("messageContainer");
    const noMessagesText = document.getElementById("noMessagesText");

    // Remove the "no messages" text if it exists
    if (noMessagesText) {
        noMessagesText.remove();
    }

    // Create a new list item for the message
    const li = document.createElement("li");
    li.classList.add("list-group-item");
    li.innerHTML = `<strong>${user}</strong>: <p>${message}</p>`;

    // Add the new message to the message container
    messageContainer.appendChild(li);
}

// Receive message from SignalR and add it to the message list
connection.on("ReceiveMessage", function (user, message) {
    console.log("Test1");
    addMessage(user, message);
});

// Start the SignalR connection
connection.start().then(function () {
    console.log("SignalR connected.");

    // Join the conversation group after connection is established
    connection.invoke("JoinConversationGroup", conversationId).catch(function (err) {
        return console.error(err.toString());
    });
}).catch(function (err) {
    return console.error(err.toString());
});

// Send a new message via SignalR when the form is submitted
document.getElementById("messageForm").addEventListener("submit", function (event) {
    event.preventDefault();

    const messageInput = document.getElementById("messageInput");
    const message = messageInput.value;

    // Call the SendMessageToGroup method on the server and pass the conversationId
    connection.invoke("SendMessageToGroup", conversationId, "@User.Identity.Name", message).catch(function (err) {
        return console.error(err.toString());
    });

    // Clear the input box
    messageInput.value = "";
});