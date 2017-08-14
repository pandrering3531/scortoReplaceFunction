$(".CustomInteger").numeric(false, function () { this.focus(); });
$('.CustomCurrency').numeric(false);

function inactivaLabelFileUpload() {

    $('input[type=button]').each(function () {

        if ($(this).attr("onclick") != undefined) {
            if ($(this).is(":disabled")) {

                var res = $(this).attr('id').split("STPC_DFi_");
                $('#UriFile_STPC_DFi_' + res[1]).prop('disabled', true);
                $('#UriFile_STPC_DFi_' + res[1]).removeClass('notFilled');
                $('#UriFile_STPC_DFi_' + res[1]).removeClass('required');
            }
        }
    })
}

///file upload

function CreacteDivDialogFileUpload(id) {

    $('#divFileUpload').html("");
    $('#divFileUpload').append('<label>Seleccione el archivo que desea cargar</label><br />');
    $('#divFileUpload').append('<input type="file" id=fileUp' + id + ' name="fileUp" value="Examinar" onchange="UploadFile()" />');
    $('#divFileUpload').append('<input type="hidden" id="MaxSizeHidden" name="MaxSizeHidden" /> ')
    $('#divFileUpload').append('<input type="hidden" id="ValidExtensionsHidden" name="ValidExtensionsHidden" /> ');
    $('#divFileUpload').append('<input type="hidden" id="ErrorExtensionsHidden" name="ErrorExtensionsHidden" /> ');
    $('#divFileUpload').append('<br />');

}

function ModalFileUpload(sender, MaxSize, ext, extError) {
    CreacteDivDialogFileUpload(sender.id);

    $('#fileUp' + sender.id).fileupload();
    $('#fileUp' + sender.id).fileupload({
        url: null,
        autoUpload: false,
        dataType: 'json',
        maxNumberOfFiles: 1,
        maxChunkSize: undefined,
        limitConcurrentUploads: undefined,
        sequentialUploads: undefined,

        add: function (e, data) {
            blockScreen();
            //Valida el tamaño del archivo
            if (data.originalFiles[0]['size'] != null && data.originalFiles[0]['size'] != undefined && (data.originalFiles[0]['size'] / 1024) > MaxSize) {
                alert('Tamaño del archivo superior a ' + MaxSize + ' bytes');
            }
            //Valida la extención del archivo
            var extencion = data.files[0].name.split(".");

            if (extencion.length > 1) {
                var extUploadedFile = extencion[1];
                if (extUploadedFile == "tiff") {
                    extUploadedFile = "tif"
                }
                if (ext.indexOf(extUploadedFile) > -1) {
                    UploadFile(data);
                }
                else {
                    if (ext != "") {
                        alert("Formato de archivo no válido")
                        $.unblockUI();
                    }
                    else {
                        UploadFile(data);
                    }
                }
            }
            else {
                alert("Error leyendo el archivo, intente de nuevo")
                $.unblockUI();
            }

        },

        done: function (e, data) {
            //Progresa bar
            $.each(data.result, function (index, file) {
                $('<p/>').text(file.name).appendTo(document.body);
            });


            var responseText = data.result.FileName

            //data.context = $('<p/>').text('Uploading...').appendTo(document.body);
            alert('Se ha cargado el archivo de forma exitosa.');
            var urlSite = window.location.origin = window.location.protocol + "//" + window.location.hostname + (window.location.port ? ':' + window.location.port : '');

            $("#UriFile_" + $("#txtSender").val()).attr('href', urlSite + '/FormPage/GetPrivateResource?resource=' + responseText);
            $("#UriFile_" + $("#txtSender").val()).attr('text', urlSite + '/FormPage/GetPrivateResource?resource=' + responseText);
            $("#UriFile_" + $("#txtSender").val()).attr('value', urlSite + '/FormPage/GetPrivateResource?resource=' + responseText);
            $("#UriFile_" + $("#txtSender").val()).html("Ver")


            $("#UriFileDelete_" + $("#txtSender").val()).attr('text', urlSite + '/FormPage/GetPrivateResource?resource=' + responseText);
            $("#UriFileDelete_" + $("#txtSender").val()).attr('value', urlSite + '/FormPage/GetPrivateResource?resource=' + responseText);
            $("#UriFileDelete_" + $("#txtSender").val()).attr('onClick', "DeleteFileLink(this);");
            $("#UriFileDelete_" + $("#txtSender").val()).html("Borrar")


            $("#UriFile_" + $("#txtSender").val()).removeClass('notFilled');
            $("#UriFile_" + $("#txtSender").val()).removeAttr('style');
            $("#UriFile_" + $("#txtSender").val()).attr('style', 'visibility: visible');
            $("#hidden_" + $("#txtSender").val()).attr('value', responseText);

            $.unblockUI();
        },

        error: function (e, data) {
            console.log('error')
            alert(e.responseText);
            $.unblockUI();
        },


    }).bind('fileuploadfail', function (e, data) {

    });
    $("#txtSender").val(sender.name);
    $("#MaxSizeHidden").val(MaxSize);
    $("#ValidExtensionsHidden").val(ext);
    $("#ErrorExtensionsHidden").val(extError);
    $("#modalContents").dialog({ height: 140, modal: true, autoOpen: true, resizable: false, close: dialogClose });
    $('#fileUp' + sender.id).click();
}

function dialogClose() {

}

function UploadFile(data) {

    if (data) {
        data.url = "../FormPage/SaveFile";
        //data.context = $('<p/>').text('Uploading...').appendTo(document.body);
        $("#modalContents").dialog('close');
        data.submit();


    }

}

// pre-submit callback 
function showRequest(formData, jqForm, options) {
    try {
        blockScreen();
        $("#modalContents").dialog('close');
        var queryString = $.param(formData);
        return true;
    }
    catch (err) {
        $.unblockUI();
    }
}

