# VALIDACIÓN EN BASE DE DATOS - Ejemplos SQL

**Propósito:** Verificar el funcionamiento de la solución FIFO directamente en PostgreSQL/SQLite  
**Fecha:** 21 de enero de 2025

---

## 🔍 Verificación de Datos Post-Conciliación

### ANTES de la corrección FIFO

```sql
-- ❌ PROBLEMA: Pago sin asociación a factura
SELECT 
    a.Matricula as Alumno,
    f.NumeroFactura as Factura,
    f.Estado as EstadoFactura,
    f.Monto as MontoFactura,
    COALESCE(SUM(p.Monto), 0) as TotalPagado,
    f.Monto - COALESCE(SUM(p.Monto), 0) as Saldo
FROM Alumnos a
JOIN Facturas f ON f.AlumnoId = a.Id
LEFT JOIN Pagos p ON p.FacturaId = f.Id
GROUP BY a.Id, a.Matricula, f.Id, f.NumeroFactura, f.Estado, f.Monto
ORDER BY a.Matricula, f.FechaVencimiento;

-- Resultado (INCORRECTO):
-- Alumno | Factura | EstadoFactura       | MontoFactura | TotalPagado | Saldo
-- --------|---------|---------------------|--------------|------------|--------
-- A12345  | F001    | Pendiente           | 1000         | 0          | 1000
-- A12345  | F002    | Pendiente           | 500          | 0          | 500
-- A12345  | F003    | Pendiente           | 800          | 0          | 800
--
-- ⚠️ Hay pagos creados pero NO asociados a facturas:
SELECT * FROM Pagos 
WHERE AlumnoId = 'guid-of-a12345' 
AND FacturaId IS NULL;
-- Resultado: 1 Pago de $1200 flotando sin aplicar ❌
```

### DESPUÉS de la corrección FIFO

```sql
-- ✅ CORRECTO: Pagos distribuidos y asociados a facturas
SELECT 
    a.Matricula as Alumno,
    f.NumeroFactura as Factura,
    f.Estado as EstadoFactura,
    f.FechaVencimiento,
    f.Monto as MontoFactura,
    COALESCE(SUM(p.Monto), 0) as TotalPagado,
    f.Monto - COALESCE(SUM(p.Monto), 0) as Saldo
FROM Alumnos a
JOIN Facturas f ON f.AlumnoId = a.Id
LEFT JOIN Pagos p ON p.FacturaId = f.Id
WHERE a.Matricula = 'A12345'
GROUP BY a.Id, a.Matricula, f.Id, f.NumeroFactura, f.Estado, 
         f.Monto, f.FechaVencimiento
ORDER BY f.FechaVencimiento, f.FechaEmision;

-- Resultado (CORRECTO):
-- Alumno | Factura | EstadoFactura           | FechaVencimiento | MontoFactura | TotalPagado | Saldo
-- --------|---------|-------------------------|------------------|--------------|------------|--------
-- A12345  | F001    | Pagada                  | 2024-12-31       | 1000         | 1000       | 0
-- A12345  | F002    | ParcialmentePagada      | 2025-01-15       | 500          | 200        | 300
-- A12345  | F003    | Pendiente               | 2025-02-15       | 800          | 0          | 800
--
-- ✓ Cada factura recibió su pago correspondiente
-- ✓ Estados actualizados correctamente
-- ✓ FIFO aplicado: vencida primero (F001), luego próxima (F002)
```

---

## 💳 Verificación de Pagos Creados

### Ver todos los pagos de una conciliación

