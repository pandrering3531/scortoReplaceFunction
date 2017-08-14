function deleteme(key) {
    if (confirm("¿Seguro de borrar esta opción?")) {
        var therowinthetable = $('span[data-key=' + key + ']').parent().parent();
        $.post("/Categories/RemoveOption", { OptionKey: key, CategoryName: $("#CategoriesList").val() },
                function (data) {
                    if (data.Success == true) {
                        $("#Options").load("/Categories/Options/", { categoryname: $("#CategoriesList").val() });
                        //$(therowinthetable).remove();
                    }
                    else alert("Delete failed");
                }, "json");
    }
}

$(function () {
    $('.ui-state-default').hover(
				function () { $(this).addClass('ui-state-hover'); },
				function () { $(this).removeClass('ui-state-hover'); }
			);
    var Key = $("#Key"), Value = $("#Value"), allFieldsInNewOptionForm = $([]).add(Key).add(Value), tips = $(".validateTips");
    $("#NewOptionForm").dialog({
        autoOpen: false,
        height: 300,
        width: 350,
        modal: true,
        buttons: {
            "Crear": function () {
                var bValid = true;
                allFieldsInNewOptionForm.removeClass("ui-state-error");

                bValid = bValid && checkLength(Key, "Option Key", 1, 200);
                bValid = bValid && checkLength(Value, "Option Value", 1, 200);
                if (bValid) {
                    //Valida si está disponible la lista de emrpesas
                    var appId;
                    if ($('#ddlAplicationNameOptions').length) {
                        console.log($('#ddlAplicationNameOptions').val())
                        if ($('#ddlAplicationNameOptions').val() == "") {
                            appId = 0;
                        }
                        else
                            appId = $('#ddlAplicationNameOptions').val();
                    }
                    else {
                        appId = 0;
                    }

                    $.post("/Categories/AddOption", {
                        OptionKey: Key.val(), OptionValue: Value.val(),
                        CategoryName: $("#CategoriesList").val(),
                        idOptionParent: $("#ddlDependencyOption").val(),
                        'aplicationNameId': appId
                    }, function (data) {
                        if (data.Success == true) {
                            $("#OptionsList > tbody:last").append("<tr><td>" + data.key + "</td><td>" + data.value + "</td><td><span class='ui-icon ui-icon-trash ui-state-default ui-corner-all' title='Borrar opción'" + " onclick=\"deleteme(\'" + data.key + "\');\"" + "></span></td></tr>");
                            $("#OptionsList").show();
                        }
                        else alert("Insert failed");
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

    $("#NewOption")
			.button()
			.click(function () {
			    $("#NewOptionForm").dialog("open");
			});
});