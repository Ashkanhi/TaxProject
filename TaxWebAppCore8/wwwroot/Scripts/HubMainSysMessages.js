
//const event_HMSM_NewIncomingLog = new CustomEvent('event_HMSM_NewIncomingLog', { detail: "LogUniqID" });
var NPSHubConnectionID = '';
(function ($) {
    $.ajax({
        url: "/signalr/hubs",
        dataType: "script",
        async: false
    });
}(jQuery));
$(document).ready(function () {

    $.connection.hub.url = "/signalr/hubs";
    var hubTraffic = $.connection.HubMainSysMessages;

    hubTraffic.client.NewMessageToUser = function (MessageHeader, MessageUniqID) {

        document.dispatchEvent(event_HMSM_NewMessage);
        new PNotify({
            title: 'پیام جدید',
            text: MessageHeader,
            styling: 'bootstrap3',
            type: 'info'
        });
    };

    hubTraffic.client.CVFGR_NewVipReq = function (MessageHeader, MessageUniqID) {

        document.dispatchEvent(event_CVFGR_NewVipReq);
        
    };
    hubTraffic.client.ConfirmGuestReuqest = function (MessageHeader, MessageUniqID) {

        document.dispatchEvent(event_ConfirmGuestReuqest);
        new PNotify({
            title: 'پیام جدید',
            text: MessageHeader,
            styling: 'bootstrap3',
            type: 'info'
        });
    };


    hubTraffic.client.SysMessageFromServer = function (CommandMsg) {
        try {
            if (CommandMsg === 'SMFSRefreshPage') {
                new PNotify({
                    title: 'پیغام سامانه',
                    text: 'خروج خودکار از سامانه به دلیل منقضی شدن زمان نشست',
                    styling: 'bootstrap3',
                    type: 'info'
                });
                setTimeout(function () {
                    window.location.reload();
                },
                    500);

            }
        }
        catch (hubTrafficclientNewLogErr) {
            console.log('Error in hubTraffic.client.NewLog : ' + hubTrafficclientNewLogErr)
        }
    };


    hubTraffic.client.NewCarLog = function (CarTrafficLogID) {
        try {
            var event_HMSM_NewIncomingLog = new CustomEvent('event_HMSM_NewIncomingLog', { detail: CarTrafficLogID });
            document.dispatchEvent(event_HMSM_NewIncomingLog);
        }
        catch (hubTrafficclientNewLogErr) {
            console.log('Error in hubTraffic.client.NewLog : ' + hubTrafficclientNewLogErr)
        }
    };

    hubTraffic.client.MessageFromServer = function (msg,title) {
        try {
                new PNotify({
                    title: title,
                    text: msg,
                    styling: 'bootstrap3',
                    hide: false,
                    type: 'info',
                    addclass: 'dark'
                });
        }
        catch (hubTrafficclientMessageFromServerErr) {
            console.log('Error in hubTraffic.client.MessageFromServer : ' + hubTrafficclientMessageFromServerErr)
        }
    };


    $.connection.hub.start().done(function (event) {
        NPSHubConnectionID = $.connection.hub.id;
    });


});