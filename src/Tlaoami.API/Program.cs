using Tlaoami.API.Middleware;
using Tlaoami.Application.Exceptions;
using Tlaoami.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using Tlaoami.Application.Interfaces;
using Tlaoami.Application.Interfaces.PagosOnline;
using Tlaoami.Application.Services;
using Tlaoami.Application.Services.PagosOnline;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Tlaoami.Application.Facturacion;
using Tlaoami.Application.Services.CfdiProviders;
using Tlaoami.Application.Configuration;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddScoped<IAlumnoService, AlumnoService>();
builder.Services.AddScoped<IFacturaService, FacturaService>();
builder.Services.AddScoped<IPagoService, PagoService>();
builder.Services.AddScoped<ICicloEscolarService, CicloEscolarService>();
builder.Services.AddScoped<IGrupoService, GrupoService>();
builder.Services.AddScoped<IAsignacionGrupoService, AsignacionGrupoService>();
builder.Services.AddScoped<IConciliacionBancariaService, ConciliacionBancariaService>();
builder.Services.AddScoped<ISugerenciasConciliacionService, SugerenciasConciliacionService>();
builder.Services.AddScoped<IConsultaConciliacionesService, ConsultaConciliacionesService>();
builder.Services.AddScoped<IImportacionEstadoCuentaService, ImportacionEstadoCuentaService>();
builder.Services.AddScoped<IPagosOnlineService, PagosOnlineService>();
builder.Services.AddScoped<IPagoOnlineProvider, FakePagoOnlineProvider>();
builder.Services.AddScoped<IReinscripcionService, ReinscripcionService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IConceptosCobroService, ConceptosCobroService>();
builder.Services.AddScoped<IReglasCobroService, ReglasCobroService>();
builder.Services.AddScoped<IReglaColegiaturaService, ReglaColegiaturaService>();
builder.Services.AddScoped<IBecaAlumnoService, BecaAlumnoService>();
builder.Services.AddScoped<IReglaRecargoService, ReglaRecargoService>();
builder.Services.AddScoped<IColegiaturasService, ColegiaturasService>();
builder.Services.AddScoped<IAvisoPrivacidadService, AvisoPrivacidadService>();
builder.Services.AddScoped<ISalonService, SalonService>();
builder.Services.AddScoped<IReporteService, ReporteService>();
builder.Services.AddHttpClient();
builder.Services.AddScoped<IReceptorFiscalService, ReceptorFiscalService>();
builder.Services.AddScoped<IFacturaFiscalService, FacturaFiscalService>();
builder.Services.AddScoped<ICfdiProvider, DummyCfdiProvider>();
builder.Services.Configure<EmisorFiscalOptions>(builder.Configuration.GetSection("EmisorFiscal"));

// Facturación provider selection: Dummy | Facturama
var factProv = builder.Configuration["Facturacion:Provider"] ?? "Dummy";
if (string.Equals(factProv, "Facturama", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddScoped<IFacturacionProvider, FacturamaClient>();
}
else
{
    builder.Services.AddScoped<IFacturacionProvider, DummyFacturacionProvider>();
}
builder.Services.AddScoped<Tlaoami.Application.Services.FacturacionService>();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFront", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Configure JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? "TlaoamiSecretKeyForDevelopmentOnly12345678";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "Tlaoami";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "TlaoamiUsers";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure DbContext to use the selected provider (Sqlite default, Postgres optional)
var databaseProvider = builder.Configuration.GetValue<string>("DatabaseProvider") ?? "Sqlite";
if (string.Equals(databaseProvider, "Postgres", StringComparison.OrdinalIgnoreCase))
{
    var pgConnection = builder.Configuration.GetConnectionString("PostgresConnection");
    builder.Services.AddDbContext<TlaoamiDbContext>(options => options.UseNpgsql(pgConnection));
}
else
{
    var sqliteConnection = builder.Configuration.GetConnectionString("DefaultConnection");
    builder.Services.AddDbContext<TlaoamiDbContext>(options => options.UseSqlite(sqliteConnection));
}

var app = builder.Build();

// Apply migrations and seed the database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<TlaoamiDbContext>();
    try
    {
        context.Database.Migrate(); // Apply pending migrations
        await DataSeeder.SeedAsync(context);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️ Seed error (non-blocking): {ex.Message}");
        // No throwear - permitir que la app siga
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Handle BusinessException uniformly with 409 and JSON {code,message}
app.UseMiddleware<BusinessExceptionMiddleware>();

// Always return JSON ProblemDetails for unhandled exceptions
app.UseExceptionHandler(handler =>
{
    handler.Run(async context =>
    {
        var feature = context.Features.Get<IExceptionHandlerFeature>();
        var status = context.Response.StatusCode != 200 ? context.Response.StatusCode : StatusCodes.Status500InternalServerError;
        context.Response.StatusCode = status;
        context.Response.ContentType = "application/problem+json";
        var problem = new ProblemDetails
        {
            Title = status == 500 ? "An unexpected error occurred." : "Request failed.",
            Status = status,
            Detail = feature?.Error.Message
        };
        await context.Response.WriteAsJsonAsync(problem);
    });
});

// Return JSON ProblemDetails for non-success status codes (like 404)
app.UseStatusCodePages(async context =>
{
    var response = context.HttpContext.Response;
    if (response.StatusCode >= 400)
    {
        response.ContentType = "application/problem+json";
        var problem = new ProblemDetails
        {
            Title = $"HTTP {response.StatusCode}",
            Status = response.StatusCode
        };
        await response.WriteAsJsonAsync(problem);
    }
});

app.UseHttpsRedirection();

app.UseCors("AllowFront");

app.UseAuthentication();
app.UseAuthorization();

// Middleware: Verificar cumplimiento de privacidad (aviso aceptado)
app.UsePrivacidadCompliance();

app.MapControllers();

// Health check endpoint para conexión del frontend
app.MapGet("/api/v1/health", () => Results.Ok(new { status = "connected", timestamp = DateTime.UtcNow }));

// Redirect root to frontend
app.MapGet("/", context =>
{
    context.Response.Redirect("http://localhost:3001", permanent: false);
    return Task.CompletedTask;
});

var port = Environment.GetEnvironmentVariable("PORT") ?? "3000";
var url = $"http://0.0.0.0:{port}";
app.Run(url);
