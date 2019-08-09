var dropdownList;

$('.attendanceAddMemberButton').on('click', function (e) {
    e.preventDefault();
    addDropInMember(dropdownList[0].selectize);
}); 

function addDropInMember(list) {
    var value = list.getValue();
    var text = list.getItem(value)[0].innerHTML;
    writeAttendanceRecordRow(value, text);
    list.removeOption(value);
}

function writeAttendanceRecordRow(value, text) {
    var tableList = $('.attendanceMemberList');
    tableList.append(`<tr>
                            <td scope="row">
                                ${text}
                            </td>
                            <td>
                                <div class="form-group">
                                    <div class="custom-control custom-checkbox">
                                        <input type="checkbox" class="custom-control-input" id="customCheck1" value="${value}" name="present" checked>
                                        <label class="custom-control-label" for="customCheck"></label>
                                    </div>
                                </div>
                            </td>
                        </tr>`);
}

function registerAttendanceDropDown() {
    dropdownList = $('.attendanceAddDropDown').selectize({
        allowEmptyOption: false
    });
}

registerAttendanceDropDown();