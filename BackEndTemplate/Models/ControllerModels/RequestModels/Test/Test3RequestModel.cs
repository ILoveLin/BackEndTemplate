using System.ComponentModel.DataAnnotations;

namespace BackEndTemplate.Models.ControllerModels
{
    public class Test3RequestModel
    {
        [Required]
        public string? str { get; set; }

        public long? id { get; set; }
    }
}