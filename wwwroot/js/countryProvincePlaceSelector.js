"use strict";
var connection = new signalR.HubConnectionBuilder().withUrl("/countryProvincePlaceSelectorHub").build();

connection.on("ProvinceList", function (provinces) {
    provinces.forEach(province => {
        provinceSelector.append(new Option(province.descrizione, province.codice));
    });
    $("#provinceSelector").prop('disabled', false);
});

document.getElementById("countrySelector").addEventListener("change", function (event) {

    $("#provinceSelector").empty();
    $("#placeSelector").empty();
    $("#provinceSelector").prop('disabled', true);
    $("#placeSelector").prop('disabled', true);

    if ($("#countrySelector").val() != "100000100") {//If not Italia --> province and place are Estero (Foreign)
        $("#provinceSelector").append(new Option("ES", "ES"));
        $("#placeSelector").append(new Option("ES", "ES"));
    } else {
        connection.invoke("GetProvinces").catch(function (err) {
            return console.error(err.toString());
        }).then(function () {
            connection.invoke("GetTowns", $("#provinceSelector").val()).catch(function (err) {
                return console.error(err.toString());
            })
        })
    }

});

connection.on("TownsList", function (towns) {
    towns.forEach(town => {
        $("#placeSelector").append(new Option(town.descrizione, town.codice));
    });
    $("#placeSelector").prop('disabled', false);
});

document.getElementById("provinceSelector").addEventListener("change", function (event) {

    $("#placeSelector").empty();
    $("#placeSelector").prop('disabled', true);

    connection.invoke("GetTowns", $("#provinceSelector").val()).catch(function (err) {
        return console.error(err.toString());
    });

    $("#placeSelector").prop('disabled', false);
});

connection.start().then(function () {

    // No info in configuration yet
    if ($("#hiddenBirthProvince").val() == "" || $("#hiddenBirthProvince").val() == undefined) {
        // Default to ES (Afghanistan)
        $("#provinceSelector").append(new Option("ES", "ES"));
        $("#placeSelector").append(new Option("ES", "ES"));
        $("#provinceSelector").prop('disabled', true);
        $("#placeSelector").prop('disabled', true);
    } else {
        connection.invoke("GetProvinces").catch(function (err) {
            return console.error(err.toString());
        }).then(function () {
            let hbp = $("#hiddenBirthProvince").val();
            $("#provinceSelector option[value='" + hbp + "']").prop("selected", true);
        })


        connection.invoke("GetTowns", $("#hiddenBirthProvince").val()).catch(function (err) {
            return console.error(err.toString());
        }).then(function () {
            let hbp = $("#hiddenBirthPlace").val();
            $("#placeSelector option[value='" + hbp + "']").prop("selected", true);


        })
    }
}).catch(function (err) {
    return console.error(err.toString());
});

let isLoaded = false;

document.getElementById("selectorIssuingCountry").addEventListener("focus", function (event) {

    if (!isLoaded) {
        isLoaded = true;
        connection.invoke("GetAllTowns").catch(function (err) {
            return console.error(err.toString());
        });
    }

});

connection.on("AllTownsList", function (towns) {
    towns.forEach(town => {
        $("#selectorIssuingCountry").append(new Option(town.descrizione, town.codice));
    });
    $("#placeSelector").prop('disabled', false);
});

document.querySelector("form").addEventListener("submit", function() {
    console.log(document.querySelector("[name='Person.BirthPlace']").value);
    console.log(document.querySelector("[name='Person.BirthProvince']").value);
});