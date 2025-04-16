"use strict";

// Create a SignalR connection to the /createDocumentsHub endpoint
var connectionD = new signalR.HubConnectionBuilder().withUrl("/createDocumentsHub").build();

// Start the SignalR connection
connectionD.start().then(function () {
    // Connection started successfully
}).catch(function (err) {
    // Log any connection error
    return console.error(err.toString());
});

// Handle click on "Generate Contract" button
document.getElementById("generateContract").addEventListener("click", function (event) {
    // Get all list items (persons) from the booking list
    var persons = $("#PersonsOnBookingList li");

    // Find the main person (roles 16, 17, or 18) and extract their input ID
    var mainPersonID = persons.toArray().map(li => li.querySelector("input")).find(input => {
        return input && ["16", "17", "18"].includes(input.dataset.rolecode);
    })?.id;

    // Get selected accommodation and booking ID
    var accommodationID = $("#AccommodationsList").val();
    var bookingID = $("#bookingID").val();

    // Invoke server-side method to generate contract document
    connectionD.invoke("CreateContract", mainPersonID, accommodationID, bookingID).catch(function (err) {
        return console.error(err.toString());
    });

    // Update contract printed dropdown to 'True' and trigger change
    $("#isContractPrinted").prop("disabled", false);
    $("#isContractPrinted option[value='False']").removeProp("selected");
    $("#isContractPrinted option[value='True']").prop("selected", true).attr("selected", "selected");
    $("#isContractPrinted").trigger("change");
    $("#isContractPrinted").prop("disabled", true);

    // Prevent default button behavior
    event.preventDefault();
});

// Handle click on "Generate Booking Details" button
document.getElementById("generateBookingDetails").addEventListener("click", function (event) {
    // Get all persons and prepare an array of their input IDs
    var persons = $("#PersonsOnBookingList li");
    var personsIDs = [];

    // For each person in persons, create a checkbox with the id and push it to the array.
    for (var i = 0; i < persons.length; ++i) {
        let person = persons[i];
        let checkbox = person.getElementsByTagName("input")[0];
        personsIDs.push(checkbox.id);
    }

    // Get selected accommodation and booking ID
    var accommodationID = $("#AccommodationsList").val();
    var bookingID = $("#bookingID").val();

    // Invoke server-side method to generate booking details document
    connectionD.invoke("CreateBookingDetails", personsIDs, accommodationID, bookingID).catch(function (err) {
        return console.error(err.toString());
    });

    // Prevent default button behavior
    event.preventDefault();
});

// Handle click on "Generate Pre-Checkin" button
document.getElementById("generatePreCheckin").addEventListener("click", function (event) {
    // Get all persons and find the main person
    var persons = $("#PersonsOnBookingList li");
    // Return the main person id between the persons in the array.
    var mainPersonID = persons.toArray().map(li => li.querySelector("input")).find(input => {
        return input && ["16", "17", "18"].includes(input.dataset.rolecode);
    })?.id;

    // Get accommodation and booking IDs
    var accommodationID = $("#AccommodationsList").val();
    var bookingID = $("#bookingID").val();

    // Invoke server-side method to generate pre-checkin document
    connectionD.invoke("CreatePreCheckin", mainPersonID, accommodationID, bookingID).catch(function (err) {
        return console.error(err.toString());
    });

    // Prevent default button behavior
    event.preventDefault();
});

// Handle click on "Generate Contract PDF" button
document.getElementById("generateContractPDF").addEventListener("click", function (event) {
    // Get booking ID
    var bookingID = $("#bookingID").val();

    // Invoke server-side method to generate contract PDF
    connectionD.invoke("CreateContractPDF", bookingID).catch(function (err) {
        return console.error(err.toString());
    });

    // Prevent default button behavior
    event.preventDefault();
});

// Handle click on "Generate Pre-Checkin PDF" button
document.getElementById("generatePreCheckinPDF").addEventListener("click", function (event) {
    // Get booking ID
    var bookingID = $("#bookingID").val();

    // Invoke server-side method to generate pre-checkin PDF
    connectionD.invoke("CreatePreCheckinPDF", bookingID).catch(function (err) {
        return console.error(err.toString());
    });

    // Prevent default button behavior
    event.preventDefault();
});

// Listener for receiving files from the server
connectionD.on("DownloadFile", (fileName, base64Data) => {
    var type = "";

    // Determine the MIME type based on file extension
    if (fileName.substring(fileName.lastIndexOf(".")) == ".docx")
        type = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
    else
        type = "application/pdf";

    // Convert base64 to binary data (Blob)
    const byteCharacters = atob(base64Data);
    const byteNumbers = new Array(byteCharacters.length);
    for (let i = 0; i < byteCharacters.length; i++) {
        byteNumbers[i] = byteCharacters.charCodeAt(i);
    }
    const byteArray = new Uint8Array(byteNumbers);
    const fileBlob = new Blob([byteArray], { type: type });

    // Create a temporary download link for the file
    const link = document.createElement("a");
    link.href = URL.createObjectURL(fileBlob);
    link.download = fileName;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
});

// Listener for server-side error messages
connectionD.on("Error", (error) => {
    // Display error message in the placeholder element
    $("#resultPH").text(error);
});