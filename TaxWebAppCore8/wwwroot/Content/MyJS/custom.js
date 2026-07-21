const event_HMSM_NewMessage = new Event('event_HMSM_NewMessage');
const event_CVFGR_NewVipReq = new Event('event_CVFGR_NewVipReq');
const event_ConfirmGuestReuqest = new Event('event_ConfirmGuestReuqest');
var Mobileregex = /^09\d{9}$/;
//لیست ماژول‌های سامانه برای اعلام حضور در صورت وجود
/// ماژول پارشیال امضا
var MDLSignPartial = false;
//ماژول ایجاد درخواست خودرو غیر پیمانکاران
var MDLCarRequestNoneContractorsPartial = false;
var ShowblockUI = false;
var mobileRegex = /^09\d{9}$/; // Regex pattern for Iranian mobile numbers

if (typeof (Dropzone) !== 'undefined') {
    Dropzone.autoDiscover = false;

}

//$(document).on("select2:open", () => {
//    document.querySelector(".select2-container--open .select2-search__field").focus()
//});

function SearchBoxOffAutoFill() {
    let el = $('.dataTables_filter label');
    if (!el.parent('form').length) {
        el.each(function (index) {
            $(this).wrapAll('<form></form>').parent()
                .attr('autocomplete', false)
                .attr('autofill', false);
        });

    }
}
function openTab(url) {
    try {
        const link = document.createElement('a');
        link.href = url;
        link.target = '_blank';
        document.body.appendChild(link);
        link.click();
        link.remove();
    } catch (openTabErr) {

        console.log('Error in openTab : ' + openTabErr);

    }

}
function isNumberKey(evt) {
    var charCode = (evt.which) ? evt.which : evt.keyCode;
    if (
        charCode != 46
        &&
        charCode > 31
        &&
        ((charCode < 48 || charCode > 57)
            &&
            (charCode < 1776 || charCode > 1786)
        )
    )
        return false;
    return true;
}

var tablePersianPacketlang = {

    "sEmptyTable": "هیچ داده ای در جدول وجود ندارد",
    "sInfo": "نمایش _START_ تا _END_ از _TOTAL_ رکورد",
    "sInfoEmpty": "نمایش 0 تا 0 از 0 رکورد",
    "sInfoFiltered": "(فیلتر شده از _MAX_ رکورد)",
    "sInfoPostFix": "",
    "sInfoThousands": ",",
    "sLengthMenu": "نمایش _MENU_ رکورد",
    "sLoadingRecords": "در حال بارگزاری...",

    "sProcessing": "در حال پردازش...",
    "sSearch": "جستجو:",
    "sZeroRecords": "رکوردی با این مشخصات پیدا نشد",
    "oPaginate": {
        "sFirst": "ابتدا",
        "sLast": "انتها",
        "sNext": "بعدی",
        "sPrevious": "قبلی"
    },
    "oAria": {
        "sSortAscending": ": فعال سازی نمایش به صورت صعودی",
        "sSortDescending": ": فعال سازی نمایش به صورت نزولی"
    },
    "select": {
        "rows": {
            _: " %d سطر انتخاب شده",
            0: " برای انتخاب یک سطر بر روی آن کلیک نمایید",
        }
    }
};


var select2Lang = {

    errorLoading: function () { return "امکان بارگذاری نتایج وجود ندارد." },
    inputTooLong: function (n) { return "لطفاً " + (n.input.length - n.maximum) + " کاراکتر را حذف نمایید" },
    inputTooShort: function (n) { return "لطفاً تعداد " + (n.minimum - n.input.length) + " کاراکتر یا بیشتر وارد نمایید" },
    loadingMore: function () { return "در حال بارگذاری نتایج بیشتر..." },
    maximumSelected: function (n) { return "شما تنها می‌توانید " + n.maximum + " آیتم را انتخاب نمایید" },
    noResults: function () { return "هیچ نتیجه‌ای یافت نشد" },
    searching: function () { return "در حال جستجو..." },
    removeAllItems: function () { return "همه موارد را حذف کنید" }
};



var LoadingLogoOnly = { css: { backgroundColor: '#20adfe', color: '#fff', 'z-index': '9999999' }, message: '<img src="/Content/Images/LogoLoading.gif" height="80" width="80"/>' };
var LoadingWithText = { css: { backgroundColor: '#20adfe', color: '#fff', 'z-index': '9999999' }, message: '<h1> <img src="/Content/Images/LogoLoading.gif" height="80" width="80"/> در حال بررسی</h1>' };

$(document).ajaxStop(function () {
    //if (ShowblockUI) {
   /* $.unblockUI();*/
    ShowblockUI = false;
    //}
    //$("#spinner").hide();
});

$(document).ajaxStart(function () {
    if (ShowblockUI) {
        $.blockUI(LoadingLogoOnly);
    }


});


