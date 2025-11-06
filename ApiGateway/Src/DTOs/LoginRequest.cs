using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiGateway.Src.DTOs
{
    public class LoginRequest
    {
        public required string Email { get; set; } = string.Empty;
        public required string Password { get; set; } = string.Empty;
    }

    public class LoginRequestExtended
    {
        public required string Email { get; set; } = string.Empty;
        public required string Id { get; set; } = string.Empty;
        public required string Username { get; set; } = string.Empty;
        public required string Role { get; set; } = string.Empty;
    }
}