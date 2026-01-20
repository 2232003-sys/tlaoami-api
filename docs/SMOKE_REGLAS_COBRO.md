# Smoke Test: Reglas de Cobro por Ciclo

## Objetivo
Validar operación del módulo de Reglas de Cobro incluyendo:
- Crear reglas vinculadas a ciclo + concepto
- Validar índice único lógico
- Listar con filtros
- Actualizar montos/turno
- Inactivar reglas

## Prerequisites
- API corriendo en `http://localhost:3000`
- Token JWT válido (admin o administrativo)
- Ciclos y Conceptos ya existen en BD

## Paso 0: Obtener Ciclo ID y Concepto ID

**Request (obtener ciclos):**
```bash
curl -X GET http://localhost:3000/api/v1/CiclosEscolares \
  -H "Authorization: Bearer {token}"
```

Guardar un `cicloId` de respuesta.

**Request (obtener conceptos):**
```bash
curl -X GET http://localhost:3000/api/v1/ConceptosCobro \
  -H "Authorization: Bearer {token}"
```

Guardar un `conceptoId` (ej: COLEGIATURA que creamos en smoke anterior).

---

## Paso 1: Crear Regla - Colegiatura Grado 1 Matutino

**Request:**
```bash
curl -X POST http://localhost:3000/api/v1/ReglasCobro \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {token}" \
  -d '{
    "cicloId": "{cicloId}",
    "grado": 1,
    "turno": "Matutino",
    "conceptoCobroId": "{conceptoId}",
    "tipoGeneracion": "Mensual",
    "diaCorte": 5,
    "montoBase": 5000.00,
    "activa": true
  }'
```

**Response esperada (201 Created):**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440003",
  "cicloId": "{cicloId}",
  "grado": 1,
  "turno": "Matutino",
  "conceptoCobroId": "{conceptoId}",
  "tipoGeneracion": "Mensual",
  "diaCorte": 5,
  "montoBase": 5000.00,
  "activa": true,
  "createdAtUtc": "2026-01-19T16:00:00Z",
  "updatedAtUtc": null
}
```

Guardar el `id` como `reglaId1`.

---

## Paso 2: Crear Regla - Colegiatura Grado 1 Vespertino

**Request:**
```bash
curl -X POST http://localhost:3000/api/v1/ReglasCobro \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {token}" \
  -d '{
    "cicloId": "{cicloId}",
    "grado": 1,
    "turno": "Vespertino",
    "conceptoCobroId": "{conceptoId}",
    "tipoGeneracion": "Mensual",
    "diaCorte": 5,
    "montoBase": 5000.00,
    "activa": true
  }'
```

**Response esperada (201 Created):** Misma estructura con diferente `id` y `turno: "Vespertino"`.

---

## Paso 3: Intentar Crear Duplicada → 409 Conflict

**Request:** (intenta crear la misma que Paso 1)
```bash
curl -X POST http://localhost:3000/api/v1/ReglasCobro \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {token}" \
  -d '{
    "cicloId": "{cicloId}",
    "grado": 1,
    "turno": "Matutino",
    "conceptoCobroId": "{conceptoId}",
    "tipoGeneracion": "Mensual",
    "diaCorte": 5,
    "montoBase": 4500.00,
    "activa": true
  }'
```

**Response esperada (409 Conflict):**
```json
{
  "code": "REGLA_DUPLICADA",
  "message": "Ya existe una regla con la misma combinación de ciclo, grado, turno, concepto y tipo de generación."
}
```

---

## Paso 4: Listar Reglas del Ciclo

**Request:**
```bash
curl -X GET "http://localhost:3000/api/v1/ReglasCobro/ciclo/{cicloId}?activa=true" \
  -H "Authorization: Bearer {token}"
```

**Response esperada (200 OK):**
```json
[
  {
    "id": "550e8400-e29b-41d4-a716-446655440003",
    "cicloId": "{cicloId}",
    "grado": 1,
    "turno": "Matutino",
    "conceptoCobroId": "{conceptoId}",
    "tipoGeneracion": "Mensual",
    "diaCorte": 5,
    "montoBase": 5000.00,
    "activa": true,
    "createdAtUtc": "2026-01-19T16:00:00Z",
    "updatedAtUtc": null
  },
  {
    "id": "...",
    "cicloId": "{cicloId}",
    "grado": 1,
    "turno": "Vespertino",
    ...
  }
]
```

---

## Paso 5: Actualizar Monto Base y Día Corte

**Request:**
```bash
curl -X PUT http://localhost:3000/api/v1/ReglasCobro/{reglaId1} \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {token}" \
  -d '{
    "montoBase": 5500.00,
    "diaCorte": 10
  }'