$(document).ajaxError(function (event, request, settings) {
    //if (request.status == 0) {
    //    alert(localizeStrings("server_disconnected"));
    //}
    if (ShowblockUI) {
        $.unblockUI();
        ShowblockUI = false;
    }


});
(function ($, sr) {
    // debouncing function from John Hann
    // http://unscriptable.com/index.php/2009/03/20/debouncing-javascript-methods/
    var debounce = function (func, threshold, execAsap) {
        var timeout;

        return function debounced() {
            var obj = this, args = arguments;
            function delayed() {
                if (!execAsap)
                    func.apply(obj, args);
                timeout = null;
            }

            if (timeout)
                clearTimeout(timeout);
            else if (execAsap)
                func.apply(obj, args);

            timeout = setTimeout(delayed, threshold || 100);
        };
    };

    // smartresize 
    jQuery.fn[sr] = function (fn) { return fn ? this.bind('resize', debounce(fn)) : this.trigger(sr); };

})(jQuery, 'smartresize');
/**
 * Resize function without multiple trigger
 *
 * Usage:
 * $(window).smartresize(function(){
 *     // code here
 * });
 */
(function ($, sr) {
    // debouncing function from John Hann
    // http://unscriptable.com/index.php/2009/03/20/debouncing-javascript-methods/
    var debounce = function (func, threshold, execAsap) {
        var timeout;

        return function debounced() {
            var obj = this, args = arguments;

            function delayed() {
                if (!execAsap)
                    func.apply(obj, args);
                timeout = null;
            }

            if (timeout)
                clearTimeout(timeout);
            else if (execAsap)
                func.apply(obj, args);

            timeout = setTimeout(delayed, threshold || 100);
        };
    };

    // smartresize
    jQuery.fn[sr] = function (fn) {
        return fn ? this.bind('resize', debounce(fn)) : this.trigger(sr);
    };

})(jQuery, 'smartresize');
/**
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */

var CURRENT_URL = window.location.href.split('#')[0].split('?')[0],
    $BODY = $('body'),
    $MENU_TOGGLE = $('#menu_toggle'),
    $SIDEBAR_MENU = $('#sidebar-menu'),
    $SIDEBAR_FOOTER = $('.sidebar-footer'),
    $LEFT_COL = $('.left_col'),
    $RIGHT_COL = $('.right_col'),
    $NAV_MENU = $('.nav_menu'),
    $FOOTER = $('footer');


// Sidebar
function init_sidebar() {
    // TODO: This is some kind of easy fix, maybe we can improve this
    var setContentHeight = function () {
        // reset height
        $RIGHT_COL.css('min-height', $(window).height());

        var bodyHeight = $BODY.outerHeight(),
            footerHeight = $BODY.hasClass('footer_fixed') ? -10 : $FOOTER.height(),
            leftColHeight = $LEFT_COL.eq(1).height() + $SIDEBAR_FOOTER.height(),
            contentHeight = bodyHeight < leftColHeight ? leftColHeight : bodyHeight;

        // normalize content
        contentHeight -= $NAV_MENU.height() + footerHeight;

        $RIGHT_COL.css('min-height', contentHeight);
    };

    $SIDEBAR_MENU.find('a').on('click', function (ev) {
        console.log('clicked - sidebar_menu');
        var $li = $(this).parent();

        if ($li.is('.active')) {
            $li.removeClass('active active-sm');
            $('ul:first', $li).slideUp(function () {
                setContentHeight();
            });
        } else {
            // prevent closing menu if we are on child menu
            if (!$li.parent().is('.child_menu')) {
                $SIDEBAR_MENU.find('li').removeClass('active active-sm');
                $SIDEBAR_MENU.find('li ul').slideUp();
            } else {
                if ($BODY.is(".nav-sm")) {
                    $li.parent().find("li").removeClass("active active-sm");
                    $li.parent().find("li ul").slideUp();
                }
            }
            $li.addClass('active');

            $('ul:first', $li).slideDown(function () {
                setContentHeight();
            });
        }
    });

    // toggle small or large menu
    $MENU_TOGGLE.on('click', function () {
        console.log('clicked - menu toggle');

        if ($BODY.hasClass('nav-md')) {
            $SIDEBAR_MENU.find('li.active ul').hide();
            $SIDEBAR_MENU.find('li.active').addClass('active-sm').removeClass('active');
        } else {
            $SIDEBAR_MENU.find('li.active-sm ul').show();
            $SIDEBAR_MENU.find('li.active-sm').addClass('active').removeClass('active-sm');
        }

        $BODY.toggleClass('nav-md nav-sm');

        setContentHeight();
    });

    // check active menu
    $SIDEBAR_MENU.find('a[href="' + CURRENT_URL + '"]').parent('li').addClass('current-page');

    $SIDEBAR_MENU.find('a').filter(function () {
        return this.href == CURRENT_URL;
    }).parent('li').addClass('current-page').parents('ul').slideDown(function () {
        setContentHeight();
    }).parent().addClass('active');

    // recompute content when resizing
    $(window).smartresize(function () {
        setContentHeight();
    });

    setContentHeight();

    // fixed sidebar
    if ($.fn.mCustomScrollbar) {
        $('.menu_fixed').mCustomScrollbar({
            autoHideScrollbar: true,
            theme: 'minimal',
            mouseWheel: { preventDefault: true }
        });
    }
};
// /Sidebar

