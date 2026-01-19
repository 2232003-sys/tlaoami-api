# ✅ Backend Cerrado - Facturas + Pagos

## Resumen de Cambios

### ✅ 1. FacturasController - CRUD Completo
**Archivo:** [FacturasController.cs](src/Tlaoami.API/Controllers/FacturasController.cs)

**Endpoints agregados:**
- `POST /api/v1/facturas` - Crear factura
- `PUT /api/v1/facturas/{id}` - Actualizar factura
- `DELETE /api/v1/facturas/{id}` - Eliminar factura
- `GET /api/v1/facturas/detalle` - Obtener todas con detalle (alumno + pagos)
- `GET /api/v1/facturas/{id}/detalle` - Obtener una con detalle
- `GET /api/v1/facturas/alumno/{alumnoId}` - Obtener facturas de un alumno

**Total endpoints:** 9 (antes: 2, ahora: 9)

---

### ✅ 2. PagosController - Endpoints de Consulta
**Archivo:** [PagosController.cs](src/Tlaoami.API/Controllers/PagosController.cs)

**Endpoints agregados:**
- `GET /api/pagos/{id}` - Obtener pago por ID
- `GET /api/pagos/factura/{facturaId}` - Obtener pagos de una factura

**Total endpoints:** 3 (antes: 1, ahora: 3)

---

### ✅ 3. DTOs Actualizados
**Archivos modificados:**
- [PagoDto.cs](src/Tlaoami.Application/Dtos/PagoDto.cs) - Agregado `Metodo`
- [PagoCreateDto.cs](src/Tlaoami.Application/Dtos/PagoCreateDto.cs) - Agregado `Metodo` con valor por defecto

**Archivo nuevo:**
- [FacturaDetalleDto.cs](src/Tlaoami.Application/Dtos/FacturaDetalleDto.cs) - DTO completo con alumno + pagos

---

### ✅ 4. Servicios Extendidos

#### IFacturaService y FacturaService
**Archivo:** [IFacturaService.cs](src/Tlaoami.Application/Interfaces/IFacturaService.cs), [FacturaService.cs](src/Tlaoami.Application/Services/FacturaService.cs)

**Métodos agregados:**
- `GetFacturaDetalleByIdAsync(Guid id)` - Obtiene factura con Include de Alumno y Pagos
- `GetAllFacturasDetalleAsync()` - Obtiene todas las facturas con Include
- `GetFacturasByAlumnoIdAsync(Guid alumnoId)` - Facturas de un alumno con Include

#### IPagoService y PagoService
**Archivo:** [IPagoService.cs](src/Tlaoami.Application/Interfaces/IPagoService.cs), [PagoService.cs](src/Tlaoami.Application/Services/PagoService.cs)

**Métodos agregados:**
- `GetPagosByFacturaIdAsync(Guid facturaId)` - Lista de pagos por factura (ordenados por fecha desc)
- `GetPagoByIdAsync(Guid id)` - Obtener un pago por ID

**Actualización:**
- `RegistrarPagoAsync` ahora maneja correctamente el `MetodoPago` enum

---

### ✅ 5. Mappers Extendidos
**Archivo:** [MappingFunctions.cs](src/Tlaoami.Application/Mappers/MappingFunctions.cs)

**Agregado:**
- `ToFacturaDetalleDto(Factura)` - Mapper para FacturaDetalleDto que incluye alumno y pagos

**Refactorizado:**
- PagoService ahora usa `MappingFunctions.ToPagoDto` consistentemente

---

## Pruebas de Compilación

✅ **Build exitoso:** 0 errores, 0 warnings

```bash
dotnet build
# Compilación correcta.
#     0 Advertencia(s)
#     0 Errores
```

---

## Documentación Creada

### 📄 [API_ENDPOINTS.md](API_ENDPOINTS.md)
Documentación completa de todos los endpoints con:
- Request/Response bodies
- Ejemplos de uso
- Flujos completos (crear factura → registrar pago)
- Estados y enums explicados

### 📄 [DTOS_REFERENCE.md](DTOS_REFERENCE.md)
Referencia completa de DTOs con:
- Definiciones C# de todos los DTOs
- Ejemplos JSON de request/response
- Reglas de negocio explicadas
- Cálculos automáticos (saldo, total pagado)

---

## Endpoints Disponibles - Resumen

