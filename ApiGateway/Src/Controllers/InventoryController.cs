using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiGateway.Protos.Inventory;
using ApiGateway.Src.DTOs;
using ApiGateway.Src.Extensions;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiGateway.Src.Controllers
{
    /// <summary>
    /// Controlador para la gestión del inventario.
    /// </summary>
    [ApiController]
    [Route("api/inventory")]
    public class InventoryController : ControllerBase
    {
        /// <summary>
        /// Cliente gRPC para el servicio de inventario.
        /// </summary>
        private readonly InventoryService.InventoryServiceClient _inventoryClient;

        /// <summary>
        /// Constructor del controlador de inventario.
        /// </summary>
        /// <param name="inventoryClient">Cliente gRPC para el servicio de inventario.</param>
        public InventoryController(InventoryService.InventoryServiceClient inventoryClient)
        {
            _inventoryClient = inventoryClient;
        }   

        /// <summary>
        /// Obtiene todos los artículos del inventario.
        /// </summary>
        /// <returns>Una lista de todos los artículos del inventario.</returns>
        [Authorize(Roles = "ADMIN")]
        [HttpGet]
        public async Task<IActionResult> GetAllInventoryItems()
        {
            try
            {
                var request = new GetAllInventoryItemsRequest();
                var response = await _inventoryClient.GetAllInventoryItemsAsync(request);
                return Ok(response);
            }
            catch (RpcException ex)
            {
                return GrpcErrorMapper.MapGrpcErrorToHttp(ex);
            }
        }

        /// <summary>
        /// Obtiene un artículo del inventario por su ID.
        /// </summary>
        /// <param name="productId">ID del artículo de inventario.</param>
        /// <returns>El artículo de inventario correspondiente al ID proporcionado.</returns>
        [HttpGet("{productId}")]
        public async Task<IActionResult> GetInventoryItemById(string productId)
        {
            try
            {
                var request = new GetInventoryItemByIdRequest { ItemId = productId };
                var response = await _inventoryClient.GetInventoryItemByIdAsync(request);
                if (response.Item == null) return NotFound(new { message = "Product not found" });
                return Ok(response);
            }
            catch (RpcException ex)
            {
                return GrpcErrorMapper.MapGrpcErrorToHttp(ex);
            }
            
        }

        /// <summary>
        /// Actualiza el stock de un artículo del inventario.
        /// </summary>
        /// <param name="productId">ID del artículo de inventario.</param>
        /// <param name="updateStockDto">Datos para la actualización del stock.</param>
        /// <returns>Resultado de la operación de actualización.</returns>
        [HttpPatch("{productId}")]
        public async Task<IActionResult> UpdateItemStock(string productId, UpdateStockDto updateStockDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var request = new UpdateInventoryItemStockRequest
                {
                    ItemId = productId,
                    Operation = updateStockDto.Operation,
                    Quantity = updateStockDto.Quantity
                };

                var response = await _inventoryClient.UpdateInventoryItemStockAsync(request);
                if (!response.Item.Success) return BadRequest(new { message = response.Item.Message });
                return Ok(response);
            }
            catch (RpcException ex)
            {
                return GrpcErrorMapper.MapGrpcErrorToHttp(ex);
            }
            
        }
    }
}