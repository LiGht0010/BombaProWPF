# BombaProMax Architecture Report

## Executive Summary

**BombaProMax** is a gas station management system built with a modern client-server architecture:
- **Client**: .NET MAUI cross-platform mobile/desktop app (.NET 9)
- **Server**: ASP.NET Core Web API (.NET 8)
- **Database**: PostgreSQL with Entity Framework Core

This report documents the communication architecture, design patterns, and provides an analysis of pros and cons.

---

## Table of Contents

1. [System Overview](#1-system-overview)
2. [API Configuration Layer](#2-api-configuration-layer)
3. [HTTP Client Management](#3-http-client-management)
4. [Service Layer (MAUI Client)](#4-service-layer-maui-client)
5. [Controller Layer (API Server)](#5-controller-layer-api-server)
6. [Data Flow Architecture](#6-data-flow-architecture)
7. [Error Handling](#7-error-handling)
8. [Stock Management & FIFO](#8-stock-management--fifo)
9. [Summary: Pros and Cons](#9-summary-pros-and-cons)
10. [Recommendations](#10-recommendations)

---

## 1. System Overview

### Architecture Diagram

```
┌──────────────────────────────────────────────────────────────────────────────┐
│                           .NET MAUI CLIENT (.NET 9)                          │
│                    (Android, iOS, Windows, macOS)                            │
├──────────────────────────────────────────────────────────────────────────────┤
│  Views (XAML) ──▶ ViewModels ──▶ Services ──▶ HttpClientFactory ──▶ ApiConfig│
└──────────────────────────────────────────────────────────────────────────────┘
                                      │
                                      │ HTTP/REST (JSON)
                                      ▼
┌──────────────────────────────────────────────────────────────────────────────┐
│                        ASP.NET CORE WEB API (.NET 8)                         │
├──────────────────────────────────────────────────────────────────────────────┤
│  Controllers ──▶ Services ──▶ AutoMapper ──▶ EF Core ──▶ PostgreSQL         │
└──────────────────────────────────────────────────────────────────────────────┘
```

### Technology Stack

| Layer | Technology | Version |
|-------|------------|---------|
| Mobile/Desktop Client | .NET MAUI | .NET 9 |
| Web API | ASP.NET Core | .NET 8 |
| ORM | Entity Framework Core | 8.x |
| Database | PostgreSQL | 15+ |
| Object Mapping | AutoMapper | Latest |
| JSON Serialization | Newtonsoft.Json (Client) / System.Text.Json (API) | - |

---

## 2. API Configuration Layer

### Implementation: `ApiConfig.cs`

```csharp
public static class ApiConfig
{
    private static string _baseUrl = "http://62.84.189.17:5003/api";
    
    public static string BaseUrl { get; set; }
    public static string Clients => $"{BaseUrl}/Clients";
    public static string Achats => $"{BaseUrl}/Achats";
    // ... 30+ endpoint properties
}
```

### Features

| Feature | Description |
|---------|-------------|
| Centralized Configuration | Single source of truth for all API endpoints |
| Environment Switching | Comment/uncomment to switch dev/prod |
| Persistence | Saves URL to MAUI Preferences |
| Runtime Configuration | Can be changed at runtime via `SetAndSaveBaseUrl()` |

### Pros ✅

| Advantage | Description |
|-----------|-------------|
| **Single Point of Change** | Modify one file to switch environments |
| **Type Safety** | Compile-time checking of endpoint properties |
| **Consistency** | All services use the same base URL automatically |
| **Persistence** | User/admin can configure URL without recompiling |
| **Debug Friendly** | Logs the active URL at startup |

### Cons ❌

| Disadvantage | Description |
|--------------|-------------|
| **Hardcoded Default** | Production URL is in source code (security concern) |
| **No Build Configuration** | No automatic dev/staging/prod switching based on build |
| **Static Class** | Harder to mock in unit tests |
| **No Validation** | URL format is not validated beyond null/whitespace |
| **Manual Switching** | Requires code change or user intervention to switch environments |

---

## 3. HTTP Client Management

### Implementation: `HttpClientFactory.cs`

```csharp
public static class HttpClientFactory
{
    private static HttpClient? _sharedClient;
    private static readonly object _lock = new();

    public static HttpClient Create()
    {
        if (_sharedClient is null)
        {
            lock (_lock)
            {
                if (_sharedClient is null)
                {
                    _sharedClient = new HttpClient
                    {
                        Timeout = TimeSpan.FromSeconds(30)
                    };
                }
            }
        }
        return _sharedClient;
    }
}
```

### Pros ✅

| Advantage | Description |
|-----------|-------------|
| **Socket Exhaustion Prevention** | Single shared instance avoids port exhaustion |
| **Thread Safety** | Double-checked locking pattern |
| **Performance** | Reuses TCP connections |
| **Simple API** | `HttpClientFactory.Create()` is easy to use |
| **Consistent Timeout** | 30-second timeout applied globally |

### Cons ❌

| Disadvantage | Description |
|--------------|-------------|
| **Not Using IHttpClientFactory** | Doesn't leverage built-in .NET `IHttpClientFactory` |
| **No Named Clients** | Cannot configure different clients for different APIs |
| **DNS Changes Not Handled** | Long-lived client may cache stale DNS |
| **No Retry Policies** | No Polly integration for transient failures |
| **No Request/Response Logging** | Missing centralized logging via DelegatingHandler |
| **Static Coupling** | Services directly depend on static class |

---

## 4. Service Layer (MAUI Client)

### Service Count: 30+ Services

```
ClientService, ProduitService, AchatService, PeriodeService,
FactureService, DepenseService, DashboardService, RapportService,
StockLotService, CreditTransactionService, LoginServices, ...
```

### Common Service Pattern

```csharp
public class [Entity]Service
{
    private readonly HttpClient _httpClient;
    private static string BaseUrl => ApiConfig.[Entity];

    public [Entity]Service()
    {
        _httpClient = HttpClientFactory.Create();
    }

    public async Task<List<EntityDto>> GetAllAsync() { ... }
    public async Task<EntityDto?> GetByIdAsync(int id) { ... }
    public async Task<EntityDto?> CreateAsync(EntityDto dto) { ... }
    public async Task<bool> UpdateAsync(EntityDto dto) { ... }
    public async Task<bool> DeleteAsync(int id) { ... }
}
```

### Service Categories

| Category | Services | Complexity |
|----------|----------|------------|
| **Core CRUD** | ClientService, ProduitService, FournisseurService | Simple |
| **Transactions** | AchatService, FactureService, DepenseService | Medium |
| **Operations** | PeriodeService, JaugeageService, PompeService | Complex |
| **Finance** | CreditTransactionService, CaisseService | Medium |
| **Analytics** | DashboardService, RapportService | Read-only |
| **Stock** | StockLotService, StockWithdrawalService | Complex |

### Pros ✅

| Advantage | Description |
|-----------|-------------|
| **Separation of Concerns** | Each entity has dedicated service |
| **Consistent API** | All services follow same pattern |
| **Audit Fields** | Automatically sets `AjoutePar`, `DateCreation`, etc. |
| **Business Logic** | Services contain entity-specific logic (e.g., `GenerateNextNumeroAsync`) |
| **Fallback Mechanisms** | Some services fallback to client-side filtering if server fails |
| **Composite Operations** | `CreatePeriodeWithDetailsAsync()` handles complex transactions |

### Cons ❌

| Disadvantage | Description |
|--------------|-------------|
| **No Interface Abstraction** | Services are concrete classes, not `IClientService` |
| **Constructor Injection Missing** | HttpClient obtained via static factory |
| **Duplicate Code** | CRUD pattern repeated in every service |
| **No Caching** | Each request hits the server (no local cache) |
| **Silent Failures** | Many methods return empty list/null on error instead of throwing |
| **Mixed Responsibilities** | Some services do both API calls and local business logic |
| **No Cancellation Tokens** | Most methods don't support cancellation |

---

## 5. Controller Layer (API Server)

### Controller Count: 35+ Controllers

```
ClientsController, AchatsController, PeriodesController,
DashboardController, FacturesController, StockLotsController, ...
```

### Common Controller Pattern

```csharp
[Route("api/[controller]")]
[ApiController]
public class [Entity]Controller : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<T> _logger;

    // Standard CRUD + entity-specific endpoints
}
```

### Endpoint Patterns

| Pattern | Example | Purpose |
|---------|---------|---------|
| `GET /api/{entity}` | `/api/Clients` | Get all |
| `GET /api/{entity}/{id}` | `/api/Clients/5` | Get by ID |
| `GET /api/{entity}/search?term=x` | `/api/Clients/search?term=abc` | Search |
| `GET /api/{entity}/{fk}/{id}` | `/api/Achats/fournisseur/5` | Filter by FK |
| `GET /api/{entity}/daterange` | `/api/Achats/daterange?start&end` | Date filter |
| `POST /api/{entity}` | `/api/Clients` | Create |
| `POST /api/{entity}/with-details` | `/api/Periodes/with-details` | Composite create |
| `PUT /api/{entity}/{id}` | `/api/Clients/5` | Update |
| `DELETE /api/{entity}/{id}` | `/api/Clients/5` | Delete |

### Pros ✅

| Advantage | Description |
|-----------|-------------|
| **RESTful Design** | Standard HTTP verbs and resource-based URLs |
| **AutoMapper Integration** | Clean DTO ↔ Entity mapping |
| **Async/Await Throughout** | Non-blocking database operations |
| **Eager Loading** | `.Include()` prevents N+1 queries |
| **Transactional Operations** | `BeginTransactionAsync()` for complex operations |
| **Execution Strategy** | `CreateExecutionStrategy()` handles retries |
| **Structured Logging** | `ILogger` with contextual information |
| **CORS Configured** | `AllowAnyOrigin` for MAUI client access |

### Cons ❌

| Disadvantage | Description |
|--------------|-------------|
| **No Authentication** | No JWT/OAuth, all endpoints are public |
| **No Authorization** | No role-based access control |
| **No Rate Limiting** | Vulnerable to abuse |
| **No API Versioning** | Breaking changes affect all clients |
| **Fat Controllers** | Some controllers have complex business logic (should be in services) |
| **No Pagination** | `GetAll()` returns entire dataset |
| **No Response Caching** | Every request hits database |
| **CORS Too Permissive** | `AllowAnyOrigin` in production is a security risk |

---

## 6. Data Flow Architecture

### Request Lifecycle

```
┌─────────────────────────────────────────────────────────────────────────────┐
│ 1. USER ACTION                                                              │
│    Button Click / Form Submit                                               │
└─────────────────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│ 2. VIEWMODEL                                                                │
│    Command.Execute() → Calls Service Method                                 │
│    Sets IsBusy = true, handles UI state                                     │
└─────────────────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│ 3. SERVICE (Client)                                                         │
│    - Sets audit fields (AjoutePar, DateCreation)                           │
│    - Serializes DTO to JSON                                                 │
│    - Builds URL from ApiConfig                                              │
│    - Sends HTTP request via HttpClientFactory.Create()                      │
└─────────────────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼ HTTP (JSON)
┌─────────────────────────────────────────────────────────────────────────────┐
│ 4. CONTROLLER (Server)                                                      │
│    - Model binding from JSON                                                │
│    - Validation                                                             │
│    - Maps DTO → Entity via AutoMapper                                       │
│    - Calls DbContext / Services                                             │
└─────────────────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│ 5. DATABASE                                                                 │
│    - EF Core generates SQL                                                  │
│    - PostgreSQL executes query                                              │
│    - Returns results                                                        │
└─────────────────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼ (Response bubbles back up)
┌─────────────────────────────────────────────────────────────────────────────┐
│ 6. RESPONSE                                                                 │
│    Entity → DTO → JSON → Deserialize → Update UI                           │
└─────────────────────────────────────────────────────────────────────────────┘
```

### Pros ✅

| Advantage | Description |
|-----------|-------------|
| **Clear Separation** | Each layer has single responsibility |
| **Testable Layers** | API can be tested independently |
| **Platform Agnostic** | Any client can consume the REST API |
| **Scalable** | API and client can be deployed separately |

### Cons ❌

| Disadvantage | Description |
|--------------|-------------|
| **Network Dependency** | App is unusable offline |
| **Latency** | Every operation requires network round-trip |
| **No Offline Support** | No local SQLite cache for offline mode |
| **Chatty Protocol** | Multiple requests for related data |

---

## 7. Error Handling

### Client-Side Pattern

```csharp
public async Task<EntityDto?> CreateAsync(EntityDto dto)
{
    try
    {
        var response = await _httpClient.PostAsync(url, content);
        if (response.IsSuccessStatusCode)
            return JsonConvert.DeserializeObject<EntityDto>(responseJson);
        return null;  // Silent failure
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
        return null;  // Silent failure
    }
}
```

### Server-Side Pattern

```csharp
catch (InvalidOperationException ex)
{
    await transaction.RollbackAsync();
    return BadRequest(new { error = "Stock insuffisant", message = ex.Message });
}
catch (Exception ex)
{
    _logger.LogError(ex, "Error creating entity");
    return StatusCode(500, $"Internal server error: {ex.Message}");
}
```

### ServiceResult Pattern (PeriodeService)

```csharp
public async Task<ServiceResult<T>> CreateAsync(T dto)
{
    // ... operation ...
    if (success)
        return ServiceResult<T>.Success(result);
    else
        return ServiceResult<T>.Failure(errorCode, errorMessage);
}
```

### Pros ✅

| Advantage | Description |
|-----------|-------------|
| **Graceful Degradation** | App doesn't crash on API errors |
| **Transaction Rollback** | Complex operations are atomic |
| **Structured Errors** | API returns `{ error, message }` format |
| **ServiceResult Pattern** | Type-safe success/failure handling |
| **Logging** | Errors are logged for debugging |

### Cons ❌

| Disadvantage | Description |
|--------------|-------------|
| **Silent Failures** | Many methods return null/empty instead of throwing |
| **Lost Error Details** | Client often doesn't show specific error to user |
| **Inconsistent Patterns** | Some services use ServiceResult, others return null |
| **No Global Exception Handler** | Middleware for consistent error responses missing |
| **User Messages in Code** | Error messages hardcoded (not localized) |

---

## 8. Stock Management & FIFO

### FIFO Stock Consumption Flow

```
┌─────────────────────────────────────────────────────────────────────────────┐
│ POST /api/Periodes/with-details                                             │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  1. BEGIN TRANSACTION                                                       │
│        │                                                                    │
│        ▼                                                                    │
│  2. Create Periode                                                          │
│        │                                                                    │
│        ▼                                                                    │
│  3. Create PeriodeDetails (pump readings)                                   │
│        │                                                                    │
│        ▼                                                                    │
│  4. Link CreditTransactions                                                 │
│        │                                                                    │
│        ▼                                                                    │
│  5. FIFO STOCK CONSUMPTION                                                  │
│     ┌─────────────────────────────────────────────────────────────────┐    │
│     │ For each PeriodeDetail:                                          │    │
│     │   - Calculate QuantiteVendue (CompteurFin - CompteurDebut)      │    │
│     │   - IStockLotService.ConsumeAsync(produit, reservoir, qty)      │    │
│     │   - Deduct from oldest StockLots first (FIFO)                   │    │
│     │   - Create StockLotConsumption records                          │    │
│     └─────────────────────────────────────────────────────────────────┘    │
│        │                                                                    │
│        ▼                                                                    │
│  6. SYNC RESERVOIR LEVELS                                                   │
│     - Recalculate NiveauActuel from StockLots.QuantiteDisponible           │
│        │                                                                    │
│        ▼                                                                    │
│  7. SYNC PUMP COUNTERS                                                      │
│     - Update Pompe.CompteurActuel from latest PeriodeDetail                │
│        │                                                                    │
│        ▼                                                                    │
│  8. COMMIT TRANSACTION                                                      │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### Pros ✅

| Advantage | Description |
|-----------|-------------|
| **FIFO Accuracy** | Correct cost tracking for profit margins |
| **Atomic Operations** | Stock and sales always in sync |
| **Audit Trail** | StockLotConsumption links sales to purchases |
| **Cascade Updates** | Subsequent periods auto-update |
| **Reversible** | Update/Delete properly reverses consumptions |

### Cons ❌

| Disadvantage | Description |
|--------------|-------------|
| **Complexity** | Many moving parts to maintain consistency |
| **Performance** | Multiple DB operations per sale |
| **Lock Contention** | Concurrent sales may conflict |
| **Recovery Difficulty** | If sync fails, manual intervention needed |

---

## 9. Summary: Pros and Cons

### Overall Architecture Pros ✅

| Category | Pros |
|----------|------|
| **Separation** | Clean client/server separation enables independent deployment |
| **Cross-Platform** | MAUI enables Android, iOS, Windows, macOS from single codebase |
| **Type Safety** | Strongly-typed DTOs prevent runtime errors |
| **Modern Stack** | Latest .NET 8/9, EF Core, PostgreSQL |
| **RESTful API** | Standard patterns, easy to understand and extend |
| **Atomic Transactions** | Complex operations are consistent |
| **FIFO Inventory** | Accurate cost tracking for financial reporting |
| **Audit Fields** | Track who created/modified records |
| **Centralized Config** | Easy environment switching |

### Overall Architecture Cons ❌

| Category | Cons |
|----------|------|
| **Security** | No authentication, authorization, or rate limiting |
| **Offline Support** | App is unusable without network |
| **Testability** | Static classes and concrete services hinder unit testing |
| **Resilience** | No retry policies for transient failures |
| **Scalability** | No pagination, caching, or load balancing |
| **Observability** | Limited structured logging, no distributed tracing |
| **Error UX** | Silent failures don't inform users |
| **Code Duplication** | CRUD pattern repeated in 30+ services |
| **DNS Caching** | Long-lived HttpClient may use stale DNS |
| **Hardcoded Strings** | URLs and error messages not localized |

---

## 10. Recommendations

### High Priority (Security & Stability)

| # | Recommendation | Effort |
|---|----------------|--------|
| 1 | **Add JWT Authentication** | High |
| 2 | **Implement Role-Based Authorization** | High |
| 3 | **Add API Rate Limiting** | Medium |
| 4 | **Restrict CORS in Production** | Low |
| 5 | **Move secrets to environment variables** | Low |

### Medium Priority (Quality & Maintainability)

| # | Recommendation | Effort |
|---|----------------|--------|
| 6 | **Extract interfaces for services** (`IClientService`) | Medium |
| 7 | **Use `IHttpClientFactory`** with named clients | Medium |
| 8 | **Add Polly retry policies** | Medium |
| 9 | **Implement pagination** for large datasets | Medium |
| 10 | **Add response caching** for read-heavy endpoints | Medium |

### Low Priority (Enhancements)

| # | Recommendation | Effort |
|---|----------------|--------|
| 11 | **Add offline support** with SQLite cache | High |
| 12 | **Implement API versioning** | Medium |
| 13 | **Add health checks** (`/health`, `/ready`) | Low |
| 14 | **Centralize error handling** middleware | Low |
| 15 | **Add OpenTelemetry** for distributed tracing | Medium |

### Code Improvement Examples

#### 1. Use IHttpClientFactory (Recommended)

```csharp
// MauiProgram.cs
builder.Services.AddHttpClient("BombaApi", client =>
{
    client.BaseAddress = new Uri(ApiConfig.BaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
})
.AddPolicyHandler(GetRetryPolicy());

// Service
public class ClientService
{
    private readonly HttpClient _httpClient;
    
    public ClientService(IHttpClientFactory factory)
    {
        _httpClient = factory.CreateClient("BombaApi");
    }
}
```

#### 2. Add Service Interfaces

```csharp
public interface IClientService
{
    Task<List<ClientDto>> GetAllAsync(CancellationToken ct = default);
    Task<ClientDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<ServiceResult<ClientDto>> CreateAsync(ClientDto dto, CancellationToken ct = default);
}
```

#### 3. Add JWT Authentication

```csharp
// API - Program.cs
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => { /* config */ });

// Controllers
[Authorize]
[ApiController]
public class ClientsController : ControllerBase { }
```

---

## Appendix A: File Structure

```
BombaProMaxFull-master/
├── BombaProMax/                    # MAUI Client (.NET 9)
│   ├── Services/
│   │   ├── ApiConfig.cs           # API endpoint configuration
│   │   ├── HttpClientFactory.cs   # Singleton HttpClient
│   │   ├── ClientService.cs       # Client entity service
│   │   ├── AchatService.cs        # Purchase service
│   │   ├── PeriodeService.cs      # Shift/period service
│   │   └── ... (30+ services)
│   ├── ViewModels/
│   ├── Views/
│   ├── Models/                    # DTOs
│   └── MauiProgram.cs             # DI configuration
│
└── BombaProMaxApi/                 # ASP.NET Core API (.NET 8)
    ├── Controllers/
    │   ├── ClientsController.cs
    │   ├── AchatsController.cs
    │   ├── PeriodesController.cs
    │   ├── DashboardController.cs
    │   └── ... (35+ controllers)
    ├── Services/
    │   ├── IStockLotService.cs
    │   ├── StockLotService.cs
    │   └── PeriodeCascadeService.cs
    ├── Data/
    │   ├── AppDbContext.cs
    │   └── Migrations/
    ├── DTOs/
    ├── Models/
    ├── Mappig/ (sic)              # AutoMapper profiles
    └── Program.cs
```

---

## Appendix B: API Endpoints Reference

| Entity | GET All | GET One | POST | PUT | DELETE | Special |
|--------|---------|---------|------|-----|--------|---------|
| Clients | ✅ | ✅ | ✅ | ✅ | ✅ | search, check-numero, check-cin, credit-balance |
| Achats | ✅ | ✅ | ✅ | ✅ | ✅ | fournisseur/{id}, produit/{id}, daterange |
| Periodes | ✅ | ✅ | ✅ | ✅ | ✅ | with-details, employe/{id}, date-range, current |
| Dashboard | - | - | - | - | - | achats, ventes, ventes-carburant |
| StockLots | ✅ | ✅ | ✅ | - | - | periode/{id}/marge |

---

*Report generated: 2025*  
*Architecture Version: 1.0*  
*BombaProMax Full Solution*