// post-submit callback 
function showResponse(responseText, statusText, xhr, $form) {
    if (responseText != null) {
        // TODO: AQUI, COMO HACER LLEGAR EL SENDER
        // validar el mensaje

        if (responseText.indexOf("Ha ocurrido") >= 0) {
            alert(responseText);
            //var urlSite = window.location.origin = window.location.protocol + "//" + window.location.hostname + (window.location.port ? ':' + window.location.port : '');
            //location.href = urlSite
        }
        else {
            if (responseText.indexOf("Username") >= 0) {
                var urlSite = window.location.origin = window.location.protocol + "//" + window.location.hostname + (window.location.port ? ':' + window.location.port : '');
                location.href = urlSite
            }
            else {
                alert('Se ha cargado el archivo de forma exitosa.');
                var urlSite = window.location.origin = window.location.protocol + "//" + window.location.hostname + (window.location.port ? ':' + window.location.port : '');

                $("#UriFile_" + $("#txtSender").val()).attr('href', urlSite + '/FormPage/GetPrivateResource?resource=' + responseText);
                $("#UriFile_" + $("#txtSender").val()).attr('text', urlSite + '/FormPage/GetPrivateResource?resource=' + responseText);
                $("#UriFile_" + $("#txtSender").val()).attr('value', urlSite + '/FormPage/GetPrivateResource?resource=' + responseText);
                $("#UriFile_" + $("#txtSender").val()).html("Ver")


                $("#UriFileDelete_" + $("#txtSender").val()).attr('text', urlSite + '/FormPage/GetPrivateResource?resource=' + responseText);
                $("#UriFileDelete_" + $("#txtSender").val()).attr('value', urlSite + '/FormPage/GetPrivateResource?resource=' + responseText);
                $("#UriFileDelete_" + $("#txtSender").val()).attr('onClick', "DeleteFileLink(this);");
                $("#UriFileDelete_" + $("#txtSender").val()).html("Borrar")


                $("#UriFile_" + $("#txtSender").val()).removeClass('notFilled');
                $("#UriFile_" + $("#txtSender").val()).removeAttr('style');
                $("#UriFile_" + $("#txtSender").val()).attr('style', 'visibility: visible');
                $("#hidden_" + $("#txtSender").val()).attr('value', responseText);

            }
        }

        $.unblockUI();
        $('#divFileUpload').html("");

    }

    updateSuccess();
}


function DeleteFileLink(objControl) {
    var res = objControl.id.split("UriFileDelete_");

    $("#UriFile_" + res[1]).removeAttr('text');
    $("#UriFile_" + res[1]).removeAttr('href');
    $("#UriFile_" + res[1]).removeAttr('value');
    $("#UriFile_" + res[1]).html("Pendiente");

    $('#UriFileDelete_' + res[1]).html(" ");
    var FormPageUid = $("#FormPageUid").val();

    $("#hidden_" + res[1]).removeAttr('value');


}


function CargarFormulas() {
    try {


        $.ajax({
            type: "POST",
            url: "/FormPage/GetMathExpresion/",
            dataType: "json",
            data: {
                FormPageUid: $("#FormPageUid").val()
            },
            success: function (data) {

                for (var i = 0; i < data.length; i = i + 2) {
                    try {
                        $("input[type=text]").each(function () {
                            var idControl2 = $(this).attr('id').split('STPC_DFi_');
                            var n = data[i].search(idControl2[1]);
                            var exp = data[i];
                            var ctrlResult = data[i + 1];
                            if (n > 0) {
                                if ($("#STPC_DFi_" + idControl2[1]).is('[disabled=disabled]')) {

                                    $("#STPC_DFi_" + idControl2[1]).bind("change", function () {

                                        //execMathExpresion(exp, ctrlResult)
                                        execMathExpresion(exp, ctrlResult)
                                    });
                                }

                                $("#STPC_DFi_" + idControl2[1]).bind("blur", function () {

                                    execMathExpresion(exp, ctrlResult)


                                });
                            }

                        });
                    } catch (e) {
                        alert(e.message)
                    }
                }
            },
            error: function (result) {
                if (result.status == 0) {
                    alert("Se ha realizado un redirecionamiento inválido sin concluir la carga de la solicitud")
                }
                else
                    alert(result.status);
            }
        });
    } catch (e) {
        alert(e.message)
    }
}

function execMathExpresion(expresion, resultId) {
    try {
        var math = mathjs()

        //Recorro los input
        $("input[type=text]").each(function () {
            var idControl2 = $(this).attr('id').split('STPC_DFi_');
            var n = expresion.search(idControl2[1]);
            var val;
            var val2;

            //if (n > 0) {
            val = $(this).val().replace("$", "")

            var className = $('#STPC_DFi_' + idControl2[1]).attr('class');

            if (className != undefined) {
                if (/CustomCurrency/.test(className)) {
                    val2 = val.replace(/\./g, "")

                }
                else {
                    val2 = val.replace(/\,/g, ".")
                }

                if (val2.trim() == "") {
                    val2 = "0";
                }
                expresion = expresion.replace(new RegExp(idControl2[1], 'g'), val2);
            }
        });
        var className = $('#STPC_DFi_' + resultId).attr('class');

        if (/CustomInteger/.test(className) || /CustomDecimal/.test(className)) {
            $("#STPC_DFi_" + resultId).val(math.eval(expresion).toFixed(2));
            $("#STPC_DFi_" + resultId).val(math.eval(expresion).toFixed(2)).trigger('change');
        }
        else {
            if (/CustomCurrency/.test(className)) {
                $("#STPC_DFi_" + resultId).val(formatMoney(math.eval(expresion)));
                $("#STPC_DFi_" + resultId).val(formatMoney(math.eval(expresion))).trigger('change')
            }
            else {
                $("#STPC_DFi_" + resultId).val(math.eval(expresion)).trigger('change');
            }
        }
        if ($("#STPC_DFi_" + resultId).val() == "Infinity" || $("#STPC_DFi_" + resultId).val() == "NaN" || $("#STPC_DFi_" + resultId).val() == "-Infinity") {
            $("#STPC_DFi_" + resultId).val(0);
        }
    }
    catch (err) {
        alert(err)
        $("#STPC_DFi_" + resultId).val("");
    }
}

