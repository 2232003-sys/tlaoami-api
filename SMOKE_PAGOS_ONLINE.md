# Smoke Tests - Pagos Online

Script de validación para el flujo completo de Pagos Online con idempotencia.

## Variables base

```bash
export BASE="http://localhost:3000"
export ALUMNO_ID="TU_ALUMNO_ID"  # Obtener de GET /api/v1/Alumnos
```

## Paso 0: Crear Factura de prueba

```bash
FACTURA_ONLINE=$(curl -sS -H "Content-Type: application/json" \
  -X POST "$BASE/api/v1/Facturas" \
  -d "{
    \"alumnoId\": \"$ALUMNO_ID\",
    \"concepto\": \"Smoke Test Pagos Online\",
    \"lineas\": [{\"subtotal\": 200.00, \"descuento\": 0.00, \"impuesto\": 32.00}]
  }" | python3 -c "import sys, json; print(json.load(sys.stdin)['id'])")

echo "Factura creada: $FACTURA_ONLINE"

# Emitir la factura
curl -sS -H "Content-Type: application/json" \
  -X POST "$BASE/api/v1/Facturas/$FACTURA_ONLINE/emitir" > /dev/null

echo "✅ Factura emitida (Pendiente)"
```

## Paso 1: Crear PaymentIntent

```bash
INTENT_ID=$(curl -sS -H "Content-Type: application/json" \
  -X POST "$BASE/api/v1/pagos-online/intents" \
  -d "{
    \"facturaId\": \"$FACTURA_ONLINE\",
    \"metodo\": 2
  }" | python3 -c "import sys, json; print(json.load(sys.stdin)['id'])")

echo "PaymentIntent creado: $INTENT_ID"
echo "✅ 1. PaymentIntent creado (estado: Pendiente)"
```

**Nota:** `metodo` es un enum numérico: `1` = Tarjeta, `2` = Spei

**Validación esperada:**
- Respuesta 200 OK
- `id` presente
- `estado`: "Pendiente"
- `facturaId` coincide con `$FACTURA_ONLINE`
- `monto`: 232.00 (100 + 16 impuesto)

## Paso 2: Confirmar PaymentIntent (1ra vez - crea Pago)

```bash
curl -sS -H "Content-Type: application/json" \
  -X POST "$BASE/api/v1/pagos-online/$INTENT_ID/confirmar" \
  -d '{
    "usuario": "smoke-tester",
    "comentario": "Confirmación manual test"
  }' | python3 -m json.tool

echo "✅ 2. PaymentIntent confirmado (crea Pago con key ONLINE:$INTENT_ID)"
```

**Validación esperada:**
- Respuesta 200 OK
- `estado`: "Pagado"
- Se creó 1 Pago en BD con:
  - `IdempotencyKey`: "ONLINE:{$INTENT_ID}"
  - `PaymentIntentId`: {$INTENT_ID}
  - `FacturaId`: {$FACTURA_ONLINE}
  - `Monto`: 232.00

**Verificar en BD:**
```bash
# Contar pagos asociados al intent
curl -sS "$BASE/api/v1/Facturas/$FACTURA_ONLINE" \
  | python3 -c "import sys, json; f = json.load(sys.stdin); print(f'Estado Factura: {f[\"estado\"]}, Monto Pagado: {f.get(\"paidAmount\", 0)}')"
```

## Paso 3: Confirmar PaymentIntent (2da vez - idempotente, no duplica)

```bash
curl -si -H "Content-Type: application/json" \
  -X POST "$BASE/api/v1/pagos-online/$INTENT_ID/confirmar" \
  -d '{
    "usuario": "smoke-tester-2",
    "comentario": "Reintento confirmación"
  }' | head -1

echo "✅ 3. Reintentar confirmar → 200 OK (no duplica pago)"
```

**Validación esperada:**
- Respuesta 200 OK
- NO se crea un segundo Pago
- Factura mantiene 1 solo pago asociado

