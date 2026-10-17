var GETInviceListURL = "/SendInvoice/GetAllInvoice"
var PostSendInvoiceURL = "/OmidSendInvoice/SendInvoice";
var SendGroupingInvoiceAsyncURL = "/SendInvoice/SendGroupingInvoice";
var GetAllTissInfoItemsURL = "/SendInvoice/GetAllTissInfoItems";
var PostGetInvoiceStatusByRefrenceURL = "/SendInvoice/GetInvoiceStatusByRefrence";
var GetGroupingInvoiceStatusByRefrenceURL = "/SendInvoice/GetGroupingInvoiceStatusByRefrence";
var GetGetAllInvoiceWithInqFromServerURL = "/SendInvoice/GetAllInvoiceWithInqFromServer";
var GetStorIdListURL = "/SendInvoice/GetStorIdList";
var GetStoresURL = "/SendInvoice/GetStore";
var GetErrorTypesURL = "/SendInvoice/GetErrorType";
var GetInvoiceByIDAsyncURL = "/SendInvoice/GetInvoiceByID";
var tblSendSuccess;
var tblSendNoStatus;
var tblDoNotSend;
var tblFactorItems;
var ListSendSuccess = []
var ListSendNoStatus = [];
var ListNotSend = [];
var ListSendReturnedStatus = [];
var ListSelectedSendNoStatus = [];
var ListSelectedNotSend = [];
var ListSelectSendReturnedStatus = [];
var ListOffAllInvoice = [];
var FactorID;
var connection1;
var omidSendInProgress = false;

function showLiveStatus(title, text) {
    TaxOperationStatus.update(title, text);
}
function clearLiveStatus() {
    TaxOperationStatus.hide();
}

$(document).ready(function () {

    connection1 = new signalR.HubConnectionBuilder()
        .withUrl("/requestshub")
        .build();
    connection1.start().then(function () {
        console.log("Connected to SignalR hub");
    }).catch(function (err) {
        return console.error(err.toString());
    });
    connection1.on("SendToTaxEvents", function () {
        document.dispatchEvent(new Event('SendToTaxEvents'));
    });

    var pnotifyInstance = null;
    function showNotification(data) {
        // بازخوانی/بارگذاری فاکتورها در کادر واحد وضعیت زنده
        showLiveStatus('بازخوانی / بارگذاری فاکتورها', data);
    }
    $(".ui-pnotify-history-container").remove();


    connection1.on("SendToTaxEvents", function (data) {
        try {
            showNotification(data);
        } catch (e) {
            console.error(e);
        }
    });
    connection1.on("SendToTaxState", function (data) {
        try {
            showNotificationState(data);
        } catch (e) {
            console.error(e);
        }
    });
    var pnotifyInstanceState = null;
    function showNotificationState(data) {
        // ارسال فاکتورها در کادر واحد وضعیت زنده (نمایش «X از کل»)
        showLiveStatus('ارسال فاکتورها', data);
    }

    connection1.on("SendToTaxEventsError", function (data) {
        try {
            TaxOperationStatus.fail('عملیات ناموفق', data);
        } catch (e) {
            console.error(e);
        }
    });

    var currentPath = window.location.pathname;

    // Check the current path and set the active class
    if (currentPath.includes("InquryIndex")) {
        $("#menu-inquiry").addClass("active");
        $("#menu-invoice").removeClass("active");

    } else if (currentPath.includes("Index")) {
        $("#menu-invoice").addClass("active");
        $("#menu-inquiry").removeClass("active");

    }
    $('.nav-tabs a').on('click', function (e) {
        e.preventDefault()
        $(this).tab('show')
    })
    InitialInqTables();
    InitialTabels();
    InitElemans();
    OnchangeStat();
    eventofexcel();
    eventofInq();
});
function clearNotification() {
    clearLiveStatus();
}
function eventofexcel() {
    connection1.on("SendToTaxExcel", function (data) {
        try {
            showNotificationExcel(data);
        } catch (e) {
            console.error(e);
        }
    });
    var pnotifyInstanceExcel = null;
    function showNotificationExcel(data) {
        // جمع‌آوری فاکتور برای اکسل در کادر واحد وضعیت زنده
        showLiveStatus('جمع‌آوری فاکتور برای اکسل', data);
    }

}
function eventofInq() {
    connection1.on("SendToTaxInq", function (data) {
        try {
            showNotificationInc(data);
        } catch (e) {
            console.error(e);
        }
    });
    var pnotifyInstanceInc = null;
    function showNotificationInc(data) {
        // استعلام/بازیابی وضعیت در کادر واحد وضعیت زنده
        showLiveStatus('استعلام / بازیابی وضعیت', data);
    }

}




function InitElemans() {
    try {

        setButtonsDesible(false);
        $("#btnSearch").on('click', function () {
            getAllInvoiceList();
        });
        $("#btnExportExcel").on('click', function () {
            ExportExcel();
        });
        $("#btnExportExcelSummary").on('click', function () {
            ExportExcelSummary();
        });

        $("#btnExportExcelInq").on('click', function () {
            ExportExcelInq();
        });
        $("#btnSearchInq").on('click', function () {
            InqSearch();
        });

        $("#btnCheckConnection").on('click', CheckConnection);
        $("#btnCheckConnection1").on('click', CheckConnection);
        $("#btnSelectAll").on('click', function () {
            SelectAndUnSelectAll("forSend");
        });
        // تنها مسیر ارسال در پنل امید؛ یک یا چند فاکتور انتخاب‌شده را می‌فرستد.
        $("#btnSendOmidInvoices").on('click', sendSelectedInvoicesToOmid);
        //$("#btnSendGroupForCheck").on('click', function () {
        //    groupingSendForCheck();
        //});
        GetStores();
        GetErrorTypes();
        $("#cmdStat").on('change', function () {
            ListOffAllInvoice = [];
            ListSelectedNotSend = [];
            ListSelectedSendNoStatus = [];
            ListSelectSendReturnedStatus = [];

            ListNotSend = [];
            ListSendNoStatus = [];
            ListSendReturnedStatus = [];
            ListSendSuccess = [];
            tblDoNotSend.clear();
            tblDoNotSend.draw();
            tblFactorItems.clear().draw();
            $('#btnSendGroupForTax').prop('disabled', true);
            //$('#btnSendGroupForCheck').prop('disabled', true);
        });

    } catch (errInitElemans) {
        console.log("InitElemans:" + errInitElemans);
    }
}

