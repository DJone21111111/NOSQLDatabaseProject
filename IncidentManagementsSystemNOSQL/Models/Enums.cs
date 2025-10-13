namespace IncidentManagementsSystemNOSQL.Models
{
    public static class Enums
    {
        public enum UserRole
        {
            employee,
            service_desk
        }

        public enum TicketStatus
        {
            open,
            in_progress,
            closed_resolved,
            closed_no_resolve
        }

        public enum DepartmentType
        {
            IT,
            HR,
            Finance,
            Operations,
            Marketing
        }

    }
}
