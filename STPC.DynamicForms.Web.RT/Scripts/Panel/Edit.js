$(document).ready(function () {
    $("#newFieldSelect").click(function () {

        return false;
    });
    $("#newFormField").button().click(function () {
        blockScreen("Un momento por favor...");
        $.ajax({
            url: this.href.replace("SelectedFieldUid", document.getElementById("SelectedFieldUid").value),
            cache: false,
            success: function (html) {
                $("#FormFieldList").append(html);

                $("#newFieldSelect").fadeIn('slow');
                $.unblockUI();
            }
        });
        return false;
    });
    $("a.deleteItem").live("click", function () {
        $(this).parents("div.FormField:first").remove();
        return false;
    });
    $("#FormFieldList").sortable({ axis: "y" });

    $("a.decorated").button();
    $(".decorated").button();


    $("#btnSave").button().click(function () {
        blockScreen("Un momento por favor...");
        var form = $("#frmData");
        var a = form.serializeArray();
        var formCollection = form.serialize();

        $.ajax({
            url: "/Panel/Edit/",
            data: $("#frmData").serialize(),
            type: 'POST',
            dataType: 'html',
            success: function (html) {
                console.log("Pintando los controles")
                getControls($("#panelId").val(), $("#colunmNumber").val())
                $.unblockUI();
            }
        });

    });

});

function backToPanels(id) {
    blockScreen("Un momento por favor....");
    $.post('@Url.Action("RefreshToken", "Account")', function (html) {
        var tokenValue = $('<div />').html(html).find('input[type="hidden"]').val();
        $('input[name="__RequestVerificationToken"]').val(tokenValue);
        $.ajax({
            type: "POST",
            url: "/Panel/ListPanels/",
            data: {
                '__RequestVerificationToken': tokenValue, 'id': id
            },
            dataType: "html",
            success: function (evt) {
                $('#right-column').html

                if ($('div#NewPageDialog').length > 0) {
                    console.log($('div#NewPageDialog').length)
                    $('#NewPageDialog').remove();
                }
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
    });
}

function getControls(id, colunmNumber) {

    try {

        blockScreen("Un momento por favor....");
        $.post('/Account/RefreshToken', function (html) {
            var tokenValue = $('<div />').html(html).find('input[type="hidden"]').val();
            $('input[name="__RequestVerificationToken"]').val(tokenValue);
            console.log(id + ' ' + colunmNumber)
            $.ajax({
                type: "POST",
                url: "/Panel/ListControls/",
                data: {
                    '__RequestVerificationToken': tokenValue, 'id': id, 'colunmNumber': colunmNumber
                },
                dataType: "html",
                success: function (evt) {
                    $('#right-column').html

                    if ($('div#NewPageDialog').length > 0) {
                        console.log($('div#NewPageDialog').length)
                        $('#NewPageDialog').remove();
                    }
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
        });
    }

    catch (e) {
        alert(e.message)
    }
}
