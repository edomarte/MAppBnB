"use strict";

// Establish SignalR connection to the country-province-place selector hub
var connection = new signalR.HubConnectionBuilder().withUrl("/countryProvincePlaceSelectorHub").build();

// When the server sends the list of provinces
connection.on("ProvinceList", function (provinces) {
    provinces.forEach(province => {
        // Add each province to the selector as an <option>
        provinceSelector.append(new Option(province.descrizione, province.codice));
    });
    // Enable the province selector after population
    $("#provinceSelector").prop('disabled', false);
});

// Event listener for when the country is changed
document.getElementById("countrySelector").addEventListener("change", function (event) {
    // Clear province and place dropdowns
    $("#provinceSelector").empty();
    $("#placeSelector").empty();

    // If the selected country is not Italy
    if ($("#countrySelector").val() != "100000100") {
        // Set "Estero" (Foreign) for both province and place
        $("#provinceSelector").append(new Option("ES", "ES"));
        $("#placeSelector").append(new Option("ES", "ES"));

        // Select the "ES" option for both selectors
        $("#provinceSelector option[value='ES']").prop("selected", true);
        $("#placeSelector option[value='ES']").prop("selected", true);
    } else {
        // If Italy is selected, request provinces from the server
        connection.invoke("GetProvinces").catch(function (err) {
            return console.error(err.toString());
        }).then(function () {
            // Once provinces are retrieved, get towns for the selected province
            connection.invoke("GetTowns", $("#provinceSelector").val()).catch(function (err) {
                return console.error(err.toString());
            });
        });
    }
});

// When the server sends the list of towns for a province
connection.on("TownsList", function (towns) {
    towns.forEach(town => {
        // Add each town to the place selector as an <option>
        $("#placeSelector").append(new Option(town.descrizione, town.codice));
    });
    // Enable the place selector after population
    $("#placeSelector").prop('disabled', false);
});

// Event listener for when the province is changed
document.getElementById("provinceSelector").addEventListener("change", function (event) {
    // Clear and disable the place selector while it is loading
    $("#placeSelector").empty();
    $("#placeSelector").prop('disabled', true);

    // Request towns for the selected province
    connection.invoke("GetTowns", $("#provinceSelector").val()).catch(function (err) {
        return console.error(err.toString());
    });

    // Enable the place selector (will be populated via the "TownsList" handler)
    $("#placeSelector").prop('disabled', false);
});

// Start the SignalR connection
connection.start().then(function () {

    // If no province is preselected and ES (foreign) is stored
    if ($("#hiddenProvince").val() == "ES") {
        // Default to "Estero" for province and place
        $("#provinceSelector").append(new Option("Estero", "ES"));
        $("#placeSelector").append(new Option("Estero", "ES"));
        $("#provinceSelector option[value='ES']").prop("selected", true);
        $("#placeSelector option[value='ES']").prop("selected", true);
    } else {
        // Otherwise, load provinces
        connection.invoke("GetProvinces").catch(function (err) {
            return console.error(err.toString());
        }).then(function () {
            let hbp = $("#hiddenProvince").val();
            // If no stored province, default to Agrigento (AG)
            if (hbp == undefined || hbp == "" || hbp == null) {
                $("#provinceSelector option[value='AG']").prop("selected", true);
            } else {
                $("#provinceSelector option[value='" + hbp + "']").prop("selected", true);
            }
        });

        // Determine which province to use for towns
        var birthProvince = $("#hiddenProvince").val();
        if (birthProvince == undefined || birthProvince == "" || birthProvince == null)
            birthProvince = "AG";
        else
            birthProvince = $("#hiddenProvince").val();

        // Request towns for the determined province
        connection.invoke("GetTowns", birthProvince).catch(function (err) {
            return console.error(err.toString());
        }).then(function () {
            let hbp = $("#hiddenCity").val();
            // If no town is preselected, default to Agrigento
            if (hbp == undefined || hbp == "" || hbp == null) {
                $("#placeSelector option[value='419084001']").prop("selected", true);
            } else {
                $("#placeSelector option[value='" + hbp + "']").prop("selected", true);
            }
        });
    }
}).catch(function (err) {
    return console.error(err.toString());
});