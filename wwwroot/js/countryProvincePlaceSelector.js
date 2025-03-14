"use strict";
//TODO: cash the mega dropdowns and defer page show after they are ready.
var connection = new signalR.HubConnectionBuilder().withUrl("/countryProvincePlaceSelectorHub").build();

//Disable the send button until connection is established.

connection.on("ProvinceList", function (provinces) {
    var provinceSelector = $("#provinceSelector");
    provinces.forEach(province => {
        var option = document.createElement("option");
        option.value = province.codice;
        option.innerHTML = province.descrizione;
        provinceSelector.append(option)
    });
    $("#provinceSelector").prop('disabled', false);
});

document.getElementById("countrySelector").addEventListener("change", function (event) {

    $("#provinceSelector").empty();
    $("#placeSelector").empty();
    $("#provinceSelector").prop('disabled', true);
    $("#placeSelector").prop('disabled', true);

    if ($("#countrySelector").val() != "100000100") {//If Italia --> province and place are Estero (Foreign)
        let optionPr = document.createElement("option");
        optionPr.value = "ES";
        optionPr.innerHTML = "ES";
        let optionPl = document.createElement("option");
        optionPl.value = "ES";
        optionPl.innerHTML = "ES";
        $("#provinceSelector").append(optionPr);
        $("#placeSelector").append(optionPl);
    } else {
        connection.invoke("GetProvinces").catch(function (err) {
            return console.error(err.toString());
        }).then(function(){
            connection.invoke("GetTowns", $("#provinceSelector").val()).catch(function (err) {
                return console.error(err.toString());
            })
        })
    }

});

connection.on("TownsList", function (towns) {
    var townSelector = $("#placeSelector");
    towns.forEach(town => {
        var option = document.createElement("option");
        option.value = town.codice;
        option.innerHTML = town.descrizione;
        townSelector.append(option)
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

    var pageTitle = $('title').text();
    if (pageTitle == "Edit - MAppBnB") {
        connection.invoke("GetProvinces").catch(function (err) {
            return console.error(err.toString());
        }).then(function () {
            $("#provinceSelector").val($("#hiddenBirthProvince").val());
        })


        connection.invoke("GetTowns", $("#hiddenBirthProvince").val()).catch(function (err) {
            return console.error(err.toString());
        }).then(function () {
            $("#placeSelector").val($("#hiddenBirthPlace").val());
        })

    } else {
        // Default to ES (Afghanistan)
        let optionPr = document.createElement("option");
        optionPr.value = "ES";
        optionPr.innerHTML = "ES";
        let optionPl = document.createElement("option");
        optionPl.value = "ES";
        optionPl.innerHTML = "ES";
        $("#provinceSelector").append(optionPr);
        $("#placeSelector").append(optionPl);
        $("#provinceSelector").prop('disabled', true);
        $("#placeSelector").prop('disabled', true);
    }
}).catch(function (err) {
    return console.error(err.toString());
});
