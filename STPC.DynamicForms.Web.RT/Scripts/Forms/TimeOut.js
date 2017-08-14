/* Page Time Out Warning Message Script */
var myVar1;
var myVar2;
var time1;
var time2;
var sessionTimeoutWarning = 0;
var sessionTimeout = 0;
var timeOnPageLoad;
var timeForExpiry;
var objInstanceName;
try {
    $.ajax({
        type: "POST",
        url: "/Account/GetConfigInfo",
        dataType: "json",
        success: function (result) {
            sessionTimeoutWarning = result[0];
            sessionTimeout = result[1]

            console.log('GetConfigInfo')
            var sTimeout = (parseInt(sessionTimeout) * 60 * 1000) - (parseInt(sessionTimeoutWarning) * 60 * 1000);

            timeOnPageLoad = new Date();
            myVar1 = setTimeout('SessionWarning()', sTimeout);
            //To redirect to the home page
            myVar2 = setTimeout('RedirectToWelcomePage()', parseInt(sessionTimeout) * 60 * 1000);

        },
        error: function (req, status, error) {
            alert("Error: " + error);
        }
    });


} catch (e) {
    alert(e.message)
}
function setWarningTimeOut(varTimeWarning, varSessionTimeout) {

    if (varTimeWarning != undefined && varSessionTimeout != undefined) {
        sessionTimeoutWarning = varTimeWarning;
        sessionTimeout = varSessionTimeout;
        console.log(varSessionTimeout)
        //alert('Estableciendo timeOut asyncrono')
        clearTimeout(myVar1);
        clearTimeout(myVar2);
        clearTimeout(time1);
        clearTimeout(time2);


        var sTimeout = (parseInt(sessionTimeout) * 60 * 1000) - (parseInt(sessionTimeoutWarning) * 60 * 1000);

        timeOnPageLoad = new Date();
        time1 = setTimeout('SessionWarning()', sTimeout);
        //clearTimeout(time2);   
        //To redirect to the home page
        time2 = setTimeout('RedirectToWelcomePage()', parseInt(sessionTimeout) * 60 * 1000);
    }
}

//Session Warning
function SessionWarning() {
    //minutes left for expiry    
    var minutesForExpiry = (parseInt(sessionTimeout) -
                                        parseInt(sessionTimeoutWarning));
    var message = "La sesión va a expirar";

    alert(message);
    var currentTime = new Date();

    //time for expiry
    timeForExpiry = timeOnPageLoad.setMinutes(timeOnPageLoad.getMinutes()
                                        + parseInt(sessionTimeout));

    //Current time is greater than the expiry time
    if (Date.parse(currentTime) > timeForExpiry) {
        //alert("La sesión ha expirado");
        //var urlSite = window.location.origin = window.location.protocol + "//" + window.location.hostname + (window.location.port ? ':' + window.location.port : '');
        //location.href = urlSite 
        CloseSession();
    }
}

//Session timeout add home page where you want to redirect after session timeout
function RedirectToWelcomePage() {

    alert("La sesión ha expirado");
    CloseSession();

}



function refreshTimeOut(value) {

    $.ajax({
        type: "POST",
        url: "/Account/GetConfigInfo",
        dataType: "json",
        success: function (result) {
            sessionTimeoutWarning = result[0];
            sessionTimeout = result[1];

            //alert('Estableciendo timeOut asyncrono')
            clearTimeout(myVar1);
            clearTimeout(myVar2);
            clearTimeout(time1);
            clearTimeout(time2);

            var sTimeout = (parseInt(sessionTimeout) * 60 * 1000) - parseInt(value);

            timeOnPageLoad = new Date();
            //time1 = setTimeout('SessionWarning()', sTimeout);
            //clearTimeout(time2);   
            //To redirect to the home page
            //time2 = setTimeout('RedirectToWelcomePage()', parseInt(value));

        },
        error: function (req, status, error) {
            alert("Error: " + error);
        }
    });


}

function CloseSession() {
    $.ajax({
        type: "POST",
        url: "/Account/LogOff",
        dataType: "json",
        success: function (result) {

            window.location.href = "/Account/LogOn";
        },
        error: function (req, status, error) {
            redirect()
        }
    });
}
function redirect()
{
    window.location.href = "/Account/LogOn";
}