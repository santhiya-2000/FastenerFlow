# FastenerFlow

Enterprise inventory and order management application demonstrating C#, ASP.NET Core Web API, Microsoft SQL Server, Entity Framework Core, REST APIs, and Microsoft Dynamics 365 Business Central integration concepts.

## Overview

FastenerFlow is a production-style portfolio project designed to demonstrate enterprise application development patterns. It features a complete inventory and order management system for a fictional fastener distribution company, with mock ERP integration that simulates Microsoft Dynamics 365 Business Central.

## Architecture

```
┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│   React Frontend│────▶│  ASP.NET Core   │────▶│   SQL Server    │
│   (Vite + TS)   │     │     Web API     │     │   (Docker)      │
└─────────────────┘     └─────────────────┘     └─────────────────┘
                              │
                              ▼
                       ┌─────────────────┐
                       │  Mock ERP API   │
                       │  (Simulated)    │
                       └─────────────────┘
```

### Layered Architecture

**Controllers** → **Services** → **Repositories** → **Entity Framework Core** → **SQL Server**

- **Controllers**: Handle HTTP requests/responses, input validation
- **Services**: Business logic, transaction management, orchestration
- **Repositories**: Data access abstraction, EF Core operations
- **Entity Framework Core**: ORM, migrations, change tracking
- **SQL Server**: Persistent data storage with constraints and indexes

### Project Structure

```
FastenerFlow/
├── FastenerFlow.Api/          # ASP.NET Core Web API
│   ├── Controllers/           # API endpoints
│   └── Program.cs             # DI configuration and middleware
├── FastenerFlow.Core/         # Business logic and data layer
│   ├── Data/                  # DbContext and migrations
│   ├── DTOs/                  # Data transfer objects
│   ├── Integrations/         # ERP sync service
│   ├── Models/                # Domain entities
│   ├── Repositories/          # Data access layer
│   └── Services/             # Business logic layer
├── FastenerFlow.MockErp/      # Mock ERP API (simulated)
│   └── Program.cs             # Inventory endpoints
├── FastenerFlow.Tests/        # xUnit test suite
├── frontend/                  # React + TypeScript frontend
│   ├── src/
│   │   ├── api/               # API client (Axios)
│   │   └── pages/             # React components
│   └── tailwind.config.js
├── docker-compose.yml         # SQL Server container
└── README.md
```

## Technology Stack

### Backend
- **C# (.NET 8.0 LTS)** - Modern, cross-platform language
- **ASP.NET Core Web API** - RESTful API framework
- **Entity Framework Core 8.0.11** - ORM with migrations
- **Microsoft SQL Server 2022** - Enterprise-grade database
- **Dependency Injection** - Built-in DI container
- **Swagger/OpenAPI** - Auto-generated API documentation
- **HttpClientFactory** - Resilient HTTP client management

### Frontend
- **React with Vite** - Fast build tool and dev server
- **TypeScript** - Type-safe JavaScript
- **Tailwind CSS** - Utility-first CSS framework
- **Axios** - HTTP client with interceptors
- **React Router** - Client-side routing

### Database
- **Microsoft SQL Server 2022** - Production database
- **Entity Framework Core migrations** - Database versioning
- **Docker Compose** - Local SQL Server containerization

### Development & Testing
- **Docker Compose** - Local SQL Server container
- **xUnit** - Unit testing framework
- **Moq** - Mocking framework for tests
- **EF Core InMemory** - In-memory database for testing

## Prerequisites

1. **.NET 8.0 SDK** - Install from https://dotnet.microsoft.com/download
2. **Docker Desktop** - Install from https://www.docker.com/products/docker-desktop (required for SQL Server on macOS)
3. **Node.js 18+** (for frontend) - Install from https://nodejs.org

## Quick Start

### 1. Clone and Setup

```bash
cd FastenerFlow
```

### 2. Install .NET SDK (if not already installed)

```bash
# macOS (Apple Silicon)
curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --install-dir ~/.dotnet --channel 8.0
export PATH="$HOME/.dotnet:$PATH"
```

### 3. Start SQL Server with Docker

```bash
docker compose up -d
```

This starts SQL Server 2022 with:
- Port: 1433
- Database: FastenerFlowDb
- User: sa
- Password: FastenerFlow123!

### 4. Run Database Migrations

```bash
dotnet dotnet-ef database update --project FastenerFlow.Core --startup-project FastenerFlow.Api
```

### 5. Run Backend Services

In separate terminals:

```bash
# Terminal 1: Main API
cd FastenerFlow.Api
dotnet run

# Terminal 2: Mock ERP API
cd FastenerFlow.MockErp
dotnet run
```

### 6. Run Frontend

```bash
cd frontend
npm install
npm run dev
```

The frontend will be available at http://localhost:5173

### 7. Access Swagger Documentation

- Main API: https://localhost:5001/swagger
- Mock ERP: https://localhost:5003/swagger

## Setup Instructions

### 1. Install .NET SDK

The project uses .NET 8.0. If not already installed:

```bash
# macOS (Intel)
brew install dotnet-sdk

# macOS (Apple Silicon) - if Homebrew fails, use script:
curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --install-dir ~/.dotnet --channel 8.0
export PATH="$HOME/.dotnet:$PATH"
```

### 2. Install Docker Desktop

Docker Desktop is required to run SQL Server on macOS.

1. Download Docker Desktop for Mac from https://www.docker.com/products/docker-desktop
2. Install and start Docker Desktop
3. Verify installation: `docker --version`

### 3. Start SQL Server

```bash
docker compose up -d
```

This starts SQL Server 2022 with:
- Port: 1433
- Database: FastenerFlowDb
- User: sa
- Password: FastenerFlow123!

### 4. Install EF Core Tools

```bash
dotnet tool install --global dotnet-ef --version 8.0.11
export PATH="$PATH:$HOME/.dotnet/tools"
```

Or use local tools (already configured in this project):

```bash
dotnet dotnet-ef migrations add InitialCreate --project FastenerFlow.Core --startup-project FastenerFlow.Api
```

### 5. Run Database Migrations

```bash
dotnet dotnet-ef database update --project FastenerFlow.Core --startup-project FastenerFlow.Api
```

### 6. Run the API

```bash
cd FastenerFlow.Api
dotnet run
```

The API will be available at https://localhost:5001 with Swagger UI at https://localhost:5001/swagger

### 7. Run the Mock ERP API

```bash
cd FastenerFlow.MockErp
dotnet run
```

The Mock ERP API will be available at https://localhost:5002 with Swagger UI at https://localhost:5002/swagger

### 8. Run the Frontend (Phase 4)

```bash
cd frontend
npm install
npm run dev
```

## API Endpoints

### Products
- `GET /api/products` - Get all products
- `GET /api/products/{id}` - Get product by ID
- `GET /api/products/low-stock` - Get products at or below reorder level
- `POST /api/products` - Create new product (requires validation)
- `PUT /api/products/{id}` - Update existing product
- `DELETE /api/products/{id}` - Delete product

**Validation Rules:**
- SKU: Required, max 50 characters, unique
- Name: Required, max 100 characters
- Category: Required, max 50 characters
- Unit Price: Required, positive decimal
- Quantity: Required, non-negative integer
- Reorder Level: Required, non-negative integer

### Orders
- `GET /api/orders` - Get all orders with items
- `GET /api/orders/{id}` - Get order by ID with details
- `POST /api/orders` - Create new order

**Business Rules:**
- Order must contain at least one item
- Each item quantity must be positive
- Total quantity per product cannot exceed available stock
- Order creation and inventory deduction are atomic (transactional)
- Failing orders do not modify inventory

### ERP Integration (Mock)
- `POST /api/erp/sync` - Trigger synchronization with mock ERP
- `GET /api/erp/sync-history` - Get historical sync records

**Sync Behavior:**
- Matches products by SKU
- Updates quantities for matching products
- Records discrepancies for analysis
- Logs all sync attempts (success/failure)
- Idempotent - safe to run multiple times

## Business Rules

### Order Processing
- Quantity must be positive
- Orders cannot exceed available stock
- Successfully placing an order reduces inventory
- Order creation and inventory updates occur in one database transaction
- Concurrent orders must not oversell stock (optimistic concurrency via row version)
- Invalid orders must not modify inventory

### Inventory Management
- Products have SKU, name, category, unit price, quantity, and reorder level
- Low stock is defined as quantity ≤ reorder level
- Product IDs are auto-generated

## Key Design Decisions

### Layered Architecture
Using Controllers → Services → Repositories pattern provides:
- **Clear separation of concerns** - Each layer has a single responsibility
- **Testable business logic** - Services can be unit tested in isolation
- **Easy to swap implementations** - Repositories can be replaced (e.g., for caching)
- **Single Responsibility Principle** - Follows SOLID principles

### Transaction Management
Orders use explicit database transactions to ensure:
- **Atomic inventory updates** - All changes succeed or fail together
- **Rollback on any failure** - No partial order creation
- **Data consistency** - Inventory is never left in an invalid state

