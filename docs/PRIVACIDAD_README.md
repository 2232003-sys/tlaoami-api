# Cumplimiento: Aviso de Privacidad

## Overview

Módulo de cumplimiento normativo que implementa aceptación de Aviso de Privacidad con auditoría y middleware de cumplimiento.

**Requisitos normalizados:**
- GDPR, CCPA, LGPD, normativas locales de protección de datos
- Prueba de consentimiento (aceptación con timestamp, IP, User-Agent)
- Bloqueo de operaciones sin aceptación
- Historial de aceptación

---

## Arquitectura

### Entidades

#### `AvisoPrivacidad`
```csharp
public class AvisoPrivacidad
{
    public Guid Id { get; set; }                        // UUID
    public string Version { get; set; }                 // "2026-01-19", "v1.0", etc
    public string Contenido { get; set; }              // Texto/HTML del aviso
    public bool Vigente { get; set; }                 // Solo uno = true
    public DateTime PublicadoEnUtc { get; set; }      // Cuándo entró en vigencia
    public DateTime CreatedAtUtc { get; set; }        // Cuándo se creó
    
    public ICollection<AceptacionAvisoPrivacidad> Aceptaciones { get; set; }
}
```

**Índices:**
- `Vigente` (UNIQUE, parcial): Garantiza solo un aviso activo
- Composite FK a `AceptacionesAvisoPrivacidad`

#### `AceptacionAvisoPrivacidad`
```csharp
public class AceptacionAvisoPrivacidad
{
    public Guid Id { get; set; }                        // UUID
    public Guid AvisoPrivacidadId { get; set; }        // FK
    public Guid UsuarioId { get; set; }                // FK
    public DateTime AceptadoEnUtc { get; set; }        // Timestamp UTC
    public string? Ip { get; set; }                    // 192.168.1.1
    public string? UserAgent { get; set; }             // Mozilla/5.0...
    
    public AvisoPrivacidad? AvisoPrivacidad { get; set; }
    public User? Usuario { get; set; }
}
```

**Índices:**
- `(UsuarioId, AvisoPrivacidadId)` (UNIQUE): Idempotencia (un usuario no acepta dos veces el mismo aviso)

---

### Servicios

#### `IAvisoPrivacidadService`

```csharp
Task<AvisoPrivacidadDto> GetAvisoVigenteAsync()
```
- Obtiene el aviso actualmente vigente
- Lanzas `NotFoundException` si no existe
- Endpoint público (sin auth)

```csharp
Task<EstadoAceptacionDto> GetEstadoAceptacionAsync(Guid usuarioId)
```
- Retorna si usuario debe aceptar y cuándo lo hizo
- Requiere JWT
- Retorna: `{ requiereAceptacion, versionActual, aceptadoEnUtc? }`

```csharp
Task<EstadoAceptacionDto> AceptarAvisoAsync(Guid usuarioId, string? ip, string? userAgent)
```
- Registra aceptación del usuario del aviso vigente
- **Idempotente**: Si ya existe, retorna 200 (no duplica)
- Captura IP y User-Agent para auditoría
- Requiere JWT

```csharp
Task<bool> UsuarioHaAceptadoVigenteAsync(Guid usuarioId)
```
- Verifica rápidamente si usuario aceptó el vigente
- Usado por middleware de cumplimiento

```csharp
Task<AvisoPrivacidadDto> PublicarAvisoAsync(AvisoPrivacidadCreateDto dto)
```
- Crea nuevo aviso y lo marca como vigente
- Desactiva el anterior (Vigente = false)
- Admin only

---

### Controlador: `AvisoPrivacidadController`

| Endpoint | Método | Auth | Descripción |
|----------|--------|------|-------------|
| `/api/v1/AvisoPrivacidad/activo` | GET | ✗ | Obtiene aviso vigente (público) |
| `/api/v1/AvisoPrivacidad/estado` | GET | ✓ | Estado de aceptación del usuario |
| `/api/v1/AvisoPrivacidad/aceptar` | POST | ✓ | Acepta aviso vigente (idempotente) |

**Errores:**
- `404 AVISO_NO_VIGENTE`: No hay aviso vigente
- `401`: JWT inválido o expirado
- `403 PRIVACIDAD_PENDIENTE`: Middleware bloquea acceso sin aceptación

