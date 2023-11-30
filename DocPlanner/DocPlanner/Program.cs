using Serilog;
using System.Text;
using System.Net.Http.Headers;
using DocPlanner.Interfaces;
using DocPlanner.Models;
using DocPlanner.Services;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("logs/myapp.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

//register Serilog
builder.Host.UseSerilog();

// Configure named HttpClients
builder.Services.AddHttpClient("Base64AuthClient", client =>
{
    var apiConfig = builder.Configuration.GetSection("SlotServiceApi").Get<SlotServiceApiConfig>();
    client.BaseAddress = new Uri(apiConfig.BaseUrl);

    var authToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{apiConfig.Username}:{apiConfig.Password}"));
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);
});

builder.Services.AddHttpClient("PlainTextAuthClient", client =>
{
    var apiConfig = builder.Configuration.GetSection("SlotServiceApi").Get<SlotServiceApiConfig>();
    client.BaseAddress = new Uri(apiConfig.BaseUrl);

    var authToken = $"{apiConfig.Username}:{apiConfig.Password}";
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);
});

// Register ISlotService
builder.Services.AddTransient<ISlotService, SlotService>();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
app.UseAuthorization();
app.MapControllers();

app.Run();

// Close Serilog
Log.CloseAndFlush();
