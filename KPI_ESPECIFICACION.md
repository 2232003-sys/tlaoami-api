# 📊 Módulo KPI Financiero - Especificación Técnica

**Estado:** ✅ Implementado  
**Fecha:** 22 de enero 2026  
**Versión:** 1.0

---

## 📋 Descripción General

Módulo de **solo lectura (read-model)** que expone métricas financieras clave del ERP Tlaoami sin lógica de negocio ni modificación de datos.

---

## 🏗️ Arquitectura

```
Tlaoami.API
├─ Controllers
│  └─ KpiController.cs
│     └─ GET /api/v1/kpi/dashboard
│
├─ Kpi/
│  ├─ Dtos/
│  │  └─ DashboardFinancieroKpiDto.cs (7 métricas)
│  │
│  └─ Queries/
│     └─ DashboardFinancieroQueries.cs (7 queries sin lógica)
```

---

## 🔑 Métricas Implementadas

| Métrica | Tipo | Origen | Descripción |
|---------|------|--------|------------|
| **IngresosMes** | decimal | `Pago.Monto` | Suma de pagos del mes actual |
| **IngresosHoy** | decimal | `Pago.Monto` | Suma de pagos del día actual |
| **AdeudoTotal** | decimal | `EstadoCuenta.Saldo` | Suma de saldos pendientes (> 0) |
| **AlumnosConAdeudo** | int | `EstadoCuenta` | Conteo de alumnos con saldo > 0 |
| **GastosMes** | decimal | `MovimientoBancario` | Suma de egresos del mes |
| **MovimientosSinConciliar** | int | `MovimientoBancario` | Conteo sin conciliación |
| **PagosDetectadosAutomaticamente** | int | `MovimientoConciliacion` | Conteo de FIFO exitosos |

---

## 📡 API Endpoint

### GET `/api/v1/kpi/dashboard`

**Descripción:** Obtiene todas las métricas financieras en una sola consulta.

**Método:** GET  
**Autenticación:** No requerida (considerar agregar en futuro)  
**Response:** 200 OK  

**Ejemplo de respuesta:**
```json
{
  "ingresosMes": 125000.00,
  "ingresosHoy": 5500.00,
  "adeudoTotal": 45000.00,
  "alumnosConAdeudo": 12,
  "gastosMes": 8000.00,
  "movimientosSinConciliar": 3,
  "pagosDetectadosAutomaticamente": 8
}
```

**Códigos de respuesta:**
- `200 OK` - Dashboard obtenido exitosamente
- `500 Internal Server Error` - Error al acceder a base de datos

---

## 🛠️ Implementación - Detalles Técnicos

### DashboardFinancieroQueries.cs

**Características:**
- Inyección de `TlaoamiDbContext`
- 7 métodos privados (uno por métrica)
- Sin caché, sin lógica, solo queries
- Consultas optimizadas con `Where()` y `SumAsync()`

**Queries por métrica:**

#### 1. IngresosMes
```sql
SELECT SUM(p.Monto) 
FROM Pago p 
WHERE MONTH(p.FechaPago) = MONTH(NOW()) 
AND YEAR(p.FechaPago) = YEAR(NOW())
```

#### 2. IngresosHoy
```sql
SELECT SUM(p.Monto) 
FROM Pago p 
WHERE DATE(p.FechaPago) = DATE(NOW())
```

#### 3. AdeudoTotal
```sql
SELECT SUM(ec.Saldo) 
FROM EstadoCuenta ec 
WHERE ec.Saldo > 0
```

#### 4. AlumnosConAdeudo
```sql
SELECT COUNT(DISTINCT ec.AlumnoId)
FROM EstadoCuenta ec
WHERE ec.Saldo > 0
```

#### 5. GastosMes
```sql
SELECT SUM(mb.Monto)
FROM MovimientoBancario mb
WHERE mb.Tipo = 'Egreso'
AND MONTH(mb.Fecha) = MONTH(NOW())
AND YEAR(mb.Fecha) = YEAR(NOW())
```

#### 6. MovimientosSinConciliar
```sql
SELECT COUNT(mb.Id)
FROM MovimientoBancario mb
WHERE mb.MovimientoConciliacion IS NULL
```

