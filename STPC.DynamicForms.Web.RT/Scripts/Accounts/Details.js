$(function () {
    if ($("#IsLockedOut").is(":checked")) {
        $("#lockme").hide();
        $("#unlockme").show();
    }
    else {
        $("#lockme").show();
        $("#unlockme").hide();
    }
    $("#approveme")
			.button()
			.click(function () {
			    $.post("/Account/ApproveUser", { username: $("#Username").val() },
			function (data) {
			    if (data.success == true) {
			        $("#IsApproved").attr('checked', 'checked');
			        $(this).remove();
			    }
			    else alert("Update failed");
			}, "json");
			});
    $("#lockme")
			.button()
			.click(function () {
			    $.post("/Account/LockUser", { username: $("#Username").val() },
			function (data) {
			    if (data.success == true) {
			        $("#IsLockedOut").attr('checked', 'checked');
			        $("#lockme").hide();
			        $("#unlockme").show();
			    }
			    else alert("Update failed");
			}, "json");
			});
    $("#unlockme")
			.button()
			.click(function () {
			    $.post("/Account/UnlockUser", { username: $("#Username").val() },
			function (data) {
			    if (data.success == true) {
			        $("#IsLockedOut").removeAttr('checked');
			        $("#lockme").show();
			        $("#unlockme").hide();
			    }
			    else alert("Update failed");
			}, "json");
			});
});

$("input.decorated").button();
$("a.decorated").button();