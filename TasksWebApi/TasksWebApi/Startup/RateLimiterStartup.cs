using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using TasksWebApi.Models;
using TasksWebApi.Services;

namespace TasksWebApi.Startup
{
    public static class RateLimiterStartup
    {
        public static void AddRateLimiter(this IServiceCollection services)
        {
            services.AddCustomPolicy();
        }

        private static void AddFixedWindowLimiter(this IServiceCollection services)
        {
            services.AddRateLimiter(rateLimiterOptions => rateLimiterOptions
                .AddFixedWindowLimiter(policyName: "fixed", options =>
                {
                    options.PermitLimit = 100;
                    options.Window = TimeSpan.FromDays(1);
                    options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    options.QueueLimit = 0;
                })
                .RejectionStatusCode = StatusCodes.Status429TooManyRequests);
        }
        
        private static void AddSlidingWindowLimiter(this IServiceCollection services)
        {
            services.AddRateLimiter(rateLimiterOptions => rateLimiterOptions
                .AddSlidingWindowLimiter("sliding", options =>
                {
                    options.PermitLimit = 100;
                    options.Window = TimeSpan.FromDays(1);
                    options.SegmentsPerWindow = 24;
                    options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    options.QueueLimit = 0;
                })
                .RejectionStatusCode = StatusCodes.Status429TooManyRequests);
        }
        
        private static void AddTokenBucketLimiter(this IServiceCollection services)
        {
            services.AddRateLimiter(rateLimiterOptions => rateLimiterOptions
                .AddTokenBucketLimiter("token", options =>
                {
                    options.TokenLimit = 100;
                    options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    options.ReplenishmentPeriod = TimeSpan.FromDays(1);
                    options.TokensPerPeriod = 20;
                    options.AutoReplenishment = true;
                    options.QueueLimit = 0;
                })
                .RejectionStatusCode = StatusCodes.Status429TooManyRequests);
        }
        
        private static void AddConcurrencyLimiter(this IServiceCollection services)
        {
            services.AddRateLimiter(rateLimiterOptions => rateLimiterOptions
                .AddConcurrencyLimiter("concurrency", options =>
                {
                    options.PermitLimit = 3;
                    options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    options.QueueLimit = 0;
                })
                .RejectionStatusCode = StatusCodes.Status429TooManyRequests);
        }
        
        private static void AddRolePolicy(this IServiceCollection services)
        {
            services.AddRateLimiter(rateLimiterOptions => rateLimiterOptions
                .AddPolicy("rolepolicy", context =>
                {
                    int superAdminFactor = context.User.IsInRole(Roles.SUPERADMIN) ? 10 : 1;
                    return RateLimitPartition.GetSlidingWindowLimiter(context.User.Identity.Name,
                        _ => new SlidingWindowRateLimiterOptions
                        {
                            PermitLimit = 100 * superAdminFactor,
                            Window = TimeSpan.FromDays(1),
                            SegmentsPerWindow = 24,
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 0
                        });
                })
                .RejectionStatusCode = StatusCodes.Status429TooManyRequests);
        }
        
        private static void AddCustomPolicy(this IServiceCollection services)
        {
            services.AddRateLimiter(rateLimiterOptions => rateLimiterOptions
                .AddPolicy<string, CustomRateLimiterPolicy>("custompolicy")
                .RejectionStatusCode = StatusCodes.Status429TooManyRequests);
        }
    }
/*La propiedad QueueLimit en el código seleccionado se utiliza para limitar el número de solicitudes que pueden ser encoladas cuando la tasa límite
se ha alcanzado en un periodo de tiempo específico.
En este caso, options.QueueLimit = 10; significa que cuando se alcanza el límite de solicitudes permitidas (definido por options.PermitLimit),
las siguientes 10 solicitudes no serán rechazadas de inmediato, sino que serán encoladas y procesadas cuando el límite de tasa permita más solicitudes.
Si se supera el QueueLimit, las solicitudes adicionales serán rechazadas hasta que haya espacio en la cola o hasta que el límite de tasa permita más solicitudes.
Esto puede ser útil para manejar ráfagas de tráfico en tu API sin rechazar inmediatamente las solicitudes cuando se alcanza el límite de tasa.*/
    
}

public class CustomRateLimiterPolicy : IRateLimiterPolicy<string>
{
    private readonly IHttpContextService _httpContextService;
    private readonly IConfiguration _configuration;
    private Func<OnRejectedContext, CancellationToken, ValueTask> _onRejected;
    
    public CustomRateLimiterPolicy(ILogger<CustomRateLimiterPolicy> logger, IHttpContextService httpContextService, IConfiguration configuration)
    {
        _httpContextService = httpContextService;
        _configuration = configuration;
        _onRejected = (ctx, _) =>
        {
            ctx.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            logger.LogWarning($"Request rejected by {nameof(CustomRateLimiterPolicy)}");
            return ValueTask.CompletedTask;
        };
    }

    public Func<OnRejectedContext, CancellationToken, ValueTask> OnRejected => _onRejected;

    public RateLimitPartition<string> GetPartition(HttpContext httpContext)
    {
        var user = _httpContextService.GetContextUser();
        return user == null ? GetPartitionByClientIp() : GetPartitionByUserId(user);
    }

    private RateLimitPartition<string> GetPartitionByUserId(UserResponse user)
    {
        var superAdminFactor = user.Roles.Contains(Roles.SUPERADMIN) ? 10 : 1;
        return RateLimitPartition.GetSlidingWindowLimiter(user.Id,
            _ => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = int.Parse(_configuration["UserRateLimiter:PermitLimit"]!) * superAdminFactor,
                Window = TimeSpan.Parse(_configuration["UserRateLimiter:Window"]!),
                SegmentsPerWindow = int.Parse(_configuration["UserRateLimiter:SegmentsPerWindow"]!),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = int.Parse(_configuration["UserRateLimiter:QueueLimit"]!)
            });
    }

    private RateLimitPartition<string> GetPartitionByClientIp()
    {
        return RateLimitPartition.GetSlidingWindowLimiter(_httpContextService.GetClientIp(),
            _ => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = int.Parse(_configuration["IpRateLimiter:PermitLimit"]!),
                Window = TimeSpan.Parse(_configuration["IpRateLimiter:Window"]!),
                SegmentsPerWindow = int.Parse(_configuration["IpRateLimiter:SegmentsPerWindow"]!),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = int.Parse(_configuration["IpRateLimiter:QueueLimit"]!)
            });
    }
}