```

**Response esperada (200 OK):**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440003",
  "cicloId": "{cicloId}",
  "grado": 1,
  "turno": "Matutino",
  "conceptoCobroId": "{conceptoId}",
  "tipoGeneracion": "Mensual",
  "diaCorte": 10,
  "montoBase": 5500.00,
  "activa": true,
  "createdAtUtc": "2026-01-19T16:00:00Z",
  "updatedAtUtc": "2026-01-19T16:05:00Z"
}
```

---

## Paso 6: Inactivar Regla (Soft Delete)

**Request:**
```bash
curl -X DELETE http://localhost:3000/api/v1/ReglasCobro/{reglaId1} \
  -H "Authorization: Bearer {token}"
```

**Response esperada (204 No Content):**
```
(sin body)
```

**Verificar inactivación:**
```bash
curl -X GET "http://localhost:3000/api/v1/ReglasCobro/ciclo/{cicloId}?activa=false" \
  -H "Authorization: Bearer {token}"
```

**Response (200 OK):**
```json
[
  {
    "id": "550e8400-e29b-41d4-a716-446655440003",
    "cicloId": "{cicloId}",
    "grado": 1,
    "turno": "Matutino",
    "conceptoCobroId": "{conceptoId}",
    "tipoGeneracion": "Mensual",
    "diaCorte": 10,
    "montoBase": 5500.00,
    "activa": false,
    "createdAtUtc": "2026-01-19T16:00:00Z",
    "updatedAtUtc": "2026-01-19T16:06:00Z"
  }
]
```

---

## Validaciones Clave

| Escenario | HTTP Status | Código | Notas |
|-----------|-------------|--------|-------|
| Crear regla válida | 201 | - | Location header con /ReglasCobro/{id} |
| Combinación duplicada | 409 | `REGLA_DUPLICADA` | Index único sobre (CicloId, Grado, Turno, ConceptoCobroId, TipoGeneracion) |
| Ciclo no existe | 404 | `CICLO_NO_ENCONTRADO` | FK constraint |
| Concepto no existe | 404 | `CONCEPTO_NO_ENCONTRADO` | FK constraint |
| Grado fuera de rango | 400 | `GRADO_INVALIDO` | 1..6 válidos |
| DiaCorte fuera de rango | 400 | `DIA_CORTE_INVALIDO` | 1..28 válidos |
| MontoBase <= 0 | 400 | `MONTO_INVALIDO` | Debe ser > 0 |
| ID no existe | 404 | `REGLA_NO_ENCONTRADA` | GET/PUT/DELETE |
| Sin autorización | 401 | - | Falta token JWT |
| Rol insuficiente (Consulta/GET) | 403 | - | POST/PUT requieren Admin+Adm |

---

## Archivo de Migración

```
src/Tlaoami.Infrastructure/Migrations/[Timestamp]_AddReglasCobro.cs
```

Ejecutar:
```bash
dotnet ef database update -p src/Tlaoami.Infrastructure -s src/Tlaoami.API
```

Verificar tabla:
```sql
\d "ReglasCobro"
```

Debe mostrar:
- Id, CicloId, Grado, Turno, ConceptoCobroId, TipoGeneracion, DiaCorte, MontoBase, Activa, CreatedAtUtc, UpdatedAtUtc
- FK: CicloId → CiclosEscolares(Id), ConceptoCobroId → ConceptosCobro(Id)
- Índice único: (CicloId, Grado, Turno, ConceptoCobroId, TipoGeneracion)

---

## Notas Importantes

✅ **No genera facturas/cargos automáticamente.** Solo define reglas de intención.  
✅ **Soft delete:** DELETE inactiva en lugar de eliminar.  
✅ **Campos opcionales:** Grado y Turno pueden ser null (aplica a todos los grados/turnos).  
✅ **Índice único lógico:** Previene duplicados al nivel de BD, pero la lógica de validación también está en el servicio.
