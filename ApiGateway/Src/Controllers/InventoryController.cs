using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiGateway.Protos.Inventory;
using ApiGateway.Src.DTOs;
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
            var request = new GetAllInventoryItemsRequest();
            var response = await _inventoryClient.GetAllInventoryItemsAsync(request);
            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetInventoryItemById(string id)
        {
            var request = new GetInventoryItemByIdRequest { ItemId = id };
            var response = await _inventoryClient.GetInventoryItemByIdAsync(request);
            return Ok(response);
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateItemStock(string id, UpdateStockDto updateStockDto)
        {
            var request = new UpdateInventoryItemStockRequest
            {
                ItemId = id,
                Operation = updateStockDto.Operation,
                Quantity = updateStockDto.Quantity
            };

            var response = await _inventoryClient.UpdateInventoryItemStockAsync(request);
            return Ok(response);
        }
    }
}