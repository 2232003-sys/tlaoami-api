# API Backend - Endpoints Disponibles

## Facturas (`/api/v1/facturas`)

### GET `/api/v1/facturas`
Obtiene todas las facturas (sin detalle de alumno ni pagos).
**Response:** `FacturaDto[]`
```json
{
  "id": "guid",
  "alumnoId": "guid",
  "numeroFactura": "string",
  "monto": 0,
  "saldo": 0,
  "fechaEmision": "datetime",
  "fechaVencimiento": "datetime",
  "estado": "Pendiente|ParcialmentePagada|Pagada|Vencida"
}
```

### GET `/api/v1/facturas/detalle`
Obtiene todas las facturas CON información completa del alumno y sus pagos.
**Response:** `FacturaDetalleDto[]`
```json
{
  "id": "guid",
  "alumnoId": "guid",
  "alumnoNombreCompleto": "string",
  "numeroFactura": "string",
  "monto": 0,
  "saldo": 0,
  "totalPagado": 0,
  "fechaEmision": "datetime",
  "fechaVencimiento": "datetime",
  "estado": "Pendiente|ParcialmentePagada|Pagada|Vencida",
  "pagos": [
    {
      "id": "guid",
      "facturaId": "guid",
      "monto": 0,
      "fechaPago": "datetime",
      "metodo": "Tarjeta|Transferencia|Efectivo"
    }
  ]
}
```

### GET `/api/v1/facturas/{id}`
Obtiene una factura por ID (sin detalle).
**Response:** `FacturaDto`

### GET `/api/v1/facturas/{id}/detalle`
Obtiene una factura por ID CON detalle completo (alumno + pagos).
**Response:** `FacturaDetalleDto`

### GET `/api/v1/facturas/alumno/{alumnoId}`
Obtiene todas las facturas de un alumno específico CON detalle.
**Response:** `FacturaDetalleDto[]`

### POST `/api/v1/facturas`
Crea una nueva factura.
**Request:** `FacturaDto`
**Response:** `FacturaDto` (201 Created)

### PUT `/api/v1/facturas/{id}`
Actualiza una factura existente.
**Request:** `FacturaDto`
**Response:** 204 No Content

### DELETE `/api/v1/facturas/{id}`
Elimina una factura.
**Response:** 204 No Content

---

## Pagos (`/api/pagos`)

### POST `/api/pagos`
Registra un nuevo pago para una factura.
**Request:** `PagoCreateDto`
```json
{
  "facturaId": "guid",
  "monto": 0,
  "fechaPago": "datetime",
  "metodo": "Tarjeta|Transferencia|Efectivo"
}
```
**Response:** `PagoDto` (201 Created)
**Nota:** Actualiza automáticamente el estado de la factura si se alcanza el monto total.

### GET `/api/pagos/{id}`
Obtiene un pago por ID.
**Response:** `PagoDto`

### GET `/api/pagos/factura/{facturaId}`
Obtiene todos los pagos de una factura específica (ordenados por fecha desc).
**Response:** `PagoDto[]`

---

## Pagos Online (`/api/v1/pagos-online`)

### POST `/api/v1/pagos-online/intents`
Crea una intención de pago (payment intent).
**Request:** `CrearPaymentIntentDto`
```json
{
  "facturaId": "guid",
  "monto": 0,
  "usuario": "string"
}
```
**Response:** `PaymentIntentDto` (200 OK)

### GET `/api/v1/pagos-online/intents/{id}`
Obtiene una intención de pago por ID.
**Response:** `PaymentIntentDto`

### GET `/api/v1/pagos-online/facturas/{facturaId}`
Obtiene todas las intenciones de pago de una factura.
**Response:** `PaymentIntentDto[]`

### POST `/api/v1/pagos-online/{id}/confirmar`
Confirma una intención de pago (crea el pago real).
**Request:** `ConfirmarPaymentIntentDto`
```json
{
  "usuario": "string",
  "comentario": "string"
}
```
**Response:** `PaymentIntentDto`

### POST `/api/v1/pagos-online/{id}/cancelar`
Cancela una intención de pago.
**Request:** `CancelarPaymentIntentDto`
```json
{
  "usuario": "string",
  "comentario": "string"
}
```
**Response:** `PaymentIntentDto`

