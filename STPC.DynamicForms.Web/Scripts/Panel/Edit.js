$(document).ready(function () {
    $("#newFieldSelect").click(function () {
        $("#FieldType").fadeIn('slow');
        return false;
    });
    $("#newFormField").click(function () {
        $.ajax({
            url: this.href.replace("SelectedFieldUid", document.getElementById("SelectedFieldUid").value),
            cache: false,
            success: function (html) {
                $("#FormFieldList").append(html);
                $("#FieldType").fadeOut('slow');
                $("#newFieldSelect").fadeIn('slow');
            }
        });
        return false;
    });
    $("a.deleteItem").live("click", function () {
        $(this).parents("div.FormField:first").remove();
        return false;
    });
    $("#FormFieldList").sortable({ axis: "y" });
});