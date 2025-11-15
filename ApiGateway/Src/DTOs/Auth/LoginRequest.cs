using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiGateway.Src.DTOs
{
    /// <summary>
    /// DTO para la solicitud de inicio de sesión.
    /// </summary>
    public class LoginRequest
    {
        /// <summary>
        /// Correo electrónico del usuario.
        /// </summary>
        public required string Email { get; set; } = string.Empty;
        /// <summary>
        /// Contraseña del usuario.
        /// </summary>
        public required string Password { get; set; } = string.Empty;
    }
    
    /// <summary>
    /// DTO extendido para la respuesta de inicio de sesión.
    /// </summary>
    public class LoginRequestExtended
    {
        /// <summary>
        /// Correo electrónico del usuario.
        /// </summary>
        public required string Email { get; set; } = string.Empty;
        /// <summary>
        /// ID del usuario.
        /// </summary>
        public required string Id { get; set; } = string.Empty;
        /// <summary>
        /// Nombre de usuario.
        /// </summary>
        public required string Username { get; set; } = string.Empty;
        /// <summary>
        /// Rol del usuario.
        /// </summary>
        public required string Role { get; set; } = string.Empty;
    }
}