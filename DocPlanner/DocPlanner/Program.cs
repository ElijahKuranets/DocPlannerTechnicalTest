using DocPlanner;
using DocPlanner.Extensions;
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddServices();
builder.Services.AddEndpointsApiExplorer();

builder.Services.ConfigureSettings(builder.Configuration);

builder.Services.SetupHttpClient(builder.Configuration);
builder.Services.SetupLogging(builder);
builder.Services.SetupSwagger();

builder.Services.AddAuthentication("BasicAuthentication")
    .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication", null);

builder.Services.AddRouting();
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
});
app.ConfigureHttpPipeline();
app.ConfigureExceptionHandling();

app.Run();