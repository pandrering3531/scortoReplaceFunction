

jQuery(document).ready(function () {
    $('#accordion').accordion({ autoHeight: false });
    setWarningTimeOut();

});
function NodeName(parentid) {

    $.getJSON("/Request/GetchildrenCountAndLevelName", { id: parentid }, function (data) {
        $('#txtJerarquia').val(data);
    });
    $("#txtNode").val(parentid);
    $("#NewCategoryForm").dialog("close");
    $("#NewCategoryFormEdit").dialog("close");
}

$(document).ready(function () {
    $("#txtFechaCreacionIni").datepicker();
});
$(document).ready(function () {
    $("#txtFechaCreacionEnd").datepicker();
});
$(document).ready(function () {
    $("#txtFechaActualizacionIni").datepicker();
});
$(document).ready(function () {
    $("#txtFechaActualizacionEnd").datepicker();
})


$(function () {
    $("#NewCategoryForm").dialog({
        autoOpen: false,
        height: 500,
        width: 400,
        modal: true,

    });

    $("#DivFieldsPage").dialog({
        autoOpen: false,
        height: 500,
        width: 400,
        modal: true,
        buttons: {
            "Aceptar": function () {
                $.blockUI({
                    css: {
                        border: 'none',
                        padding: '15px',
                        backgroundColor: '#000',
                        '-webkit-border-radius': '10px',
                        '-moz-border-radius': '10px',
                        opacity: .5,
                        color: '#fff'
                    }
                });

                $(".CustomCurrency ").each(function () {
                    this.value = this.value.replace('$ ', '');
                });


                var par = $("#DivFieldsPage input,select").serialize();

                var idTipoDoc = $('#ddlTipoDoc').val();
                var NumRequest = $('#txtNumeroSolicitud').val();
                var Node = $('#txtNode').val();
                var requesUpdateDate = $('#txtFechaActualizacionIni').val();
                var requesCreateDate = $('#txtFechaCreacionIni').val();
                var requesUpdateDateEnd = $('#txtFechaActualizacionEnd').val();
                var requesCreateDateEnd = $('#txtFechaCreacionEnd').val();
                var requesUpdatedBy = $('#UpdatedBy').val();
                var requesCreatedBy = $('#CreatedBy').val();
                var requestType = $('#ddlRequestType').val();
                var sruidFormState = $('#ddlStateForm').val();

                $("#list option[value='2']").text()
                var requestName = $("#ddlRequestType option[value=" + requestType + "]").text();

                $.ajax({
                    type: "POST",
                    url: "/Request/IndexConsulta/",
                    data: {
                        'srNumRequest': NumRequest,
                        'srNode': Node,
                        'requesUpdateDate': requesUpdateDate,
                        'requesCreateDate': requesCreateDate,
                        'requesUpdateDateEnd': requesUpdateDateEnd,
                        'requesCreateDateEnd': requesCreateDateEnd,
                        'requesCreatedBy': requesCreatedBy,
                        'requesUpdatedBy': requesUpdatedBy,
                        'par': par,
                        'srRequestType': requestType,
                        'srRequestName': requestName,
                        'uidFormState': sruidFormState
                    },
                    dataType: "html",
                    success: function (evt) {
                        //
                        $('#detailsDiv').html(evt);
                        $('#DivFieldsPage').html("")
                        $('#txtNode').val("");
                        $.unblockUI();
                        $("input[type=text]").each(function () {
                            $(this).val("");
                        });

                        $("select").each(function () {
                            $(this).val("0");

                        });
                        //$("#EditCustomerProfileForm").submit();
                    },
                    error: function (req, status, error) {
                        alert("Error!Occured" + error);
                        $.unblockUI();
                    }
                });
                $(this).dialog("close");
            }
        }
    });
    function ValidateDate(dtValue) {
        if (dtValue != "") {
            var dtRegex = new RegExp(/^(?:(?:(?:0?[13578]|1[02])(\/|-)31)|(?:(?:0?[1,3-9]|1[0-2])(\/|-)(?:29|30)))(\/|-)(?:[1-9]\d\d\d|\d[1-9]\d\d|\d\d[1-9]\d|\d\d\d[1-9])$|^(?:(?:0?[1-9]|1[0-2])(\/|-)(?:0?[1-9]|1\d|2[0-8]))(\/|-)(?:[1-9]\d\d\d|\d[1-9]\d\d|\d\d[1-9]\d|\d\d\d[1-9])$|^(0?2(\/|-)29)(\/|-)(?:(?:0[48]00|[13579][26]00|[2468][048]00)|(?:\d\d)?(?:0[48]|[2468][048]|[13579][26]))$/);
            return dtRegex.test(dtValue);
        }
        else {

            return true
        }

    }
    $("#StarQuery")
       .button()
       .click(function () {
           //setWarningTimeOut();
           $.blockUI({
               css: {
                   border: 'none',
                   padding: '15px',
                   backgroundColor: '#000',
                   '-webkit-border-radius': '10px',
                   '-moz-border-radius': '10px',
                   opacity: .5,
                   color: '#fff'
               }
           });

           if (ValidateDate($("#txtFechaCreacionIni").val()) &&
               ValidateDate($("#txtFechaCreacionEnd").val()) &&
               ValidateDate($("#txtFechaActualizacionIni").val()) &&
               ValidateDate($("#txtFechaActualizacionEnd").val())
               ) {



               $(".CustomCurrency ").each(function () {
                   this.value = this.value.replace('$ ', '');

               });
               var par = $("#DivFieldsPage input,select").serialize();

               var idTipoDoc = $('#ddlTipoDoc').val();
               var NumRequest = $('#txtNumeroSolicitud').val();
               var Node = $('#txtNode').val();
               var requesUpdateDate = $('#txtFechaActualizacionIni').val();
               var requesCreateDate = $('#txtFechaCreacionIni').val();
               var requesUpdateDateEnd = $('#txtFechaActualizacionEnd').val();
               var requesCreateDateEnd = $('#txtFechaCreacionEnd').val();
               var requesUpdatedBy = $('#UpdatedBy').val();
               var requesCreatedBy = $('#CreatedBy').val();
               var requestType = $('#ddlRequestType').val();
               var sruidFormState = $('#ddlStateForm').val();

               $("#list option[value='2']").text()
               var requestName = $("#ddlRequestType option[value=" + requestType + "]").text();

               $.ajax({
                   type: "POST",
                   url: "/Request/IndexConsulta/",
                   data: {
                       'srNumRequest': NumRequest,
                       'srNode': Node,
                       'requesUpdateDate': requesUpdateDate,
                       'requesCreateDate': requesCreateDate,
                       'requesUpdateDateEnd': requesUpdateDateEnd,
                       'requesCreateDateEnd': requesCreateDateEnd,
                       'requesCreatedBy': requesCreatedBy,
                       'requesUpdatedBy': requesUpdatedBy,
                       'par': par,
                       'srRequestType': requestType,
                       'srRequestName': requestName,
                       'uidFormState': sruidFormState
                   },
                   dataType: "html",
                   success: function (evt) {
                       //
                       $('#detailsDiv').html(evt);
                       $.unblockUI();
                       $("input[type=text]").each(function () {
                           $(this).val("");

                       });

                       $("select").each(function () {
                           $(this).val("0");

                       });
                       $('#txtNode').val("");
                       $('#DivFieldsPage').html("")
                       //$("#EditCustomerProfileForm").submit();
                   },
                   error: function (req, status, error) {
                       alert("Error!Occured" + error);
                       $.unblockUI();
                   }
               });
           }
           else {
               alert("Formato de fecha inválido")
           }

       });



    $("#NewCategory")
            .button()
            .click(function () {
                $("#NewCategoryForm").dialog("open");
            });


    $("#btnLoadParameters")
        .button()
        .click(function () {
            $.blockUI({
                css: {
                    border: 'none',
                    padding: '15px',
                    backgroundColor: '#000',
                    '-webkit-border-radius': '10px',
                    '-moz-border-radius': '10px',
                    opacity: .5,
                    color: '#fff'
                }
            });
            var uidRequest = $("#ddlRequestType").val();

            if (uidRequest != '') {
                $.ajax({
                    type: "GET",
                    url: "/Request/LoadFieldsByForm/",
                    data: {
                        'uid': uidRequest
                    },
                    dataType: "html",
                    success: function (evt) {

                        //$('#DivFieldsPage').replaceWith(evt);
                        $("#DivFieldsPage").dialog("open");

                        $('#DivFieldsPage').html(evt);
                        $.unblockUI();
                    },
                    error: function (req, status, error) {
                        alert("Error!Occured" + error);
                        $.unblockUI();
                    }
                });
            }
            else {
                $.unblockUI();
            }
        });


    $("#CategoriesList").change(function () {
        if ($("#CategoriesList").val() != null) {
            $("#Options").load("/Categories/Options/", { categoryname: $("#CategoriesList").val() });
            $("#DeleteCategory").html('<span class="ui-button-text">Eliminar ' + $("#CategoriesList").val() + '</span>').show();
        }
    });

});

