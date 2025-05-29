using Annium.Core.DependencyInjection;
using Annium.Xs.Server.Host;
using Microsoft.AspNetCore.Builder;
using Swashbuckle.AspNetCore.Swagger;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseServicePack<ServicePack>();
builder.Logging.ConfigureLoggingBridge();
builder.WebHost.UseKestrelDefaults();

var app = builder.Build();

app.UseSwagger(new SwaggerOptions());
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("v1/swagger.json", "v1");
    options.RoutePrefix = "swagger";
});
app.UseExceptionMiddleware();
app.UseRouting();
app.UseCorsDefaults();
app.MapControllers();

await app.RunAsync();
