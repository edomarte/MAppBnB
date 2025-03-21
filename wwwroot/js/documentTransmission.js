"use strict";

var connectionT = new signalR.HubConnectionBuilder().withUrl("/docsTransmissionHub").build();

connectionT.start().then(function () {
    
}).catch(function (err) {
    return console.error(err.toString());
});

connectionT.on("TransmissionResult", function (rooms) {
    //TODO: Display result
});

document.getElementById("TransmitToTown").addEventListener("click", function (event) {
    var bookingID=document.getElementById("bookingID").value;

    var list=document.getElementById("PersonsOnBookingList");
    var persons=list.getElementsByTagName("li");
    var mainPersonID=persons[0].getElementsByTagName("input")[0].id;

    connectionT.invoke("SendToTown", bookingID, mainPersonID).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});

document.getElementById("TransmitToRegionPolice").addEventListener("click", function (event) {
    var bookingID=document.getElementById("bookingID").value;

    var list=document.getElementById("PersonsOnBookingList");
    var persons=list.getElementsByTagName("li");
    var personsIds=[]
    for(const person of Array.from(persons)) {
        let input = person.getElementsByTagName("input")[0]; // Get the first input inside <li>
        if (input) {
            personsIds.push(input.id); // Push input ID to array
        }
    }

    connectionT.invoke("SendToRegionPolice", bookingID, personsIds).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});