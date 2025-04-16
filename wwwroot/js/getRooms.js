"use strict";

// Create a SignalR connection to the /roomSelectorHub endpoint
var connectionR = new signalR.HubConnectionBuilder().withUrl("/roomSelectorHub").build();

// Event handler triggered when the server sends an updated list of rooms
connectionR.on("RoomsList", function (rooms) {
    // Get the rooms dropdown element
    var roomsSelector = document.getElementById("RoomsSelector");

    // Clear existing options
    roomsSelector.innerHTML = "";

    // Add each room to the dropdown as an option
    rooms.forEach(room => {
        var option = document.createElement("option");
        option.value = room.id;           // Set option value to room ID
        option.innerHTML = room.name;     // Set display text to room name
        document.getElementById("RoomsSelector").appendChild(option);
    });
});

// Function to request updated room list based on selected accommodation
function updateRoomList() {
    var selectedAccommodation = $("#AccommodationsList").val(); // Get selected accommodation ID

    // Invoke the "RoomSelector" method on the server, passing the selected accommodation ID
    connectionR.invoke("RoomSelector", selectedAccommodation).catch(function (err) {
        return console.error(err.toString()); // Log any error
    });

    event.preventDefault(); // Prevent default behavior of event
}

// Start the SignalR connection and request initial room list when ready
connectionR.start().then(function () {
    updateRoomList(); // Populate rooms after successful connection
}).catch(function (err) {
    return console.error(err.toString()); // Log connection errors
});

// Re-fetch room list when the accommodation selection changes
document.getElementById("AccommodationsList").addEventListener("change", function (event) {
    updateRoomList(); // Refresh room options for new accommodation
});