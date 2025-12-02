# 🌳 Git Flow - Estrategia de Ramas

## ✅ Ramas Creadas

```
main (producción)
  └── develop (desarrollo)
       └── feature/brahiam/jwt-authentication (tu rama de trabajo)
```

---

## 📊 Estado Actual

### Ramas Locales
- ✅ `main` - Rama principal (producción)
- ✅ `develop` - Rama de desarrollo
- ✅ `feature/brahiam/jwt-authentication` - Tu rama de trabajo (ACTUAL)

### Ramas Remotas (GitHub)
- ✅ `origin/main`
- ✅ `origin/develop`
- ✅ `origin/feature/brahiam/jwt-authentication`

**Todas las ramas tienen el mismo código** (7 commits)

---

## 🔄 Flujo de Trabajo

### 1. Trabajar en tu Rama
```bash
# Ya estás en feature/brahiam/jwt-authentication
git status  # Verificar rama actual

# Hacer cambios (implementar JWT)
# ...

# Guardar cambios
git add .
git commit -m "feat(auth): implement JWT authentication"

# Subir a GitHub
git push origin feature/brahiam/jwt-authentication
```

### 2. Pull Request a `develop`
```bash
# Cuando termines JWT, crear PR en GitHub:
# feature/brahiam/jwt-authentication → develop
```

**En GitHub:**
1. Ve a: https://github.com/Brahiamriwi/Micro-Back-Brahiam/pulls
2. Click "New Pull Request"
3. Base: `develop` ← Compare: `feature/brahiam/jwt-authentication`
4. Título: "feat(auth): Implement JWT Authentication"
5. Descripción: Explicar qué implementaste
6. Click "Create Pull Request"
7. Revisar cambios
8. Click "Merge Pull Request"

### 3. Pull Request a `main` (Producción)
```bash
# Después de aprobar en develop, crear PR:
# develop → main
```

**En GitHub:**
1. New Pull Request
2. Base: `main` ← Compare: `develop`
3. Título: "Release: JWT Authentication"
4. Merge cuando esté listo para producción

---

## 📋 Comandos Útiles

### Ver en qué rama estás
```bash
git branch
# * feature/brahiam/jwt-authentication  ← Estás aquí
```

### Cambiar de rama
```bash
git checkout develop        # Ir a develop
git checkout main           # Ir a main
git checkout feature/brahiam/jwt-authentication  # Volver a tu rama
```

### Actualizar tu rama con develop
```bash
# Si develop tiene cambios nuevos
git checkout feature/brahiam/jwt-authentication
git pull origin develop
```

### Ver historial
```bash
git log --oneline --graph --all
```

---

## 🎯 Próximos Pasos: Implementar JWT

### Fase 1: Instalación de Paquetes
```bash
dotnet add src/API/API.csproj package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add src/API/API.csproj package System.IdentityModel.Tokens.Jwt
dotnet add src/API/API.csproj package BCrypt.Net-Next
```

### Fase 2: Crear Entidades
- `RefreshToken` entity
- Modificar `User` para incluir `PasswordHash`

### Fase 3: Crear Servicios
- `AuthService` - Login, Register, RefreshToken
- `TokenService` - Generar JWT

### Fase 4: Crear Controller
- `AuthController` con endpoints:
  - `POST /api/Auth/register`
  - `POST /api/Auth/login`
  - `POST /api/Auth/refresh-token`

### Fase 5: Configurar JWT
- Agregar configuración en `appsettings.json`
- Configurar middleware en `Program.cs`

### Fase 6: Proteger Endpoints
- Agregar `[Authorize]` a controllers existentes

### Fase 7: Commit y PR
```bash
git add .
git commit -m "feat(auth): implement JWT authentication with refresh tokens"
git push origin feature/brahiam/jwt-authentication
# Crear PR en GitHub
```

---

## ✅ Ventajas de este Flujo

1. **Trazabilidad:** Cada cambio tiene su historia
2. **Seguridad:** `main` siempre está estable
3. **Colaboración:** Fácil revisar cambios antes de merge
4. **Rollback:** Puedes volver atrás si algo falla
5. **Profesional:** Estándar en la industria

---

## 🚀 Estado Actual

- ✅ Git Flow configurado
- ✅ 3 ramas creadas y sincronizadas
- ✅ Listo para implementar JWT
- 📍 **Estás en:** `feature/brahiam/jwt-authentication`

**¿Empezamos con JWT?** 🔐
