# Censudex - API Gateway

Punto de entrada único y centralizado para todos los microservicios de la plataforma de retail Censudex. Este gateway actúa como intermediario entre los clientes externos y los servicios backend, manejando autenticación, enrutamiento, balanceo de carga y traducción de protocolos HTTP/gRPC.

## Arquitectura y Patrón de Diseño

### Arquitectura: API Gateway Pattern con Load Balancing

El API Gateway implementa el patrón Gateway Aggregation y Gateway Routing, actuando como punto de entrada centralizado que orquesta las comunicaciones entre clientes y microservicios.
```mermaid
flowchart TD
    Client[Cliente/Frontend
    HTTP/REST] --> LB[NGINX
    Load Balancer]
    
    LB --> Gateway[API Gateway
    ASP.NET Core]
    
    subgraph Gateway_Layer
        Gateway --> Router[Request Router]
        Gateway --> Translator[Protocol Translator
        HTTP ↔ gRPC]
    end
    
    subgraph Microservices
        Router -->|HTTP| AuthSvc[Auth Service
        HTTP:5237]
        Translator -->|gRPC| ProductsSvc[Products Service
        gRPC:50051]
        Translator -->|gRPC| InventorySvc[Inventory Service
        gRPC:5001]
        Translator -->|gRPC| OrdersSvc[Orders Service
        gRPC:50052]
    end
```

### Patrones de Diseño Implementados:

1. **Gateway Aggregation Pattern:** Agrega múltiples solicitudes a servicios backend
2. **Gateway Routing Pattern:** Enruta solicitudes al microservicio apropiado
3. **Gateway Offloading Pattern:** Centraliza autenticación y autorización
4. **Load Balancing Pattern:** Distribución de carga mediante NGINX

## Tecnologías Utilizadas

- **Framework:** ASP.NET Core 9.0
- **Load Balancer:** NGINX
- **Comunicación:**
  - **HTTP/REST:** Auth Service y clientes externos
  - **gRPC:** Comunicación interna con microservicios
- **Autenticación:** JWT Bearer Token
- **gRPC Client:** Grpc.Net.Client
- **HTTP Client:** HttpClientFactory
- **Control de Versiones:** Git con Conventional Commits

## Responsabilidades del Gateway

### 1. Enrutamiento de Solicitudes
Dirige cada request al microservicio correspondiente basándose en la ruta del endpoint.

### 2. Autenticación y Autorización Centralizada
- Valida tokens JWT en cada solicitud protegida
- Comunica con Auth Service para validación de tokens
- Maneja roles (cliente/administrador) y permisos
- Protege endpoints según nivel de acceso

### 3. Traducción de Protocolos
- **Cliente → Gateway:** HTTP/REST
- **Gateway → Microservicios:** gRPC
- **Gateway → Cliente:** HTTP/REST
- Convierte requests HTTP a llamadas gRPC
- Traduce responses gRPC a formato HTTP/JSON

### 4. Balanceo de Carga con NGINX
- Distribuye carga entre múltiples instancias del gateway
- Health checks automáticos
- Failover en caso de caída de instancia
- Algoritmo: Least Connections

### 5. Manejo de Errores y Resiliencia
- Respuestas HTTP estandarizadas
- Códigos de estado correctos (200, 400, 401, 403, 404, 500)

## Endpoints Expuestos

### Autenticación (HTTP → Auth Service HTTP)

| Método | Endpoint | Destino | Descripción | Autenticación |
|--------|----------|---------|-------------|---------------|
| `POST` | `/api/auth/login` | Auth Service | Iniciar sesión | No |
| `GET` | `/api/auth/validate-token` | Auth Service | Validar token (interno) | No |
| `POST` | `/api/auth/logout` | Auth Service | Cerrar sesión | Sí |


### Productos (HTTP → Products Service gRPC)

| Método | Endpoint | Destino | Descripción | Autenticación |
|--------|----------|---------|-------------|---------------|
| `POST` | `/api/products` | Products gRPC | Crear producto | Sí (Admin) |
| `GET` | `/api/products` | Products gRPC | Listar productos | No |
| `GET` | `/api/products/{id}` | Products gRPC | Obtener producto por ID | No |
| `PATCH` | `/api/products/{id}` | Products gRPC | Actualizar producto | Sí (Admin) |
| `DELETE` | `/api/products/{id}` | Products gRPC | Eliminar producto (soft) | Sí (Admin) |

