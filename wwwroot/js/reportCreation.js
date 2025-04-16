"use strict";

// Create a SignalR connection to the /createDocumentsHub endpoint
var connection = new signalR.HubConnectionBuilder().withUrl("/createDocumentsHub").build();

// Start the SignalR connection
connection.start().then(function () {
    // Connection established successfully (no action needed here)
}).catch(function (err) {
    // Log connection errors to the console
    return console.error(err.toString());
});

// Event handler for clicking on elements with class 'DownloadReportA'
$('.DownloadReportA').on("click", function (event) {
    // Get selected accommodation ID from the dropdown
    var accommodationID = document.getElementById("AccommodationsList").value;   

    // Get the closest row (<tr>) of the clicked button
    let row = this.closest("tr");

    // Get the hidden input containing the channel ID (assumed to be just before the row)
    let hiddenInput = row.previousElementSibling;
    let channelID = hiddenInput.value;

    // Get the date range for the report
    let dateFrom = document.getElementById("DateFrom").value;
    let dateTo = document.getElementById("DateTo").value;

    // Call the server-side method to generate the report
    connection.invoke("CreateReportExcel", accommodationID, channelID, dateFrom, dateTo).catch(function (err) {
        // Log errors to the console
        return console.error(err.toString());
    });

    // Prevent default form submission or link navigation
    event.preventDefault();
});

// Event handler for receiving and downloading the generated Excel file
connection.on("DownloadFile", (fileName, base64Data) => {
    var type = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    // Decode the Base64 string into binary data
    const byteCharacters = atob(base64Data);
    const byteNumbers = new Array(byteCharacters.length);
    for (let i = 0; i < byteCharacters.length; i++) {
        byteNumbers[i] = byteCharacters.charCodeAt(i);
    }
    const byteArray = new Uint8Array(byteNumbers);

    // Create a Blob object representing the Excel file
    const fileBlob = new Blob([byteArray], { type: type });

    // Create a temporary download link and trigger it programmatically
    const link = document.createElement("a");
    link.href = URL.createObjectURL(fileBlob);
    link.download = fileName; // Set file name for download
    document.body.appendChild(link);
    link.click(); // Simulate click to start download
    document.body.removeChild(link); // Clean up DOM
});

// Event handler for receiving and displaying error messages from the server
connection.on("Error", (error) => {
    $("#errorMessage").text(error); // Display error message in the page
});