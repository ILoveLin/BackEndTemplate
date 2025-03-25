using System.ComponentModel.DataAnnotations;

namespace BackEndTemplate.Models.ControllerModels.RequestModels.User;

public class BindParentAccountRequestModel
{
    [Required]
    [Range(0,long.MaxValue)]
    public string? ParentIdentifier { get; set; }
    
}