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
    [ApiController]
    [Route("api/inventory")]
    public class InventoryController : ControllerBase
    {
        private readonly InventoryService.InventoryServiceClient _inventoryClient;

        public InventoryController(InventoryService.InventoryServiceClient inventoryClient)
        {
            _inventoryClient = inventoryClient;
        }

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