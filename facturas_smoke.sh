#!/bin/bash

set -e  # Exit on error

API_URL="http://localhost:3000/api/v1"
echo "=== Test: Módulo de Facturas ==="
echo ""

# 1. Obtener un alumno existente
echo "1. Obteniendo alumno de prueba..."
ALUMNO_ID=$(curl -s "$API_URL/alumnos" | jq -r '.[0].id')
echo "   ✓ Alumno ID: $ALUMNO_ID"
echo ""

# 2. Crear factura nueva
echo "2. Creando factura de colegiatura..."
FACTURA_PAYLOAD=$(cat <<EOF
{
  "alumnoId": "$ALUMNO_ID",
  "monto": 15000,
  "concepto": "Colegiatura Enero 2026",
  "fechaEmision": "2026-01-15T00:00:00Z",
  "fechaVencimiento": "2026-02-15T00:00:00Z"
}
EOF
)

FACTURA_RESPONSE=$(curl -s -X POST "$API_URL/facturas" \
  -H "Content-Type: application/json" \
  -d "$FACTURA_PAYLOAD")
FACTURA_ID=$(echo "$FACTURA_RESPONSE" | jq -r '.id')
NUMERO_FACTURA=$(echo "$FACTURA_RESPONSE" | jq -r '.numeroFactura')
echo "   ✓ Factura creada: $NUMERO_FACTURA (ID: $FACTURA_ID)"
echo ""

# 3. Crear otra factura
echo "3. Creando factura de inscripción..."
FACTURA2_PAYLOAD=$(cat <<EOF
{
  "alumnoId": "$ALUMNO_ID",
  "monto": 5000,
  "concepto": "Inscripción 2026",
  "fechaEmision": "2026-01-10T00:00:00Z"
}
EOF
)

FACTURA2_RESPONSE=$(curl -s -X POST "$API_URL/facturas" \
  -H "Content-Type: application/json" \
  -d "$FACTURA2_PAYLOAD")
FACTURA2_ID=$(echo "$FACTURA2_RESPONSE" | jq -r '.id')
NUMERO_FACTURA2=$(echo "$FACTURA2_RESPONSE" | jq -r '.numeroFactura')
echo "   ✓ Factura creada: $NUMERO_FACTURA2 (ID: $FACTURA2_ID)"
echo ""

# 4. Listar todas las facturas
echo "4. Listando todas las facturas..."
FACTURAS_COUNT=$(curl -s "$API_URL/facturas" | jq '. | length')
echo "   ✓ Total facturas: $FACTURAS_COUNT"
echo ""

# 5. Filtrar facturas por alumno
echo "5. Filtrando facturas por alumno..."
FACTURAS_ALUMNO=$(curl -s "$API_URL/facturas?alumnoId=$ALUMNO_ID" | jq '. | length')
echo "   ✓ Facturas del alumno: $FACTURAS_ALUMNO"
echo ""

# 6. Filtrar por estado
echo "6. Filtrando facturas pendientes..."
FACTURAS_PENDIENTES=$(curl -s "$API_URL/facturas?estado=Pendiente" | jq '. | length')
echo "   ✓ Facturas pendientes: $FACTURAS_PENDIENTES"
echo ""

# 7. Filtrar por rango de fechas
echo "7. Filtrando por rango de fechas (Enero 2026)..."
FACTURAS_ENERO=$(curl -s "$API_URL/facturas?desde=2026-01-01&hasta=2026-01-31" | jq '. | length')
echo "   ✓ Facturas en enero: $FACTURAS_ENERO"
echo ""

# 8. Obtener detalle de factura
echo "8. Obteniendo detalle de factura..."
FACTURA_DETALLE=$(curl -s "$API_URL/facturas/$FACTURA_ID")
CONCEPTO=$(echo "$FACTURA_DETALLE" | jq -r '.concepto')
MONTO=$(echo "$FACTURA_DETALLE" | jq -r '.monto')
SALDO=$(echo "$FACTURA_DETALLE" | jq -r '.saldo')
echo "   ✓ $NUMERO_FACTURA: $CONCEPTO - \$${MONTO} (Saldo: \$${SALDO})"
echo ""

# 9. Actualizar factura
echo "9. Actualizando monto de factura..."
UPDATE_PAYLOAD=$(cat <<EOF
{
  "id": "$FACTURA_ID",
  "alumnoId": "$ALUMNO_ID",
  "numeroFactura": "$NUMERO_FACTURA",
  "concepto": "Colegiatura Enero 2026 (Ajustado)",
  "monto": 16000,
  "saldo": 16000,
  "fechaEmision": "2026-01-15T00:00:00Z",
  "fechaVencimiento": "2026-02-15T00:00:00Z",
  "estado": "Pendiente"
}
EOF
)

curl -s -X PUT "$API_URL/facturas/$FACTURA_ID" \
  -H "Content-Type: application/json" \
  -d "$UPDATE_PAYLOAD" > /dev/null
echo "   ✓ Factura actualizada (nuevo monto: \$16000)"
echo ""

# 10. Ver estado de cuenta del alumno
echo "10. Estado de cuenta del alumno..."
ESTADO_CUENTA=$(curl -s "$API_URL/alumnos/$ALUMNO_ID/estado-cuenta")
TOTAL_FACTURADO=$(echo "$ESTADO_CUENTA" | jq -r '.totalFacturado')
SALDO_PENDIENTE=$(echo "$ESTADO_CUENTA" | jq -r '.saldoPendiente')
echo "   ✓ Total facturado: \$${TOTAL_FACTURADO}"
echo "   ✓ Saldo pendiente: \$${SALDO_PENDIENTE}"
echo ""

# 11. Eliminar una factura (opcional)
echo "11. Eliminando factura de prueba..."
curl -s -X DELETE "$API_URL/facturas/$FACTURA2_ID" > /dev/null
echo "   ✓ Factura $NUMERO_FACTURA2 eliminada"
echo ""

echo "=== ✅ Todos los tests pasaron ==="
