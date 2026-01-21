# 🧪 Smoke Tests: Reportes Caja & Cobranza

## Prerequisitos

```bash
# Obtener token JWT
TOKEN=$(curl -s -X POST http://localhost:5271/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"admin123"}' \
  | jq -r '.token')

# Verificar que hay datos (o crear facturas/pagos de prueba)
# Obtener IDs de ejemplo:
CICLO_ID="<guid-ciclo-activo>"
GRUPO_ID="<guid-grupo-existente>"
```

---

## Test 1: Reporte de Adeudos (sin filtros)

```bash
curl -X GET "http://localhost:5271/api/v1/Reportes/adeudos" \
  -H "Authorization: Bearer $TOKEN" \
  | jq '.'
```

**Esperado**:
- HTTP 200 OK
- Array de adeudos con: `matricula`, `nombreCompleto`, `grupo`, `grado`, `totalFacturado`, `totalPagado`, `saldo`, `ultimoPagoAtUtc`
- Facturas canceladas excluidas
- Saldo con tolerancia de 0.01

---

## Test 2: Adeudos por Ciclo Escolar

```bash
curl -X GET "http://localhost:5271/api/v1/Reportes/adeudos?cicloId=$CICLO_ID" \
  -H "Authorization: Bearer $TOKEN" \
  | jq '.'
```

**Esperado**:
- Solo alumnos del ciclo especificado
- Datos completos de adeudos

---

## Test 3: Adeudos por Grado

```bash
curl -X GET "http://localhost:5271/api/v1/Reportes/adeudos?grado=3" \
  -H "Authorization: Bearer $TOKEN" \
  | jq '.'
```

**Esperado**:
- Solo alumnos de 3er grado
- Cálculos correctos

---

## Test 4: Reporte de Pagos (rango de fechas)

```bash
# Pagos del mes actual
FROM_DATE="2026-01-01T00:00:00Z"
TO_DATE="2026-01-31T23:59:59Z"

curl -X GET "http://localhost:5271/api/v1/Reportes/pagos?from=$FROM_DATE&to=$TO_DATE" \
  -H "Authorization: Bearer $TOKEN" \
  | jq '.'
```

**Esperado**:
- HTTP 200 OK
- Array de pagos con: `pagoId`, `fechaUtc`, `alumnoId`, `alumnoNombre`, `facturaId`, `monto`, `metodo`, `referencia`
- Solo pagos en el rango especificado
- Ordenados por fecha descendente

---

## Test 5: Export Adeudos a CSV

```bash
curl -X GET "http://localhost:5271/api/v1/Reportes/adeudos/export" \
  -H "Authorization: Bearer $TOKEN" \
  -o adeudos_export.csv

# Verificar archivo
head -20 adeudos_export.csv
```

**Esperado**:
- HTTP 200 OK
- Content-Type: text/csv
- Archivo descargado con nombre `adeudos_YYYYMMDD_HHMMSS.csv`
- Encabezados: `Matricula,Nombre Completo,Grupo,Grado,Total Facturado,Total Pagado,Saldo,Ultimo Pago`
- Datos en formato CSV (Excel-friendly)
- Números con formato `F2` (dos decimales)

---

## Test 6: Export Pagos a CSV

```bash
curl -X GET "http://localhost:5271/api/v1/Reportes/pagos/export?from=$FROM_DATE&to=$TO_DATE" \
  -H "Authorization: Bearer $TOKEN" \
  -o pagos_export.csv

# Verificar archivo
head -20 pagos_export.csv
```

**Esperado**:
- HTTP 200 OK
- Content-Type: text/csv
- Archivo descargado con nombre `pagos_YYYYMMDD_YYYYMMDD.csv`
- Encabezados: `Fecha,Alumno Nombre,Factura ID,Monto,Metodo,Referencia`
- Datos correctos en CSV
- Fechas en formato ISO

---

## Validaciones de Negocio

### Adeudos:
- [x] Excluye facturas canceladas
- [x] Considera solo pagos confirmados
- [x] Aplica tolerancia de 0.01 en saldo
- [x] Respeta filtro de fechaCorte en pagos
- [x] Agrupa correctamente por alumno
- [x] Muestra grupo y grado de asignación activa

### Pagos:
- [x] Filtra por rango de fechas (obligatorio)
- [x] Permite filtro opcional por grupo
- [x] Permite filtro opcional por método
- [x] Ordena por fecha descendente
- [x] Incluye pagos con y sin factura

### Export CSV:
- [x] Formato Excel-friendly (UTF-8 with BOM)
- [x] Encabezados claros
- [x] Números con formato decimal correcto
- [x] Fechas en formato estándar
- [x] Campos de texto entrecomillados

---

## Casos de Error

### Sin autenticación:
```bash
curl -X GET "http://localhost:5271/api/v1/Reportes/adeudos"
```
**Esperado**: HTTP 401 UNAUTHORIZED

### Fechas inválidas en pagos:
```bash
curl -X GET "http://localhost:5271/api/v1/Reportes/pagos?from=2026-01-31&to=2026-01-01" \
  -H "Authorization: Bearer $TOKEN"
```
**Esperado**: HTTP 400 BAD REQUEST - "Fecha 'to' debe ser mayor o igual a 'from'"

### Export sin datos:
```bash
curl -X GET "http://localhost:5271/api/v1/Reportes/adeudos/export?cicloId=00000000-0000-0000-0000-000000000000" \
  -H "Authorization: Bearer $TOKEN"
```
**Esperado**: CSV vacío con solo encabezados

---

## Performance

- Queries usan `AsNoTracking()` para lectura optimizada
- Filtros aplicados a nivel de DB (no en memoria)
- Sin paginación por ahora (MVP)
- Recomendado: agregar paginación si > 1000 registros

---

## Próximas Mejoras (fuera de MVP)

- [ ] Paginación en reportes JSON
- [ ] Export a Excel (XLSX) con formato avanzado
- [ ] Gráficas de adeudos por mes
- [ ] Reporte de proyección de ingresos
- [ ] Auditoría: capturadoPorUserId en pagos
- [ ] Filtros adicionales: alumnoId, periodo custom
- [ ] Cache para reportes frecuentes
