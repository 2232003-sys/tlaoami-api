# SMOKE TEST: Aviso de Privacidad & Cumplimiento

Validar flujo completo de Aviso de Privacidad con aceptación y middleware de cumplimiento.

**Entidades:**
- `AvisoPrivacidad`: Id, Version, Contenido, Vigente, PublicadoEnUtc, CreatedAtUtc
- `AceptacionAvisoPrivacidad`: Id, AvisoPrivacidadId, UsuarioId, AceptadoEnUtc, Ip?, UserAgent?

**Índices:**
- `AvisosPrivacidad.Vigente`: UNIQUE (partial, solo Vigente=true)
- `AceptacionesAvisoPrivacidad.(UsuarioId, AvisoPrivacidadId)`: UNIQUE (una aceptación por usuario+aviso)

**Endpoints:**
- `GET /api/v1/AvisoPrivacidad/activo` - público, sin auth
- `GET /api/v1/AvisoPrivacidad/estado` - requiere JWT
- `POST /api/v1/AvisoPrivacidad/aceptar` - requiere JWT, idempotente

**Middleware de Cumplimiento:**
Bloquea acceso (403 PRIVACIDAD_PENDIENTE) si usuario no aceptó aviso vigente.
Excepto: endpoints de privacidad, /auth/login, /swagger, /healthz.

---

## Paso 1: Consultar aviso vigente (público)

```bash
curl -X GET "http://localhost:3000/api/v1/AvisoPrivacidad/activo" \
  -H "Content-Type: application/json" \
  -v
```

**Respuesta esperada (200 OK):**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "version": "2026-01-19",
  "contenido": "AVISO DE PRIVACIDAD\n\nEn Tlaoami, protegemos su información...",
  "publicadoEnUtc": "2026-01-19T10:30:00Z"
}
```

**Validación:**
- ✅ Status 200
- ✅ Id y Version presentes
- ✅ Contenido no vacío
- ✅ Sin autorización requerida

---

## Paso 2: Autenticarse (login)

```bash
curl -X POST "http://localhost:3000/api/v1/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "admin",
    "password": "admin123"
  }' \
  -v
```

**Respuesta esperada (200 OK):**
```json
{
  "token": "eyJhbGc...aaabbbccc...",
  "usuario": {
    "id": "11111111-1111-1111-1111-111111111111",
    "username": "admin",
    "role": "Admin"
  }
}
```

**Extrae el JWT para pasos siguientes:**
```bash
TOKEN="eyJhbGc...aaabbbccc..."
USUARIO_ID="11111111-1111-1111-1111-111111111111"
```

---

## Paso 3: Consultar estado de aceptación (sin aceptar aún)

```bash
curl -X GET "http://localhost:3000/api/v1/AvisoPrivacidad/estado" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -v
```

**Respuesta esperada (200 OK):**
```json
{
  "requiereAceptacion": true,
  "versionActual": "2026-01-19",
  "aceptadoEnUtc": null
}
```

**Validación:**
- ✅ Status 200
- ✅ `requiereAceptacion`: true
- ✅ `aceptadoEnUtc`: null
- ✅ JWT requerido (sin JWT → 401)

---

## Paso 4: Intentar acceder a endpoint protegido sin aceptar

```bash
curl -X GET "http://localhost:3000/api/v1/Ciclos" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -v
```

**Respuesta esperada (403 Forbidden):**
```json
{
  "code": "PRIVACIDAD_PENDIENTE",
  "message": "Debe aceptar el aviso de privacidad vigente para acceder a este recurso."
}
```

**Validación:**
- ✅ Status 403 (bloqueado por middleware)
- ✅ Código "PRIVACIDAD_PENDIENTE"
- ✅ Usuario no puede operar sin aceptar

---

## Paso 5: Aceptar aviso

```bash
curl -X POST "http://localhost:3000/api/v1/AvisoPrivacidad/aceptar" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{}' \
  -v
```

**Respuesta esperada (200 OK):**
```json
{
  "requiereAceptacion": false,
  "versionActual": "2026-01-19",
  "aceptadoEnUtc": "2026-01-19T11:45:30Z"
}
```

**Validación:**
- ✅ Status 200
- ✅ `requiereAceptacion`: false
- ✅ `aceptadoEnUtc`: presente con timestamp UTC
- ✅ IP y UserAgent guardados en BD

---

## Paso 6: Verificar estado después de aceptar

```bash
curl -X GET "http://localhost:3000/api/v1/AvisoPrivacidad/estado" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -v
```

**Respuesta esperada (200 OK):**
```json
{
  "requiereAceptacion": false,
  "versionActual": "2026-01-19",
  "aceptadoEnUtc": "2026-01-19T11:45:30Z"
}
```

**Validación:**
- ✅ Status 200
- ✅ `requiereAceptacion`: false
- ✅ Timestamp aceptación presente

---

## Paso 7: Acceder a endpoint protegido después de aceptar

```bash
curl -X GET "http://localhost:3000/api/v1/Ciclos" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -v
```

**Respuesta esperada (200 OK):**
```json
[
  { "id": "...", "nombre": "2024-2025", ... },
  ...
]
```

**Validación:**
- ✅ Status 200
- ✅ Acceso permitido (middleware no bloquea)
- ✅ Usuario ahora puede operar

---

## Paso 8: Verificar idempotencia (aceptar nuevamente)

```bash
curl -X POST "http://localhost:3000/api/v1/AvisoPrivacidad/aceptar" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{}' \
  -v