function HideControlsById(result) {

    console.log('HideControlsById 1')
    for (x = 0; x < result.length; x++) {


        if ($('#STPC_DFi_' + result[x].ListenerFieldId).attr('type') != "button") {
            $('#UriFile_STPC_DFi_' + result[x].ListenerFieldId).val("");

        }

        $('#UriFile_STPC_DFi_' + result[x].ListenerFieldId).attr('href', "");
        $('#UriFile_STPC_DFi_' + result[x].ListenerFieldId).attr('text', "");
        $("#hidden_STPC_DFi_" + result[x].ListenerFieldId).attr('value', "");


        if (result[x].EventType == "Hide") {
            $('#STPC_DFi_' + result[x].ListenerFieldId).addClass("inputHidden")
            $('#STPC_DFi_' + result[x].ListenerFieldId).find("input:text, input:password, input:file, select, textarea, li,ul,a").removeClass('notFilled');
            $('#STPC_DFi_' + result[x].ListenerFieldId).find("input:text, input:password, input:file, select, textarea, li,ul,a").removeClass('requiredRadioButon');
            $('#STPC_DFi_' + result[x].ListenerFieldId).find("select, textarea, li,ul").addClass('Not_requiredRadioButon');
            $('#STPC_DFi_' + result[x].ListenerFieldId).find("input:text, input:password, input:file, textarea").addClass('Not_required');
            $('#STPC_DFi_' + result[x].ListenerFieldId).val("");
            $('#STPC_DFi_' + result[x].ListenerFieldId).find("input:text, input:password, input:file, select, textarea, li,ul").val("");
            $('#UriFile_STPC_DFi_' + result[x].ListenerFieldId).css("display", "none")
            $('#UriFileDelete_STPC_DFi_' + result[x].ListenerFieldId).css("display", "none")
            $('#UriFile_STPC_DFi_' + result[x].ListenerFieldId).removeClass('notFilled');
            $('#UriFile_STPC_DFi_' + result[x].ListenerFieldId).html("Pendiente");
            $('#UriFile_STPC_DFi_' + result[x].ListenerFieldId).val("");
            

            $('#UriFile_STPC_DFi_' + result[x].ListenerFieldId).removeClass("required");
            $('#UriFile_STPC_DFi_' + result[x].ListenerFieldId).removeClass("requiredRadioButon");
            $('#UriFile_STPC_DFi_' + result[x].ListenerFieldId).addClass("Not_required");

            $("label[for=" + "STPC_DFi_" + result[x].ListenerFieldId + "]").css("display", "none")

            if ($('#STPC_DFi_' + result[x].ListenerFieldId).length > 0)
                var LocalName = $('#STPC_DFi_' + result[x].ListenerFieldId)[0].localName;

            if (LocalName != "ul")
                $("#STPC_DFi_" + result[x].ListenerFieldId + ":input").attr("disabled", true);
            else {
                $("#" + $('#STPC_DFi_' + result[x].ListenerFieldId).attr('id') + ' li input').each(function (idx1, chele) {
                    $(this).css("display", "none")

                    $(this).checked = false;
                });
            }
        }

        if (result[x].EventType == "Disabled") {

            if (!$('#STPC_DFi_' + result[x].ListenerFieldId).is(':disabled')) {
                if ($('#STPC_DFi_' + result[x].ListenerFieldId).attr('type') != "button") {
                    $('#STPC_DFi_' + result[x].ListenerFieldId).val("");
                    $('#STPC_DFi_' + result[x].ListenerFieldId).find("input:text, input:password, input:file, select, textarea, li,ul").val("");
                }

            }

            $('#STPC_DFi_' + result[x].ListenerFieldId).prop('disabled', true);
            $('#STPC_DFi_' + result[x].ListenerFieldId).find("input:text, input:password, input:file, select, textarea, li,ul,a").removeClass('notFilled');
            $('#STPC_DFi_' + result[x].ListenerFieldId).find("input:text, input:password, input:file, select, textarea, li,ul,a").removeClass('requiredRadioButon');
            $('#STPC_DFi_' + result[x].ListenerFieldId).find("input:text, input:password, input:file, select, textarea, li,ul,a").prop('disabled', true);
            $('#STPC_DFi_' + result[x].ListenerFieldId).find("input:text, input:password, input:file, select, textarea, li,ul,a").addClass('Not_requiredRadioButon');

            $('#STPC_DFi_' + result[x].ListenerFieldId).removeClass('notFilled');

            $('#UriFile_STPC_DFi_' + result[x].ListenerFieldId).prop('disabled', true);
            $('#UriFile_STPC_DFi_' + result[x].ListenerFieldId).removeClass('notFilled');
            $('#UriFileDelete_STPC_DFi_' + result[x].ListenerFieldId).prop('disabled', true);
            $("label[for=" + "STPC_DFi_" + result[x].ListenerFieldId + "]").prop('disabled', true);
            $('#UriFile_STPC_DFi_' + result[x].ListenerFieldId).html("Pendiente");
            $('#UriFile_STPC_DFi_' + result[x].ListenerFieldId).removeAttr("href");
            $('#UriFile_STPC_DFi_' + result[x].ListenerFieldId).val("");
            $('#UriFileDelete_STPC_DFi_' + result[x].ListenerFieldId).val("");
            $('#UriFileDelete_STPC_DFi_' + result[x].ListenerFieldId).html("");
            if ($('#STPC_DFi_' + result[x].ListenerFieldId).length > 0)
                var LocalName = $('#STPC_DFi_' + result[x].ListenerFieldId)[0].localName;

            if (LocalName != "ul")
                $("#STPC_DFi_" + result[x].ListenerFieldId + ":input").attr("disabled", true);
            else {
                $("#" + $('#STPC_DFi_' + result[x].ListenerFieldId).attr('id') + ' li input').each(function (idx1, chele) {
                    $(this).attr("disabled", true);
                    $(this).attr('checked', false);

                });
            }
            //$("label[for=" + "STPC_DFi_" + result[x].ListenerFieldId + "]").attr('disabled', true);
        }
        else {
            var childControl = $('#STPC_DFi_' + result[x].ListenerFieldId).prop("tagName");
        }
        var className = $('#STPC_DFi_' + result[x].ListenerFieldId).attr('class');
        var classNameUriFile = $('#UriFile_STPC_DFi_' + result[x].ListenerFieldId).attr('class');

        if (/required/.test(className)) {
            $('#STPC_DFi_' + result[x].ListenerFieldId).removeClass("required");
            $('#STPC_DFi_' + result[x].ListenerFieldId).addClass("Not_required");
            $('#UriFile_STPC_DFi_' + result[x].ListenerFieldId).addClass("Not_required");
            $('#UriFile_STPC_DFi_' + result[x].ListenerFieldId).removeClass("required");


        }
        if (/required/.test(classNameUriFile)) {
            $('#UriFile_STPC_DFi_' + result[x].ListenerFieldId).addClass("Not_required");
            $('#UriFile_STPC_DFi_' + result[x].ListenerFieldId).removeClass("required");


        }
        if (/requiredcheckBox/.test(className)) {
            $('#STPC_DFi_' + result[x].ListenerFieldId).removeClass("requiredcheckBox");
            $('#STPC_DFi_' + result[x].ListenerFieldId).addClass("Not_requiredcheckBox");
        }
        if (/requiredRadioButon/.test(className)) {
            $('#STPC_DFi_' + result[x].ListenerFieldId).removeClass("requiredRadioButon");
            $('#STPC_DFi_' + result[x].ListenerFieldId).addClass("Not_requiredRadioButon");
        }
    }
    //$.unblockUI();
}

