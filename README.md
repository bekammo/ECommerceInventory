# 🛒 ECommerce Inventory System

A modern, production-ready e-commerce inventory management system built with .NET 10, featuring clean architecture, advanced security, and enterprise-grade patterns.

## 📋 Table of Contents

- [Features](#-features)
- [Architecture](#-architecture)
- [Tech Stack](#-tech-stack)
- [Getting Started](#-getting-started)
- [API Documentation](#-api-documentation)
- [Project Structure](#-project-structure)
- [Key Features & Patterns](#-key-features--patterns)
- [Docker Deployment](#-docker-deployment)
- [Testing](#-testing)
- [Security](#-security)
- [Contributing](#-contributing)
- [Documentation](#-documentation)

## ✨ Features

### Core Functionality
- **Product Management**: Complete CRUD operations for product catalog
- **Order Processing**: Order creation, tracking, and status management
- **User Authentication**: Secure JWT-based authentication with session management
- **Payment Processing**: Background payment processing with retry logic
- **Discount System**: Flexible discount cards (percentage and fixed amount)

### Enterprise Features
- **Optimistic Concurrency Control**: Prevents race conditions in stock management
- **Outbox Pattern**: Ensures reliable event publishing with at-least-once delivery
- **Background Services**: Payment processing and event publishing workers
- **Session Management**: Multi-device login support with logout capabilities
- **API Response Standardization**: Consistent response format across all endpoints
- **Comprehensive Error Handling**: Global exception middleware with proper logging

## 🏗️ Architecture

This project follows **Clean Architecture** principles with clear separation of concerns:

```
┌─────────────────────────────────────────────────────┐
│                  Presentation Layer                  │
│              (ECommerceInventory.API)                │
│         Controllers, Middleware, DTOs                │
└─────────────────────────────────────────────────────┘
                         ↓
┌─────────────────────────────────────────────────────┐
│                 Application Layer                    │
│          (ECommerceInventory.Application)            │
│          Interfaces, DTOs, Contracts                 │
└─────────────────────────────────────────────────────┘
                         ↓
┌─────────────────────────────────────────────────────┐
│                  Business Logic Layer                │
│             (ECommerceInventory.Domain)              │
│       Entities, Value Objects, Domain Logic          │
└─────────────────────────────────────────────────────┘
                         ↓
┌─────────────────────────────────────────────────────┐
│                Infrastructure Layer                  │
│          (ECommerceInventory.Infrastructure)         │
│    Repositories, Services, Data Access, Security     │
└─────────────────────────────────────────────────────┘
```

## 🛠️ Tech Stack

- **Framework**: .NET 10
- **Language**: C# 13
- **Database**: SQL Server 2022
- **ORM**: Entity Framework Core 10
- **Authentication**: JWT Bearer Tokens
- **API Documentation**: OpenAPI (Swagger)
- **Containerization**: Docker & Docker Compose
- **Testing**: xUnit, Moq
- **Logging**: Microsoft.Extensions.Logging

## 🚀 Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop) (for containerized deployment)
- SQL Server 2022 (or use Docker Compose)

### Option 1: Docker Compose (Recommended)

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd ECommerceInventory
   ```

2. **Start the application**
   ```bash
   docker-compose up --build
   ```

3. **Access the API**
   - API: http://localhost:5000
   - Swagger UI: http://localhost:5000/swagger

### Option 2: Local Development

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd ECommerceInventory
   ```

2. **Update connection string**
   
   Edit `ECommerceInventory.API/appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Database=ECommerceInventory;Trusted_Connection=True;TrustServerCertificate=True"
     }
   }
   ```

3. **Apply database migrations**
   ```bash
   cd ECommerceInventory.API
   dotnet ef database update
   ```

4. **Run the application**
   ```bash
   dotnet run
   ```

5. **Access the API**
   - API: https://localhost:5001 (or http://localhost:5000)
   - Swagger UI: https://localhost:5001/swagger

### First Time Setup

See [FRESH_CLONE_SETUP.md](FRESH_CLONE_SETUP.md) for detailed setup instructions.

## 📚 API Documentation

### Authentication Endpoints

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| POST | `/api/auth/register` | Register a new user | ❌ |
| POST | `/api/auth/login` | Login and get JWT token | ❌ |
| POST | `/api/auth/logout` | Logout current session | ✅ |
| POST | `/api/auth/logout-all` | Logout all sessions | ✅ |
| GET | `/api/auth/sessions` | Get all active sessions | ✅ |

### Product Endpoints

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| GET | `/api/products` | Get all products | ❌ |
| GET | `/api/products/{id}` | Get product by ID | ❌ |
| POST | `/api/products` | Create new product | ✅ |
| PUT | `/api/products/{id}` | Update product | ✅ |
| DELETE | `/api/products/{id}` | Delete product | ✅ |

### Order Endpoints

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| POST | `/api/orders` | Create new order | ✅ |
| GET | `/api/orders` | Get user orders | ✅ |
| GET | `/api/orders/{id}` | Get order by ID | ✅ |
| GET | `/api/orders/{id}/status` | Get order status | ✅ |

### Example Usage

**Register a new user:**
```bash
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "username": "john_doe",
    "email": "john@example.com",
    "password": "SecurePass123!",
    "fullName": "John Doe"
  }'
```

**Login:**
```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "usernameOrEmail": "john_doe",
    "password": "SecurePass123!"
  }'
```

**Create a product (with authentication):**
```bash
curl -X POST http://localhost:5000/api/products \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -d '{
    "name": "Laptop",
    "description": "High-performance laptop",
    "price": 999.99,
    "stock": 50
  }'
```

## 📁 Project Structure

```
ECommerceInventory/
├── ECommerceInventory.API/              # Presentation Layer
│   ├── Controllers/                     # API Controllers
│   │   ├── AuthController.cs
│   │   ├── OrdersController.cs
│   │   └── ProductsController.cs
│   ├── Middleware/                      # Custom Middleware
│   │   ├── AuthenticationMiddleware.cs
│   │   └── ExceptionHandlingMiddleware.cs
│   ├── Program.cs                       # Application entry point
│   ├── appsettings.json
│   └── Dockerfile
│
├── ECommerceInventory.Application/      # Application Layer
│   ├── DTOs/                            # Data Transfer Objects
│   │   ├── Auth/
│   │   ├── Common/
│   │   ├── Order/
│   │   └── Product/
│   └── Interfaces/                      # Service & Repository Interfaces
│       ├── Repositories/
│       └── Services/
│
├── ECommerceInventory.Domain/           # Domain Layer
│   ├── Entities/                        # Domain Entities
│   │   ├── Order.cs
│   │   ├── OrderItem.cs
│   │   ├── OutboxEvent.cs
│   │   ├── Product.cs
│   │   ├── Session.cs
│   │   └── User.cs
│   ├── Discounts/                       # Discount Strategy Pattern
│   │   ├── IDiscountCard.cs
│   │   ├── PercentageDiscountCard.cs
│   │   ├── FixedAmountDiscountCard.cs
│   │   └── DiscountCardFactory.cs
│   ├── Enums/
│   │   ├── OrderStatus.cs
│   │   └── PaymentStatus.cs
│   └── Exceptions/
│       └── ConcurrencyException.cs
│
├── ECommerceInventory.Infrastructure/   # Infrastructure Layer
│   ├── BackgroundServices/              # Background Workers
│   │   ├── OutboxEventPublisherService.cs
│   │   └── PaymentProcessingService.cs
│   ├── Data/                            # Database Context
│   │   ├── ApplicationDbContext.cs
│   │   └── ApplicationDbContextFactory.cs
│   ├── Repositories/                    # Repository Implementations
│   │   ├── EfSessionRepository.cs
│   │   └── InMemorySessionRepository.cs
│   ├── Security/                        # Security Services
│   │   ├── ITokenGenerator.cs
│   │   ├── TokenGenerator.cs
│   │   └── PasswordHasher.cs
│   └── Services/                        # Service Implementations
│       ├── AuthService.cs
│       ├── OrderService.cs
│       └── ProductService.cs
│
├── tests/
│   └── ECommerceInventory.UnitTests/    # Unit Tests
│       ├── Concurrency/
│       ├── Domain/
│       ├── Helpers/
│       ├── Security/
│       └── Services/
│
├── docker-compose.yml                    # Development Docker setup
├── docker-compose.prod.yml               # Production Docker setup
├── Dockerfile                            # API Docker image
├── .dockerignore
├── .gitignore
└── README.md
```

## 🎯 Key Features & Patterns

### 1. **Clean Architecture**
- Clear separation between layers
- Dependency inversion (Infrastructure depends on Domain/Application)
- Business logic isolated from infrastructure concerns

### 2. **Strategy Pattern (Discounts)**
```csharp
IDiscountCard percentageDiscount = new PercentageDiscountCard(10, minimumAmount: 100);
IDiscountCard fixedDiscount = new FixedAmountDiscountCard(50, minimumAmount: 200);
```

### 3. **Optimistic Concurrency Control**
- Prevents overselling of products
- Uses row versioning (RowVersion/Timestamp)
- Automatic retry on concurrency conflicts

### 4. **Outbox Pattern**
- Ensures reliable event publishing
- Atomic database writes with event creation
- Background worker processes events with retry logic

### 5. **JWT Authentication**
- Secure token-based authentication
- Session tracking with device information
- Token validation on every request
- Logout and logout-all functionality

### 6. **Background Services**
- **PaymentProcessingService**: Processes pending payments asynchronously
- **OutboxEventPublisherService**: Publishes domain events reliably

### 7. **Repository Pattern**
- Abstraction over data access
- Easy to swap implementations (EF Core, In-Memory, etc.)
- Testable data layer

### 8. **Global Exception Handling**
```csharp
// Catches all unhandled exceptions
// Returns consistent error responses
// Logs errors with proper context
```

## 🐳 Docker Deployment

### Development
```bash
docker-compose up --build
```

### Production
```bash
# Configure environment
cp .env.template .env
# Edit .env with production values

# Deploy
docker-compose -f docker-compose.prod.yml up -d --build
```

### Database Management
```bash
# Run migrations
docker-compose exec api dotnet ef database update

# View logs
docker-compose logs -f api

# Stop services
docker-compose down

# Remove all data (including database)
docker-compose down -v
```

For detailed deployment instructions, see [DOCKER_DEPLOYMENT.md](DOCKER_DEPLOYMENT.md).

## 🧪 Testing

### Run All Tests
```bash
dotnet test
```

### Run Specific Test Category
```bash
# Security tests
dotnet test --filter Category=Security

# Concurrency tests
dotnet test --filter Category=Concurrency

# Service tests
dotnet test --filter Category=Services
```

### Test Coverage
- **Unit Tests**: Services, Domain Logic, Security
- **Concurrency Tests**: Stock management, race conditions
- **Integration Tests**: Repository implementations

### Test Structure
```
tests/ECommerceInventory.UnitTests/
├── Concurrency/
│   ├── ProductRepositoryTests.cs
│   └── StockConcurrencyTests.cs
├── Domain/
│   └── DiscountTests.cs
├── Security/
│   ├── PasswordHasherTests.cs
│   └── TokenGeneratorTests.cs
└── Services/
    ├── AuthServiceTests.cs
    ├── OrderServiceTests.cs
    └── ProductServiceTests.cs
```

## 🔒 Security

### Authentication
- JWT Bearer token-based authentication
- Secure password hashing (PBKDF2)
- Session management with device tracking

### Authorization
- Token validation middleware
- User context injection via HttpContext
- Protected endpoints require valid JWT

### Data Protection
- SQL injection prevention (parameterized queries via EF Core)
- Password complexity requirements
- Secure token storage recommendations

### Best Practices Applied
- Environment-specific configuration
- Secrets management (use environment variables in production)
- HTTPS enforcement
- CORS configuration
- Input validation

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

### Code Style
- Follow existing coding conventions
- Add XML documentation for public APIs
- Write unit tests for new features
- Update README for significant changes

## 📖 Documentation

Additional documentation available:
- [BEST_PRACTICES_APPLIED.md](BEST_PRACTICES_APPLIED.md) - Code quality improvements
- [DOCKER_DEPLOYMENT.md](DOCKER_DEPLOYMENT.md) - Docker deployment guide
- [DOCKER_RUNTIME_GUIDE.md](DOCKER_RUNTIME_GUIDE.md) - Docker runtime operations
- [FRESH_CLONE_SETUP.md](FRESH_CLONE_SETUP.md) - Setup instructions for new developers
- [DEPLOYMENT_CHECKLIST.md](DEPLOYMENT_CHECKLIST.md) - Production deployment checklist
- [CODE_REVIEW_FIXES.md](CODE_REVIEW_FIXES.md) - Code review findings and fixes
- [PORTABILITY_STATUS.md](PORTABILITY_STATUS.md) - Cross-platform compatibility status

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🙏 Acknowledgments

- Built with .NET 10
- Inspired by Clean Architecture principles
- Following Domain-Driven Design patterns

## 💬 Support

For issues, questions, or contributions, please open an issue on the GitHub repository.

---

**Built with ❤️ using .NET 10**
