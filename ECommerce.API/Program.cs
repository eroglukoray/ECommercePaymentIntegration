
using ECommerce.API.HealthChecks;
using ECommerce.Application.Commands;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Validators;
using ECommerce.Infrastructure.Data;
using ECommerce.Infrastructure.Repositories;
using ECommerce.Infrastructure.Services;
using FluentValidation;
using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Timeout;
using System.Net;
using System.Text;
using CorrelationId;
using CorrelationId.DependencyInjection;
using Prometheus;
using Serilog;
using ECommerce.Application.Mapping;



var builder = WebApplication.CreateBuilder(args);
// Serilog konfigürasyonu
builder.Host.UseSerilog((ctx, lc) => lc
    .Enrich.FromLogContext()
    .Enrich.WithCorrelationIdHeader()    // X-Correlation-ID ekler
    .WriteTo.Console()
);
builder.Services.AddDefaultCorrelationId(options =>
{
    options.RequestHeader = "X-Correlation-Id";  // gelen header
    options.ResponseHeader = "X-Correlation-Id";  // isteğe bağlı: dönen yanıtta da header olarak dön
    options.AddToLoggingScope = true;               // Serilog/ILogger scope’a ekle
    options.UpdateTraceIdentifier = true;               // HttpContext.TraceIdentifier’ı da ayarla
    // options.IgnoreRequestHeader = false;               // istersen config’e ekle
});

//  MemoryCache
builder.Services.AddMemoryCache();

// EF Core In-Memory
builder.Services.AddDbContext<ApplicationDbContext>(opts =>
    opts.UseInMemoryDatabase("EcomDb"));
builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);
// Polly politikalarını tanımla
// Timeout: 10 sn içinde dönmezse TimeoutRejectedException
var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(
    TimeSpan.FromSeconds(10),
    TimeoutStrategy.Pessimistic,
    onTimeoutAsync: (ctx, ts, t) =>
    {
        // Log timeout istediğin gibi
        return Task.CompletedTask;
    });

// Circuit Breaker: 2 başarısız denemeden sonra 30 sn devre açık
var circuitBreakerPolicy = Policy<HttpResponseMessage>
    .Handle<TimeoutRejectedException>()
    .Or<HttpRequestException>()
    .OrResult(r => r.StatusCode == HttpStatusCode.InternalServerError)
    .CircuitBreakerAsync(
        handledEventsAllowedBeforeBreaking: 2,
        durationOfBreak: TimeSpan.FromSeconds(30),
        onBreak: (outcome, breakDelay) =>
        {
            // Log circuit açıldı
        },
        onReset: () =>
        {
            // Log circuit kapandı
        },
        onHalfOpen: () =>
        {
            // Log circuit half-open
        });

// Retry: tüm hata durumlarında 3 kez dene, 2^attempt saniye bekle
var retryPolicy = Policy<HttpResponseMessage>
    .Handle<Exception>()
    .WaitAndRetryAsync(
        retryCount: 3,
        sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
        onRetry: (outcome, delay, attempt, ctx) =>
        {
            // Log retryAttempt, outcome.Exception veya outcome.Result.StatusCode
        });

// Fallback: tüm politikalardan sonra hata alırsan her çağrıda yeni response üret
var fallbackPolicy = Policy<HttpResponseMessage>
    .Handle<Exception>()
    .FallbackAsync(
        fallbackAction: (outcome, ctx, ct) =>
        {
            var fake = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    "{\"success\":true,\"data\":[]}",
                    Encoding.UTF8,
                    "application/json")
            };
            return Task.FromResult(fake);
        },
        onFallbackAsync: (outcome, ctx) =>
        {
            // Log fallback tetiklendi
            return Task.CompletedTask;
        });

// Hepsini sırayla sar: Fallback → Retry → Circuit → Timeout
var policyWrap = Policy.WrapAsync(fallbackPolicy, retryPolicy, circuitBreakerPolicy, timeoutPolicy);

// HttpClient + Polly
builder.Services
    .AddHttpClient<IBalanceManagementService, BalanceManagementService>(c =>
    {
        c.BaseAddress = new Uri("https://balance-management-pi44.onrender.com");
    })
    .AddPolicyHandler(request =>
        request.Method == HttpMethod.Get
            ? Policy.NoOpAsync<HttpResponseMessage>()
            : policyWrap);

// DI & MediatR & Repository
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IIdempotencyRepository, IdempotencyRepository>();
builder.Services.AddMediatR(typeof(CreateOrderCommand).Assembly);

// Controllers + FluentValidation
builder.Services.AddControllers();

// FluentValidation otomatik valide ve client-side adapter’ları ekle
builder.Services
    .AddFluentValidationAutoValidation()
    .AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<CreateOrderRequestValidator>();

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.WebHost
    .ConfigureKestrel(opts => opts.ListenAnyIP(7004));

//  HealthChecks kayıtları
builder.Services.AddHealthChecks()
    // EF Core DbContext kont­rolü
    .AddDbContextCheck<ApplicationDbContext>("InMemoryDb")
    // Dış servisi kontrol eden custom santé check
    .AddCheck<BalanceServiceHealthCheck>("Balance-Service");

var app = builder.Build();
app.UseCorrelationId();
app.UseSerilogRequestLogging();   
// veya kendi logging middleware’in
// **Health-Checks endpoint’leri** 
app.UseRouting();

//  HTTP metrics middleware
app.UseHttpMetrics();

// otomatik olarak _in-flight_, duration, status code metrikleri toplar
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();

    // 2) /metrics endpoint’ini expose et
    endpoints.MapMetrics();
});
app.UseAuthorization();

//  Middleware pipeline
app.UseDeveloperExceptionPage();
app.UseSwagger();
app.UseSwaggerUI();

//  app.UseHttpsRedirection();
app.Run();
// Partial Program, integration test için
namespace ECommerce.API
{
    public partial class Program { }
}
