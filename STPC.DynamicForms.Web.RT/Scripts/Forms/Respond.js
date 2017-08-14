function blockScreenMessage(Mensaje) {
    $.blockUI({
        message: Mensaje,
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
$(function () {

    $(".FormPageSelectorDt").button().click(function () {
        blockScreenMessage("Un momento por favor..")
        var idPage = $(this).attr('id');

        $.ajax({
            type: "POST",
            url: '/FormPage/Respond_dt',
            data: { id: idPage },
            dataType: "html",
            success: function (data) {
                $("#FormPageAnswers").html(data);
                $.unblockUI();
                //$(".CustomInteger").numeric(false, function () { this.focus(); });    
                jQuery(function ($) {
                    $.datepicker.regional['es'] = {
                        closeText: 'Cerrar',
                        prevText: '<Ant',
                        nextText: 'Sig>',
                        currentText: 'Hoy',
                        monthNames: ['Enero', 'Febrero', 'Marzo', 'Abril', 'Mayo', 'Junio', 'Julio', 'Agosto', 'Septiembre', 'Octubre', 'Noviembre', 'Diciembre'],
                        monthNamesShort: ['Enero', 'Febrero', 'Marzo', 'Abril', 'Mayo', 'Junio', 'Julio', 'Agosto', 'Septiembre', 'Octubre', 'Noviembre', 'Diciembre'],
                        dayNames: ['Domingo', 'Lunes', 'Martes', 'Miércoles', 'Jueves', 'Viernes', 'Sábado'],
                        dayNamesShort: ['Domingo', 'Lunes', 'Martes', 'Miércoles', 'Jueves', 'Viernes', 'Sábado'],
                        dayNamesMin: ['Do', 'Lu', 'Ma', 'Mi', 'Ju', 'Vi', 'Sá'],
                        weekHeader: 'Sm',
                        firstDay: 1,
                        isRTL: false,
                        showMonthAfterYear: false,
                        yearSuffix: ''
                    };
                    $.datepicker.setDefaults($.datepicker.regional['es']);
                });

                $(".jQueryCalendarInput").datepicker({
                    dateFormat: "mm/dd/yy", changeMonth: true, yearRange: '-100:+0',
                    changeYear: true, format: "mm/dd/yy",
                    buttonImage: $Url.resolve('~/Content/images/aprobado_th.gif'),
                    buttonImageOnly: true
                });

            }

        });
        //$.get(this.href, function (data) {
        //    $("#FormPageAnswers").html(data);

        //});
        $(".FormPageSelector").button("option", "icons", { primary: null });
        $(this).button("option", "icons", { primary: 'ui-icon-circle-triangle-e' });
        $.ajaxSetup({
            async: true
        });


    });
    $(".FormPageSelector:first").button("option", "icons", { primary: 'ui-icon-circle-triangle-e' });
    //$(".CustomInteger").numeric(false, function () {  });  
    jQuery(function ($) {
        $.datepicker.regional['es'] = {
            closeText: 'Cerrar',
            prevText: '<Ant',
            nextText: 'Sig>',
            currentText: 'Hoy',
            monthNames: ['Enero', 'Febrero', 'Marzo', 'Abril', 'Mayo', 'Junio', 'Julio', 'Agosto', 'Septiembre', 'Octubre', 'Noviembre', 'Diciembre'],
            monthNamesShort: ['Enero', 'Febrero', 'Marzo', 'Abril', 'Mayo', 'Junio', 'Julio', 'Agosto', 'Septiembre', 'Octubre', 'Noviembre', 'Diciembre'],
            dayNames: ['Domingo', 'Lunes', 'Martes', 'Miércoles', 'Jueves', 'Viernes', 'Sábado'],
            dayNamesShort: ['Domingo', 'Lunes', 'Martes', 'Miércoles', 'Jueves', 'Viernes', 'Sábado'],
            dayNamesMin: ['Do', 'Lu', 'Ma', 'Mi', 'Ju', 'Vi', 'Sá'],
            weekHeader: 'Sm',
            firstDay: 1,
            isRTL: false,
            showMonthAfterYear: false,
            yearSuffix: ''
        };
        $.datepicker.setDefaults($.datepicker.regional['es']);
    });

    $(".jQueryCalendarInput").datepicker({
        dateFormat: "mm/dd/yy", changeMonth: true, yearRange: '-100:+0',
        changeYear: true,
        format: "mm/dd/yy",
        buttonImage: "Content/Images/aprobado_th.gif",
        buttonImageOnly: true
    });

});

$(document).ready(function () {
    //$(".CustomInteger").numeric(false, function () { this.value = ""; this.focus(); });  
    //$('.CustomCurrency').numeric(false, function () { this.focus(); });

});
