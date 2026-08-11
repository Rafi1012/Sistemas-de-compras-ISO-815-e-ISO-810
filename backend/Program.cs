using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using SistemaDeCompras.Data;
using SistemaDeCompras.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

const string AngularCorsPolicy = "AngularCorsPolicy";

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.Configure<ContabilidadOptions>(
    builder.Configuration.GetSection(ContabilidadOptions.SectionName));

builder.Services.AddHttpClient<IContabilidadClient, ContabilidadClient>((sp, client) =>
{
    var options = builder.Configuration.GetSection(ContabilidadOptions.SectionName).Get<ContabilidadOptions>()
        ?? new ContabilidadOptions();
    if (!string.IsNullOrWhiteSpace(options.BaseUrl))
    {
        client.BaseAddress = new Uri(options.BaseUrl);
    }
    client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds > 0 ? options.TimeoutSeconds : 15);
});

builder.Services.AddScoped<IOrdenCompraService, OrdenCompraService>();

builder.Services.AddSingleton<BackgroundTaskQueue>();
builder.Services.AddSingleton<IBackgroundTaskQueue>(sp => sp.GetRequiredService<BackgroundTaskQueue>());
builder.Services.AddHostedService<QueuedHostedService>();

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
    options.AddPolicy(AngularCorsPolicy, policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
// Agregar pol�tica de CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        // En desarrollo permite localhost, en producci�n permite la URL que le des en AWS
        var frontendUrl = builder.Configuration["FrontendUrl"] ?? "http://localhost:5173";

        policy.WithOrigins(frontendUrl)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();



using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.UseCors(AngularCorsPolicy);

app.UseCors("AllowFrontend");

app.UseAuthorization();

app.MapControllers();

var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
app.Run($"http://0.0.0.0:{port}");

app.Run();
