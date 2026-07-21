var MDL_EntityComment = (function () {
    var MDL_EC_Init = false;
    var MDL_EC_EntityID = null;
    var MDL_EC_PersonID = null;
    var MDL_EC_CallBackForUnSaveReq = null;
    var MDL_EC_tblCommentList;
    var MDL_EC_ExttblCommentList;
    var MDL_EC_Mode; var MDL_EC_Item;
    var MDL_EC_updateInputField = function (event) {
        var buttonText = event.target.textContent;
        document.getElementById("MDL_EC_Subject").value = buttonText;
    }
    var MDL_EntityComment_Init = function () {
        //$('#MDL_EntityComments').on('shown.bs.modal', function () {
        //    $(this).find('.modal-dialog').css({
        //        'max-width': '90%', // Adjust as needed
        //        'width': 'auto' // Allow auto width
        //    });
        //});
        if (MDL_EC_Init === false) {
            MDL_EC_Init = true;
            $("#MDL_EC_AddCommentBtn").on("click", MDL_EC_AddComment);
            var buttons = document.querySelectorAll(".MDL_EC_btnsForSubject");
            buttons.forEach(function (button) {
                button.addEventListener("click", MDL_EC_updateInputField);
            });

            try {
                MDL_EC_tblCommentList = $("#MDL_EC_tblCommentList").DataTable({
                    "language": tablePersianPacketlang,
                    "order": [],
                    "autoWidth": false,
                    "createdRow": function (row, data, dataIndex) {
                        if (data["FirstFiles"]) {
                            $(row).css("background-color", "#02a802");
                            $(row).css("color", "#000000");
                            //$(row).addClass("warning");
                        }
                    },
                    columns: [
                        {
                            data: "fCommentUniqueID", name: "fCommentUniqueID", title: "fCommentUniqueID",
                            visible: false, searchable: false
                        },
                        {
                            title: "ردیف",
                            "data": "Index",
                            name: "Index",
                            render: function (data, type, row, meta) {
                                return meta.row + meta.settings._iDisplayStart + 1;
                            }
                        }
                        ,
                        { data: "fSubject", name: "fSubject", title: "عنوان", sortable: false },
                        { data: "EventDate", name: "EventDate", title: "تاریخ موضوع", sortable: false },
                        { data: "CreatedDate", name: "CreatedDate", title: "زمان ثبت", sortable: false },

                        {
                            title: "عملیات",
                            searchable: false,
                            sortable: false,
                            defaultContent:
                                "<td class=\"nowrap\">" +
                                //"<a href=\"#\" class=\"btn btn-round btn-info btn-xs DownloadFile\"><i class=\"fa fa-download\"></i> دانلود </a>" +
                                "<a href=\"#\" class=\"btn btn-round btn-info btn-xs delete\"><i class=\"fa fa-trash\"></i>حذف </a>" +
                                "</td>"
                        },
                    ],
                    responsive: true
                });
                $('#MDL_EC_tblCommentList tbody').on('click', 'tr', function () {
                    var data = MDL_EC_tblCommentList.row(this).data();
                    if (data) {
                        
                        $.post(MDL_APIListAddress_GetEntityCommentByIDAsyncURL,
                            { ID: data.fCommentUniqueID },
                            function (data) {
                                if (data.success) {
                                    MDL_EC_Item = data.Data;
                                    $('#MDL_EC_Subject').val(MDL_EC_Item.fSubject);
                                    $('#MDL_EC_EventDate').val(MDL_EC_Item.EventDate);
                                    $('#MDL_EC_Comment').val(MDL_EC_Item.fComment);
                                }
                                else {
                                    new PNotify({
                                        title: 'عملیات ناموفق',
                                        text: data.responseText,
                                        styling: 'bootstrap3'
                                    });
                                }

                            });
                    }

                });
                $('#MDL_EC_tblCommentList').on('click', '.delete', function () {
                    var current_row = $(this).parents('tr');
                    if (current_row.hasClass('child')) {
                        current_row = current_row.prev();
                    }
                    var data = MDL_EC_tblCommentList.row(current_row).data();
                    var dataID = data["fCommentUniqueID"];
                    $.post(MDL_APIListAddress_DeleteEntityCommentByIDAsyncURL,
                        { ID: dataID },
                        function (data) {
                            if (data.success) {
                                MDL_EC_Items = data.Data;

                                MDL_EC_tblCommentList.clear();
                                if (MDL_EC_Items)
                                    MDL_EC_tblCommentList.rows.add(MDL_EC_Items);
                                MDL_EC_tblCommentList.draw();
                            }
                            else {
                                new PNotify({
                                    title: 'عملیات ناموفق',
                                    text: data.responseText,
                                    styling: 'bootstrap3'
                                });
                            }

                        });
                });

                $("#MDL_EC_tblCommentList").on("click", ".DownloadFile", function () {


                    var current_row = $(this).parents('tr');
                    if (current_row.hasClass('child')) {
                        current_row = current_row.prev();
                    }
                    var data = MDL_EC_tblCommentList.row(current_row).data();
                    var dataID = data["fCommentUniqueID"];
                    var currentComment = MDL_EC_Items.find(item =>
                        item.fCommentUniqueID === dataID);
                    if (currentComment && currentComment.FirstFiles)
                        window.open("/FilesData/DownloadAttachment?ID=" + currentComment.FirstFiles.fFileDataUniqueID);
                    else {
                        new PNotify({
                            title: 'عملیات ناموفق',
                            text: 'این کامنت دارای فایل نمی باشد!',
                            styling: 'bootstrap3'
                        });
                    }

                });
            } catch (e) {
                console.log(e);
            }
        }
    }

    var MDL_EC_AddComment = function () {

        {
            //var input = document.getElementById("MDL_EC_attachInput");
            //if (input.files.length !== 0) {
            //    MDL_EntityComment_AttachFile(input);

            //}
            //else
            {
                MDL_EntityComment_Save();
            }

        }
    }

    var MDL_EntityComment_Save = function () {

        
        if ($("#MDL_EC_Comment").val() === "" || $("#MDL_EC_Subject").val() === "") {
            new PNotify({
                title: 'هشدار',
                text: "اظلاعات ضروری را وارد نمایید!",
                styling: 'bootstrap3'
            });
            return;
        }

        MDL_EC_Item.EventDate = $("#MDL_EC_EventDate").val();
        MDL_EC_Item.fComment = $("#MDL_EC_Comment").val();
        MDL_EC_Item.fSubject = $("#MDL_EC_Subject").val();
        MDL_EC_Item.fEntityUniqueID = MDL_EC_EntityID;

        $.post(MDL_APIListAddress_AddEntityCommentAsyncURL,
            { comment: MDL_EC_Item }
            , function (pData) {
                if (pData.success) {
                    MDL_EC_Item = {};
                    $('#MDL_EC_frm').trigger("reset");
                    MDL_EC_Items = pData.Data;

                    MDL_EC_tblCommentList.clear();
                    if (MDL_EC_Items)
                        MDL_EC_tblCommentList.rows.add(pData.Data);
                    MDL_EC_tblCommentList.draw();

                }
            });
    }
    var MDL_EntityComment_HideDialouge = function () {
        $("#MDL_EntityComments").modal('hide');
    }
    var MDL_EntityComment_ResetForm = function () {
        MDL_EntityComment_Init();
        MDL_EC_ExttblCommentList = null;;
        MDL_EC_Item = {};
        MDL_EC_Mode = '';
        MDL_EC_Items = [];
        $('#MDL_EC_frm').trigger("reset");
        $('#MDL_EC_QuickButtonDiv').hide();
        $('#MDL_EC_QuickButtonPersonlyDiv').hide();
        MDL_EC_tblCommentList.clear().draw(false);
        MDL_EC_EntityID = null;
        MDL_EC_PersonID = null;
        CallBackForUnSaveReq = null;
    }
    var MDL_EntityComment_ShowAdminGeneral = function (EntityID, ExttblCommentList) {
        MDL_EntityComment_ResetForm();
        MDL_EC_PersonID = false;
        MDL_EC_EntityID = EntityID;
        MDL_EC_ExttblCommentList = ExttblCommentList;
        MDL_EntityComment_LoadComments();
    }
    var MDL_EntityComment_AttachFile = function (fileInput) {

        var formdata = new FormData();
        for (i = 0; i < fileInput.files.length; i++) {
            formdata.append(fileInput.files[i].name, fileInput.files[i]);
            formdata.append("fSubject", $("#MDL_EC_Subject").val());
            formdata.append("fVersion", 1);
            formdata.append("fFileTypeUniqeID", 'D9EE1478-274D-49EA-8CD8-639C7B9B3AE2');
        }
        var xhr = new XMLHttpRequest();
        xhr.open('POST', '/FilesData/UploadNewComments', true);
        xhr.send(formdata);
        xhr.onreadystatechange = function () {
            if (xhr.readyState === 4 && xhr.status === 200) {
                var response = JSON.parse(xhr.responseText);
                if (response.Message !== "Error") {
                    {
                        MDL_EntityComment_Save();
                        $('#MDL_EC_frm').trigger("reset");

                    }
                }
                else {
                    alert("اشکال در آپلود فایل");

                }
            }

            return "UnExpected Error";
        };
    }
    var MDL_EntityComment_ShowAdminPersonly = function (EntityID, ExttblCommentList) {
        MDL_EntityComment_ResetForm();
        MDL_EC_PersonID = false;
        MDL_EC_EntityID = EntityID;
        MDL_EC_ExttblCommentList = ExttblCommentList;

        MDL_EntityComment_LoadComments();
    }
    var MDL_EntityComment_ShowAdmin = function () {
        MDL_EntityComment_ResetForm();
        MDL_EC_PersonID = false;
        //MDL_EC_EntityID = EntityID;
        //MDL_EC_ExttblCommentList = ExttblCommentList;

        MDL_EntityComment_LoadComments();
    }
    var MDL_EntityComment_LoadComments = function () {
        $.post(MDL_APIListAddress_GetEntityCommentByUserIDAsyncURL,
            /* { ID: MDL_EC_EntityID, PersonID: MDL_EC_PersonID }*/
            function (pData) {
                if (pData.success) {
                    MDL_EC_Items = pData.Data;

                    MDL_EC_tblCommentList.clear();
                    if (pData.Data)
                        MDL_EC_tblCommentList.rows.add(pData.Data);
                    MDL_EC_tblCommentList.draw();
                    $("#MDL_EntityComments").modal({
                        backdrop: 'static',
                        keyboard: false,
                        show: true
                    });
                }
            });
    }
    var MDL_EntityComment_ShowLimit = function (EntityID) {
        MDL_EntityComment_ResetForm();
        MDL_EC_EntityID = EntityID;
        MDL_EC_PersonID = true;
        MDL_EntityComment_LoadComments();

    }
    var MDL_EntityComment_AddCommentsForUnSaveReq = function (CallBackForUnSaveReq) {
        MDL_EntityComment_ResetForm();

        MDL_EC_Mode = "UnSave";
        MDL_EC_CallBackForUnSaveReq = CallBackForUnSaveReq;
        $("#MDL_EntityComments").modal({
            backdrop: 'static',
            keyboard: false,
            show: true
        });

    }

    return {
        MDL_EntityComment_ShowLimit: MDL_EntityComment_ShowLimit,
        MDL_EntityComment_ShowAdmin: MDL_EntityComment_ShowAdmin,
        MDL_EntityComment_ShowAdminPersonly: MDL_EntityComment_ShowAdminPersonly,
        MDL_EntityComment_ShowAdminGeneral: MDL_EntityComment_ShowAdminGeneral,
        MDL_EntityComment_AddCommentsForUnSaveReq: MDL_EntityComment_AddCommentsForUnSaveReq,
    };
})();
