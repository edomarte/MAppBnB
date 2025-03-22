"use strict";

document.getElementById("isPaidSelector").addEventListener("change", function (event) {
    if($("#isPaidSelector").val()=="True"){
        $("#paymentDateInput").attr("disabled", false);
    }else{
        $("#paymentDateInput").attr("disabled", true);
    }
});