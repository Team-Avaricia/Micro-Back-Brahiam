# 🔐 JWT Authentication - Implementación Completa

## ✅ Estado: LISTO PARA MIGRACIÓN

**Fecha:** 1 de Diciembre, 2025
**Rama:** `feature/brahiam/jwt-authentication`

---

## 📋 Resumen de Implementación

### Entidades Creadas/Modificadas

#### 1. **RefreshToken** (NUEVA)
```csharp
- Id, UserId, Token, ExpiresAt
- IsRevoked, CreatedByIp, RevokedByIp, RevokedAt
- Métodos: Revoke(), IsActive, IsExpired
```

#### 2. **User** (MODIFICADA)
```csharp
+ PasswordHash (string, 500 chars)
+ RefreshTokens (ICollection<RefreshToken>)
+ SetPassword(string passwordHash)
```

---

## 🗄️ Cambios en Base de Datos (Pendientes de Migración)

### Tabla: `RefreshTokens`
| Columna | Tipo | Descripción |
|---------|------|-------------|
| Id | UUID | PK |
| UserId | UUID | FK a Users |
| Token | VARCHAR(500) | Refresh token único |
| ExpiresAt | TIMESTAMP | Fecha de expiración |
| IsRevoked | BOOLEAN | Si está revocado |
| CreatedByIp | VARCHAR(50) | IP de creación |
| RevokedByIp | VARCHAR(50) | IP de revocación |
| RevokedAt | TIMESTAMP | Fecha de revocación |
| CreatedAt | TIMESTAMP | Fecha creación |
| UpdatedAt | TIMESTAMP | Fecha actualización |

**Índices:**
- `Token` (UNIQUE)
- `UserId`
- `(UserId, IsRevoked, ExpiresAt)` (compuesto)

### Tabla: `Users` (MODIFICADA)
| Columna Agregada | Tipo | Descripción |
|------------------|------|-------------|
| PasswordHash | VARCHAR(500) | Hash BCrypt de contraseña |

---

## 🔧 Servicios Implementados

### 1. **TokenService**
```csharp
+ GenerateAccessToken(User user) → string
+ GenerateRefreshToken() → string
+ GetUserIdFromToken(string token) → Guid?
```

**Configuración JWT:**
- SecretKey: 64 caracteres
- Issuer: "FinancialAssistantAPI"
- Audience: "FinancialAssistantClients"
- Expiración: 60 minutos

### 2. **AuthService**
```csharp
+ RegisterAsync(RegisterRequest, ipAddress) → AuthResponse
+ LoginAsync(LoginRequest, ipAddress) → AuthResponse
+ RefreshTokenAsync(refreshToken, ipAddress) → AuthResponse
+ RevokeTokenAsync(refreshToken, ipAddress) → void
```

**Características:**
- ✅ Hash de contraseñas con BCrypt
- ✅ Validación de email y teléfono únicos
- ✅ Generación de Access Token (JWT)
- ✅ Generación de Refresh Token (7 días)
- ✅ Logging de eventos de autenticación

---

## 🎯 Endpoints de Autenticación

### 1. POST `/api/Auth/register`
**Request:**
```json
{
  "name": "Juan Pérez",
  "email": "juan@example.com",
  "phoneNumber": "+573001234567",
  "password": "MiPassword123!",
  "initialBalance": 100000
}
```

**Response:**
```json
{
  "userId": "guid",
  "name": "Juan Pérez",
  "email": "juan@example.com",
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "base64-random-token",
  "expiresAt": "2025-12-01T17:32:00Z"
}
```

### 2. POST `/api/Auth/login`
**Request:**
```json
{
  "email": "juan@example.com",
  "password": "MiPassword123!"
}
```

**Response:** (mismo formato que register)

### 3. POST `/api/Auth/refresh-token`
**Request:**
```json
{
  "refreshToken": "base64-random-token"
}
```

**Response:** (nuevo access token y refresh token)