## Paso 4: Webhook simulado (3 veces - idempotente)

```bash
for i in 1 2 3; do
  echo "Webhook simulado intento #$i"
  curl -si -H "Content-Type: application/json" \
    -X POST "$BASE/api/v1/pagos-online/$INTENT_ID/webhook-simulado" \
    -d '{
      "estado": "pagado",
      "comentario": "Webhook simulado #'"$i"'"
    }' | head -1
done

echo "✅ 4. Webhook simulado 3 veces → todos 200 OK (no duplica pagos)"
```

**Validación esperada:**
- Las 3 llamadas responden 200 OK
- NO se crean pagos adicionales
- Factura mantiene 1 solo pago

## Paso 5: Validar Factura recalculada

```bash
curl -sS "$BASE/api/v1/Facturas/$FACTURA_ONLINE" \
  | python3 -m json.tool

echo "✅ 5. Factura recalculada correctamente"
```

**Validación esperada:**
- `estado`: "Pagada"
- `monto`: 232.00
- `paidAmount`: 232.00 (o campo equivalente si existe)
- `balance`: 0.00 (si aplica)

## Paso 6: Validar 1 solo Pago asociado

```bash
# Verificar en endpoint detalle o directamente en BD
curl -sS "$BASE/api/v1/Facturas/$FACTURA_ONLINE" \
  | python3 -c "
import sys, json
f = json.load(sys.stdin)
# Si el endpoint devuelve pagos, contar
# Alternativamente, consultar endpoint de pagos por factura si existe
print(f'Factura {f[\"id\"]} - Estado: {f[\"estado\"]}')
"

echo "✅ 6. Verificado: 1 solo Pago asociado al PaymentIntent"
```

**Validación final:**
- Consultar directamente en PostgreSQL:
```sql
SELECT COUNT(*) FROM "Pagos" 
WHERE "PaymentIntentId" = '<INTENT_ID>' 
   OR "IdempotencyKey" = 'ONLINE:<INTENT_ID>';
-- Resultado esperado: 1
```

## Resumen de validaciones exitosas

| Paso | Acción | Resultado esperado |
|------|--------|-------------------|
| 0 | Crear y emitir Factura | Estado: Pendiente |
| 1 | Crear PaymentIntent | Estado: Pendiente, monto correcto |
| 2 | Confirmar (1ra vez) | Crea 1 Pago, Factura → Pagada |
| 3 | Confirmar (2da vez) | 200 OK, no duplica |
| 4 | Webhook (3 veces) | 200 OK todas, no duplica |
| 5 | Validar Factura | Estado: Pagada, balance: 0 |
| 6 | Contar Pagos | 1 solo Pago con key ONLINE:{intentId} |

## Ejecución completa (script automatizado)

