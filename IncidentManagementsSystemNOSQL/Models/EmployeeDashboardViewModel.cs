using System;
using System.Collections.Generic;

namespace IncidentManagementsSystemNOSQL.Models
{
    public class EmployeeDashboardViewModel
    {
        public string EmployeeId { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeeEmail { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public string DepartmentDescription { get; set; } = string.Empty;

        public List<Ticket> Tickets { get; set; } = new();

        public int TotalTickets { get; set; }
        public int OpenCount { get; set; }
        public int InProgressCount { get; set; }
        public int ClosedResolvedCount { get; set; }
        public int ClosedNoResolveCount { get; set; }

        public int PendingCount => OpenCount + InProgressCount;

        public double PendingPercent => TotalTickets == 0
            ? 0
            : Math.Round((double)PendingCount / TotalTickets * 100, 1);

        public double ResolvedPercent => TotalTickets == 0
            ? 0
            : Math.Round((double)ClosedResolvedCount / TotalTickets * 100, 1);

        public double ClosedNoResolvePercent => TotalTickets == 0
            ? 0
            : Math.Round((double)ClosedNoResolveCount / TotalTickets * 100, 1);
    }
}
