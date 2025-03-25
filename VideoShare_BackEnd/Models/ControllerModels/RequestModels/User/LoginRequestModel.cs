using System.ComponentModel.DataAnnotations;
using VideoShare_BackEnd.Models.User;

namespace VideoShare_BackEnd.Models.ControllerModels
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