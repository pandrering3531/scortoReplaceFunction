$(function () {
	$("#HierarchyLevelsSelect").change(function () {
	    $.getJSON("/Hierarchies/GetHierarchiesForLevel",
				{ level: $(this).val() },
				function (result) {
					var select = $("#HierarchyId");
					select.empty();
					$.each(result, function (index, itemData) { select.append($('<option/>', { value: itemData.Value, text: itemData.Text })); });
				});
	});
});

$("input.decorated").button();
$("a.decorated").button();