function onloadEvents() {

    $(".Cascade, .Disabled, .Hide").each(function () {

        if ($(this).attr("onchange") != undefined) {
            var n = $(this).attr("onchange").split(";")
            if (n.length == 0) {
                if ($(this).val() != "")
                    eval($(this).attr("onchange"))
            }
            else
                if ($(this).val() != "")
                    eval(n[0])
        }
        if ($(this).attr("onclick") != undefined) {
            var n = $(this).attr("onclick").split(";")
            if ($(this).attr("onclick") != "ExecuteStrategy")
                if ($(this).is("ul")) {
                    if ($('#' + $(this).attr('id') + ' li input[type=radio]').attr('checked') != undefined) {
                        if (n.length == 0)
                            eval($(this).attr("onclick"))
                        else {
                            eval(n[0])

                        }
                    }
                    else {
                        console.log('opciones sin seleccion, no inhabilita')
                    }
                }
                else {
                    if (n.length == 0)
                        eval($(this).attr("onclick"))
                    else
                        eval(n[0])

                }
        }

    });
}

function traernombreusuario() {

    $(".basico1Usuario_input").each(function () {

        if ($(this).val() == "") {

            if ($(this).prop("readonly") == false) {

                $(this).val($("#bienvenida").text().substring(10, $("#bienvenida").text().indexOf("Última")));
                $(this).prop("readonly", true);
                $(this).css("background-color", "#EBEBE4");


            }
            else {
                $(this).prop("readonly", true);
            }
        }


    });

}

function ExecuteStrategy(sender, inputName, PageStrategyUid) {
    var form = $("#frmPage");
    //$("#" + inputName).attr("disabled", "disabled");

    (function ($) {
        $.fn.serializeAll = function () {
            var data = $(this).serializeArray();

            $(':disabled[name]', this).each(function () {
                data.push({ name: this.name, value: $(this).val() });
            });

            return data;
        }
    })(jQuery);


    var data = form.serializeAll();

    $.ajax({

        type: 'POST',
        url: '/Strategy/ExecuteStrategy',
        data: {
            'TriggerFieldUid': inputName,
            'PageStrategyUid': PageStrategyUid,
            'data': data
        },
        dataType: 'json',
        success: function (result) {
            if (result != null && result != '') {
                var arr = result.split("/");

                for (var i = 0; i < arr.length; i++) {
                    if (arr[i] != null && arr[i] != '') {
                        var res = arr[i].split("|");
                        if (res.length >= 2) {

                            var val = $("#STPC_DFi_" + res[0]).val();
                            val = val + res[1];
                            $("#STPC_DFi_" + res[0]).val(res[1])
                            var tr = $.trim(res[0]);

                            $("#STPC_DFi_" + tr).text("")
                            $("#STPC_DFi_" + tr).val("")
                            $("#STPC_DFi_" + tr).attr('value', "")

                            console.log(res[1])

                            $("#STPC_DFi_" + tr).text(res[1])
                            $("#STPC_DFi_" + tr).val(res[1])
                            $("#STPC_DFi_" + tr).attr('value', res[1])


                            if ($("#STPC_DFi_" + res[0]).attr("onchange") != undefined) {

                                var parent = $("#STPC_DFi_" + res[0]).parent()

                                parent.children('input').each(function () {
                                    console.log('ExecuteStrategy')
                                    eval($("#STPC_DFi_" + res[0]).attr("onchange"))
                                });

                            }

                            if (tiene_letras(val) == 0) {
                                if ($("#STPC_DFi_" + res[0]).hasClass("CustomCurrency"))
                                    applyFormatCurrency($("#STPC_DFi_" + res[0]));
                            }

                        }
                    }
                }

                //$("#StrategyResponse" + sender.name).text(result)
            }

            $("#" + inputName).attr("disabled", false);
        },
        error: function (req, status, error) {
            alert("Error: " + error);
        }
    });
}

function HideControl(sender, ChildrenIdControl, ControlEvent) {

    console.log('hidcontrol')
    var inputs = $('#' + ChildrenIdControl + ' li input')
    var allVals = [];
    allVals[0] = $("#" + $(sender).attr('id') + " option:selected").val();
    var valueThis;
    var classObject = $(sender).attr('class');

    if ($(sender).get(0).tagName == "INPUT") {

        valueThis = $("#" + $(sender).attr('id')).val()
    }
    if ($(sender).get(0).tagName == "SELECT") {
        valueThis = $("#" + $(sender).attr('id') + " option:selected").val()

    }
    if (/Hide/.test(classObject) || /Disabled/.test(classObject)) {

        $.ajax({
            type: 'POST',
            url: '/Strategy/GetShowControls',
            data: {
                'UidControl': $(sender).attr('id'),
                'valueControl': ''
            },
            dataType: 'json',
            success: function (resultShowControls) {

                RestoreControls(resultShowControls);

                $.ajax({
                    type: 'POST',
                    url: '/Strategy/GetHiddenControls',
                    data: {
                        'UidControl': $(sender).attr('id'),
                        'valueControl': valueThis
                    },
                    dataType: 'json',
                    success: function (result) {

                        HideControlsById(result);


                    },
                    error: function (req, status, error) {
                        alert("Error: " + error);
                    }
                });

            },
            error: function (req, status, error) {
                alert("Error: " + error);
            }
        });
        //else {
    }
    var checkBox = inputs[0];
    $.ajax({
        async: false,
        type: 'POST',
        url: '/Strategy/GetListCascadeControls',
        data: {
            'UidControl': $(sender).attr('name'),
            'valueControl': $(sender).val()
        },
        dataType: 'json',
        success: function (result) {
            var myArray = new Array();
            for (x = 0; x < result.length; x++) {
                var fer = result[x].EventType;

                //Busca controles
                var childControl = $("#STPC_DFi_" + result[x].ListenerFieldId).prop("tagName");
                if (childControl == "UL")
                    LoadChildRadioList(checkBox, $("#STPC_DFi_" + result[x].ListenerFieldId).attr('id'), allVals, sender);
                else {

                    var found = $.inArray($("#STPC_DFi_" + result[x].ListenerFieldId).attr('id'), myArray)
                    if (found == -1)
                        LoadChildDropDown(sender, $("#STPC_DFi_" + result[x].ListenerFieldId).attr('id'));
                    myArray.push($("#STPC_DFi_" + result[x].ListenerFieldId).attr('id'))
                }
            }

        },
        error: function (req, status, error) {
            alert("Error: " + error);
        }
    });

}