```sql
-- Buscar el movimiento bancario
SELECT 
    mb.Id as MovimientoBancarioId,
    mb.Fecha,
    mb.Monto,
    mb.Descripcion,
    mb.Estado as EstadoConciliacion
FROM MovimientosBancarios mb
WHERE mb.Monto = 1200  -- El monto que depositó
AND mb.Estado = 'Conciliado'
LIMIT 1;

-- Copiar el Id (por ej: 'a1b2c3d4-e5f6-7890-1234-567890abcdef')
-- Usar para buscar los pagos creados:

SELECT 
    p.Id,
    p.IdempotencyKey,
    p.FacturaId,
    p.AlumnoId,
    p.Monto,
    p.FechaPago,
    p.Metodo,
    CASE 
        WHEN p.FacturaId IS NULL THEN '💰 ANTICIPO'
        ELSE '✓ Contra Factura'
    END as Tipo
FROM Pagos p
WHERE p.IdempotencyKey LIKE 'BANK:a1b2c3d4-e5f6-7890-1234-567890abcdef%'
ORDER BY p.IdempotencyKey;

-- Resultado esperado:
-- Id           | IdempotencyKey                          | FacturaId | Monto | Tipo
-- --------------|----------------------------------------|-----------|-------|------------------------
-- guid-pago-1   | BANK:a1b2...f6%:F0                     | guid-f1   | 1000  | ✓ Contra Factura
-- guid-pago-2   | BANK:a1b2...f6%:F1                     | guid-f2   | 200   | ✓ Contra Factura
-- (no hay F2 porque se consumió todo en F1 y F2)
```

### Estructura de IdempotencyKey

```
BANK:{MovimientoBancarioId}:F{n}    → Pago específico a factura n
BANK:{MovimientoBancarioId}:ANTICIPO → Pago anticipo (sobrante)

Ejemplos:
BANK:a1b2c3d4-e5f6-7890-1234-567890abcdef:F0
BANK:a1b2c3d4-e5f6-7890-1234-567890abcdef:F1
BANK:a1b2c3d4-e5f6-7890-1234-567890abcdef:F2
BANK:a1b2c3d4-e5f6-7890-1234-567890abcdef:ANTICIPO

Beneficio: Identifica qué factura recibió qué pago
```

---

## 📊 Casos de Uso - SQL de Validación

### Caso 1: Distribución FIFO Completa

```sql
-- Setup: 3 facturas, $1,200 depositados
INSERT INTO Facturas (Id, AlumnoId, NumeroFactura, Periodo, Monto, FechaEmision, 
                     FechaVencimiento, Estado, IssuedAt)
VALUES 
    ('f001', 'alumno-guid', 'F2025-001', '2025-01', 1000, '2024-12-15', '2024-12-31', 'Pendiente', NOW()),
    ('f002', 'alumno-guid', 'F2025-002', '2025-01', 500,  '2024-12-20', '2025-01-15', 'Pendiente', NOW()),
    ('f003', 'alumno-guid', 'F2025-003', '2025-02', 800,  '2025-01-01', '2025-02-15', 'Pendiente', NOW());

INSERT INTO MovimientosBancarios (Id, Fecha, Descripcion, Monto, Saldo, Tipo, Estado, HashMovimiento)
VALUES ('mov-guid', NOW(), 'Depósito alumno', 1200, 1200, 'Deposito', 'NoConciliado', 'hash123');

-- Ejecutar: await service.ConciliarMovimientoAsync(
--     movimiento.Id, alumnoId: 'alumno-guid', facturaId: null, crearPago: true)

-- Validar resultado:
SELECT 
    f.NumeroFactura,
    f.Estado as EstadoAntes,
    COUNT(p.Id) as NumPagos,
    SUM(p.Monto) as TotalPagado,
    f.Monto - SUM(p.Monto) as Saldo
FROM Facturas f
LEFT JOIN Pagos p ON p.FacturaId = f.Id
WHERE f.AlumnoId = 'alumno-guid'
GROUP BY f.Id, f.NumeroFactura, f.Estado, f.Monto
ORDER BY f.FechaVencimiento;

-- Resultado esperado:
-- NumeroFactura | EstadoAntes | NumPagos | TotalPagado | Saldo
-- --------------|-------------|----------|-------------|--------
-- F2025-001     | Pagada      | 1        | 1000        | 0
-- F2025-002     | ParcialmentePagada | 1 | 200        | 300
-- F2025-003     | Pendiente   | 0        | 0           | 800
-- ✓ Distribución correcta por FIFO
```

### Caso 2: Con Anticipo (Sobrante)