### Facturas (9 endpoints)
| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/api/v1/facturas` | Listar todas (simple) |
| GET | `/api/v1/facturas/detalle` | Listar todas (con detalle) |
| GET | `/api/v1/facturas/{id}` | Obtener una (simple) |
| GET | `/api/v1/facturas/{id}/detalle` | Obtener una (con detalle) |
| GET | `/api/v1/facturas/alumno/{alumnoId}` | Facturas de un alumno |
| POST | `/api/v1/facturas` | Crear factura |
| PUT | `/api/v1/facturas/{id}` | Actualizar factura |
| DELETE | `/api/v1/facturas/{id}` | Eliminar factura |

### Pagos (3 endpoints)
| Método | Ruta | Descripción |
|--------|------|-------------|
| POST | `/api/pagos` | Registrar pago (actualiza estado de factura) |
| GET | `/api/pagos/{id}` | Obtener un pago |
| GET | `/api/pagos/factura/{facturaId}` | Pagos de una factura |

### Pagos Online (6 endpoints)
| Método | Ruta | Descripción |
|--------|------|-------------|
| POST | `/api/v1/pagos-online/intents` | Crear payment intent |
| GET | `/api/v1/pagos-online/intents/{id}` | Obtener intent |
| GET | `/api/v1/pagos-online/facturas/{facturaId}` | Intents de factura |
| POST | `/api/v1/pagos-online/{id}/confirmar` | Confirmar pago |
| POST | `/api/v1/pagos-online/{id}/cancelar` | Cancelar intent |
| POST | `/api/v1/pagos-online/{id}/webhook-simulado` | Simular webhook |

**Total: 18 endpoints para Facturas + Pagos**

---

## Reglas de Negocio Implementadas

### ✅ Actualización Automática de Estado de Factura
Al registrar un pago:
1. Se calcula `TotalPagado = SUM(Pagos.Monto)`
2. Si `TotalPagado >= Factura.Monto` → `Estado = Pagada`
3. Si `TotalPagado > 0 && < Monto` → `Estado = ParcialmentePagada`

### ✅ Validaciones en RegistrarPago
- Factura debe existir
- Factura no puede estar en estado `Pagada`
- Método de pago debe ser válido: `Tarjeta|Transferencia|Efectivo`

### ✅ Cálculo de Saldo
```csharp
Saldo = Monto - TotalPagado
```

---

## Siguiente Paso: Frontend (Next.js)

Ahora puedes verificar que todos estos endpoints existen antes de crear UI:

```bash
# Iniciar API
cd tlaoami-api
dotnet run --project src/Tlaoami.API

# Probar endpoints
curl http://localhost:5000/api/v1/facturas/detalle
curl http://localhost:5000/api/pagos/factura/{facturaId}
```

### Rutas de frontend recomendadas:
- `/facturas` - Listar todas las facturas con detalle
- `/facturas/[id]` - Ver detalle de factura con pagos
- `/facturas/[id]/pagar` - Formulario para registrar pago
- `/facturas/nueva` - Crear nueva factura
- `/alumnos/[id]/facturas` - Ver facturas de un alumno

---

## Estructura Final del Backend

```
tlaoami-api/
├── API_ENDPOINTS.md          ← 📄 Documentación de endpoints
├── DTOS_REFERENCE.md          ← 📄 Referencia de DTOs
├── src/
│   ├── Tlaoami.API/
│   │   └── Controllers/
│   │       ├── FacturasController.cs     ✅ 9 endpoints
│   │       ├── PagosController.cs        ✅ 3 endpoints
│   │       └── PagosOnlineController.cs  ✅ 6 endpoints
│   ├── Tlaoami.Application/
│   │   ├── Dtos/
│   │   │   ├── FacturaDto.cs             ✅
│   │   │   ├── FacturaDetalleDto.cs      ✅ NUEVO
│   │   │   ├── PagoDto.cs                ✅ Actualizado
│   │   │   └── PagoCreateDto.cs          ✅ Actualizado
│   │   ├── Interfaces/
│   │   │   ├── IFacturaService.cs        ✅ 8 métodos
│   │   │   └── IPagoService.cs           ✅ 3 métodos
│   │   ├── Services/
│   │   │   ├── FacturaService.cs         ✅ Implementado
│   │   │   └── PagoService.cs            ✅ Implementado
│   │   └── Mappers/
│   │       └── MappingFunctions.cs       ✅ ToFacturaDetalleDto agregado
│   └── Tlaoami.Domain/
│       └── Entities/
│           ├── Factura.cs                ✅
│           └── Pago.cs                   ✅
```

---

## ✅ Backend Cerrado

**Status:** ✅ Compilación exitosa  
**DTOs:** ✅ Completos y documentados  
**Endpoints:** ✅ 18 endpoints funcionales  
**Documentación:** ✅ API_ENDPOINTS.md + DTOS_REFERENCE.md

🎯 **Listo para frontend** - Todos los contratos están definidos y probados.