var randNum = function () {
    return (Math.floor(Math.random() * (1 + 40 - 20))) + 20;
};


// Panel toolbox
//$(document).ready(function () {
//    $('.collapse-link').on('click', function () {
//        var $BOX_PANEL = $(this).closest('.x_panel'),
//            $ICON = $(this).find('i'),
//            $BOX_CONTENT = $BOX_PANEL.find('.x_content');

//        // fix for some div with hardcoded fix class
//        if ($BOX_PANEL.attr('style')) {
//            $BOX_CONTENT.slideToggle(200, function () {
//                $BOX_PANEL.removeAttr('style');
//            });
//        } else {
//            $BOX_CONTENT.slideToggle(200);
//            $BOX_PANEL.css('height', 'auto');
//        }

//        $ICON.toggleClass('fa-chevron-up fa-chevron-down');
//    });

//    $('.close-link').click(function () {
//        var $BOX_PANEL = $(this).closest('.x_panel');

//        $BOX_PANEL.remove();
//    });
//});
// /Panel toolbox

// Tooltip
//$(document).ready(function () {
//    $('[data-toggle="tooltip"]').tooltip({
//        container: 'body'
//    });
//});
// /Tooltip

// Progressbar
if ($(".progress .progress-bar")[0]) {
    $('.progress .progress-bar').progressbar();
}
// /Progressbar

// Switchery
//$(document).ready(function () {
//    if ($(".js-switch")[0]) {
//        var elems = Array.prototype.slice.call(document.querySelectorAll('.js-switch'));
//        elems.forEach(function (html) {
//            var switchery = new Switchery(html, {
//                color: '#26B99A'
//            });
//        });
//    }
//});
// /Switchery


// iCheck
//$(document).ready(function () {
//    if ($("input.flat")[0]) {
//        $(document).ready(function () {
//            $('input.flat').iCheck({
//                checkboxClass: 'icheckbox_flat-green',
//                radioClass: 'iradio_flat-green'
//            });
//        });
//    }
//});
// /iCheck

// Table
$('table input').on('ifChecked', function () {
    checkState = '';
    $(this).parent().parent().parent().addClass('selected');
    countChecked();
});
$('table input').on('ifUnchecked', function () {
    checkState = '';
    $(this).parent().parent().parent().removeClass('selected');
    countChecked();
});

var checkState = '';

$('.bulk_action input').on('ifChecked', function () {
    checkState = '';
    $(this).parent().parent().parent().addClass('selected');
    countChecked();
});
$('.bulk_action input').on('ifUnchecked', function () {
    checkState = '';
    $(this).parent().parent().parent().removeClass('selected');
    countChecked();
});
$('.bulk_action input#check-all').on('ifChecked', function () {
    checkState = 'all';
    countChecked();
});
$('.bulk_action input#check-all').on('ifUnchecked', function () {
    checkState = 'none';
    countChecked();
});

function countChecked() {
    if (checkState === 'all') {
        $(".bulk_action input[name='table_records']").iCheck('check');
    }
    if (checkState === 'none') {
        $(".bulk_action input[name='table_records']").iCheck('uncheck');
    }

    var checkCount = $(".bulk_action input[name='table_records']:checked").length;

    if (checkCount) {
        $('.column-title').hide();
        $('.bulk-actions').show();
        $('.action-cnt').html(checkCount + ' انتخاب شده');
    } else {
        $('.column-title').show();
        $('.bulk-actions').hide();
    }
}


// Accordion
//$(document).ready(function () {
//    $(".expand").on("click", function () {
//        $(this).next().slideToggle(200);
//        $expand = $(this).find(">:first-child");

//        if ($expand.text() == "+") {
//            $expand.text("-");
//        } else {
//            $expand.text("+");
//        }
//    });
//});

// NProgress








//hover and retain popover when on popover content
var originalLeave = $.fn.popover.Constructor.prototype.leave;
$.fn.popover.Constructor.prototype.leave = function (obj) {
    var self = obj instanceof this.constructor ?
        obj : $(obj.currentTarget)[this.type](this.getDelegateOptions()).data('bs.' + this.type);
    var container, timeout;

    originalLeave.call(this, obj);

    if (obj.currentTarget) {
        container = $(obj.currentTarget).siblings('.popover');
        timeout = self.timeout;
        container.one('mouseenter', function () {
            //We entered the actual popover – call off the dogs
            clearTimeout(timeout);
            //Let's monitor popover content instead
            container.one('mouseleave', function () {
                $.fn.popover.Constructor.prototype.leave.call(self, self);
            });
        });
    }
};

