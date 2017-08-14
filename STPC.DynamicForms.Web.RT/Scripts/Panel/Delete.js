$(document).ready(function () {
    $(".deleteField").button().click(function () {
        if (confirm("¿Seguro de borrar este control?")) {
            blockScreen("Un momento por favor....");
            $.ajax({
                url: this.href.replace("SelectedFieldUid", document.getElementById("SelectedFieldUid").value),
                cache: false,
                success: function (html) {
                    $('#right-column').html(html);

                    $.unblockUI();
                }
            });
        }
        return false;
    });

   

    $("#FormFieldList").sortable({ axis: "y" });
});
$("a.decorated").button();
$("decorated").button();



function deletePanel(key) {
    if (confirm("¿Seguro de borrar este panel?")) {
        blockScreen("Un momento por favor....");
        $.ajax({
            type: "POST",
            url: "/Panel/DeletePost/",
            data: {
                'id': key
            },
            dataType: "html",
            success: function (evt) {
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

    }
}