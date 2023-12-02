using DocPlanner;
using DocPlanner.Filters;
using DocPlanner.Interfaces;
using DocPlanner.Models;
using DocPlanner.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Net.Http.Headers;

// Initialize Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.File("logs/myapp.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// Register Serilog
builder.Host.UseSerilog();

// Register Services
builder.Services.AddTransient<ISlotService, SlotService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddSingleton<IHttpContextAccessorService, HttpContextAccessorService>();

// Configure DockPlanner HttpClient
builder.Services.AddHttpClient("Base64AuthClient", (serviceProvider, client) =>
{
    var httpContextAccessorService = serviceProvider.GetRequiredService<IHttpContextAccessorService>();
    var currentContext = httpContextAccessorService.GetCurrentHttpContext();
    var authorizationHeader = currentContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").LastOrDefault();

    if (!string.IsNullOrEmpty(authorizationHeader))
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authorizationHeader);
    }
    var apiConfig = builder.Configuration.GetSection("SlotServiceApi").Get<SlotServiceApiConfig>();
    client.BaseAddress = new Uri(apiConfig!.BaseUrl);
});
builder.Services.Configure<List<UserCredentialsConfig>>(builder.Configuration.GetSection("UserCredentials"));

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo {Title = "DocPlanner API", Version = "v1"});

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
builder.Services.AddAuthentication("BasicAuthentication")
    .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication", null);
builder.Services.AddControllers();
builder.Services.AddRouting();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
});

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

// Close Serilog
Log.CloseAndFlush();
