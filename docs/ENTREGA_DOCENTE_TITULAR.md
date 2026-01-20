# 📋 Entrega: Docente Titular por Grupo (Fase 1 Primaria)

## ✅ Estado: Implementación Completa

**Fecha**: 20 de enero de 2026  
**Épica**: Docente Titular por Grupo (MUST Fase 1)  
**Enfoque**: Minimalista y sólido, sin romper endpoints existentes

---

## 📁 Archivos Modificados/Creados

### Domain Layer (1 archivo modificado)

1. **`src/Tlaoami.Domain/Entities/Grupo.cs`**
   - ✅ Agregado: `DocenteTitularId` (Guid? nullable)
   - ✅ Agregado: `DocenteTitular` (User? navigation property)

### Application Layer (3 archivos modificados)

2. **`src/Tlaoami.Application/Dtos/GrupoDto.cs`**
   - ✅ Agregado a `GrupoDto`: `DocenteTitularId`, `DocenteTitularNombre`
   - ✅ Creado: `GrupoUpdateDocenteTitularDto { DocenteTitularId? }`

3. **`src/Tlaoami.Application/Interfaces/IGrupoService.cs`**
   - ✅ Agregado método: `Task<GrupoDto> AssignDocenteTitularAsync(Guid grupoId, Guid? docenteTitularId)`

4. **`src/Tlaoami.Application/Services/GrupoService.cs`**
   - ✅ Implementado: `AssignDocenteTitularAsync` con validaciones:
     - Grupo existe (404 GRUPO_NO_ENCONTRADO)
     - Docente existe si docenteTitularId != null (404 DOCENTE_NO_ENCONTRADO)
     - Idempotencia: mismo docente ya asignado → 200 sin cambios
     - TODO: validar rol "Docente" cuando RBAC esté completo
   - ✅ Actualizado: `MapToDto` incluye `DocenteTitularId`, `DocenteTitularNombre`
   - ✅ Actualizado: todos los queries incluyen `.Include(g => g.DocenteTitular)`

### Infrastructure Layer (2 archivos)

5. **`src/Tlaoami.Infrastructure/TlaoamiDbContext.cs`**
   - ✅ Configuración: Grupo → DocenteTitular relationship (HasOne/WithMany/SetNull)

6. **`src/Tlaoami.Infrastructure/Migrations/20260120231858_AddDocenteTitularToGrupos.cs`** (NUEVA)
   - ✅ Creada migración: agrega columna `DocenteTitularId` (Guid? nullable) a tabla `Grupos`
   - ✅ FK constraint: `Grupos.DocenteTitularId` → `Users.Id` (ON DELETE SET NULL)
   - ✅ Aplicada a base de datos PostgreSQL

### API Layer (1 archivo modificado)

7. **`src/Tlaoami.API/Controllers/GruposController.cs`**
   - ✅ Agregado endpoint: `[Authorize] PUT /api/v1/Grupos/{id}/docente-titular`
   - ✅ Body: `{ "docenteTitularId": "uuid or null" }`
   - ✅ Respuestas: 200 OK, 404 NOT FOUND, 409 CONFLICT
   - ✅ Manejo de excepciones: NotFoundException, BusinessException

---

## 🔧 Comandos Ejecutados

```bash
# 1. Crear migración
cd /Users/erik/Library/CloudStorage/OneDrive-Personal/2026/Intento\ 3/tlaoami-api
dotnet ef migrations add AddDocenteTitularToGrupos \
  --project src/Tlaoami.Infrastructure \
  --startup-project src/Tlaoami.API

# 2. Verificar build
dotnet build

# 3. Aplicar migración a base de datos
dotnet ef database update \
  --project src/Tlaoami.Infrastructure \
  --startup-project src/Tlaoami.API
```

**Resultado**:
- ✅ Build: 0 errores, 0 advertencias
- ✅ Migración aplicada: `20260120231858_AddDocenteTitularToGrupos`
- ✅ Columna `DocenteTitularId` agregada a tabla `Grupos`

---

## 🧪 Smoke Tests Manuales

### Prerequisitos

```bash
# Obtener token JWT (reemplazar credenciales)
TOKEN=$(curl -s -X POST http://localhost:5271/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"admin123"}' \
  | jq -r '.token')

# Crear usuario docente de prueba (si no existe)
curl -X POST http://localhost:5271/api/v1/users \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "username": "docente.prueba",
    "password": "docente123",
    "role": "Docente"
  }'

# Guardar IDs (reemplazar con IDs reales de tu DB)
GRUPO_ID="<guid-de-grupo-existente>"
DOCENTE_ID="<guid-del-docente-creado>"
```

### Test 1: Consultar grupo sin docente asignado

```bash
curl -X GET "http://localhost:5271/api/v1/Grupos/$GRUPO_ID" \
  -H "Authorization: Bearer $TOKEN"
```

**Esperado**: `docenteTitularId: null, docenteTitularNombre: null`

### Test 2: Asignar docente titular válido

```bash
curl -X PUT "http://localhost:5271/api/v1/Grupos/$GRUPO_ID/docente-titular" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d "{\"docenteTitularId\": \"$DOCENTE_ID\"}"
```

**Esperado**:
- HTTP 200 OK
- Response: `{ "docenteTitularId": "<uuid>", "docenteTitularNombre": "docente.prueba", ... }`

### Test 3: Idempotencia - repetir misma asignación

```bash
curl -X PUT "http://localhost:5271/api/v1/Grupos/$GRUPO_ID/docente-titular" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d "{\"docenteTitularId\": \"$DOCENTE_ID\"}"
```

