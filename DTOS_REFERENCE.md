# DTOs de Facturas y Pagos

Este documento contiene la definición completa de todos los DTOs relacionados con facturas y pagos.

## Facturas

### FacturaDto
```csharp
public class FacturaDto
{
    public Guid Id { get; set; }
    public Guid AlumnoId { get; set; }
    public string? NumeroFactura { get; set; }
    public decimal Monto { get; set; }
    public decimal Saldo { get; set; }  // Calculado: Monto - TotalPagado
    public DateTime FechaEmision { get; set; }
    public DateTime FechaVencimiento { get; set; }
    public string? Estado { get; set; }  // "Pendiente", "ParcialmentePagada", "Pagada", "Vencida"
}
```

### FacturaDetalleDto
DTO extendido que incluye información del alumno y los pagos asociados.
```csharp
public class FacturaDetalleDto
{
    public Guid Id { get; set; }
    public Guid AlumnoId { get; set; }
    public string? AlumnoNombreCompleto { get; set; }  // "{Nombre} {Apellido}"
    public string? NumeroFactura { get; set; }
    public decimal Monto { get; set; }
    public decimal Saldo { get; set; }  // Calculado: Monto - TotalPagado
    public decimal TotalPagado { get; set; }  // Suma de todos los pagos
    public DateTime FechaEmision { get; set; }
    public DateTime FechaVencimiento { get; set; }
    public string? Estado { get; set; }
    public List<PagoDto> Pagos { get; set; } = new();
}
```

**Uso recomendado:**
- `FacturaDto`: Para listados simples, filtros, operaciones CRUD
- `FacturaDetalleDto`: Para vistas detalladas que muestran alumno y pagos

---

## Pagos

### PagoDto
```csharp
public class PagoDto
{
    public Guid Id { get; set; }
    public Guid FacturaId { get; set; }
    public decimal Monto { get; set; }
    public DateTime FechaPago { get; set; }
    public string? Metodo { get; set; }  // "Tarjeta", "Transferencia", "Efectivo"
}
```

### PagoCreateDto
DTO para crear un nuevo pago.
```csharp
public class PagoCreateDto
{
    public Guid FacturaId { get; set; }
    public decimal Monto { get; set; }
    public DateTime FechaPago { get; set; }
    public string Metodo { get; set; } = "Efectivo";  // Valor por defecto
}
```

**Validaciones en el servicio:**
- Factura debe existir
- Factura no debe estar en estado "Pagada"
- Actualiza automáticamente estado de la factura si `TotalPagado >= Monto`

---

## Enumeraciones

### EstadoFactura (Domain Entity)
```csharp
public enum EstadoFactura
{
    Pendiente,           // Sin pagos o saldo > 0
    ParcialmentePagada,  // Tiene pagos pero saldo > 0
    Pagada,              // TotalPagado >= Monto
    Vencida              // Pasó FechaVencimiento sin estar pagada
}
```

### MetodoPago (Domain Entity)
```csharp
public enum MetodoPago
{
    Tarjeta,       // Tarjeta de crédito/débito
    Transferencia, // Transferencia bancaria
    Efectivo       // Pago en efectivo
}
```

---

## Reglas de Negocio

### Actualización automática de estado de factura:
1. Al crear un pago, se calcula `TotalPagado = SUM(Pagos.Monto)`
2. Si `TotalPagado >= Factura.Monto` → Estado = `Pagada`
3. Si `TotalPagado > 0 && TotalPagado < Factura.Monto` → Estado = `ParcialmentePagada`
4. Si `TotalPagado == 0` → Estado = `Pendiente`

### Cálculo de Saldo:
```csharp
Saldo = Monto - TotalPagado
```

---

## Ejemplos de JSON

### Crear Factura (POST)
```json
{
  "alumnoId": "12345678-1234-1234-1234-123456789012",
  "numeroFactura": "F-2026-001",
  "monto": 10000.00,
  "fechaEmision": "2026-01-18T00:00:00",
  "fechaVencimiento": "2026-02-18T00:00:00",
  "estado": "Pendiente"
}
```

### Response FacturaDetalleDto (GET)
```json
{
  "id": "87654321-4321-4321-4321-210987654321",
  "alumnoId": "12345678-1234-1234-1234-123456789012",
  "alumnoNombreCompleto": "Juan Pérez",
  "numeroFactura": "F-2026-001",
  "monto": 10000.00,
  "saldo": 4000.00,
  "totalPagado": 6000.00,
  "fechaEmision": "2026-01-18T00:00:00",
  "fechaVencimiento": "2026-02-18T00:00:00",
  "estado": "ParcialmentePagada",
  "pagos": [
    {
      "id": "11111111-1111-1111-1111-111111111111",
      "facturaId": "87654321-4321-4321-4321-210987654321",
      "monto": 5000.00,
      "fechaPago": "2026-01-19T10:30:00",
      "metodo": "Transferencia"
    },
    {
      "id": "22222222-2222-2222-2222-222222222222",
      "facturaId": "87654321-4321-4321-4321-210987654321",
      "monto": 1000.00,
      "fechaPago": "2026-01-20T14:15:00",
      "metodo": "Efectivo"
    }
  ]
}
```

### Registrar Pago (POST)
```json
{
  "facturaId": "87654321-4321-4321-4321-210987654321",
  "monto": 4000.00,
  "fechaPago": "2026-01-21T09:00:00",
  "metodo": "Tarjeta"
}
```

### Response PagoDto
```json
{
  "id": "33333333-3333-3333-3333-333333333333",
  "facturaId": "87654321-4321-4321-4321-210987654321",
  "monto": 4000.00,
  "fechaPago": "2026-01-21T09:00:00",
  "metodo": "Tarjeta"
}
```

---

## Endpoints relacionados

Ver [API_ENDPOINTS.md](./API_ENDPOINTS.md) para la lista completa de endpoints disponibles.