---

### Middleware: `PrivacidadComplianceMiddleware`

**Lógica:**
1. Si endpoint exento → permitir (no validar)
2. Si no autenticado → permitir (otros middlewares lo rechazan)
3. Si autenticado y usuario no aceptó vigente → `403 PRIVACIDAD_PENDIENTE`
4. Si autenticado y aceptó → permitir acceso

**Endpoints exentos:**
- `/api/v1/avisoprivacidad/*` (endpoints de privacidad)
- `/api/v1/auth/login`
- `/swagger`, `/healthz`

**Respuesta de bloqueo:**
```json
{
  "code": "PRIVACIDAD_PENDIENTE",
  "message": "Debe aceptar el aviso de privacidad vigente para acceder a este recurso."
}
```

---

## Flujo de Uso (Happy Path)

1. **Usuario no autenticado** accede `/activo` → obtiene aviso vigente
2. **Usuario se autentica** → POST `/auth/login` → recibe JWT
3. **Usuario consulta estado** → GET `/estado` → `requiereAceptacion: true`
4. **Intenta acceder recurso protegido** → middleware bloquea con `403 PRIVACIDAD_PENDIENTE`
5. **Usuario acepta** → POST `/aceptar` → registro creado, `requiereAceptacion: false`
6. **Intenta nuevamente** → middleware permite acceso → `200 OK`
7. **Consulta estado nuevamente** → GET `/estado` → `aceptadoEnUtc: "2026-01-19T..."`

---

## Configuración EF Core

### DbContext Setup

```csharp
public class TlaoamiDbContext : DbContext
{
    public DbSet<AvisoPrivacidad> AvisosPrivacidad { get; set; }
    public DbSet<AceptacionAvisoPrivacidad> AceptacionesAvisoPrivacidad { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // AvisoPrivacidad
        modelBuilder.Entity<AvisoPrivacidad>()
            .HasIndex(a => a.Vigente)
            .IsUnique(); // En Postgres: with filter "Vigente = true"

        // AceptacionAvisoPrivacidad
        modelBuilder.Entity<AceptacionAvisoPrivacidad>()
            .HasIndex(aa => new { aa.UsuarioId, aa.AvisoPrivacidadId })
            .IsUnique(); // Idempotencia
    }
}
```

### Inyección de Dependencias

```csharp
// Program.cs
builder.Services.AddScoped<IAvisoPrivacidadService, AvisoPrivacidadService>();

// Middleware
app.UsePrivacidadCompliance();
```

---

## Migración EF Core

```bash
dotnet ef migrations add AddAvisoPrivacidad \
  --project src/Tlaoami.Infrastructure \
  --startup-project src/Tlaoami.API

dotnet ef database update \
  --project src/Tlaoami.Infrastructure \
  --startup-project src/Tlaoami.API
```

**Genera:**
- Tabla `AvisosPrivacidad`
- Tabla `AceptacionesAvisoPrivacidad`
- Índices (Vigente UNIQUE, UsuarioId+AvisoId UNIQUE)
- Foreign keys a `Users`

---

## Data Seeding (Desarrollo)

`DataSeeder.cs` crea un aviso vigente por defecto:

```csharp
var avisoPrivacidad = new AvisoPrivacidad
{
    Id = Guid.NewGuid(),
    Version = "2026-01-19",
    Contenido = "AVISO DE PRIVACIDAD\n\nEn Tlaoami, protegemos...",
    Vigente = true,
    PublicadoEnUtc = DateTime.UtcNow,
    CreatedAtUtc = DateTime.UtcNow
};

context.AvisosPrivacidad.Add(avisoPrivacidad);
```

---

## Casos de Uso

### 1. Publicar Nuevo Aviso (Admin)

```csharp
var dto = new AvisoPrivacidadCreateDto
{
    Version = "2026-02-01",
    Contenido = "Nuevo aviso con cambios..."
};

var nuevoAviso = await _avisoService.PublicarAvisoAsync(dto);
// Resultado: Vigente=true, aviso anterior Vigente=false
```

### 2. Validar Aceptación en Servicio Externo

