$(function () {
    $('#optOptios').on('click', function () {
        getOptionsCache();
    });

    $('#optEvents').on('click', function () {
        getPageEventsCache()
    });

    $('#optPageMathOperations').on('click', function () {
        getPageMathOperatiosCache()
    });

    //Funcion que carga la lista de ocpiones disponible en el cache

    function getOptionsCache() {
        blockScreen("Cargando opciones desde el caché");
        $.ajax({
            type: "POST",
            dataType: "html",
            url: "/RedisCache/getListOptionsFromRedis/",
            success: function (data) {

                $("#section_list_objects").html(data)
                $.unblockUI();
            }
        });
    }
    function getPageEventsCache() {
        blockScreen("Cargando eventos de página desde el caché");
        $.ajax({
            type: "POST",
            dataType: "html",
            url: "/RedisCache/getListPageEventsFromRedis/",
            success: function (data) {

                $("#section_list_objects").html(data)
                $.unblockUI();
               

            }
        });
    }
    function getPageMathOperatiosCache() {
        blockScreen("Cargando operaciones desde el caché");
        $.ajax({
            type: "POST",
            dataType: "html",
            url: "/RedisCache/getListPageMathOperationsFromRedis/",
            success: function (data) {

                $("#section_list_objects").html(data)
                $.unblockUI();


            }
        });
    }


    function blockScreen(message) {
        $.blockUI({
            message: message,//'Validando usuario y contraseña',
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
    }
    $("#EditCacheCategory").button();
    $("#EditCacheCategory").click(function () {
        blockScreen("Actualizando caché");
        $.post("/RedisCache/UpdateCache",
                       function (data) {
                           if (data.Success == true) {
                               $.unblockUI();
                               alert("Caché actualizado con éxito");
                           }
                           else {
                               $.unblockUI();
                               alert(data.name);
                           }
                       }, "json");

    });


  
   
});