$(function () {

    $("#btnFirst").button().click(function () {
        var fetch = $("#txtfetch").val();
        blockScreen("Cargando solicitudes")
        var page = parseInt($("#txtPageNumber").val());
        var offSet = 0
        page = 1;
        var totalPage = parseInt($('#txtTotalPage').val());

        $.ajax({
            type: "POST",
            url: "/Request/RequestsByParamProcedurePaged/",
            data: {
                'parameters': $("#txtNameSp").val(), 'offSet': offSet, 'filter': $("#txtSearchUser").val()
            },
            dataType: "html",
            success: function (evt) {
                //

                $("#txtPageNumber").val(page);
                $('#dvForm').html(evt);
                $.unblockUI();
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
    })
    $("#btnPrevius").button().click(function () {
        var fetch = $("#txtfetch").val();
        blockScreen("Cargando solicitudes")
        var page = parseInt($("#txtPageNumber").val());
        if (page > 1) {
            page--;
            var offSet = (page * fetch)
            var totalPage = parseInt($('#txtTotalPage').val());

            $.ajax({
                type: "POST",
                url: "/Request/RequestsByParamProcedurePaged/",
                data: {
                    'parameters': $("#txtNameSp").val(), 'offSet': offSet, 'filter': $("#txtSearchUser").val()
                },
                dataType: "html",
                success: function (evt) {

                    $("#txtPageNumber").val(page);
                    $('#dvForm').html(evt);
                    $.unblockUI();
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

    })
    $("#btnNext").button().click(function () {
        var fetch = $("#txtfetch").val();
        blockScreen("Cargando solicitudes")
        var page = parseInt($("#txtPageNumber").val());
        page++;
        var offSet = (page * fetch)
        var totalPage = parseInt($('#txtTotalPage').val());
        if (page <= totalPage) {
            $.ajax({
                type: "POST",
                url: "/Request/RequestsByParamProcedurePaged/",
                data: {
                    'parameters': $("#txtNameSp").val(), 'offSet': offSet, 'filter': $("#txtSearchUser").val()
                },
                dataType: "html",
                success: function (evt) {

                    $("#txtPageNumber").val(page);
                    $('#dvForm').html(evt);
                    $.unblockUI();
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
        else
            $.unblockUI();

    })
    $("#btnLast").button().click(function () {
        var fetch = $("#txtfetch").val();
        blockScreen("Cargando solicitudes")
        var page = parseInt($('#txtTotalPage').val());
        var offSet = (page * fetch)
        $.ajax({
            type: "POST",
            url: "/Request/RequestsByParamProcedurePaged/",
            data: {
                'parameters': $("#txtNameSp").val(), 'offSet': offSet, 'filter': $("#txtSearchUser").val()
            },
            dataType: "html",
            success: function (evt) {

                $("#txtPageNumber").val(page);
                $("#txtPageNumber").val(page);
                $('#dvForm').html(evt);
                $.unblockUI();
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
    })
    $("#btnBuscar").button().click(function () {

        blockScreen("Cargando solicitudes")
        var page = parseInt($('#txtTotalPage').val());
        var offSet = 0
        $.ajax({
            type: "POST",
            url: "/Request/RequestsByParamProcedurePaged/",
            data: {
                'parameters': $("#txtNameSp").val(), 'offSet': offSet, 'filter': $("#txtSearchUser").val()
            },
            dataType: "html",
            success: function (evt) {

                $("#txtPageNumber").val(1);

                $('#dvForm').html(evt);
                if ($("#txtTotalPagePaged").val() == "0")
                    $("#txtTotalPage").val(1);
                else
                    $("#txtTotalPage").val($("#txtTotalPagePaged").val());

                $.unblockUI();
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

    })
   


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


});