### Concurrency Control
Products use row version (byte[] Version) for optimistic concurrency:
- **Prevents lost updates** - Concurrent updates don't overwrite each other
- **No database locks on reads** - Better performance for read-heavy workloads
- **Efficient conflict detection** - DbUpdateConcurrencyException thrown on conflicts

### Database Design
- **Indexes** on frequently queried columns (SKU, Category, OrderDate, SyncDate)
- **Foreign key constraints** with appropriate cascade/restrict behaviors
- **Decimal precision** for monetary values (18,2)
- **Unique constraint** on SKU to prevent duplicates

### Mock ERP Pattern
The mock ERP demonstrates:
- **HttpClientFactory usage** - Proper HTTP client management
- **External API integration patterns** - REST API consumption
- **DTO mapping** - Translation between system boundaries
- **Error handling and logging** - Comprehensive error management
- **Idempotent sync operations** - Safe to retry without side effects

## Testing

Run backend tests with xUnit:

```bash
dotnet test FastenerFlow.Tests
```

### Test Coverage (13 tests)

**ProductServiceTests (4 tests):**
- CreateProduct_ValidProduct_ReturnsProduct
- GetLowStockProducts_WithLowStock_ReturnsProducts
- UpdateProduct_ValidProduct_UpdatesProduct
- DeleteProduct_ValidProduct_DeletesProduct

**OrderServiceTests (5 tests):**
- CreateOrder_ValidOrder_CreatesOrderAndDeductsStock
- CreateOrder_InsufficientStock_ThrowsException
- CreateOrder_InvalidQuantity_ThrowsException
- CreateOrder_NonExistentProduct_ThrowsException
- CreateOrder_TransactionRollbackOnFailure_PreservesStock

**ErpSyncServiceTests (4 tests):**
- SyncInventoryAsync_MatchingSkus_UpdatesQuantities
- SyncInventoryAsync_UnmatchedSku_RecordsFailure
- SyncInventoryAsync_ErpFailure_RecordsSyncLogAsFailed

### Test Strategy
- **InMemory database** for fast, isolated tests
- **Moq** for mocking external dependencies (HttpClient, ILogger)
- **Transaction warning suppression** for EF Core InMemory limitations
- **Arrangement-Act-Assert** pattern for clear test structure

## Development Workflow

1. **Make changes** to code
2. **Verify compilation**: `dotnet build`
3. **Run tests**: `dotnet test FastenerFlow.Tests`
4. **Run API**: `cd FastenerFlow.Api && dotnet run`
5. **Test endpoints** with Swagger UI at https://localhost:5001/swagger
6. **Run frontend**: `cd frontend && npm run dev`
7. **Test full flow** through the React dashboard

## Database Schema

### Products Table
```sql
- Id (PK, int, identity)
- Sku (unique, varchar(50))
- Name (varchar(100))
- Category (varchar(50))
- UnitPrice (decimal(18,2))
- Quantity (int)
- ReorderLevel (int)
- Version (rowversion, for concurrency)
```

### Orders Table
```sql
- Id (PK, int, identity)
- OrderDate (datetime)
- Status (varchar(50))
```

### OrderItems Table
```sql
- Id (PK, int, identity)
- OrderId (FK to Orders, cascade delete)
- ProductId (FK to Products, restrict delete)
- Quantity (int)
- UnitPrice (decimal(18,2))
```

### ErpSyncLogs Table
```sql
- Id (PK, int, identity)
- SyncDate (datetime)
- Status (varchar(50))
- ProductsUpdated (int)
- ProductsFailed (int)
- Details (nvarchar(max))
```

## Frontend Pages

### Dashboard
- Summary statistics (total products, inventory, low stock, orders)
- Last ERP sync status
- Quick navigation to other pages

### Products
- Searchable product table
- Add/Edit/Delete products
- Low stock highlighting (yellow background)
- Form validation

### Orders
- Create orders with multiple items
- View order history with details
- Real-time stock availability check
- Order status display

### ERP Integration
- Manual sync trigger button
- Sync result display (success/failure)
- Discrepancy list
- Sync history table
- Clear "Mock ERP" warning banner

## Phase Implementation Status

- [x] Phase 1: Solution structure, SQL Server configuration, data models, migrations, and product APIs
- [x] Phase 2: Order processing, transactions, and concurrency handling
- [x] Phase 3: Mock Business Central REST service and synchronization
- [x] Phase 4: React frontend and dashboard
- [x] Phase 5: Automated tests, API documentation, and README