async function ExportExcelInq() {
    try {
        //var filter = CreateFilter();
        var filter = {};

        filter.DateFromStr = $('#txtFromDate').val();
        filter.DateToStr = $('#txtToDate').val();
        filter.Status = $('#cmdStat').val();
        filter.StorIDs = $('#cmbStores').val();

        $('#waiting').show();
        setButtonsDesible(true);
        var operationId = await TaxOperationStatus.begin(connection1, 'تهیه خروجی Excel استعلام', 0);
        $.ajax({
            url: '/SendInvoice/GetAllInvoiceWithInqFromServerInExcel', // Replace with your API endpoint
            //url: GETInviceListURL, // Replace with your API endpoint
            type: 'POST',
            data: {
                searchFilter: filter,
                operationId: operationId
            }, // Send the InvoiceId to fetch items
            //data: { search: filter }, // Send the InvoiceId to fetch items
            success: function (data) {
                // Assuming response contains an array of items
                if (data.success) {

                    file = data.Data;
                    window.location.href = "/Resources/FilesData" + "/" + file;
                    TaxOperationStatus.complete('خروجی Excel آماده شد', {
                        operationId: operationId,
                        message: 'فایل گزارش استعلام آماده دانلود است.'
                    });

                    $('#waiting').hide();
                    setButtonsDesible(false);
                    OnchangeStat();
                } else {
                    console.log(data.message);
                    TaxOperationStatus.fail('تهیه Excel ناموفق بود', { operationId: operationId, message: data.message || 'فایل گزارش ساخته نشد.' });
                    $('#waiting').hide();
                    setButtonsDesible(false);
                    OnchangeStat();
                }
            },
            error: function () {
                $('#waiting').hide();
                setButtonsDesible(false);
                OnchangeStat();
                TaxOperationStatus.fail('تهیه Excel ناموفق بود', { operationId: operationId, message: 'در تهیه فایل گزارش خطایی رخ داد.' });
                //loadingMessage.text('Error loading items.'); // Handle error case
            }
        });

    } catch (errgetAllInvoiceList) {
        console.log("getAllInvoiceList:" + errgetAllInvoiceList);
        $('#waiting').hide();
        setButtonsDesible(false);
        OnchangeStat();
        TaxOperationStatus.fail('تهیه Excel ناموفق بود', { message: errgetAllInvoiceList.message || 'فایل گزارش ساخته نشد.' });
    }
}
async function ExportExcel() {
    try {
        //var filter = CreateFilter();
        var filter = {
            InvoiceNumberFrom: $("#txtNumberFrom").val(),
            DateFromStr: $("#txtFromDate").val(),
            InvoiceNumberTo: $("#txtNumberTo").val(),
            DateToStr: $("#txtToDate").val(),
            Status: $("#cmdStat").val(),
            StorIDs: $("#cmbStores").val()
        };
        $('#waiting').show();
        setButtonsDesible(true);
        var operationId = await TaxOperationStatus.begin(connection1, 'تهیه خروجی Excel', 0);
        $.ajax({
            url: '/SendInvoice/GetAllInvoiceInExcel', // Replace with your API endpoint
            //url: GETInviceListURL, // Replace with your API endpoint
            type: 'POST',
            data: {
                searchFilter: filter,
                operationId: operationId
            }, // Send the InvoiceId to fetch items
            //data: { search: filter }, // Send the InvoiceId to fetch items
            success: function (data) {
                // Assuming response contains an array of items
                if (data.success) {

                    file = data.Data;
                    window.location.href = "/Resources/FilesData" + "/" + file;
                    TaxOperationStatus.complete('خروجی Excel آماده شد', { operationId: operationId, message: 'فایل گزارش آماده دانلود است.' });

                    $('#waiting').hide();
                    setButtonsDesible(false);
                    OnchangeStat();
                } else {
                    console.log(data.message);
                    TaxOperationStatus.fail('تهیه Excel ناموفق بود', { operationId: operationId, message: data.message || 'فایل گزارش ساخته نشد.' });
                    $('#waiting').hide();
                    setButtonsDesible(false);
                    OnchangeStat();
                }
            },
            error: function () {
                $('#waiting').hide();
                setButtonsDesible(false);
                OnchangeStat();
                TaxOperationStatus.fail('تهیه Excel ناموفق بود', { operationId: operationId, message: 'در تهیه فایل گزارش خطایی رخ داد.' });
                //loadingMessage.text('Error loading items.'); // Handle error case
            }
        });

    } catch (errgetAllInvoiceList) {
        console.log("getAllInvoiceList:" + errgetAllInvoiceList);
        $('#waiting').hide();
        setButtonsDesible(false);
        OnchangeStat();
        TaxOperationStatus.fail('تهیه Excel ناموفق بود', { message: errgetAllInvoiceList.message || 'فایل گزارش ساخته نشد.' });
    }
}

