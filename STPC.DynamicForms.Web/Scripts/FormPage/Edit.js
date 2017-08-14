$(function () {
    if ($("#NoItems").val() == 'True') {
        $("#TheTable").hide();
        $("#NothingToSeeHere").show();
    } else {

        $("#TheTable").show();
        $("#NothingToSeeHere").hide();

    }
    $("#NewPanel")
			.button()
			.click(function () {
			    $("#NewPanelDialog").dialog("open");
			});

    var PanelName = $("#PanelName"),
                    PanelDescription = $("#PanelDescription"),
                    PanelColumns = $("#PanelColumns"),
                    PanelStylesheet = $("#PanelStylesheet"),
                    allFieldsInNewOptionForm = $([])
                        .add(PanelName)
                        .add(PanelDescription)
                        .add(PanelColumns)
                        .add(PanelStylesheet),
                    tips = $(".validateTips");

    $("a.decorated").button();

    $("#slider-range-min").slider({
        range: "min",
        value: 1,
        min: 1,
        max: 8,
        slide: function (event, ui) {
            $("#PanelColumns").val(ui.value);
        }
    });
    $("#amount").val("$" + $("#slider-range-min").slider("value"));

    $("#NewPanelDialog").dialog({
        autoOpen: false,
        height: 460,
        width: 350,
        modal: true,
        buttons: {
            "Crear": function () {
                var bValid = true;
                allFieldsInNewOptionForm.removeClass("ui-state-error");
                bValid = bValid && checkLength(PanelName, "Nombre", 1, 64);
                bValid = bValid && checkLength(PanelDescription, "Nombre", 1, 200);
                bValid = bValid && checkLength(PanelStylesheet, "Estilos", 1, 128);
                if (bValid) {
                    $.post("/Panel/Create", { name: PanelName.val(), desc: PanelDescription.val(), stylesheet: PanelStylesheet.val(), columns: PanelColumns.val(), formpageId: $("#FormPageId").val() },
			function (data) {
			    if (data.success == true) {
			        window.location.reload(false);
			        //$("#TheTable > tbody:last").append("<tr><td>" + data.Name +
			        //	"</td><td>" + data.Desc + "</td><td>" + data.Columns + "</td><td>" + data.CssStyle +
			        //	"</td><td><a class='decorated' href='/Panel/Edit/" + data.uid + "'>Editar</a> " +
			        //	"<a class='decorated' href='/Panel/Delete/" + data.uid + "'>Eliminar</a></td></tr>");
			        //$("#TheTable").show();
			        //$("#NothingToSeeHere").hide();
			        //$("a.decorated").button();
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
            $("#PanelColumns").val("1");
            $("#slider-range-min").slider("value", 1);
        }
    });
});

$("#StarQuery")
        .button()
        .click(function () {

        });

function ConfirmDone() {
    //for testing purposes we can always show the dialog box
    return confirm("Are you sure you want to lose unsaved changes?");
}


function updateRollAction(roll, action) {
    try {
        $.post("/FormPage/UpdateRoolByAction", { RollName: roll, FormaActionId: action, state: $("#" + roll + "-" + action).is(':checked') }, function (data) {
            if (data.Success == true) {

            }
            else alert("Insert failed");
        }, "json");
    }
    catch (err) {
        alert(err);
    }

}

function UpdateStateByAction(stateId, action) {
    try {
        $.post("/FormPage/UpdateStateByAction", { StateId: stateId, FormaActionId: action, state: $("#" + stateId + "-" + action).is(':checked') }, function (data) {
            if (data.Success == true) {

            }
            else alert("Insert failed");
        }, "json");
    }
    catch (err) {
        alert(err);
    }

}


$('.chkStrategyExec').change(function () {
    try {

        console.log('test')
        //Levanta dialog para pedir la página a la cual debe ir
        var nameAction = $("#" + $(this).attr('id') + "NameAction").val();

        var idFormPage = $("#FormPageId").val()
        var idForm = $("#FormId").val()

        if ($(this).is(':checked')) {
            $.ajax({
                type: "GET",
                url: "/FormPage/GetPagesStrategies/",
                data: {
                    'FormPageId': idFormPage,
                    'FormId': idForm,
                    'FormaActionId': $(this).attr('id')

                },
                dataType: "html",
                success: function (evt) {

                    $('#modalContentStrategies').dialog("open");
                    $('#modalContentStrategies').html(evt);

                },
                error: function (req, status, error) {
                    alert("Error!Occured" + error);
                }
            });
        } else {
            //$('input[name=EjecutaEstrategia' + $(this).attr('id') + ']').attr("disabled", "disabled");
            //$('input[name=Guarda' + $(this).attr('id') + ']').attr("disabled", "disabled");


            //$('input[name=EjecutaEstrategia' + $(this).attr('id') + ']').prop("checked", false);
            //$('input[name=Guarda' + $(this).attr('id') + ']').prop("checked", false);

            //$("#" + $(this).attr('id') + "StateGuid").val("" + "/" + $(this).attr('id'));
            //$('#EstadoAccion' + $(this).attr('id')).html('');
        }
    }
    catch (err) {
        alert(err);
    }
})

$('.chkSaveAction').change(function () {
    try {

        $.post("/FormPage/UpdateSaveAction", { FormaActionId: $(this).attr('id'), state: $(this).is(':checked') }, function (data) {
            if (data.Success == true) {

            }
            else alert("Insert failed");
        }, "json");
    }
    catch (err) {
        alert(err);
    }
})


$('.chkAcion').change(function () {
    try {

        if ($(this).is(':checked')) {
            $('input[name=EjecutaEstrategia' + $(this).attr('id') + ']').removeAttr("disabled");
            $('input[name=Guarda' + $(this).attr('id') + ']').removeAttr("disabled");
            $('input[name=ModificaEstado' + $(this).attr('id') + ']').removeAttr("disabled");
            $('input[class=chkRol_' + $(this).attr('id') + ']').removeAttr("disabled");

            //Levanta dialog para pedir la página a la cual debe ir
            var nameAction = $("#" + $(this).attr('id') + "NameAction").val();

            var idFormPage = $("#FormPageId").val()
            var idForm = $("#FormId").val()

            if (nameAction == "IrPaginaEspecifica")
                $.ajax({
                    type: "GET",
                    url: "/FormPage/GetPagesOfForm/",
                    data: {
                        'FormPageId': idFormPage,
                        'FormId': idForm,
                        'FormaActionId': $(this).attr('id')

                    },
                    dataType: "html",
                    success: function (evt) {

                        $('#modalContent2').dialog("open");
                        $('#modalContent2').html(evt);

                    },
                    error: function (req, status, error) {
                        alert("Error!Occured" + error);
                    }
                });
        } else {
            $('input[name=EjecutaEstrategia' + $(this).attr('id') + ']').attr("disabled", "disabled");
            $('input[name=Guarda' + $(this).attr('id') + ']').attr("disabled", "disabled");
            //$('input[name=ModificaEstado' + $(this).attr('id') + ']').attr("disabled", "disabled");
            $('input[class=chkRol_' + $(this).attr('id') + ']').attr("disabled", "disabled");

            $('input[name=EjecutaEstrategia' + $(this).attr('id') + ']').prop("checked", false);
            $('input[name=Guarda' + $(this).attr('id') + ']').prop("checked", false);
            //$('input[name=ModificaEstado' + $(this).attr('id') + ']').prop("checked", false);
            $('input[class=chkRol_' + $(this).attr('id') + ']').prop("checked", false);


        }
    }
    catch (err) {
        alert(err);
    }
})

$('.chkState').change(function () {
    try {


        //Levanta dialog para pedir la página a la cual debe ir
        var nameAction = $("#" + $(this).attr('id') + "NameAction").val();

        var idFormPage = $("#FormPageId").val()
        var idForm = $("#FormId").val()

        if ($(this).is(':checked')) {
            $.ajax({
                type: "GET",
                url: "/FormPage/GetPagesStates/",
                data: {
                    'FormPageId': idFormPage,
                    'FormId': idForm,
                    'FormaActionId': $(this).attr('id')

                },
                dataType: "html",
                success: function (evt) {

                    $('#modalContentStates').dialog("open");
                    $('#modalContentStates').html(evt);

                },
                error: function (req, status, error) {
                    alert("Error!Occured" + error);
                }
            });
        } else {
            //$('input[name=EjecutaEstrategia' + $(this).attr('id') + ']').attr("disabled", "disabled");
            //$('input[name=Guarda' + $(this).attr('id') + ']').attr("disabled", "disabled");


            //$('input[name=EjecutaEstrategia' + $(this).attr('id') + ']').prop("checked", false);
            //$('input[name=Guarda' + $(this).attr('id') + ']').prop("checked", false);

            //$("#" + $(this).attr('id') + "StateGuid").val("" + "/" + $(this).attr('id'));
            //$('#EstadoAccion' + $(this).attr('id')).html('');
        }
    }
    catch (err) {
        alert(err);
    }
})

$('.chkStateLabel').click(function () {
    try {


        //Levanta dialog para pedir la página a la cual debe ir
        var nameAction = $("#" + $(this).attr('id') + "NameAction").val();

        var idFormPage = $("#FormPageId").val()
        var idForm = $("#FormId").val()
        var nameState = "ModificaEstado" + $(this).attr('name');

        if ($("input:checkbox[name='" + nameState + "']").is(':checked')) {
            $.ajax({
                type: "GET",
                url: "/FormPage/GetPagesStates/",
                data: {
                    'FormPageId': idFormPage,
                    'FormId': idForm,
                    'FormaActionId': $(this).attr('name')

                },
                dataType: "html",
                success: function (evt) {

                    $('#modalContentStates').dialog("open");
                    $('#modalContentStates').html(evt);

                },
                error: function (req, status, error) {
                    alert("Error!Occured" + error);
                }
            });
        } else {
            $('input[name=EjecutaEstrategia' + $(this).attr('id') + ']').attr("disabled", "disabled");
            $('input[name=Guarda' + $(this).attr('id') + ']').attr("disabled", "disabled");


            $('input[name=EjecutaEstrategia' + $(this).attr('id') + ']').prop("checked", false);
            $('input[name=Guarda' + $(this).attr('id') + ']').prop("checked", false);

            $("#" + $(this).attr('id') + "StateGuid").val("" + "/" + $(this).attr('id'));

        }
    }
    catch (err) {
        alert(err);
    }
})


$("#modalContent2").dialog({

    autoOpen: false,
    height: 150,
    width: 350,
    modal: true,
    buttons: {
        "Aceptar": function () {

            $.post("/FormPage/GetPagesOfForm", { id: $("#ddlPages").val(), actionId: $("#hiddenDate").val() }, function (data) {
                if (data.Success == true) {



                    $("#" + data.ActionId + "GoToPage").val(data.name + "/" + data.ActionId);
                    $("#ActionId").val(data.ActionId);
                }
                else alert("Insert failed");
            }, "json");
            $(this).dialog("close");
        }
    }
});
$("#modalContentStates").dialog({

    autoOpen: false,
    height: 200,
    width: 400,
    modal: true,
    buttons: {
        "Aceptar": function () {
            $.post("/FormPage/GetPagesStates", { id: $("#ddlPages").val(), actionId: $("#hiddenDate").val(), stateName: $("#ddlPages option:selected").text() }, function (data) {
                if (data.Success == true) {
                    $('#EstadoAccion' + data.ActionId).html(data.NameEstado);
                    $("#" + data.ActionId + "StateGuid").val(data.name + "/" + data.ActionId);
                    $("#ActionId").val(data.ActionId);
                }
                else alert("Insert failed");
            }, "json");
            $(this).dialog("close");
        }
    }
});

$("#modalContentStrategies").dialog({

    autoOpen: false,
    height: 200,
    width: 400,
    modal: true,
    buttons: {
        "Aceptar": function () {
            $.post("/FormPage/GetPagesStates", { id: $("#ddlPages").val(), actionId: $("#hiddenDate").val(), stateName: $("#ddlPages option:selected").text() }, function (data) {
                if (data.Success == true) {
                    $('#EstadoAccion' + data.ActionId).html(data.NameEstado);
                    $("#" + data.ActionId + "StateGuid").val(data.name + "/" + data.ActionId);
                    $("#ActionId").val(data.ActionId);
                }
                else alert("Insert failed");
            }, "json");
            $(this).dialog("close");
        }
    }
});


$("#NewAccion")
        .button()
        .click(function () {
            $('#NewAccion').attr("disabled", true);
            try {
                var idFormPage = $("#FormPageId").val()
                $.ajax({
                    type: "GET",
                    url: "/FormPage/AddAction/",
                    data: {
                        'FormPageId': idFormPage


                    },
                    dataType: "html",
                    success: function (evt) {

                        $('#modalAddActions').dialog("open");
                        $('#modalAddActions').html(evt);

                    },
                    error: function (req, status, error) {
                        alert("Error!Occured" + error);
                    }
                });
                return false;
            }
            catch (err) {
                alert(err);
            }
        })


$("#modalAddActions").dialog({

    autoOpen: false,
    height: 150,
    width: 350,
    modal: true,
    buttons: {
        "Aceptar": function () {

            var idFormPage = $("#FormPageId").val()

            //$.post("/FormPage/SaveAction", { srNameAction: $("#ddlPages :selected").text(), FormPageId: idFormPage }, function (data) {
            //    if (data.Success == true) {
            //        window.location.reload();
            //    }
            //    else alert("Insert failed");
            //}, "json");
            //$(this).dialog("close");
            $.ajax({
                type: 'POST',
                url: '/FormPage/SaveAction',
                data: {
                    'srNameAction': $("#ddlAcciones :selected").text(),
                    'FormPageId': idFormPage
                },
                dataType: 'json',
                success: function (result) {

                    window.location.href = '/FormPage/FormPageAction/' + idFormPage;
                    $("#modalAddActions").dialog('close');
                },
                error: function (req, status, error) {
                    alert("Error: " + error);
                }
            });
        }
    }


});