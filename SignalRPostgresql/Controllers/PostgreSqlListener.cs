using System;
using System.Collections.Generic;
using System.Web;
using Microsoft.AspNet.SignalR;
using Npgsql;
using SignalRPostgresql.Models;
using System.Data;
using SignalRPostgresql.Hubs;

namespace SignalRPostgresql.Controllers
{
    public class PostgreSqlListener
    {        
        private TicketStatus ticketStatus;
        private Object threadSafeCode = new Object();
        NpgsqlConnection conn;
        NpgsqlCommand command;
        public bool notificationflg = false;

        public PostgreSqlListener()
        {
            // Create ticketstatus object
            ticketStatus = new TicketStatus();           
        }

        // Get tickets from the PostgreSQL database and return the tickets as a JSON string
        public string GetTicketsList()
        {
            this.ticketStatus.TicketsDetailsList = new List<TicketDetails>();
         
            if (conn == null)
            {
                conn = new NpgsqlConnection("Server=127.0.0.1;Port=5432;User Id=postgres;Password=postges@2017;Database=postgres;MINPOOLSIZE=10;MaxPoolSize=1000;Pooling=true;Timeout=500");               
                
            }

            using (NpgsqlCommand sqlCmd = new NpgsqlCommand())
            {
                sqlCmd.Connection = conn;
                if (sqlCmd.Connection.State == ConnectionState.Closed)
                {
                    sqlCmd.Connection.Open();
                }

                // Check if the postgreSQL listener 'notifytickets' is already created
                if (command == null)
                {
                    command = new NpgsqlCommand("listen notifytickets;", conn);
                    command.ExecuteNonQuery();
                }

                // Query tickets.
                sqlCmd.CommandType = CommandType.Text;
                sqlCmd.CommandText = "SELECT TICKETID,CREATED, DATEASSIGNED,TITLE,DESCRIPTION,coalesce(PRIORITY,'') AS PRIORITY ," +
                    "coalesce(ASSIGNEDTO,'') AS ASSIGNEDTO," +
                    " TICKETSTATUS FROM TICKETS";

                // Check if Notification is already registered to OnNotification
                // connection object
                if (notificationflg == false)
                {
                    conn.Notification += OnNotification; // new Notifidler(OnNotification);
                    notificationflg = true;                   
                } 

                using (NpgsqlDataReader reader = sqlCmd.ExecuteReader())
                {
                    string DateAssignedstr = "";
                    while (reader.Read())
                      //  conn.Wait();
                    {
                        // check if DateTime field DateAssigned is null
                        if (!reader.IsDBNull(2))
                        {
                         DateAssignedstr = reader.GetDateTime(2).ToString();
                        }
                        else
                        {
                         DateAssignedstr = String.Empty;
                        }
                        TicketDetails ticket = new TicketDetails();
                        ticket.TicketId = reader.GetInt32(0);
                        ticket.Created = reader.GetDateTime(1).ToString();
                        ticket.DateAssigned = DateAssignedstr; // 
                        ticket.Title = reader.GetString(3);
                        ticket.Description = reader.GetString(4);
                        ticket.Priority = reader.GetString(5);
                        ticket.AssignedTo = reader.GetString(6);
                        ticket.TicketStatus = reader.GetString(7);

                        // Add this ticket to the TicketsDetailsList list object
                        ticketStatus.TicketsDetailsList.Add(ticket);
                    }
                    reader.Close();
                }
               
            }

            lock (threadSafeCode)
            {
                // Serialize the tickets to JSON
                HttpRuntime.Cache["Tickets"] = SerializeObjectToJson(this.ticketStatus);
            }      

            // Return JSON string ticket details
            return (HttpRuntime.Cache["Tickets"] as string);            
        }


        private void OnNotification(object sender, NpgsqlNotificationEventArgs e)
        {
            
            var context = GlobalHost.ConnectionManager.GetHubContext<TicketsHub>();

            string actionName = e.AdditionalInformation.ToString();
            string actionType = "";
            if (actionName.Contains("DELETE"))
            {
                actionType = "Delete";
            }
            if (actionName.Contains("UPDATE"))
            {
                actionType = "Update";
            }
            if (actionName.Contains("INSERT"))
            {
                actionType = "Insert";
            }
            // Now broadcast the tickets and actiontype (SignalR)            
            context.Clients.All.addMessage(this.GetTicketsList(), actionType);

            //// NotificationEventHandler pp = new NotificationEventHandler(OnNotification);
            // conn.Notification -= OnNotification; // pp; // OnNotification;
            //conn.Close(); //close connection for this event           

        }

        // Serialize ticketdetails objet to a JSON string
        public String SerializeObjectToJson(Object ticketStatus)
        {
            try
            {
                return new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(ticketStatus);
            }
            catch (Exception) { return null; }
        }

        //public String SafeGetString(this NpgsqlDataReader reader, int colIndex)
        //{
        //    if (!reader.IsDBNull(colIndex))
        //        return reader.GetDateTime(colIndex).ToString();
        //    return string.Empty;
        //}



    }
}