async function ExportExcelSummary() {
    try {
        //var filter = CreateFilter();
        var filter = {
            InvoiceNumberFrom: $("#txtNumberFrom").val(),
            DateFromStr: $("#txtFromDate").val(),
            InvoiceNumberTo: $("#txtNumberTo").val(),
            DateToStr: $("#txtToDate").val(),
            Status: $("#cmdStat").val(),
            StorIDs: $("#cmbStores").val()
        };
        $('#waiting').show();
        setButtonsDesible(true);
        var operationId = await TaxOperationStatus.begin(connection1, 'تهیه خروجی Excel خلاصه', 0);
        $.ajax({
            url: '/SendInvoice/GetAllInvoiceInExcelSummary', // Replace with your API endpoint
            //url: GETInviceListURL, // Replace with your API endpoint
            type: 'POST',
            data: {
                searchFilter: filter,
                operationId: operationId
            }, // Send the InvoiceId to fetch items
            //data: { search: filter }, // Send the InvoiceId to fetch items
            success: function (data) {
                // Assuming response contains an array of items
                if (data.success) {

                    file = data.Data;
                    window.location.href = "/Resources/FilesData" + "/" + file;
                    TaxOperationStatus.complete('خروجی Excel خلاصه آماده شد', { operationId: operationId, message: 'فایل گزارش خلاصه آماده دانلود است.' });

                    $('#waiting').hide();
                    setButtonsDesible(false);
                    OnchangeStat();
                } else {
                    console.log(data.message);
                    TaxOperationStatus.fail('تهیه Excel ناموفق بود', { operationId: operationId, message: data.message || 'فایل گزارش ساخته نشد.' });
                    $('#waiting').hide();
                    setButtonsDesible(false);
                    OnchangeStat();
                }
            },
            error: function () {
                $('#waiting').hide();
                setButtonsDesible(false);
                OnchangeStat();
                TaxOperationStatus.fail('تهیه Excel ناموفق بود', { operationId: operationId, message: 'در تهیه فایل گزارش خطایی رخ داد.' });
                //loadingMessage.text('Error loading items.'); // Handle error case
            }
        });

    } catch (errgetAllInvoiceList) {
        console.log("getAllInvoiceList:" + errgetAllInvoiceList);
        $('#waiting').hide();
        setButtonsDesible(false);
        OnchangeStat();
        TaxOperationStatus.fail('تهیه Excel ناموفق بود', { message: errgetAllInvoiceList.message || 'فایل گزارش ساخته نشد.' });
    }
}
function CheckConnection() {
    var $buttons = $("#btnCheckConnection, #btnCheckConnection1");
    $buttons.prop("disabled", true);

    // تست امید تا Login واقعی پیش می‌رود؛ این مسیر هیچ فاکتوری ارسال نمی‌کند.
    $.ajax({
        url: "/OmidInquiry/CheckConnection",
        type: "GET",
        dataType: "json"
    }).done(function (data) {
        var succeeded = data && data.success === true;
        new PNotify({
            title: succeeded ? "ارتباط و ورود موفق" : "تست ارتباط ناموفق",
            text: data && data.message
                ? data.message
                : "پاسخ معتبر از تست ورود امید دریافت نشد.",
            type: succeeded ? "success" : "error",
            styling: "bootstrap3",
            delay: 5000,
            closer: true,
            sticker: false
        });
    }).fail(function (xhr) {
        var message = xhr.responseJSON && xhr.responseJSON.message
            ? xhr.responseJSON.message
            : "تست ورود به امید ناموفق بود؛ لاگ امید را بررسی کنید.";
        new PNotify({
            title: "تست ارتباط ناموفق",
            text: message,
            type: "error",
            styling: "bootstrap3",
            delay: 6000,
            closer: true,
            sticker: false
        });
    }).always(function () {
        $buttons.prop("disabled", false);
    });
}
function InqSearch() {

    var filter = {};
    filter.DateFromStr = $('#txtFromDate').val();
    filter.DateToStr = $('#txtToDate').val();
    filter.Status = $('#cmdStat').val();
    filter.StorIDs = $('#cmbStores').val();
    $('#waiting').show();
    setButtonsDesible(true);
    $.ajax({
        url: '/SendInvoice/GetAllInvoiceWithInqFromServer',
        type: 'POST',
        data: {
            searchFilter: filter
        },
        success: function (data) {
            $('#waiting').hide();
            setButtonsDesible(false);
            OnchangeStat();
            if (data.success) {
                ListOffAllInvoice = [];
                ListSelectedNotSend = [];
                ListSelectedSendNoStatus = [];
                ListSelectSendReturnedStatus = [];
                ListNotSend = [];
                ListSendNoStatus = [];
                ListSendReturnedStatus = [];
                ListSendSuccess = [];
                tblInquiry.clear();
                tblInquiry.rows.add(data.Data);
                tblInquiry.draw();
                ListOffAllInvoice = data.Data;
                $('#waiting').hide();
                setButtonsDesible(false);
                OnchangeStat();
            }
            else {
                console.log(data.message);
                $('#waiting').hide();
                setButtonsDesible(false);
                OnchangeStat();
            }
        },
        error: function () {
            $('#waiting').hide();
            setButtonsDesible(false);
            OnchangeStat();
        }
    });
}
function GetStores() {

    $.get(GetStoresURL, function (RetData) {
        if (RetData.success) {
            var storeslist = RetData.Data;
            var $select = $('#cmbStores');

            $select.empty(); // پاک‌سازی قبلی

            $.each(storeslist, function () {
                $select.append($("<option />").val(this.StoreID).text(this.Name));
            });

            // Destroy previous instance if exists
            if ($select.hasClass("select2-hidden-accessible")) {
                $select.select2("destroy");
            }

            // فعال‌سازی Select2 با قابلیت چندتایی
            $select.select2({
                placeholder: "انتخاب فروشگاه‌ها",
                allowClear: true,
                width: '100%',
                closeOnSelect: false, // برای تیک‌زنی پیاپی بدون بسته شدن
                language: {
                    noResults: function () {
                        return "یافت نشد";
                    },
                    searching: function () {
                        return "در حال جستجو...";
                    }
                }
            });
        }
    });
    $('#cmbStores').on('change', function (e) {
        let selected = $(this).val(); // آرایه‌ای از StoreIDهای انتخاب‌شده

        // اگر گزینه "همه" انتخاب شده و گزینه‌های دیگری هم هست، فقط "همه" را نگه می‌داریم
        if (selected && selected.includes("-1") && selected.length > 1) {
            // فقط گزینه "همه" را نگه دار
            $(this).val(["-1"]).trigger('change');
        }

        // اگر گزینه دیگری انتخاب شود، گزینه "همه" را حذف کن
        else if (selected && !selected.includes("-1") && selected.length > 0) {
            $(this).find('option[value="-1"]').prop('selected', false);
        }
    });


}
function GetErrorTypes() {


    $('#cmdStat').change(function () {
        if ($(this).val() === "3" || $(this).val() === 3) {
            $('#cmbErrorTypes').show(); // Hide the item if value is 1
        } else {
            $('#cmbErrorTypes').hide(); // Show the item for any other value
        }
    });
    $.get(GetErrorTypesURL, function (RetData) {
        $('#cmbErrorTypes').hide();
        if (RetData.success) {
            var storeslist = RetData.Data;
            $('#cmbErrorTypes').empty();
            $('#cmbErrorTypes').append($("<option />").val(-1).text('همه'));
            $.each(RetData.Data, function (item) {
                $('#cmbErrorTypes').append($("<option />").val(this.fID).text(this.fCaption));
            });
        }
        else {
            //new PNotify({
            //    title: 'عملیات ناموفق',
            //    text: RetData.responseText,
            //    styling: 'bootstrap3'
            //});


        }
    });

}
function updateButtonStates() {
    var selectedRows = tblDoNotSend.rows('.selected').data();
    var selectedRows = tblDoNotSend.rows('.row_selected').data();

    var status = $("#cmdStat").val();

    if (selectedRows.length > 0) {
        if (
            status == 1 || status == 3 ||
            status == '1' || status == '3'
        ) {
            $('#btnSendGroupForTax').prop('disabled', false);
            //$('#btnSendGroupForCheck').prop('disabled', true);
        } else {
            $('#btnSendGroupForTax').prop('disabled', true);
            //$('#btnSendGroupForCheck').prop('disabled', false);
        }
    } else {
        $('#btnSendGroupForTax').prop('disabled', false);
        //$('#btnSendGroupForCheck').prop('disabled', true);
    }
}
function format(data) {
    var InvoiceId = data["InvoiceId"];
    var BookerWorkstationID = data["BookerWorkstationID"];
    var StoreID = data["StoreID"];
    var detailTable = $('<table>').addClass('table table-bordered table-striped').css('width', '100%');
    var loadingMessage = $('<div>').text('در حال بارگذاری...');

    // Append the loading message to the detail table
    detailTable.append(loadingMessage);

    // Make an AJAX call to fetch the items for the selected invoice
    $.ajax({
        url: GetAllTissInfoItemsURL, // Replace with your API endpoint
        type: 'POST',
        data: { InvoiceID: InvoiceId, StoreID: StoreID, BookerWorkstationID: BookerWorkstationID }, // Send the InvoiceId to fetch items
        success: function (response) {
            // Assuming response contains an array of items
            if (response && response.Items) {
                // Clear the loading message
                loadingMessage.remove();

                // Calculate the sum of tsstamS

                // Initialize the nested DataTable
                var dataTable = detailTable.DataTable({
                    data: response.Items, // Use the items from the response
                    columns: [
                        { data: "ItemID", name: "ItemID", title: "ItemID", visible: false, searchable: false },
                        {
                            title: "ردیف",
                            "data": "Index",
                            name: "Index", sortable: false,
                            render: function (data, type, row, meta) {
                                return meta.row + meta.settings._iDisplayStart + 1;
                            }
                        },
                        { data: "InvoiceID", name: "InvoiceID", title: "InvoiceID", visible: false, searchable: false },
                        { data: "sstt", name: "sstt", title: "نام کالا" },
                        { data: "feeS", name: "fee", title: "قیمت(ریال)" },
                        { data: "amS", name: "am", title: "مقدار" },
                        { data: "prdisS", name: "prdis", title: "قیمت قبل تخفیف(ریال)" },
                        { data: "dis", name: "dis", title: "تخفیف" },
                        { data: "adisS", name: "adis", title: "قیمت با تخفیف(ریال)" },
                        { data: "tsstamS", name: "tsstam", title: "قابل پرداخت(ریال)" },
                    ],
                    ordering: false,
                    searching: false,
                    paging: false,
                    info: false,
                    drawCallback: function () {
                        // Append the total row at the end of the table
                        if ($.fn.dataTable.isDataTable(detailTable)) {
                            // Clear any existing total row
                            $(detailTable).find('.total-row').remove();

                            // Create a new row for the total
                            var totalRow = $('<tr class="total-row">');
                            totalRow.append('<td colspan="8" style="text-align:left;"><strong>[جمع کل]:</strong></td>'); // Adjust colspan as needed
                            totalRow.append('<td>' + response.SumOfItems + '</td>'); // Display the total with 2 decimal points

                            // Append the total row to the table body
                            $(detailTable).find('tbody').append(totalRow);
                        }
                    }
                });
            } else {
                loadingMessage.text('No items found.'); // Handle no items case
            }
        },
        error: function () {
            loadingMessage.text('Error loading items.'); // Handle error case
        }
    });

    // Return the detail table to be displayed
    return detailTable;
}

