using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using ApiGateway.Src.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace ApiGateway.Src.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AuthController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("AuthService");
                var response = await client.PostAsJsonAsync("/api/auth/login", request);

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

        [HttpPost("ValidateToken")]
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