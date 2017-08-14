function deletecategory() {
    if (confirm("¿Seguro de borrar esta categoría?")) {
        $.post("/Categories/DeleteCategory", { Name: $("#CategoriesList").val() }, function (data) {
            if (data.Success == true) {
                $("#CategoriesList option:selected").remove();
                $("#DeleteCategory").hide();
                $("#Options").html("");
            }
            else alert("Falló la eliminación");
        }, "json");
    }
}

$(function () {
    $("#dialog:ui-dialog").dialog("destroy");
    var name = $("#name"),
		allFields = $([]).add(name);
    $("#DeleteCategory").button().hide().on("click", deletecategory);
    $("#EditCategory").button().hide();


    function checkRegexp(o, regexp, n) {
        if (!(regexp.test(o.val()))) {
            o.addClass("ui-state-error");
            updateTips(n);
            return false;
        } else {
            return true;
        }
    }

    $("#NewCategoryForm").dialog({
        autoOpen: false,
        height: 300,
        width: 350,
        modal: true,
        buttons: {
            "Crear": function () {
                var bValid = true;
                allFields.removeClass("ui-state-error");
                bValid = bValid && checkLength(name, "Category Name", 3, 200);
                if (bValid) {
                    $.post("/Categories/AddCategory", {
                        Name: name.val(), Dependency: $("#ddlDependency").val(),
                        aplicationName: $("#ddlAplicationName").val()
                    }, function (data) {
                        if (data.Success == true) {
                            $("#CategoriesList").append($('<option></option').val(data.name).html(data.name));
                        }
                        else alert(data.name);
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
    $("#EditCategoryForm").dialog({
        autoOpen: false,
        height: 300,
        width: 350,
        modal: true,
        buttons: {
            "Crear": function () {
                var bValid = true;
                allFields.removeClass("ui-state-error");
                bValid = bValid && checkLength(name, "Category Name", 3, 200);
                if (bValid) {
                    $.post("/Categories/AddCategory", { Name: name.val(), Dependency: $("#ddlDependency").val() }, function (data) {
                        if (data.Success == true) {
                            $("#CategoriesList").append($('<option></option').val(data.name).html(data.name));
                        }
                        else alert(data.name);
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

    $("#NewCategory")
			.button()
			.click(function () {

			    $("#NewCategoryForm").dialog("open");
			});


    $("#CategoriesList").change(function () {
        if ($("#CategoriesList").val() != null) {
            $("#Options").load("/Categories/Options/", { categoryname: $("#CategoriesList").val() });
            $("#DeleteCategory").html('<span class="ui-button-text">Eliminar ' + $("#CategoriesList").val() + '</span>').show();
            $("#EditCategory").html('<span class="ui-button-text">Editar ' + $("#CategoriesList").val() + '</span>').show();
        }
    });
});

$("#ddlAplicationName").change(function () {
    var appNameId;
    if ($(this).val() == "") {
        appNameId = 0;
    }
    else {
        appNameId = $(this).val();
    }
    $.ajax({
        type: "POST",
        url: "/Categories/OptionsByAplicationName/",
        data: {
            'aplicationNameId': appNameId,
            'categoryname': $("#CategoriesList").val()
        },
        dataType: "html",
        success: function (data) {
            if (data.Success != false) {
                $("#Options").html("")
                $("#Options").html(data)
            }
            else {
                alert(data.ErrorMessage)
            }
        }
    });
});