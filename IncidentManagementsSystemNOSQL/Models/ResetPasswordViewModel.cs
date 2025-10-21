namespace IncidentManagementsSystemNOSQL.Models
{
    public class ResetPasswordViewModel
    {
        [System.ComponentModel.DataAnnotations.Required]
        public string UserId { get; set; }

        [System.ComponentModel.DataAnnotations.Required]
        public string Token { get; set; }

        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.MinLength(6)]
        [System.ComponentModel.DataAnnotations.DataType(System.ComponentModel.DataAnnotations.DataType.Password)]
        public string NewPassword { get; set; }

        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.Compare(nameof(NewPassword))]
        [System.ComponentModel.DataAnnotations.DataType(System.ComponentModel.DataAnnotations.DataType.Password)]
        public string ConfirmPassword { get; set; }
    }
}
