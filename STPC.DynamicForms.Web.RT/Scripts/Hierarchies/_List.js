function addchild(parentid) {
    $("#parentid").val(parentid);
    $.getJSON("/Hierarchies/GetchildrenCountAndLevelName", { id: $("#parentid").val() }, function (data) {
        if (data.ChildCount == null) return false;
        $("#levelname").val(data.ChildLevelName);
        if (data.ChildCount > 0) {
            $("#levelname").attr('disabled', true);
        }
        else {
            $("#levelname").attr('disabled', false);
        }
        $("#NewChildForm").dialog("open");
    });
}

function EdtiNode(parentid) {
    $("#parentid").val(parentid);
    $.getJSON("/Hierarchies/EditNode", { id: $("#parentid").val() }, function (data) {

        $("#levelnameNode").val(data.ChildLevelName);
        $("#nameNode").val(data.NodeName);
        $("#ddlEditTipoNodo").val(data.NodeType)
        $("#EditChildForm").dialog("open");
    });
}

function deleteme(key) {
    if (confirm("¿Seguro de borrar esta página?")) {
        var theiteminthelist = $('span[data-key=' + key + ']').parent();
        $.post("/Hierarchies/DeleteHierarchy", { id: key },
                function (data) {
                    if (data.Success == true) {
                        $(theiteminthelist).remove();
                    }
                    else alert("Delete failed");
                }, "json");
    }
}

$(function () {
    var name = $("#name"), levelname = $("#levelname"), allFields = $([]).add(name).add(levelname), tips = $(".validateTips");

    $('.ui-state-default').hover(
				function () { $(this).addClass('ui-state-hover'); },
				function () { $(this).removeClass('ui-state-hover'); }
			);

    $("#NewChildForm").dialog({
        autoOpen: false,
        height: 300,
        width: 350,
        modal: true,
        buttons: {
            "Crear": function () {
                var bValid = true;
                allFields.removeClass("ui-state-error");
                bValid = bValid && checkLength(name, "nombre", 3, 200);
                if (bValid) {
                    $.post("/Hierarchies/AddChild", { Name: name.val(), ParentId: $("#parentid").val(), LevelName: levelname.val(), nodeType: $("#ddlNewTipoNodo").val() }, function (data) {
                        if (data.Success == true) {
                            window.location.reload(false);
                        }
                        else alert("Insert failed");
                    }, "json");
                    $(this).dialog("close");
                }
            },
            "Cancelar": function () {
                $(this).dialog("close");
            }
        },
        close: function () {
            allFields.val("").removeClass("ui-state-error");
        }
    });
});


$(function () {
    var name = $("#nameNode"), levelname = $("#levelnameNode"), allFields = $([]).add(name).add(levelname), tips = $(".validateTips");

    $('.ui-state-default').hover(
				function () { $(this).addClass('ui-state-hover'); },
				function () { $(this).removeClass('ui-state-hover'); }
			);
    $("#EditChildForm").dialog({
        autoOpen: false,
        height: 300,
        width: 350,
        modal: true,
        buttons: {
            "Editar": function () {
                var bValid = true;
                allFields.removeClass("ui-state-error");
                bValid = bValid && checkLength(name, "nombre", 3, 200);
                if (bValid) {
                    $.post("/Hierarchies/EditNode", { Name: name.val(), nodeId: $("#parentid").val(), LevelName: levelname.val(), nodeType: $("#ddlEditTipoNodo").val() }, function (data) {
                        if (data.Success == true) {
                            window.location.reload(false);
                        }
                        else alert("Insert failed");
                    }, "json");
                    $(this).dialog("close");
                }
            },
            "Cancelar": function () {
                $(this).dialog("close");
            }
        },
        close: function () {
            allFields.val("").removeClass("ui-state-error");
        }
    });
});
