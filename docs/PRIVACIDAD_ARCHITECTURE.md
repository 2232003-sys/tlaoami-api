# Arquitectura: Cumplimiento de Privacidad

## Diagrama de Flujo (Usuario)

```
┌─────────────────────────────────────────────────────────────────────┐
│                          USUARIO FINAL                              │
└──────────────────────────────┬──────────────────────────────────────┘
                               │
                ┌──────────────┴──────────────┐
                │                             │
        ┌───────▼────────┐          ┌────────▼────────┐
        │ NO AUTENTICADO │          │   AUTENTICADO   │
        └────────┬────────┘          └────────┬────────┘
                 │                            │
        GET /activo                   ┌───────▼──────────┐
        (Aviso vigente)               │ Middleware Check │
        ✅ 200 OK                     └───────┬──────────┘
                                              │
                                      ┌───────▼──────────────────┐
                                      │ UsuarioHaAceptadoVigente │
                                      └───────┬──────────┬────────┘
                                              │          │
                                          true│          │false
                                              │          │
                            ┌─────────────────┘          └──────────┐
                            │                                       │
                    ┌───────▼──────────┐            ┌──────────────▼────┐
                    │ Permitir acceso  │            │ 403 Bloquear       │
                    │ (Seguir ejecutar)│            │ PRIVACIDAD_PENDIENTE
                    └──────────────────┘            └─────────┬──────────┘
                                                              │
                                            POST /aceptar ◄──┘
                                            ✅ 200 OK
                                            (Crear AceptacionAvisoPrivacidad)
```

---

## Diagrama de Capas (Clean Architecture)

