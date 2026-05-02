using ContactApi.Data;
using ContactApi.Helpers;
using ContactApi.Models;
using ContactApi.Repository;
using ContactApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// --- 1. Database & Identity Services ---
// Utilizing SQL Server as per your established project stack.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// --- 2. Configuration & JWT Logic ---
// Using the Options pattern for better type safety and testability.
var jwtSection = builder.Configuration.GetSection("Jwt");
builder.Services.Configure<JwtSettings>(jwtSection);
var jwtSettings = jwtSection.Get<JwtSettings>() ?? throw new InvalidOperationException("JWT settings are not configured.");

var key = Encoding.UTF8.GetBytes(jwtSettings.Key ?? throw new InvalidOperationException("JWT Key missing."));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = true;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidateAudience = true,
        ValidAudience = jwtSettings.Audience,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// --- 3. Dependency Injection (Repositories & Services) ---
builder.Services.AddScoped<IContactRepository, ContactRepository>();
builder.Services.AddScoped<IContactQueryRepository, ContactQueryRepository>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

// --- 4. HttpClient Registration (With "Trailing Slash Insurance") ---
var rawBaseUrl = builder.Configuration["CountryApi:BaseUrl"];
if (string.IsNullOrWhiteSpace(rawBaseUrl))
    throw new InvalidOperationException("Country API BaseUrl is not configured.");

// Ensure the BaseAddress always ends with a slash to prevent resolution issues.
var baseUrl = rawBaseUrl.EndsWith("/") ? rawBaseUrl : $"{rawBaseUrl}/";

builder.Services.AddHttpClient<ICountryServices, CountryServices>(client =>
{
    client.BaseAddress = new Uri(baseUrl);
    client.Timeout = TimeSpan.FromSeconds(60);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// --- 5. API Configuration ---
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Standardizing Swagger for JWT Security as used in your LMS and Food projects[cite: 1].
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Contact API", Version = "v1" });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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

// Adding CORS policy (Critical for modern web frontends)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

// --- 6. Initialization & Seeding ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await SeedDataHelper.SeedRolesAndUsersAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred during database seeding.");
    }
}

// --- 7. Middleware Pipeline ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll"); // Apply CORS before Auth

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();