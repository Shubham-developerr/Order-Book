var dataTable;

$(document).ready(function () {
    var url = window.location.search;
    if (url.includes("inprocess")) {
        loadDataTable("inprocess");
    }
    else {
        if (url.includes("completed")) {
            loadDataTable("completed");
        }
        else {
            if (url.includes("pending")) {
                loadDataTable("pending");
            }
            else {
                if (url.includes("approved")) {
                    loadDataTable("approved");
                }
                else {
                    loadDataTable("all");
                }
            }
        }
    }
});
function loadDataTable(status) {
    dataTable= $('#tblData').DataTable({
        "ajax": {
            "url": `/Admin/Order/GetAll?status=` +status  
        },
        "columns": [
            { "data": "id"},
            { "data": "name"},
            { "data": "phoneNumber"},
            { "data": "applicationUser.email"},
            { "data": "orderStatus"},
            { "data": "orderTotal"},
            {
                "data": "id",
                "render": function (data) {
                    return `
                    <a style="width:65%" href="/Admin/Order/Details?orderId=${data}" class="btn btn-warning">Details</a>
                   
                    `
                }
            }
        ]
    });
}