function RestoreControls(result) {
    for (x = 0; x < result.length; x++) {

        if (result[x].EventType == "Hide") {
            //$('#STPC_DFi_' + result[x].ListenerFieldId).css('visibility', 'visible').is(':hidden') == true
            $('#STPC_DFi_' + result[x].ListenerFieldId).removeAttr("style");
            $('#STPC_DFi_' + result[x].ListenerFieldId).removeClass("inputHidden")
            $("label[for=" + "STPC_DFi_" + result[x].ListenerFieldId + "]").css('visibility', 'visible');
            $("label[for=" + "STPC_DFi_" + result[x].ListenerFieldId + "]").removeAttr("style");
            $('#UriFile_STPC_DFi_' + result[x].ListenerFieldId).removeAttr("style");
            $('#UriFile_STPC_DFi_' + result[x].ListenerFieldId).css('visibility', 'visible').is(':hidden');
            $('#UriFileDelete_STPC_DFi_' + result[x].ListenerFieldId).css('visibility', 'visible').is(':hidden');

            


            if ($('#STPC_DFi_' + result[x].ListenerFieldId).length > 0)
                var LocalName = $('#STPC_DFi_' + result[x].ListenerFieldId)[0].localName;
            if (LocalName == "ul") {
                $("#" + $('#STPC_DFi_' + result[x].ListenerFieldId).attr('id') + ' li input').each(function (idx1, chele) {
                    $(this).css('visibility', 'visible').is(':hidden');
                    $(this).removeAttr("style");
                });
            }
        }
        else {

            if ($('#STPC_DFi_' + result[x].ListenerFieldId).attr('readonly') == undefined) {
                $('#STPC_DFi_' + result[x].ListenerFieldId).prop('disabled', false);
                $('#STPC_DFi_' + result[x].ListenerFieldId).find("input:text, input:password, input:file, select, textarea, li,ul").prop('disabled', false);
            }

            if ($('#STPC_DFi_' + result[x].ListenerFieldId).length > 0)
                var LocalName = $('#STPC_DFi_' + result[x].ListenerFieldId)[0].localName;

            if (LocalName != "ul") {
                if ($('#STPC_DFi_' + result[x].ListenerFieldId).attr('readonly') == undefined)
                    $("#STPC_DFi_" + result[x].ListenerFieldId + ":input").attr("disabled", false);
                else {
                    $("#" + $('#STPC_DFi_' + result[x].ListenerFieldId).attr('id') + ' li input').each(function (idx1, chele) {
                        $(this).attr("disabled", false);
                    });
                }
            }
            else {
                $("#" + $('#STPC_DFi_' + result[x].ListenerFieldId).attr('id') + ' li input').each(function (idx1, chele) {
                    $(this).attr("disabled", false);
                });
            }
        }

        var className = $('#STPC_DFi_' + result[x].ListenerFieldId).attr('class');
        var classNameUriFile = $('#UriFile_STPC_DFi_' + result[x].ListenerFieldId).attr('class');
        $('#STPC_DFi_' + result[x].ListenerFieldId).find("input:text, input:password, input:file, select, textarea, li,ul").addClass('requiredRadioButon');

        $('#STPC_DFi_' + result[x].ListenerFieldId).find("input:text, input:password, input:file, select, textarea, li,ul").removeClass('Not_requiredRadioButon');

        if (/Not_required/.test(className)) {
            $('#STPC_DFi_' + result[x].ListenerFieldId).removeClass("Not_required");
            $('#STPC_DFi_' + result[x].ListenerFieldId).addClass("required");

            $('#UriFile_STPC_DFi_' + result[x].ListenerFieldId).removeClass("Not_required");
            $('#UriFile_STPC_DFi_' + result[x].ListenerFieldId).addClass("required");

        }
        if (/Not_requiredcheckBox/.test(className)) {

            $('#STPC_DFi_' + result[x].ListenerFieldId).removeClass("Not_requiredcheckBox");
            $('#STPC_DFi_' + result[x].ListenerFieldId).addClass("requiredcheckBox");
            $('#UriFile_STPC_DFi_' + result[x].ListenerFieldId).removeClass("Not_required");
            $('#UriFile_STPC_DFi_' + result[x].ListenerFieldId).removeClass("required");
        }
        if (/requiredRadioButon/.test(className)) {

            $('#STPC_DFi_' + result[x].ListenerFieldId).removeClass("requiredRadioButon");
            $('#STPC_DFi_' + result[x].ListenerFieldId).addClass("requiredRadioButon");
            $('#UriFile_STPC_DFi_' + result[x].ListenerFieldId).removeClass("Not_required");
            $('#UriFile_STPC_DFi_' + result[x].ListenerFieldId).removeClass("required");

        }

        if (/Not_required/.test(classNameUriFile)) {
            $('#UriFile_STPC_DFi_' + result[x].ListenerFieldId).removeClass("Not_required");
            $('#UriFile_STPC_DFi_' + result[x].ListenerFieldId).addClass("required");

        }


    }
}

function HideCheckListControl(sender, ChildrenIdControl, FiedlValue) {
    console.log('hidcontrol')
    if ($('#' + sender.id).attr("disabled") == "disabled") {
        return;
    }
    var inputs = $('#' + sender.id + ' li input')
    var controlsChild = ChildrenIdControl.split(",");
    //var classObject = $(inputs).attr('class');
    var classObject = $(sender).attr('class');
    var allVals = [];
    var GlobalResult;

    console.log("HideCheckListControl")
    //HideChildControls(sender.id)

    if (inputs.length > 0) {
        var checkBox = inputs[0];
        var values = "";
        var i = 0;
        var values = $("#" + sender.id + ' input:checkbox:checked').map(function () {
            allVals[i] = this.value;
            i++;
        }).get();
        values = $("#" + sender.id + ' input:radio:checked').map(function () {
            allVals[i] = this.value;
            i++;
        }).get();

        $.ajax({
            type: 'POST',
            url: '/Strategy/GetListCascadeControls',
            data: {
                'UidControl': inputs[0].name,
                'valueControl': inputs[0].value,
            },
            dataType: 'json',
            success: function (result) {

                for (x = 0; x < result.length; x++) {
                    var fer = result[x].EventType;

                    //Busca controles
                    var childControl = $("#STPC_DFi_" + result[x].ListenerFieldId).prop("tagName");


                    GlobalResult = result;
                    if (childControl == "SELECT")
                        LoadChildDropDownFromCheckBoxList(checkBox, $("#STPC_DFi_" + result[x].ListenerFieldId).attr('id'), allVals);
                    if (childControl == "UL")
                        LoadChildRadioList(checkBox, $("#STPC_DFi_" + result[x].ListenerFieldId).attr('id'), allVals, sender);
                    else
                        LoadChildRadioList(checkBox, $("#STPC_DFi_" + result[x].ListenerFieldId).attr('id'), allVals);

                }
            },
            error: function (req, status, error) {
                alert("Error: " + error);
            }
        });
    }
    $.ajax({
        type: 'POST',
        url: '/Strategy/GetShowControls',
        data: {
            'UidControl': $(sender).attr('id'),
            'valueControl': ''
        },
        dataType: 'json',
        success: function (resultShowControls) {

            RestoreControls(resultShowControls);
            for (x = 0; x < inputs.length; x++) {

                if (inputs[x].checked) {
                    if (/Hide/.test(classObject) || /Disabled/.test(classObject)) {
                        $.ajax({
                            type: 'POST',
                            url: '/Strategy/GetHiddenControls',
                            data: {
                                'UidControl': inputs[x].name,
                                'valueControl': inputs[x].value,
                            },
                            dataType: 'json',
                            success: function (result) {
                                if (result.length) {

                                    HideControlsById(result);

                                }


                            },
                            error: function (req, status, error) {
                                alert("Error: " + error);
                            }
                        });
                    }
                }

            }
        }
    });

}