var tblInquiry;
function InitialInqTables() {
    tblInquiry = $("#tblInquiry").DataTable({
        'paging': true,
        'lengthChange': true,
        "language": tablePersianPacketlang,
        'searching': true,
        'ordering': true,
        lengthMenu: [
            [10, 25, 50, 100, 500, 1000, 2000, 5000],
            ['10 ', '25 ', '50 ', '100', '500', '1000', '2000', '5000']
            /* ['10 ردیف', '25 ردیف', '50 ردیف', '100 ردیف', '500 ردیف']*/
        ],
        'info': true,
        'autoWidth': false,
        "order": [],
        select: {
            style: 'multi',
            selector: 'tr'
        },
        createdRow: function (row, data, dataIndex) {
            if (data["fSendState"] === '3' || data["fSendState"] === 3) {

                $(row).css("background-color", "#fa4205");
                $(row).css("color", "#000000");

            } else if (data["fSendState"] === '4' || data["fSendState"] === 4) {
                $(row).css("background-color", "green");
                $(row).css("color", "#24242b");
            }
            else if (data["fSendState"] === '2' || data["fSendState"] === 2) {
                $(row).css("background-color", "yellow");
                $(row).css("color", "#24242b");
            }
            $(row).find('td.details-control').on('click', function () {
                var tr = $(this).closest('tr');
                var row = tblInquiry.row(tr);

                if (row.child.isShown()) {
                    // This row is already open - close it
                    row.child.hide();
                    tr.removeClass('shown');
                } else {
                    // Open this row
                    row.child(format(row.data())).show();
                    tr.addClass('shown');
                }
            });

        },


        columns: [
            {
                className: 'details-control',
                orderable: false,
                data: null,
                defaultContent: '<i class="fa fa-plus"></i>',
                title: 'اقلام' // No title needed for the expand button
            },
            {
                data: "fSendState", name: "fSendState", title: "fSendState", visible:
                    false, searchable: false
            },
            {
                title: "ردیف",
                "data": "Index",
                name: "Index", sortable: false,
                render: function (data, type, row, meta) {
                    return meta.row + meta.settings._iDisplayStart + 1;
                }
            },
            { data: "InvoiceId", name: "InvoiceId", title: "InvoiceId", visible: false, searchable: false },
            { data: "StoreID", name: "StoreID", title: "StoreID", visible: false, searchable: false },
            { data: "BookerWorkstationID", name: "BookerWorkstationID", title: "BookerWorkstationID", visible: false, searchable: false },

            { data: "RealInvoiceNumber", name: "RealInvoiceNumber", title: "شماره فاکتور" },
            { data: "SendStatStr", name: "SendStatStr", title: "وضعیت" },
            { data: "PersianCreateDate", name: "PersianCreateDate", title: "تاریخ ثبت" },
            { data: "CustomerName", name: "CustomerName", title: "مشتری", sortable: false },
            { data: "InqMessage", name: "InqMessage", title: "شرح", sortable: false },

            { data: "NetPriceWithSeperator", name: "NetPriceWithSeperator", title: "مبلغ خالص(ریال)", sortable: false },
            { data: "TotalTax", name: "TotalTax", title: "مالیات", sortable: false },

            {
                title: "عملیات",
                name: "Operations",
                searchable: false,
                sortable: false,
                defaultContent: ""
                //  "<td class=\"nowrap\"><a href=\"#\" class=\"btn btn-info btn-xs send\"><i class=\"fa fa-paper-plane\"></i> ارسال </a></td>"
            }

        ],
        responsive: true,

    });
    $('#tblInquiry tbody').on('click', 'tr', function () {

        $(this).toggleClass('row_selected');

        //if (data.Add)
        //    row.addClass('row_selected');
        //else
        //    row.removeClass('row_selected');

        updateButtonStates();
    });
}
function InitialTabels() {
    tblDoNotSend = $("#tblDoNotSend").DataTable({
        paging: true,
        lengthChange: true,
        language: tablePersianPacketlang,
        searching: true,
        ordering: false,
        lengthMenu: [
            [10, 25, 50, 100, 500, 1000, 2000, 5000],
            ['10 ', '25 ', '50 ', '100', '500', '1000', '2000', '5000']
        ],
        info: true,
        autoWidth: false,
        order: [],
        select: {
            style: 'multi',
            selector: 'tr'
        },

        createdRow: function (row, data) {
            if (data.fSendState === '3' || data.fSendState === 3) {
                $(row).addClass("row-error");
            } else if (data.fSendState === '4' || data.fSendState === 4) {
                $(row).addClass("row-success");
            } else if (data.fSendState === '2' || data.fSendState === 2) {
                $(row).addClass("row-warning");
            }

            // باز و بسته کردن ردیف‌های اقلام فاکتور
            $(row).find('td.details-control').on('click', function () {
                var tr = $(this).closest('tr');
                var invoiceRow = tblDoNotSend.row(tr);

                if (invoiceRow.child.isShown()) {
                    invoiceRow.child.hide();
                    tr.removeClass('shown');
                } else {
                    invoiceRow.child(format(invoiceRow.data())).show();
                    tr.addClass('shown');
                }
            });
        },

        columns: [
            {
                className: 'details-control',
                orderable: false,
                data: null,
                defaultContent: '<i class="fa fa-plus"></i>',
                title: 'اقلام'
            },
            {
                data: "fSendState",
                name: "fSendState",
                title: "fSendState",
                visible: false,
                searchable: false
            },
            {
                title: "ردیف",
                data: "Index",
                name: "Index",
                sortable: false,
                render: function (data, type, row, meta) {
                    return meta.row + meta.settings._iDisplayStart + 1;
                }
            },
            {
                data: "InvoiceId",
                name: "InvoiceId",
                title: "InvoiceId",
                visible: false,
                searchable: false
            },
            {
                data: "StoreID",
                name: "StoreID",
                title: "StoreID",
                visible: false,
                searchable: false
            },
            {
                data: "BookerWorkstationID",
                name: "BookerWorkstationID",
                title: "BookerWorkstationID",
                visible: false,
                searchable: false
            },

            { data: "StoreName", name: "StoreName", title: "شعبه" },
            { data: "RealInvoiceNumber", name: "RealInvoiceNumber", title: "شماره فاکتور" },
            { data: "SendStatStr", name: "SendStatStr", title: "وضعیت" },
            { data: "fListOfErrorCaption", name: "fListOfErrorCaption", title: "توضیح وضعیت" },
            { data: "PersianCreateDate", name: "PersianCreateDate", title: "تاریخ ثبت" },
            { data: "CustomerName", name: "CustomerName", title: "مشتری", sortable: false },
            { data: "InvoiceDescription", name: "InvoiceDescription", title: "شرح", sortable: false },
            { data: "TotalPriceWithSeperator", name: "TotalPriceWithSeperator", title: "مبلغ فاکتور(ریال)", sortable: false },
            { data: "TotalDiscount", name: "TotalDiscount", title: "مبلغ تخفیف", sortable: false },
            { data: "NetPriceWithSeperator", name: "NetPriceWithSeperator", title: "مبلغ خالص(ریال)", sortable: false },
            { data: "TotalTax", name: "TotalTax", title: "مالیات", sortable: false }
        ],
        responsive: true
    });

    // انتخاب ردیف برای کارهای دیگر جدول
    $('#tblDoNotSend tbody').on('click', 'tr', function () {
        $(this).toggleClass('row_selected');
        updateButtonStates();
    });

    tblFactorItems = $("#tblFactorItems").DataTable({
        paging: true,
        lengthChange: true,
        language: tablePersianPacketlang,
        searching: false,
        ordering: false,
        info: true,
        autoWidth: false,
        order: [],
        columns: [
            {
                data: "ItemID",
                name: "ItemID",
                title: "ItemID",
                visible: false,
                searchable: false
            },
            {
                title: "ردیف",
                data: "Index",
                name: "Index",
                sortable: false,
                render: function (data, type, row, meta) {
                    return meta.row + meta.settings._iDisplayStart + 1;
                }
            },
            {
                data: "InvoiceID",
                name: "InvoiceID",
                title: "InvoiceID",
                visible: false,
                searchable: false
            },
            { data: "ItemName", name: "ItemName", title: "نام کالا" },
            { data: "fee", name: "fee", title: "قیمت(ریال)" }
        ],
        responsive: true
    });
}