### Inventario (HTTP → Inventory Service gRPC)

| Método | Endpoint | Destino | Descripción | Autenticación |
|--------|----------|---------|-------------|---------------|
| `GET` | `/api/inventory` | Inventory gRPC | Listar inventario | Sí (Admin) |
| `GET` | `/api/inventory/{productId}` | Inventory gRPC | Consultar stock por producto | Sí |
| `PATCH` | `/api/inventory/{productId}` | Inventory gRPC | Actualizar stock | Sí (Admin) |

### Pedidos (HTTP → Orders Service gRPC)

| Método | Endpoint | Destino | Descripción | Autenticación |
|--------|----------|---------|-------------|---------------|
| `POST` | `/api/orders` | Orders gRPC | Crear pedido | Sí |
| `GET` | `/api/orders/history/{userId}` | Orders gRPC | Listar pedidos del usuario | Sí |
| `GET` | `/api/orders/` | Orders gRPC | Obtener todos los pedidos con o sin filtros| Sí |
| `PATCH` | `/api/orders/{orderId}/status` | Orders gRPC | Actualizar estado de pedido | Sí (Admin) |
| `PATCH` | `/api/orders/{idOrTrackingNumber}` | Orders gRPC | Cancelar pedido | Sí(Ambos roles) |
| `GET` | `/api/orders/{trackingNumber}/status` | Orders gRPC | Obtener estado del pedido | Sí |

## Instalación y Configuración

### Requisitos Previos

