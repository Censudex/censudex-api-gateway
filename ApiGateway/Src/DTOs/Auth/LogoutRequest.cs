using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ApiGateway.Src.DTOs
{
    /// <summary>
    /// DTO para la solicitud de cierre de sesión.
    /// </summary>
    public class LogoutRequest
    {
        /// <summary>
        /// Token de autenticación del usuario.
        /// </summary>
        public string Token { get; set; } = string.Empty;
    }
}