function LoadChildDropDown(DdlParentControl, ddlChildControl) {
    var ParentControl = $("#" + DdlParentControl.id + " option:selected").val();
    var selecteValueChildList = $("#" + ddlChildControl).val();


    if (ParentControl != '' && ParentControl != undefined) {
        // $(".btnAtions").attr("disabled", "disabled");
        var select = $("#" + ddlChildControl);

        select.append($('<option/>', {
            value: "",
            text: "Cargando...",
            selected: true
        }));

        $.ajax({

            type: "GET",
            url: "/Strategy/GetItemOfChildSelect/",
            dataType: "json",
            data: {
                ChildControl: ddlChildControl,
                Value: ParentControl
            },
            success: function (carData) {
                select.empty();
                select.append($('<option/>', {
                    value: "",
                    text: ""
                }));

                $.each(carData, function (index, itemData) {
                    if (itemData.Value == selecteValueChildList)
                        select.append($('<option/>', {
                            value: itemData.Value,
                            text: itemData.Text,
                            selected: true
                        }));
                    else
                        select.append($('<option/>', {
                            value: itemData.Value,
                            text: itemData.Text,
                            selected: false

                        }));
                });

                //if ($("#" + ddlChildControl)[0].attributes.onchange != undefined) {
                //    var arg = $("#" + ddlChildControl)[0].attributes.onchange.nodeValue.split(",");
                //    var patron = "'"
                //    var ControlChildId = arg[1].replace(patron, '');
                //    var ControlChildId = ControlChildId.replace(patron, '');
                //    LoadChildDropDown($("#" + ddlChildControl)[0], ControlChildId, "");
                //}
            },
            error: function (result, status, error) {
                alert("Error")
            }
        });


    }
    else {
        var select = $("#" + ddlChildControl);

        select.empty();

    }
    //   $(".btnAtions").removeAttr('disabled');
}




function applyFormatCurrency(sender) {
    try {

        $(sender).formatCurrency({

            roundToDecimalPlace: 0, negativeFormat: '-%s%n', decimalSymbol: ',', digitGroupSymbol: '.'
        });
    } catch (e) {
        alert(e.message);
    }

}

$(function () {
    $(".btnAtions").button().click(function () {
        console.log("btnAtions");
        var isSave = $(this).attr('name').split("/");


        var isSaveWithOutValidate = isSave[0] =="GuardarSinValidar";
        if (isSaveWithOutValidate == true) {
            var n = $(this).attr('name').split("/");
            var nId = $(this).attr('id').split("/");


            executeEvent(n[0], n[1], n[2], nId[1]);
        }
        else {
            if (isSave[1] == "True") {
                var op = 0;
                var op1 = 0;
                var op2 = 0;
                $('.ValidateMinValue:enabled').each(function (idx1, chele) {

                    var className = $(this).attr('class');
                    var valuesText2 = $(this).val().replace("$", '');

                    if (/CustomInteger/.test(className) || /CustomDecimal/.test(className))
                        valuesText2 = valuesText2.replace(/\,/g, '')
                    if (/CustomCurrency/.test(className))
                        valuesText2 = valuesText2.replace(/\./g, '')

                    var minValue = parseInt($(this).prop('min'));
                    var value = parseInt(valuesText2);


                    if (minValue > value) {
                        alert("Ingrese un valor mayor o igual a: " + minValue)
                        $(this).addClass('notFilled');
                        return;
                    }
                    else
                        $(this).removeClass('notFilled');


                    var MaxValue = parseInt($(this).prop('max'));

                    if (MaxValue < value) {
                        alert("Ingrese un valor menor o igual a: " + MaxValue)
                        $(this).addClass('notFilled');
                        return;

                    }
                    else
                        $(this).removeClass('notFilled');
                });

                $('.email').each(function (idx1, chele) {
                    if (!validateEmail($(this).val().trim()) && $(this).val() != "") {
                        $(this).addClass('notFilled');
                    }
                });

                $('.hasDatepicker').each(function (idx1, chele) {
                    if (!ValidateDate($(this).val()) && $(this).val() != "") {
                        $(this).addClass('notFilled');
                    }
                });

                $("#frmPage").find('.notFilled').live('keyup', function () {

                    if ($(this).val() != "") {
                        $(this).removeClass('notFilled');
                    }
                });


                $("#frmPage").find('.notFilled').live('change', function () {
                    if ($(this).val() != "") {
                        $(this).removeClass('notFilled');
                    }
                });

                $("#frmPage").find('.required').each(function () {
                    if ($(this).attr('value') == '' || $.trim($(this).attr('value')) == '$') {
                        if ($(this).is(':disabled') == false && $(this).is(':visible') == true) {
                            $(this).addClass('notFilled');
                            op = 1;
                            var className = $(this).attr('class');
                            if (/FileView /.test(className))
                                $(this).html("Pendiente")
                        }
                    }
                });

                $('input:checkbox.required').each(function () {
                    if ($(this).is(":checked") == false) {

                        var idControl = $(this).attr('id');
                        if ($(this).is(':disabled') == false) {
                            $("#" + idControl + "_wrapper").addClass('notFilled');
                            op = 1;

                        }
                    }
                });

                $("#frmPage").find('input.required:checkbox').live('click', function () {
                    var idControl = $(this).attr('id');
                    if ($(this).is(":checked") != false) {
                        $("#" + idControl + "_wrapper").removeClass('notFilled');
                    }
                    else
                        $("#" + idControl + "_wrapper").addClass('notFilled');
                });

                var ulActual;
                $('ul.requiredRadioButon').each(function (idx1, chele) {
                    ulActual = $(this)
                    if ($('#' + ulActual.attr('id') + '.requiredRadioButon input[type="radio"]:checked').length == 0) {
                        op1 = 1;
                        $(this).addClass('notFilled');
                        $(this).find('li').each(function (i, j) {
                            if (($(this).attr('disabled')) != "disabled")
                                $(this).find('[type="radio"]').each(function (x, y) {
                                    $(this).live("click", function () {

                                        if ($('ul.requiredRadioButon input[type="radio"]:checked').length == 0) {
                                            $(this).closest('ul').addClass('notFilled');
                                            op1 = 1;

                                        }
                                        else
                                            $(this).closest('ul').removeClass('notFilled');
                                        op1 = 0;
                                    });
                                });
                            else {
                                $(this).closest('ul').removeAttr('requiredRadioButon');
                                $(this).closest('ul').removeClass('notFilled');
                                op1 = 0;
                            }
                        });
                    }
                    else {
                        $(this).removeClass('notFilled');
                        op = 0;
                    }
                });

                $('ul.requiredcheckBox').each(function (idx1, chele) {
                    ulActualc = $(this);
                    if ($('#' + ulActualc.attr('id') + '.requiredcheckBox input[type="checkbox"]:checked').length == 0) {

                        op2 = 1
                        $(this).addClass('notFilled');
                        $(this).find('li').each(function (i, j) {
                            if (($(this).attr('disabled')) != "disabled")
                                $(this).find('[type="checkbox"]').each(function (x, y) {

                                    $(this).live("click", function () {
                                        $(this).closest('ul').removeClass('notFilled');
                                        if ($('ul.requiredcheckBox input[type="checkbox"]:checked').length == 0) {
                                            $(this).closest('ul').addClass('notFilled');
                                            op2 = 1;
                                        }

                                        else {
                                            $(this).closest('ul').removeClass('notFilled');
                                            op2 = 0;
                                        }
                                    });
                                });
                            else {
                                $(this).closest('ul').removeAttr('requiredRadioButon');
                                $(this).closest('ul').removeClass('notFilled');
                                op2 = 0;
                            }
                        });
                    }
                    else {
                        op2 = 0;
                        $(this).removeClass('notFilled');
                    }

                });


                var numItems = $('.notFilled').length
                if (numItems == undefined)
                    numItems = 0;

                //$("#frmPage").find('.notFilled').first().focus(); SE PONE ENTRE COMENTARIOS, YA QUE ESTÁ DISPARANDO 

                var n = $(this).attr('name').split("/");
                var nId = $(this).attr('id').split("/");

                if (op == 0 && op1 == 0 && op2 == 0 && numItems == 0) {
                    executeEvent(n[0], n[1], n[2], nId[1]);

                }
                else
                    return false;
                op = 0;
            } else {
                var n = $(this).attr('name').split("/");
                var nId = $(this).attr('id').split("/");


                executeEvent(n[0], n[1], n[2], nId[1]);

            }
        }
        });
})

