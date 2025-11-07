using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ApiGateway.Src.DTOs
{
    public class UpdateStockDto
    {
        [RegularExpression("^(?i)(increase|decrease)$", ErrorMessage = "Operation must be either 'increase' or 'decrease'.")]
        public required string Operation { get; set; } = string.Empty;
        
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be a positive integer.")]
        public required int Quantity { get; set; } 
    }
}