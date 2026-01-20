# SMOKE TESTS - SALONES (AULAS)

## Descripción General

Este documento define los casos de prueba de humo para validar el módulo **SALONES** (gestión de aulas físicas) implementado en la API de Tlaoami.

**Contexto**: Fase 1 Primaria - cada grupo tiene un salón asignado (sin horarios).

## Endpoints Principales

```
GET    /api/v1/Salones
GET    /api/v1/Salones/{id}
POST   /api/v1/Salones
PUT    /api/v1/Salones/{id}
DELETE /api/v1/Salones/{id}
```

---

## Casos de Prueba

### CASO 1: Crear Salón Exitosamente

**Descripción**: Crear un nuevo salón con código único.

**Request**:
```
POST /api/v1/Salones
Authorization: Bearer {JWT_ADMIN}
Content-Type: application/json

{
  "codigo": "A101",
  "nombre": "Aula 101 - Primer Piso",
  "capacidad": 30
}
```

**Respuesta Esperada**:
```
HTTP 201 Created
Location: /api/v1/Salones/{id}

Body:
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "codigo": "A101",
  "nombre": "Aula 101 - Primer Piso",
  "capacidad": 30,
  "activo": true,
  "createdAt": "2026-01-20T22:30:00Z",
  "updatedAt": null
}
```

**Validaciones**:
- [ ] HTTP 201
- [ ] Header Location presente
- [ ] Código único asignado
- [ ] Activo = true por defecto
- [ ] CreatedAt en UTC

**cURL**:
```bash
curl -X POST https://localhost:7000/api/v1/Salones \
  -H "Authorization: Bearer $JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "codigo": "A101",
    "nombre": "Aula 101 - Primer Piso",
    "capacidad": 30
  }'
```

---

### CASO 2: Crear Salón con Código Duplicado (409)

**Descripción**: Intentar crear salón con código ya existente.

**Condiciones Previas**:
- Ya existe salón con código "A101"

**Request**:
```json
{
  "codigo": "A101",
  "nombre": "Intento Duplicado",
  "capacidad": 25
}
```

**Respuesta Esperada**:
```
HTTP 409 Conflict

{
  "code": "SALON_CODIGO_DUPLICADO",
  "message": "Ya existe un salón con código 'A101'"
}
```

**Validaciones**:
- [ ] HTTP 409
- [ ] Error code = SALON_CODIGO_DUPLICADO
- [ ] No se crea registro en BD

---

### CASO 3: Listar Todos los Salones

**Descripción**: Obtener listado completo de salones.

**Request**:
```
GET /api/v1/Salones
Authorization: Bearer {JWT_TOKEN}
```

**Respuesta Esperada**:
```
HTTP 200 OK

[
  {
    "id": "...",
    "codigo": "A101",
    "nombre": "Aula 101",
    "capacidad": 30,
    "activo": true,
    "createdAt": "2026-01-20T22:30:00Z"
  },
  {
    "id": "...",
    "codigo": "A102",
    "nombre": "Aula 102",
    "capacidad": 25,
    "activo": true,
    "createdAt": "2026-01-20T22:35:00Z"
  }
]
```

**Validaciones**:
- [ ] HTTP 200
- [ ] Array de salones
- [ ] Ordenado por código (alfabético)

**cURL**:
```bash
curl -X GET https://localhost:7000/api/v1/Salones \
  -H "Authorization: Bearer $JWT_TOKEN"
```

---

### CASO 4: Filtrar Salones Activos

**Descripción**: Obtener solo salones activos.

**Request**:
```
GET /api/v1/Salones?activo=true
Authorization: Bearer {JWT_TOKEN}
```

**Respuesta Esperada**:
```
HTTP 200 OK

[
  {
    "id": "...",
    "codigo": "A101",
    "activo": true
  }
]
```

**Validaciones**:
- [ ] Solo salones con Activo = true
- [ ] No aparecen inactivos

---

### CASO 5: Obtener Salón por ID

**Descripción**: Consultar detalles de un salón específico.

**Request**:
```
GET /api/v1/Salones/{id}
Authorization: Bearer {JWT_TOKEN}
```

**Respuesta Esperada (200)**:
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "codigo": "A101",
  "nombre": "Aula 101",
  "capacidad": 30,
  "activo": true,
  "createdAt": "2026-01-20T22:30:00Z",
  "updatedAt": null
}
```

**Respuesta Si No Existe (404)**:
```json
{
  "code": "SALON_NO_ENCONTRADO",
  "message": "Salón no encontrado"
}
```

**Validaciones**:
- [ ] HTTP 200 si existe
- [ ] HTTP 404 si no existe

---

### CASO 6: Actualizar Salón

**Descripción**: Modificar datos de un salón existente.

**Request**:
```
PUT /api/v1/Salones/{id}
Authorization: Bearer {JWT_ADMIN}
Content-Type: application/json

{
  "codigo": "A101-NUEVO",
  "nombre": "Aula 101 Renovada",
  "capacidad": 35,
  "activo": true
}
```

**Respuesta Esperada**:
```
HTTP 200 OK

{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "codigo": "A101-NUEVO",
  "nombre": "Aula 101 Renovada",
  "capacidad": 35,
  "activo": true,
  "createdAt": "2026-01-20T22:30:00Z",
  "updatedAt": "2026-01-20T23:00:00Z"
}
```

**Validaciones**:
- [ ] HTTP 200
- [ ] Campos actualizados
- [ ] UpdatedAt presente y posterior a CreatedAt

**cURL**:
```bash
curl -X PUT https://localhost:7000/api/v1/Salones/{id} \
  -H "Authorization: Bearer $JWT_ADMIN" \
  -H "Content-Type: application/json" \
  -d '{
    "nombre": "Aula 101 Renovada",
    "capacidad": 35
  }'
