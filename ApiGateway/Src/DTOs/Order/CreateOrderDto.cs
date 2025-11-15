
using System.ComponentModel.DataAnnotations;

namespace ApiGateway.Src.DTOs
{
    /// <summary>
    /// DTO para la creación de una orden.
    /// Contiene la información mínima requerida por el API Gateway para crear una orden en el servicio correspondiente.
    /// </summary>
    public class CreateOrderDto
    {
        /// <summary>
        /// Identificador del usuario que realiza la orden.
        /// Obligatorio.
        /// </summary>
        [Required(ErrorMessage = "UserId is required.")]
        public required string UserId { get; set; }

        /// <summary>
        /// Nombre del cliente asociado a la orden.
        /// Obligatorio.
        /// </summary>
        [Required(ErrorMessage = "ClientName is required.")]
        public required string ClientName { get; set; }

        /// <summary>
        /// Dirección de envío para la orden.
        /// Obligatorio.
        /// </summary>
        [Required(ErrorMessage = "ShippingAddress is required.")]
        public required string ShippingAddress { get; set; }

        /// <summary>
        /// Lista de ítems incluidos en la orden.
        /// Cada elemento representa un producto/servicio y su cantidad.
        /// Obligatorio (debe contener al menos un ítem).
        /// </summary>
        [Required(ErrorMessage = "Items are required.")]
        public List<OrderItemDto> Items { get; set; } = new();


    }
}
