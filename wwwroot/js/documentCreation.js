"use strict";

var connectionD = new signalR.HubConnectionBuilder().withUrl("/createDocumentsHub").build();

connectionD.start().then(function () {

}).catch(function (err) {
    return console.error(err.toString());
});

document.getElementById("generateContract").addEventListener("click", function (event) {
    var persons=$("#PersonsOnBookingList li");
    var mainPersonID = persons.toArray().map(li => li.querySelector("input")).find(input => {
        return input && ["16", "17", "18"].includes(input.dataset.rolecode);
    })?.id; // Extract the input's id
  
    var accommodationID=$("#AccommodationsList").val();

    var bookingID = $("#bookingID").val();

    connectionD.invoke("CreateContract", mainPersonID,accommodationID,bookingID).catch(function (err) {
        return console.error(err.toString());
    });

    $("#isContractPrinted").prop("disabled", false);
    $("#isContractPrinted option[value='False']").removeProp("selected");
    $("#isContractPrinted option[value='True']").prop("selected", true).attr("selected", "selected");
    $("#isContractPrinted").trigger("change");
    $("#isContractPrinted").prop("disabled", true);


    event.preventDefault();
});

document.getElementById("generateBookingDetails").addEventListener("click", function (event) {
       
    var persons = $("#PersonsOnBookingList li");
    var personsIDs=[];

    for (var i = 0; i < persons.length; ++i) {
        let person = persons[i];
        let checkbox=person.getElementsByTagName("input")[0];
        personsIDs.push(checkbox.id);
      }
  
    var accommodationID=$("#AccommodationsList").val();
    var bookingID = $("#bookingID").val();

    connectionD.invoke("CreateBookingDetails", personsIDs, accommodationID, bookingID).catch(function (err) {
        return console.error(err.toString());
    });

    event.preventDefault();
});

document.getElementById("generatePreCheckin").addEventListener("click", function (event) {
       
    var persons = $("#PersonsOnBookingList li");
    var mainPersonID=persons[0].getElementsByTagName("input")[0].id;

    var accommodationID=$("#AccommodationsList").val();
    var bookingID = $("#bookingID").val();
   

    connectionD.invoke("CreatePreCheckin", mainPersonID,accommodationID,bookingID).catch(function (err) {
        return console.error(err.toString());
    });

    event.preventDefault();
});

document.getElementById("generateContractPDF").addEventListener("click", function (event) {
       
    var bookingID = $("#bookingID").val();
   
    connectionD.invoke("CreateContractPDF", bookingID).catch(function (err) {
        return console.error(err.toString());
    });

    event.preventDefault();
});

document.getElementById("generatePreCheckinPDF").addEventListener("click", function (event) {
       
    var bookingID = $("#bookingID").val();
   
    connectionD.invoke("CreatePreCheckinPDF", bookingID).catch(function (err) {
        return console.error(err.toString());
    });

    event.preventDefault();
});

connectionD.on("DownloadFile", (fileName, base64Data) => {
    var type=""
    if(fileName.substring(fileName.lastIndexOf("."))==".docx")
        type="application/vnd.openxmlformats-officedocument.wordprocessingml.document"
    else
        type="application/pdf"
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
    $("#resultPH").text(error);
});