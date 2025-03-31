"use strict";

var connectionT = new signalR.HubConnectionBuilder().withUrl("/docsTransmissionHub").build();

connectionT.start().then(function () {
    
}).catch(function (err) {
    return console.error(err.toString());
});

connectionT.on("TransmissionResult", function (result) {
    $("#resultPH").text(result);

    //Change the correspondent boolean dropdown
    if(result=="Schedine sent correctly."){
        $("#isSent2Police").prop("disabled", false);
        $("#isSent2Police option[value='False']").removeProp("selected");
        $("#isSent2Police option[value='True']").prop("selected", true).attr("selected", "selected");
        $("#isSent2Police").trigger("change");
        $("#isSent2Police").prop("disabled", true);

        $("#Sent2Region").prop("disabled", false);
        $("#Sent2Region option[value='False']").removeProp("selected");
        $("#Sent2Region option[value='True']").prop("selected", true).attr("selected", "selected");
        $("#Sent2Region").trigger("change");
        $("#Sent2Region").prop("disabled", true);
    }else if(result=="Email sent correctly."){
        $("#isSent2Town").prop("disabled", false);
        $("#isSent2Town option[value='False']").removeProp("selected");
        $("#isSent2Town option[value='True']").prop("selected", true).attr("selected", "selected");
        $("#isSent2Town").trigger("change");
        $("#isSent2Town").prop("disabled", true);
    }
});

document.getElementById("TransmitToTown").addEventListener("click", function (event) {
    var bookingID=document.getElementById("bookingID").value;

    var list=document.getElementById("PersonsOnBookingList");
    var persons=list.getElementsByTagName("li");
    var mainPersonID=persons[0].getElementsByTagName("input")[0].id;

    connectionT.invoke("SendContract", bookingID, mainPersonID).catch(function (err) {
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