

function deleteForm(key) {
    if (confirm("¿Seguro de borrar este formulario?")) {
        blockScreen("Un momento por favor....");
        $.ajax({
            type: "POST",
            url: "/Form/DeletePost/",
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

