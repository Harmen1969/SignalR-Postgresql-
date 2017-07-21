$(document).ready(function () {
   
  
    // SignalR initialise    
    var ticketshub = $.connection.ticketsHub;    
    
    // Show first notification
    ticketshub.client.showMessage = function (message, action) {
        var actionName = action,
            toastMessage1 = {
                text: message + '-' +actionName,
                sticky: false,
                position: 'top-right',
                type: 'success',
                closeText: ''
            };
        var Toast1 = $().toastmessage('showToast', toastMessage1); // display notification
    }

    //  show database notifiction status
    ticketshub.client.addMessage = function (jsonTickets, action) {
        var header = '';
        switch (action) {
            case "Select":
                header = "Query executed...";
                break;
            case "Update":
                header = "update action...";
                break;
            case "Delete":
                header = "delete action...";
                break;
            case "Insert":
                header = "insert action...";
                break;
            default:
                header = "action?...";
        }

        var actionName2 = header,
            toastMessage2 = {
                text: actionName2,
                sticky: false,
                position: 'top-right',
                type: 'success',
                closeText: ''
            };

        var TicketDetails = [];

        var obj = $.parseJSON(jsonTickets);

        // assign ticketdetails from server to TicketDetails array
        TicketDetails = obj.TicketsDetailsList;

        // build up table row from array TicketsDetailsList
        if (TicketDetails != undefined) {
            var content = '';
            $.each(TicketDetails, function () {
                content += "<tr> <td>" + this['TicketId'] + "</td><td>" + this['Created'] + "</td>" + "</td > <td>" + this['Title'] + "</td>" + "</td > <td><abbr>" + this['Description'] + "</abbr></td>" + "</td > <td>" + this['Priority'] + "</td>" + "</td > <td>" + this['AssignedTo'] + "</td>" + "</td > <td>" + this['TicketStatus'] + "</td></tr>";
            });
            $('#ticketsGrid tbody').html(content);
            var Toast1 = $().toastmessage('showToast', toastMessage2); // display notification 
        };
    };

    // start SignalR    
    $.connection.hub.start().done(function () {
        ticketshub.server.send('Start', 'Hub started'); // call (server) function send() in TicketsHub.cs
        ticketshub.server.start(); // call (server) function start() in TicketsHub.cs



    });



});