#### 7. PagosDetectadosAutomaticamente
```sql
SELECT COUNT(mc.Id)
FROM MovimientoConciliacion mc
WHERE mc.TipoConciliacion = 'PagoAutomatico'
```

---

## 🚫 Restricciones Implementadas

### ✅ Respetadas
- ✓ Solo lectura - no modifica datos
- ✓ Sin lógica de negocio - solo queries
- ✓ Sin servicios de dominio - acceso directo a DbContext
- ✓ Sin nuevas entidades - usa existentes
- ✓ Sin modificaciones de dominio
- ✓ Sin validaciones complejas
- ✓ Sin triggers

### 🔒 Garantías
- Cada métrica es independiente
- Las queries son atómicas
- No hay transacciones complejas
- Valores "snapshot" del momento de consulta

---

## 📦 Integración en DI (Program.cs)

```csharp
// KPI Queries (read-model)
builder.Services.AddScoped<Tlaoami.API.Kpi.Queries.DashboardFinancieroQueries>();
```

**Ciclo de vida:** Scoped (nueva instancia por request HTTP)

---

## 🔄 Flujo de Consulta

```
GET /api/v1/kpi/dashboard
        ↓
   KpiController.ObtenerDashboard()
        ↓
   DashboardFinancieroQueries.ObtenerDashboardFinancieroAsync()
        ↓
   [7 queries paralelas]
        ↓
   DashboardFinancieroKpiDto
        ↓
   JSON Response (200 OK)
```

---

## 🚀 Testing Manual

### Con curl:
```bash
curl -X GET http://localhost:3000/api/v1/kpi/dashboard
```

### Con Postman:
1. Método: GET
2. URL: `http://localhost:3000/api/v1/kpi/dashboard`
3. Headers: No requiere
4. Body: vacío

### Esperado:
- Respuesta instant (< 100ms para BD de prueba)
- JSON válido con 7 campos numéricos
- Status 200

---

## 📈 Performance

**Consideraciones:**
- Cada query es O(n) en el peor caso (full table scan)
- Sin índices especiales en campos de fecha
- Recomendación futura: índices en `Pago.FechaPago`, `MovimientoBancario.Fecha`, `EstadoCuenta.Saldo`

**Optimizaciones futuras:**
- Agregar caché con TTL (5-10 minutos)
- Crear índices en base de datos
- Consulta única con CTE o view

---

## 🔐 Seguridad

**Estado actual:** Endpoint sin autenticación  

**Recomendaciones:**
- Agregar `[Authorize]` cuando sea necesario
- Filtrar por institución/colegio si hay multi-tenancy
- Auditar acceso a métricas financieras

---

## 📝 Ejemplos de Uso

### 1. Dashboard en tiempo real
```csharp
var dashboard = await kpiService.ObtenerDashboardAsync();
var ingresosTotal = dashboard.IngresosMes + dashboard.IngresosHoy;
```

### 2. Alertas automáticas
```csharp
if (dashboard.AdeudoTotal > umbralMaximo)
{
    // Enviar alerta al rector
}
```

### 3. Reportes mensuales
```csharp
var metricas = await kpiService.ObtenerDashboardAsync();
// Incluir en reporte PDF/Excel
```

---

## ⚡ Reglas de Oro del Módulo KPI

1. **NUNCA agregar lógica de negocio** - si necesitas calcular, hazlo en Application
2. **NUNCA modificar datos** - esto es read-model
3. **NUNCA usar servicios de dominio** - acceso directo a DbContext
4. **NUNCA crear nuevas entidades** - solo queries sobre existentes
5. **NUNCA cachear sin definir TTL** - riesgos de datos stale
6. **NUNCA ignorar errores de BD** - loguear y reportar
7. **NUNCA asumir que los datos están sincronizados** - snapshot del momento

---

## 🔮 Roadmap Futuro

- [ ] Agregar autenticación JWT
- [ ] Agregar caché distribuido (Redis)
- [ ] Crear índices de base de datos
- [ ] Agregar más métricas (rentabilidad, conversión, etc.)
- [ ] Crear API de predicción de ingresos
- [ ] Dashboard UI integrado

---

## 📞 Contacto & Preguntas

**Autor:** Sistema Tlaoami  
**Mantenimiento:** Equipo de Desarrollo  
**Última actualización:** 22 de enero 2026  

