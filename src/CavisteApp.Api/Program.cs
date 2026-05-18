using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using CavisteApp.Api.Configuration;
using CavisteApp.Api.Data;
using CavisteApp.Api.Entities;
using CavisteApp.Api.ExternalApi;
using CavisteApp.Api.Middleware;
using CavisteApp.Api.Services.Auth;
using CavisteApp.Api.Services.Email;
using CavisteApp.Api.Services.QuestPDF;
using CavisteApp.Api.Services.Stock;
using CavisteApp.Api.Services.WineApi;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Configuration QuestPDF License
QuestPDF.Settings.License = LicenseType.Community;

// EF Core MySQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("ConnectionString 'DefaultConnection' manquante.");

builder.Services.AddDbContext<CavisteDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// ASP.NET Core Identity 
builder.Services.AddIdentityCore<ApplicationUser>(options =>
{
    // Politique mot de passe
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;

    // Fonctionnalités avancées non utilisées
    options.SignIn.RequireConfirmedEmail = false;
    options.User.RequireUniqueEmail = false;
    options.Lockout.AllowedForNewUsers = false;
})
    .AddRoles<IdentityRole<int>>()
    .AddRoleManager<RoleManager<IdentityRole<int>>>()
    .AddEntityFrameworkStores<CavisteDbContext>();

// Services metier custom
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<AuthSeeder>();

// JWT (validation des tokens entrants)
var jwtSettings = builder.Configuration.GetSection("Jwt");
var jwtKey = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]
    ?? throw new InvalidOperationException("Configuration Jwt:SecretKey manquante."));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(jwtKey),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("ClientWpf", policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

// Controllers
builder.Services.AddControllers()
    // Configurer les options JSON pour sérialiser les enums en chaînes de caractères
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Caviste API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Saisir : Bearer {token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// Services Mailtrap
builder.Services.Configure<MailtrapOptions>(builder.Configuration.GetSection("Mailtrap"));
builder.Services.AddScoped<IEmailService, MailtrapEmailService>();
builder.Services.AddScoped<AlerteStockService>();

// Services QuestPDF
builder.Services.AddScoped<QuestPdfService>();

// Client HTTP pour l'API externe de vins
builder.Services.AddHttpClient<IWineApiClient, WineApiClient>(c =>
{
    c.BaseAddress = new Uri("https://api.wineapi.io/");
    c.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/json"));
    c.DefaultRequestHeaders.TryAddWithoutValidation(
        "X-API-Key", builder.Configuration["WineApi:Key"]);
    c.Timeout = TimeSpan.FromSeconds(15);
});

builder.Services.AddScoped<WineImportService>();

var app = builder.Build();

// Pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<GestionExceptionsMiddleware>();
app.UseCors("ClientWpf");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Migrations + seed au démarrage
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CavisteDbContext>();
    await db.Database.MigrateAsync();

    var seeder = scope.ServiceProvider.GetRequiredService<AuthSeeder>();
    await seeder.SeedAsync();
}

app.Run();
