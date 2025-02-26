"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/personSearchHub").build();

//Disable the send button until connection is established.
document.getElementById("sendButton").disabled = true;

connection.on("ResultList", function (persons) {
    persons.forEach(person => {
        var li = document.createElement("li");
        var label = document.createElement("label");
        label.innerText = person.name+", "+person.surname+","+person.birthDate;
        var checkbox = document.createElement("input");
        checkbox.id = person.id;
        checkbox.innerText = person.name;
        checkbox.type = "checkbox";
        label.appendChild(checkbox);
        li.appendChild(label);
        document.getElementById("ResultList").appendChild(li);
    });
});

connection.start().then(function () {
    document.getElementById("sendButton").disabled = false;
}).catch(function (err) {
    return console.error(err.toString());
});

document.getElementById("sendButton").addEventListener("click", function (event) {
    document.getElementById("ResultList").innerHTML = "";
    var personName = document.getElementById("SearchPerson").value;
    connection.invoke("SearchPerson", personName).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});

document.getElementById("addSelectedButton").addEventListener("click", function (event) {
    var searchResultChildren=document.getElementById("PersonsOnBookingList").children;
    searchResultChildren.forEach(child => {
        if(child.children[0].children[0].checked){ // if checkbox checked, add to the other list
            /*var li = document.createElement("li");
            var label = document.createElement("label");
            label.innerText = child.children[0].children[0].innerText;
            var hiddenInput = document.createElement("input");
            hiddenInput.type = "hidden";
            hiddenInput.name = "PersonIds";
            hiddenInput.value = child.children[0].children[0].id;
            label.appendChild(hiddenInput);
            li.appendChild(label);
            document.getElementById("PersonsOnBookingList").appendChild(li);*/
    }});

    event.preventDefault();
});