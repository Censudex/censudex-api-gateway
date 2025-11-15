using System;
using System.ComponentModel.DataAnnotations;

namespace ApiGateway.Src.DTOs
{
    /// <summary>
    /// DTO que representa un ítem dentro de una orden.
    /// Contiene el identificador del producto, la cantidad solicitada y el precio unitario.
    /// Las anotaciones de datos aplicadas garantizan las validaciones mínimas requeridas.
    /// </summary>
    public class OrderItemDto
    {
        /// <summary>
        /// Identificador único del producto asociado al ítem.
        /// Obligatorio.
        /// </summary>
        [Required]
        public Guid ProductId { get; set; }

        /// <summary>
        /// Cantidad del producto en la orden.
        /// Debe ser un entero mayor o igual a 1.
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public int Quantity { get; set; }

        /// <summary>
        /// Precio unitario del producto.
        /// Debe ser un valor mayor que 0.
        /// </summary>
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public double Price { get; set; }
    }
}
