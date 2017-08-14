if ($("[id='IrPaginaEspecifica/885677b2-7e3c-45eb-b329-ddb02f0934c2']").length) {
    jQuery("[id='IrPaginaEspecifica/885677b2-7e3c-45eb-b329-ddb02f0934c2']")
        .detach()
        .appendTo($("#STPC_DFi_fe06cf46-0b0b-4da9-a42e-f07e753d3339").parent().parent().next());

}

if ($("[id='IrPaginaEspecifica/D0CBF2FE-A27E-4D9F-81EA-65A6335CF7F4']").length) {
    jQuery("[id='IrPaginaEspecifica/D0CBF2FE-A27E-4D9F-81EA-65A6335CF7F4']")
        .detach()
        .appendTo($("#STPC_DFi_22df88ee-3827-4e1a-9060-6979bc4b0165").parent().parent().next());

}

$('input.checkLinea').on('change', function() {
    $('input.checkLinea').not(this).prop('checked', false);
});


$(".CheckAutomatico").change(function(event) {
    if (this.checked) {
        var lines = $(".tAreaVerde_input").val().split('\n');
        lines[0] = lines[0].replace("Monto: ", "");
        lines[1] = lines[1].replace("Plazo: ", "");
        lines[2] = lines[2].replace("Cuota proyectada: ", "");
        lines[3] = lines[3].replace("Tasa: ", "").replace("%", "");
        $(".monto").val(lines[0] == '' ? '0' : lines[0]);
        $(".plazo_input").val(lines[1] == '' ? '0' : lines[1]);
        $(".cproyectada").val(lines[2] == '' ? '0' : lines[2]);
        $(".tasa_input").val(lines[3] == '' ? '0' : lines[3]);
    }
});

$(".CheckConsumo").change(function(event) {
    if (this.checked) {
        var lines = $(".tAreaAzul_input").val().split('\n');
        lines[0] = lines[0].replace("Monto: ", "");
        lines[1] = lines[1].replace("Plazo: ", "");
        lines[2] = lines[2].replace("Cuota proyectada: ", "");
        lines[3] = lines[3].replace("Tasa: ", "").replace("%", "");
        $(".monto").val(lines[0] == '' ? '0' : lines[0]);
        $(".plazo_input").val(lines[1] == '' ? '0' : lines[1]);
        $(".cproyectada").val(lines[2] == '' ? '0' : lines[2]);
        $(".tasa_input").val(lines[3] == '' ? '0' : lines[3]);
    }
});

$(".CheckReportado").change(function(event) {
    if (this.checked) {
        var lines = $(".fucsia_input").val().split('\n');
        lines[0] = lines[0].replace("Monto: ", "");
        lines[1] = lines[1].replace("Plazo: ", "");
        lines[2] = lines[2].replace("Cuota proyectada: ", "");
        lines[3] = lines[3].replace("Tasa: ", "").replace("%", "");
        $(".monto").val(lines[0] == '' ? '0' : lines[0]);
        $(".plazo_input").val(lines[1] == '' ? '0' : lines[1]);
        $(".cproyectada").val(lines[2] == '' ? '0' : lines[2]);
        $(".tasa_input").val(lines[3] == '' ? '0' : lines[3]);
    }
});

$(".CheckCCartera").change(function(event) {
    if (this.checked) {

        var lines = $(".tAreaCafe_input").val().split('\n');
        lines[0] = lines[0].replace("Monto: ", "");
        lines[1] = lines[1].replace("Plazo: ", "");
        lines[2] = lines[2].replace("Cuota proyectada: ", "");
        lines[3] = lines[3].replace("Tasa: ", "").replace("%", "");
        $(".monto").val(lines[0] == '' ? '0' : lines[0]);
        $(".plazo_input").val(lines[1] == '' ? '0' : lines[1]);
        $(".cproyectada").val(lines[2] == '' ? '0' : lines[2]);
        $(".tasa_input").val(lines[3] == '' ? '0' : lines[3]);
    }
});



/*Funcion para fecha de pago en informacion basica*/

$(".plazo_input").keyup(function() {
    if (this.value != "1") {
        $(".cunicfecha").attr('disabled', 'disabled');
    } else {
        $(".cunicfecha").removeAttr('disabled');
    }
});


    $('fieldset.collapsible legend').click(function() {
        var $this = $(this);
        var parent = $this.parent();
        var contents = parent.contents().not(this);
        if (contents.length > 0) {
            $this.data("contents", contents.remove());
	    
        } else {
            $this.data("contents").appendTo(parent);
        }
        return false;
    });

if ($("textarea.observacion_input").length) {
    $("textarea.observacion_input").height( $("textarea.observacion_input")[0].scrollHeight );
}


$(":submit").click(function() {
    $('html,body').animate({
        scrollTop: $("#dvForm").offset().top},
        'slow');
});

