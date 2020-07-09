$('body').on('click', ".table #linkRow", function (e) {
    var row = $(this);
    if (!$(e.target).is("td"))
        return;

    window.location.href = row.data("link");
});


// Possibly use this later to check if the user had the side menu opened last screen to keep it open or closed.
function setCookie(cname, cvalue, exdays) {
    var d = new Date();
    d.setTime(d.getTime() + (exdays * 24 * 60 * 60 * 1000));
    var expires = "expires=" + d.toGMTString();
    document.cookie = cname + "=" + cvalue + ";" + expires + ";path=/";
}

function getCookie(cname) {
    var name = cname + "=";
    var decodedCookie = decodeURIComponent(document.cookie);
    var ca = decodedCookie.split(';');
    for (var i = 0; i < ca.length; i++) {
        var c = ca[i];
        while (c.charAt(0) == ' ') {
            c = c.substring(1);
        }
        if (c.indexOf(name) == 0) {
            return c.substring(name.length, c.length);
        }
    }
    return "";
}

var checkSessionTimeout = function () {
    // Get the session cookie
    var cookie = getCookie("SessionGuid");

    // If the cookie is not available
    if (cookie === "") {
        window.location.href = "/Session/SignIn?Timeout=true";
    }
};

// Every 10 seconds check if the session timed out
window.setInterval(checkSessionTimeout, 10000);

function debounce(func, wait, immediate) {
    var timeout;
    return function () {
        var context = this, args = arguments;
        var later = function () {
            timeout = null;
            if (!immediate) func.apply(context, args);
        };
        var callNow = immediate && !timeout;
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
        if (callNow) func.apply(context, args);
    };
};

function displaySaveMessage() {
    $("#successTag").fadeIn("normal");
    setTimeout(function () {
        $("#successTag").fadeOut("normal", function () {
            $(this).remove();
        });
    }, 4000);
}


function registerComboBoxes() {
    $(function () {
        // Register any combobox that is not a custom one.
        $('select:not(.customSelect)').each(function () {
            var comboBox = $(this);
            var select = comboBox.selectize({
                create: true,
                sortField: 'text'
            });

            // Disable combo box if set to disabled.
            if (comboBox.hasClass("disabled")) {
                select[0].selectize.disable();
            }
        });
    });
}

$('body').on('click', "#attendanceTable tr", function (event) {
    if (event.target.type !== 'checkbox') {
        $(':checkbox', this).trigger('click');
    }
});

// Change url to add tab name onto the end.
$('body').on('click', '#tabList .nav-link', function () {
    var tabButton = $(this);
    var tabName = tabButton.attr('href').replace("#", "");
    window.history.pushState(null, null, `?tab=${tabName}`);
});
