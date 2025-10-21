using System;
using System.Collections.Generic;
using IncidentManagementsSystemNOSQL.Models;

namespace IncidentManagementsSystemNOSQL.Service
{
    public interface ITicketService
    {
        List<Ticket> GetAll();
        List<Ticket> GetAllSortedByPriority(bool descending = true);
        List<Ticket> GetByPriority(Enums.TicketPriority priority);
        Ticket? GetById(string id);
        List<Ticket> GetByUserId(string userId);
        List<Ticket> GetByStatus(Enums.TicketStatus status);

        List<Ticket> GetByDateRange(DateTime startDate, DateTime endDate);

        void AddTicket(Ticket ticket);
        void UpdateTicket(string id, Ticket updatedTicket);
        void DeleteTicket(string id);

        Dictionary<Enums.TicketStatus, int> GetTicketCountsByStatus();
        Dictionary<Enums.DepartmentType, int> GetTicketCountsByDepartment();
        Dictionary<Enums.TicketStatus, int> GetTicketCountsByStatusForEmployee(string employeeId);

        string GetNextTicketId();
    }
}