$('body').popover({
    selector: '[data-popover]',
    trigger: 'click hover',
    delay: {
        show: 50,
        hide: 400
    }
});



/* AUTOSIZE */

function init_autosize() {

    if (typeof $.fn.autosize !== 'undefined') {

        autosize($('.resizable_textarea'));

    }

};



/* KNOB */

function init_knob() {

    if (typeof ($.fn.knob) === 'undefined') {
        return;
    }
    console.log('init_knob');

    $(".knob").knob({
        change: function (value) {
            //console.log("change : " + value);
        },
        release: function (value) {
            //console.log(this.$.attr('value'));
            console.log("release : " + value);
        },
        cancel: function () {
            console.log("cancel : ", this);
        },
        /*format : function (value) {
                   return value + '%';
                   },*/
        draw: function () {

            // "tron" case
            if (this.$.data('skin') == 'tron') {

                this.cursorExt = 0.3;

                var a = this.arc(this.cv) // Arc
                    ,
                    pa // Previous arc
                    , r = 1;

                this.g.lineWidth = this.lineWidth;

                if (this.o.displayPrevious) {
                    pa = this.arc(this.v);
                    this.g.beginPath();
                    this.g.strokeStyle = this.pColor;
                    this.g.arc(this.xy, this.xy, this.radius - this.lineWidth, pa.s, pa.e, pa.d);
                    this.g.stroke();
                }

                this.g.beginPath();
                this.g.strokeStyle = r ? this.o.fgColor : this.fgColor;
                this.g.arc(this.xy, this.xy, this.radius - this.lineWidth, a.s, a.e, a.d);
                this.g.stroke();

                this.g.lineWidth = 2;
                this.g.beginPath();
                this.g.strokeStyle = this.o.fgColor;
                this.g.arc(this.xy, this.xy, this.radius - this.lineWidth + 1 + this.lineWidth * 2 / 3, 0, 2 * Math.PI, false);
                this.g.stroke();

                return false;
            }
        }

    });

    // Example of infinite knob, iPod click wheel
    var v, up = 0,
        down = 0,
        i = 0,
        $idir = $("div.idir"),
        $ival = $("div.ival"),
        incr = function () {
            i++;
            $idir.show().html("+").fadeOut();
            $ival.html(i);
        },
        decr = function () {
            i--;
            $idir.show().html("-").fadeOut();
            $ival.html(i);
        };
    $("input.infinite").knob({
        min: 0,
        max: 20,
        stopper: false,
        change: function () {
            if (v > this.cv) {
                if (up) {
                    decr();
                    up = 0;
                } else {
                    up = 1;
                    down = 0;
                }
            } else {
                if (v < this.cv) {
                    if (down) {
                        incr();
                        down = 0;
                    } else {
                        down = 1;
                        up = 0;
                    }
                }
            }
            v = this.cv;
        }
    });

};

/* INPUT MASK */

function init_InputMask() {

    if (typeof ($.fn.inputmask) === 'undefined') {
        return;
    }
    console.log('init_InputMask');

    $(":input").inputmask();

};

/* COLOR PICKER */

function init_ColorPicker() {

    if (typeof ($.fn.colorpicker) === 'undefined') {
        return;
    }
    console.log('init_ColorPicker');

    $('.demo1').colorpicker();
    $('.demo2').colorpicker();

    $('#demo_forceformat').colorpicker({
        format: 'rgba',
        horizontal: true
    });

    $('#demo_forceformat3').colorpicker({
        format: 'rgba',
    });

    $('.demo-auto').colorpicker();

};


/* ION RANGE SLIDER */

function init_IonRangeSlider() {

    if (typeof ($.fn.ionRangeSlider) === 'undefined') {
        return;
    }
    console.log('init_IonRangeSlider');

    $("#range_27").ionRangeSlider({
        type: "double",
        min: 1000000,
        max: 2000000,
        grid: true,
        force_edges: true
    });
    $("#range").ionRangeSlider({
        hide_min_max: true,
        keyboard: true,
        min: 0,
        max: 5000,
        from: 1000,
        to: 4000,
        type: 'double',
        step: 1,
        prefix: "$",
        grid: true
    });
    $("#range_25").ionRangeSlider({
        type: "double",
        min: 1000000,
        max: 2000000,
        grid: true
    });
    $("#range_26").ionRangeSlider({
        type: "double",
        min: 0,
        max: 10000,
        step: 500,
        grid: true,
        grid_snap: true
    });
    $("#range_31").ionRangeSlider({
        type: "double",
        min: 0,
        max: 100,
        from: 30,
        to: 70,
        from_fixed: true
    });
    $(".range_min_max").ionRangeSlider({
        type: "double",
        min: 0,
        max: 100,
        from: 30,
        to: 70,
        max_interval: 50
    });
    $(".range_time24").ionRangeSlider({
        min: +moment().subtract(12, "hours").format("X"),
        max: +moment().format("X"),
        from: +moment().subtract(6, "hours").format("X"),
        grid: true,
        force_edges: true,
        prettify: function (num) {
            var m = moment(num, "X");
            return m.format("Do MMMM, HH:mm");
        }
    });

};


