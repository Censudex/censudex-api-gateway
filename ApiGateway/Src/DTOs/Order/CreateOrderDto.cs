using System.ComponentModel.DataAnnotations;

namespace ApiGateway.Src.DTOs
{
    public class CreateOrderDto
    {
        [Required(ErrorMessage = "UserId is required.")]
        public required string UserId { get; set; }

        [Required(ErrorMessage = "ClientName is required.")]
        public required string ClientName { get; set; }

        [Required(ErrorMessage = "ShippingAddress is required.")]
        public required string ShippingAddress { get; set; }

        [Required(ErrorMessage = "Items are required.")]
        public List<OrderItemDto> Items { get; set; } = new();


    }
}