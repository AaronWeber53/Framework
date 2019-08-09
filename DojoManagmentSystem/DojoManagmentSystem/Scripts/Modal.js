$(function () {
    // When a modal form is submitted...
    $("body").on("submit", "#ModalForm", function (e) {
        e.preventDefault();  // prevent standard form submission

        // Get the form.
        var form = $(this);

        $.ajax({
            url: form.attr("action"),
            method: form.attr("method"),  // post
            data: form.serialize(),
            success: function (data) {
                if (data.ErrorMessage != undefined && data.ErrorMessage !== "") {
                    // Display given error message
                    var html = `<div class="modal-body"><h5>${data.ErrorMessage}</h5></div>`;
                    html += `<div class="modal-footer">`;
                    html += `<button type="button" class="btn btn-primary" data-dismiss="modal"><span aria-hidden="true">Close</span></button>`;
                    html += `</div>`;
                    $('.modal-header').prepend(`<h5 class="modal-title text-danger">Error</h5>`);
                    $("#modal-data").html(html);
                }
                else if (data.RedirectLink != undefined && data.RedirectLink !== "") {
                    // Redirect to given URL.
                    if (data.ModalRedirect) {
                        openAjaxModal(data.RedirectLink);
                    }
                    else {
                        window.location.href = data.RedirectLink;
                    }
                }
                else if (data.RefreshScreen) {
                    // Reload the screen.
                    window.location.reload();
                }
                else {
                    // Replace form with updated form
                    $("#modal-data").html(data);
                    registerComboBoxes();
                    registerTableCheckBoxes();                
                }
            }
        });
    });

    // When a modal link is clicked...
    $('body').on('click', '.modal-link', function (e) {
        openModalLink($(this), e);
    });

    $('.modal-row-link').on('click', function (e) {
        if (!$(e.target).is("td"))
            return;
        openModalLink($(this), e);
    });

    var openModalLink = debounce(function (caller, e) {
        e.preventDefault();
        openAjaxModal(caller.data("targeturl"));
    }, 200);

    function openAjaxModal(link) {
        $('body').off('shown.bs.modal').on('shown.bs.modal', function () {
            // Load the modal information to display.
            $.get(link, function (data) {
                // The data in this is the html that will be on the modal.
                $("#modalPlaceholder").replaceWith(data);

                // Register any new combo boxes.
                registerComboBoxes();
            });

        });
        openModal();
    }

    function openModal() {
        checkSessionTimeout();

        // Remove any current modal open.
        $("#modal-container").remove();
        $(".modal-backdrop").remove();

        // Create the structure for the modal and display.
        $(
            '<div id="modal-container" class="modal fade">' +
            '<div class="modal-dialog" role="document">' +
            '<div class="modal-content" id= "modalbody" >' +
            '<div class="modal-header">' +
            '<button type="button" class="close" data-dismiss="modal" aria-label="Close">' +
            '<span aria-hidden="true">&times;</span></button></div ><div id="modal-data" class="modal-body">' +
            '<div id="modalPlaceholder" class="d-flex justify-content-center">' +
            '<div class="spinner-grow text-primary" role="status">' +
            '<span class="sr-only" > Loading...</span>' +
            '</div>' +
            '<div class="spinner-grow text-primary" role="status">' +
            '<span class="sr-only" > Loading...</span>' +
            '</div>' +
            '<div class="spinner-grow text-primary" role="status">' +
            '<span class="sr-only" > Loading...</span>' +
            '</div>' +
            '<div class="spinner-grow text-primary" role="status">' +
            '<span class="sr-only" > Loading...</span>' +
            '</div>' +
            '</div > ' +
            '</div></div></div></div>'
        ).modal({ backdrop: 'static' }); // Make it so you cant click the side of the modal to close.

    }
});