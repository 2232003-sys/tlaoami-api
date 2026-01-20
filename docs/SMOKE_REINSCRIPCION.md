# SMOKE TESTS - REINSCRIPCIÓN BLOQUEADA POR ADEUDO

## Descripción General

Este documento define los casos de prueba de humo para validar el proceso de negocio **REINSCRIPCIÓN BLOQUEADA POR ADEUDO** implementado en la API de Tlaoami.

## Endpoint Principal

```
POST /api/v1/Reinscripciones
Authorization: Bearer {JWT_TOKEN}
Content-Type: application/json
```

## Casos de Prueba

### CASO 1: Reinscripción Exitosa (SIN ADEUDO)

**Descripción**: Alumno sin adeudo se reinscribe exitosamente a un nuevo ciclo/grupo.

**Datos de Entrada**:
```json
{
  "alumnoId": "550e8400-e29b-41d4-a716-446655440000",
  "cicloDestinoId": "550e8400-e29b-41d4-a716-446655440001",
  "grupoDestinoId": "550e8400-e29b-41d4-a716-446655440002"
}
```

**Condiciones Previas**:
- Alumno existe y NO tiene adeudo (saldo = 0 o negativo)
- Ciclo destino existe y está activo
- Grupo destino existe en ciclo destino
- Alumno NO está inscrito en ciclo destino

**Respuesta Esperada**:
```
HTTP 201 Created
Location: /api/v1/Reinscripciones/{reinscripcionId}

Body:
{
  "id": "550e8400-e29b-41d4-a716-446655440003",
  "alumnoId": "550e8400-e29b-41d4-a716-446655440000",
  "cicloDestinoId": "550e8400-e29b-41d4-a716-446655440001",
  "grupoDestinoId": "550e8400-e29b-41d4-a716-446655440002",
  "estado": "Completada",
  "saldoAlMomento": 0.00,
  "createdAtUtc": "2026-01-20T10:30:00Z"
}
```

**Validaciones Post-Éxito**:
- [ ] Registro Reinscripcion creado en BD con estado "Completada"
- [ ] AsignacionesGrupo anterior cerrada (FechaFin asignada)
- [ ] AsignacionesGrupo nueva creada para alumno+grupo destino con Activo=true
- [ ] Header Location contiene URL del nuevo recurso

**Comando cURL**:
```bash
curl -X POST https://localhost:7000/api/v1/Reinscripciones \
  -H "Authorization: Bearer $JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "alumnoId": "550e8400-e29b-41d4-a716-446655440000",
    "cicloDestinoId": "550e8400-e29b-41d4-a716-446655440001",
    "grupoDestinoId": "550e8400-e29b-41d4-a716-446655440002"
  }'
```

---

### CASO 2: Reinscripción Bloqueada por Adeudo (CON ADEUDO)

**Descripción**: Alumno con adeudo > 0.01 intenta reinscribirse. Request bloqueado.

**Datos de Entrada**:
```json
{
  "alumnoId": "550e8400-e29b-41d4-a716-446655440010",
  "cicloDestinoId": "550e8400-e29b-41d4-a716-446655440001",
  "grupoDestinoId": "550e8400-e29b-41d4-a716-446655440002"
}
```

**Condiciones Previas**:
- Alumno existe y TIENE adeudo > 0.01 (saldo pendiente > 0.01)
- Ciclo destino existe y está activo
- Grupo destino existe en ciclo destino

**Respuesta Esperada**:
```
HTTP 409 Conflict

Body:
{
  "code": "REINSCRIPCION_BLOQUEADA_ADEUDO",
  "message": "Reinscripción bloqueada por adeudo. Saldo pendiente: $125.50",
  "saldo": 125.50,
  "detalleAdeudo": "El alumno debe pagar su adeudo antes de reinscribirse"
}
```

**Validaciones Post-Rechazo**:
- [ ] HTTP status = 409
- [ ] Code = "REINSCRIPCION_BLOQUEADA_ADEUDO"
- [ ] Saldo mostrado = valor real de adeudo
- [ ] Registro Reinscripcion creado en BD con estado "Bloqueada" y MotivoBloqueo="ADEUDO"
- [ ] AsignacionesGrupo NO modificadas
- [ ] No se deassigna grupo anterior

