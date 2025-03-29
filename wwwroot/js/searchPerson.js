"use strict";

var connectionS = new signalR.HubConnectionBuilder().withUrl("/personSearchHub").build();

//Disable the send button until connection is established.
document.getElementById("searchPersonButton").disabled = true;

connectionS.on("ResultList", function (persons) {
    persons.forEach(personRole => {
        var li = $("<li>");
        var label = $("<label>")
            .text(personRole.person.name + ", " + personRole.person.surname + ", " + personRole.person.birthDate + ", " + personRole.roleName);
        var checkbox = $("<input>")
            .attr("id", personRole.person.id)
            .attr("data-roleCode", personRole.person.roleRelation)
            .attr("type", "checkbox")
            .attr("name", "selectedPersons")
            .text(personRole.person.name)
        label.append(checkbox);
        li.append(label);
        $("#ResultList").append(li);
    });
});

connectionS.start().then(function () {
    document.getElementById("searchPersonButton").disabled = false;
    $("#isSent2Police").prop("disabled", true);
    $("#isSent2Region").prop("disabled", true);
    $("#isSent2Town").prop("disabled", true);
    $("#isContractPrinted").prop("disabled", true);

    if ($("#isPaidSelector").val() == "True") {
        $("#paymentDateInput").attr("disabled", false);
    } else {
        $("#paymentDateInput").attr("disabled", true);
    }

}).catch(function (err) {
    return console.error(err.toString());
});

document.getElementById("searchPersonButton").addEventListener("click", function (event) {
    document.getElementById("ResultList").innerHTML = "";
    var personName = document.getElementById("SearchPerson").value;
    connectionS.invoke("SearchPerson", personName, getPersonsInBooking()).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});

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

document.getElementById("form").addEventListener("submit", async function (event) {
    var isPersonSelectionValid = await checkPersonRolesAreCorrect(event);
    // If disabled it does not send data to controller
    enableBooleanPlaceholders();
    if (isPersonSelectionValid)
        this.submit();
});

async function checkPersonRolesAreCorrect(event) {
    event.preventDefault();
    var isPersonSelectionValid = false;

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

        // Check if the person is already in another booking at the same time.
        if ($(document).attr('title') == "Create") {
            if (await isPersonAlreadyInContemporaryBooking(personId))
                return isPersonSelectionValid;
        }

        $("#PersonIDs").val($("#PersonIDs").val() + personId + ",")

        var rolecode = checkbox.getAttribute("data-roleCode");
        if (rolecode == "16" || rolecode == "17" || rolecode == "18") { // If the selected person is a mainPerson role type.
            mainPersonCount++;
            mainPersonRole = rolecode;
        } else {
            if (rolecode == "19")
                familyMemberCount++;
            else
                groupComponentCount++;
        }
    }

    if (mainPersonCount != 1) {
        $("#personsErrorAlert").text("Select just one main person")
    } else {
        if (mainPersonRole == "16" && (familyMemberCount + groupComponentCount > 0)) {
            $("#personsErrorAlert").text("A booking with a single guest cannot contain any other person.")
        } else {
            if (mainPersonRole == "17" && ((familyMemberCount == 0) || (groupComponentCount > 0))) {
                $("#personsErrorAlert").text("A booking with a family head must only contain one or more family members.")
            } else {
                if (mainPersonRole == "18" && ((familyMemberCount > 0) || (groupComponentCount == 0))) {
                    $("#personsErrorAlert").text("A booking with a group head must only contain one or more group members.")
                } else {
                    isPersonSelectionValid = true;
                    return isPersonSelectionValid;
                }
            }
        }
    }
    return isPersonSelectionValid;
}

//TODO: non più persone nella prenotazione di quante nella stanza

function enableBooleanPlaceholders() {
    $("#isSent2Police").prop("disabled", false);
    $("#isSent2Region").prop("disabled", false);
    $("#isSent2Town").prop("disabled", false);
    $("#isContractPrinted").prop("disabled", false);
}

async function isPersonAlreadyInContemporaryBooking(personId) {
    let dateFrom = $("#checkInDate").val();
    let dateTo = $("#checkOutDate").val();

    var result = await connectionS.invoke("IsPersonAlreadyInBooking", personId, dateFrom, dateTo)
    if (result != null) {
        let linkToFoundBooking = $("<a>")
            .attr("href", "/Booking/Edit/" + result[1])
            .text("Booking where " + result[0] + " is present.");
        $("#personsErrorAlert").text(result[0] + " is already present in another booking at the same time.");
        $("#personsErrorAlert").append(linkToFoundBooking);
        return true;
    }
    return false;
}