```sql
-- Setup: Factura de $1,000, depósito de $1,500
INSERT INTO Facturas (Id, AlumnoId, NumeroFactura, Periodo, Monto, FechaEmision,
                     FechaVencimiento, Estado, IssuedAt)
VALUES ('f010', 'alumno2-guid', 'F2025-010', '2025-01', 1000, '2024-12-15', 
        '2024-12-31', 'Pendiente', NOW());

INSERT INTO MovimientosBancarios (Id, Fecha, Descripcion, Monto, Saldo, Tipo, Estado, HashMovimiento)
VALUES ('mov2-guid', NOW(), 'Depósito alumno 2', 1500, 1500, 'Deposito', 'NoConciliado', 'hash456');

-- Ejecutar: await service.ConciliarMovimientoAsync(
--     'mov2-guid', alumnoId: 'alumno2-guid', facturaId: null, crearPago: true)

-- Validar anticipos:
SELECT 
    p.IdempotencyKey,
    p.FacturaId,
    p.Monto,
    CASE WHEN p.FacturaId IS NULL THEN '💰 ANTICIPO' ELSE '✓ Factura' END as Tipo
FROM Pagos p
WHERE p.AlumnoId = 'alumno2-guid'
ORDER BY p.IdempotencyKey;

-- Resultado esperado:
-- IdempotencyKey              | FacturaId | Monto | Tipo
-- ----------------------------|-----------|-------|------------------------
-- BANK:mov2-guid:F0           | f010      | 1000  | ✓ Factura
-- BANK:mov2-guid:ANTICIPO     | NULL      | 500   | 💰 ANTICIPO
-- ✓ $1,500 distribuidos: $1,000 a factura + $500 a anticipo
```

### Caso 3: Idempotencia (Aplicar dos veces)

```sql
-- Suponer que ya existe la conciliación del Caso 1
-- Intentar aplicar de nuevo con el mismo movimiento

-- Ejecutar: await service.ConciliarMovimientoAsync(
--     'mov-guid', alumnoId: 'alumno-guid', facturaId: null, crearPago: true)
-- (Segunda vez con el mismo movimiento)

-- Validar que NO se crearon pagos duplicados:
SELECT COUNT(*) as TotalPagos
FROM Pagos p
WHERE p.IdempotencyKey LIKE 'BANK:mov-guid%';

-- Resultado esperado: 2 (solo los originales, no duplicados)
-- ✓ Idempotencia garantizada

-- Validar facturas sin cambios:
SELECT 
    f.NumeroFactura,
    f.Estado,
    SUM(p.Monto) as TotalPagado
FROM Facturas f
LEFT JOIN Pagos p ON p.FacturaId = f.Id
WHERE f.AlumnoId = 'alumno-guid'
GROUP BY f.Id, f.NumeroFactura, f.Estado;

-- Los estados y montos deben ser idénticos a antes ✓
```

### Caso 4: Reversión

```sql
-- Suponer que existe conciliación del Caso 1
-- Ejecutar: await service.RevertirConciliacionAsync('mov-guid')

-- Validar que los pagos fueron eliminados:
SELECT COUNT(*) as PagosRestantes
FROM Pagos p
WHERE p.IdempotencyKey LIKE 'BANK:mov-guid%';

-- Resultado esperado: 0 (todos eliminados)
-- ✓ Reversión correcta

-- Validar que las facturas fueron recalculadas:
SELECT 
    f.NumeroFactura,
    f.Estado,
    COUNT(p.Id) as NumPagos,
    SUM(p.Monto) as TotalPagado
FROM Facturas f
LEFT JOIN Pagos p ON p.FacturaId = f.Id
WHERE f.AlumnoId = 'alumno-guid'
GROUP BY f.Id, f.NumeroFactura, f.Estado;

-- Resultado esperado:
-- NumeroFactura | Estado    | NumPagos | TotalPagado
-- --------------|-----------|----------|-------------
-- F2025-001     | Pendiente | 0        | NULL (0)
-- F2025-002     | Pendiente | 0        | NULL (0)
-- F2025-003     | Pendiente | 0        | NULL (0)
-- ✓ Todas revertidas a Pendiente
```

---

## 🔍 Queries Útiles para Monitoreo

### Estado de Cuenta de un Alumno