**Comando cURL**:
```bash
curl -X POST https://localhost:7000/api/v1/Reinscripciones \
  -H "Authorization: Bearer $JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "alumnoId": "550e8400-e29b-41d4-a716-446655440010",
    "cicloDestinoId": "550e8400-e29b-41d4-a716-446655440001",
    "grupoDestinoId": "550e8400-e29b-41d4-a716-446655440002"
  }'
```

---

### CASO 3: Alumno No Encontrado

**Descripción**: Se intenta reinscribir alumno inexistente.

**Datos de Entrada**:
```json
{
  "alumnoId": "00000000-0000-0000-0000-000000000000",
  "cicloDestinoId": "550e8400-e29b-41d4-a716-446655440001",
  "grupoDestinoId": "550e8400-e29b-41d4-a716-446655440002"
}
```

**Respuesta Esperada**:
```
HTTP 404 Not Found

Body:
{
  "code": "ALUMNO_NO_ENCONTRADO",
  "message": "Alumno no encontrado"
}
```

**Validaciones**:
- [ ] HTTP status = 404
- [ ] No se crea Reinscripcion
- [ ] No se modifica AsignacionesGrupo

---

### CASO 4: Ciclo Destino No Encontrado

**Descripción**: Ciclo escolar destino no existe.

**Datos de Entrada**:
```json
{
  "alumnoId": "550e8400-e29b-41d4-a716-446655440000",
  "cicloDestinoId": "00000000-0000-0000-0000-000000000000",
  "grupoDestinoId": "550e8400-e29b-41d4-a716-446655440002"
}
```

**Respuesta Esperada**:
```
HTTP 404 Not Found

Body:
{
  "code": "CICLO_NO_ENCONTRADO",
  "message": "Ciclo escolar no encontrado"
}
```

---

### CASO 5: Grupo Destino No Encontrado

**Descripción**: Grupo destino no existe o no pertenece al ciclo destino.

**Datos de Entrada**:
```json
{
  "alumnoId": "550e8400-e29b-41d4-a716-446655440000",
  "cicloDestinoId": "550e8400-e29b-41d4-a716-446655440001",
  "grupoDestinoId": "00000000-0000-0000-0000-000000000000"
}
```

**Respuesta Esperada**:
```
HTTP 404 Not Found

Body:
{
  "code": "GRUPO_NO_ENCONTRADO",
  "message": "Grupo no encontrado en ciclo destino"
}
```

---

### CASO 6: Alumno Ya Inscrito en Ciclo Destino (Idempotencia)

**Descripción**: Alumno intenta reinscribirse en ciclo donde ya está inscrito.

**Datos de Entrada**:
```json
{
  "alumnoId": "550e8400-e29b-41d4-a716-446655440000",
  "cicloDestinoId": "550e8400-e29b-41d4-a716-446655440001",
  "grupoDestinoId": "550e8400-e29b-41d4-a716-446655440005"
}
```

**Condiciones Previas**:
- Alumno ya tiene AsignacionesGrupo activo en ciclo destino (grupo diferente)
- Alumno intenta cambiar a otro grupo del MISMO ciclo

**Respuesta Esperada**:
```
HTTP 409 Conflict

Body:
{
  "code": "ALUMNO_YA_INSCRITO_EN_CICLO",
  "message": "El alumno ya está inscrito en el ciclo destino"
}
```

**Validaciones**:
- [ ] HTTP status = 409
- [ ] AsignacionesGrupo anterior NO se modifica
- [ ] No se crea Reinscripcion o se crea con estado "Rechazada"

---

### CASO 7: Grupo Sin Cupo (Capacidad Completa)

**Descripción**: Grupo destino está lleno.

