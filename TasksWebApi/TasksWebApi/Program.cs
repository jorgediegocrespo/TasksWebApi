using TasksWebApi.Exceptions;
using TasksWebApi.Middlewares;
using TasksWebApi.Startup;

var builder = WebApplication.CreateBuilder(args);

builder.SetupSerilog();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddRateLimiter();

builder.RegisterDbContext();
builder.Services.AddRegistrations();

builder.Services.AddControllers(x => x.AuditSetupFilter(builder.Configuration));
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddVersioning();

builder.Services.AddEndpointsApiExplorer();
builder.Services.ConfigureSwaggerOptions();

builder.Services.AddHostedService<HostedService>();
builder.Services.Configure<HostOptions>(opts => opts.ShutdownTimeout = TimeSpan.FromSeconds(50));
builder.UseSerilog();

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.CustomUseSwagger();
if (app.Environment.IsProduction())
    app.UseHsts();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.AddMiddlewares();
app.MapControllers();
app.UseAudit(builder.Configuration);
app.UseExceptionHandler();
app.UseRateLimiter();

app.Run();

public abstract partial class Program
{
}