using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace ApiGateway.Src.DTOs
{
    /// <summary>
    /// DTO para la creación de un nuevo producto a través de la API Gateway.
    /// </summary>
    public class CreateProductApiRequest
    {
        [Required(ErrorMessage = "Name is required.")]
        public required string Name { get; set; }

        [Required(ErrorMessage = "Description is required.")]
        public required string Description { get; set; }

        [Required(ErrorMessage = "Price is required.")]
        [Range(0.01, (double)float.MaxValue, ErrorMessage = "Price must be a positive number.")]
        public required float Price { get; set; }

        [Required(ErrorMessage = "Category is required.")]
        public required string Category { get; set; }

        [Required(ErrorMessage = "ImageFile is required.")]
        public required IFormFile ImageFile { get; set; }
    }
}