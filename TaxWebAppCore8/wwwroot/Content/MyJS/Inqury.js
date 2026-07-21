var GETInviceListURL = "/SendInvoice/GetAllInvoice"
var PostSendInvoiceURL = "/SendInvoice/SendInvoice";
var PostSendGroupingInvoiceURL = "/SendInvoice/SendGroupingInvoice";
var SendGroupingInvoiceAsyncURL = "/SendInvoice/SendGroupingInvoice";
var GetAllTissInfoItemsURL = "/SendInvoice/GetAllTissInfoItems";
var SendGroupingInvoiceWithParameterAsync2URL = "/SendInvoice/SendGroupingInvoiceWithParameter2";
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

// ===== کادر واحد «وضعیت زنده» =====
// رفع باگ ماندگاری پیام‌ها (حذف setInterval تکرارشونده) و جلوگیری از نمایش روی‌هم پیام‌ها.
var liveStatusNotify = null;
var liveStatusTimer = null;
var LIVE_STATUS_IDLE_MS = 180000; // اگر تا ۳ دقیقه پیام جدیدی نیاید، کادر خودکار بسته می‌شود
function showLiveStatus(title, text) {
    try {
        if (liveStatusTimer) { clearTimeout(liveStatusTimer); liveStatusTimer = null; }
        var isOpen = liveStatusNotify && (liveStatusNotify.state === 'open' || liveStatusNotify.state === 'opening');
        if (isOpen) {
            liveStatusNotify.update({ title: title, text: text, type: 'success' });
        } else {
            liveStatusNotify = new PNotify({
                title: title,
                text: text,
                type: 'success',
                addclass: 'custom-blue-notification',
                styling: 'bootstrap3',
                hide: false,
                closer: true,
                sticker: false,
                width: '380px',
                history: false
            });
        }
        liveStatusTimer = setTimeout(clearLiveStatus, LIVE_STATUS_IDLE_MS);
    } catch (e) {
        console.log('showLiveStatus:' + e);
    }
}
function clearLiveStatus() {
    if (liveStatusTimer) { clearTimeout(liveStatusTimer); liveStatusTimer = null; }
    if (liveStatusNotify) {
        try { liveStatusNotify.remove(); } catch (e) { }
        liveStatusNotify = null;
    }
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
    //connection1.on("SendToTaxEvents", function () {
    //    // Trigger the custom event
    //    document.dispatchEvent(new Event('SendToTaxEvents'));
    //});
    //let currentNotification = null; // Variable to hold the current notification

    //let currentNotification = null; // Variable to hold the current notification

    //connection1.on("SendToTaxEvents", function (data) {
    //    try {
    //        setButtonsDesible(false);

    //        // Remove all existing notifications

    //        PNotify.removeAll();

    //        // Create a new notification
    //        currentNotification = new PNotify({
    //            title: 'ثبت موفق',
    //            text: data,
    //            type: 'success',
    //            styling: 'bootstrap3',
    //            closer: true, // Ensure the close button is enabled
    //            sticker: false, // Disable the sticker if you want to ensure it does not
    //            delay: false, // Keep the notification open indefinitely
    //            after_close: function () {
    //                currentNotification = null; // Reset on close
    //            }
    //        });
    //    } catch (e) {
    //        console.error(e);
    //    }
    //});

    function showNotification(data) {
        // نمایش در کادر واحد وضعیت زنده
        showLiveStatus('ثبت / بازخوانی فاکتورها', data);
    }
    $(".ui-pnotify-history-container").remove();
    function clearNotification() {
        clearLiveStatus();
    }

    //connection1.on("SendToTaxEvents", function (data) {
    //    try {
    //        setButtonsDesible(false);
    //        showNotification(data);
    //    } catch (e) {
    //        console.error(e);
    //    }
    //});
    //connection1.on("SendToTaxEvents", function (data) {
    //    try {
    //        setButtonsDesible(false);
    //        //$('#waiting').hide();
    //        // You can access the data object here
    //        new PNotify({
    //            title: 'ثبت موفق',
    //            text: data,
    //            type: 'success',
    //            styling: 'bootstrap3',
    //            closer: true, // Ensure the close button is enabled
    //            sticker: false // Disable the sticker if you want to ensure it does not
    //        });
    //    } catch (e) {
    //        console.error(e);
    //    }
    //});
    connection1.on("SendToTaxEventsError", function (data) {
        try {
            setButtonsDesible(false);
            //$('#waiting').hide();
            // You can access the data object here
            new PNotify({
                title: 'استعلام با خطا',
                text: data,
                type: 'warning',
                styling: 'bootstrap3',
                closer: true, // Ensure the close button is enabled
                sticker: false, // Disable the sticker if you want to ensure it does not
                addclass: 'custom-blue-notification',

            });
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
function InitElemans() {
    try {

        setButtonsDesible(false);
        $("#btnSearch").on('click', function () {
            getAllInvoiceList();
        });
        $("#btnExportExcel").on('click', function () {
            ExportExcel();
        }); $("#btnExportExcelInq").on('click', function () {
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
        $("#btnSendGroupForTax").on('click', function () {
            groupingSendForTaxWithParameter();
        });
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

function ExportExcelInq() {
    try {
        //var filter = CreateFilter();
        var filter = {};

        filter.DateFromStr = $('#txtFromDate').val();
        filter.DateToStr = $('#txtToDate').val();
        filter.Status = $('#cmdStat').val();
        filter.StorIDs = $('#cmbStores').val();

        $('#waiting').show();
        setButtonsDesible(true);
        $.ajax({
            url: '/SendInvoice/GetAllInvoiceWithInqFromServerInExcel', // Replace with your API endpoint
            //url: GETInviceListURL, // Replace with your API endpoint
            type: 'POST',
            data: {
                searchFilter: filter
            }, // Send the InvoiceId to fetch items
            //data: { search: filter }, // Send the InvoiceId to fetch items
            success: function (data) {
                // Assuming response contains an array of items
                if (data.success) {

                    file = data.Data;
                    window.location.href = "/Resources/FilesData" + "/" + file;

                    $('#waiting').hide();
                    setButtonsDesible(false);
                    OnchangeStat();
                } else {
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
                //loadingMessage.text('Error loading items.'); // Handle error case
            }
        });

    } catch (errgetAllInvoiceList) {
        console.log("getAllInvoiceList:" + errgetAllInvoiceList);
        $('#waiting').hide();
        setButtonsDesible(false);
        OnchangeStat();
    }
}
function ExportExcel() {
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
        $.ajax({
            url: '/SendInvoice/GetAllInvoiceInExcel', // Replace with your API endpoint
            //url: GETInviceListURL, // Replace with your API endpoint
            type: 'POST',
            data: {
                searchFilter: filter
            }, // Send the InvoiceId to fetch items
            //data: { search: filter }, // Send the InvoiceId to fetch items
            success: function (data) {
                // Assuming response contains an array of items
                if (data.success) {

                    file = data.Data;
                    window.location.href = "/Resources/FilesData" + "/" + file;

                    $('#waiting').hide();
                    setButtonsDesible(false);
                    OnchangeStat();
                } else {
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
                //loadingMessage.text('Error loading items.'); // Handle error case
            }
        });

    } catch (errgetAllInvoiceList) {
        console.log("getAllInvoiceList:" + errgetAllInvoiceList);
        $('#waiting').hide();
        setButtonsDesible(false);
        OnchangeStat();
    }
}
function CheckConnection() {
    $.get("/SendInvoice/CheckConnection", function (data) {
        if (data.Data === true) {
            new PNotify({
                title: 'ارتباط موفق',
                text: 'ارتباط با سرور برقرار است!',
                type: 'success',
                styling: 'bootstrap3',
                delay: 2000,
                closer: true, // Ensure the close button is enabled
                sticker: false // Disable the sticker if you want to ensure it does not
            });
        }
        else {
            new PNotify({
                title: 'عملیات ناموفق',
                text: 'قطع ارتباط',
                delay: 2000,
                styling: 'bootstrap3',
                closer: true, // Ensure the close button is enabled
                sticker: false // Disable the sticker if you want to ensure it does not
            });
        }
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
        if (status == 1 || status == 3) {
            $('#btnSendGroupForTax').prop('disabled', false);
            //$('#btnSendGroupForCheck').prop('disabled', true);
        } else {
            $('#btnSendGroupForTax').prop('disabled', true);
            //$('#btnSendGroupForCheck').prop('disabled', false);
        }
    } else {
        $('#btnSendGroupForTax').prop('disabled', true);
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

//function format(data) {
//    var InvoiceId = data["InvoiceId"];
//    var detailTable = $('<table>').addClass('table table-bordered table-striped').css('width', '100%');
//    var loadingMessage = $('<div>').text('در حال بارگذاری...');

//    // Append the loading message to the detail table
//    detailTable.append(loadingMessage);

//    // Make an AJAX call to fetch the items for the selected invoice
//    $.ajax({
//        url: GetAllTissInfoItemsURL, // Replace with your API endpoint
//        type: 'POST',
//        data: { InvoiceID: InvoiceId }, // Send the InvoiceId to fetch items
//        success: function (response) {
//            // Assuming response contains an array of items
//            if (response && response.Items) {
//                // Clear the loading message
//                loadingMessage.remove();

//                // Initialize the nested DataTable
//                detailTable.DataTable({
//                    data: response.Items, // Use the items from the response
//                    columns: [

//                        { data: "ItemID", name: "ItemID", title: "ItemID", visible: false, searchable: false },
//                        {
//                            title: "ردیف",
//                            "data": "Index",
//                            name: "Index", sortable: false,
//                            render: function (data, type, row, meta) {
//                                return meta.row + meta.settings._iDisplayStart + 1;
//                            }
//                        },
//                        { data: "InvoiceID", name: "InvoiceID", title: "InvoiceID", visible: false, searchable: false },

//                        { data: "sstt", name: "sstt", title: "نام کالا" },
//                        { data: "feeS", name: "fee", title: "قیمت(ریال)" },
//                        { data: "amS", name: "am", title: "مقدار" },
//                        { data: "prdisS", name: "prdis", title: "قیمت قبل تخفیف(ریال)" },
//                        { data: "dis", name: "dis", title: "تخفیف" },
//                        { data: "adisS", name: "adis", title: "قیمت با تخفیف(ریال)" },
//                        { data: "tsstamS", name: "tsstam", title: "قابل پرداخت(ریال)" },

//                    ],
//                    ordering: false,
//                    searching: false,
//                    paging: false,
//                    info: false
//                });
//            } else {
//                loadingMessage.text('No items found.'); // Handle no items case
//            }
//        },
//        error: function () {
//            loadingMessage.text('Error loading items.'); // Handle error case
//        }
//    });

//    // Return the detail table to be displayed
//    return detailTable;

//}
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
            { data: "fSendState", name: "fSendState", title: "fSendState", visible: false, searchable: false },
            {
                title: "ردیف",
                "data": "Index",
                name: "Index", sortable: false,
                render: function (data, type, row, meta) {
                    return meta.row + meta.settings._iDisplayStart + 1;
                }
            },
            { data: "InvoiceId", name: "InvoiceId", title: "InvoiceId", visible: false, searchable: false },
            { data: "StoreName", name: "StoreName", title: "شعبه" },
            { data: "RealInvoiceNumber", name: "RealInvoiceNumber", title: "شماره فاکتور" },
            { data: "SendStatStr", name: "SendStatStr", title: "وضعیت" },
            { data: "fListOfErrorCaption", name: "fListOfErrorCaption", title: "توضیح وضعیت" },
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
    $("#tblInquiry").on("click", ".send", function () {

        var current_row = $(this).parents('tr');
        if (current_row.hasClass('child')) {
            current_row = current_row.prev();
        }
        var data = tblInquiry.row(current_row).data();
        var dataID = data["InvoiceId"];
        var sendStat = data["fSendState"];
        if (sendStat == 1 || sendStat == "1" || sendStat == 3 || sendStat == "3") {
            SendFactorForTax(dataID);
        }
        else if (sendStat == 2 || sendStat == "2") {
            SendFactorForCheckTax(dataID);
        }



    });


}
function InitialTabels() {
    tblDoNotSend = $("#tblDoNotSend").DataTable({
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
                var row = tblDoNotSend.row(tr);

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
            { data: "fSendState", name: "fSendState", title: "fSendState", visible: false, searchable: false },
            {
                title: "ردیف",
                "data": "Index",
                name: "Index", sortable: false,
                render: function (data, type, row, meta) {
                    return meta.row + meta.settings._iDisplayStart + 1;
                }
            },
            { data: "InvoiceId", name: "InvoiceId", title: "InvoiceId", visible: false, searchable: false },

            { data: "RealInvoiceNumber", name: "RealInvoiceNumber", title: "شماره فاکتور" },
            { data: "SendStatStr", name: "SendStatStr", title: "وضعیت" },
            { data: "PersianCreateDate", name: "PersianCreateDate", title: "تاریخ ثبت" },
            { data: "CustomerName", name: "CustomerName", title: "مشتری", sortable: false },
            { data: "InvoiceDescription", name: "InvoiceDescription", title: "شرح", sortable: false },
            { data: "TotalPriceWithSeperator", name: "TotalPriceWithSeperator", title: "مبلغ فاکتور(ریال)", sortable: false },
            { data: "TotalDiscount", name: "TotalDiscount", title: "مبلغ تخفیف", sortable: false },
            { data: "NetPriceWithSeperator", name: "NetPriceWithSeperator", title: "مبلغ خالص(ریال)", sortable: false },
            { data: "TotalTax", name: "TotalTax", title: "مالیات", sortable: false },

            {
                title: "عملیات",
                name: "Operations",
                searchable: false
                , sortable: false,

                defaultContent: ""
                //  "<td class=\"nowrap\"><a href=\"#\" class=\"btn btn-info btn-xs send\"><i class=\"fa fa-paper-plane\"></i> ارسال </a></td>"

                //  "<td class=\"nowrap\"><a href=\"#\" class=\"btn btn-info btn-xs send\"><i class=\"fa fa-paper-plane\"></i> ارسال </a><a href=\"#\" class=\"btn btn-primary btn-xs showG\"><i class=\"fa fa-eye\"></i> نمایش</a></td>"
            }

        ],
        responsive: true,

    });
    $('#tblDoNotSend tbody').on('click', 'tr', function () {

        $(this).toggleClass('row_selected');

        //if (data.Add)
        //    row.addClass('row_selected');
        //else
        //    row.removeClass('row_selected');

        updateButtonStates();
    });
    $("#tblDoNotSend").on("click", ".send", function () {

        var current_row = $(this).parents('tr');
        if (current_row.hasClass('child')) {
            current_row = current_row.prev();
        }
        var data = tblDoNotSend.row(current_row).data();
        var dataID = data["InvoiceId"];
        var sendStat = data["fSendState"];
        if (sendStat == 1 || sendStat == "1" || sendStat == 3 || sendStat == "3") {
            SendFactorForTax(dataID);
        }
        else if (sendStat == 2 || sendStat == "2") {
            SendFactorForCheckTax(dataID);
        }



    });

    tblFactorItems = $("#tblFactorItems").DataTable({
        'paging': true,
        'lengthChange': true,
        "language": tablePersianPacketlang,
        'searching': false,
        'ordering': false,

        'info': true,
        'autoWidth': false,
        "order": [],

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

            { data: "ItemName", name: "ItemName", title: "نام کالا" },
            { data: "fee", name: "fee", title: "قیمت(ریال)" },




        ],
        responsive: true,

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

        //if (ListOffAllInvoice.length > 0) {
        //    ListOffAllInvoice.forEach(function (item) {
        //        if (item.SendStat == 1 || item.SendStat == 3) {
        //            type1 = type1 + 1;
        //        }
        //        if (item.SendStat == 2 || item.SendStat == 4) {
        //            type2 = type2 + 1;
        //        }


        //    });

        //}
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

                //tblDoNotSend.$('tr').addClass('row_selected');


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
function getAllInvoiceList() {
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
                    // ListNotSend = data.doNotSend;
                    // ListSendNoStatus=data.sendNoStatus;
                    // ListSendReturnedStatus=data.sendReturnedStatus;
                    //ListSendSuccess=data.sendSuccess;
                    $('#waiting').hide();
                    setButtonsDesible(false);
                    OnchangeStat();
                } else {
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
                //loadingMessage.text('Error loading items.'); // Handle error case
            }
        });

    } catch (errgetAllInvoiceList) {
        console.log("getAllInvoiceList:" + errgetAllInvoiceList);
        $('#waiting').hide();
        setButtonsDesible(false);
        OnchangeStat();
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
function SendFactorForTax(dataID) {
    try {

        var request = { InvoiceId: dataID, isReturn: true };
        $('#waiting').show();
        setButtonsDesible(true);

        $.post(PostSendInvoiceURL, request)
            .done(function (data) {
                if (data.success) {
                    if (data.res.success == true) {
                        new PNotify({
                            title: 'ارتباط موفق',
                            text: 'فاکتور با موفقیت ارسال شد!',
                            type: 'success',
                            styling: 'bootstrap3',
                            delay: 2000,
                            closer: true, // Ensure the close button is enabled
                            sticker: false // Disable the sticker if you want to ensure it does not
                        });
                        getAllInvoiceList();
                        OnchangeStat();
                    } else {
                        alert(data.res.message);
                        setButtonsDesible(false);
                        $('#waiting').hide(); OnchangeStat();
                    }

                } else {
                    alert(data.res.message);
                    setButtonsDesible(false);
                    $('#waiting').hide(); OnchangeStat();

                }
                console.log(data);
            })
            .fail(function (error) {
                setButtonsDesible(false);
                $('#waiting').hide();
                OnchangeStat();
            });

    } catch (errSendFactorForTax) {
        console.log("SendFactorForTax:" + errSendFactorForTax);
        setButtonsDesible(false);
        $('#waiting').hide(); OnchangeStat();
    }
}
function groupingSendForTax() {
    try {
        var selectedRows = tblDoNotSend.rows('.selected').data();
        if (selectedRows.length > 0) {


            var Requests = [];

            for (var i = 0; i < selectedRows.length; i++) {
                var myFieldData = selectedRows[i]['InvoiceId'];
                Requests.push(myFieldData);
            }
            $('#waiting').show();
            setButtonsDesible(true);

            if (Requests.length > 0) {
                var formdata = new FormData();
                formdata.append("idList", Requests);
                var xhr = new XMLHttpRequest();
                xhr.open('POST', PostSendGroupingInvoiceURL, true);
                xhr.send(formdata);
                xhr.onreadystatechange = function () {
                    if (xhr.readyState === 4 && xhr.status === 200) {
                        var response = JSON.parse(xhr.responseText);
                        if (response.success == true) {
                            setButtonsDesible(false);
                            $('#waiting').hide();
                            getAllInvoiceList();
                            clearLiveStatus(); // بستن کادر پیشرفت زنده پس از پایان کار
                            new PNotify({
                                title: 'ارسال موفق',
                                text: 'فاکتورهای انتخاب‌شده با موفقیت ارسال شدند.',
                                type: 'success',
                                hide: false, // پیام ثابت می‌ماند تا کاربر آن را ببندد
                                styling: 'bootstrap3',
                                closer: true, // Ensure the close button is enabled
                                sticker: true
                            });
                            OnchangeStat();
                        }
                        else {
                            alert(response.message);
                            console.log(response);
                            setButtonsDesible(false); $('#waiting').hide(); OnchangeStat();
                        }
                    }
                }

            } else {

                new PNotify({
                    title: 'ارتباط ناموق',
                    text: 'فاکتوری جهت ارسال انتخاب نشده است!',
                    //type: 'success',
                    delay: 2000,
                    styling: 'bootstrap3',
                    closer: true, // Ensure the close button is enabled
                    sticker: false // Disable the sticker if you want to ensure it does not
                });

                setButtonsDesible(false); $('#waiting').hide(); OnchangeStat();
            }
        } else {
            new PNotify({
                title: 'ارتباط ناموق',
                text: 'فاکتوری جهت ارسال انتخاب نشده است!',
                //type: 'success',
                delay: 2000,
                styling: 'bootstrap3',
                closer: true, // Ensure the close button is enabled
                sticker: false // Disable the sticker if you want to ensure it does not
            });
            setButtonsDesible(false); $('#waiting').hide(); OnchangeStat();
        }

    }
    catch (errgroupingSendForTax) {
        console.log("groupingSendForTax:" + errgroupingSendForTax);
        $('#waiting').hide();
        setButtonsDesible(false); $('#waiting').hide(); OnchangeStat();
    }
}
function groupingSendForTaxWithParameter() {
    try {
        var selectedRows = tblDoNotSend.rows('.selected').data();
        var selectedRows = tblDoNotSend.rows('.row_selected').data();

        if (selectedRows.length > 0) {
            var Requests = [];

            // Collect InvoiceId from selected rows
            for (var i = 0; i < selectedRows.length; i++) {
                var myFieldData = selectedRows[i]['InvoiceId'];
                Requests.push(myFieldData);
            }

            $('#waiting').show();
            setButtonsDesible(true);

            // Function to send requests in batches
            function sendBatch(batch) {
                $.post(SendGroupingInvoiceAsyncURL, { stringList: batch }, function (response) {
                    if (response.success) {
                        // Handle success response
                        getAllInvoiceList();
                        clearLiveStatus(); // بستن کادر پیشرفت زنده پس از پایان کار
                        new PNotify({
                            title: 'ارسال موفق',
                            text: 'فاکتورهای انتخاب‌شده با موفقیت ارسال شدند.',
                            type: 'success',
                            hide: false, // پیام ثابت می‌ماند تا کاربر آن را ببندد
                            styling: 'bootstrap3',
                            closer: true,
                            sticker: true
                        });
                    } else {
                        alert(response.message);
                        console.log(response);
                    }
                }).fail(function (xhr, status, error) {
                    console.error('Error sending batch:', error);
                }).always(function () {
                    setButtonsDesible(false);
                    $('#waiting').hide();
                });
            }

            // Send requests in batches of 500
            for (var i = 0; i < Requests.length; i += 1000) {
                var batch = Requests.slice(i, i + 1000);
                sendBatch(batch);
            }

        } else {
            alert("فاکتوری جهت ارسال انتخاب نشده است");
            setButtonsDesible(false);
            $('#waiting').hide();
            OnchangeStat();
        }
    } catch (errgroupingSendForTaxWithParameter) {
        setButtonsDesible(false);
        $('#waiting').hide();
        OnchangeStat();

        console.log("groupingSendForTaxWithParameter:" + errgroupingSendForTaxWithParameter);
        $('#waiting').hide();
        setButtonsDesible(false);
    }
}

//function groupingSendForTaxWithParameter() {
//    try {
//        var selectedRows = tblDoNotSend.rows('.selected').data();
//        var selectedRows = tblDoNotSend.rows('.row_selected').data();
//        if (selectedRows.length > 0) {
//            var Requests = [];

//            for (var i = 0; i < selectedRows.length; i++) {
//                var myFieldData = selectedRows[i]['InvoiceId'];
//                Requests.push(myFieldData);
//            }

//            $('#waiting').show();
//            setButtonsDesible(true);
//            if (Requests.length > 0) {
//                $.post(SendGroupingInvoiceAsyncURL,
//                    { stringList: Requests },
//                    function (response) {
//                        setButtonsDesible(false); $('#waiting').hide(); OnchangeStat();
//                        if (response.success == true) {
//                            setButtonsDesible(false);
//                            $('#waiting').hide();
//                            getAllInvoiceList();

//                            new PNotify({
//                                title: 'ارتباط موفق',
//                                delay: 2000,
//                                text: 'ثبت با موفقیت انجام شد!',
//                                type: 'success',
//                                styling: 'bootstrap3',
//                                closer: true, // Ensure the close button is enabled
//                                sticker: false // Disable the sticker if you want to ensure it does not
//                            });

//                        }
//                        else {
//                            setButtonsDesible(false);
//                            alert(response.message);
//                            console.log(response);
//                        }
//                    });
//            } else {

//                alert("فاکتوری جهت ارسال انتخاب نشده است"); setButtonsDesible(false); $('#waiting').hide(); OnchangeStat();
//            }
//        }
//        else { alert("فاکتوری جهت ارسال انتخاب نشده است"); setButtonsDesible(false); $('#waiting').hide(); OnchangeStat(); }
//    } catch (errgroupingSendForTaxWithParameter) {
//        setButtonsDesible(false); $('#waiting').hide(); OnchangeStat();

//        console.log("groupingSendForTaxWithParameter:" + errgroupingSendForTaxWithParameter);
//        $('#waiting').hide();
//        setButtonsDesible(false); $('#waiting').hide(); 
//    }
//}

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
                //var xhr = new XMLHttpRequest();
                //xhr.open('POST', PostSendGroupingInvoiceURL, true);
                //xhr.send(formdata);


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
    try {
        setButtonsDesible(true);
        var id = { refId: dataID };
        $.post(PostGetInvoiceStatusByRefrenceURL, id)
            .done(function (data) {
                if (data.success) {

                    alert(data.res.message);
                    console.log(data);
                    getAllInvoiceList();
                    OnchangeStat();
                    OnchangeStat();
                } else {
                    alert("error");
                    console.log(data);
                    setButtonsDesible(false); OnchangeStat();
                }
            })
            .fail(function (error) { setButtonsDesible(false); OnchangeStat(); })
    } catch (errSendFactorForTax) {
        console.log("SendFactorForTax:" + errSendFactorForTax);
        setButtonsDesible(false); OnchangeStat();
    }
}

function setButtonsDesible(value) {
    $('#btnSearch').prop('disabled', value);
    $('#btnSelectAll').prop('disabled', value);
    $('#btnSendGroupForTax').prop('disabled', value);
    //$('#btnSendGroupForCheck').prop('disabled', value);
    $('#btnSearchInq').prop('disabled', value);
    $('#btnExportExcel').prop('disabled', value);
    $('#btnExportExcelInq').prop('disabled', value);
}
