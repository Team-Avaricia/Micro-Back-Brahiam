# 📚 Lista Completa de Endpoints - MS Core (Juez IA)

## 🔗 Base URL
```
http://localhost:5000
```

---

## 1️⃣ Usuarios

### POST `/api/User`
Crea un nuevo usuario.

**Request:**
```json
{
  "name": "Brahiam",
  "email": "brahiam@test.com",
  "phoneNumber": "+573001234567",
  "initialBalance": 1000000
}
```

**Response:**
```json
{
  "message": "Usuario creado exitosamente",
  "userId": "guid-del-usuario",
  "name": "Brahiam",
  "email": "brahiam@test.com",
  "phoneNumber": "+573001234567",
  "currentBalance": 1000000
}
```

---

### GET `/api/User/{id}`
Obtiene un usuario por ID.

**Response:**
```json
{
  "id": "guid",
  "name": "Brahiam",
  "email": "brahiam@test.com",
  "phoneNumber": "+573001234567",
  "currentBalance": 1000000,
  "createdAt": "2025-11-26T..."
}
```

---

### GET `/api/User/email/{email}`
Obtiene un usuario por email.

---

### GET `/api/User/phone/{phoneNumber}`
Obtiene un usuario por número de teléfono.

---

## 2️⃣ Validación de Gastos

### POST `/api/SpendingValidation/validate`
Valida si un gasto es permitido según las reglas del usuario.

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

---

## 3️⃣ Transacciones

### POST `/api/Transaction`
Crea una nueva transacción y actualiza el saldo automáticamente.

**Request:**
```json
{
  "userId": "guid-del-usuario",
  "amount": 50000,
  "type": "Expense",
  "category": "Comida",
  "description": "Almuerzo",
  "source": "Telegram"
}
```

**Valores para `type`:**
- `"Income"` - Ingreso
- `"Expense"` - Gasto

**Valores para `source`:**
- `"Manual"` - Ingreso manual
- `"Telegram"` - Desde Telegram
- `"WhatsApp"` - Desde WhatsApp
- `"Automatic"` - Desde n8n/Email

**Response:**
```json
{
  "id": "guid-de-la-transaccion",
  "userId": "guid-del-usuario",
  "amount": 50000,
  "type": "Expense",
  "category": "Comida",
  "description": "Almuerzo",
  "source": "Telegram",
  "createdAt": "2025-11-26T10:30:00Z"
}
```

---

### GET `/api/Transaction/user/{userId}`
Obtiene todas las transacciones de un usuario (ordenadas por fecha descendente).

**Response:**
```json
[
  {
    "id": "guid-1",
    "userId": "guid-del-usuario",
    "amount": 50000,
    "type": "Expense",
    "category": "Comida",
    "description": "Almuerzo",
    "source": "Telegram",
    "date": "2025-11-26T10:30:00Z",
    "createdAt": "2025-11-26T10:30:00Z"
  }
]
```

---

### GET `/api/Transaction/{id}`
Obtiene una transacción específica por ID.

---

### DELETE `/api/Transaction/{id}`
Elimina una transacción y **revierte el saldo del usuario automáticamente**.

**Response:**
```json
{
  "message": "Transacción eliminada y saldo revertido exitosamente"
}
```

---

## 4️⃣ Reglas Financieras

### POST `/api/FinancialRule`
Crea una nueva regla financiera.

**Request:**
```json
{
  "userId": "guid-del-usuario",
  "type": "MonthlyBudget",
  "category": "Comida",
  "amountLimit": 500000,
  "period": "Monthly"
}
```

**Valores para `type`:**
- `"DailyLimit"` - Límite diario
- `"MonthlyBudget"` - Presupuesto mensual
- `"CategoryLimit"` - Límite por categoría
- `"SavingsGoal"` - Meta de ahorro

**Valores para `period`:**
- `"Daily"` - Diario
- `"Weekly"` - Semanal
- `"Monthly"` - Mensual
- `"Yearly"` - Anual

**Response:**
```json
{
  "id": "guid-de-la-regla",
  "userId": "guid-del-usuario",
  "type": "MonthlyBudget",
  "category": "Comida",
  "amountLimit": 500000,
  "period": "Monthly",
  "isActive": true,
  "createdAt": "2025-11-26T10:30:00Z"
}
```

---

### GET `/api/FinancialRule/user/{userId}`
Obtiene todas las reglas **activas** de un usuario.

**Response:**
```json
[
  {
    "id": "guid-1",
    "userId": "guid-del-usuario",
    "type": "MonthlyBudget",
    "category": "Comida",
    "amountLimit": 500000,
    "period": "Monthly",
    "isActive": true,
    "createdAt": "2025-11-26T..."
  }
]
```

---

### GET `/api/FinancialRule/{id}`
Obtiene una regla específica por ID.

---

### PATCH `/api/FinancialRule/{id}/deactivate`
Desactiva una regla (soft delete - la regla sigue en BD pero `isActive = false`).

**Response:**
```json
{
  "message": "Regla desactivada exitosamente"
}
```

---

### DELETE `/api/FinancialRule/{id}`
Elimina permanentemente una regla.

**Response:**
```json
{
  "message": "Regla eliminada exitosamente"
}
```

---

## 📊 Resumen de Endpoints

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| **Usuarios** |
| POST | `/api/User` | Crear usuario |
| GET | `/api/User/{id}` | Obtener usuario por ID |
| GET | `/api/User/email/{email}` | Obtener usuario por email |
| GET | `/api/User/phone/{phoneNumber}` | Obtener usuario por teléfono |
| **Validación** |
| POST | `/api/SpendingValidation/validate` | Validar gasto |
| **Transacciones** |
| POST | `/api/Transaction` | Crear transacción |
| GET | `/api/Transaction/user/{userId}` | Listar transacciones de usuario |
| GET | `/api/Transaction/{id}` | Obtener transacción por ID |
| DELETE | `/api/Transaction/{id}` | Eliminar transacción (revierte saldo) |
| **Reglas Financieras** |
| POST | `/api/FinancialRule` | Crear regla |
| GET | `/api/FinancialRule/user/{userId}` | Listar reglas activas de usuario |
| GET | `/api/FinancialRule/{id}` | Obtener regla por ID |
| PATCH | `/api/FinancialRule/{id}/deactivate` | Desactivar regla |
| DELETE | `/api/FinancialRule/{id}` | Eliminar regla |

---

## 📂 Categorías Soportadas

### Gastos:
- `Comida`
- `Transporte`
- `Entretenimiento`
- `Salud`
- `Educación`
- `Hogar`
- `Ropa`
- `Tecnología`
- `Otros`

### Ingresos:
- `Salario`
- `Freelance`
- `Inversiones`
- `Regalos`
- `Otros`

---

## 🧪 Cómo Probar

1. **Abre Swagger**: `https://localhost:XXXX/swagger`
2. **Crea un usuario** con `POST /api/User`
3. **Copia el `userId`** de la respuesta
4. **Crea una regla** con `POST /api/FinancialRule`
5. **Valida un gasto** con `POST /api/SpendingValidation/validate`
6. **Crea una transacción** con `POST /api/Transaction`

---

**Total de Endpoints:** 15
