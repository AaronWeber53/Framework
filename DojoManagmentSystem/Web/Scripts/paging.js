// searching tables
function search(e) {
    // Find the table in parallel with the textboxes parent tag
    var table = e.closest("#pagingList");

    // Get the search text box value
    var textbox = table.find(".table-search");

    // Set the search data.
    table.data("search", textbox.val());
    table.data("page", 1);

    // Update the contents of the table.
    updateTable(table);
}

// Sort the table
function sort(e) {
    // Get the sort data
    var sort = e.data("sort");
    var filter = e.data("filter");
    var table = e.closest("#pagingList");

    // Set the sort data on the table
    table.data("sort", sort);
    table.data("filter", filter);
    table.data("search", "");
    table.data("page", 1);

    // Update the contents of the table.
    updateTable(table);
}

function paging(e) {
    // Get the page number
    var pagenumber = e.text();
    var table = e.closest("#pagingList");

    // Set the page number for the table
    table.data("page", pagenumber);

    // Update the contents of the table.
    updateTable(table);
}

function resetTable(e) {
    var table = e.closest("#pagingList");
    table.data("sort", "");
    table.data("filter", "");
    table.data("search", "");
    table.data("page", 1);

    updateTable(table);
}

function updateTable(table) {
    // Get all of the table information to update.
    var page = table.data("page");
    var sort = table.data("sort");
    var filter = table.data("filter");
    var search = table.data("search");
    var url = table.data("baseurl");

    // Build the url to update the table.
    var completeurl = buildurl(url, page, sort, search, filter);

    // Call the function to get table html update.
    $.ajax({
        url: completeurl,
        method: "get",
        success: function (data) {
            // Reset the updated table.
            table.replaceWith(data);
        }
    });
}

function buildurl(baseurl, page, sort, search, filter) {
    return `${baseurl}?filter=${filter}&sortOrder=${sort}&searchString=${search}&page=${page}`;
}

$('body').on('click', '.sortbutton', function (e) {
    e.preventDefault();
    sort($(this));
});

$('body').on('click', '.table-searchbutton', function (e) {
    e.preventDefault();
    search($(this));
});

$('body').on('click', '.page-link', function (e) {
    e.preventDefault();
    paging($(this));
});

$('body').on('click', '.table-reset', function (e) {
    e.preventDefault();
    resetTable($(this));
});

$('body').on('keyup', ".table-search", function (e) {
    if (event.keyCode == 13) {
        var table = $(this).closest("#pagingList");
        table.find(".table-searchbutton").click();
    }
});