```

**Respuesta esperada (200 OK - misma que paso 5):**
```json
{
  "requiereAceptacion": false,
  "versionActual": "2026-01-19",
  "aceptadoEnUtc": "2026-01-19T11:45:30Z"
}
```

**Validación:**
- ✅ Status 200 (no 409 duplicado)
- ✅ No crea nueva aceptación (índice único UsuarioId+AvisoId)
- ✅ Mismo timestamp (idempotente)

---

## Paso 9: Probar con segundo usuario

```bash
# Login segundo usuario
curl -X POST "http://localhost:3000/api/v1/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "admin1",
    "password": "admin123"
  }'

# Guardar TOKEN2 y USUARIO_ID2
TOKEN2="..."
USUARIO_ID2="..."

# Verificar estado (debe requerir aceptación)
curl -X GET "http://localhost:3000/api/v1/AvisoPrivacidad/estado" \
  -H "Authorization: Bearer $TOKEN2" \
  -H "Content-Type: application/json"
```

**Respuesta esperada:**
```json
{
  "requiereAceptacion": true,
  "versionActual": "2026-01-19",
  "aceptadoEnUtc": null
}
```

**Validación:**
- ✅ Cada usuario tiene estado independiente
- ✅ Aceptación de un usuario no afecta otro

---

## Paso 10: Sin aviso vigente (test edge case)

Desactivar aviso vigente en BD (SQL):
```sql
UPDATE "AvisosPrivacidad" SET "Vigente" = false;
```

Consultar /activo:
```bash
curl -X GET "http://localhost:3000/api/v1/AvisoPrivacidad/activo" \
  -H "Content-Type: application/json" \
  -v
```

**Respuesta esperada (404 Not Found):**
```json
{
  "code": "AVISO_NO_VIGENTE",
  "message": "No hay aviso de privacidad vigente publicado."
}
```

**Validación:**
- ✅ Status 404
- ✅ Código "AVISO_NO_VIGENTE"

---

## Matriz de Validación

| Caso | Endpoint | Auth | Esperado | Status | Notas |
|------|----------|------|----------|--------|-------|
| 1. Ver aviso | GET /activo | No | Aviso vigente | 200 | Público |
| 2. Login | POST /auth/login | No | JWT + usuario | 200 | Demo: admin/admin123 |
| 3. Estado sin aceptar | GET /estado | Sí | requiereAceptacion=true | 200 | Primer acceso |
| 4. Acceso sin aceptar | GET /ciclos | Sí | PRIVACIDAD_PENDIENTE | 403 | Middleware bloquea |
| 5. Aceptar | POST /aceptar | Sí | requiereAceptacion=false | 200 | Crea registro |
| 6. Estado con aceptación | GET /estado | Sí | requiereAceptacion=false | 200 | Timestamp presente |
| 7. Acceso permitido | GET /ciclos | Sí | Datos | 200 | Middleware permite |
| 8. Aceptar segunda vez | POST /aceptar | Sí | requiereAceptacion=false | 200 | Idempotente |
| 9. Otro usuario | GET /estado | Sí (otro) | requiereAceptacion=true | 200 | Aceptación independiente |
| 10. Sin aviso vigente | GET /activo | No | AVISO_NO_VIGENTE | 404 | Edge case |

---

## Checklist de Cumplimiento

- ✅ Entidades creadas (AvisoPrivacidad, AceptacionAvisoPrivacidad)
- ✅ Índices únicos configurados (Vigente, UsuarioId+AvisoId)
- ✅ Endpoints implementados (activo, estado, aceptar)
- ✅ Idempotencia verificada (aceptar dos veces = 200)
- ✅ Middleware de cumplimiento bloqueador
- ✅ JWT requerido en /estado y /aceptar
- ✅ Timestamps en UTC (PublicadoEnUtc, AceptadoEnUtc, CreatedAtUtc)
- ✅ IP y UserAgent capturados en auditoría
- ✅ Manejo de error "sin aviso vigente" (404)
- ✅ Seed de aviso demo (desarrollo)
- ✅ Migración EF Core aplicada

