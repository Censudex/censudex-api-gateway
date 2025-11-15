using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ApiGateway.Src.DTOs
{
    /// <summary>
    /// DTO para actualizar el stock de un artículo de inventario.
    /// </summary>
    public class UpdateStockDto
    {
        /// <summary>
        /// Operación a realizar: "increase" para aumentar el stock, "decrease" para disminuirlo.
        /// </summary>
        [RegularExpression("^(?i)(increase|decrease)$", ErrorMessage = "Operation must be either 'increase' or 'decrease'.")]
        public required string Operation { get; set; } = string.Empty;
        
        /// <summary>
        /// Cantidad a aumentar o disminuir en el stock.
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be a positive integer.")]
        public required int Quantity { get; set; } 
    }
}