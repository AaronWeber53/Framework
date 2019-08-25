// searching tables
function search(e) {
    // Get the search text box value
    var textbox = e.siblings("input").val();

    // Find the table in parallel with the textboxes parent tag
    var table = e.parent().siblings("#pagingList");

    // Set the search data.
    table.data("search", textbox);
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

$('body').on('click', '.searchButton', function (e) {
    e.preventDefault();
    search($(this));
});

$('body').on('click', '.page-link', function (e) {
    e.preventDefault();
    paging($(this));
});