### 4. POST `/api/Auth/revoke-token` (Requiere autenticación)
**Request:**
```json
{
  "refreshToken": "base64-random-token"
}
```

---

## 🔒 Configuración de Seguridad

### appsettings.json
```json
{
  "Jwt": {
    "SecretKey": "TuClaveSecretaSuperSeguraDeAlMenos32CaracteresParaJWT2024!",
    "Issuer": "FinancialAssistantAPI",
    "Audience": "FinancialAssistantClients",
    "ExpirationMinutes": 60
  }
}
```

### Program.cs
```csharp
✅ AddAuthentication(JwtBearer)
✅ AddAuthorization()
✅ UseAuthentication() (antes de UseAuthorization)
✅ UseAuthorization()
```

---

## 📦 Paquetes NuGet Agregados

### API Project
- ✅ Microsoft.AspNetCore.Authentication.JwtBearer 8.0.11
- ✅ System.IdentityModel.Tokens.Jwt
- ✅ BCrypt.Net-Next

### Core.Application Project
- ✅ Microsoft.Extensions.Configuration.Abstractions
- ✅ System.IdentityModel.Tokens.Jwt
- ✅ BCrypt.Net-Next

---

## 🔄 Repositorios Agregados

### IRefreshTokenRepository
```csharp
+ GetByIdAsync(Guid id)
+ GetByTokenAsync(string token)
+ GetByUserIdAsync(Guid userId)
+ GetActiveByUserIdAsync(Guid userId)
+ AddAsync(RefreshToken)
+ UpdateAsync(RefreshToken)
+ DeleteAsync(Guid id)
```

**Implementación:** `RefreshTokenRepository` en `RepositoryImplementations.cs`

---

## 🎯 Próximos Pasos

### 1. Actualizar Connection String
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=NUEVO_HOST;Port=5432;Database=NUEVA_DB;Username=USER;Password=PASS"
  }
}
```

### 2. Crear Migración
```bash
dotnet ef migrations add AddJWTAuthentication --project src/Infrastructure --startup-project src/API
```

### 3. Aplicar Migración
```bash
dotnet ef database update --project src/Infrastructure --startup-project src/API
```

### 4. Probar Endpoints
```bash
# 1. Registrar usuario
POST /api/Auth/register

# 2. Login
POST /api/Auth/login

# 3. Usar token en headers
Authorization: Bearer {accessToken}

# 4. Refresh token cuando expire
POST /api/Auth/refresh-token
```

---

## 🔐 Proteger Endpoints Existentes (Opcional)

Para proteger endpoints que requieran autenticación:

```csharp
[Authorize]
[HttpGet("user/{userId}")]
public async Task<IActionResult> GetUser(string userId)
{
    // Solo usuarios autenticados pueden acceder
}
```

---

## ✅ Checklist de Verificación

- [x] RefreshToken entity creada
- [x] User entity actualizada con PasswordHash
- [x] TokenService implementado
- [x] AuthService implementado
- [x] AuthController con 4 endpoints
- [x] JWT configurado en Program.cs
- [x] RefreshTokenRepository implementado
- [x] DbContext actualizado
- [x] Compilación exitosa (0 errores)
- [ ] Migración creada (pendiente de credenciales DB)
- [ ] Migración aplicada (pendiente de credenciales DB)
- [ ] Endpoints probados en Swagger

---

## 📝 Notas Importantes

1. **Seguridad:**
   - La SecretKey debe cambiarse en producción
   - RequireHttpsMetadata debe ser `true` en producción
   - Considerar agregar rate limiting para login

2. **Refresh Tokens:**
   - Expiran en 7 días
   - Se revocan automáticamente al usarse
   - Se puede revocar manualmente con `/revoke-token`

3. **Compatibilidad con Johan:**
   - Las tablas `Users` y `RefreshTokens` estarán disponibles
   - Johan puede usar los mismos endpoints de autenticación
   - La misma base de datos compartida

---

**Estado:** ✅ Código completo y listo para migración
**Próximo paso:** Esperar credenciales de nueva base de datos
