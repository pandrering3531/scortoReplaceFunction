//function NewEventDialog() {
//    var id = $("#Data_FormPageId").val();

//    if (id != 0 && id != '') {
//        $.ajax({
//            type: "GET",
//            url: "/ControlEvents/Create/",
//            data: {

//                'idPage': id
//            },
//            dataType: "html",
//            success: function (evt) {

//                ShowParameters(evt);
//            },
//            error: function (req, status, error) {
//                alert("Error!Occured" + error);
//            }
//        });
//    }

//    else {
//        alert('Debe seleccionar una estrategia.');
//    }
//}
//$("a.decorated").button();
//$("button.decorated").button();

//function ShowParameters(evt) {
//    $('#modalContent2').html(evt);
//    //$("#modalContent").dialog({ bgiframe: true, height: 250, width: 480, modal: true, resizable: false });

//    $('#modalContent').dialog({
//        bgiframe: true,
//        resizable: false,
//        modal: true,
//        height: 300,
//        width: 700,
//        closeOnEscape: true,
//        overlay: { backgroundColor: "#000", opacity: 0.5 },
//        buttons: {
//            "Cancelar": function () {
//                $(this).dialog("close");
//            },

//            "Guardar": function () {

//            },
//        },
//    });
//}

