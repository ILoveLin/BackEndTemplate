using System.ComponentModel.DataAnnotations;
using BackEndTemplate.Models.User;

namespace BackEndTemplate.Models.ControllerModels.RequestModels.User;

public class RegisterRequestModel
{
    /// <summary>
    /// 该值一旦设置，将无法修改，系统自动生成的可以保留一次修改机会
    /// </summary>
    [Required]
    [StringLength(100)]
    public string? Account { get; set; }

    [Required]
    [StringLength(100)]
    public string? Username { get; set; }

    [Required]
    [StringLength(100)]
    public string? Password { get; set; }

    /// <summary>
    /// 该值一旦设置，将无法修改
    /// </summary>
    [Required]
    public UserType? UserType { get; set; }

    public string? Phone { get; set; }

    public string? Avatar { get; set; }
}