function OnchangeStat() {
    try {
        var status = $("#cmdStat").val();
        if (status == 1 || status == 3) {
            $('#btnSendGroupForTax').prop('disabled', false);
            //$('#btnSendGroupForCheck').prop('disabled', true);
        }
        else {
            $('#btnSendGroupForTax').prop('disabled', true);
            //$('#btnSendGroupForCheck').prop('disabled', false);
        }

    } catch (errOnchangeStat) {
        console.log("OnchangeStat:" + errOnchangeStat);
    }
}
function SelectAndUnSelectAll(typ) {
    tblDoNotSend.$('tr').toggleClass('row_selected');

    return;
    try {
        var type1 = 0;
        var type2 = 0;

        if (type1 > 0 && type2 > 0) {

            alert("لیست انتخاب شده دارای ردیف هایی با وضعیت های متفاوت است و نمیتوان یک عملیات بر روی آنها انجام داد");
            return;
        }
        if (type1 > 0 && type2 == 0) {
            tblDoNotSend.$('tr').toggleClass('row_selected');

            if (ListSelectedNotSend.length == 0) {

                ListOffAllInvoice.forEach(function (item) {

                    ListSelectedNotSend.push(item.InvoiceId);
                });

            } else {
                ListSelectedNotSend = [];
                // tblDoNotSend.$('tr').removeClass('row_selected');
                tblDoNotSend.rows('.row_selected').nodes().each(function () {
                    $(this).removeClass('row_selected');
                    // $(this).toggleClass('row_selected');
                });


            }
        }
        else if (type1 == 0 && type2 > 0) {

            if (ListSelectedSendNoStatus.length == 0) {

                ListOffAllInvoice.forEach(function (item) {

                    ListSelectedSendNoStatus.push(item.InvoiceId);
                });

                tblDoNotSend.$('tr').addClass('row_selected');


            } else {
                ListSelectedSendNoStatus = [];
                tblDoNotSend.$('tr').removeClass('row_selected');
                tblDoNotSend.rows('.row_selected').nodes().each(function () {
                    $(this).removeClass('row_selected');
                });


            }
        }

        //if (typ == "forSend") {

        //} else if (typ == "forCheck") { }

    }
    catch (errSelectAndUnSelectAll) {
        console.log("SelectAndUnSelectAll:" + errSelectAndUnSelectAll);
    }
}
async function getAllInvoiceList(trackOperation) {

    try {
        //var filter = CreateFilter();
        var filter = {
            InvoiceNumberFrom: $("#txtNumberFrom").val(),
            DateFromStr: $("#txtFromDate").val(),
            InvoiceNumberTo: $("#txtNumberTo").val(),
            DateToStr: $("#txtToDate").val(),
            Status: $("#cmdStat").val(),
            StorIDs: $("#cmbStores").val(),
            ErrorTypeID: $("#cmbErrorTypes").val()
        };
        $('#waiting').show();
        setButtonsDesible(true);
        var shouldTrack = trackOperation !== false;
        var operationId = shouldTrack
            ? await TaxOperationStatus.begin(connection1, 'فراخوانی فاکتورها', 0)
            : null;
        $.ajax({
            url: '/SendInvoice/GetAllInvoice',
            type: 'POST',
            data: {
                searchFilter: filter
            },
            success: function (data) {
                // Assuming response contains an array of items
                if (data.success) {
                    ListOffAllInvoice = [];
                    ListSelectedNotSend = [];
                    ListSelectedSendNoStatus = [];
                    ListSelectSendReturnedStatus = [];

                    ListNotSend = [];
                    ListSendNoStatus = [];
                    ListSendReturnedStatus = [];
                    ListSendSuccess = [];
                    tblDoNotSend.clear();
                    tblDoNotSend.rows.add(data.Data);
                    tblDoNotSend.draw();
                    ListOffAllInvoice = data.Data;
                    if (shouldTrack) {
                        TaxOperationStatus.complete('فراخوانی فاکتورها تکمیل شد', {
                            operationId: operationId,
                            message: data.Data.length + ' فاکتور در جدول بارگذاری شد.',
                            processed: data.Data.length,
                            total: data.Data.length,
                            succeeded: data.Data.length,
                            failed: 0
                        });
                    }
                    // ListNotSend = data.doNotSend;
                    // ListSendNoStatus=data.sendNoStatus;
                    // ListSendReturnedStatus=data.sendReturnedStatus;
                    //ListSendSuccess=data.sendSuccess;
                    $('#waiting').hide();
                    setButtonsDesible(false);
                    OnchangeStat();
                } else {
                    console.log(data.message);
                    if (shouldTrack)
                        TaxOperationStatus.fail('فراخوانی فاکتورها ناموفق بود', { operationId: operationId, message: data.message || 'فاکتورها بارگذاری نشدند.' });
                    $('#waiting').hide();
                    setButtonsDesible(false);
                    OnchangeStat();
                }
            },
            error: function () {
                $('#waiting').hide();
                setButtonsDesible(false);
                OnchangeStat();
                if (shouldTrack)
                    TaxOperationStatus.fail('فراخوانی فاکتورها ناموفق بود', { operationId: operationId, message: 'در دریافت فهرست فاکتورها خطایی رخ داد.' });
                //loadingMessage.text('Error loading items.'); // Handle error case
            }
        });

    } catch (errgetAllInvoiceList) {
        console.log("getAllInvoiceList:" + errgetAllInvoiceList);
        $('#waiting').hide();
        setButtonsDesible(false);
        OnchangeStat();
        TaxOperationStatus.fail('فراخوانی فاکتورها ناموفق بود', { message: errgetAllInvoiceList.message || 'فاکتورها بارگذاری نشدند.' });
    }
}
function CreateFilter() {

    try {
        var filter = {
            InvoiceNumberFrom: $("#txtNumberFrom").val(),
            DateFromStr: $("#txtFromDate").val(),
            InvoiceNumberTo: $("#txtNumberTo").val(),
            DateToStr: $("#txtToDate").val(),
            Status: $("#cmdStat").val(),
            StorIDs: $("#cmbStores").val()
        };
        return filter;
    } catch (errCreateFilter) {
        console.log("CreateFilter:" + errCreateFilter);
    }

}
async function SendFactorForTax(row) {
    var request = {
        InvoiceID: row.InvoiceId,
        StoreID: row.StoreID,
        BookerWorkstationID: row.BookerWorkstationID,
        Status: row.fSendState
    };

    if (!request.InvoiceID || request.StoreID == null) {
        alert('شناسهٔ فاکتور یا شناسهٔ فروشگاه خالی است.');
        return;
    }

    $('#waiting').show();
    setButtonsDesible(true);

    try {
        // ابتدا فقط وضعیت محلی را می‌خوانیم؛ این درخواست فاکتور را ارسال نمی‌کند.
        var localStatus = await $.get(
            '/OmidSendInvoice/GetStatus',
            { invoiceId: request.InvoiceID }
        );

        if (!localStatus || localStatus.success !== true) {
            alert('وضعیت قبلی فاکتور خوانده نشد؛ ارسال انجام نشد.');
            return;
        }

        if (localStatus.found === true) {
            var previous = localStatus.Data;
            var message = 'این فاکتور قبلاً در امید ثبت شده است.'
                + '\nوضعیت محلی: ' + previous.State;

            if (previous.TrackingId) {
                message += '\nشناسهٔ پیگیری: ' + previous.TrackingId;
            }

            message += '\nبرای جلوگیری از ارسال تکراری، درخواست جدیدی ارسال نشد.';
            alert(message);
            return;
        }

        // فقط وقتی رکورد قبلی وجود ندارد، ارسال واقعی آغاز می‌شود.
        // رزرو اتمی سمت سرور همچنان جلوی درخواست‌های هم‌زمان را می‌گیرد.
        var response = await $.post(PostSendInvoiceURL, request);
        var result = response && response.Data;

        if (response && response.success === true &&
            result && result.Success === true) {
            new PNotify({
                title: 'درخواست ارسال پذیرفته شد',
                text: result.Message || 'نتیجهٔ نهایی باید استعلام شود.',
                type: 'success',
                styling: 'bootstrap3',
                delay: 3000
            });

            getAllInvoiceList();
        } else {
            alert(result && result.Message
                ? result.Message
                : 'نتیجهٔ ارسال مشخص نیست؛ پیش از تلاش دوباره وضعیت فاکتور را بررسی کنید.');
        }
    } catch (error) {
        // قطع ارتباط بعد از POST می‌تواند نتیجهٔ نامشخص داشته باشد.
        alert('ارتباط قطع شد یا پاسخ دریافت نشد؛ پیش از تلاش دوباره وضعیت فاکتور را بررسی کنید.');
    } finally {
        $('#waiting').hide();
        setButtonsDesible(false);
        OnchangeStat();
    }
}
async function groupingSendForTaxWithParameter() {
    try {
        var selectedRows = tblDoNotSend.rows('.row_selected').data();

        if (selectedRows.length > 0) {
            var Requests = [];

            // Collect InvoiceId from selected rows
            for (var i = 0; i < selectedRows.length; i++) {
                if (
                    selectedRows[i]['fSendState'] === 2 ||
                    selectedRows[i]['fSendState'] === '2' ||
                    selectedRows[i]['fSendState'] === 4 ||
                    selectedRows[i]['fSendState'] === '4'
                ) {
                    continue;
                }
                var InvoiceId = selectedRows[i]['InvoiceId'];
                var BookerWorkstationID = selectedRows[i]['BookerWorkstationID'];
                var StoreID = selectedRows[i]['StoreID'];
                var Status = selectedRows[i]['fSendState'];
                var dtoInvoiceLimitParameter = {};
                dtoInvoiceLimitParameter.InvoiceID = InvoiceId;
                dtoInvoiceLimitParameter.BookerWorkstationID = BookerWorkstationID;
                dtoInvoiceLimitParameter.StoreID = StoreID;
                dtoInvoiceLimitParameter.Status = Status;
                Requests.push(dtoInvoiceLimitParameter);
            }

            if (Requests.length === 0) {
                TaxOperationStatus.fail('ارسال انجام نشد', { message: 'فاکتورهای انتخاب‌شده در وضعیت قابل ارسال نیستند.' });
                return;
            }

            $('#waiting').show();
            setButtonsDesible(true);
            var sumofCount = Requests.length;
            var operationId = await TaxOperationStatus.begin(connection1, 'ارسال فاکتورها', sumofCount);
            var finalResponse = null;

            for (let i = 0; i < Requests.length; i += 100) {
                const batch = Requests.slice(i, i + 100);
                finalResponse = await $.post(SendGroupingInvoiceAsyncURL, {
                    stringList: batch,
                    SumCount: sumofCount,
                    batchSize: 100,
                    operationId: operationId
                });

                if (finalResponse.Partial === true) {
                    TaxOperationStatus.update('آماده‌سازی ارسال فاکتورها', {
                        operationId: operationId,
                        message: 'اطلاعات ' + Math.min(i + batch.length, sumofCount) + ' فاکتور از ' + sumofCount + ' دریافت شد؛ ارسال هنوز ادامه دارد.',
                        processed: 0,
                        total: sumofCount,
                        succeeded: 0,
                        failed: 0
                    });
                }
            }

            if (finalResponse && finalResponse.success) {
                var sentCount = typeof finalResponse.countOfSend === 'number' ? finalResponse.countOfSend : sumofCount;
                TaxOperationStatus.complete('ارسال تکمیل شد', {
                    operationId: operationId,
                    message: 'عملیات ارسال فاکتورهای انتخاب‌شده پایان یافت.',
                    processed: sumofCount,
                    total: sumofCount,
                    succeeded: sentCount,
                    failed: Math.max(0, sumofCount - sentCount)
                });
                getAllInvoiceList(false);
            } else {
                var partialSentCount = finalResponse && finalResponse.res
                    ? Number(finalResponse.res.Count != null ? finalResponse.res.Count : finalResponse.res.count || 0)
                    : 0;
                TaxOperationStatus.fail('ارسال با خطا پایان یافت', {
                    operationId: operationId,
                    message: (finalResponse && finalResponse.message) || 'ارسال فاکتورها کامل نشد.',
                    processed: sumofCount,
                    total: sumofCount,
                    succeeded: partialSentCount,
                    failed: Math.max(0, sumofCount - partialSentCount)
                });
            }
        } else {
            TaxOperationStatus.fail('ارسال انجام نشد', { message: 'فاکتوری جهت ارسال انتخاب نشده است.' });
        }
    } catch (errgroupingSendForTaxWithParameter) {
        console.log("groupingSendForTaxWithParameter:" + errgroupingSendForTaxWithParameter);
        TaxOperationStatus.fail('ارسال ناموفق', { message: errgroupingSendForTaxWithParameter.message || 'در ارسال فاکتورها خطایی رخ داد.' });
    } finally {
        $('#waiting').hide();
        setButtonsDesible(false);
        OnchangeStat();
    }
}

