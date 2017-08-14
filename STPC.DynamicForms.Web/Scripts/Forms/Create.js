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
                var bValid = true;
                allFieldsInNewOptionForm.removeClass("ui-state-error");
                bValid = bValid && checkLength(FormName, "Nombre", 1, 200);
                if (bValid) {
                    $.post("/Form/Create", { name: FormName.val() },
			function (data) {
			    if (data.success == true) {
			        window.location.reload(false);
			    }
			    else alert("Falló la creación");
			}, "json");
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

