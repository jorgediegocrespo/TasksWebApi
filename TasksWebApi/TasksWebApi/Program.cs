using Serilog;
using TasksWebApi.Exceptions;
using TasksWebApi.Middlewares;
using TasksWebApi.Startup;

var builder = WebApplication.CreateBuilder(args);

builder.Services.RegisterConfigurationValuesService(builder.Configuration);
builder.SetupOpenTelemetrySerilog(builder.Environment);

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddRateLimiter();
builder.Services.AddCache();
builder.Services.AddOpenTelemetry(builder.Environment, builder.Configuration);

builder.RegisterDbContext();
builder.Services.AddHttpContextAccessor();
builder.Services.AddRegistrations();

builder.Services.AddControllers();
builder.Services.AddJwtAuthentication();
builder.Services.AddVersioning();
builder.Services.AddCustomHealthChecks(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();
builder.Services.ConfigureSwaggerOptions();
builder.Services.AddHttpClient();

builder.Services.AddHostedService<HostedService>();
builder.Services.Configure<HostOptions>(opts => opts.ShutdownTimeout = TimeSpan.FromSeconds(50));
builder.UseOpenTelemetrySerilog();

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.CustomUseSwagger();
if (app.Environment.IsProduction())
    app.UseHsts();

app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.AddMiddlewares();
app.MapControllers();
app.MapCustomHealthChecks();
app.UseExceptionHandler();
app.UseRateLimiter();

app.Run();

public abstract partial class Program
{
}