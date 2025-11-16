using System.ComponentModel.DataAnnotations;

namespace ApiGateway.Src.DTOs
{
    /// <summary>
    /// DTO para actualizar el estado de una orden.
    /// Contiene el estado objetivo que debe aplicarse a la orden indicada en la petición.
    /// </summary>
    public class UpdateOrderStatusDto
    {
        /// <summary>
        /// Nuevo estado de la orden.
        /// Obligatorio. Se recomienda usar valores definidos en el modelo de dominio (por ejemplo: "Pending", "Processing", "Shipped", "Cancelled").
        /// </summary>
        [Required(ErrorMessage = "Status is required.")]
        public string Status { get; set; } = string.Empty;
    }
}