using DocPlanner.Interfaces;
using DocPlanner.Models;
using DocPlanner.Services;
using System.Net.Http.Headers;
using DocPlanner.Filters;
using Microsoft.OpenApi.Models;
using Serilog;

namespace DocPlanner.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection ConfigureSettings(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<List<UserCredentials>>(configuration.GetSection(nameof(UserCredentials)));

        return services;
    }

    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
        services.AddTransient<ISlotService, SlotService>();
        services.AddSingleton<IHttpContextAccessorService, HttpContextAccessorService>();

        return services;
    }

    public static IServiceCollection SetupLogging(this IServiceCollection services, WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((context, configuration) =>
            configuration.ReadFrom.Configuration(context.Configuration)
            .WriteTo.File("logs/myapp.txt", rollingInterval: RollingInterval.Day));

        services.AddLogging(loggingBuilder =>
            loggingBuilder.AddSerilog(dispose: true));

        return services;
    }

    public static IServiceCollection SetupHttpClient(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHttpClient("Base64AuthClient", (serviceProvider, client) =>
        {
            var httpContextAccessorService = serviceProvider.GetRequiredService<IHttpContextAccessorService>();
            var currentContext = httpContextAccessorService.GetCurrentHttpContext();
            var authorizationHeader = currentContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").LastOrDefault();

            if (!string.IsNullOrEmpty(authorizationHeader))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authorizationHeader);
            }
            var apiConfig = configuration.GetSection("SlotServiceApi").Get<SlotServiceApiConfig>();
            client.BaseAddress = new Uri(apiConfig!.BaseUrl);
        });
        return services;
    }

    public static IServiceCollection SetupSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "DocPlanner API", Version = "v1" });

            c.AddSecurityDefinition("basic", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "basic",
                In = ParameterLocation.Header,
                Description = "Basic Authorization header using the Bearer scheme."
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "basic"
                        }
                    },
                    Array.Empty<string>()
                }
            });
            c.SchemaFilter<SwaggerDateTimeFormatSchemaFilter>();
        });

        return services;
    }

    public static void ConfigureExceptionHandling(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }
    }

    public static void ConfigureHttpPipeline(this WebApplication app)
    {
        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
    }
}