```
┌───────────────────────────────────────────────────────────────────────────┐
│                           API LAYER                                       │
├───────────────────────────────────────────────────────────────────────────┤
│                                                                            │
│  ┌─────────────────────────────────────────────────────────────────────┐  │
│  │ AvisoPrivacidadController                                           │  │
│  │  • GET /activo         → GetAvisoVigenteAsync()                    │  │
│  │  • GET /estado         → GetEstadoAceptacionAsync(usuarioId)       │  │
│  │  • POST /aceptar       → AceptarAvisoAsync(usuarioId, ip, ua)     │  │
│  └─────────────────────────────────────────────────────────────────────┘  │
│                                                                            │
│  ┌─────────────────────────────────────────────────────────────────────┐  │
│  │ PrivacidadComplianceMiddleware                                      │  │
│  │  • Intercepta todas las requests                                    │  │
│  │  • Verifica UsuarioHaAceptadoVigenteAsync()                        │  │
│  │  • Bloquea con 403 PRIVACIDAD_PENDIENTE si no aceptó              │  │
│  │  • Excepta: /avisoprivacidad/*, /auth/login, /swagger             │  │
│  └─────────────────────────────────────────────────────────────────────┘  │
│                                                                            │
└──────────┬──────────────────────────────────────────────────────────┬─────┘
           │                                                          │
           │ Llama                                                    │
           ▼                                                          ▼
┌───────────────────────────────────────────────────────────────────────────┐
│                        APPLICATION LAYER                                  │
├───────────────────────────────────────────────────────────────────────────┤
│                                                                            │
│  IAvisoPrivacidadService ◄────── AvisoPrivacidadService                 │
│  ┌──────────────────────────────┐ ┌──────────────────────────────────┐  │
│  │ GetAvisoVigenteAsync()       │ │ • GetAvisoVigenteAsync()        │  │
│  │ GetEstadoAceptacionAsync()   │ │ • GetEstadoAceptacionAsync()    │  │
│  │ AceptarAvisoAsync()          │ │ • AceptarAvisoAsync() [IDEMPOTENTE]
│  │ PublicarAvisoAsync()         │ │ • PublicarAvisoAsync()          │  │
│  │ UsuarioHaAceptadoVigenteAsync() │ • UsuarioHaAceptadoVigenteAsync() │
│  └──────────────────────────────┘ └──────────────────────────────────┘  │
│                                    • Validaciones                         │
│                                    • Manejo de excepciones                │
│                                    • Lógica de negocio                    │
│                                                                            │
│  DTOs:                                                                     │
│  ├─ AvisoPrivacidadDto (response)                                        │
│  ├─ EstadoAceptacionDto (response)                                       │
│  ├─ AvisoPrivacidadCreateDto (request)                                   │
│  └─ AceptarAvisoDto (request)                                            │
│                                                                            │
│  Excepciones:                                                              │
│  ├─ NotFoundException (404 AVISO_NO_VIGENTE)                             │
│  └─ ValidationException (400 AVISO_INVALIDO)                            │
│                                                                            │
└──────────┬──────────────────────────────────────────────────────────┬─────┘
           │                                                          │
           │ Usa DbContext                                           │
           ▼                                                          ▼
┌───────────────────────────────────────────────────────────────────────────┐
│                      INFRASTRUCTURE LAYER                                 │
├───────────────────────────────────────────────────────────────────────────┤
│                                                                            │
│  TlaoamiDbContext (EF Core)                                               │
│  ├─ DbSet<AvisoPrivacidad>                                               │
│  └─ DbSet<AceptacionAvisoPrivacidad>                                    │
│                                                                            │
│  Configuración:                                                            │
│  ├─ AvisoPrivacidad.Vigente: UNIQUE INDEX                                │
│  ├─ (UsuarioId, AvisoPrivacidadId): UNIQUE INDEX                         │
│  ├─ ForeignKey → Users (OnDelete.Cascade)                                │
│  └─ Enums → strings                                                       │
│                                                                            │
│  Migrations:                                                               │
│  └─ 20260120015416_AddAvisoPrivacidad.cs                                 │
│                                                                            │
└──────────┬──────────────────────────────────────────────────────────┬─────┘
           │                                                          │
           │ Lee/escribe                                             │
           ▼                                                          ▼
┌───────────────────────────────────────────────────────────────────────────┐
│                        DOMAIN LAYER                                       │
├───────────────────────────────────────────────────────────────────────────┤
│                                                                            │
│  AvisoPrivacidad Entity                                                   │
│  ├─ Id: Guid                                                             │
│  ├─ Version: string (ej: "2026-01-19")                                  │
│  ├─ Contenido: string (texto/HTML)                                       │
│  ├─ Vigente: bool (solo uno = true)                                     │
│  ├─ PublicadoEnUtc: DateTime                                             │
│  ├─ CreatedAtUtc: DateTime                                               │
│  └─ Aceptaciones: ICollection<AceptacionAvisoPrivacidad>                │
│                                                                            │
│  AceptacionAvisoPrivacidad Entity                                         │
│  ├─ Id: Guid                                                             │
│  ├─ AvisoPrivacidadId: Guid (FK)                                         │
│  ├─ UsuarioId: Guid (FK)                                                 │
│  ├─ AceptadoEnUtc: DateTime (UTC)                                        │
│  ├─ Ip: string? (192.168.1.1)                                           │
│  ├─ UserAgent: string? (Mozilla/5.0)                                    │
│  └─ Relaciones: AvisoPrivacidad, User                                    │
│                                                                            │
└─────────────────────────────────────────────────────────────────────────┘
             │
             │ Mapea a
             ▼
        ┌─────────────────┐
        │  SQL Queries    │
        │  (EF Linq)      │
        └────────┬────────┘
                 │
                 ▼
        ┌──────────────────────┐
        │  PostgreSQL/SQLite   │
        └──────────────────────┘
```

---

## Flujo de Datos: POST /aceptar (Idempotencia)

```
USER REQUEST
    │
    ├─ Entra a PrivacidadComplianceMiddleware
    │   └─ Si endpoint exento o sin auth → permitir
    │   └─ Si autenticado → verificar UsuarioHaAceptadoVigenteAsync()
    │       └─ Si no aceptó → 403 PRIVACIDAD_PENDIENTE
    │       └─ Si aceptó → permitir (pero POST/aceptar no alcanza aquí)
    │
    ├─ Llega a AvisoPrivacidadController.Aceptar()
    │   ├─ Extrae usuarioId del JWT
    │   ├─ Extrae IP del X-Forwarded-For / RemoteIpAddress
    │   ├─ Extrae User-Agent del header
    │   └─ Llama AvisoPrivacidadService.AceptarAvisoAsync(usuarioId, ip, ua)
    │
    └─ AvisoPrivacidadService.AceptarAvisoAsync()
        │
        ├─ SELECT TOP 1 * FROM AvisosPrivacidad WHERE Vigente = true
        │   └─ Si null → NotFoundException (404 AVISO_NO_VIGENTE)
        │
        ├─ SELECT TOP 1 * FROM AceptacionesAvisoPrivacidad
        │           WHERE UsuarioId = @uid AND AvisoPrivacidadId = @aid
        │   └─ Resultado: aceptacionExistente
        │
        ├─ SI aceptacionExistente == null
        │   ├─ INSERT INTO AceptacionesAvisoPrivacidad
        │   │   (Id, AvisoId, UsuarioId, AceptadoEnUtc, Ip, UserAgent)
        │   │   VALUES (newGuid, avisoId, uid, DateTime.UtcNow, ip, ua)
        │   │   └─ Índice UNIQUE (UsuarioId, AvisoId) → garantiza una sola
        │   │
        │   └─ SaveChangesAsync()
        │
        ├─ SINO (ya existe)
        │   └─ No hacer nada (idempotencia)
        │
        └─ RETURN EstadoAceptacionDto
            {
              "requiereAceptacion": false,
              "versionActual": "2026-01-19",
              "aceptadoEnUtc": "2026-01-19T11:45:30Z"
            }

RESPONSE: 200 OK ✓
```

