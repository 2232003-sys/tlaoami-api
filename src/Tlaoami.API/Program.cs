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
    context.Database.Migrate(); // Apply pending migrations
    await DataSeeder.SeedAsync(context);
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

app.MapControllers();

// Serve a simple HTML page to display the data
app.MapGet("/", () => {
    return Results.Content("""
        <!DOCTYPE html>
        <html>
        <head>
            <title>Tlaoami Data</title>
            <link rel="stylesheet" href="https://cdn.simplecss.org/simple.min.css">
            <style>
                body { padding: 2rem; }
                #search-container { margin-bottom: 1rem; }
                #alumnos-list { margin-top: 1rem; }
            </style>
        </head>
        <body>
            <h1>Alumnos</h1>
            
            <div id="search-container">
                <input type="text" id="alumno-id-input" placeholder="Introduce el ID del alumno" />
                <button onclick="fetchAlumnoById()">Buscar Alumno</button>
            </div>

            <h2>Alumno Específico</h2>
            <pre id="json-output-single"></pre>

            <h2 id="alumnos-list">Lista Completa de Alumnos</h2>
            <pre id="json-output-all"></pre>

            <script>
                function fetchAlumnoById() {
                    const alumnoId = document.getElementById('alumno-id-input').value;
                    if (!alumnoId) {
                        document.getElementById('json-output-single').textContent = 'Por favor, introduce un ID.';
                        return;
                    }
                    
                    document.getElementById('json-output-single').textContent = 'Cargando...';
                    fetch(`/api/alumnos/${alumnoId}`)
                        .then(response => {
                            if (!response.ok) {
                                throw new Error(`Error: ${response.status} ${response.statusText}`);
                            }
                            return response.json();
                        })
                        .then(data => {
                            document.getElementById('json-output-single').textContent = JSON.stringify(data, null, 2);
                        })
                        .catch(error => {
                            document.getElementById('json-output-single').textContent = `No se pudo encontrar el alumno. ${error.message}`;
                        });
                }

                // Fetch all alumnos on page load
                fetch('/api/alumnos')
                    .then(response => response.json())
                    .then(data => {
                        document.getElementById('json-output-all').textContent = JSON.stringify(data, null, 2);
                    });
            </script>
        </body>
        </html>
    """, "text/html");
});

var port = Environment.GetEnvironmentVariable("PORT") ?? "3000";
var url = $"http://0.0.0.0:{port}";
app.Run(url);