```sql
SELECT 
    a.Matricula,
    a.Nombre,
    a.Apellido,
    COUNT(DISTINCT f.Id) as TotalFacturas,
    SUM(CASE WHEN f.Estado = 'Pagada' THEN 1 ELSE 0 END) as Pagadas,
    SUM(CASE WHEN f.Estado = 'ParcialmentePagada' THEN 1 ELSE 0 END) as Parciales,
    SUM(CASE WHEN f.Estado = 'Pendiente' THEN 1 ELSE 0 END) as Pendientes,
    SUM(f.Monto) as TotalDeudas,
    COALESCE(SUM(p.Monto), 0) as TotalPagado,
    SUM(f.Monto) - COALESCE(SUM(p.Monto), 0) as SaldoPendiente
FROM Alumnos a
LEFT JOIN Facturas f ON f.AlumnoId = a.Id
LEFT JOIN Pagos p ON p.FacturaId = f.Id
GROUP BY a.Id, a.Matricula, a.Nombre, a.Apellido
ORDER BY a.Matricula;
```

### Movimientos Pendientes de Conciliar

```sql
SELECT 
    mb.Id,
    mb.Fecha,
    mb.Monto,
    mb.Descripcion,
    mb.Estado,
    COUNT(mc.Id) as MovimientosConciliacionAsociados
FROM MovimientosBancarios mb
LEFT JOIN MovimientosConciliacion mc ON mc.MovimientoBancarioId = mb.Id
WHERE mb.Estado = 'NoConciliado'
GROUP BY mb.Id, mb.Fecha, mb.Monto, mb.Descripcion, mb.Estado
ORDER BY mb.Fecha DESC;
```

### Pagos Sin Factura (Anticipos)

```sql
SELECT 
    p.Id,
    p.IdempotencyKey,
    a.Matricula,
    p.Monto,
    p.FechaPago,
    p.Metodo,
    'Available for future invoices' as Status
FROM Pagos p
JOIN Alumnos a ON a.Id = p.AlumnoId
WHERE p.FacturaId IS NULL  -- Anticipos
ORDER BY a.Matricula, p.FechaPago;
```

### Facturas con Distribución FIFO

```sql
SELECT 
    f.NumeroFactura,
    f.FechaVencimiento,
    f.Monto,
    f.Estado,
    COUNT(p.Id) as NumPagos,
    SUM(p.Monto) as TotalPagado,
    f.Monto - SUM(p.Monto) as Saldo,
    STRING_AGG(p.IdempotencyKey, ', ') as IdempotencyKeys
FROM Facturas f
LEFT JOIN Pagos p ON p.FacturaId = f.Id
WHERE f.AlumnoId = 'alumno-guid'
GROUP BY f.Id, f.NumeroFactura, f.FechaVencimiento, f.Monto, f.Estado
ORDER BY f.FechaVencimiento;
```

---

## 📈 Métricas de Validación

### 1. Consistencia de Datos

```sql
-- Verificar que no hay pagos huérfanos
SELECT COUNT(*) as PagosHuerfanos
FROM Pagos p
LEFT JOIN Facturas f ON f.Id = p.FacturaId
LEFT JOIN Alumnos a ON a.Id = p.AlumnoId
WHERE (p.FacturaId IS NOT NULL AND f.Id IS NULL)  -- Factura eliminada
   OR (p.AlumnoId IS NOT NULL AND a.Id IS NULL);  -- Alumno eliminado

-- Resultado esperado: 0
```

### 2. Precisión Decimal

```sql
-- Verificar sumas coinciden (sin errores de redondeo)
SELECT 
    f.NumeroFactura,
    f.Monto as MontoFactura,
    SUM(p.Monto) as SumaPagos,
    ABS(f.Monto - SUM(p.Monto)) as Diferencia
FROM Facturas f
LEFT JOIN Pagos p ON p.FacturaId = f.Id
GROUP BY f.Id, f.NumeroFactura, f.Monto
HAVING ABS(f.Monto - SUM(p.Monto)) > 0.01;

-- Resultado esperado: Vacío (sin diferencias > $0.01)
```

### 3. Idempotencia

```sql
-- Contar movimientos con pagos duplicados (por IdempotencyKey)
SELECT 
    p.IdempotencyKey,
    COUNT(*) as Repeticiones
FROM Pagos p
GROUP BY p.IdempotencyKey
HAVING COUNT(*) > 1;

-- Resultado esperado: Vacío (sin duplicados)
```

### 4. Estado de Facturas

