using System.Text;
using ApiGateway.Protos.Inventory;
using ApiGateway.Protos.Product;
using ApiGateway.Protos.Order;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.HttpOverrides;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "API Gateway",
        Version = "v1",
        Description = "API Gateway for microservices using gRPC and HTTP"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token in the text input below.\nExample: Bearer {your token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET") 
    ?? throw new InvalidOperationException("JWT_SECRET no está configurado");

var keyBytes = Encoding.UTF8.GetBytes(jwtSecret);

if (keyBytes.Length < 32)
{
    throw new InvalidOperationException(
        $"JWT_SECRET debe tener al menos 32 caracteres. Actual: {keyBytes.Length}");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER"),
            ValidAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE"),
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes)
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddGrpcClient<InventoryService.InventoryServiceClient>(options =>
{
    options.Address = new Uri(Environment.GetEnvironmentVariable("INVENTORY_SERVICE_URL") ?? "https://localhost:5001");
});

builder.Services.AddGrpcClient<ProductsService.ProductsServiceClient>(options =>
{
    options.Address = new Uri(Environment.GetEnvironmentVariable("PRODUCT_SERVICE_URL") ?? "http://localhost:50051");
});

builder.Services.AddGrpcClient<OrderService.OrderServiceClient>(options =>
{
    options.Address = new Uri(Environment.GetEnvironmentVariable("ORDER_SERVICE_URL") ?? "http://localhost:50052");
});

builder.Services.AddHttpClient("AuthService", client =>
{
    client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("AUTH_SERVICE_URL") 
        ?? "http://localhost:5237");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | 
                              ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();