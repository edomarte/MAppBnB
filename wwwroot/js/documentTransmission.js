"use strict";

// Create a SignalR connection to the /docsTransmissionHub endpoint
var connectionT = new signalR.HubConnectionBuilder().withUrl("/docsTransmissionHub").build();

// Start the SignalR connection
connectionT.start().then(function () {
    // Connection established
}).catch(function (err) {
    // Log connection error
    return console.error(err.toString());
});

// Handler for incoming transmission results from the server
connectionT.on("TransmissionResult", function (result) {
    // Display the result message in the UI
    $("#resultPH").text(result);

    // Update related dropdowns based on the result message
    if (result == "Schedine sent correctly.") {
        // Update isSent2Police dropdown to True
        $("#isSent2Police").prop("disabled", false);
        $("#isSent2Police option[value='False']").removeProp("selected");
        $("#isSent2Police option[value='True']").prop("selected", true).attr("selected", "selected");
        $("#isSent2Police").trigger("change");
        $("#isSent2Police").prop("disabled", true);

        // Update PreCheckinSent dropdown to True
        $("#PreCheckinSent").prop("disabled", false);
        $("#PreCheckinSent option[value='False']").removeProp("selected");
        $("#PreCheckinSent option[value='True']").prop("selected", true).attr("selected", "selected");
        $("#PreCheckinSent").trigger("change");
        $("#PreCheckinSent").prop("disabled", true);

    } else if (result == "Contract sent correctly.") {
        // Update isContractSent dropdown to True
        $("#isContractSent").prop("disabled", false);
        $("#isContractSent option[value='False']").removeProp("selected");
        $("#isContractSent option[value='True']").prop("selected", true).attr("selected", "selected");
        $("#isContractSent").trigger("change");
        $("#isContractSent").prop("disabled", true);

    } else if (result == "Pre-Checkin sent correctly.") {
        // Update isContractSent dropdown to True (same dropdown reused)
        $("#isContractSent").prop("disabled", false);
        $("#isContractSent option[value='False']").removeProp("selected");
        $("#isContractSent option[value='True']").prop("selected", true).attr("selected", "selected");
        $("#isContractSent").trigger("change");
        $("#isContractSent").prop("disabled", true);
    }
});

// Handle click on "Send Contract" button
document.getElementById("SendContract").addEventListener("click", function (event) {
    // Get booking ID from input
    var bookingID = $("#bookingID").val();

    // Get main person ID (with role code 16, 17, or 18)
    var persons = $("#PersonsOnBookingList li");
    var mainPersonID = persons.toArray().map(li => li.querySelector("input")).find(input => {
        return input && ["16", "17", "18"].includes(input.dataset.rolecode);
    })?.id;

    // Invoke SignalR method to send contract
    connectionT.invoke("SendContract", bookingID, mainPersonID).catch(function (err) {
        return console.error(err.toString());
    });

    // Prevent default form behavior
    event.preventDefault();
});

// Handle click on "Send Pre-CheckIn" button
document.getElementById("SendPreCheckIn").addEventListener("click", function (event) {
    // Get booking ID
    var bookingID = $("#bookingID").val();

    // Get main person ID (with appropriate role code)
    var persons = $("#PersonsOnBookingList li");
    var mainPersonID = persons.toArray().map(li => li.querySelector("input")).find(input => {
        return input && ["16", "17", "18"].includes(input.dataset.rolecode);
    })?.id;

    // Invoke SignalR method to send pre-checkin
    connectionT.invoke("SendPreCheckIn", bookingID, mainPersonID).catch(function (err) {
        return console.error(err.toString());
    });

    // Prevent default form behavior
    event.preventDefault();
});

// Handle click on "Transmit To Region Police" button
document.getElementById("TransmitToRegionPolice").addEventListener("click", function (event) {
    // Get booking ID
    var bookingID = $("#bookingID").val();

    // Collect all person IDs from booking list
    var persons = $("#PersonsOnBookingList li");
    var personsIds = [];
    for (const person of Array.from(persons)) {
        let input = person.getElementsByTagName("input")[0]; // Get first input element
        if (input) {
            personsIds.push(input.id); // Add input ID to list
        }
    }

    // Invoke SignalR method to transmit data to regional police
    connectionT.invoke("SendToRegionPolice", bookingID, personsIds).catch(function (err) {
        return console.error(err.toString());
    });

    // Prevent default form behavior
    event.preventDefault();
});
