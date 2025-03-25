using System.ComponentModel.DataAnnotations;

namespace VideoShare_BackEnd.Models.ControllerModels.RequestModels.User;

public class BindParentAccountRequestModel
{
    [Required]
    [Range(0,long.MaxValue)]
    public string? ParentIdentifier { get; set; }
    
}