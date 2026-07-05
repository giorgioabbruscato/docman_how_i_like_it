using System.Threading.RateLimiting;
using HrPortal.Api.Infrastructure.Filters;
using HrPortal.Api.Infrastructure.Middleware;
using HrPortal.Api.Infrastructure.OpenApi;
using System.Reflection;
using HrPortal.Api.Infrastructure.Persistence;
using HrPortal.AccessControl;
using HrPortal.Attendance;
using HrPortal.Audit;
using HrPortal.Authorization;
using HrPortal.Configuration;
using HrPortal.Departments;
using HrPortal.Documents;
using HrPortal.Employees;
using HrPortal.Identity;
using HrPortal.Leave;
using HrPortal.Notifications;
using HrPortal.SharedKernel.Persistence;
using HrPortal.Storage;
using HrPortal.Tenancy;
using HrPortal.Tenancy.Infrastructure;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
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
        Description = "Modular monolith HR platform API. Business endpoints require Bearer JWT. Tenant header (X-Tenant-Id) is required in multi-tenant mode and optional in single-tenant mode."
    });

    var xmlPath = Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);

    options.OperationFilter<TenantHeaderOperationFilter>();
    options.OperationFilter<AuthResponsesOperationFilter>();
    options.OperationFilter<ExamplesOperationFilter>();

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

if (builder.Environment.IsProduction() && string.IsNullOrWhiteSpace(databaseOptions.ConnectionString))
{
    throw new InvalidOperationException(
        "Database:ConnectionString must be configured via environment variables in Production.");
}

builder.Services.AddDbContext<HrPortalDbContext>(options =>
    options.UseNpgsql(databaseOptions.ConnectionString));

builder.Services.AddScoped<DbContext>(sp => sp.GetRequiredService<HrPortalDbContext>());
builder.Services.AddScoped<IUnitOfWork, EfUnitOfWork>();

builder.Services.AddTenancy(builder.Configuration);
builder.Services.AddHrPortalIdentity(builder.Configuration);
builder.Services.AddHrPortalAccessControl();
builder.Services.AddHrPortalAuthorization();
builder.Services.AddHrPortalStorage(builder.Configuration);
builder.Services.AddHrPortalNotifications();
builder.Services.AddHrPortalAudit();
builder.Services.AddDepartmentsModule();
builder.Services.AddEmployeesModule();
builder.Services.AddLeaveModule();
builder.Services.AddAttendanceModule();
builder.Services.AddDocumentsModule();

var corsOptions = builder.Configuration
    .GetSection(CorsOptions.SectionName)
    .Get<CorsOptions>() ?? new CorsOptions();

if (builder.Environment.IsProduction() && corsOptions.AllowedOrigins.Length == 0)
{
    throw new InvalidOperationException(
        "Cors:AllowedOrigins must be configured in Production.");
}

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        if (corsOptions.AllowedOrigins.Length > 0)
            policy.WithOrigins(corsOptions.AllowedOrigins);
        else if (!builder.Environment.IsProduction())
            policy.AllowAnyOrigin();

        policy.AllowAnyHeader()
            .AllowAnyMethod();
    });
});

if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddRateLimiter(options =>
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        options.OnRejected = async (context, cancellationToken) =>
        {
            context.HttpContext.Response.ContentType = "application/problem+json";
            await context.HttpContext.Response.WriteAsJsonAsync(new
            {
                title = "Too many requests",
                detail = "Rate limit exceeded. Try again later.",
                status = StatusCodes.Status429TooManyRequests
            }, cancellationToken);
        };

        options.AddFixedWindowLimiter("api", limiterOptions =>
        {
            limiterOptions.Window = TimeSpan.FromMinutes(1);
            limiterOptions.PermitLimit = 100;
            limiterOptions.QueueLimit = 0;
        });
    });
}

var healthChecksBuilder = builder.Services.AddHealthChecks();

if (!builder.Environment.IsEnvironment("Testing"))
{
    healthChecksBuilder.AddNpgSql(databaseOptions.ConnectionString, name: "postgresql", tags: ["ready"]);
}

var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();

if (!app.Environment.IsEnvironment("Testing"))
{
    app.UseForwardedHeaders(new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
    });
}

if (app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
    app.UseHsts();
}

app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();

if (!app.Environment.IsEnvironment("Testing"))
    app.UseRateLimiter();

app.UseAuthentication();
app.UseMiddleware<TenantResolverMiddleware>();
app.UseAuthorization();

if (app.Environment.IsEnvironment("Testing"))
    app.MapControllers();
else
    app.MapControllers().RequireRateLimiting("api");

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
