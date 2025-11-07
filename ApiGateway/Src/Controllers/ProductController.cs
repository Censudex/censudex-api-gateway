using ApiGateway.Protos.Product;
using ApiGateway.Src.DTOs;
using ApiGateway.Src.Extensions;
using Google.Protobuf;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiGateway.Src.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly ProductsService.ProductsServiceClient _productClient;

        public ProductController(ProductsService.ProductsServiceClient productClient)
        {
            _productClient = productClient;
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        

        public async Task<IActionResult> CreateProduct([FromForm] CreateProductApiRequest apiRequest)
        {
            try
            {
                ByteString imageData;
                using (var memoryStream = new MemoryStream())
                {
                    await apiRequest.ImageFile.CopyToAsync(memoryStream);
                    imageData = ByteString.CopyFrom(memoryStream.ToArray());
                }


                var grpcRequest = new CreateProductRequest
                {
                    Name = apiRequest.Name,
                    Description = apiRequest.Description,
                    Price = apiRequest.Price,
                    Category = apiRequest.Category,
                    ImageData = imageData
                };

                var response = await _productClient.CreateProductAsync(grpcRequest);
                return Ok(response.Product);
            }
            catch (RpcException ex)
            {
                return GrpcErrorMapper.MapGrpcErrorToHttp(ex);
            }
        }

        [HttpGet]
        [Authorize(Roles = "CLIENT")]
        public async Task<IActionResult> GetAllProducts()
        {
            try
            {
                var response = await _productClient.GetProductsAsync(new GetProductsRequest());
                return Ok(response.Products);
            }
            catch (RpcException ex)
            {
                return GrpcErrorMapper.MapGrpcErrorToHttp(ex);
            }
        }


        [HttpGet("{id}")]
        [Authorize(Roles = "CLIENT")]
        public async Task<IActionResult> GetProductById(string id)
        {
            try
            {
                var request = new GetProductByIdRequest { Id = id };
                var response = await _productClient.GetProductByIdAsync(request);
                return Ok(response.Product);
            }
            catch (RpcException ex)
            {
                return GrpcErrorMapper.MapGrpcErrorToHttp(ex);
            }
        }

        [HttpPatch("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> UpdateProduct(string id, [FromForm] UpdateProductApiRequest apiRequest)
        {
            try
            {
                ByteString imageData = ByteString.Empty;
                if (apiRequest.NewImageFile != null)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await apiRequest.NewImageFile.CopyToAsync(memoryStream);
                        imageData = ByteString.CopyFrom(memoryStream.ToArray());
                    }
                }

                var grpcRequest = new UpdateProductRequest
                {
                    Id = id,
                    Name = apiRequest.Name ?? string.Empty,
                    Description = apiRequest.Description ?? string.Empty,
                    Price = apiRequest.Price ?? 0,
                    Category = apiRequest.Category ?? string.Empty,
                    NewImageData = imageData
                };

                var response = await _productClient.UpdateProductAsync(grpcRequest);
                return Ok(response.Product);
            }
            catch (RpcException ex)
            {
                return GrpcErrorMapper.MapGrpcErrorToHttp(ex);
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> DeleteProduct(string id)
        {
            try
            {
                var response = await _productClient.DeleteProductAsync(new DeleteProductRequest { Id = id });
                return Ok(response.Product);
            }
            catch (RpcException ex)
            {
                return GrpcErrorMapper.MapGrpcErrorToHttp(ex);
            }
        }
    }
}