### POST `/api/v1/pagos-online/{id}/webhook-simulado`
Simula un webhook de proveedor de pagos (para testing).
**Request:** `WebhookSimuladoDto`
```json
{
  "estado": "succeeded|failed|cancelled",
  "comentario": "string"
}
```
**Response:** `PaymentIntentDto`

---

## Conciliación (`/api/conciliacion`)

### POST `/api/conciliacion/importar-csv`
Importa movimientos bancarios desde CSV.
**Request:** Multipart/form-data con archivo CSV
**Response:** `ImportacionResultadoDto`

### GET `/api/conciliacion/movimientos`
Obtiene todos los movimientos bancarios.
**Response:** `MovimientoBancarioDto[]`

### GET `/api/conciliacion/movimientos/{id}`
Obtiene un movimiento bancario por ID.
**Response:** `MovimientoBancarioDto`

### GET `/api/conciliacion/sugerencias`
Obtiene sugerencias de conciliación.
**Response:** `SugerenciaConciliacionDto[]`

### POST `/api/conciliacion/conciliar`
Concilia un movimiento con una factura.
**Request:** `ConciliarDto`
**Response:** `ConciliacionDetalleDto`

### GET `/api/conciliacion`
Obtiene todas las conciliaciones realizadas.
**Response:** `ConciliacionDetalleDto[]`

---

## Estados y Enums

### EstadoFactura
- `Pendiente`: Factura sin pagos
- `ParcialmentePagada`: Tiene pagos pero no alcanza el monto total
- `Pagada`: Pagada completamente
- `Vencida`: Pasó su fecha de vencimiento sin pagar

### MetodoPago
- `Tarjeta`: Pago con tarjeta de crédito/débito
- `Transferencia`: Transferencia bancaria
- `Efectivo`: Pago en efectivo

### EstadoPaymentIntent
- `Pendiente`: Creado, esperando pago
- `Procesando`: En proceso de pago
- `Completado`: Pago exitoso
- `Fallido`: Pago rechazado
- `Cancelado`: Cancelado manualmente

---

## Notas Importantes

1. **Facturas:**
   - Use `/detalle` endpoints para UIs que necesitan mostrar nombre del alumno y pagos
   - Use endpoints simples para listados rápidos sin datos relacionados
   - El estado se actualiza automáticamente al registrar pagos

2. **Pagos:**
   - Cada pago se asocia a UNA factura
   - Se puede pagar parcialmente una factura
   - El estado de la factura cambia automáticamente a `Pagada` cuando totalPagado >= monto

3. **Pagos Online:**
   - Flujo: Crear intent → Webhook simula procesamiento → Confirmar/Cancelar
   - Un payment intent confirmado crea automáticamente un Pago
   - Útil para simular integraciones con Stripe/MercadoPago/etc

4. **Conciliación:**
   - Importar movimientos bancarios (CSV)
   - Sistema sugiere matches automáticos
   - Confirmar conciliaciones manualmente

---

## Ejemplos de Uso

### Flujo: Crear factura y registrar pago

1. **Crear factura**
```http
POST /api/v1/facturas
{
  "alumnoId": "12345678-1234-1234-1234-123456789012",
  "numeroFactura": "F-2026-001",
  "monto": 5000,
  "fechaEmision": "2026-01-15",
  "fechaVencimiento": "2026-02-15",
  "estado": "Pendiente"
}
```

2. **Obtener factura con detalle**
```http
GET /api/v1/facturas/{id}/detalle
```

3. **Registrar pago**
```http
POST /api/pagos
{
  "facturaId": "{factura-id}",
  "monto": 5000,
  "fechaPago": "2026-01-18",
  "metodo": "Transferencia"
}
```

4. **Ver pagos de la factura**
```http
GET /api/pagos/factura/{factura-id}
```

### Flujo: Pago online

1. **Crear intención de pago**
```http
POST /api/v1/pagos-online/intents
{
  "facturaId": "{factura-id}",
  "monto": 5000,
  "usuario": "admin@tlaoami.com"
}
```

2. **Simular webhook de proveedor**
```http
POST /api/v1/pagos-online/{intent-id}/webhook-simulado
{
  "estado": "succeeded",
  "comentario": "Pago procesado exitosamente"
}
```

3. **Confirmar el pago**
```http
POST /api/v1/pagos-online/{intent-id}/confirmar
{
  "usuario": "admin@tlaoami.com",
  "comentario": "Confirmado por administrador"
}
```
