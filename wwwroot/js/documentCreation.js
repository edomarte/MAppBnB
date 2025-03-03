"use strict";

var connectionD = new signalR.HubConnectionBuilder().withUrl("/createDocumentsHub").build();

connectionD.start().then(function () {
   
}).catch(function (err) {
    return console.error(err.toString());
});

document.getElementById("generateContract").addEventListener("click", function (event) {
    var contractDetails="";
       
//TODO: get Main Person Data
    var list=document.getElementById("PersonsOnBookingList");
    var persons=list.getElementsByTagName("li");
    // Assume the first person added to the booking is the main person
    var mainPersonID=persons[0].getElementsByTagName("input")[0].id;
  
    var accommodationID=document.getElementById("AccommodationsList").value;

    var bookingID=document.getElementById("bookingID").value;
   

    connectionD.invoke("CreateContract", mainPersonID,accommodationID,bookingID).catch(function (err) {
        return console.error(err.toString());
    });

    event.preventDefault();
});