function copyToClipboard(copyString) {
    if (window.clipboardData && clipboardData.setData) {
        clipboardData.setData('text', copyString);
    }
}

//Useful for inserting content into textboxes / textareas
function insertAtCursor(myField, myValue) {
    //IE
    if (document.selection) {
        myField.focus();
        sel = document.selection.createRange();
        sel.text = myValue;
    }
    //Mozilla/Netscape
    else if (myField.selectionStart || myField.selectionStart == '0') {
        var startPos = myField.selectionStart;
        var endPos = myField.selectionEnd;
        myField.value = myField.value.substring(0, startPos)
                  + myValue
                  + myField.value.substring(endPos, myField.value.length);
    } else {
        myField.value += myValue;
    }
}

function confirmation(message, destination) {
    var answer = confirm(message);
    if (answer) {
        window.location = destination;
    }
    else {
        return false;
    }
}