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

        public void Send(string jsonTickets, string action)
        {
            Clients.All.showMessage(jsonTickets, action);
        }

        public void Start()
        {
            // check if application cache has previously been populated
            if (String.IsNullOrEmpty((HttpRuntime.Cache["Tickets"] as string))) 
            {
                lock (threadSafeCode)
                {
                    PostgreSqlListener listener = new PostgreSqlListener();

                    string jsonTickets = listener.GetTicketsList();
                    HttpRuntime.Cache["Tickets"] = jsonTickets;
                    Clients.Caller.addMessage(jsonTickets, "Select");                  
                }
            }
            else
            {
                Clients.Caller.addMessage((HttpRuntime.Cache["Tickets"] as string), "Select");           
            }
        }
    }
}