$(function () {
    $(".FormPageSelector").button().click(function () {
        //event.preventDefault();
        $.ajaxSetup({
            async: false
        });
        $("#ajaxloading").show();
        $.get(this.href, function (data) {
            $("#FormPageAnswers").html(data);
            $(".CustomInteger").numeric(false, function () { this.value = ""; this.focus(); });

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
            });
        });
        $(".FormPageSelector").button("option", "icons", { primary: null });
        $(this).button("option", "icons", { primary: 'ui-icon-circle-triangle-e' });
        $.ajaxSetup({
            async: true
        });
        $("#ajaxloading").hide();

    });
    $(".FormPageSelector:first").button("option", "icons", { primary: 'ui-icon-circle-triangle-e' });
    $(".CustomInteger").numeric(false, function () { this.value = ""; this.focus(); });

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
    });

});
