function confirmation(message, destination) {
    var answer = confirm(message);
    if (answer) {
        window.location = destination;
    }
    else {
        return false;
    }
}

function updateTips(t) {
	$(".validateTips")
				.text(t)
				.addClass("ui-state-highlight");
	setTimeout(function () {
		$(".validateTips").removeClass("ui-state-highlight", 1500);
	}, 500);
}

function checkLength(o, n, min, max) {
	if (o.val().length > max || o.val().length < min) {
		o.addClass("ui-state-error");
		updateTips("La longitud de " + n + " debe ser entre " +
					min + " y " + max + ".");
		return false;
	} else {
		return true;
	}
}

function checkNumeric(o, n) {
    if (!o.val()) {
        o.addClass("ui-state-error");
        updateTips("Debe indicar " + n);
        return false;
    }
if(isNaN(o.val())) {
		o.addClass("ui-state-error");
		updateTips(n + " debe ser un número");
		return false;
	} else {
		return true;
}
}
$(document).ready(function () {
	$("#ajaxloading").hide();
	$(document).ajaxStart(function () { $("#ajaxloading").show(); });
	$(document).ajaxStop(function () { $("#ajaxloading").hide(); });
	$(".decoratedButton").button();
});