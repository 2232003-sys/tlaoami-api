# Integración de Cumplimiento de Privacidad en Otros Módulos

Este documento describe cómo validar aceptación de Aviso de Privacidad en servicios y controladores específicos.

---

## Escenario 1: Validar en Servicio Antes de Operación

**Ejemplo:** No permitir reinscripción sin aceptación de privacidad

```csharp
// ReinscripcionService.cs
public class ReinscripcionService : IReinscripcionService
{
    private readonly IAvisoPrivacidadService _avisoService;
    private readonly IAsignacionGrupoService _asignacionService;
    
    public ReinscripcionService(
        IAvisoPrivacidadService avisoService,
        IAsignacionGrupoService asignacionService)
    {
        _avisoService = avisoService;
        _asignacionService = asignacionService;
    }

    public async Task ReinscribirAlumnoAsync(Guid alumnoId, Guid grupoId, Guid usuarioId)
    {
        // Validación 1: Aceptación de privacidad
        bool haAceptado = await _avisoService.UsuarioHaAceptadoVigenteAsync(usuarioId);
        if (!haAceptado)
            throw new BusinessException(
                "Usuario debe aceptar el aviso de privacidad para realizar reinscripción.",
                code: "PRIVACIDAD_NO_ACEPTADA");

        // Validación 2: Adeudo (existente)
        // ... resto de la lógica
    }
}
```

---

## Escenario 2: Bypass Manual en Ambiente de Admin

**Ejemplo:** Endpoint especial para admin que **no valida** privacidad (para soporte/emergencias)

```csharp
// FacturasController.cs
[ApiController]
[Route("api/v1/[controller]")]
public class FacturasController : ControllerBase
{
    private readonly IFacturaService _service;

    [HttpPost("crear")]
    [Authorize(Roles = Roles.AdminAndAdministrativo)]
    public async Task<ActionResult<FacturaDto>> CrearFactura([FromBody] FacturaCreateDto dto)
    {
        // Si viene con flag especial de "soporte", bypass de privacidad
        // (En prod: requerir MFA, auditar, etc.)
        if (Request.Headers.TryGetValue("X-Bypass-Privacidad", out var bypass) 
            && bypass == "admin-support-key")
        {
            // Admin puede crear sin privacidad (con auditoría)
            var factura = await _service.CrearFacturaAsync(dto);
            return Ok(factura);
        }

        // Ruta normal: middleware valida privacidad automáticamente
        var resultado = await _service.CrearFacturaAsync(dto);
        return Ok(resultado);
    }
}
```

---

## Escenario 3: Validación Condicional Según Rol

**Ejemplo:** Consulta (rol) puede leer datos sin aceptar; Admin debe aceptar

```csharp
// AlumnosController.cs
[HttpGet("estado-cuenta")]
[Authorize(Roles = Roles.AllRoles)]
public async Task<ActionResult> GetEstadoCuenta(Guid alumnoId)
{
    var usuarioId = ObtenerUsuarioId();
    var usuarioRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

    // Si es Admin/Administrativo, validar privacidad
    if (usuarioRole == Roles.Admin || usuarioRole == Roles.Administrativo)
    {
        bool aceptoPrivacidad = await _avisoService.UsuarioHaAceptadoVigenteAsync(usuarioId);
        if (!aceptoPrivacidad)
            throw new BusinessException(
                "Admin debe aceptar privacidad",
                code: "PRIVACIDAD_REQUERIDA");
    }

    // Si es Consulta, permitir sin privacidad (solo lectura)
    var estado = await _alumnoService.GetEstadoCuentaAsync(alumnoId);
    return Ok(estado);
}
```

---

## Escenario 4: Middleware Selectivo por Endpoint

**Ejemplo:** Solo validar privacidad en endpoints de escritura, no lectura

```csharp
// PrivacidadComplianceMiddleware.cs (versión mejorada)
public class PrivacidadComplianceMiddleware
{
    private readonly RequestDelegate _next;
    
    // Endpoints que NO requieren privacidad
    private readonly string[] _endpointsLectura = new[]
    {
        "/api/v1/ciclos",           // GET solo
        "/api/v1/alumnos",          // GET solo
        "/api/v1/grupos",           // GET solo
    };

    public async Task InvokeAsync(HttpContext context, IAvisoPrivacidadService avisoService)
    {
        var path = context.Request.Path.Value?.ToLowerInvariant() ?? "";
        var method = context.Request.Method;

        // GET en endpoints de lectura = permitir sin privacidad
        if (method == "GET" && _endpointsLectura.Any(e => path.StartsWith(e)))
        {
            await _next(context);
            return;
        }

        // POST/PUT/DELETE = validar privacidad siempre
        var usuarioId = ObtenerUsuarioIdDelContext(context);
        if (usuarioId != Guid.Empty)
        {
            bool acepto = await avisoService.UsuarioHaAceptadoVigenteAsync(usuarioId);
            if (!acepto)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(
                    new { code = "PRIVACIDAD_PENDIENTE", message = "Debe aceptar privacidad para escribir datos." });
                return;
            }
        }

        await _next(context);
    }
}
```

---

## Escenario 5: Auditoría Extendida

**Ejemplo:** Registrar operaciones del usuario después de aceptar privacidad

