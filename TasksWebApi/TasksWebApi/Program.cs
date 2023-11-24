using TasksWebApi.Exceptions;
using TasksWebApi.Middlewares;
using TasksWebApi.Startup;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.RegisterDbContext();
builder.Services.AddRegistrations();

builder.Services.AddControllers();
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddVersioning();

builder.Services.AddEndpointsApiExplorer();
builder.Services.ConfigureSwaggerOptions();

builder.Services.AddHostedService<HostedService>();
builder.Services.Configure<HostOptions>(opts => opts.ShutdownTimeout = TimeSpan.FromSeconds(50));

WebApplication app = builder.Build();

bool isDevelopment = app.Environment.IsDevelopment();

if (isDevelopment)
{
    app.CustomUseSwagger();
}

app.ConfigureExceptionHandler();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.AddMiddlewares();
app.MapControllers();

app.Run();

public partial class Program
{
}