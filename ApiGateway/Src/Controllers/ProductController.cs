using ApiGateway.Protos.Product;
using ApiGateway.Src.DTOs;
using ApiGateway.Src.Extensions;
using Google.Protobuf;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiGateway.Src.Controllers
{
    /// <summary>
    /// Controlador que expone endpoints HTTP para operaciones sobre productos.
    /// Actúa como API Gateway y delega la lógica al servicio gRPC de productos.
    /// </summary>
    [ApiController]
    [Route("api/products")]
    public class ProductController : ControllerBase
    {
        /// <summary>
        /// Cliente gRPC para comunicarse con el servicio de productos.
        /// </summary>
        private readonly ProductsService.ProductsServiceClient _productClient;

        /// <summary>
        /// Crea una instancia del controlador con el cliente gRPC inyectado.
        /// </summary>
        /// <param name="productClient">Cliente gRPC del servicio de productos.</param>
        public ProductController(ProductsService.ProductsServiceClient productClient)
        {
            _productClient = productClient;
        }
    
        /// <summary>
        /// Crea un nuevo producto en el servicio de productos.
        /// Convierte el DTO recibido en la petición gRPC correspondiente y devuelve el producto creado.
        /// </summary>
        /// <param name="apiRequest">DTO con los datos del producto a crear.</param>
        /// <returns>200 OK con el producto creado o el error correspondiente.</returns>
        /// <response code="200">Producto creado exitosamente.</response>
        /// <response code="400">Solicitud inválida.</response>
        /// <response code="500">Error interno del servidor.</response>
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

        /// <summary>
        /// Obtiene todos los productos del servicio de productos.
        /// </summary>
        /// <returns>200 OK con la lista de productos o el error correspondiente.</returns>
        /// <response code="200">Lista de productos obtenida exitosamente.</response>
        /// <response code="500">Error interno del servidor.</response>
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

        /// <summary>
        /// Obtiene un producto por su ID del servicio de productos.
        /// </summary>
        /// <param name="id">ID del producto a obtener.</param>
        /// <returns>200 OK con el producto o el error correspondiente.</returns>
        /// <response code="200">Producto obtenido exitosamente.</response>
        /// <response code="404">Producto no encontrado.</response>
        /// <response code="500">Error interno del servidor.</response>
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

        /// <summary>
        /// Actualiza un producto existente en el servicio de productos.
        /// </summary>
        /// <param name="id">ID del producto a actualizar.</param>
        /// <param name="apiRequest">DTO con los datos del producto a actualizar.</param>
        /// <returns>200 OK con el producto actualizado o el error correspondiente.</returns>
        /// <response code="200">Producto actualizado exitosamente.</response>
        /// <response code="404">Producto no encontrado.</response>
        /// <response code="500">Error interno del servidor.</response>
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

        /// <summary>
        /// Elimina un producto del servicio de productos.
        /// </summary>
        /// <param name="id">ID del producto a eliminar.</param>
        /// <returns>200 OK con el producto eliminado o el error correspondiente.</returns>
        /// <response code="200">Producto eliminado exitosamente.</response>
        /// <response code="404">Producto no encontrado.</response>
        /// <response code="500">Error interno del servidor.</response>
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