```sql
-- Verificar estado es consistente con pagos
SELECT 
    f.Id,
    f.NumeroFactura,
    f.Estado,
    SUM(p.Monto) as TotalPagado,
    CASE 
        WHEN SUM(p.Monto) IS NULL THEN 'Debería ser: Pendiente'
        WHEN SUM(p.Monto) < f.Monto THEN 'Debería ser: ParcialmentePagada'
        WHEN SUM(p.Monto) >= f.Monto THEN 'Debería ser: Pagada'
    END as EstadoEsperado
FROM Facturas f
LEFT JOIN Pagos p ON p.FacturaId = f.Id
GROUP BY f.Id, f.NumeroFactura, f.Estado, f.Monto
WHERE f.Estado != CASE 
        WHEN SUM(p.Monto) IS NULL THEN 'Pendiente'
        WHEN SUM(p.Monto) < f.Monto THEN 'ParcialmentePagada'
        WHEN SUM(p.Monto) >= f.Monto THEN 'Pagada'
    END;

-- Resultado esperado: Vacío (todos los estados son correctos)
```

---

## 🚨 Troubleshooting

### Problema: Pago no aparece en la factura

```sql
-- Verificar el pago existe
SELECT * FROM Pagos WHERE IdempotencyKey LIKE 'BANK:%';

-- Verificar factura relacionada
SELECT * FROM Facturas WHERE Id = 'pago.FacturaId';

-- Verificar cambio tracking en DbContext (puede ser problema de tests)
-- Solución: Usar context.Entry(...).Reload() en tests
```

### Problema: Estado de factura no actualizado

```sql
-- Verificar RecalculateFrom() fue llamado
-- Revisar logs en ConciliacionBancariaService.cs línea XXX

-- Manual check:
SELECT 
    f.NumeroFactura,
    f.Estado as EstadoActual,
    SUM(p.Monto) as TotalPagado,
    f.Monto,
    CASE 
        WHEN SUM(p.Monto) >= f.Monto THEN 'Pagada'
        WHEN SUM(p.Monto) > 0 THEN 'ParcialmentePagada'
        ELSE 'Pendiente'
    END as EstadoCalculado
FROM Facturas f
LEFT JOIN Pagos p ON p.FacturaId = f.Id
WHERE f.NumeroFactura = 'F001'
GROUP BY f.Id, f.NumeroFactura, f.Estado, f.Monto;
```

### Problema: Anticipo no se crea

```sql
-- Verificar que montoRestante > TOLERANCE
-- TOLERANCE = 0.01m (ver ConciliacionBancariaService.cs línea 14)

-- Debug: Revisar logs
-- INFO: "Creado pago anticipo de ${Monto}" debería aparecer

SELECT * FROM Pagos 
WHERE FacturaId IS NULL 
AND IdempotencyKey LIKE 'BANK:%:ANTICIPO';
```

---

## ✅ Checklist de Validación SQL

- [ ] Facturas tienen EstadoFactura correcto
- [ ] Pagos asociados a facturas específicas (no NULL)
- [ ] FIFO aplicado: vencidas primero
- [ ] Anticipos creados si hay sobrante
- [ ] IdempotencyKey único por factura
- [ ] Sin pagos duplicados (conteo por IdempotencyKey = 1)
- [ ] Estados de Movimiento Bancario correctos
- [ ] Sumas no tienen errores de redondeo
- [ ] Reversión elimina todos los pagos
- [ ] Reversión recalcula estados

---

## 🎯 Conclusión

Estas queries permiten validar completamente el funcionamiento de la solución FIFO:

✅ **Verificación de distribución** - Cada factura recibe el monto correcto  
✅ **Verificación de FIFO** - Se respeta orden de vencimiento  
✅ **Verificación de anticipos** - Sobrantes se guardan correctamente  
✅ **Verificación de idempotencia** - No hay duplicados  
✅ **Verificación de estado** - Estados de factura son correctos  

**Para testing local:**
```bash
# Conectar a PostgreSQL/SQLite
psql -U usuario -d tlaoami_db

# Copiar y pegar queries desde este documento
```

---

**Documentación SQL:** 21 de enero de 2025  
**Aplicable a:** PostgreSQL 12+ / SQLite 3.32+  
**Mantenedor:** Backend Team (Tlaoami)
