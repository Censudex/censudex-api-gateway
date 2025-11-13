using ApiGateway.Protos.Order;
using ApiGateway.Src.DTOs;
using Google.Protobuf;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiGateway.Src.Controllers
{
    [ApiController]
    [Route("api/orders")]
    public class OrderController : ControllerBase
    {
        private readonly OrderService.OrderServiceClient _orderClient;

        public OrderController(OrderService.OrderServiceClient orderClient)
        {
            _orderClient = orderClient;
        }

        // 🔹 Crear orden
        [HttpPost]
        //[Authorize(Roles = "CLIENT")]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto apiRequest)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var grpcRequest = new CreateOrderRequest
                {
                    UserId = apiRequest.UserId.ToString(),
                    ClientName = apiRequest.ClientName,
                    ShippingAddress = apiRequest.ShippingAddress,
                };

                foreach (var item in apiRequest.Items)
                {
                    grpcRequest.Items.Add(new OrderItem
                    {
                        ProductId = item.ProductId.ToString(),
                        Quantity = item.Quantity,
                        Price = (float)item.Price
                    });
                }

                var response = await _orderClient.CreateOrderAsync(grpcRequest);

                return Ok(new
                {
                    Message = "Orden creada exitosamente",
                    Order = response.Order
                });
            }
            catch (RpcException ex)
            {
                return StatusCode((int)ex.StatusCode, new { Error = ex.Status.Detail });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpGet]
        //[Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> GetAllOrders([FromQuery] string? id, [FromQuery] string? userId, [FromQuery] string? startDate, [FromQuery] string? endDate)
        {
            try
            {
                var request = new GetAllOrdersRequest
                {
                    Id = id ?? "",
                    UserId = userId ?? "",
                    StartDate = startDate ?? "",
                    EndDate = endDate ?? ""
                };

                var response = await _orderClient.GetAllOrdersAsync(request);
                return Ok(response.Orders);
            }
            catch (RpcException ex)
            {
                return StatusCode((int)ex.StatusCode, new { Error = ex.Status.Detail });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpGet("{trackingNumber}/status")]
        //[Authorize(Roles = "CLIENT")]
        public async Task<IActionResult> GetOrderStatus(string trackingNumber)
        {
            try
            {
                var grpcRequest = new GetOrderStatusRequest
                {
                    TrackingNumber = trackingNumber
                };

                var response = await _orderClient.GetOrderStatusAsync(grpcRequest);
                return Ok(new { TrackingNumber = trackingNumber, Status = response.Status });
            }
            catch (RpcException ex)
            {
                return StatusCode((int)ex.StatusCode, new { Error = ex.Status.Detail });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        // 🔹 Actualizar el estado de una orden
        [HttpPatch("{id}/status")]
        //[Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> UpdateOrderStatus(string id, [FromBody] UpdateOrderStatusDto apiRequest)
        {
            try
            {
                var grpcRequest = new UpdateOrderStatusRequest
                {
                    Id = id,
                    Status = apiRequest.Status
                };

                var response = await _orderClient.UpdateOrderStatusAsync(grpcRequest);
                return Ok(new { Success = response.Success });
            }
            catch (RpcException ex)
            {
                return StatusCode((int)ex.StatusCode, new { Error = ex.Status.Detail });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        // 🔹 Cancelar una orden
        [HttpPatch("{idOrTracking}")]
        [Authorize(Roles = "CLIENT,ADMIN")]
        public async Task<IActionResult> CancelOrder(string idOrTracking, [FromBody] string reason)
        {
            try
            {
                // 🔹 Obtener el rol desde el token JWT
                var role = User.IsInRole("ADMIN") ? "admin" : "user";

                // 🔹 Crear request gRPC
                var grpcRequest = new CancelOrderRequest
                {
                    IdOrTracking = idOrTracking,
                    Role = role,
                    Reason = reason ?? string.Empty
                };

                // 🔹 Llamar al servicio gRPC
                var response = await _orderClient.CancelOrderAsync(grpcRequest);

                // 🔹 Respuesta HTTP
                return Ok(new
                {
                    Success = response.Success,
                    Message = response.Message
                });
            }
            catch (RpcException ex)
            {
                return StatusCode((int)ex.StatusCode, new { Error = ex.Status.Detail });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        // 🔹 Obtener historial de órdenes de un usuario
        [HttpGet("history/{userId}")]
        //[Authorize(Roles = "CLIENT")]
        public async Task<IActionResult> GetOrderHistory(string userId)
        {
            try
            {
                var grpcRequest = new GetOrderHistoryRequest
                {
                    UserId = userId
                };

                var response = await _orderClient.GetOrderHistoryAsync(grpcRequest);
                return Ok(response.Orders);
            }
            catch (RpcException ex)
            {
                return StatusCode((int)ex.StatusCode, new { Error = ex.Status.Detail });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }
    }
}
