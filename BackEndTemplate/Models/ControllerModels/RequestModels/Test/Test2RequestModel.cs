using System.ComponentModel.DataAnnotations;

namespace BackEndTemplate.Models.ControllerModels
{
    public class Test2RequestModel
    {
        [Required]
        public long? id { get; set; }
    }
}