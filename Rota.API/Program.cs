using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Rota.API.Middlewares;
using Rota.Application.Interfaces;
using Rota.Application.Mappings;
using Rota.Application.Services;
using Rota.Application.Settings;
using Rota.Infra.IoC;
using CleanArchMvc.Infra.IoC;
using Rota.Infra.Data;
using DotNetEnv;
using Rota.Infrastructure.Storage;
using Rota.Domain.Interfaces;
using Rota.Infrastructure.Persistence;
using Swashbuckle.AspNetCore.Annotations;
using Rota.Application.Jobs;
using Rota.Infra.Data.Interceptors;
using System.Threading.RateLimiting;
using Rota.Infra.Data.Repositories;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;

Env.Load();

var builder = WebApplication.CreateBuilder(args);



builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.SetMinimumLevel(LogLevel.Information);
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<AuditingInterceptor>();

builder.Services.AddHttpLogging(opts =>
{
    opts.LoggingFields =
          HttpLoggingFields.RequestMethod
        | HttpLoggingFields.RequestPath
        | HttpLoggingFields.RequestHeaders
        | HttpLoggingFields.RequestBody
        | HttpLoggingFields.ResponseStatusCode
        | HttpLoggingFields.ResponseHeaders
        | HttpLoggingFields.ResponseBody;
});



builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddStorage(builder.Configuration);


var argon2Cfg = builder.Configuration.GetSection("Argon2Settings");
builder.Services.AddSingleton(new Argon2Hasher(
    argon2Cfg.GetValue<int>("DegreeOfParallelism", 8),
    argon2Cfg.GetValue<int>("MemorySize", 65536),
    argon2Cfg.GetValue<int>("Iterations", 4)));
builder.Services.AddScoped<Argon2Hasher>();


var conn = builder.Configuration.GetConnectionString("DefaultConnection")!;
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(
        conn,
        ServerVersion.AutoDetect(conn),
        b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));


builder.Services.AddDbContext<ApplicationDbContext>((sp, options) =>
{
    var interceptor = sp.GetRequiredService<AuditingInterceptor>();
    var conn = builder.Configuration.GetConnectionString("DefaultConnection")!;
    
    options.UseMySql(
        conn,
        ServerVersion.AutoDetect(conn),
        b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName))
           .AddInterceptors(interceptor); 
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });


var jwtCfg = builder.Configuration.GetSection("JwtSettings");
builder.Services.Configure<JwtSettings>(jwtCfg);

builder.Services.AddTransient<IPasswordResetService, PasswordResetService>();


builder.Services.AddHttpClient<AgencyImportService>();
builder.Services.AddScoped<AgencyInsertService>();

builder.Services.AddHttpClient<TransactionImportService>();
builder.Services.AddScoped<TransactionService>();
builder.Services.AddScoped<TransactionInsertService>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();

builder.Services.AddHostedService<MonthlyTransactionImportJob>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtCfg["Issuer"],
            ValidAudience = jwtCfg["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtCfg["Secret"]!)),
            RoleClaimType = ClaimTypes.Role
        };
    });
    
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.User.Identity?.Name ?? httpContext.Request.Headers.Host.ToString(),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1)    
            }));
    
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

builder.Services.AddAuthorization();



builder.Services.AddAutoMapper(typeof(DomainToDTOMappingProfile).Assembly);


builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddTransient<IEmailService, EmailService>();
builder.Services.AddSingleton<IEmailTemplateService, EmailTemplateService>();


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => { c.EnableAnnotations(); c.SupportNonNullableReferenceTypes(); });
builder.Services.AddInfrastructureSwagger();


builder.Services.AddControllers();

builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();
builder.Services.AddHostedService<AgencyRoutineService>();
builder.Services.AddScoped<CommissionCalculationService>();
builder.Services.AddScoped<IMonthlyCommissionRepository, MonthlyCommissionRepository>();
builder.Services.AddScoped<IMonthlyCommissionService, MonthlyCommissionService>();
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular",
        policy =>
        {
            policy.WithOrigins("http://localhost:4200") 
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials(); 
        });
});


builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>();





var app = builder.Build();

app.UseMiddleware<ErrorHandlingMiddleware>();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var seeder = services.GetRequiredService<CreateAdmin>();
        await seeder.SeedAdminUserAsync();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocorreu um erro durante o data seeding.");
    }
}

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Docker"))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsEnvironment("Docker"))
{
    app.UseHttpsRedirection();
}


app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var response = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                exception = entry.Value.Exception?.Message ?? "none",
                duration = entry.Value.Duration.ToString()
            })
        };
        await JsonSerializer.SerializeAsync(context.Response.Body, response);
    }
});

app.UseRouting();
app.UseCors("AllowAngular");

app.UseHttpLogging();

app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.MapControllers();

app.Run();