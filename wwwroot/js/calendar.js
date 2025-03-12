"use strict";

var connectionCL = new signalR.HubConnectionBuilder().withUrl("/calendarHub").build();
var firstDayOnCalendar;
var lastDayOnCalendar;

//Disable the send button until connection is established.

connectionCL.on("UpdateCalendar", function (bookings) {
    console.log("Raw bookings data:", bookings);
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

        for(let i=first;i<(last+1);i++){
            let option = document.createElement("p");
        option.innerHTML = booking.id;
            document.getElementById(i).appendChild(option);
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
    $("p").remove();
    $("#calendarTable tbody").empty();

    var startDate=new Date($("#monthYearPicker").val())
    for (let i = 0; i < 42; i++)
        {
            var date = startDate+i;// TODO: incrementare il giorno della data

            if (i % 7 == 0 && i > 0)
            {
                $("#calendarTable tbody").appendChild("</tr><tr>")
                
                // Corrected row handling
            }

            //<td id="@date.DayOfYear">@date.Day</td>
            // Correctly placed within the loop
        }
    // do the same as in index.html (get list of days from datetime given the selected month-year)
    updateCalendar()
});