﻿$(function () {
	if ($("#NoItems").val()=='True') {
		$("#TheTable").hide();
		$("#NothingToSeeHere").show();
	} else {
		
			$("#TheTable").show();
			$("#NothingToSeeHere").hide();
		 
	}
	$("#NewPage")
			.button()
			.click(function () {
				$("#NewPageDialog").dialog("open");
			});

	var PageName = $("#PageName"),
				PageDescription = $("#PageDescription"),
				PageStrategy = $("#PageStrategy"),
				allFieldsInNewOptionForm = $([])
					.add(PageName)
					.add(PageDescription)
					.add(PageStrategy),
				tips = $(".validateTips");

	$("a.decorated").button();

	$("#NewPageDialog").dialog({
		autoOpen: false,
		height: 400,
		width: 350,
		modal: true,
		buttons: {
			"Crear": function () {
				var bValid = true;
				allFieldsInNewOptionForm.removeClass("ui-state-error");
				bValid = bValid && checkLength(PageName, "Nombre", 1, 64);
				bValid = bValid && checkLength(PageDescription, "Nombre", 1, 200);
				bValid = bValid && checkNumeric(PageStrategy, "Estrategia");
				if (bValid) {
					$.post("/FormPage/Create", { name: PageName.val(), desc: PageDescription.val(), strategy: PageStrategy.val(), formId: $("#FormId").val() },
			function (data) {
				if (data.success == true) {
					$("#TheTable > tbody:last").append("<tr><td>" + data.Name +
						"</td><td>" + data.Desc + "</td><td>" + data.Strategy + 
						"</td><td><a class='decorated' href='/FormPage/Edit/" + data.uid + "'>Editar</a> " +
						"<a class='decorated' href='/FormPage/Delete/" + data.uid + "'>Eliminar</a></td></tr>");
					$("#TheTable").show();
					$("#NothingToSeeHere").hide();
					$("a.decorated").button();
				}
				else alert("Falló la creación");
			}, "json");
					$(this).dialog("close");
				}
			},
			"Cancelar": function () {
				$(this).dialog("close");
			}
		},
		close: function () {
			allFieldsInNewOptionForm.val("").removeClass("ui-state-error");
		}
	});
});

function deleteForm(key) {
    if (confirm("¿Seguro de borrar este formulario?")) {
        $.post("/Form/DeletePost", { id: key },
                 function (data) {
                     if (data.Success == true) {
                      
                         window.location.reload(false);
                     }
                     else alert("Delete failed");
                 }, "json");
    }
}