function executeEvent(Action, isSave, ActionId, uidAction) {

    try {
        blockScreenMessage("Un momento por favor..")

        if (Action == 'Salir') {
            window.location.href('/Home/Index');
        }

        if (Action == 'Guardar' || Action == "GuardarSinValidar") {
            $("#Guardar").attr("disabled", "disabled");
            var form = $("#frmPage");
            var disabled = form.find(':input:disabled').removeAttr('disabled');
            var a = form.serializeArray();
            var formCollection = form.serialize();
            disabled.attr('disabled', 'disabled');

            $.ajax({
                type: "POST",
                url: "/FormPage/RespondAction/",
                data: {
                    'campos': a, 'uidAction': uidAction
                },

                success: function (data) {

                    if (data.Success == true) {

                        //alert(data.Message);

                        $(".btnAtions").each(function () {
                            var idAction = $(this).attr('id');
                            var btnactionLoc = $(this);
                            $.ajax({
                                type: "POST",
                                url: "/FormPage/UpdateStateActions/",
                                data: {
                                    'campos': a, 'uidAction': idAction
                                },
                                dataType: "json",
                                success: function (evt) {
                                    if (evt == false)
                                        btnactionLoc.css('visibility', 'hidden');
                                },
                                error: function (req, status, error) {
                                    alert("Error al guardar: " + error.replace(/\\n/g, "\n"));
                                    $.unblockUI();
                                }
                            });
                        });

                        var idcontrolId = $(this).attr('id');
                        var objControl = $(this);
                        blockScreenMessage("Actualizando acciones por rol")
                        $.ajax({
                            type: "POST",
                            url: "/Panel/UpdateStateControlsByRole/",
                            data: {
                                'campos': a
                            },
                            dataType: "json",
                            success: function (blresut) {
                                alert("Solicitud guardada con éxito");
                                $.unblockUI();
                                if (blresut == true) {
                                    var disabled = form.find(':input').removeAttr('disabled');
                                    disabled.attr('disabled', 'disabled');
                                }
                                //else {
                                //    var disabled = form.find(':input').removeAttr('disabled');
                                //}

                            },
                            error: function (req, status, error) {
                                alert("Error al guardar: " + error.replace(/\\n/g, "\n"));
                                $.unblockUI();
                            }
                        });
                        if (data.UidPage != '00000000-0000-0000-0000-000000000000') {
                            strategyRedirect(data.UidPage);

                        }

                    }
                    else {
                        alert(data.Message) //display exception                      
                        $.unblockUI();
                    }

                },
                error: function (result, status, error) {

                    if (result.responseText == "EndSesion") {
                        var urlSite = window.location.origin = window.location.protocol + "//" + window.location.hostname + (window.location.port ? ':' + window.location.port : '');
                        location.href = urlSite
                    }
                    else {
                        alert("Error al guardar: " + result.responseText.replace(/\\n/g, "\n"));
                    }
                    $.unblockUI();
                    $(".btnAtions").removeAttr("disabled", "disabled");
                }

            });
        }
        if (Action == 'Siguiente') {
            var form = $("#frmPage");
            var disabled = form.find(':input:disabled').removeAttr('disabled');
            var a = form.serializeArray();
            disabled.attr('disabled', 'disabled');
            $.ajax({
                type: "POST",
                url: "/FormPage/RespondAction/",
                data: {
                    'formFields': a, 'uidAction': uidAction
                },
                dataType: "html",
                success: function (evt) {
                    $('#FormPageAnswers').html(evt);
                    $.unblockUI();
                },
                error: function (result, status, error) {
                    console.log("Error")
                    if (result.responseText == "EndSesion") {
                        var urlSite = window.location.origin = window.location.protocol + "//" + window.location.hostname + (window.location.port ? ':' + window.location.port : '');
                        location.href = urlSite
                    }
                    else {
                        alert(result.responseText.replace(/\\n/g, "\n"));
                    }
                    $.unblockUI();
                    $(".btnAtions").removeAttr("disabled", "disabled");
                }
            });
        }

        if (Action == 'Anterior') {
            var form = $("#frmPage");
            var disabled = form.find(':input:disabled').removeAttr('disabled');
            var a = form.serializeArray();
            disabled.attr('disabled', 'disabled');
            $.ajax({
                type: "POST",
                url: "/FormPage/RespondAction/",
                data: {
                    'formFields': a, 'uidAction': uidAction
                },
                dataType: "html",
                success: function (evt) {
                    $('#FormPageAnswers').html(evt);
                    //alert(evt.Message); //display success
                    $.unblockUI();
                },
                error: function (result, status, error) {

                    if (result.responseText == "EndSesion") {
                        var urlSite = window.location.origin = window.location.protocol + "//" + window.location.hostname + (window.location.port ? ':' + window.location.port : '');
                        location.href = urlSite
                    }
                    else {
                        alert(result.responseText.replace(/\\n/g, "\n"));
                    }
                    $.unblockUI();
                    $(".btnAtions").removeAttr("disabled", "disabled");
                }
            });
        }
        if (Action == 'IrPaginaEspecifica') {
            $(".btnAtions").attr('disabled', 'disabled')
            var form = $("#frmPage");
            var disabled = form.find(':input:disabled').removeAttr('disabled');
            var a = form.serializeArray();
            disabled.attr('disabled', 'disabled');

            $.ajax({
                type: "POST",
                url: "/FormPage/RespondAction/",
                data: {
                    'formFields': a, 'uidAction': uidAction
                },
                dataType: "html",
                success: function (evt) {
                    $('#FormPageAnswers').html(evt);
                    //alert(evt.Message); //display success
                    $.unblockUI();
                },
                error: function (result, status, error) {

                    if (result.responseText == "EndSesion") {
                        var urlSite = window.location.origin = window.location.protocol + "//" + window.location.hostname + (window.location.port ? ':' + window.location.port : '');
                        location.href = urlSite
                    }
                    else {
                        alert(result.responseText.replace(/\\n/g, "\n"));
                    }
                    $.unblockUI();
                    $(".btnAtions").removeAttr("disabled", "disabled");
                }
            });
        }

        //Valida si es acción que cierra pagina y va al inicio
        if (Action == 'CerrarFormulario') {

            var form = $("#frmPage");
            var disabled = form.find(':input:disabled').removeAttr('disabled');
            var a = form.serializeArray();
            var formCollection = form.serialize();
            disabled.attr('disabled', 'disabled');
            if (isSave == "True") {
                $.ajax({
                    type: "POST",
                    url: "/FormPage/RespondAction/",
                    data: {
                        'campos': a, 'uidAction': uidAction
                    },
                    dataType: "html",
                    success: function (data) {
                        if (data.Success) {
                            alert("Solicitud guardad con éxito")
                            window.location.href = '/Home/Index';
                        }
                        else {
                            alert(data.Message) //display exception
                        }
                        $.unblockUI();
                    },
                    error: function (result, status, error) {

                        if (result.responseText == "EndSesion") {
                            var urlSite = window.location.origin = window.location.protocol + "//" + window.location.hostname + (window.location.port ? ':' + window.location.port : '');
                            location.href = urlSite
                        }
                        else {
                            alert("Error al ir cerrar el formulario: " + result.responseText.replace(/\\n/g, "\n"));
                        }
                        $.unblockUI();
                    }
                });
            }
            else {
                window.location.href = '/Home/Index';
            }
        }

        if (Action == 'CerraSesión') {

            var form = $("#frmPage");
            var disabled = form.find(':input:disabled').removeAttr('disabled');
            var a = form.serializeArray();
            var formCollection = form.serialize();
            disabled.attr('disabled', 'disabled');
            if (isSave == "True") {
                $.ajax({
                    type: "POST",
                    url: "/FormPage/RespondAction/",
                    data: {
                        'campos': a, 'uidAction': uidAction
                    },
                    dataType: "html",
                    success: function (data) {

                        window.location.href = '/Account/LogOff';

                        $.unblockUI();
                    },
                    error: function (result) {

                        var urlSite = window.location.origin = window.location.protocol + "//" + window.location.hostname + (window.location.port ? ':' + window.location.port : '');
                        location.href = urlSite
                        $.unblockUI();
                    }
                });
            }
            else {
                window.location.href = '/Account/LogOff';
            }
        }

        if (Action == 'VerReporteViewer') {
            blockScreen("Un momento por favor....");
            var requestid = $("#RequestId").val();
            //var disabled = form.find(':input:disabled').removeAttr('disabled');


            $.ajax({
                type: "POST",
                url: "/FormPage/ReturnResource/",
                data: {
                    'uidAction': uidAction

                },
                success: function (data1) {

                    $.ajax({
                        type: "POST",
                        url: "/FormPage/GetReportByRequest/",
                        data: {
                            'parameters': data1, 'requestid': requestid

                        },
                        success: function (data) {

                            $('#dvFormReportMenu').html(data);
                            $("#dvFormReportMenu").dialogr();
                            $("#dvFormReportMenu").dialogr('open');
                            $.unblockUI();
                        }
                    });

                }
            });
        }
        if (Action == 'VerReporte') {

            var form = $("#frmPage");
            var a = form.serializeArray();
            $.ajax({
                type: "POST",
                url: "/FormPage/getDataReport/",
                data: {
                    'formFields': a, 'uidAction': uidAction
                },
                dataType: "json",
                success: function (evt) {
                    var srRequestId = evt.RequestId
                    //event.stopPropagation();
                    //$("#right-column").append($("#dvFormReport"));                        
                    $("#dvFormReport").html("");
                    console.log('cargando reporte')
                    $registerStatusDialogHandle = $("<iframe src=/FormPage/DownloadPdf?uidAction=" + uidAction + "&requestId=" + srRequestId + "  width='100%' height='900' id='reportFrame'><iframe> <input type='button' value='Volver' class='buckinput' name='items[]' style='padding:5px;' />").dialogr({
                        close: function() {
                            $(this).hide();
                        }
                    })
                    $registerStatusDialogHandle.dialogr('open');
                    
                    $.unblockUI();
                },
                error: function (result, status, error) {
                    alert('err')
                    if (result.responseText == "EndSesion") {
                        var urlSite = window.location.origin = window.location.protocol + "//" + window.location.hostname + (window.location.port ? ':' + window.location.port : '');
                        location.href = urlSite
                    }
                    else {
                        alert("Error generar el reporte: " + result.responseText.replace(/\\n/g, "\n"));
                    }
                    $.unblockUI();
                }
            });
        }

        //setWarningTimeOut()
    }
    catch (err) {
        alert(err);
        $.unblockUI();
    }
}