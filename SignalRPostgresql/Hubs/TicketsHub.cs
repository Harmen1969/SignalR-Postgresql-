using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using SignalRPostgresql.Controllers;

namespace SignalRPostgresql.Hubs
{
    public class TicketsHub : Hub
    {
        private Object threadSafeCode = new Object();

        public void Send(string message, string action)
        {
            Clients.All.showMessage(message, action);
        }

        public void Start()
        {
            // check if application cache has previously been populated

            // PostgresSqlListener_New ==> Install-Package Npgsql (3.2.4.1)
            if (String.IsNullOrEmpty((HttpRuntime.Cache["Tickets"] as string)))
            {
                lock (threadSafeCode)
                {
                    PostgresSqlListener_New list = new PostgresSqlListener_New();
                    list.ListenForNotifications();

                    string jsonTickets = list.GetTicketsList();
                    HttpRuntime.Cache["Tickets"] = jsonTickets;
                    Clients.Caller.addMessage(jsonTickets, "Select");
                }

                // PostgreSqlListener ==> Install-Package Npgsql -Version 2.2.7

                //lock (threadSafeCode)
                //{
                //    PostgreSqlListener listener = new PostgreSqlListener();
                //    string jsonTickets = listener.GetTicketsList();
                //    HttpRuntime.Cache["Tickets"] = jsonTickets;
                //    Clients.Caller.addMessage(jsonTickets, "Select");
                //}
            }
            else
            {
                Clients.Caller.addMessage((HttpRuntime.Cache["Tickets"] as string), "Select");
            }
        }
    }
}