$(function () {
    $("#NewCategoryForm").dialog({
        autoOpen: false,
        height: 500,
        width: 400,
        modal: true,

    });
    $("#NewCategory")
            .button()
            .click(function () {
                $("#NewCategoryForm").dialog("open");
            });

});


$(function () {
    $("#NewCategoryFormEdit").dialog({
        autoOpen: false,
        height: 500,
        width: 400,
        modal: true,

    });

    $("#NewCategoryEdit")
            .button()
            .click(function () {
                $("#NewCategoryFormEdit").dialog("open");
            });
});



function GetReport(param, requestId) {
    console.log("get report inicio")
    blockScreen("Un momento por favor....");
    $.ajax({
        type: "POST",
        dataType: "html",
        url: "/Request/GetReport/",
        data: {
            'formUid': param,
            'requestId': requestId
        },
        success: function (data) {
            console.log("get report success")
            $('#dvFormReportMenu').html(data);
            $("#dvFormReportMenu").dialogr();
            $("#dvFormReportMenu").dialogr('open');
            $.unblockUI();
        },
        error: function (req, status, error) {
            console.log("get report error")
            var err = JSON.parse(req.responseText);
            errorMessage = err.Message;
            alert("Error: " + errorMessage);
            $.unblockUI();
        }

    });
}