function groupingSendForCheck() {
    try {
        var selectedRows = tblDoNotSend.rows('.selected').data();
        var selectedRows = tblDoNotSend.rows('.row_selected').data();
        if (selectedRows.length > 0) {


            var Requests = [];

            for (var i = 0; i < selectedRows.length; i++) {
                var myFieldData = selectedRows[i]['InvoiceId'];
                Requests.push(myFieldData);
            }

            //data = JSON.stringify({ ids: Requests });
            setButtonsDesible(true);
            if (Requests.length > 0) {

                var formdata = new FormData();
                formdata.append("idList", Requests);


                $.post(GetGroupingInvoiceStatusByRefrenceURL,
                    { ids: Requests },
                    function (response) {
                        setButtonsDesible(false); $('#waiting').hide(); OnchangeStat();
                        if (response.success == true) {

                            setButtonsDesible(false);
                            $('#waiting').hide();
                            getAllInvoiceList();
                            OnchangeStat();
                            new PNotify({
                                title: 'ارتباط موفق',
                                delay: 2000,
                                text: 'پیگیری با موفقیت انجام شد.نتایج از بررسی اطلاعات لیست مشخص می شوند!',
                                type: 'success',
                                styling: 'bootstrap3',
                                closer: true, // Ensure the close button is enabled
                                sticker: false // Disable the sticker if you want to ensure it does not
                            });
                        }
                        else {

                            new PNotify({
                                title: 'عملیات ناموفق',
                                delay: 2000,
                                text: 'قطع ارتباط',
                                styling: 'bootstrap3',
                                closer: true, // Ensure the close button is enabled
                                sticker: false // Disable the sticker if you want to ensure it does not
                            });

                            console.log(response.message);
                            setButtonsDesible(false); $('#waiting').hide(); OnchangeStat();
                        }
                    });

            } else { alert("فاکتوری جهت ارسال انتخاب نشده است"); setButtonsDesible(false); OnchangeStat(); }
        } else { alert("فاکتوری جهت ارسال انتخاب نشده است"); setButtonsDesible(false); OnchangeStat(); }

    } catch (errgroupingSendForTax) {
        console.log("groupingSendForTax:" + errgroupingSendForTax);
        setButtonsDesible(false); OnchangeStat();
    }
}
function SendFactorForCheckTax(dataID) {
    // این بررسی فقط وضعیت ثبت‌شده در دیتابیس محلی را می‌خواند.
    // هنوز استعلام نتیجهٔ نهایی از سرور امید نیست.
    setButtonsDesible(true);

    $.get('/OmidSendInvoice/GetStatus', { invoiceId: dataID })
        .done(function (data) {
            if (!data || data.success !== true) {
                alert('خواندن وضعیت محلی فاکتور ناموفق بود.');
                return;
            }

            if (data.found !== true || !data.Data) {
                alert('برای این فاکتور هنوز رکورد ارسال امید وجود ندارد.');
                return;
            }

            var status = data.Data;
            var stateText = {
                Prepared: 'آمادهٔ ارسال؛ درخواست HTTP هنوز آغاز نشده است',
                Sending: 'ارسال آغاز شده؛ نتیجه هنوز ثبت نشده است',
                Pending: 'رسید دریافت شده؛ نتیجهٔ نهایی باید استعلام شود',
                Accepted: 'وضعیت تأییدشده در سیستم محلی',
                Failed: 'وضعیت ردشده در سیستم محلی',
                Unknown: 'نتیجه نامشخص است؛ بدون بررسی دوباره ارسال نکنید'
            }[status.State] || status.State;

            var message = 'وضعیت محلی: ' + stateText;

            if (status.TrackingId) {
                message += '\nشناسهٔ پیگیری: ' + status.TrackingId;
            }

            alert(message);
        })
        .fail(function () {
            alert('ارتباط با مسیر خواندن وضعیت محلی برقرار نشد.');
        })
        .always(function () {
            setButtonsDesible(false);
            OnchangeStat();
        });
}
function setButtonsDesible(value) {
    $('#btnSearch').prop('disabled', value);
    $('#btnSelectAll').prop('disabled', value);
    $('#btnSendOmidInvoices').prop('disabled', value);
    $('#btnSendGroupForTax').prop('disabled', value);
    //$('#btnSendGroupForCheck').prop('disabled', value);
    $('#btnSearchInq').prop('disabled', value);
    $('#btnExportExcel').prop('disabled', value);
    $('#btnExportExcelSummary').prop('disabled', value);
    $('#btnExportExcelInq').prop('disabled', value);
}

