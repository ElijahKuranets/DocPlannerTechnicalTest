using DocPlanner.Clients;
using DocPlanner.Filters;
using DocPlanner.Interfaces;
using DocPlanner.Models;
using DocPlanner.Services;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Net.Http.Headers;
using System.Reflection;

namespace DocPlanner.Extensions;

public static class ServiceCollectionExtensions
{
    public static void ConfigureSettings(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<List<UserCredentials>>(configuration.GetSection(nameof(UserCredentials)));
    }

    public static void AddServices(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
        services.AddTransient<ISlotService, SlotService>();
        services.AddSingleton<IHttpContextAccessorService, HttpContextAccessorService>();
    }

    public static void SetupLogging(this IServiceCollection services, WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((context, configuration) =>
            configuration.ReadFrom.Configuration(context.Configuration)
            .WriteTo.File("logs/docPlannerLog.txt", rollingInterval: RollingInterval.Day));

        services.AddLogging(loggingBuilder =>
            loggingBuilder.AddSerilog(dispose: true));
    }

    public static void SetupHttpClient(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHttpClient<DocPlannerClient>((serviceProvider, client) =>
        {
            var httpContextAccessorService = serviceProvider.GetRequiredService<IHttpContextAccessorService>();
            var currentContext = httpContextAccessorService.GetCurrentHttpContext();
            var authorizationHeader = currentContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").LastOrDefault();

            if (!string.IsNullOrEmpty(authorizationHeader))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authorizationHeader);
            }
            var apiConfig = configuration.GetSection(nameof(SlotServiceApiConfig)).Get<SlotServiceApiConfig>();
            client.BaseAddress = new Uri(apiConfig?.BaseUrl);
        });
    }

    public static void SetupSwagger(this IServiceCollection services)
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

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            c.IncludeXmlComments(xmlPath);

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