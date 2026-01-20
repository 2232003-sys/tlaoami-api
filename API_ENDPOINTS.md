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
  # API Backend — Endpoints Disponibles (v1)

  Este documento refleja exactamente lo implementado en los controllers bajo `src/Tlaoami.API/Controllers`.

  ## Alumnos (`/api/v1/Alumnos`)

  - GET `/api/v1/Alumnos` — Lista de alumnos. Respuesta: `AlumnoDto[]`
  - GET `/api/v1/Alumnos/{id}` — Alumno por id. Respuesta: `AlumnoDto`
  - GET `/api/v1/Alumnos/matricula/{matricula}` — Alumno por matrícula. Respuesta: `AlumnoDto`
  - GET `/api/v1/Alumnos/{id}/grupo-actual` — Alumno con su grupo activo. Respuesta: `AlumnoDto`
  - GET `/api/v1/Alumnos/{id}/estado-cuenta` — Estado de cuenta. Respuesta: `EstadoCuentaDto`
  - POST `/api/v1/Alumnos` — Crear alumno. Cuerpo: `AlumnoCreateDto`. Respuesta: `AlumnoDto` (201)
  - PUT `/api/v1/Alumnos/{id}` — Actualizar alumno. Cuerpo: `AlumnoUpdateDto`. Respuesta: `AlumnoDto`
  - DELETE `/api/v1/Alumnos/{id}` — Eliminar alumno. Respuesta: 204

  ---

  ## Asignaciones (`/api/v1/Asignaciones`)

  - POST `/api/v1/Asignaciones/alumno-grupo` — Asignar alumno a grupo. Cuerpo: `AsignarAlumnoGrupoDto`. Respuesta: `AlumnoGrupoDto` (201)
  - GET `/api/v1/Asignaciones/alumno/{alumnoId}/grupo-actual` — Grupo actual del alumno. Respuesta: `GrupoDto`
  - POST `/api/v1/Asignaciones/alumno/{alumnoId}/desasignar` — Desasignar alumno del grupo activo. Respuesta: 204

  ---

  ## Ciclos (`/api/v1/Ciclos`)

  - GET `/api/v1/Ciclos` — Listar ciclos. Respuesta: `CicloEscolarDto[]`
  - GET `/api/v1/Ciclos/activo` — Ciclo activo. Respuesta: `CicloEscolarDto`
  - GET `/api/v1/Ciclos/{id}` — Ciclo por id. Respuesta: `CicloEscolarDto`
  - POST `/api/v1/Ciclos` — Crear ciclo. Cuerpo: `CicloEscolarCreateDto`. Respuesta: `CicloEscolarDto` (201)
  - PUT `/api/v1/Ciclos/{id}` — Actualizar ciclo. Cuerpo: `CicloEscolarCreateDto`. Respuesta: `CicloEscolarDto`
  - DELETE `/api/v1/Ciclos/{id}` — Eliminar ciclo. Respuesta: 204

  ---

  ## Grupos (`/api/v1/Grupos`)

  - GET `/api/v1/Grupos` — Listar grupos. Respuesta: `GrupoDto[]`
  - GET `/api/v1/Grupos/ciclo/{cicloId}` — Grupos por ciclo. Respuesta: `GrupoDto[]`
  - GET `/api/v1/Grupos/{id}` — Grupo por id. Respuesta: `GrupoDto`
  - POST `/api/v1/Grupos` — Crear grupo. Cuerpo: `GrupoCreateDto`. Respuesta: `GrupoDto` (201)
  - PUT `/api/v1/Grupos/{id}` — Actualizar grupo. Cuerpo: `GrupoCreateDto`. Respuesta: `GrupoDto`
  - DELETE `/api/v1/Grupos/{id}` — Eliminar grupo. Respuesta: 204

  ---

  ## Facturas (`/api/v1/Facturas`)

  - GET `/api/v1/Facturas` — Listado con filtros: `alumnoId`, `estado`, `desde`, `hasta`. Respuesta: `FacturaDetalleDto[]`
  - GET `/api/v1/Facturas/detalle` — Todas las facturas con detalle. Respuesta: `FacturaDetalleDto[]`
  - GET `/api/v1/Facturas/{id}` — Factura por id (sin detalle). Respuesta: `FacturaDto`
  - GET `/api/v1/Facturas/{id}/detalle` — Factura por id con detalle. Respuesta: `FacturaDetalleDto`
  - GET `/api/v1/Facturas/alumno/{alumnoId}` — Facturas de un alumno (detalle). Respuesta: `FacturaDetalleDto[]`
  - POST `/api/v1/Facturas/{id}/emitir` — Emitir factura. Idempotente: si ya está emitida o con pagos → 200 OK; si cancelada → 409.
  - POST `/api/v1/Facturas/{id}/cancelar` — Cancelar factura. Cuerpo opcional: `{ motivo: string }`. Idempotente: si ya está cancelada → 200 OK; si pagada → 409.
  - POST `/api/v1/Facturas` — Crear factura. Cuerpo: `CrearFacturaDto`. Respuesta: `FacturaDto` (201)
  - PUT `/api/v1/Facturas/{id}` — Actualizar factura. Cuerpo: `FacturaDto`. Respuesta: 204
  - DELETE `/api/v1/Facturas/{id}` — Eliminar factura. Respuesta: 204

  ---

  ## Pagos (`/api/v1/Pagos`)

  - POST `/api/v1/Pagos` — Registrar pago idempotente. Cuerpo: `PagoCreateDto` (requiere `idempotencyKey` 8-128 chars). Si ya existe el pago con mismo `FacturaId` y `IdempotencyKey` → 200 con el mismo recurso; si no, crea 201. Respuesta: `PagoDto`.
  - GET `/api/v1/Pagos/{id}` — Obtener pago por id. Respuesta: `PagoDto`
  - GET `/api/v1/Pagos/factura/{facturaId}` — Pagos por factura. Respuesta: `PagoDto[]`

  Notas: Registrar un pago actualiza automáticamente el estado de la factura cuando corresponde.

  ---

  ## Pagos Online (`/api/v1/pagos-online`)

  - POST `/api/v1/pagos-online/intents` — Crear intención de pago. Cuerpo: `CrearPaymentIntentDto`. Respuesta: `PaymentIntentDto`
  - GET `/api/v1/pagos-online/intents/{id}` — Obtener intención por id. Respuesta: `PaymentIntentDto`
  - GET `/api/v1/pagos-online/facturas/{facturaId}` — Intenciones por factura. Respuesta: `PaymentIntentDto[]`
  - POST `/api/v1/pagos-online/{id}/confirmar` — Confirmar intención (crea Pago). Cuerpo: `ConfirmarPaymentIntentDto`. Respuesta: `PaymentIntentDto`
  - POST `/api/v1/pagos-online/{id}/cancelar` — Cancelar intención. Cuerpo: `CancelarPaymentIntentDto`. Respuesta: `PaymentIntentDto`
  - POST `/api/v1/pagos-online/{id}/webhook-simulado` — Simular webhook de proveedor. Cuerpo: `WebhookSimuladoDto`. Respuesta: `PaymentIntentDto`

  ---

  ## Conciliación (`/api/v1/conciliacion`)

  - GET `/api/v1/conciliacion/movimientos` — Listar movimientos bancarios. Query: `estado`, `tipo`, `desde`, `hasta`, `page`, `pageSize`. Respuesta: `MovimientoBancarioDto[]`
  - POST `/api/v1/conciliacion/importar-estado-cuenta` — Importar CSV. Form-data campo: `archivoCsv`. Respuesta: `ImportacionResultadoDto`
  - POST `/api/v1/conciliacion/conciliar` — Conciliar un movimiento. Cuerpo: `ConciliarRequest`. Respuesta: 200 con mensaje
  - POST `/api/v1/conciliacion/revertir` — Revertir conciliación. Cuerpo: `RevertirRequest`. Respuesta: 200 con mensaje
  - GET `/api/v1/conciliacion/{movimientoBancarioId}/sugerencias` — Sugerencias para un movimiento. Respuesta: `SugerenciaConciliacionDto[]`
  - GET `/api/v1/conciliacion/conciliados` — Conciliaciones realizadas (query `desde`, `hasta`). Respuesta: `ConciliacionDetalleDto[]`

  ---

  ## Setup (DEV) (`/api/v1/Setup`)

  - POST `/api/v1/Setup/test-data` — Crea datos mínimos de prueba. Respuesta: `{ facturaId, alumnoId }`
  - POST `/api/v1/Setup/seed-alumnos-grupos` — Crea ciclo 2025-2026, grupos 2A/2B y alumnos ejemplo con asignaciones activas (idempotente). Respuesta: `{ message, created[] }`

  ---

  ## Enums y Estados (referencia)

  - EstadoFactura: `Pendiente | ParcialmentePagada | Pagada | Vencida`
  - MetodoPago: `Tarjeta | Transferencia | Efectivo`
  - EstadoPaymentIntent: `Pendiente | Procesando | Completado | Fallido | Cancelado`

  ---

  ## Notas

  - Todas las rutas (salvo Pagos Online y Conciliación) usan convención `api/v1/[Controller]` con mayúsculas del nombre del controller real (`Alumnos`, `Pagos`, etc.).
  - `GET /api/v1/Facturas` devuelve detalle (incluye pagos y alumno) cuando aplica; use `/{id}` para DTO simple.
  - La importación de conciliación requiere `archivoCsv` como campo de formulario.

  ## Ejemplos rápidos

  Crear factura

  ```http
  POST /api/v1/Facturas
  Content-Type: application/json

  {
    "alumnoId": "12345678-1234-1234-1234-123456789012",
    "numeroFactura": "F-2026-001",
    "monto": 5000,
    "fechaEmision": "2026-01-15",
    "fechaVencimiento": "2026-02-15",
    "estado": "Pendiente"
  }
  ```

  Registrar pago

  ```http
  POST /api/v1/Pagos
  Content-Type: application/json

  {
    "facturaId": "{factura-id}",
    "monto": 5000,
    "fechaPago": "2026-01-18",
    "metodo": "Transferencia"
  }
  ```

  Importar estado de cuenta (CSV)

  ```http
  POST /api/v1/conciliacion/importar-estado-cuenta
  Content-Type: multipart/form-data

  archivoCsv=@/ruta/estado_cuenta.csv
  ```
### GET `/api/conciliacion/movimientos`
