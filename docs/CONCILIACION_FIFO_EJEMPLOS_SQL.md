-- ===============================================================
-- DEMOSTRACIÓN: Consolidación FIFO en Acción
-- ===============================================================
-- Este script muestra ejemplos de cómo la consolidación FIFO
-- distribuye pagos automáticamente entre colegiaturas

-- ANTES: Alumno con 3 facturas pendientes
-- ===============================================================

-- 1. Crear alumno de prueba
INSERT INTO alumnos (id, matricula, nombre, apellido, email, telefono, activo, fecha_inscripcion)
VALUES 
  ('a1111111-1111-1111-1111-111111111111'::uuid, 'A2001', 'Carlos', 'García', 'carlos@example.com', '5551234567', true, NOW())
;

-- 2. Crear 3 facturas de colegiatura
INSERT INTO facturas (
  id, alumno_id, numero_factura, concepto, periodo, monto, 
  fecha_emision, fecha_vencimiento, estado, issued_at
) VALUES
  -- Factura 1: VENCIDA (10 días atrás)
  ('f1111111-1111-1111-1111-111111111111'::uuid, 
   'a1111111-1111-1111-1111-111111111111'::uuid,
   'FAC-001', 'Colegiatura', '2025-11-01', 1000.00,
   NOW() - INTERVAL '40 days',
   NOW() - INTERVAL '10 days',
   'Pendiente', NOW() - INTERVAL '39 days'),
   
  -- Factura 2: POR VENCER (5 días)
  ('f2222222-2222-2222-2222-222222222222'::uuid,
   'a1111111-1111-1111-1111-111111111111'::uuid,
   'FAC-002', 'Colegiatura', '2025-12-01', 500.00,
   NOW() - INTERVAL '25 days',
   NOW() + INTERVAL '5 days',
   'Pendiente', NOW() - INTERVAL '24 days'),
   
  -- Factura 3: FUTURA (30 días)
  ('f3333333-3333-3333-3333-333333333333'::uuid,
   'a1111111-1111-1111-1111-111111111111'::uuid,
   'FAC-003', 'Colegiatura', '2026-01-01', 800.00,
   NOW() - INTERVAL '10 days',
   NOW() + INTERVAL '30 days',
   'Pendiente', NOW() - INTERVAL '9 days')
;

-- Estado de cuenta ANTES
-- Total Facturado: $2,300.00
-- Total Pagado: $0.00
-- Saldo Pendiente: $2,300.00

-- ===============================================================
-- TRANSACCIÓN: Abono a Cuenta de $1,200.00
-- ===============================================================

-- 3. Crear movimiento bancario
INSERT INTO movimientos_bancarios (
  id, monto, fecha, referencia, concepto, tipo, estado, saldo
) VALUES
  ('m1234567-1234-1234-1234-123456789abc'::uuid,
   1200.00, NOW(), 'DEP-2026-001', 'Depósito Caja', 'Deposito', 'NoConciliado', 1200.00)
;

-- 4. Conciliar movimiento a cuenta del alumno (SIN factura específica)
-- El sistema ejecutará:
-- - Buscar facturas PENDIENTES del alumno
-- - Ordenar por FechaVencimiento (más antiguas primero)
-- - Distribuir $1,200 automáticamente

-- RESULTADO DE LA DISTRIBUCIÓN FIFO:
-- ===============================================================

-- Pago 1: Factura VENCIDA (F1) - $1,000.00
-- IdempotencyKey: BANK:m1234567-1234-1234-1234-123456789abc:F0
-- INSERT INTO pagos:
--   FacturaId: f1111111-1111-1111-1111-111111111111
--   Monto: 1000.00
--   Estado Factura ANTES: Pendiente
--   Estado Factura DESPUÉS: ✅ PAGADA

-- Pago 2: Factura POR VENCER (F2) - $200.00 (del sobrante)
-- IdempotencyKey: BANK:m1234567-1234-1234-1234-123456789abc:F1
-- INSERT INTO pagos:
--   FacturaId: f2222222-2222-2222-2222-222222222222
--   Monto: 200.00
--   Estado Factura ANTES: Pendiente
--   Estado Factura DESPUÉS: ✅ PARCIALMENTE_PAGADA (200/500)

-- Factura 3 (F3): Sin cambios (aún en futuro)
-- Estado: Pendiente (sin pagos)

-- ===============================================================
-- Estado de Cuenta DESPUÉS
-- ===============================================================

-- Consulta para verificar estado de cuenta:
SELECT 
  a.nombre,
  a.apellido,
  COUNT(DISTINCT f.id) as total_facturas,
  SUM(f.monto) as total_facturado,
  COALESCE(SUM(p.monto), 0) as total_pagado,
  SUM(f.monto) - COALESCE(SUM(p.monto), 0) as saldo_pendiente,
  (SELECT COUNT(*) FROM pagos WHERE alumno_id = a.id 
     AND id_empotencia LIKE 'BANK:m1234567%') as pagos_aplicados
FROM alumnos a
LEFT JOIN facturas f ON f.alumno_id = a.id
LEFT JOIN pagos p ON p.factura_id = f.id
WHERE a.id = 'a1111111-1111-1111-1111-111111111111'::uuid
GROUP BY a.id, a.nombre, a.apellido
;

-- Resultado Esperado:
-- nombre  | apellido | total_facturas | total_facturado | total_pagado | saldo_pendiente | pagos_aplicados
-- --------|----------|----------------|-----------------|--------------|-----------------|------------------
-- Carlos  | García   | 3              | 2300.00         | 1200.00      | 1100.00         | 2

-- ===============================================================
-- DETALLE DE CADA FACTURA
-- ===============================================================

