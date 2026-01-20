# ✅ Flujo Pagos Online Implementado

## Resumen Ejecutivo

El flujo completo de **Pagos Online con PaymentIntent** está implementado y validado contra PostgreSQL con idempotencia robusta.

## Componentes Implementados

### 1. Endpoints (PagosOnlineController)

| Método | Ruta | Descripción |
|--------|------|-------------|
| POST | `/api/v1/pagos-online/intents` | Crear PaymentIntent para una Factura |
| GET | `/api/v1/pagos-online/intents/{id}` | Obtener PaymentIntent por ID |
| GET | `/api/v1/pagos-online/facturas/{facturaId}` | Listar intents de una Factura |
| POST | `/api/v1/pagos-online/{id}/confirmar` | Confirmar pago (idempotente) |
| POST | `/api/v1/pagos-online/{id}/cancelar` | Cancelar intent |
| POST | `/api/v1/pagos-online/{id}/webhook-simulado` | Simular webhook (idempotente) |

### 2. Lógica de Negocio (PagosOnlineService)

**Método clave:** `EnsurePagoForIntentAsync`
- Busca Pago existente por `PaymentIntentId` o `IdempotencyKey = "ONLINE:{intentId}"`
- Si existe → regresa el existente (idempotente)
- Si no existe → crea Pago único, liga a Factura, recalcula estado
- Maneja unique constraint violations por concurrencia

**Flujo de Confirmación:**
1. Verificar estado del PaymentIntent (solo `Pendiente` → `Pagado`)
2. Actualizar estado a `Pagado`
3. Llamar `EnsurePagoForIntentAsync` para crear/obtener Pago
4. Recalcular estado de Factura con `RecalculateFrom`
5. Guardar en transacción única

**Flujo de Webhook:**
- Idéntico a confirmación en lógica de idempotencia
- Permite múltiples reintentos sin duplicar pagos
- Valida estado antes de procesar

### 3. Idempotencia

**Key estable:** `ONLINE:{PaymentIntentId}`
- Garantiza 1 solo Pago por PaymentIntent
- Unique index en BD: `(FacturaId, IdempotencyKey)`
- Manejo de concurrencia:
  - Catch `DbUpdateException` con unique violation
  - Detach entidad nueva
  - Recuperar existente de BD
  - Devolver pago existente (no falla)

**Protección contra:**
- ✅ Reintentos de confirmación
- ✅ Webhooks duplicados (2-5+ veces)
- ✅ Requests concurrentes (race conditions)

### 4. Recálculo de Estado

Todas las operaciones que crean/modifican Pagos llaman:
```csharp
factura.RecalculateFrom(null, factura.Pagos);
```

Esto actualiza automáticamente:
- `Estado`: Draft → Pendiente → PartiallyPaid → Pagada
- `PaidAmount`: suma de pagos
- `Balance`: monto - pagado

### 5. Validación PostgreSQL

**Smoke Test ejecutado exitosamente:**
1. ✅ Crear PaymentIntent → estado Pendiente
2. ✅ Confirmar 1ra vez → crea Pago, Factura → Pagada
3. ✅ Confirmar 2da vez → 200 OK, no duplica
4. ✅ Webhook 3 veces → 200 OK cada una, no duplica
5. ✅ Factura recalculada → estado Pagada
6. ✅ Verificado en BD: 1 solo Pago con key `ONLINE:{intentId}`

**Query de verificación:**
```sql
SELECT COUNT(*) FROM "Pagos" 
WHERE "IdempotencyKey" = 'ONLINE:<intent-id>' 
   OR "PaymentIntentId" = '<intent-id>';
-- Resultado: 1
```

## Stack Validado

- ✅ **PostgreSQL 16** (tipos nativos: boolean, uuid, timestamptz)
- ✅ **EF Core 8** con Npgsql
- ✅ **Idempotencia** (manual + online)
- ✅ **Concurrencia** (unique constraint + reflection)
- ✅ **Recálculo** centralizado en dominio
- ✅ **UTC DateTimes** para compatibilidad Postgres

## Archivos Clave

```
src/
  Tlaoami.API/Controllers/
    PagosOnlineController.cs           ← Endpoints REST
  Tlaoami.Application/Services/
    PagosOnline/
      PagosOnlineService.cs            ← Lógica de negocio + idempotencia
      IPagosOnlineService.cs           ← Interface
    PagoService.cs                     ← Pagos manuales (también idempotentes)
  Tlaoami.Domain/Entities/
    PaymentIntent.cs                   ← Entidad PaymentIntent
    Pago.cs                            ← Entidad Pago (IdempotencyKey)
    Factura.cs                         ← RecalculateFrom
  Tlaoami.Infrastructure/
    TlaoamiDbContext.cs                ← Configuración EF
    Migrations/
      20260119191054_InitialPostgres.cs ← Migración PostgreSQL

docs/
  SMOKE_PAGOS_ONLINE.md                ← Tests de validación
```

## Próximos Pasos (Opcionales)

### Mejoras Sugeridas
1. **Auditoría**: Tabla `PaymentIntentAudit` para registrar `usuario` y `comentario` en confirmar/cancelar
2. **Notificaciones**: Eventos de dominio al cambiar estado de Factura
3. **Webhooks reales**: Integración con proveedores (Stripe, OpenPay, etc.)
4. **Expiración automática**: Job background para marcar intents expirados
5. **Retry policy**: Implementar Polly para reintentos automáticos en provider

### Documentación Adicional
- OpenAPI/Swagger con ejemplos de payloads
- Diagramas de secuencia (confirmar/webhook)
- Casos de error y códigos HTTP

## Conclusión

🎉 **FLUJO PAGOS ONLINE COMPLETAMENTE FUNCIONAL**

- ✅ Implementación completa sin romper código existente
- ✅ Idempotencia robusta con manejo de concurrencia
- ✅ Recálculo automático de estados
- ✅ Validado con smoke tests contra PostgreSQL
- ✅ Documentación lista para equipo

**El bloque de Pagos Online está sellado y listo para producción** (con las mejoras sugeridas para features avanzados).
