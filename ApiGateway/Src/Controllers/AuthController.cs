using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using ApiGateway.Src.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace ApiGateway.Src.Controllers
{
    /// <summary>
    /// Controlador para la autenticación de usuarios.
    /// </summary>
    [ApiController]
    [Route("api")]
    public class AuthController : ControllerBase
    {
        /// <summary>
        /// Fábrica de clientes HTTP para realizar solicitudes a servicios externos.
        /// </summary>
        private readonly IHttpClientFactory _httpClientFactory;

        /// <summary>
        /// Constructor del controlador de autenticación.
        /// </summary>
        /// <param name="httpClientFactory">Fábrica de clientes HTTP.</param>
        public AuthController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        /// <summary>
        /// Inicia sesión para un usuario.
        /// </summary>
        /// <param name="request">Datos de inicio de sesión.</param>
        /// <returns>Resultado de la operación de inicio de sesión.</returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("AuthService");

                LoginRequestExtended loginRequestExtended;

                if (request.Email.Equals(Environment.GetEnvironmentVariable("ADMIN_EMAIL")) &&
                    request.Password.Equals(Environment.GetEnvironmentVariable("ADMIN_PASSWORD")))
                {
                    loginRequestExtended = new LoginRequestExtended
                    {
                        Id = "12345",
                        Username = "Admin",
                        Email = request.Email,
                        Role = "ADMIN"
                    };
                }
                else if (request.Email.Equals(Environment.GetEnvironmentVariable("CLIENT_EMAIL")) &&
                        request.Password.Equals(Environment.GetEnvironmentVariable("CLIENT_PASSWORD")))
                {
                    loginRequestExtended = new LoginRequestExtended
                    {
                        Id = "67890",
                        Username = "ClientUser",
                        Email = request.Email,
                        Role = "CLIENT"
                    };
                }
                else
                {
                    return Unauthorized(new { message = "Invalid email or password" });
                }

                var response = await client.PostAsJsonAsync("/api/auth/login", loginRequestExtended);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode) return StatusCode((int)response.StatusCode, JsonSerializer.Deserialize<object>(content));

                return Ok(JsonSerializer.Deserialize<object>(content));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", detail = ex.Message });
            }
        }

        /// <summary>
        /// Valida un token de autenticación.
        /// </summary>
        /// <param name="request">Datos del token a validar.</param>
        /// <returns>Resultado de la validación del token.</returns>
        [HttpPost("validate-token")]
        public async Task<IActionResult> ValidateToken([FromBody] ValidateTokenRequest request)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("AuthService");
                var response = await client.PostAsJsonAsync($"/api/Auth/validate-token", request);

                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, JsonSerializer.Deserialize<object>(content));
                }

                return Ok(JsonSerializer.Deserialize<object>(content));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", detail = ex.Message });
            }
        }

        /// <summary>
        /// Cierra la sesión de un usuario.
        /// </summary>
        /// <param name="request">Datos de cierre de sesión.</param>
        /// <returns>Resultado de la operación de cierre de sesión.</returns>
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("AuthService");
                var response = await client.PostAsJsonAsync($"/api/auth/logout", request);

                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, JsonSerializer.Deserialize<object>(content));
                }

                return Ok(JsonSerializer.Deserialize<object>(content));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", detail = ex.Message });
            }
        }
    }
}