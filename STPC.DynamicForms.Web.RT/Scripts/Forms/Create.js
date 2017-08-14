$(function () {
    $("#NewForm")
			.button()
			.click(function () {
			    $("#NewFormDialog").dialog("open");
			});

    var FormName = $("#FormName"), allFieldsInNewOptionForm = $([]).add(FormName), tips = $(".validateTips");

    $("a.decorated").button();

    $("#NewFormDialog").dialog({
        autoOpen: false,
        height: 300,
        width: 350,
        modal: true,
        buttons: {
            "Crear": function () {
                blockScreen("Un momento por favor....");
                var bValid = true;
                allFieldsInNewOptionForm.removeClass("ui-state-error");
                bValid = bValid && checkLength(FormName, "Nombre", 1, 200);
                if (bValid) {
                    $.ajax({
                        type: "POST",
                        url: "/Form/Create/",
                        data: {
                            'name': FormName.val()
                        },
                        dataType: "html",
                        success: function (evt) {
                            $('#right-column').html(evt);
                            $.unblockUI();
                           
                            //$("#EditCustomerProfileForm").submit();
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
                    $(this).dialog("close");
                }
            },
            "Cancelar": function () {
                $(this).dialog("close");
            }
        },
        close: function () {
            allFieldsInNewOptionForm.val("").removeClass("ui-state-error");
        }
    });


});