using System.ComponentModel.DataAnnotations;

namespace VideoShare_BackEnd.Models.ControllerModels
{
    public class Test2RequestModel
    {
        [Required]
        public long? id { get; set; }
    }
}