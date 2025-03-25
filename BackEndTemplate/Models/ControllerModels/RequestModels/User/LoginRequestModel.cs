using System.ComponentModel.DataAnnotations;
using BackEndTemplate.Models.User;

namespace BackEndTemplate.Models.ControllerModels
{
    public class LoginRequestModel
    {
        [Required]
        [StringLength(100)]
        public string? Account { get; set; }

        [Required]
        [StringLength(100)]
        public string? Password { get; set; }

        [Required]
        public UserAuthType? AuthType { get; set; } = UserAuthType.Default;
    }
}