/* SMART WIZARD */

//function init_SmartWizard() {
//    // TODO: Fix buttons order
//    if (typeof ($.fn.smartWizard) === 'undefined') {
//        return;
//    }
//    console.log('init_SmartWizard');
//    $('#wizard').smartWizard({
//        labelNext: 'بعدی', // label for Next button
//        labelPrevious: 'قبلی', // label for Previous button
//        labelFinish: 'پایان',  // label for Finish button
//    });

//    $('#wizard_verticle').smartWizard({
//        transitionEffect: 'slide',
//        labelNext: 'بعدی', // label for Next button
//        labelPrevious: 'قبلی', // label for Previous button
//        labelFinish: 'پایان',  // label for Finish button,
//    });

//    $('.buttonNext').addClass('btn btn-success');
//    $('.buttonPrevious').addClass('btn btn-primary');
//    $('.buttonFinish').addClass('btn btn-default');

//};


/* VALIDATOR */
var isvalidatorinitialize = false;
function init_validator() {

    if (typeof (validator) === 'undefined') {
        return;
    }

    if (isvalidatorinitialize) {
        return;
    }
    console.log('init_validator');

    // initialize the validator function
    validator.message.invalid = 'ورودی مورد انتظار نیست';
    validator.message.short = 'ورودی کوتاه است';
    validator.message.long = 'ورودی بلند است';
    validator.message.checked = 'باید تایید شود';
    validator.message.empty = 'مقداردهی اجباری';
    validator.message.select = 'لطفا یک گزینه را انتخاب کنید';
    validator.message.number_min = 'خیلی کم';
    validator.message.number_max = 'خیلی زیاد';
    validator.message.url = 'URL نامعتبر';
    validator.message.number = 'عدد نمی‌باشد';
    validator.message.email = 'آدرس ایمیل نا معتبر است.';
    validator.message.email_repeat = 'تکرار ایمیل با ایمیل مطابقت ندارد';
    validator.message.date = 'تاریخ نامعتبر';
    validator.message.time = 'زمان نامعتبر';
    validator.message.password_repeat = 'تکرار رمزعبور با رمزعبور مطابقت ندارد';
    validator.message.no_match = 'مطابقت ندارد';
    validator.message.complete = 'ورودی کامل نیست';
    isvalidatorinitialize = true;

    // validate a field on "blur" event, a 'select' on 'change' event & a '.reuired' classed multifield on 'keyup':
    //$('form')
    //    .on('blur', 'input[required], input.optional, select.required', validator.checkField)
    //    .on('change', 'select.required', validator.checkField)
    //    .on('keypress', 'input[required][pattern]', validator.keypress);

    //$('.multi.required').on('keyup blur', 'input', function () {
    //    validator.checkField.apply($(this).siblings().last()[0]);
    //});

    //$('form').submit(function (e) {
    //    e.preventDefault();
    //    var submit = true;

    //    // evaluate the form using generic validaing
    //    if (!validator.checkAll($(this))) {
    //        submit = false;
    //    }

    //    if (submit)
    //        this.submit();

    //    return false;
    //});

};

/* PNotify */


/* CUSTOM NOTIFICATION */

function init_CustomNotification() {

    console.log('run_customtabs');
    CustomTabs = function (options) {
        $('.tabbed_notifications > div').hide();
        $('.tabbed_notifications > div:first-of-type').show();
        $('#custom_notifications').removeClass('dsp_none');
        $('.notifications a').click(function (e) {
            e.preventDefault();
            var $this = $(this),
                tabbed_notifications = '#' + $this.parents('.notifications').data('tabbed_notifications'),
                others = $this.closest('li').siblings().children('a'),
                target = $this.attr('href');
            others.removeClass('active');
            $this.addClass('active');
            $(tabbed_notifications).children('div').hide();
            $(target).show();
        });
    };
    if (typeof (CustomTabs) === 'undefined') {
        return;
    }
    console.log('init_CustomTabs');

    var cnt = 10;

    TabbedNotification = function (options) {
        var message = "<div id='ntf" + cnt + "' class='text alert-" + options.type + "' style='display:none'><h2><i class='fa fa-bell'></i> " + options.title +
            "</h2><div class='close'><a href='javascript:;' class='notification_close'><i class='fa fa-close'></i></a></div><p>" + options.text + "</p></div>";

        if (!document.getElementById('custom_notifications')) {
            alert('doesnt exists');
        } else {
            $('#custom_notifications ul.notifications').append("<li><a id='ntlink" + cnt + "' class='alert-" + options.type + "' href='#ntf" + cnt + "'><i class='fa fa-bell animated shake'></i></a></li>");
            $('#custom_notifications #notif-group').append(message);
            cnt++;
            CustomTabs(options);
        }
    };

    CustomTabs();

    var tabid = idname = '';

    $(document).on('click', '.notification_close', function (e) {
        idname = $(this).parent().parent().attr("id");
        tabid = idname.substr(-2);
        $('#ntf' + tabid).remove();
        $('#ntlink' + tabid).parent().remove();
        $('.notifications a').first().addClass('active');
        $('#notif-group div').first().css('display', 'block');
    });

};



