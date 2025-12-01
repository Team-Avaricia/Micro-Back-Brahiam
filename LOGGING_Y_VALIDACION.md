# 📝 Guía: Logging y Validación de Entrada

## 🔍 ¿Qué es Logging?

**Logging** es registrar eventos importantes de tu aplicación en archivos o consola para:
- **Debugging**: Encontrar errores
- **Monitoreo**: Ver qué está pasando en producción
- **Auditoría**: Saber quién hizo qué y cuándo

### ✅ Lo que ya implementamos (Serilog)

```csharp
// En Program.cs
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()  // Muestra logs en la consola
    .WriteTo.File("logs/ms-core-.txt", rollingInterval: RollingInterval.Day)  // Guarda en archivos
    .CreateLogger();
```

**Resultado:**
- Los logs se guardan en `logs/ms-core-2025-11-27.txt` (un archivo por día)
- También se muestran en la consola cuando ejecutas la API

### Niveles de Log:

```csharp
_logger.LogInformation("Usuario creado: {UserId}", userId);  // Info normal
_logger.LogWarning("Usuario no encontrado: {UserId}", userId);  // Advertencia
_logger.LogError(ex, "Error al procesar transacción");  // Error
_logger.LogFatal(ex, "La aplicación falló");  // Error crítico
```

### Ejemplo de Log Generado:

```
2025-11-27 16:10:00 [INF] Iniciando MS Core API
2025-11-27 16:10:05 [INF] Transacción recurrente creada: abc123 para usuario xyz789, Monto: 2000000, Frecuencia: Monthly
2025-11-27 16:10:10 [WRN] Intento de crear transacción recurrente para usuario inexistente: invalid-id
2025-11-27 16:10:15 [ERR] Error al procesar transacción recurrente: abc123
```

---

## ✅ ¿Qué es FluentValidation? (Paso 5)

**FluentValidation** es una librería para validar que los datos que llegan a tu API sean correctos ANTES de procesarlos.

### Problema sin validación:

```csharp
// Usuario envía esto:
{
  "userId": "",  // ❌ Vacío
  "amount": -50000,  // ❌ Negativo
  "type": "InvalidType",  // ❌ No es Income ni Expense
  "category": "",  // ❌ Vacío
}

// Tu API crashea o guarda datos incorrectos
```

### Solución con FluentValidation:

```csharp
public class CreateTransactionRequestValidator : AbstractValidator<CreateTransactionRequest>
{
    public CreateTransactionRequestValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("El userId es requerido")
            .Must(BeAValidGuid).WithMessage("El userId debe ser un GUID válido");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("El monto debe ser mayor a 0");

        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("La categoría es requerida")
            .MaximumLength(100).WithMessage("La categoría no puede tener más de 100 caracteres");
    }

    private bool BeAValidGuid(string userId)
    {
        return Guid.TryParse(userId, out _);
    }
}
```

**Resultado:**
Si el usuario envía datos incorrectos, la API retorna automáticamente:

```json
{
  "errors": {
    "UserId": ["El userId es requerido"],
    "Amount": ["El monto debe ser mayor a 0"],
    "Category": ["La categoría es requerida"]
  }
}
```

---

## 🚀 ¿Deberías implementar FluentValidation ahora?

### Pros:
- ✅ Evita datos incorrectos en la base de datos
- ✅ Mejora la experiencia del usuario (mensajes claros)
- ✅ Reduce bugs

### Contras:
- ⏳ Toma tiempo (1-2 horas para todos los DTOs)
- 🤔 Johan ya valida en su lado (OpenAI procesa el texto)

### Mi Recomendación:

**NO lo implementes ahora.** Razones:

1. Johan ya valida los datos con OpenAI antes de enviártelos
2. Tu API ya funciona y está lista para integrarse
3. Puedes agregarlo después si ves errores frecuentes

**Prioridad:**
1. ✅ Logging (YA HECHO)
2. ⏳ Background Job (si Johan lo necesita)
3. 🔜 FluentValidation (solo si hay problemas)

---

## 📊 Estado Actual del Proyecto

| Feature | Estado | Prioridad |
|---------|--------|-----------|
| Endpoints CRUD | ✅ Completo | Alta |
| Transacciones Recurrentes | ✅ Completo | Alta |
| Logging (Serilog) | ✅ Completo | Alta |
| Background Job | ⏳ Pendiente | Media |
| FluentValidation | ⏳ Pendiente | Baja |

---

## 🎯 Próximo Paso

**Exponer con ngrok** para que Johan pueda consumir la API.

¿Continuamos con eso?
