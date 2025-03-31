"use strict";

var connectionR = new signalR.HubConnectionBuilder().withUrl("/roomSelectorHub").build();

//Disable the send button until connection is established.

connectionR.on("RoomsList", function (rooms) {
    var roomsSelector=document.getElementById("RoomsSelector");
    roomsSelector.innerHTML="";
    rooms.forEach(room => {
        var option = document.createElement("option");
        option.value=room.id;
        option.innerHTML=room.name;
        document.getElementById("RoomsSelector").appendChild(option);
    });
});

function updateRoomList(){
    var selectedAccommodation=$("#AccommodationsList").val();
    connectionR.invoke("RoomSelector", selectedAccommodation).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
}

connectionR.start().then(function () {
    updateRoomList()
}).catch(function (err) {
    return console.error(err.toString());
});

document.getElementById("AccommodationsList").addEventListener("change", function (event) {
    updateRoomList()
});