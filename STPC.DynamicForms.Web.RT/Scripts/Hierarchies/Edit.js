$("#ddlEditTipoNodo").change(function () {
    $.ajax({
        type: "POST",
        url: "/Hierarchies/GetSchemaTable/",
        data: {
            'NodeType': $("#ddlEditTipoNodo").val(),
            'NodeId': $("#NodeIdEdit").val()
        },
        dataType: "html",
        success: function (evt) {
            //
            $('#CustomerFormAttr2Edit').html(evt);
            $("#CustomerFormAttrEdit").css('visibility', 'hidden').is(':hidden') == false
            $("#CustomerFormAttrEdit").html('');
            //$("#EditCustomerProfileForm").submit();
        },
        error: function (req, status, error) {
            alert("Error!Occured" + error);

        }
    });
});