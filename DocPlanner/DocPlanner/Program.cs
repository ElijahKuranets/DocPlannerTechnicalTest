using Serilog;
using System.Text;
using System.Net.Http.Headers;
using DocPlanner;
using DocPlanner.Interfaces;
using DocPlanner.Models;
using DocPlanner.Services;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("logs/myapp.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

//register Serilog
builder.Host.UseSerilog();

// Register ISlotService
builder.Services.AddTransient<ISlotService, SlotService>();
builder.Services.AddScoped<IUserService, UserService>();

// Configure named HttpClients
builder.Services.AddHttpClient("Base64AuthClient", client =>
{
    var apiConfig = builder.Configuration.GetSection("SlotServiceApi").Get<SlotServiceApiConfig>();
    client.BaseAddress = new Uri(apiConfig!.BaseUrl);

    var authToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{apiConfig.Username}:{apiConfig.Password}"));
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);
});
var apiConfig = builder.Configuration.GetSection("SlotServiceApi").Get<SlotServiceApiConfig>();
builder.Services.AddSingleton(apiConfig);


// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo {Title = "My API", Version = "v1"});

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
            new string[] { }
        }
    });
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