/* COMPOSE */

function init_compose() {

    if (typeof ($.fn.slideToggle) === 'undefined') {
        return;
    }
    // console.log('init_compose');

    $('#compose, .compose-close').click(function () {
        $('.compose').slideToggle();
    });

};


function toggleFullScreen() {
    if (!document.fullscreenElement &&    // alternative standard method
        !document.mozFullScreenElement && !document.webkitFullscreenElement && !document.msFullscreenElement) {  // current working methods
        if (document.documentElement.requestFullscreen) {
            document.documentElement.requestFullscreen();
        } else if (document.documentElement.msRequestFullscreen) {
            document.documentElement.msRequestFullscreen();
        } else if (document.documentElement.mozRequestFullScreen) {
            document.documentElement.mozRequestFullScreen();
        } else if (document.documentElement.webkitRequestFullscreen) {
            document.documentElement.webkitRequestFullscreen(Element.ALLOW_KEYBOARD_INPUT);
        }
    } else {
        if (document.exitFullscreen) {
            document.exitFullscreen();
        } else if (document.msExitFullscreen) {
            document.msExitFullscreen();
        } else if (document.mozCancelFullScreen) {
            document.mozCancelFullScreen();
        } else if (document.webkitExitFullscreen) {
            document.webkitExitFullscreen();
        }
    }
}
var IsPageLocked = false;
window.onbeforeunload = function () {
    if (IsPageLocked) {

        logout();
        IsPageLocked = false;
    }
}

function lock_screen() {
    if (typeof (persianDate) !== 'undefined') {
        var currentTimeString = "<h1>" + new persianDate().format('LLLL') + "</h1>";
        $("#lock_screen .clock").html(currentTimeString);
    }
    setInterval(function () {
        if (typeof (persianDate) !== 'undefined') {
            var currentTimeString = "<h1>" + new persianDate().format('LLLL') + "</h1>";
            $("#lock_screen .clock").html(currentTimeString);
        }

    }, 30000);
    $(document).on('click', '.lock_btn', function (e) {
        e.preventDefault();
        IsPageLocked = true;
        $('body').addClass('lock');
    });
    $(document).on('mouseover', '.unlock #icon_lock', function (e) {
        $(this).removeClass('fa-lock').addClass('fa-unlock');
    });
    $(document).on('mouseout', '.unlock #icon_lock', function (e) {
        $(this).removeClass('fa-unlock').addClass('fa-lock');
    });
    $(document).on('click', '.unlock', function (e) {
        e.preventDefault();
      //  ASP_AskPassword();
        //$('body').removeClass('lock');

    });
}
$(window).on('load', function () {
    if (typeof NProgress != 'undefined') {
        NProgress.done();
    }
});
$(document).ready(function () {
    if (typeof NProgress != 'undefined') {
        NProgress.start();
    }


    init_sidebar();

    init_InputMask();

    init_validator();

    init_compose();

    init_autosize();

    lock_screen();
    AccordionInitialize();
    PanelToolBoxInitialize();
    datatoggleInitialize();
    SwitcheryInitialize();
    iCheckInitialize();
    //if (Push && Push.Permission)
    //    if (Push.Permission.has()) {
    //        //Push.create("سلام", {
    //        //    body: "به سامانه جامع اتوماسیون پترو پایش فیدار خوش آمدید",
    //        //    icon: '/Content/Images/GALogoIcon.png',
    //        //    timeout: 6000,
    //        //    onClick: function () {
    //        //        window.focus();
    //        //        this.close();
    //        //    }
    //        //});
    //    }
    //    else {
    //        Push.Permission.request(onGranted, onDenied);
    //    }

    if (typeof (CUP_MainInitialize) !== 'undefined') {
        CUP_MainInitialize();
    }
    //AskPass_Initialize();
    //if (typeof (initEnter) !== 'undefined') {
    //    initEnter();
    //}

    //$('#menu_toggle').trigger('click');
    //init_InputMask();
});