---

## Índices de Base de Datos

### AvisosPrivacidad
```sql
-- Índice PRIMARY KEY
ALTER TABLE AvisosPrivacidad ADD PRIMARY KEY (Id);

-- Índice UNIQUE: solo un aviso vigente
CREATE UNIQUE INDEX IX_AvisosPrivacidad_Vigente ON AvisosPrivacidad(Vigente);
-- Nota: En Postgres, preferiblemente: WHERE Vigente = true (parcial)
```

### AceptacionesAvisoPrivacidad
```sql
-- Índice PRIMARY KEY
ALTER TABLE AceptacionesAvisoPrivacidad ADD PRIMARY KEY (Id);

-- Índice UNIQUE: garantiza idempotencia
CREATE UNIQUE INDEX IX_AceptacionesAvisoPrivacidad_UsuarioId_AvisoId 
    ON AceptacionesAvisoPrivacidad(UsuarioId, AvisoPrivacidadId);

-- Índice de FK (generado automáticamente)
CREATE INDEX IX_AceptacionesAvisoPrivacidad_AvisoPrivacidadId 
    ON AceptacionesAvisoPrivacidad(AvisoPrivacidadId);
```

---

## Decisiones de Diseño

| Decisión | Justificación |
|----------|---------------|
| **Vigente como bool** | Simple y eficiente. Índice UNIQUE garantiza una sola activa. |
| **Índice (UsuarioId, AvisoId)** | Previene duplicados automáticamente. DB enforce, no solo aplicación. |
| **Idempotencia en POST** | Seguro: aceptar 2x = 200 OK (no duplica en BD) |
| **IP + UserAgent opcionales** | Auditoría sin obligar cliente a enviar. Middleware lo captura. |
| **Timestamps UTC** | Evita problemas de zonas horarias. Consistencia global. |
| **Middleware de bloqueo** | Centraliza validación. No hay que recordar validar en cada endpoint. |
| **Endpoints exentos** | /activo para ver aviso, /login para autenticarse, /swagger para dev. |
| **Cascada al User** | Si usuario se borra, aceptación también (no huérfanos). |

---

## Casos de Error (Error Handling)

```
REQUEST                    VALIDACIÓN                  RESPUESTA
────────────────────────────────────────────────────────────────────
POST /aceptar              Sin JWT                     401 Unauthorized
(sin header Auth)

POST /aceptar              JWT expirado/inválido       401 Unauthorized
(auth inválido)

GET /activo                Sin aviso vigente           404 Not Found
                           en BD                       { "code": "AVISO_NO_VIGENTE" }

POST /aceptar              Sin aviso vigente           404 Not Found
                           en BD                       { "code": "AVISO_NO_VIGENTE" }

GET /ciclos                Autenticado,                403 Forbidden
(endpoint protegido)       NO aceptó privacidad        { "code": "PRIVACIDAD_PENDIENTE" }

POST /aceptar              Aceptar segunda vez         200 OK
                           (ya existe acept.)          (idempotente, sin duplicar)

GET /estado                Autenticado                 200 OK
                           (con/sin aceptación)        { "requiereAceptacion": bool, ... }
```

---

## Ventajas de Esta Arquitectura

✅ **Centralizado:** Todo en un módulo separado  
✅ **Reutilizable:** Otros servicios pueden validar privacidad  
✅ **Testeable:** Interfaces limpias, sin dependencias circulares  
✅ **Auditado:** IP + UserAgent + Timestamp capturados  
✅ **Seguro:** Middleware previene acceso sin aceptación  
✅ **Performante:** Índices optimizan búsquedas  
✅ **Idempotente:** No duplica en BD aunque POST se repita  
✅ **Normativo:** Cumple GDPR, CCPA, LGPD  