**Datos de Entrada**:
```json
{
  "alumnoId": "550e8400-e29b-41d4-a716-446655440000",
  "cicloDestinoId": "550e8400-e29b-41d4-a716-446655440001",
  "grupoDestinoId": "550e8400-e29b-41d4-a716-446655440006"
}
```

**Condiciones Previas**:
- Grupo destino capacidad = N
- Grupo destino tiene N asignaciones activas (lleno)

**Respuesta Esperada**:
```
HTTP 409 Conflict

Body:
{
  "code": "GRUPO_SIN_CUPO",
  "message": "El grupo no tiene cupo disponible"
}
```

---

## Endpoints de Soporte

### Consultar Reinscripción

```
GET /api/v1/Reinscripciones/{id}
Authorization: Bearer {JWT_TOKEN}
```

**Ejemplo**:
```bash
curl -X GET https://localhost:7000/api/v1/Reinscripciones/550e8400-e29b-41d4-a716-446655440003 \
  -H "Authorization: Bearer $JWT_TOKEN"
```

---

### Listar Reinscripciones por Alumno

```
GET /api/v1/Reinscripciones/alumno/{alumnoId}?cicloDestinoId={cicloId}
Authorization: Bearer {JWT_TOKEN}
```

**Ejemplo**:
```bash
curl -X GET "https://localhost:7000/api/v1/Reinscripciones/alumno/550e8400-e29b-41d4-a716-446655440000?cicloDestinoId=550e8400-e29b-41d4-a716-446655440001" \
  -H "Authorization: Bearer $JWT_TOKEN"
```

---

## Matriz de Validación

| Caso | Entrada | Código | HTTP | Esperado | Reales | ✓ |
|------|---------|--------|------|----------|--------|---|
| 1 | Sin adeudo | Completa | 201 | Reinscripción creada | | |
| 2 | Con adeudo | BLOQUEADA | 409 | Rechazada, grabada | | |
| 3 | Alumno ∅ | - | 404 | No procesada | | |
| 4 | Ciclo ∅ | - | 404 | No procesada | | |
| 5 | Grupo ∅ | - | 404 | No procesada | | |
| 6 | Ya inscrito | DUPLICADO | 409 | No procesa doble | | |
| 7 | Sin cupo | SIN_CUPO | 409 | Rechazada | | |

---

## Configuración de Ambiente

### JWT Token (Rol requerido: Admin o Administrativa)

```bash
# En variable de entorno
export JWT_TOKEN="eyJhbGciOiJIUzI1NiIsInR..."

# O hardcodeado en curl
JWT_TOKEN="eyJhbGciOiJIUzI1NiIsInR..."
```

### Base de Datos (Pre-setup)

Asegurarse de que los siguientes datos existan:
- Ciclo 2026: Id `550e8400-e29b-41d4-a716-446655440001`, activo
- Grupo 1A: Id `550e8400-e29b-41d4-a716-446655440002`, capacidad 30
- Alumno Juan: Id `550e8400-e29b-41d4-a716-446655440000`, sin adeudo
- Alumno Pedro: Id `550e8400-e29b-41d4-a716-446655440010`, con adeudo $125.50

### Ejecución Manual

```bash
# 1. Iniciar API
cd tlaoami-api && dotnet run

# 2. En otra terminal, ejecutar CASO 1
curl -X POST http://localhost:5000/api/v1/Reinscripciones ...

# 3. Verificar respuesta (201, Location header)
```

---

## Notas Importantes

⚠️ **ADEUDO > 0.01**: El umbral para bloqueo es saldo > 0.01m (NO >=). Saldos de 0.00 y 0.01 son válidos.

✅ **TRANSACCIÓN**: Deassign + Assign ocurre atómicamente en una transacción DB.

✅ **AUDITORÍA**: CreatedByUserId, SaldoAlMomento, y MotivoBloqueo se registran para cada Reinscripción.

✅ **IDEMPOTENCIA**: Índice UNIQUE(AlumnoId, CicloDestinoId) previene duplicados.

✅ **ROLES**: Solo Admin y Administrativa pueden crear reinscripciones (POST). Todos pueden ver (GET).

---

Última actualización: 2026-01-20
