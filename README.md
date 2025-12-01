# Juez IA - MS Core (.NET)

Motor de Reglas y Validación Financiera para el sistema "Juez IA".

## 🏗️ Arquitectura

Este microservicio implementa DDD (Domain-Driven Design) con las siguientes capas:

- **Core.Domain**: Entidades, Enums, Interfaces de Repositorios
- **Core.Application**: Servicios de Negocio (SpendingValidationService, TransactionService), DTOs
- **Infrastructure**: Implementación de Repositorios, DbContext (PostgreSQL)
- **API**: Controllers REST, Configuración de DI y CORS

## 📦 Instalación

### 1. Instalar Paquetes NuGet
```bash
dotnet add src/Infrastructure/Infrastructure.csproj package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add src/Infrastructure/Infrastructure.csproj package Microsoft.EntityFrameworkCore.Design
dotnet add src/API/API.csproj package Microsoft.EntityFrameworkCore.Tools
```

### 2. Configurar PostgreSQL
Edita `src/API/appsettings.json` con tus credenciales:
```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Database=juez_ia_db;Username=TU_USUARIO;Password=TU_PASSWORD"
}
```

### 3. Crear Base de Datos
```bash
dotnet ef migrations add InitialCreate --project src/Infrastructure/Infrastructure.csproj --startup-project src/API/API.csproj
dotnet ef database update --project src/Infrastructure/Infrastructure.csproj --startup-project src/API/API.csproj
```

### 4. Ejecutar
```bash
dotnet run --project src/API/API.csproj
```

## 🔌 Endpoints Principales

### 1. **POST /api/SpendingValidation/validate**
**Para MS AI Worker (Johan)**: Valida si un gasto es permitido.

**Request:**
```json
{
  "userId": "guid-del-usuario",
  "amount": 50000,
  "category": "Comida",
  "description": "Almuerzo"
}
```

**Response:**
```json
{
  "isApproved": true,
  "verdict": "Aprobado",
  "reason": "Gasto permitido. Saldo después: $450.000",
  "remainingBudget": 450000
}
```

### 2. **POST /api/Transaction**
**Para n8n**: Registra una transacción automáticamente.

### 3. **POST /api/FinancialRule**
**Para Dashboard Vue.js**: Crea reglas financieras.

## 🧪 Testing con Swagger
Accede a `https://localhost:XXXX/swagger` para probar los endpoints.

## 🤝 Integración con Otros Microservicios
- **MS AI Worker (Spring Boot)**: Consume `/api/SpendingValidation/validate`
- **n8n**: Consume `/api/Transaction`
- **Dashboard (Vue.js)**: Consume `/api/FinancialRule`

CORS está configurado para permitir comunicación desde `localhost:8080`, `localhost:3000`, `localhost:8081`.
