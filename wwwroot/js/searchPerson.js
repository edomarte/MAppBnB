"use strict";

// Initialize a SignalR connection to the "personSearchHub"
var connectionS = new signalR.HubConnectionBuilder().withUrl("/personSearchHub").build();

// Disable the search button until the SignalR connection is established
document.getElementById("searchPersonButton").disabled = true;

// Handle the "ResultList" event from the server, which returns a list of matching persons
connectionS.on("ResultList", function (persons) {
    persons.forEach(personRole => {
        // Create a list item and a label with person details
        var li = $("<li>");
        var label = $("<label>")
            .text(personRole.person.name + ", " + personRole.person.surname + ", " + personRole.person.birthDate + ", " + personRole.roleName);
        // Create a checkbox for the person, storing their ID and role
        var checkbox = $("<input>")
            .attr("id", personRole.person.id)
            .attr("data-roleCode", personRole.person.roleRelation)
            .attr("type", "checkbox")
            .attr("name", "selectedPersons")
            .text(personRole.person.name)
        // Append checkbox to label, and label to list item
        label.append(checkbox);
        li.append(label);
        // Add the list item to the result list in the UI
        $("#ResultList").append(li);
    });
});

// Start the SignalR connection
connectionS.start().then(function () {
    // Enable the search button once connection is established
    document.getElementById("searchPersonButton").disabled = false;

    // Disable contract-related fields initially
    $("#isSent2Police").prop("disabled", true);
    $("#isPreCheckinSent").prop("disabled", true);
    $("#isContractSent").prop("disabled", true);
    $("#isContractPrinted").prop("disabled", true);

    // Enable or disable payment date input depending on the payment selector value
    if ($("#isPaidSelector").val() == "True") {
        $("#paymentDateInput").attr("disabled", false);
    } else {
        $("#paymentDateInput").attr("disabled", true);
    }

}).catch(function (err) {
    // Log any connection errors
    return console.error(err.toString());
});

// Add event listener to the search button
document.getElementById("searchPersonButton").addEventListener("click", function (event) {
    // Clear previous search results
    document.getElementById("ResultList").innerHTML = "";

    // Get the inputted person name and send search request to server
    var personName = document.getElementById("SearchPerson").value;
    connectionS.invoke("SearchPerson", personName, getPersonsInBooking()).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault(); // Prevent form submission
});

// Get the IDs of persons currently added to the booking
function getPersonsInBooking() {
    var list = document.getElementById("PersonsOnBookingList");
    var persons = list.getElementsByTagName("li");
    var personIds = [];
    for (var i = 0; i < persons.length; ++i) {
        let person = persons[i];
        let checkbox = person.getElementsByTagName("input")[0];
        personIds.push(checkbox.id);
    }
    return personIds;
}

// Add selected persons from result list to the booking list
document.getElementById("addSelectedButton").addEventListener("click", function (event) {
    var persons = $("#ResultList li");
    var personsLength = persons.length;
    for (var i = 0; i < personsLength; i++) {
        let person = persons[i];
        let checkbox = person.getElementsByTagName("input")[0];
        if (checkbox.checked) {
            $("#PersonsOnBookingList").append(person);
        }
    }
    event.preventDefault();
});

// Remove selected persons from booking list and move them back to result list
document.getElementById("removeSelectedButton").addEventListener("click", function (event) {
    var persons = $("#PersonsOnBookingList li");
    var personsLength = persons.length;
    for (var i = 0; i < personsLength; i++) {
        let person = persons[i];
        let checkbox = person.getElementsByTagName("input")[0];
        if (checkbox.checked) {
            $("#ResultList").append(person);
        }
    }
    event.preventDefault();
});

// On form submission, validate selected persons and enable disabled fields before submission
document.getElementById("form").addEventListener("submit", async function (event) {
    var isPersonSelectionValid = await checkPersonRolesAreCorrect(event);
    // Enable boolean fields so that values are included in the form submission
    enableBooleanPlaceholders();
    if (isPersonSelectionValid)
        this.submit();
});

// Validate if the selected person roles meet the booking requirements
async function checkPersonRolesAreCorrect(event) {
    event.preventDefault();
    var isPersonSelectionValid = false;

    // Reset previous error and state values
    $("#roomBlockedP").val("");
    $("#personsErrorAlert").val("");
    $("#PersonIDs").val("");

    var persons = $("#PersonsOnBookingList li");
    var mainPersonRole;
    var groupComponentCount = 0;
    var familyMemberCount = 0
    var mainPersonCount = 0;

    for (var i = 0; i < persons.length; ++i) {
        let person = persons[i];
        let checkbox = person.getElementsByTagName("input")[0];
        var personId = checkbox.id;

        // If creating a new booking, check for overlapping bookings
        if ($(document).attr('title') == "Create") {
            if (await isPersonAlreadyInContemporaryBooking(personId))
                return isPersonSelectionValid;
        }

        // Append selected person ID to hidden input
        $("#PersonIDs").val($("#PersonIDs").val() + personId + ",")

        // Determine role and count appropriately
        var rolecode = checkbox.getAttribute("data-roleCode");
        if (rolecode == "16" || rolecode == "17" || rolecode == "18") { // main roles
            mainPersonCount++;
            mainPersonRole = rolecode;
        } else {
            if (rolecode == "19") // family member
                familyMemberCount++;
            else // group component
                groupComponentCount++;
        }
    }

    // Validate according to booking rules
    if (mainPersonCount != 1) {
        $("#personsErrorAlert").text("Select just one OSPITE SINGOLO")
    } else {
        if (mainPersonRole == "16" && (familyMemberCount + groupComponentCount > 0)) {
            $("#personsErrorAlert").text("A booking with a OSPITE SINGOLO cannot contain any other person.")
        } else {
            if (mainPersonRole == "17" && ((familyMemberCount == 0) || (groupComponentCount > 0))) {
                $("#personsErrorAlert").text("A booking with a CAPOFAMIGLIA must only contain one or more FAMILIARE.")
            } else {
                if (mainPersonRole == "18" && ((familyMemberCount > 0) || (groupComponentCount == 0))) {
                    $("#personsErrorAlert").text("A booking with a CAPOGRUPPO must only contain one or more MEMBRO GRUPPO.")
                } else {
                    isPersonSelectionValid = true;
                    return isPersonSelectionValid;
                }
            }
        }
    }
    return isPersonSelectionValid;
}

// TODO: Ensure the number of people in the booking does not exceed the number of beds in the room

// Enable all boolean inputs (so their values are included when the form is submitted)
function enableBooleanPlaceholders() {
    $("#isSent2Police").prop("disabled", false);
    $("#isPreCheckinSent").prop("disabled", false);
    $("#isContractSent").prop("disabled", false);
    $("#isContractPrinted").prop("disabled", false);
}

// Check asynchronously if a person is already present in another booking at the same time
async function isPersonAlreadyInContemporaryBooking(personId) {
    let dateFrom = $("#checkInDate").val();
    let dateTo = $("#checkOutDate").val();

    var result = await connectionS.invoke("IsPersonAlreadyInBooking", personId, dateFrom, dateTo)
    if (result != null) {
        // If person found in another booking, show link to edit that booking
        let linkToFoundBooking = $("<a>")
            .attr("href", "/Booking/Edit/" + result[1])
            .text("Booking where " + result[0] + " is present.");
        $("#personsErrorAlert").text(result[0] + " is already present in another booking at the same time.");
        $("#personsErrorAlert").append(linkToFoundBooking);
        return true;
    }
    return false;
}