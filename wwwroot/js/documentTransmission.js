"use strict";

var connectionT = new signalR.HubConnectionBuilder().withUrl("/docsTransmissionHub").build();

connectionT.start().then(function () {

}).catch(function (err) {
    return console.error(err.toString());
});

connectionT.on("TransmissionResult", function (result) {
    $("#resultPH").text(result);

    //Change the correspondent boolean dropdown
    if (result == "Schedine sent correctly.") {
        $("#isSent2Police").prop("disabled", false);
        $("#isSent2Police option[value='False']").removeProp("selected");
        $("#isSent2Police option[value='True']").prop("selected", true).attr("selected", "selected");
        $("#isSent2Police").trigger("change");
        $("#isSent2Police").prop("disabled", true);

        $("#PreCheckinSent").prop("disabled", false);
        $("#PreCheckinSent option[value='False']").removeProp("selected");
        $("#PreCheckinSent option[value='True']").prop("selected", true).attr("selected", "selected");
        $("#PreCheckinSent").trigger("change");
        $("#PreCheckinSent").prop("disabled", true);
    } else if (result == "Contract sent correctly.") {
        $("#isContractSent").prop("disabled", false);
        $("#isContractSent option[value='False']").removeProp("selected");
        $("#isContractSent option[value='True']").prop("selected", true).attr("selected", "selected");
        $("#isContractSent").trigger("change");
        $("#isContractSent").prop("disabled", true);
    }else if (result == "Pre-Checkin sent correctly.") {
        $("#isContractSent").prop("disabled", false);
        $("#isContractSent option[value='False']").removeProp("selected");
        $("#isContractSent option[value='True']").prop("selected", true).attr("selected", "selected");
        $("#isContractSent").trigger("change");
        $("#isContractSent").prop("disabled", true);
    }
});

document.getElementById("SendContract").addEventListener("click", function (event) {
    var bookingID = $("#bookingID").val();

    var persons = $("#PersonsOnBookingList li");
    var mainPersonID = persons.toArray().map(li => li.querySelector("input")).find(input => {
        return input && ["16", "17", "18"].includes(input.dataset.rolecode);
    })?.id;
    connectionT.invoke("SendContract", bookingID, mainPersonID).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});

document.getElementById("SendPreCheckIn").addEventListener("click", function (event) {
    var bookingID = $("#bookingID").val();

    var persons = $("#PersonsOnBookingList li");
    var mainPersonID = persons.toArray().map(li => li.querySelector("input")).find(input => {
        return input && ["16", "17", "18"].includes(input.dataset.rolecode);
    })?.id;
    connectionT.invoke("SendPreCheckIn", bookingID, mainPersonID).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});

document.getElementById("TransmitToRegionPolice").addEventListener("click", function (event) {
    var bookingID = $("#bookingID").val();

    var persons = $("#PersonsOnBookingList li");

    var personsIds = []
    for (const person of Array.from(persons)) {
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