```

---

### CASO 7: Desactivar Salón (Soft Delete)

**Descripción**: Marcar salón como inactivo sin eliminarlo.

**Request**:
```
PUT /api/v1/Salones/{id}
Authorization: Bearer {JWT_ADMIN}
Content-Type: application/json

{
  "activo": false
}
```

**Respuesta Esperada**:
```
HTTP 200 OK

{
  "id": "...",
  "codigo": "A101",
  "activo": false,
  "updatedAt": "2026-01-20T23:05:00Z"
}
```

**Validaciones**:
- [ ] Activo = false
- [ ] Salón NO aparece en filtro ?activo=true
- [ ] Sigue existiendo en BD

---

### CASO 8: Eliminar Salón SIN Grupos Asignados

**Descripción**: Eliminación física de salón sin dependencias.

**Condiciones Previas**:
- Salón NO tiene grupos asignados

**Request**:
```
DELETE /api/v1/Salones/{id}
Authorization: Bearer {JWT_ADMIN}
```

**Respuesta Esperada**:
```
HTTP 204 No Content
```

**Validaciones**:
- [ ] HTTP 204
- [ ] Salón eliminado de BD
- [ ] GET /api/v1/Salones/{id} retorna 404

**cURL**:
```bash
curl -X DELETE https://localhost:7000/api/v1/Salones/{id} \
  -H "Authorization: Bearer $JWT_ADMIN"
```

---

### CASO 9: Eliminar Salón CON Grupos Asignados (409)

**Descripción**: Bloqueo de eliminación si hay grupos usando el salón.

**Condiciones Previas**:
- Salón tiene al menos 1 grupo asignado (Grupo.SalonId = {salonId})

**Request**:
```
DELETE /api/v1/Salones/{id}
Authorization: Bearer {JWT_ADMIN}
```

**Respuesta Esperada**:
```
HTTP 409 Conflict

{
  "code": "SALON_CON_GRUPOS_ASIGNADOS",
  "message": "No se puede eliminar el salón porque tiene grupos asignados"
}
```

**Validaciones**:
- [ ] HTTP 409
- [ ] Salón NO eliminado
- [ ] Mensaje claro indica causa de bloqueo

---

## Integración con Grupos

### CASO 10: Asignar Salón a Grupo

**Descripción**: Actualizar grupo para asignarle un salón.

**Request**:
```
PUT /api/v1/Grupos/{grupoId}
Authorization: Bearer {JWT_ADMIN}
Content-Type: application/json

{
  "salonId": "550e8400-e29b-41d4-a716-446655440000"
}
```

**Respuesta Esperada**:
```
HTTP 200 OK

{
  "id": "...",
  "nombre": "3A",
  "grado": 3,
  "turno": "Matutino",
  "salonId": "550e8400-e29b-41d4-a716-446655440000",
  "salon": {
    "codigo": "A101",
    "nombre": "Aula 101"
  }
}
```

**Validaciones**:
- [ ] GrupoDto incluye SalonId
- [ ] Relación FK válida
- [ ] Salón puede ser null (opcional)

---

## Matriz de Validación

| Caso | Input | HTTP | Code | Esperado | ✓ |
|------|-------|------|------|----------|---|
| 1 | Código único | 201 | - | Salón creado | |
| 2 | Código duplicado | 409 | DUPLICADO | Rechazado | |
| 3 | Listar todos | 200 | - | Array completo | |
| 4 | Filtro activo | 200 | - | Solo activos | |
| 5 | Get by ID | 200/404 | - | Detalle o error | |
| 6 | Update | 200 | - | Actualizado | |
| 7 | Desactivar | 200 | - | Activo=false | |
| 8 | Delete sin grupos | 204 | - | Eliminado | |
| 9 | Delete con grupos | 409 | CON_GRUPOS | Bloqueado | |
| 10 | Asignar a grupo | 200 | - | FK asignada | |

---

## Configuración

### Roles Requeridos

- **Admin**: CRUD completo (POST, PUT, DELETE)
- **Todos los usuarios autenticados**: GET (lectura)

### JWT Token

```bash
export JWT_ADMIN="eyJhbGciOiJIUzI1NiIsInR5cCI6..."
```

### Pre-setup BD

Asegurar que:
- Base de datos con migración `AddSalones` aplicada
- Tabla `Salones` existe
- Columna `Grupos.SalonId` existe (nullable)

---

## Reglas de Negocio Validadas

✅ **Código Único**: No se permiten duplicados (unique index)  
✅ **Capacidad Opcional**: Puede ser null (sin límite)  
✅ **Soft Delete**: Desactivar con `Activo=false` en vez de eliminar  
✅ **Protección FK**: No se puede eliminar salón con grupos asignados  
✅ **Asignación Opcional**: Grupo puede no tener salón (SalonId nullable)  
✅ **Ordenamiento**: Listados ordenados por código alfabéticamente  

---

## Notas Técnicas

⚠️ **Sin Horarios en Fase 1**: El salón es asignación fija por grupo (no por materia/horario)

✅ **Migración**: `20260120225545_AddSalones.cs`

✅ **Tests**: 6/6 pasando en `SalonServiceTests.cs`

✅ **RBAC**: Solo Admin puede crear/modificar/eliminar

---

**Última actualización**: 2026-01-20  
**Status**: ✅ LISTO PARA QA