**Esperado**:
- HTTP 200 OK (sin cambios, idempotente)
- Response igual a Test 2

### Test 4: Quitar docente titular (null)

```bash
curl -X PUT "http://localhost:5271/api/v1/Grupos/$GRUPO_ID/docente-titular" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"docenteTitularId": null}'
```

**Esperado**:
- HTTP 200 OK
- Response: `{ "docenteTitularId": null, "docenteTitularNombre": null, ... }`

### Test 5: Asignar docente inexistente

```bash
curl -X PUT "http://localhost:5271/api/v1/Grupos/$GRUPO_ID/docente-titular" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"docenteTitularId": "00000000-0000-0000-0000-000000000000"}'
```

**Esperado**:
- HTTP 404 NOT FOUND
- Response: `{ "error": "Usuario no encontrado", "code": "DOCENTE_NO_ENCONTRADO" }`

### Test 6: Grupo inexistente

```bash
curl -X PUT "http://localhost:5271/api/v1/Grupos/00000000-0000-0000-0000-000000000000/docente-titular" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d "{\"docenteTitularId\": \"$DOCENTE_ID\"}"
```

**Esperado**:
- HTTP 404 NOT FOUND
- Response: `{ "error": "Grupo no encontrado", "code": "GRUPO_NO_ENCONTRADO" }`

### Test 7: Sin autorización

```bash
curl -X PUT "http://localhost:5271/api/v1/Grupos/$GRUPO_ID/docente-titular" \
  -H "Content-Type: application/json" \
  -d '{"docenteTitularId": null}'
```

**Esperado**:
- HTTP 401 UNAUTHORIZED

---

## 📊 Matriz de Validación

| Test | Escenario | Resultado Esperado | Status |
|------|-----------|-------------------|--------|
| 1 | GET grupo sin docente | `docenteTitularId: null` | ⏳ Pendiente |
| 2 | Asignar docente válido | 200 OK + datos docente | ⏳ Pendiente |
| 3 | Idempotencia (repetir) | 200 OK sin cambios | ⏳ Pendiente |
| 4 | Quitar docente (null) | 200 OK + null | ⏳ Pendiente |
| 5 | Docente inexistente | 404 DOCENTE_NO_ENCONTRADO | ⏳ Pendiente |
| 6 | Grupo inexistente | 404 GRUPO_NO_ENCONTRADO | ⏳ Pendiente |
| 7 | Sin autorización | 401 UNAUTHORIZED | ⏳ Pendiente |

---

## 🔒 Seguridad

- ✅ Endpoint protegido con `[Authorize]`
- ✅ Validación: usuario existe en base de datos
- 🔄 **TODO**: Validar rol "Docente" cuando RBAC esté completo (línea comentada en GrupoService.cs)
- 🔄 **TODO**: Considerar `[Authorize(Policy="AdminOnly")]` si solo Admin/ControlEscolar deben asignar

---

## 📝 Notas Importantes

1. **No Breaking Changes**: 
   - Todos los endpoints existentes de Grupos siguen funcionando
   - `DocenteTitularId` es nullable (no rompe grupos existentes)
   - GET /Grupos ahora incluye campos adicionales (backwards compatible)

2. **Idempotencia**:
   - Asignar el mismo docente múltiples veces no genera error
   - Retorna 200 OK sin realizar cambios en DB

3. **Soft Delete en FK**:
   - Si se elimina un usuario docente, `DocenteTitularId` en Grupos se setea a NULL (DeleteBehavior.SetNull)

4. **Validación de Rol**:
   - Actualmente solo valida que el usuario exista
   - Línea comentada en GrupoService.cs para validar `Role == "Docente"`
   - Descomentar cuando RBAC esté completo

5. **Eager Loading**:
   - Todos los queries de Grupo incluyen `.Include(g => g.DocenteTitular)`
   - Evita N+1 queries al consultar grupos

---

## 🎯 Checklist de Implementación

- [x] Entidad Grupo con DocenteTitularId + navegación User
- [x] DTOs actualizados (GrupoDto, GrupoUpdateDocenteTitularDto)
- [x] Service method con validaciones completas
- [x] Endpoint PUT /Grupos/{id}/docente-titular con [Authorize]
- [x] EF Core configuration (FK, DeleteBehavior.SetNull)
- [x] Migración creada y aplicada
- [x] Build limpio (0 errores, 0 advertencias)
- [x] GET /Grupos/{id} incluye DocenteTitularId + DocenteTitularNombre
- [ ] Smoke tests manuales ejecutados (pendiente)
- [ ] Validación de rol "Docente" (cuando RBAC esté listo)

---

## 🚀 Próximos Pasos

1. **Ejecutar smoke tests** usando curl (ver sección arriba)
2. **Validar rol docente**: Descomentar validación en `GrupoService.cs` cuando RBAC esté completo
3. **RBAC granular**: Evaluar si solo Admin/ControlEscolar pueden asignar docentes
4. **Auditoría**: Considerar timestamps de asignación (quién asignó y cuándo)
5. **Frontend**: Crear UI para asignar docente titular desde módulo de Grupos

---

## 📚 Referencias Técnicas

- **Patrón**: Clean Architecture (Domain → Application → Infrastructure → API)
- **ORM**: Entity Framework Core 8.0.11
- **DB**: PostgreSQL + Npgsql
- **Auth**: JWT Bearer tokens
- **Soft Delete**: DeleteBehavior.SetNull en FK
- **Idempotencia**: Validación en service layer antes de SaveChanges
