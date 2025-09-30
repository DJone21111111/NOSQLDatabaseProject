namespace IncidentManagementsSystemNOSQL.Models
{
    public class CommentAuthorEmbedded
    {
        public string EmployeeId { get; set; } = null!;
        public string Name { get; set; } = null!;
        public Enums.UserRole Role { get; set; }


    }
}
