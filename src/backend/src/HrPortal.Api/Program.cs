using HrPortal.Api.Infrastructure.Filters;
using HrPortal.Api.Infrastructure.Middleware;
using HrPortal.Api.Infrastructure.Persistence;
using HrPortal.Audit;
using HrPortal.Authorization;
using HrPortal.Configuration;
using HrPortal.Departments;
using HrPortal.Employees;
using HrPortal.Identity;
using HrPortal.Notifications;
using HrPortal.SharedKernel.Persistence;
using HrPortal.Storage;
using HrPortal.Tenancy;
using HrPortal.Tenancy.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) =>
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console());

builder.Services.AddControllers(options =>
    options.Filters.Add<ValidationFilter>());

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "HR Portal API",
        Version = "v1",
        Description = "Modular monolith HR platform API"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
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

var databaseOptions = builder.Configuration
    .GetSection(DatabaseOptions.SectionName)
    .Get<DatabaseOptions>() ?? new DatabaseOptions();

builder.Services.AddDbContext<HrPortalDbContext>(options =>
    options.UseNpgsql(databaseOptions.ConnectionString));

builder.Services.AddScoped<DbContext>(sp => sp.GetRequiredService<HrPortalDbContext>());
builder.Services.AddScoped<IUnitOfWork, EfUnitOfWork>();

builder.Services.AddTenancy(builder.Configuration);
builder.Services.AddHrPortalIdentity(builder.Configuration);
builder.Services.AddHrPortalAuthorization();
builder.Services.AddHrPortalStorage(builder.Configuration);
builder.Services.AddHrPortalNotifications();
builder.Services.AddHrPortalAudit();
builder.Services.AddDepartmentsModule();
builder.Services.AddEmployeesModule();

var corsOptions = builder.Configuration
    .GetSection(CorsOptions.SectionName)
    .Get<CorsOptions>() ?? new CorsOptions();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        if (corsOptions.AllowedOrigins.Length > 0)
            policy.WithOrigins(corsOptions.AllowedOrigins);
        else
            policy.AllowAnyOrigin();

        policy.AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddHealthChecks()
    .AddNpgSql(databaseOptions.ConnectionString, name: "postgresql", tags: ["ready"]);

var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseAuthentication();
app.UseMiddleware<TenantResolverMiddleware>();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");
app.MapHealthChecks("/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});

using (var scope = app.Services.CreateScope())
{
    await DbInitializer.InitializeAsync(scope.ServiceProvider);
}

app.Run();

public partial class Program;