## Notes

- This is a demonstration project for portfolio purposes
- Mock ERP simulates Business Central integration patterns
- All ERP integration is clearly labeled as simulated
- Designed to be explainable to a CTO in ~5 minutes
- All business logic is in C# backend for better demonstration of enterprise patterns

## Troubleshooting

### SQL Server Connection Issues
- Ensure Docker Desktop is running: `docker ps`
- Check SQL Server container logs: `docker logs fastenerflow-sqlserver`
- SQL Server may take 30-60 seconds to start on first run

### .NET SDK Not Found
- Add to PATH: `export PATH="$HOME/.dotnet:$PATH"`
- Make permanent by adding to `~/.zprofile` or `~/.bash_profile`

### EF Core Tools Not Found
- Install globally: `dotnet tool install --global dotnet-ef --version 8.0.11`
- Or use local tools: `dotnet dotnet-ef` (already configured)

### SSL Certificate Errors
- For development, the API uses self-signed certificates
- Frontend is configured to work with HTTPS
- If browser shows warning, proceed to the site (it's safe for local dev)

### Mock ERP Sync Fails
- Ensure Mock ERP API is running on https://localhost:5003
- Check that both APIs are using different ports
- Verify CORS is configured correctly (AllowAll policy)

## Connecting to Real Business Central

To replace the mock ERP with a real Microsoft Dynamics 365 Business Central integration:

1. **Authentication**: Replace the mock API calls with OAuth 2.0 authentication
   - Register an app in Azure AD
   - Use `Microsoft.Identity.Client` (MSAL) for token acquisition
   - Configure proper scopes for Business Central API

2. **API Endpoints**: Update the sync service to use real Business Central endpoints:
   - `GET /api/v2.0/companies({companyId})/items` - Get inventory items
   - Use the `/api/v2.0/` prefix for Business Central API v2

3. **Configuration**: Update `appsettings.json`:
   ```json
   {
     "BusinessCentral": {
       "BaseUrl": "https://api.businesscentral.dynamics.com/v2.0/{tenantId}",
       "CompanyId": "{companyId}",
       "ClientId": "{clientId}",
       "ClientSecret": "{clientSecret}",
       "Authority": "https://login.microsoftonline.com/{tenantId}"
     }
   }
   ```

4. **Error Handling**: Implement retry policies and proper error handling for:
   - Rate limiting (429 responses)
   - Authentication token expiration
   - Network failures

5. **Idempotency**: Ensure sync operations are idempotent by:
   - Using Business Central's `eTag` for optimistic concurrency
   - Tracking last sync timestamp
   - Implementing delta sync instead of full sync

The current mock implementation (`ErpSyncService.cs`) demonstrates all the patterns needed - just replace the HTTP calls and DTOs with the real Business Central equivalents.

## Why This is a Good Portfolio Project

### Demonstrates Enterprise Patterns
- **Layered architecture** with clear separation of concerns
- **Transaction management** for data consistency
- **Optimistic concurrency** for handling concurrent updates
- **Dependency injection** throughout the application
- **Repository pattern** for data access abstraction

### Shows Full-Stack Skills
- **Backend**: C#, ASP.NET Core, EF Core, SQL Server
- **Frontend**: React, TypeScript, modern tooling (Vite)
- **Database**: Schema design, migrations, indexing
- **Integration**: External API consumption, DTO mapping
- **Testing**: Unit tests with mocking, in-memory databases

### Real-World Features
- **Validation** at multiple layers (DTOs, API, Database)
- **Error handling** with proper HTTP status codes
- **Logging** for debugging and monitoring
- **Swagger documentation** for API exploration
- **Docker** for database containerization

### Production Considerations
- **Security**: HTTPS, proper error messages (no stack traces in production)
- **Performance**: Database indexes, connection pooling (EF Core default)
- **Scalability**: Stateless API, async/await throughout
- **Maintainability**: Clean code, SOLID principles, comprehensive tests

### Easy to Explain
The entire project can be explained in ~5 minutes:
1. **Problem**: Inventory management for fastener distributor
2. **Solution**: Full-stack app with backend, frontend, database
3. **Architecture**: Layered design with Controllers → Services → Repositories
4. **Key Features**: Product management, order processing, ERP sync
5. **Tech Stack**: .NET 8, React, SQL Server, Docker
6. **Patterns**: Transactions, concurrency, DI, RESTful APIs

This demonstrates you can build production-ready applications with enterprise-grade patterns, not just simple CRUD apps.
