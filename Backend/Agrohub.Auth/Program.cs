using Agrohub.Auth.Behaviors;
using Agrohub.Auth.Data.Interceptors;
using Agrohub.Auth.Interfaces;
using Agrohub.Auth.Implementations;
using Carter;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// --- DB + interceptors
var connectionString = builder.Configuration.GetConnectionString("Database");
builder.Services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
builder.Services.AddDbContext<IdentityDbContext>((sp, options) =>
{
    options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
    options.UseNpgsql(connectionString);
});

// --- Carter + MediatR + FluentValidation
builder.Services.AddCarter();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

// --- Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- CORS dla Angular SPA (credentials wymagane do ciastek refresh)
builder.Services.AddCors(o => o.AddPolicy("spa", p =>
    p.WithOrigins("http://localhost:4200")   // + ewentualnie produkcyjny origin
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()
));

// --- JWT AuthN/AuthZ
var jwt = builder.Configuration.GetSection("Jwt");
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opts =>
    {
        opts.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwt["Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("admin", p => p.RequireRole("admin"));
});

// --- Twoje serwisy do tokenów/haszowania/czasu (podmień na swoje klasy)
builder.Services.AddScoped<IPasswordHasher, AspNetPasswordHasherAdapter>();
builder.Services.AddScoped<ITokenService, JwtTokenService>();
builder.Services.AddSingleton<IClock, SystemClock>();

// Flaga dla cookie SameSite=None gdy SPA na innym originie
builder.Configuration["Auth:CrossSiteSpa"] ??= "true";

var app = builder.Build();

// --- Middleware kolejność
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseCors("spa");              // ⬅️ przed auth, bo preflight potrafi się wyłożyć
app.UseAuthentication();
app.UseAuthorization();

app.MapCarter();

app.Run();
