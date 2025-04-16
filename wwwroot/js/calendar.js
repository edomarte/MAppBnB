"use strict";

// Create connection to the SignalR calendarHub.
var connectionCL = new signalR.HubConnectionBuilder().withUrl("/calendarHub").build();
var firstDayOnCalendar;
var lastDayOnCalendar;

// When the Hub method sends data on UpdateCalendar.
connectionCL.on("UpdateCalendar", function (bookings) {
    // For each booking object in the array received.
    bookings.forEach(booking => {
        var first = 0;

        // If the firstday on the calendar is later than the checkinDay, use that as first day of the calendar. 
        // Else use the checkin date (to avoid going over the displayed calendar days).
        if (firstDayOnCalendar - booking.checkinDay > 0)
            first = firstDayOnCalendar;
        else
            first = booking.checkinDay

        var last = 0;
        // If the last of calendar is earlier than the checkout day, use that as last day of the calendar. 
        // Else use the checkout date (to avoid going over the displayed calendar days).
        if (lastDayOnCalendar - booking.checkoutDay < 0)
            last = lastDayOnCalendar;
        else
            last = booking.checkoutDay;

        // For each day of the booking, add a paragraph to the calendar with a link to the booking.
        for (let i = first; i < (last + 1); i++) {
            let par = $("<p>");
            // URL of the booking
            let url = `/Booking/Edit/${booking.id}`;

            // Link to the booking
            let a = $("<a>")
                .attr("href", url) 
                .text(booking.room.name + " - " + booking.mainPerson.name + " " + booking.mainPerson.surname)
            par.append(a);
            // i is the day number of the cell in the calendar.
            $("#" + i).append(par);
        }
    });
});

// Function to update the calendar.
function updateCalendar() {
    // Get the accommodation value and the mothYearPicker value selected.
    var selectedAccommodation = $("#AccommodationsList").val();
    var monthYearPicker = $("#monthYearPicker").val();
    // Invoke the SignalR method for getting the Bookings in the month.
    connectionCL.invoke("GetBookingsInMonth", selectedAccommodation, monthYearPicker).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
}

// Start the connection to the SignalR hub
connectionCL.start().then(function () {
    // Get id (DayOfYear) of first day on calendar (tbody->tr->td)
    firstDayOnCalendar = document.getElementById("tbody").firstElementChild.firstElementChild.id * 1;
    // Get the last day on the calendar (42 days on display).
    lastDayOnCalendar = firstDayOnCalendar + 41;
    // Update the calendar content.
    updateCalendar()
}).catch(function (err) {
    return console.error(err.toString());
});

// When the accommodation selector changes value, remove all the bookings displayed and update the calendar.
document.getElementById("AccommodationsList").addEventListener("change", function (event) {
    // All paragraph (contains the link to the booking for the previously selected accommodation).
    $("p").remove();
    updateCalendar()
});

// When the monthYear picker selector changes.
document.getElementById("monthYearPicker").addEventListener("change", function (event) {
    // Manage the "cancel" button in the date picker
    if ($("#monthYearPicker").val() == "") {
        return;
    }
    let $tbody = $("#tbody");
    $tbody.empty(); // Clear previous content in the table body.

    // Get the first day of the month picked.
    let firstDayOfTheMonth = new Date($("#monthYearPicker").val());
    // Initialize the start date to the first day of the month picked.
    let startDate = new Date(firstDayOfTheMonth);
    // Get the date of the first cell of the calendar to disply by subtracting the day of the week of the first day of the picked month to the first day of the month (to get the previous monday).
    startDate.setDate(firstDayOfTheMonth.getDate() - firstDayOfTheMonth.getDay());

    let $currentRow;
    // Render 42 days (6 weeks)
    for (let i = 0; i < 42; i++) {
        // If it is a monday, add a row to the table (new week).
        if (i % 7 === 0) {
            $currentRow = $("<tr>");
            $tbody.append($currentRow);
        }

        // Get the day of the year by subtracting the 31/12 of the previous year to the start date and dividing by the number of milliseconds in a day.
        // It returns a number between 1 and 365/366 (leap years)
        // Calculate the day of the year (1–365/366)
        let dayOfYear = Math.floor((startDate - new Date(startDate.getFullYear(), 0, 0)) / 86400000);

        // Populate the id of the cell with its day of the year.
        let $cell = $("<td>")
            .attr("id", dayOfYear)

        // Create and append the day number
        let day = $("<span>")
            .attr("class", "dayPlaceholder")
            .text(startDate.getDate());

        // Append the day placeholder to the cell.
        $cell.append(day)
        $cell.append($("<br>"))

        // Append the cell to the current row.
        $currentRow.append($cell);

        // Move to next day
        startDate.setDate(startDate.getDate() + 1);

    }
    
    // Update calendar range tracking variables
    // Valorize the first day on calendar variable with the id of the first cell of the table (it must be int so *1).
    firstDayOnCalendar = document.getElementById("tbody").firstElementChild.firstElementChild.id * 1;
    // Valorize the last day on calendar variable with the id of the first cell of the table + 41 (42 cells total).
    lastDayOnCalendar = firstDayOnCalendar + 41;

    // Update the calendar content.
    updateCalendar()
});