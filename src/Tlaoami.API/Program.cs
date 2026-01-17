using Tlaoami.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Tlaoami.Application.Interfaces;
using Tlaoami.Application.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddScoped<IAlumnoService, AlumnoService>();
builder.Services.AddScoped<IFacturaService, FacturaService>();
builder.Services.AddScoped<IPagoService, PagoService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure DbContext to use the connection string from appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<TlaoamiDbContext>(options =>
    options.UseSqlite(connectionString));

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

app.UseHttpsRedirection();

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