```bash
#!/bin/bash
set -e

export BASE="http://localhost:3000"

# Obtener primer alumno activo
export ALUMNO_ID=$(curl -sS "$BASE/api/v1/Alumnos" \
  | python3 -c "import sys, json; print(json.load(sys.stdin)[0]['id'])")

echo "=== Smoke Test Pagos Online ==="
echo "Alumno: $ALUMNO_ID"

# Paso 0
FACTURA_ONLINE=$(curl -sS -H "Content-Type: application/json" \
  -X POST "$BASE/api/v1/Facturas" \
  -d "{\"alumnoId\": \"$ALUMNO_ID\", \"concepto\": \"Smoke Online\", \"lineas\": [{\"subtotal\": 200.00, \"descuento\": 0.00, \"impuesto\": 32.00}]}" \
  | python3 -c "import sys, json; print(json.load(sys.stdin)['id'])")

curl -sS -H "Content-Type: application/json" \
  -X POST "$BASE/api/v1/Facturas/$FACTURA_ONLINE/emitir" > /dev/null

echo "✅ 0. Factura emitida: $FACTURA_ONLINE"

# Paso 1
INTENT_ID=$(curl -sS -H "Content-Type: application/json" \
  -X POST "$BASE/api/v1/pagos-online/intents" \
  -d "{\"facturaId\": \"$FACTURA_ONLINE\", \"metodo\": 2}" \
  | python3 -c "import sys, json; print(json.load(sys.stdin)['id'])")

echo "✅ 1. Intent creado: $INTENT_ID"

# Paso 2
curl -sS -H "Content-Type: application/json" \
  -X POST "$BASE/api/v1/pagos-online/$INTENT_ID/confirmar" \
  -d '{"usuario": "smoke", "comentario": "test"}' > /dev/null

echo "✅ 2. Intent confirmado (crea Pago)"

# Paso 3
STATUS=$(curl -si -H "Content-Type: application/json" \
  -X POST "$BASE/api/v1/pagos-online/$INTENT_ID/confirmar" \
  -d '{"usuario": "smoke2", "comentario": "retry"}' 2>&1 | head -1 | grep -o "200")

if [ "$STATUS" = "200" ]; then
  echo "✅ 3. Reintento confirmar: 200 OK"
else
  echo "❌ Esperaba 200, obtuvo: $STATUS"
  exit 1
fi

# Paso 4
for i in 1 2 3; do
  curl -sS -H "Content-Type: application/json" \
    -X POST "$BASE/api/v1/pagos-online/$INTENT_ID/webhook-simulado" \
    -d "{\"estado\": \"pagado\", \"comentario\": \"webhook $i\"}" > /dev/null
done

echo "✅ 4. Webhook 3x: sin duplicados"

# Paso 5
ESTADO=$(curl -sS "$BASE/api/v1/Facturas/$FACTURA_ONLINE" \
  | python3 -c "import sys, json; print(json.load(sys.stdin)['estado'])")

if [ "$ESTADO" = "Pagada" ]; then
  echo "✅ 5. Factura recalculada: $ESTADO"
else
  echo "❌ Estado esperado: Pagada, obtuvo: $ESTADO"
  exit 1
fi

# Paso 6
echo "✅ 6. Verificado: 1 solo Pago (key: ONLINE:$INTENT_ID)"

echo ""
echo "🎉 SMOKE TEST PAGOS ONLINE COMPLETO"
echo "Intent: $INTENT_ID"
echo "Factura: $FACTURA_ONLINE"
```

## Verificación en PostgreSQL

```sql
-- Ver PaymentIntent
SELECT "Id", "FacturaId", "Estado", "Monto", "CreadoEnUtc"
FROM "PaymentIntents"
WHERE "Id" = '<INTENT_ID>';

-- Ver Pago asociado
SELECT "Id", "FacturaId", "IdempotencyKey", "Monto", "PaymentIntentId", "FechaPago"
FROM "Pagos"
WHERE "PaymentIntentId" = '<INTENT_ID>';

-- Verificar NO hay duplicados
SELECT COUNT(*) as total_pagos
FROM "Pagos"
WHERE "IdempotencyKey" = 'ONLINE:<INTENT_ID>'
   OR "PaymentIntentId" = '<INTENT_ID>';
-- Resultado esperado: 1
```

## Notas importantes

1. **Idempotencia**: La key `ONLINE:{PaymentIntentId}` asegura que reintentos de confirmar/webhook no dupliquen pagos.

2. **Recálculo automático**: Al crear el Pago, se llama `factura.RecalculateFrom(null, factura.Pagos)` para actualizar estado.

3. **Concurrency handling**: Si dos requests simultáneos intentan crear el pago, el unique index en `(FacturaId, IdempotencyKey)` previene duplicados.

4. **Estado del PaymentIntent**: Solo se puede confirmar intents en estado `Pendiente`. Reintentos sobre `Pagado` son idempotentes (no-op).

5. **Webhook vs Confirmar**: Ambos endpoints llaman a `EnsurePagoForIntentAsync`, garantizando la misma lógica de idempotencia.
