using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SignalRPostgresql.Models
{
    public class TicketStatus
    {
        public List<TicketDetails> TicketsDetailsList;

        public TicketStatus()
        {
            TicketsDetailsList = new List<TicketDetails>();
        }
    }
}