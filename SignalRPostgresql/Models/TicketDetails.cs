using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SignalRPostgresql.Models
{
    public class TicketDetails
    {
        public TicketDetails() { }

        public int TicketId { get; set; }
        public string Created { get; set; }
        public string DateAssigned { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Priority { get; set; }
        public string AssignedTo { get; set; }
        public string TicketStatus { get; set; }

    }
}