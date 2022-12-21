var dataTable;

$(document).ready(function () {
    companyDataTable()
});

function companyDataTable() {
    dataTable=$('#companyTable').DataTable({
        "ajax": {
            "url": "/Admin/Company/GetAll"
            
        },
        "columns": [
            { "data": "name" },
            { "data":"streetAddress"},
            { "data": "city" },
            { "data": "state" },
            { "data": "postalCode" },
            { "data": "phoneNumber" },
            {
                "data": "id",
                "render": function (data) {
                    return `
                    <a style="width:45%" href="/Admin/Company/Upsert?id=${data}" class="btn btn-warning">Update</a>
                    <a onclick="return Delete('/Admin/Company/Delete/${data}')" style="width:40%" class="btn btn-danger">Delete</a>
                    `
                }

            }
        ]
    });
}

async function Delete(url) {
    try {
        const response = await fetch(url, { method: 'DELETE' });
        if (response.ok) {
            const responseJson = await response.json();
            if (responseJson.success) {
                swal({
                    title: "Are you sure?",
                    text: "Sure to delete Company?",
                    icon: "warning",
                    buttons: true,
                    dangerMode: true,
                }).then((willDelete) => {
                    if (willDelete) {
                        swal("Company has been deleted!", {
                            icon: "success",
                        });
                        dataTable.ajax.reload();
                    } else {
                        swal("Your imaginary file is safe!");
                    }
                });
            }
        }
    }
    catch (error) {
        throw new Error(error);
    }
    
}