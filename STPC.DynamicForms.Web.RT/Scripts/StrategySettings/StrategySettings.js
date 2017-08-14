/* $('#CustomerForm').dialog({
        autoOpen: false,
        width: 400,
        resizable: false,
        title: 'Campos estrategia',
        modal: true,
        buttons: {
            "Cancelar": function () {
                $(this).dialog("close");
            },
            "Aceptar": function () {
               
            }
        }
    });*/



$(function () {

    $("#ddlStrategy").change(function () {
        try {
            var StrategyId = $("#ddlStrategy option:selected").val();
            $.ajax({
                type: "POST",
                url: "/StrategySettings/GetParametersStrategy/",
                data: {
                    'StrategyId': StrategyId
                },
                dataType: "html",
                success: function (evt) {
                    $('#CustomerForm').html(evt);
                    $(".CustomCurrency ").each(function () {
                        applyFormatCurrency($(this));
                    });
                    $.unblockUI();
                },
                error: function (result, status, error) {
                    $.unblockUI();
                    if (result.responseText == "EndSesion") {
                        var urlSite = window.location.origin = window.location.protocol + "//" + window.location.hostname + (window.location.port ? ':' + window.location.port : '');
                        location.href = urlSite
                    }
                    else {
                        alert("Error al cargar la pagina: " + result.responseText);
                    }

                }
            });
        }
        catch (er) {
            alert(er)
        }
    });

    $(".CustomCurrency").blur(function () {
        applyFormatCurrency($(this));
    });
    function applyFormatCurrency(sender) {

        $(sender).formatCurrency({
            roundToDecimalPlace: 0, negativeFormat: '-%s%n'
        });
    }
   
});



//$('.CustomCurrency').on("blur", function () {

//    applyFormatCurrency($(this));

//});


function EditStrategyAttribute(Uid, strategyId) {
    function applyFormatCurrency(sender) {

        $(sender).formatCurrency({
            roundToDecimalPlace: 0, negativeFormat: '-%s%n'
        });
    }
    $(".CustomCurrency ").each(function () {
        this.value = this.value.replace('$ ', '');
        this.value = this.value.replace('.', '');
    });
    var value = $("#id_" + Uid).val()

    $.ajax({
        type: "POST",
        url: "/StrategySettings/UpdateAttributeValue/",
        data: {
            'uid': Uid, 'value': value, 'strategyId': strategyId
        },
        dataType: "html",
        success: function (evt) {
            $('#CustomerForm').html();
            $('#CustomerForm').html(evt);
            $(".CustomCurrency ").each(function () {
                applyFormatCurrency($(this));
            });
            var error = $("#txtError").val();

            if (error != "") {
                alert(error);
            }
            else
                alert("Atributo actualizado con éxito")
        },
        error: function (result, status, error) {

            alert("Error al cargar la pagina: " + error);


        }
    });

}


