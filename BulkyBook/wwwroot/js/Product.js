var dataTable;

$(document).ready(function () {
    loadDataTable()
});

function loadDataTable() {
    dataTable= $('#tblData').DataTable({
        "ajax": {
            "url": "/Admin/Product/GetAll"
        },
        "columns": [
            { "data": "title"},
            { "data": "isbn"},
            { "data": "author"},
            { "data": "price"},
            { "data": "category.name"},
            { "data": "coverType.name"},
            {
                "data": "id",
                "render": function (data) {
                    return `
                    <a style="width:45%" href="/Admin/Product/Upsert?id=${data}" class="btn btn-warning">Update</a>
                    <a onclick="return Delete('/Admin/Product/Delete/${data}')" style="width:40%" class="btn btn-danger" >Delete</a>
                    `
                }
            }
        ]
    });
}
function Delete(url) {
    $.ajax({
        url: url,
        type: 'DELETE',
        success: function (data) {
            if (data.success) {
                swal({
                    title: "Are you sure?",
                    text: "Sure to delete Product!",
                    icon: "warning",
                    buttons: true,
                    dangerMode: true,
                }).then((willDelete) => {
                    if (willDelete) {
                        swal("Poof! Product has been deleted!", {
                            icon: "success",
                        });
                        dataTable.ajax.reload();
                    } else {
                        swal("Your imaginary file is safe!");
                    }
                });
            }
        }
    })
}