- **.NET 9 SDK**: [Download](https://dotnet.microsoft.com/download/dotnet/9.0)
- **Docker Desktop**: [Download](https://www.docker.com/products/docker-desktop)
- **Visual Studio 2022** o **VS Code**: [Download](https://code.visualstudio.com/)
- **Todos los microservicios deben estar corriendo**:
  - Auth Service (puerto 5237)
  - Products Service (puerto 50051)
  - Inventory Service (puerto 5001)
  - Orders Service (puerto 50052)

### 1. Clonar el Repositorio
```bash
git clone https://github.com/Censudex/censudex-api-gateway.git

cd censudex-api-gateway
```

### 2. Configurar Variables de Entorno
Crea un archivo `.env` en la raíz del proyecto (mismo nivel que `AuthService.csproj`):
```env
# Microservices URLs (gRPC)
INVENTORY_SERVICE_URL=http://localhost:5001

# JWT Configuration
JWT_SECRET=my-super-secret-key-at-least-32-characters-long-for-jwt-validation!
JWT_ISSUER=CensudexAPIGateway
JWT_AUDIENCE=CensudexClients
JWT_EXPIRATION_MINUTES=60

# Auth Service URL (HTTP)
AUTH_SERVICE_URL=http://localhost:5237

# Admin User Credentials
ADMIN_ROLE=ADMIN
ADMIN_EMAIL=admin@censudex.com
ADMIN_PASSWORD=Admin123

# Client User Credentials
CLIENT_ROLE=CLIENT
CLIENT_EMAIL=client@gmail.com
CLIENT_PASSWORD=client123
```

### 3. Instalar Dependencias
```bash
dotnet restore
```

### 4. Copiar Archivos .proto
(En caso de que no se esten incluidos los archivos .proto de los microservicios)
Debes copiar los archivos `.proto` de cada microservicio a la carpeta `Protos/`:
```bash
# Estructura esperada
Protos/
├── clients.proto      # Copiado del Clients Service
├── products.proto     # Copiado del Products Service
├── inventory.proto    # Copiado del Inventory Service
└── orders.proto       # Copiado del Orders Service
```

**Asegúrate de configurar en APIGateway.csproj:**
```xml
<ItemGroup>
  <Protobuf Include="Protos\clients.proto" GrpcServices="Client" />
  <Protobuf Include="Protos\products.proto" GrpcServices="Client" />
  <Protobuf Include="Protos\inventory.proto" GrpcServices="Client" />
  <Protobuf Include="Protos\orders.proto" GrpcServices="Client" />
</ItemGroup>
```

### 5. Compilar el Proyecto
```bash
dotnet build
```
### 6. Crear las instancias de NGINX y del Gateway con Docker Compose
```bash
docker-compose up -d
```

### 7. Ejecutar el Proyecto

**Modo Desarrollo (sin Docker):**
```bash
dotnet run # instancia unica del gateway

dotnet run --urls "http://localhost:6001" # instancia 2 del gateway
dotnet run --urls "http://localhost:6002" # instancia 3 del gateway
dotnet run --urls "http://localhost:6003" # instancia 4 del gateway
```

El API Gateway estará disponible en:
- **HTTP:** http://localhost:5152
- **Swagger UI:** http://localhost:5152/swagger

## Ejemplos de Uso

### Authservice Endpoints

### 1. Login (POST /api/login)

**Nota importante:** : Dado que no tenemos servico de clientes solo esixten dos posibles usarios administrador y cliente, sus credenciales estan en el archivo .env

**Request Body:**
```json
{
  "email": "admin@censudex.com",
  "password": "Admin123"
}
```

**Response (200):** 
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjEyMzQ1IiwiZ2l2ZW5fbmFtZSI6IkFkbWluIiwiZW1haWwiOiJhZG1pbkBjZW5zdWRleC5jb20iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBRE1JTiIsImp0aSI6ImEzZTI1MTIwLWQ1YTEtNDhjNy05Mjg3LTBjNjI0NTMyYWIyMyIsImlhdCI6IjE3NjMyMTc2MjkiLCJleHAiOjE3NjMyMjEyMjksImlzcyI6IkNlbnN1ZGV4QVBJR2F0ZXdheSIsImF1ZCI6IkNlbnN1ZGV4Q2xpZW50cyJ9.j2DGkJmMwaGy0xeg9O134gT8shsLA0ESIj6oFFqIEY4",
  "user": {
    "id": "12345",
    "email": "admin@censudex.com",
    "username": "Admin",
    "role": "ADMIN"
  }
}
```
### 2. Validar Token (GET /api/validate-token)
**Request Body:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjEyMzQ1IiwiZ2l2ZW5fbmFtZSI6IkFkbWluIiwiZW1haWwiOiJhZG1pbkBjZW5zdWRleC5jb20iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBRE1JTiIsImp0aSI6ImEzZTI1MTIwLWQ1YTEtNDhjNy05Mjg3LTBjNjI0NTMyYWIyMyIsImlhdCI6IjE3NjMyMTc2MjkiLCJleHAiOjE3NjMyMjEyMjksImlzcyI6IkNlbnN1ZGV4QVBJR2F0ZXdheSIsImF1ZCI6IkNlbnN1ZGV4Q2xpZW50cyJ9.j2DGkJmMwaGy0xeg9O134gT8shsLA0ESIj6oFFqIEY4"
}
```

**Response (200):**
```json
{
  "isValid": true,
  "message": "Token is valid",
  "userId": null,
  "username": null,
  "role": "ADMIN"
}
```
### 3. Logout (POST /api/logout)

**Request Body:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjEyMzQ1IiwiZ2l2ZW5fbmFtZSI6IkFkbWluIiwiZW1haWwiOiJhZG1pbkBjZW5zdWRleC5jb20iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBRE1JTiIsImp0aSI6ImEzZTI1MTIwLWQ1YTEtNDhjNy05Mjg3LTBjNjI0NTMyYWIyMyIsImlhdCI6IjE3NjMyMTc2MjkiLCJleHAiOjE3NjMyMjEyMjksImlzcyI6IkNlbnN1ZGV4QVBJR2F0ZXdheSIsImF1ZCI6IkNlbnN1ZGV4Q2xpZW50cyJ9.j2DGkJmMwaGy0xeg9O134gT8shsLA0ESIj6oFFqIEY4"
}
```

**Response (200):**
```json
{
  "success": true,
  "message": "Session closed successfully"
}
```

### Productsservice Endpoints

#### 1. Crear Producto (POST /api/products) (form-data)

**Nota:** Solo accesible para administradores.

**Request Body**
```bash
curl -X POST "http://localhost:5152/api/products" \
  -H "Authorization: Bearer TU_TOKEN_AQUI" \
  -H "Accept: application/json" \
  -F "Name=Televisor LG 55\"" \
  -F "Description=Pantalla 4K Ultra HD" \
  -F "Price=450000" \
  -F "Category=Tecnologia" \
  -F "ImageFile=@./imagen.jpg"
```

**Response (200):**
```json
{
    "id": "a1bceff2-5cea-4394-a88b-004cfbdb4792",
    "name": "Televisor LG 55",
    "description": "Pantalla 4K Ultra HD",
    "price": 450000,
    "category": "Tecnologia",
    "imageUrl": "https://res.cloudinary.com/censudex-products-service/image/upload/v1763296401/censudex-products/trhlmyvsdiaktqao8oqv.png",
    "status": "active",
    "createdAt": "2025-11-16T12:33:28.417Z"
}
```

#### 2. Listar Productos (GET /api/products)

**Nota:** Solo accesible para clientes.

**Request Body**
```json
{
}
```

**Response (200):**
```json
[
    {
        "id": "ebe9f8da-3f0d-4c4e-bdcb-3847206ca548",
        "name": "Producto Pixel Prueba",
        "description": "Probando la corrección de camelCase",
        "price": 15000,
        "category": "Pruebas",
        "imageUrl": "https://res.cloudinary.com/censudex-products-service/image/upload/v1761016523/censudex-products/p7j8aggxrfetebhd1vne.gif",
        "status": "inactive",
        "createdAt": "2025-10-21T03:10:48.014Z"
    },
    {
        "id": "a1bceff2-5cea-4394-a88b-004cfbdb4792",
        "name": "Televisor LG 55",
        "description": "Pantalla 4K Ultra HD",
        "price": 450000,
        "category": "Tecnologia",
        "imageUrl": "https://res.cloudinary.com/censudex-products-service/image/upload/v1763296401/censudex-products/trhlmyvsdiaktqao8oqv.png",
        "status": "active",
        "createdAt": "2025-11-16T12:33:28.417Z"
    }
]
```

#### 3. Obtener Producto por ID (GET /api/products/{id})

**Nota:** Solo accesible para clientes.

**Request Body**
```json
{
}
```

**Response (200):**
```json
{
    "id": "a1bceff2-5cea-4394-a88b-004cfbdb4792",
    "name": "Televisor LG 55",
    "description": "Pantalla 4K Ultra HD",
    "price": 450000,
    "category": "Tecnologia",
    "imageUrl": "https://res.cloudinary.com/censudex-products-service/image/upload/v1763296401/censudex-products/trhlmyvsdiaktqao8oqv.png",
    "status": "active",
    "createdAt": "2025-11-16T12:33:28.417Z"
}
```

#### 4. Actualizar Producto (PATCH /api/products/{id})

**Nota:** Solo accesible para administradores.

**Request Body**
```bash
curl -X PATCH "http://localhost:5000/api/products/ID_DEL_PRODUCTO" \
  -H "Authorization: Bearer TU_TOKEN_AQUI" \
  -H "Accept: application/json" \
  -F "Name=Nuevo Nombre" \
  -F "Description=Nueva descripción" \
  -F "Price=99999" \
  -F "Category=NuevaCategoria" \
  -F "NewImageFile=@./nueva-imagen.jpg"
```

**Response (200):**
```json
{
    "id": "a1bceff2-5cea-4394-a88b-004cfbdb4792",
    "name": "Nuevo Nombre",
    "description": "Nueva descripción",
    "price": 99999,
    "category": "NuevaCategoria",
    "imageUrl": "https://res.cloudinary.com/censudex-products-service/image/upload/v1763296401/censudex-products/trhlmyvsdiaktqao8oqv.png",
    "status": "active",
    "createdAt": "2025-11-16T12:33:28.417Z"
}
```


#### 5. Eliminar Producto (DELETE /api/products/{id})

**Nota:** Solo accesible para administradores.

**Request Body**
```json
{
}
```

**Response (200):**
```json
{
    "id": "a1bceff2-5cea-4394-a88b-004cfbdb4792",
    "name": "Televisor LG 55",
    "description": "Pantalla 4K Ultra HD",
    "price": 450000,
    "category": "Tecnologia",
    "imageUrl": "https://res.cloudinary.com/censudex-products-service/image/upload/v1763296401/censudex-products/trhlmyvsdiaktqao8oqv.png",
    "status": "inactive",
    "createdAt": "2025-11-16T12:33:28.417Z"
}
```

### Inventoryservice Endpoints

#### 1. Listar Inventario (GET /api/inventory)

**Nota:** Solo accesible para administradores.**

**Response (200):**
```json
{
  "items": [
    {
      "productId": "019a83e7-ca92-71e7-81b3-edaac53906b0",
      "productName": "Bluetooth Speaker",
      "productStock": 220,
      "productStatus": false
    },
    {
      "productId": "019a83e7-ca92-7262-af39-285d7b3abcbe",
      "productName": "USB-C Hub",
      "productStock": 250,
      "productStatus": false
    }
  ]
}
```

#### 2. Consultar Stock por Producto (GET /api/inventory/{productId})

**Response (200):**
```json
{
  "item": {
    "productId": "019a83e7-ca92-71e7-81b3-edaac53906b0",
    "productName": "Bluetooth Speaker",
    "productCategory": "Audio",
    "productStock": 220,
    "productStatus": false,
    "thresholdLimit": 22,
    "createdAt": "2025-11-14T19:46:37.3436960Z",
    "updatedAt": ""
  }
}
```
#### 3. Actualizar Stock (PATCH /api/inventory/{productId})

**Request Body:**
```json
{
  "operation": "increase",
  "quantity": 50
}
```

**Response (200):**
```json
{
  "item": {
    "success": true,
    "message": "Stock updated successfully.",
    "productId": "019a83e7-ca92-71e7-81b3-edaac53906b0",
    "productName": "Bluetooth Speaker",
    "productCategory": "Audio",
    "previousStock": 220,
    "updatedStock": 270,
    "operation": "increase",
    "quantityChanged": 50,
    "alert": ""
  }
}
```

### Ordersservice Endpoints

#### 1. Crear Pedido (POST /api/orders)

**Request Body:**
```json
{
  "userId": "a6c4b1f2-98ef-4b7f-b1ef-33b4ff49d87f",
  "clientName": "Nicolás Galleguillos",
  "shippingAddress": "Av. Los Robles 1234, Santiago",
  "items": [
    { "productId": "019a64c4-7188-7fc6-82ee-6759fc681b16", "quantity": 2, "price": 25.5 },
    { "productId": "019a64c4-71b3-728b-8d1c-acff4c707ae5", "quantity": 1, "price": 10.0 }
  ]
}

```

**Response (200):**
```json
{
    "message": "Orden creada exitosamente",
    "order": {
        "id": "e25d598f-0ca9-4b9f-ad25-fa0fc3b200fc",
        "userId": "a6c4b1f2-98ef-4b7f-b1ef-33b4ff49d87f",
        "clientName": "Nicolás Galleguillos",
        "totalAmount": 61,
        "status": "pendiente",
        "trackingNumber": "TRK-HTBTVWAOCY",
        "shippingAddress": "Av. Los Robles 1234, Santiago",
        "createdAt": "Sun Nov 16 2025 01:29:08 GMT+0000 (Coordinated Universal Time)",
        "items": [
            {
                "id": "55555fe1-1f26-4ddc-89f9-00f3db7b490e",
                "orderId": "e25d598f-0ca9-4b9f-ad25-fa0fc3b200fc",
                "productId": "019a64c4-7188-7fc6-82ee-6759fc681b16",
                "quantity": 2,
                "price": 25.5
            },
            {
                "id": "4d854c3a-31d8-4b39-b4ef-cb42c9473e75",
                "orderId": "e25d598f-0ca9-4b9f-ad25-fa0fc3b200fc",
                "productId": "019a64c4-71b3-728b-8d1c-acff4c707ae5",
                "quantity": 1,
                "price": 10
            }
        ]
    }
}
```

#### 2. Listar Pedidos del Usuario (GET /api/orders/history/{userId})

**Request Body:**
```json
{
}
```

**Response (200):**
```json
[
    {
        "id": "b34562bf-5e16-4029-be74-237d0bc18ad5",
        "userId": "a6c4b1f2-98ef-4b7f-b1ef-33b4ff49d87f",
        "clientName": "Nicolás Galleguillos",
        "totalAmount": 61,
        "status": "pendiente",
        "trackingNumber": "TRK-WOB9MJTCGZ",
        "shippingAddress": "Av. Los Robles 1234, Santiago",
        "createdAt": "Sun Nov 16 2025 01:32:06 GMT+0000 (Coordinated Universal Time)",
        "items": [
            {
                "id": "6d5000e1-4340-431b-b584-e37aa5dee66f",
                "orderId": "b34562bf-5e16-4029-be74-237d0bc18ad5",
                "productId": "019a64c4-71b3-728b-8d1c-acff4c707ae5",
                "quantity": 1,
                "price": 10
            },
            {
                "id": "e36d5f5d-b38a-4845-a11f-41d03bc3e7e8",
                "orderId": "b34562bf-5e16-4029-be74-237d0bc18ad5",
                "productId": "019a64c4-7188-7fc6-82ee-6759fc681b16",
                "quantity": 2,
                "price": 25.5
            }
        ]
    },
    {
        "id": "e25d598f-0ca9-4b9f-ad25-fa0fc3b200fc",
        "userId": "a6c4b1f2-98ef-4b7f-b1ef-33b4ff49d87f",
        "clientName": "Nicolás Galleguillos",
        "totalAmount": 61,
        "status": "cancelado",
        "trackingNumber": "TRK-HTBTVWAOCY",
        "shippingAddress": "Av. Los Robles 1234, Santiago",
        "createdAt": "Sun Nov 16 2025 01:29:08 GMT+0000 (Coordinated Universal Time)",
        "items": [
            {
                "id": "4d854c3a-31d8-4b39-b4ef-cb42c9473e75",
                "orderId": "e25d598f-0ca9-4b9f-ad25-fa0fc3b200fc",
                "productId": "019a64c4-71b3-728b-8d1c-acff4c707ae5",
                "quantity": 1,
                "price": 10
            },
            {
                "id": "55555fe1-1f26-4ddc-89f9-00f3db7b490e",
                "orderId": "e25d598f-0ca9-4b9f-ad25-fa0fc3b200fc",
                "productId": "019a64c4-7188-7fc6-82ee-6759fc681b16",
                "quantity": 2,
                "price": 25.5
            }
        ]
    }
]
```

#### 3. Obtener Pedido con o sin filtros (GET /api/orders?startDate=10/10/2026&endDate=12/12/2027)

**Request Body:**
```json
{
}
```

**Response (200):**
```json
{
}
```

#### 4. Obtener estado del pedido (GET /api/orders/{trackingNumber}/status)

**Request Body:**
```json
{
}
```

**Response (200):**
```json
{
    "trackingNumber": "TRK-HTBTVWAOCY",
    "status": "pendiente"
}
```

#### 5. Cancelar pedido (PATCH /api/orders/{idOrTrackingNumber})

**Request Body:**
```json
{
""
}
```

**Response (200):**
```json
{
    "success": true,
    "message": "Pedido cancelado por usuario."
}
```

#### 6. Actualizar estado del pedido (PATCH /api/orders/{orderId}/status)

**Request Body:**
```json
{
  "status": "en procesamiento"
}
```

**Response (200):**
```json
{
    "success": true
}
```

## Manejo de Errores

### Códigos de Estado HTTP

| Código | Descripción | Ejemplo |
|--------|-------------|---------|
| **200** | OK | Operación exitosa |
| **201** | Created | Recurso creado exitosamente |
| **400** | Bad Request | Datos de entrada inválidos |
| **401** | Unauthorized | Token no proporcionado o inválido |
| **403** | Forbidden | Sin permisos (rol insuficiente) |
| **404** | Not Found | Recurso no encontrado |
| **500** | Internal Server Error | Error del servidor/microservicio |
| **503** | Service Unavailable | Microservicio no disponible |


