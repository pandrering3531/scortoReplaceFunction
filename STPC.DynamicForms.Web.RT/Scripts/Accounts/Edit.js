//jQuery(document).bind('keydown', function (e) {
//    if (e.key == "F5") {
//        $(window).unbind("mouseover");
//        $(window).unbind("mouseout");
//        window.onunload = null;
//        console.log('keydown')
//    }
//    else if (e.key.toUpperCase() == "W" && prevKey == "CONTROL") {
//        window.onbeforeunload = ConfirmLeave;
//        window.onunload = doUnloading;
//        console("1");
//    }
//    else if (e.key.toUpperCase() == "R" && prevKey == "CONTROL") {
//        instanEvents()
//        console("2");
//    }
//    else if (e.key.toUpperCase() == "F4" && (prevKey == "ALT" || prevKey == "CONTROL")) {
//        instanEvents()
//        console("3");
//    }
//    prevKey = e.key.toUpperCase();

//});
//$(window).on('mouseover', (function () {
//    window.onbeforeunload = null;
//    if (navigator.userAgent.indexOf("Firefox") != -1) {
//    }
    
//}));
//$(window).on('mouseout', (function () {
    
//    window.onbeforeunload = confirmUload;
//    window.onunload = doLeave;
//}));
//function doLeave() {
//    $.get('/Account/LogOff')
//};
//function confirmUload() {
//    return 'leave';
//}