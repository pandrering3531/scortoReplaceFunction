


$("#ddlAplicationName").change(function () {
    blockScreen("Cargando jerarquias por empresa");
    var appNameId;
    if ($(this).val() == "") {
        appNameId = 0;
    }
    else {
        appNameId = $(this).val();
    }
    $.ajax({
        type: "POST",
        url: "/Hierarchies/getHierarchieByAplicationNameID/",
        data: {
            'aplicationNameId': appNameId,

        },
        dataType: "html",
        success: function (data) {
            if (data.Success != false) {
                $("#CustomerForm").html("")
                $("#CustomerForm").html(data)
                $.unblockUI();
            }
            else {
                $.unblockUI();
                alert(data.ErrorMessage)
            }
        }
    });
});

function hierarchiesupdateSuccess()
{
    $.unblockUI();
    alert('Nodo de jerarquia guardado con éxito');
}