function onGranted() {
    Push.create("سلام", {
        body: "به سامانه جامع اتوماسیون پترو پایش فیدار خوش آمدید",
        icon: '/Content/Images/GALogoIcon.png',
        timeout: 6000,
        onClick: function () {
            window.focus();
            this.close();
        }
    });
}
function onDenied() {

}

function AccordionInitialize() {
    try {
        $(".expand").on("click", function () {
            $(this).next().slideToggle(200);
            $expand = $(this).find(">:first-child");

            if ($expand.text() == "+") {
                $expand.text("-");
            } else {
                $expand.text("+");
            }
        });
    } catch (AccordionInitializeErr) {
        console.log('Error in AccordionInitialize : ' + AccordionInitializeErr);
    }
}




function PanelToolBoxInitialize() {
    try {

        $('.collapse-link').on('click', function () {
            var $BOX_PANEL = $(this).closest('.x_panel'),
                $ICON = $(this).find('i'),
                $BOX_CONTENT = $BOX_PANEL.find('.x_content');

            // fix for some div with hardcoded fix class
            if ($BOX_PANEL.attr('style')) {
                $BOX_CONTENT.slideToggle(200, function () {
                    $BOX_PANEL.removeAttr('style');
                });
            } else {
                $BOX_CONTENT.slideToggle(200);
                $BOX_PANEL.css('height', 'auto');
            }

            $ICON.toggleClass('fa-chevron-up fa-chevron-down');
        });

        $('.close-link').click(function () {
            var $BOX_PANEL = $(this).closest('.x_panel');

            $BOX_PANEL.remove();
        });
    } catch (PanelToolBoxInitializeErr) {
        console.log('Error in PanelToolBoxInitialize : ' + PanelToolBoxInitializeErr);
    }
}


function datatoggleInitialize() {
    try {

        $('[data-toggle="tooltip"]').tooltip({
            container: 'body'
        });
    } catch (datatoggleInitializeErr) {
        console.log('Error in datatoggleInitialize : ' + datatoggleInitializeErr);
    }
}


function SwitcheryInitialize() {
    try {
        if ($(".js-switch")[0]) {
            var elems = Array.prototype.slice.call(document.querySelectorAll('.js-switch'));
            elems.forEach(function (html) {
                var switchery = new Switchery(html, {
                    color: '#26B99A'
                });
            });
        }
    } catch (SwitcheryInitializeErr) {
        console.log('Error in SwitcheryInitialize : ' + SwitcheryInitializeErr);
    }
}

function iCheckInitialize() {
    try {
        if ($("input.flat")[0]) {
            //$(document).ready(function () {
            $('input.flat').iCheck({
                checkboxClass: 'icheckbox_flat-green',
                radioClass: 'iradio_flat-green'
            });
            //});
        }
    } catch (iCheckInitializeErr) {
        console.log('Error in iCheckInitialize : ' + iCheckInitializeErr);
    }
}


//function AskPass_Initialize() {
//    try {
//        $('#btn_ASP_Check').on('click', ASP_CheckPassOnServer);
//        $('#btn_ASP_Cancel').on('click', function () {
//            $("#ASP_AskForPassModal").modal('hide');
//        });
//        var passInput = document.getElementById("txt_ASP_Password");
//        passInput.addEventListener("keyup", function (event) {
//            if (event.keyCode === 13) {
//                ASP_CheckPassOnServer();
//            }
//        });
//    } catch (AskPass_InitializeErr) {
//        console.log('Error in AskPass_Initialize : ' + AskPass_InitializeErr);
//    }

//}
//function ASP_AskPassword() {
//    try {
//        $("#txt_ASP_Password").val('');
//        $("#ASP_AskForPassModal").modal();


//    } catch (ASP_AskPasswordErr) {
//        console.log('Error in ASP_AskPassword : ' + ASP_AskPasswordErr);
//    }

//}


function ASP_CheckPassOnServer() {
    try {
        var PassVal = $("#txt_ASP_Password").val();
        if (PassVal) {
            $.post('/Home/CheckPasswordForIdentity', { pass: PassVal }, function (retData) {
                if (retData.success) {
                    $("#ASP_AskForPassModal").modal('hide');
                    $('body').removeClass('lock');
                    IsPageLocked = false;
                }
                else {
                    new PNotify({
                        title: 'هشدار',
                        text: retData.responseText,
                        styling: 'bootstrap3'
                    });
                }
            });
        }
    } catch (ASP_CheckPassOnServerErr) {
        console.log('Error in ASP_CheckPassOnServer : ' + ASP_CheckPassOnServerErr);
    }

    //تابع بررسی صحت کدملی 

}
function CheckNationalCode(input) {
    if (!/^\d{10}$/.test(input)
        || input == '0000000000'
        || input == '1111111111'
        || input == '2222222222'
        || input == '3333333333'
        || input == '4444444444'
        || input == '5555555555'
        || input == '6666666666'
        || input == '7777777777'
        || input == '8888888888'
        || input == '9999999999')
        return false;
    var check = parseInt(input[9]);
    var sum = 0;
    var i;
    for (i = 0; i < 9; ++i) {
        sum += parseInt(input[i]) * (10 - i);
    }
    sum %= 11;
    return (sum < 2 && check == sum) || (sum >= 2 && check + sum == 11);
}
//async function CheckPersonNationalCodeReturnPersonDTO(nationalCode) {
//    if (nationalCode === '') return null;
//    var CheckPersonWithNationalCodeURL = "/General/CheckPersonWithNationalCode";