SELECT 
  f.numero_factura,
  f.monto,
  COALESCE(SUM(p.monto), 0) as pagado,
  f.monto - COALESCE(SUM(p.monto), 0) as saldo,
  f.estado,
  f.fecha_vencimiento,
  CASE WHEN f.fecha_vencimiento < NOW() THEN '⏰ VENCIDA' 
       ELSE '📅 ' || EXTRACT(DAY FROM f.fecha_vencimiento - NOW()) || ' días' END as fecha_status
FROM facturas f
LEFT JOIN pagos p ON p.factura_id = f.id
WHERE f.alumno_id = 'a1111111-1111-1111-1111-111111111111'::uuid
GROUP BY f.id
ORDER BY f.fecha_vencimiento
;

-- Resultado:
-- numero_factura | monto  | pagado | saldo | estado                | fecha_status
-- --------------|--------|--------|-------|----------------------|---------------
-- FAC-001       | 1000   | 1000   | 0     | ✅ PAGADA            | ⏰ VENCIDA
-- FAC-002       | 500    | 200    | 300   | 📊 PARCIALMENTE_PAGADA | 📅 5 días
-- FAC-003       | 800    | 0      | 800   | ⏳ PENDIENTE         | 📅 30 días

-- ===============================================================
-- DETALLE DE PAGOS APLICADOS
-- ===============================================================

SELECT 
  p.id,
  p.id_empotencia,
  f.numero_factura,
  p.monto,
  p.fecha_pago,
  EXTRACT(HOUR FROM (NOW() - p.fecha_pago)) || ' minutos atrás' as hace
FROM pagos p
JOIN facturas f ON f.id = p.factura_id
WHERE p.id_empotencia LIKE 'BANK:m1234567%'
ORDER BY p.fecha_pago ASC
;

-- Resultado:
-- id           | id_empotencia                                    | numero_factura | monto | hora_pago       | hace
-- -------------|--------------------------------------------------|----------------|-------|-----------------|------------------
-- pago-id-001  | BANK:m1234567-1234-1234-1234-123456789abc:F0     | FAC-001        | 1000  | 2026-01-21...   | 0 minutos atrás
-- pago-id-002  | BANK:m1234567-1234-1234-1234-123456789abc:F1     | FAC-002        | 200   | 2026-01-21...   | 0 minutos atrás

-- ===============================================================
-- CASO 2: Excedente se convierte en ANTICIPO
-- ===============================================================

-- Si el abono es $1,400 (no $1,200):
-- Factura 1: $1,000 ✅ PAGADA
-- Factura 2: $400... pero saldo es $500
--   → Factura 2: PARCIALMENTE_PAGADA ($400/$500)
--   → Sobrante: $0 ✅ (no hay anticipo)

-- Si el abono es $1,500:
-- Factura 1: $1,000 ✅ PAGADA
-- Factura 2: $500 ✅ PAGADA (saldo era $500)
-- Sobrante: $0 ✅

-- Si el abono es $1,600:
-- Factura 1: $1,000 ✅ PAGADA
-- Factura 2: $500 ✅ PAGADA
-- Factura 3: $100 (partial - tiene $800)
--   → Factura 3: PARCIALMENTE_PAGADA ($100/$800)
--   → Sobrante: $0 ✅

-- Si el abono es $3,000 (mayor que deuda total):
-- Factura 1: $1,000 ✅ PAGADA
-- Factura 2: $500 ✅ PAGADA
-- Factura 3: $800 ✅ PAGADA
-- ANTICIPO (FacturaId = NULL): $700
--   IdempotencyKey: BANK:m1234567-1234-1234-1234-123456789abc:ANTICIPO
--   Monto: 700.00
--   → Se guarda para aplicar a futuras colegiaturas

-- ===============================================================
-- CASO 3: Reversión de Conciliación
-- ===============================================================

-- Si el usuario revierte el movimiento:
-- DELETE FROM pagos WHERE id_empotencia LIKE 'BANK:m1234567%'
-- UPDATE facturas SET estado = 'Pendiente'
--   WHERE id IN (f1111111..., f2222222..., ...)

-- Estado de Cuenta DESPUÉS de Reversión:
-- Total Facturado: $2,300.00
-- Total Pagado: $0.00
-- Saldo Pendiente: $2,300.00
-- ✅ Vuelve al estado inicial

-- ===============================================================
-- IDEMPOTENCIA: Aplicar 2 veces el mismo movimiento
-- ===============================================================

-- Primera aplicación: Crea pagos :F0, :F1
-- Segunda aplicación: Verifica IdempotencyKey, ve que ya existen,
--                     devuelve error 409 (Conflict) o no hace nada

-- SQL para verificar:
SELECT 
  COUNT(*) as pagos_duplicados,
  STRING_AGG(DISTINCT id_empotencia, ', ') as keys
FROM pagos
WHERE id_empotencia LIKE 'BANK:m1234567%'
;

-- Esperado: 2 pagos (no 4, no 6...)
-- keys: BANK:m1234567-1234-1234-1234-123456789abc:F0, :F1

-- ===============================================================
-- LIMPIEZA (para pruebas)
-- ===============================================================

-- DELETE FROM pagos WHERE id_empotencia LIKE 'BANK:m1234567%';
-- DELETE FROM facturas WHERE alumno_id = 'a1111111-1111-1111-1111-111111111111'::uuid;
-- DELETE FROM alumnos WHERE id = 'a1111111-1111-1111-1111-111111111111'::uuid;
-- DELETE FROM movimientos_bancarios WHERE id = 'm1234567-1234-1234-1234-123456789abc'::uuid;
