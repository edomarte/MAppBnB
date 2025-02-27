"use strict";

var connectionS = new signalR.HubConnectionBuilder().withUrl("/personSearchHub").build();

//Disable the send button until connection is established.
document.getElementById("sendButton").disabled = true;

connectionS.on("ResultList", function (persons) {
    persons.forEach(person => {
        var li = document.createElement("li");
        var label = document.createElement("label");
        label.innerText = person.name+", "+person.surname+","+person.birthDate;
        var checkbox = document.createElement("input");
        checkbox.id = person.id;
        checkbox.name="selectedPersons";
        checkbox.innerText = person.name;
        checkbox.type = "checkbox";
        label.appendChild(checkbox);
        li.appendChild(label);
        document.getElementById("ResultList").appendChild(li);
    });
});

connectionS.start().then(function () {
    document.getElementById("sendButton").disabled = false;
}).catch(function (err) {
    return console.error(err.toString());
});

document.getElementById("sendButton").addEventListener("click", function (event) {
    document.getElementById("ResultList").innerHTML = "";
    var personName = document.getElementById("SearchPerson").value;
    connectionS.invoke("SearchPerson", personName,getPersonsInBooking()).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});

function getPersonsInBooking(){
    var list=document.getElementById("PersonsOnBookingList");
    var persons=list.getElementsByTagName("li");
    var personIds=[];
    for (var i = 0; i < persons.length; ++i) {
        let person = persons[i];
        let checkbox=person.getElementsByTagName("input")[0];
        personIds.push(checkbox.id);
      }
    return personIds;
}

document.getElementById("addSelectedButton").addEventListener("click", function (event) {
    var list=document.getElementById("ResultList");
    var persons=list.getElementsByTagName("li");
    for (var i = 0; i < persons.length; ++i) {
        let person = persons[i];
        let checkbox=person.getElementsByTagName("input")[0];
        if(checkbox.checked){
            document.getElementById("PersonsOnBookingList").appendChild(person);
        }
      }

    event.preventDefault();
});

document.getElementById("removeSelectedButton").addEventListener("click", function (event) {
    var list=document.getElementById("PersonsOnBookingList");
    var persons=list.getElementsByTagName("li");
    for (var i = 0; i < persons.length; ++i) {
        let person = persons[i];
        let checkbox=person.getElementsByTagName("input")[0];
        if(checkbox.checked){
            document.getElementById("ResultList").appendChild(person);
        }
      }

    event.preventDefault();
});

document.getElementById("form").addEventListener("submit", function (event) {
    var list=document.getElementById("PersonsOnBookingList");
    var persons=list.getElementsByTagName("li");
    for (var i = 0; i < persons.length; ++i) {
        let person = persons[i];
        let checkbox=person.getElementsByTagName("input")[0];
        document.getElementById("PersonIDs").value+=checkbox.id+",";
      }
});