using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http; // Necesario para IFormFile

namespace ApiGateway.Src.DTOs
{
    /// <summary>
    /// DTO para la actualización de un producto a través de la API Gateway.
    /// </summary>
    public class UpdateProductApiRequest
    {
        public string? Name { get; set; }

        public string? Description { get; set; }

        [Range(0.01, (double)float.MaxValue, ErrorMessage = "Price must be a positive number.")]
        public float? Price { get; set; } 

        public string? Category { get; set; }

        public IFormFile? NewImageFile { get; set; }
    }
}