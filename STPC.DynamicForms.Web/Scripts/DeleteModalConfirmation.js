$(function () {
    var link;

    var $myDialog = $("#cancelDialog").dialog({
        autoOpen: false,
        modal: true,
        buttons: {
            'Si, continuar': function () {
                alert(link)
                $(this).click(function () {
                    var iGuidList = $('#GuidList').val();

                    $.post("@Url.Action('DeletePost', 'Form')", {
                        iGuidList: iGuidList
                    },
                    function (carData) {
                    });
                });
                $(this).dialog('close');
            },
            'No, cancelar': function () {
                $(this).dialog('close');
            }
        }
    });

    $("[id$=btnCancel]").click(function (e) {

        e.preventDefault();
        link = $(this).attr('href');
        $myDialog.dialog('open');
    });
});