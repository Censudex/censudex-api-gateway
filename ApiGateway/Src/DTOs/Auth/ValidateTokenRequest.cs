using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ApiGateway.Src.DTOs
{
    /// <summary>
    /// DTO para la solicitud de validación de token.
    /// </summary>
    public class ValidateTokenRequest
    {
        /// <summary>
        /// Token de autenticación a validar.
        /// </summary>
        [Required(ErrorMessage = "The token is required for logout.")]
        public string Token { get; set; } = string.Empty;
    }
}