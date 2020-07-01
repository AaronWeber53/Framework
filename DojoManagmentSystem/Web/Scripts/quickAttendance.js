// When the attendance check in button is clicked.
$('#quickSubmitButton').on('click', function () {
    var button = $(this);
    var body = button.closest('#formBody');

    // Get the discipline and memberid
    var disciplineId = body.find("#DisciplineID").val();
    var memberId = body.find("#MemberID").val();

    // Submit the record.
    submitAttendanceRecord(memberId, disciplineId);
});

function submitAttendanceRecord(memberId, disciplineId) {

    $.ajax({
        url: "/AttendanceSheet/QuickAttendanceRecord",
        method: "POST",
        data: {
            memberId,
            disciplineId
        },
        success: function (data) {
            if (data.Error) {

                return;
            }

            var message = "";
            // If the member has been checkedin...
            if (data.checkedIn) {
                message = `${data.member} has already been checked in`;
            }
            else {
                message = `${data.member} has been checked in for ${data.classSession}`;
            }

            var tag = $('body').find('#showTag');

            // Display the checkin message.
            showCheckInTag(tag, message);
        }
    });

}

function showCheckInTag(tag, message) {
    // Create popup html.
    var html = `<div class="alert alert-dismissible alert-success" id="checkInTag" style="display:none;">`;
    html += `<strong id="checkInText">${message}</strong>`;
    html += `</div></div>`;

    tag.html(html);
    var checkInMessage = $('#checkInTag');

    // Display message and fade after 4 seconds.
    checkInMessage.fadeIn("normal");
    setTimeout(function () {
        checkInMessage.fadeOut("normal", function () {
            $(this).remove();
        });
    }, 4000);

}