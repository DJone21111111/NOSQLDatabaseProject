using System.ComponentModel.DataAnnotations;

namespace IncidentManagementsSystemNOSQL.Models.Api
{
    public class TicketUpdateRequest
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(4000)]
        public string? Description { get; set; }

        [Required]
        [StringLength(32)]
        public string Status { get; set; } = string.Empty;
    }
}
