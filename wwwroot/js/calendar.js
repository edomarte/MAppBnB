"use strict";

var connectionCL = new signalR.HubConnectionBuilder().withUrl("/calendarHub").build();
var firstDayOnCalendar;
var lastDayOnCalendar;

//Disable the send button until connection is established.

connectionCL.on("UpdateCalendar", function (bookings) {
    bookings.forEach(booking => {


        var first = 0;
        if (firstDayOnCalendar - booking.checkinDay > 0)
            first = firstDayOnCalendar;
        else
            first = booking.checkinDay

        var last = 0;
        if (lastDayOnCalendar - booking.checkoutDay < 0)
            last = lastDayOnCalendar;
        else
            last = booking.checkoutDay;

        for (let i = first; i < (last + 1); i++) {
            let par = $("<p>");
            let url = `/Booking/Edit/${booking.id}`;

            let a = $("<a>")
                .attr("href", url) 
                .text(booking.room.name + " - " + booking.mainPerson.name + " " + booking.mainPerson.surname)
            par.append(a);
            $("#" + i).append(par);
        }
    });
});

function updateCalendar() {
    var selectedAccommodation = document.getElementById("AccommodationsList").value;
    var monthYearPicker = document.getElementById("monthYearPicker").value;
    connectionCL.invoke("GetBookingsInMonth", selectedAccommodation, monthYearPicker).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
}

connectionCL.start().then(function () {
    // Get id (DayOfYear) of first day on calendar (tbody->tr->td)
    firstDayOnCalendar = document.getElementById("tbody").firstElementChild.firstElementChild.id * 1;
    lastDayOnCalendar = firstDayOnCalendar + 41;
    updateCalendar()
}).catch(function (err) {
    return console.error(err.toString());
});

document.getElementById("AccommodationsList").addEventListener("change", function (event) {
    $("p").remove();
    updateCalendar()
});

document.getElementById("monthYearPicker").addEventListener("change", function (event) {
    let $tbody = $("#tbody");
    $tbody.empty(); // Clear previous content

    let firstDayOfTheMonth = new Date($("#monthYearPicker").val());
    let startDate = new Date(firstDayOfTheMonth);
    startDate.setDate(firstDayOfTheMonth.getDate() - firstDayOfTheMonth.getDay());

    let $currentRow;

    for (let i = 0; i < 42; i++) {
        if (i % 7 === 0) {
            $currentRow = $("<tr>");
            $tbody.append($currentRow);
        }

        let dayOfYear = Math.floor((startDate - new Date(startDate.getFullYear(), 0, 0)) / 86400000);

        let $cell = $("<td>")
            .attr("id", dayOfYear)

        let day = $("<span>")
            .attr("class", "dayPlaceholder")
            .text(startDate.getDate());

        $cell.append(day)
        $cell.append($("<br>"))

        $currentRow.append($cell);

        startDate.setDate(startDate.getDate() + 1);

    }
    firstDayOnCalendar = document.getElementById("tbody").firstElementChild.firstElementChild.id * 1;
    lastDayOnCalendar = firstDayOnCalendar + 41;

    // do the same as in index.html (get list of days from datetime given the selected month-year)
    updateCalendar()
});