```csharp
// Ej: Antes de procesar factura
bool aceptoPrivacidad = await _avisoService.UsuarioHaAceptadoVigenteAsync(usuarioId);
if (!aceptoPrivacidad)
    throw new BusinessException("Usuario debe aceptar aviso de privacidad", "PRIVACIDAD_NO_ACEPTADA");
```

### 3. Auditoría: Consultar Aceptaciones de Usuario

```sql
SELECT 
    ap.Version,
    aap.AceptadoEnUtc,
    aap.Ip,
    aap.UserAgent
FROM AceptacionesAvisoPrivacidad aap
JOIN AvisosPrivacidad ap ON aap.AvisoPrivacidadId = ap.Id
WHERE aap.UsuarioId = @usuarioId
ORDER BY aap.AceptadoEnUtc DESC;
```

---

## Seguridad & Cumplimiento

| Requisito | Implementación |
|-----------|-----------------|
| **Consentimiento probado** | Timestamp + IP + UserAgent en BD |
| **No duplicar aceptación** | Índice UNIQUE (UsuarioId, AvisoId) |
| **Solo 1 vigente** | Índice UNIQUE en Vigente |
| **Bloqueo sin aceptación** | Middleware 403 |
| **UTC timestamps** | `DateTime.UtcNow` en todos los campos |
| **JWT requerido** | `[Authorize]` en /estado, /aceptar |
| **Auditoría** | IP + UserAgent capturados |
| **Idempotencia** | POST /aceptar dos veces = 200 OK |

---

## Postgres vs SQLite

### Índice Parcial (Postgres)

En Postgres, el índice UNIQUE parcial optimiza:
```sql
CREATE UNIQUE INDEX idx_aviso_vigente 
ON AvisosPrivacidad(Vigente) 
WHERE Vigente = true;
```
Solo indexa filas con `Vigente=true`, permitiendo múltiples `false`.

### SQLite

SQLite no soporta índices parciales (sin WHERE). Usamos índice simple:
```sql
CREATE UNIQUE INDEX idx_aviso_vigente ON AvisosPrivacidad(Vigente);
```
**Limitación:** Rechaza dos `Vigente=false` (pero lógica de servicio lo previene).

---

## DTOs

### `AvisoPrivacidadDto` (Response)
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "version": "2026-01-19",
  "contenido": "AVISO DE PRIVACIDAD...",
  "publicadoEnUtc": "2026-01-19T10:30:00Z"
}
```

### `EstadoAceptacionDto` (Response)
```json
{
  "requiereAceptacion": false,
  "versionActual": "2026-01-19",
  "aceptadoEnUtc": "2026-01-19T11:45:30Z"
}
```

### `AvisoPrivacidadCreateDto` (Request)
```json
{
  "version": "2026-02-01",
  "contenido": "Nuevo contenido del aviso..."
}
```

### `AceptarAvisoDto` (Request)
```json
{}
```
(Body vacío, parámetros se extraen del JWT)

---

## Testing

Ver [SMOKE_PRIVACIDAD.md](./SMOKE_PRIVACIDAD.md) para pasos de validación completos.

**Tests recomendados:**
- Unit: `AvisoPrivacidadService` (idempotencia, sin vigente, múltiples usuarios)
- Integration: Middleware bloquea sin aceptación, permite con aceptación
- E2E: Flujo completo (login → estado → aceptar → acceso)

---

## Errores Manejados

| Error | HTTP | Código | Descripción |
|-------|------|--------|-------------|
| Aviso no vigente | 404 | `AVISO_NO_VIGENTE` | No hay aviso activo |
| JWT inválido | 401 | (JWT) | Token expirado o invalido |
| Usuario no aceptó | 403 | `PRIVACIDAD_PENDIENTE` | Middleware bloquea |
| Validación falla | 400 | `AVISO_INVALIDO` | Version o Contenido vacíos |

---

## Próximos Pasos Opcionales

1. **Notificaciones:** Email cuando hay nuevo aviso vigente
2. **Analytics:** Dashboard de % de usuarios que aceptaron
3. **Versiones previas:** Guardar historial de avisos anteriores
4. **Políticas personalizadas:** Avisos diferentes por rol/ciclo
5. **Consentimiento granular:** Aceptar/rechazar consentimiento según tipo (marketing, técnico, etc.)
6. **Integración con 3rd-party:** OneTrust, Privin, etc.

