"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/createDocumentsHub").build();

connection.start().then(function () {
   
}).catch(function (err) {
    return console.error(err.toString());
});

document.getElementById("DownloadReportA").addEventListener("click", function (event) {
    var accommodationID=document.getElementById("AccommodationsList").value;   

    let row = this.closest("tr");
    // Get the hidden input value (assuming it's before the row)
    let hiddenInput = row.previousElementSibling;
    let channelID=hiddenInput.value;

    let dateFrom=document.getElementById("DateFrom").value;
    let dateTo=document.getElementById("DateTo").value;

    connection.invoke("CreateReportExcel", accommodationID,channelID,dateFrom,dateTo).catch(function (err) {
        return console.error(err.toString());
    });

    event.preventDefault();
});

connection.on("DownloadFile", (fileName, base64Data) => {
    var type="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
    // Convert Base64 to Blob
    const byteCharacters = atob(base64Data);
    const byteNumbers = new Array(byteCharacters.length);
    for (let i = 0; i < byteCharacters.length; i++) {
        byteNumbers[i] = byteCharacters.charCodeAt(i);
    }
    const byteArray = new Uint8Array(byteNumbers);
    const fileBlob = new Blob([byteArray], { type: type });

    // Create Download Link
    const link = document.createElement("a");
    link.href = URL.createObjectURL(fileBlob);
    link.download = fileName;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
});

connectionD.on("Error", (error) => {
    $("#errorMessage").text(error);
});