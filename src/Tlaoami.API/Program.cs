using Tlaoami.Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<TlaoamiDbContext>(options =>
    options.UseSqlite("Data Source=tlaoami.db"));

var app = builder.Build();

// Seed the database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<TlaoamiDbContext>();
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
        </head>
        <body>
            <h1>Alumnos</h1>
            <pre id="json-output"></pre>
            <script>
                fetch('/api/alumnos')
                    .then(response => response.json())
                    .then(data => {
                        document.getElementById('json-output').textContent = JSON.stringify(data, null, 2);
                    });
            </script>
        </body>
        </html>
    """, "text/html");
});


var port = Environment.GetEnvironmentVariable("PORT") ?? "3000";
var url = $"http://0.0.0.0:{port}";
app.Run(url);