```csharp
// RegistroAuditoriaService.cs
public class RegistroAuditoriaService
{
    private readonly TlaoamiDbContext _context;
    private readonly IAvisoPrivacidadService _avisoService;

    public async Task RegistrarOperacionAsync(
        Guid usuarioId,
        string entidad,
        string operacion,  // Create, Update, Delete
        string detalles)
    {
        // Verificar que usuario aceptó privacidad
        var aceptoPrivacidad = await _avisoService.UsuarioHaAceptadoVigenteAsync(usuarioId);
        
        var registro = new RegistroAuditoria
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuarioId,
            Entidad = entidad,
            Operacion = operacion,
            Detalles = detalles,
            PrivacidadAceptada = aceptoPrivacidad,
            TimestampUtc = DateTime.UtcNow
        };

        _context.RegistrosAuditoria.Add(registro);
        await _context.SaveChangesAsync();
    }
}
```

---

## Escenario 6: Migración de Usuarios Existentes

**Problema:** ¿Qué hacer con usuarios creados antes de implementar privacidad?

**Opción 1: Fuerza total (recomendado para cumplimiento)**
```csharp
// En migración de datos
// Todos deben aceptar antes de operar
// (Middleware bloquea hasta aceptación)
```

**Opción 2: Grace period**
```csharp
// AceptacionAvisoPrivacidad.cs
public DateTime? GracePeriodHastaUtc { get; set; }

// En middleware
var usuarioAntesDePrivacidad = /* check si usuario creado antes de 2026-01-19 */;
if (usuarioAntesDePrivacidad && DateTime.UtcNow < gracePeriodFinal)
    return; // Permitir sin aceptación por 30 días

// Luego: aplicar fuerza total
```

**Opción 3: Validación por rol**
```csharp
// Solo Administrativo/Admin deben aceptar inmediatamente
// Consulta y Alumnos tienen 30 días
```

---

## Integración en Program.cs

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// ... otras dependencias

// Agregar servicios de privacidad
builder.Services.AddScoped<IAvisoPrivacidadService, AvisoPrivacidadService>();

// ... DbContext, etc.

var app = builder.Build();

// ... middleware de autenticación

// Middleware de cumplimiento de privacidad
app.UsePrivacidadCompliance();

// ... rest de middlewares y routes
app.MapControllers();
app.Run();
```

---

## Testing Integrado

### Unit Test: Validar en Servicio
```csharp
[Fact]
public async Task ReinscripcionService_ShouldThrowException_WhenUserNoAceptoPrivacidad()
{
    // Arrange
    var usuarioId = Guid.NewGuid();
    var alumnoId = Guid.NewGuid();
    var grupoId = Guid.NewGuid();
    
    // Mock: usuario NO aceptó privacidad
    _avisoServiceMock
        .Setup(x => x.UsuarioHaAceptadoVigenteAsync(usuarioId))
        .ReturnsAsync(false);

    // Act & Assert
    var ex = await Assert.ThrowsAsync<BusinessException>(
        () => _service.ReinscribirAlumnoAsync(alumnoId, grupoId, usuarioId));
    
    Assert.Equal("PRIVACIDAD_NO_ACEPTADA", ex.Code);
}
```

### Integration Test: Middleware Bloquea
```csharp
[Fact]
public async Task PrivacidadMiddleware_ShouldReturn403_WhenUserNotAceptedPrivacidad()
{
    // Arrange
    var cliente = _webApplicationFactory.CreateClient();
    var token = GenerarJwtToken(usuarioId: Guid.NewGuid());

    // Act
    var response = await cliente.GetAsync("/api/v1/Ciclos",
        new HttpRequestMessage { Headers = { Authorization = new("Bearer", token) } });

    // Assert
    Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    var body = await response.Content.ReadAsAsync<dynamic>();
    Assert.Equal("PRIVACIDAD_PENDIENTE", body.code);
}
```

---

## Configuración para Diferentes Entornos

### Development
```json
// appsettings.Development.json
{
  "PrivacidadCompliance": {
    "Enabled": true,
    "RequiereAceptacion": true,
    "BypassRoles": ["Admin"]
  }
}
```

### Staging/Prod
```json
// appsettings.json
{
  "PrivacidadCompliance": {
    "Enabled": true,
    "RequiereAceptacion": true,
    "BypassRoles": []
  }
}
```

### Legacy System (Migración)
```json
{
  "PrivacidadCompliance": {
    "Enabled": true,
    "RequiereAceptacion": false,  // Grace period activo
    "GracePeriodDias": 30
  }
}
```

---

## Errores Comunes

| Error | Causa | Solución |
|-------|-------|----------|
| 403 PRIVACIDAD_PENDIENTE en /activo | Endpoint no exento | Agregar a `_endpointsExentos` |
| 403 en /swagger | Middleware bloquea sin auth | Agregar `/swagger` a exentos |
| Índice UNIQUE falla en SQLite | Índice parcial no soportado | Usar índice simple (ver migración) |
| Usuario no puede hacer nada | Middleware bloqueador demasiado estricto | Aumentar endpoints exentos |
| Duplicados en aceptación | Índice no aplicado | Verificar migración aplicada |

---

## Checklist de Integración

- [ ] Agregar `IAvisoPrivacidadService` en Program.cs
- [ ] Agregar middleware con `app.UsePrivacidadCompliance()`
- [ ] Migración aplicada: `dotnet ef database update`
- [ ] Seed de aviso vigente en DataSeeder
- [ ] Tests: middleware bloquea sin aceptación
- [ ] Tests: middleware permite con aceptación
- [ ] Tests: idempotencia (aceptar 2x = 200)
- [ ] Documentación actualizada (SMOKE_PRIVACIDAD.md)
- [ ] Endpoints exentos validados (login, swagger, etc.)
- [ ] JWT token contiene UserId correctamente
- [ ] IP/UserAgent capturados en auditoría
- [ ] Manejo de "sin aviso vigente" (404)

