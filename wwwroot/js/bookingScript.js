"use strict";

// When the isPaidSelector changes value, enable or disable the paymentDateInput field according to its value.
document.getElementById("isPaidSelector").addEventListener("change", function (event) {
    if($("#isPaidSelector").val()=="True"){
        $("#paymentDateInput").attr("disabled", false);
    }else{
        $("#paymentDateInput").attr("disabled", true);
    }
});