using System.ComponentModel.DataAnnotations;

namespace ApiGateway.Src.DTOs
{
    public class UpdateOrderStatusDto
    {
        [Required]
        public string Status { get; set; } = string.Empty;
    }
}