$(function () {
    $("#HierarchyLevelsSelect").change(function () {
        loadHierarchy($(this).val(), "");

    });
});
$("input.decorated").button();
$("a.decorated").button();

function loadHierarchy(levelName, hierarchy) {
    var appId;
    if ($("#ddlAplicationName").val() == "") {
        appId = 0;
    }
    else {
        appId = $("#ddlAplicationName").val();
    }
    $.getJSON("/Hierarchies/GetHierarchiesForLevelByAplicationName",
               { level: levelName, aplicationNameId: appId },
               function (result) {
                   var select = $("#HierarchyId");
                   select.empty();
                   $.each(result, function (index, itemData) {
                       console.log(itemData.Value)
                       if (itemData.Value == hierarchy) {
                           select.append($('<option/>', {
                               value: itemData.Value,
                               text: itemData.Text,
                               selected: true
                           }));
                       }
                       else {
                           select.append($('<option/>', {
                               value: itemData.Value,
                               text: itemData.Text

                           }));
                       }
                   });
               });
}