using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiGateway.Protos.Inventory;
using ApiGateway.Src.DTOs;
using ApiGateway.Src.Extensions;
using Grpc.Core;
using Microsoft.AspNetCore.Mvc;

namespace ApiGateway.Src.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryController : ControllerBase
    {
        private readonly InventoryService.InventoryServiceClient _inventoryClient;

        public InventoryController(InventoryService.InventoryServiceClient inventoryClient)
        {
            _inventoryClient = inventoryClient;
        }

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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetInventoryItemById(string id)
        {
            try
            {
                var request = new GetInventoryItemByIdRequest { ItemId = id };
                var response = await _inventoryClient.GetInventoryItemByIdAsync(request);
                if (response.Item == null) return NotFound(new { message = "Product not found" });
                return Ok(response);
            }
            catch (RpcException ex)
            {
                return GrpcErrorMapper.MapGrpcErrorToHttp(ex);
            }
            
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateItemStock(string id, UpdateStockDto updateStockDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var request = new UpdateInventoryItemStockRequest
                {
                    ItemId = id,
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