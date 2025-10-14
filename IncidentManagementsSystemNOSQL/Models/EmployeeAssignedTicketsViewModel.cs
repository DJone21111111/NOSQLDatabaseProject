using System.Collections.Generic;

namespace IncidentManagementsSystemNOSQL.Models
{
    public class EmployeeAssignedTicketsViewModel
    {
        public string EmployeeId { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeeEmail { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public List<Ticket> Tickets { get; set; } = new();
        public int TotalTickets { get; set; }
        public int OpenCount { get; set; }
        public int InProgressCount { get; set; }
        public int ClosedResolvedCount { get; set; }
        public int ClosedNoResolveCount { get; set; }
        public double OpenPercent { get; set; }
        public double InProgressPercent { get; set; }
        public double ClosedResolvedPercent { get; set; }
        public double ClosedNoResolvePercent { get; set; }
    }
}
