using Microsoft.AspNet.SignalR.Messaging;
using Npgsql;
using SignalRPostgresql.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using SignalRPostgresql.Hubs;
using Microsoft.AspNet.SignalR;


namespace SignalRPostgresql.Controllers
{
    public class PostgresSqlListener_New
    {

        private TicketStatus ticketStatus;
        private Object threadSafeCode = new Object();
           

        public PostgresSqlListener_New()
        {            
            ticketStatus = new TicketStatus();
        }

        static string GetConnectionString()
        {
            var csb = new NpgsqlConnectionStringBuilder
            {
                Host = "127.0.0.1",
                Database = "postgres",                
                Username = "postgres",
                Password = "postgres@2017",
                Port = 5432 ,
                KeepAlive = 30
            };

            return csb.ConnectionString;
        }


        public void ListenForNotifications() 
        {
            NpgsqlConnection conn = new NpgsqlConnection(GetConnectionString()); 
            
            conn.StateChange += conn_StateChange;
            var context = GlobalHost.ConnectionManager.GetHubContext<TicketsHub>();

            conn.Open();
            var listenCommand = conn.CreateCommand();
            listenCommand.CommandText = $"listen notifytickets;";
            listenCommand.ExecuteNonQuery();

            conn.Notification += PostgresNotificationReceived;
            // Show the tickets (call client function addMessage (see init.js))
            context.Clients.All.addMessage(this.GetTicketsList(),"Select");
            while (true)
            {       
                // wait until an asynchronous PostgreSQL notification arrives...
               conn.Wait();
            }
        }

        private void PostgresNotificationReceived(object sender, NpgsqlNotificationEventArgs e)
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
            // Now show the tickets and actiontype (SignalR: call client function addMessage (see init.js))            
            context.Clients.All.addMessage(this.GetTicketsList(), actionType);
        }

        // Get tickets from the PostgreSQL database and return the tickets as a JSON string
        public string GetTicketsList()
        {
            this.ticketStatus.TicketsDetailsList = new List<TicketDetails>();

            using (NpgsqlCommand sqlCmd = new NpgsqlCommand())
            {
                sqlCmd.CommandType = System.Data.CommandType.Text;
                   sqlCmd.CommandText = "SELECT TICKETID,CREATED, DATEASSIGNED,TITLE,DESCRIPTION,coalesce(PRIORITY,'') AS PRIORITY ," +
                       "coalesce(ASSIGNEDTO,'') AS ASSIGNEDTO," +
                       " TICKETSTATUS FROM TICKETS";

                NpgsqlConnection conn = new NpgsqlConnection(GetConnectionString());
                            
                conn.Open();
                // SELECT records from changed table...  
                // Query tickets.
                sqlCmd.Connection = conn;


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
                    conn.Close();
                    }              
            }

             HttpRuntime.Cache["Tickets"] = SerializeObjectToJson(ticketStatus);
            return (HttpRuntime.Cache["Tickets"] as string);
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

        //
        private void conn_StateChange(object sender, System.Data.StateChangeEventArgs e)
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<TicketsHub>();
            // Broadcast if connection state changed ( client function showMessage (see init.js)
            context.Clients.All.showMessage("Current State: " + e.CurrentState.ToString() + " Original State: " + e.OriginalState.ToString(), "connection state changed");
        }

    }
}