// تمام فاکتورهای انتخاب‌شده را از مسیر مستقل امید ارسال می‌کند.
// هر درخواست حداکثر ۱۰۰ فاکتور دارد و درخواست‌ها پشت سر هم اجرا می‌شوند.
async function sendSelectedInvoicesToOmid() {
    if (omidSendInProgress) return;

    var selectedRows = tblDoNotSend.rows('.row_selected').data().toArray();

    if (selectedRows.length === 0) {
        alert('ابتدا حداقل یک فاکتور انتخاب کنید.');
        return;
    }

    // فاکتورهای نامعتبر را بی‌صدا از انتخاب کاربر حذف نمی‌کنیم.
    var invalidRow = selectedRows.find(function (row) {
        var state = String(row.fSendState);
        return (state !== '1' && state !== '3') ||
            !row.InvoiceId || row.StoreID == null;
    });

    if (invalidRow) {
        alert('یکی از فاکتورهای انتخاب‌شده اطلاعات یا وضعیت قابل ارسال ندارد.');
        return;
    }

    var missingWorkstationRow = selectedRows.find(function (row) {
        return row.BookerWorkstationID == null || row.BookerWorkstationID === '';
    });

    if (missingWorkstationRow) {
        alert('BookerWorkstationID فاکتور ' +
            (missingWorkstationRow.RealInvoiceNumber || missingWorkstationRow.InvoiceId) +
            ' در فهرست خالی است؛ ارسال انجام نشد.');
        return;
    }

    var requests = selectedRows.map(function (row) {
        return {
            InvoiceID: row.InvoiceId,
            StoreID: row.StoreID,
            BookerWorkstationID: row.BookerWorkstationID,
            Status: row.fSendState
        };
    });

    omidSendInProgress = true;
    $('#btnSendOmidInvoices').prop('disabled', true);
    $('#waiting').show();
    setButtonsDesible(true);

    var pendingTotal = 0;
    var requestStarted = false;

    try {
        for (var offset = 0; offset < requests.length; offset += 100) {
            var batch = requests.slice(offset, offset + 100);
            requestStarted = true;

            var response = await $.ajax({
                url: '/OmidSendInvoice/SendSelectedInvoices',
                type: 'POST',
                contentType: 'application/json; charset=utf-8',
                data: JSON.stringify(batch)
            });

            pendingTotal += Number(response && response.pendingCount || 0);

            // اگر نتیجهٔ یک دسته نامشخص شد، دستهٔ بعدی را نمی‌فرستیم.
            if (!response || response.success !== true) {
                alert(
                    'ارسال در دستهٔ شروع‌شونده از فاکتور ' + (offset + 1) +
                    ' متوقف شد.\n' +
                    (response && response.message ? response.message : 'نتیجه مشخص نیست.') +
                    '\nتعداد Pending تا این لحظه: ' + pendingTotal +
                    '\nبدون بررسی وضعیت، دوباره ارسال نکنید.'
                );
                return;
            }
        }

        alert('درخواست ارسال ' + pendingTotal +
            ' فاکتور پذیرفته شد.\nنتیجهٔ نهایی را بعداً استعلام کنید.');
    } catch (error) {
        var message = error.responseJSON && error.responseJSON.message
            ? error.responseJSON.message
            : 'ارتباط قطع شد یا پاسخ دریافت نشد.';

        alert(message +
            '\nتعداد Pending ثبت‌شده تا این لحظه: ' + pendingTotal +
            '\nقبل از تلاش دوباره وضعیت فاکتورها را بررسی کنید.');
    } finally {
        if (requestStarted) getAllInvoiceList(false);

        $('#waiting').hide();
        setButtonsDesible(false);
        OnchangeStat();
        $('#btnSendOmidInvoices').prop('disabled', false);
        omidSendInProgress = false;
    }
}
