$(document).ready(function () {
    $("#deleteItem").click(function () {
        $.ajax({
            url: this.href.replace("SelectedFieldUid", document.getElementById("SelectedFieldUid").value),
            cache: false,
            success: function (html) {
                $("#FormFieldList").remove(html);
                $("#FieldType").fadeOut('slow');
            }
        });
        return false;
    });
    $("#FormFieldList").sortable({ axis: "y" });
});

