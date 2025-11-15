using ApiGateway.Protos.Order;
using ApiGateway.Src.DTOs;
using Google.Protobuf;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiGateway.Src.Controllers
{
    /// <summary>
    /// Controlador que expone endpoints HTTP para operaciones sobre órdenes.
    /// Actúa como API Gateway y delega la lógica al servicio gRPC de órdenes.
    /// </summary>
    [ApiController]
    [Route("api/orders")]
    public class OrderController : ControllerBase
    {
        /// <summary>
        /// Cliente gRPC generado para comunicarse con el servicio de órdenes.
        /// </summary>
        private readonly OrderService.OrderServiceClient _orderClient;

        /// <summary>
        /// Crea una instancia del controlador con el cliente gRPC inyectado.
        /// </summary>
        /// <param name="orderClient">Cliente gRPC del servicio de órdenes.</param>
        public OrderController(OrderService.OrderServiceClient orderClient)
        {
            _orderClient = orderClient;
        }

        /// <summary>
        /// Crea una nueva orden en el servicio de órdenes.
        /// Convierte el DTO recibido en la petición gRPC correspondiente y devuelve la orden creada.
        /// </summary>
        /// <param name="apiRequest">DTO con los datos de la orden a crear.</param>
        /// <returns>200 OK con la orden creada o el error correspondiente.</returns>
        [HttpPost]
        [Authorize(Roles = "CLIENT")]
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

        /// <summary>
        /// Obtiene todas las órdenes que cumplan los filtros opcionales.
        /// Solo accesible por usuarios con rol ADMIN.
        /// </summary>
        /// <param name="id">Filtro por id de orden (opcional).</param>
        /// <param name="userId">Filtro por id de usuario (opcional).</param>
        /// <param name="startDate">Fecha inicial del rango (opcional, formato ISO).</param>
        /// <param name="endDate">Fecha final del rango (opcional, formato ISO).</param>
        /// <returns>200 OK con la lista de órdenes o error en caso de fallo.</returns>
        [HttpGet]
        [Authorize(Roles = "ADMIN")]
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

        /// <summary>
        /// Obtiene el estado actual de una orden a partir de su número de seguimiento.
        /// Accesible para clientes (rol CLIENT).
        /// </summary>
        /// <param name="trackingNumber">Número de seguimiento de la orden.</param>
        /// <returns>200 OK con el estado de la orden o error.</returns>
        [HttpGet("{trackingNumber}/status")]
        [Authorize(Roles = "CLIENT")]
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

        /// <summary>
        /// Actualiza el estado de una orden identificada por su id.
        /// Solo accesible por administradores (rol ADMIN).
        /// </summary>
        /// <param name="id">Identificador de la orden a actualizar.</param>
        /// <param name="apiRequest">DTO que contiene el nuevo estado.</param>
        /// <returns>200 OK con el resultado de la operación o error.</returns>
        [HttpPatch("{id}/status")]
        [Authorize(Roles = "ADMIN")]
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

        /// <summary>
        /// Cancela una orden por id o número de tracking.
        /// Usuarios con rol CLIENT pueden cancelar sus órdenes; ADMIN puede cancelar cualquier orden.
        /// </summary>
        /// <param name="idOrTracking">Id de la orden o número de tracking.</param>
        /// <param name="reason">Motivo de la cancelación (cadena en el cuerpo de la petición).</param>
        /// <returns>200 OK con el resultado de la cancelación o error.</returns>
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
                    Reason = reason ?? "no reason provided"
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

        /// <summary>
        /// Obtiene el historial de órdenes de un usuario específico.
        /// Accesible por clientes (rol CLIENT).
        /// </summary>
        /// <param name="userId">Identificador del usuario cuyo historial se solicita.</param>
        /// <returns>200 OK con la lista de órdenes del usuario o error.</returns>
        [HttpGet("history/{userId}")]
        [Authorize(Roles = "CLIENT")]
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