//    await $.post(CheckPersonWithNationalCodeURL,
//        { nationalCode: nationalCode },
//        function (data) {
//            if (data.success) {
//                return data.Data;
//            }
//            return null;

//        }).fail(function (response) {
//            new PNotify({
//                title: 'هشدار',
//                text: response.responseText,
//                styling: 'bootstrap3'
//            });
//            return null;
//        });

//}
function CheckPersonNationalCodeReturnPersonDTO(nationalCode) {
    return new Promise(function (resolve, reject) {
        if (nationalCode === '') {
            resolve(null);
        } else {
            var CheckPersonWithNationalCodeURL = "/General/CheckPersonWithNationalCode";

            $.post(CheckPersonWithNationalCodeURL, { nationalCode: nationalCode })
                .done(function (data) {
                    if (data.success) {
                        resolve(data.Data);
                    } else {
                        resolve(null);
                    }
                })
                .fail(function (response) {
                    new PNotify({
                        title: 'هشدار',
                        text: response.responseText,
                        styling: 'bootstrap3'
                    });
                    resolve(null);
                });
        }
    });
}





String.prototype.getBaseConversionNumber = function (label) {
    const faDigits = ['۱', '۲', '۳', '۴', '۵', '۶', '۷', '۸', '۹', '۰'];
    const enDigits = ['1', '2', '3', '4', '5', '6', '7', '8', '9', '0'];
    const arDigits = ['٠', '٩', '٨', '٧', '٦', '٥', '٤', '٣', '٢', '١']

    var whichDigit = {};

    switch (label) {
        case 'fa':
            whichDigit[label] = faDigits;
            break;
        case 'en':
            whichDigit[label] = enDigits;
            break;
        case 'ar':
            whichDigit[label] = arDigits;
            break;
        case 'all':
            whichDigit = { "fa": faDigits, "en": enDigits, "ar": arDigits };
            break;
        default:
            whichDigit = [];
    }

    return whichDigit;
}


String.prototype.CvnFromTo = function (fromDigits, toDigits, str) {
    var str = str == undefined ? this : str;
    for (var i = 0; i < toDigits.length; i++) {
        const currentFromDigit = fromDigits[i];
        const currentToDigit = toDigits[i];
        const regex = new RegExp(currentFromDigit, 'g');
        str = str.replace(regex, currentToDigit);
    }
    return str;
}

String.prototype.convertDigits = function (to) {
    let str = this;
    const toCvn = (this.getBaseConversionNumber(to))[to];
    const allDigits = this.getBaseConversionNumber("all");

    delete allDigits[to];

    const Objkeys = Object.keys(allDigits);
    for (var i = 0; i < Objkeys.length; i++) {
        const currentKey = Objkeys[i];
        const fromCvn = allDigits[currentKey];
        str = this.CvnFromTo(fromCvn, toCvn, str)
    }
    return str;
}

function CheckNumberKeyDown(e) {
    var key = e.charCode || e.keyCode || 0;
    // allow backspace, tab, delete, enter, arrows, numbers and keypad numbers ONLY
    // home, end, period, and numpad decimal
    return (
        key == 8 ||
        key == 9 ||
        key == 13 ||
        key == 46 ||
        key == 110 ||
        key == 190 ||
        (key >= 35 && key <= 40) ||
        (key >= 48 && key <= 57) ||
        (key >= 96 && key <= 105));
}
jQuery.fn.ForceNumericOnly =
    function () {
        return this.each(function () {
            $(this).keydown(function (e) {
                var key = e.charCode || e.keyCode || 0;
                // allow backspace, tab, delete, enter, arrows, numbers and keypad numbers ONLY
                // home, end, period, and numpad decimal
                return (
                    key == 8 ||
                    key == 9 ||
                    key == 13 ||
                    key == 46 ||
                    key == 110 ||
                    key == 190 ||
                    (key >= 35 && key <= 40) ||
                    (key >= 48 && key <= 57) ||
                    (key >= 96 && key <= 105));
            });
        });
    };