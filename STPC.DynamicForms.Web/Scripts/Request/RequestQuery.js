

function NodeName(parentid) {
  
    $.getJSON("/Campaign/GetchildrenCountAndLevelName", { id: parentid }, function (data) {
        $('#Jerarquia').val(data);
    });
    $("#txtNode").val(parentid);
    $("#NewCategoryForm").dialog("close");
    $("#NewCategoryFormEdit").dialog("close");
}


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