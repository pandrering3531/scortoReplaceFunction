function CreacteDivDialogFileUpload() {
    $('#divFileUpload').html("");
    $('#divFileUpload').append('<label>Seleccione el archivo que desea cargar</label><br />');
    $('#divFileUpload').append('<input type="file" id="fileUp" name="fileUp" value="Examinar" />');
    $('#divFileUpload').append('<input type="button" value="Modify" onclick="UploadFile()" />');
    $('#divFileUpload').append('<input type="hidden" id="MaxSizeHidden" name="MaxSizeHidden" /> ')
    $('#divFileUpload').append('<input type="hidden" id="ValidExtensionsHidden" name="ValidExtensionsHidden" /> ');
    $('#divFileUpload').append('<input type="hidden" id="ErrorExtensionsHidden" name="ErrorExtensionsHidden" /> ');
    $('#divFileUpload').append('<br />');
}
function ModalFileUpload(sender, MaxSize, ext, extError) {
    CreacteDivDialogFileUpload();
    $("#txtSender").val(sender.name);
    $("#MaxSizeHidden").val(MaxSize);
    $("#ValidExtensionsHidden").val(ext);
    $("#ErrorExtensionsHidden").val(extError);
    $("#modalContents").dialog({ height: 140, modal: true, autoOpen: true, resizable: false, close: dialogClose });

}
function dialogClose() {

}
function UploadFile() {
    try {
        if ($("#fileUp").val() != '') {
            var ajaxSubmitOptions = {
                beforeSubmit: showRequest,  // pre-submit callback 
                success: showResponse,  // post-submit callback 
                error: function (response, status, err) {
                    // This option doesn't catch any of the error below, 
                    // everything is always 'OK' a.k.a 200
                    if (response.status == 400) {
                        alert("Sorry, this is bad request!");
                    }
                    if (response.status == 601) {
                        sessionTimedOut();
                    }

                }
            }

            $("#ActualizarListas").ajaxSubmit(ajaxSubmitOptions);

            return false;
        }
    }
    catch (err) {
        alert(err);
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