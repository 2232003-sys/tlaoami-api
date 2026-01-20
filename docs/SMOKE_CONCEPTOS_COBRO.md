# Smoke Test: Conceptos de Cobro

## Objetivo
Validar la operación completa del módulo de Conceptos de Cobro incluyendo:
- Crear conceptos
- Validar conflictos de clave duplicada
- Listar con filtros
- Actualizar
- Inactivar

## Prerequisites
- API corriendo en `http://localhost:3000`
- Token JWT válido para rol Admin o Administrativo

## Paso 1: Obtener Token de Autenticación

**Request:**
```bash
curl -X POST http://localhost:3000/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "username": "admin",
    "password": "admin123"
  }'
```

**Response esperada (200 OK):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "username": "admin"
}
```

Guardar el `token` para los siguientes pasos.

---

## Paso 2: Crear Concepto "COLEGIATURA"

**Request:**
```bash
curl -X POST http://localhost:3000/api/v1/ConceptosCobro \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {token}" \
  -d '{
    "clave": "COLEGIATURA",
    "nombre": "Colegiatura Mensual",
    "periodicidad": "Mensual",
    "requiereCFDI": true,
    "activo": true,
    "orden": 1
  }'
```

**Response esperada (201 Created):**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440001",
  "clave": "COLEGIATURA",
  "nombre": "Colegiatura Mensual",
  "periodicidad": "Mensual",
  "requiereCFDI": true,
  "activo": true,
  "orden": 1,
  "createdAtUtc": "2026-01-19T15:30:00Z",
  "updatedAtUtc": null
}
```

---

## Paso 3: Crear Concepto "REINSCRIPCION"

**Request:**
```bash
curl -X POST http://localhost:3000/api/v1/ConceptosCobro \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {token}" \
  -d '{
    "clave": "REINSCRIPCION",
    "nombre": "Cuota de Reinscripción",
    "periodicidad": "Unica",
    "requiereCFDI": true,
    "activo": true,
    "orden": 2
  }'
```

**Response esperada (201 Created):**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440002",
  "clave": "REINSCRIPCION",
  "nombre": "Cuota de Reinscripción",
  "periodicidad": "Unica",
  "requiereCFDI": true,
  "activo": true,
  "orden": 2,
  "createdAtUtc": "2026-01-19T15:30:05Z",
  "updatedAtUtc": null
}
```

---

## Paso 4: Intentar Crear "COLEGIATURA" Nuevamente → 409 Conflict

**Request:**
```bash
curl -X POST http://localhost:3000/api/v1/ConceptosCobro \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {token}" \
  -d '{
    "clave": "COLEGIATURA",
    "nombre": "Otra Colegiatura",
    "periodicidad": "Mensual"
  }'
```

**Response esperada (409 Conflict):**
```json
{
  "code": "CLAVE_DUPLICADA",
  "message": "Ya existe un concepto de cobro con clave 'COLEGIATURA'."
}
```

---

## Paso 5: Listar Conceptos Activos

**Request:**
```bash
curl -X GET "http://localhost:3000/api/v1/ConceptosCobro?activo=true" \
  -H "Authorization: Bearer {token}"
```

**Response esperada (200 OK):**
```json
[
  {
    "id": "550e8400-e29b-41d4-a716-446655440001",
    "clave": "COLEGIATURA",
    "nombre": "Colegiatura Mensual",
    "periodicidad": "Mensual",
    "requiereCFDI": true,
    "activo": true,
    "orden": 1,
    "createdAtUtc": "2026-01-19T15:30:00Z",
    "updatedAtUtc": null
  },
  {
    "id": "550e8400-e29b-41d4-a716-446655440002",
    "clave": "REINSCRIPCION",
    "nombre": "Cuota de Reinscripción",
    "periodicidad": "Unica",
    "requiereCFDI": true,
    "activo": true,
    "orden": 2,
    "createdAtUtc": "2026-01-19T15:30:05Z",
    "updatedAtUtc": null
  }
]
```

---

## Paso 6: Actualizar Orden y Nombre

**Request:**
```bash
curl -X PUT http://localhost:3000/api/v1/ConceptosCobro/550e8400-e29b-41d4-a716-446655440001 \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {token}" \
  -d '{
    "nombre": "Colegiatura Mensual Actualizada",
    "orden": 10
  }'
```

**Response esperada (200 OK):**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440001",
  "clave": "COLEGIATURA",
  "nombre": "Colegiatura Mensual Actualizada",
  "periodicidad": "Mensual",
  "requiereCFDI": true,
  "activo": true,
  "orden": 10,
  "createdAtUtc": "2026-01-19T15:30:00Z",
  "updatedAtUtc": "2026-01-19T15:35:00Z"
}
```

---

## Paso 7: Inactivar Concepto (Soft Delete)

**Request:**
```bash
curl -X DELETE http://localhost:3000/api/v1/ConceptosCobro/550e8400-e29b-41d4-a716-446655440001 \
  -H "Authorization: Bearer {token}"
```

**Response esperada (204 No Content):**
```
(sin body)
```

**Verificar inactivación - GET:**
```bash
curl -X GET "http://localhost:3000/api/v1/ConceptosCobro?activo=false" \
  -H "Authorization: Bearer {token}"
```

**Response (200 OK):**
```json
[
  {
    "id": "550e8400-e29b-41d4-a716-446655440001",
    "clave": "COLEGIATURA",
    "nombre": "Colegiatura Mensual Actualizada",
    "periodicidad": "Mensual",
    "requiereCFDI": true,
    "activo": false,
    "orden": 10,
    "createdAtUtc": "2026-01-19T15:30:00Z",
    "updatedAtUtc": "2026-01-19T15:36:00Z"
  }
]
```

---

## Validaciones Clave

| Escenario | HTTP Status | Código | Notas |
|-----------|-------------|--------|-------|
| Crear concepto válido | 201 | - | Retorna Location header |
| Clave duplicada | 409 | `CLAVE_DUPLICADA` | Case-insensitive |
| Validación de longitud | 400 | `CLAVE_LONGITUD_INVALIDA` | 3-30 caracteres |
| ID no existe | 404 | `CONCEPTO_NO_ENCONTRADO` | GET/PUT/DELETE |
| Sin autorización | 401 | - | Falta token JWT |
| Rol insuficiente | 403 | - | Consulta puede GET, Admin+Adm pueden POST/PUT |

---

## Archivo de migración esperado

```
src/Tlaoami.Infrastructure/Migrations/[Timestamp]_AddConceptosCobro.cs
```

Ejecutar migración:
```bash
dotnet ef database update -p src/Tlaoami.Infrastructure -s src/Tlaoami.API
```

Verificar tabla en PostgreSQL:
```sql
\d "ConceptosCobro"
```

Debe mostrar columnas: Id, Clave, Nombre, Periodicidad, RequiereCFDI, Activo, Orden, CreatedAtUtc, UpdatedAtUtc
