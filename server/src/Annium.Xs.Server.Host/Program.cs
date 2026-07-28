using Annium.AspNetCore.Extensions;
using Annium.Infrastructure.Hosting;
using Annium.Logging.Microsoft;
using Annium.Xs.Server.Host;
using Microsoft.AspNetCore.Builder;
using Swashbuckle.AspNetCore.Swagger;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseServicePack<ServicePack>();
builder.Logging.ConfigureLoggingBridge();
builder.WebHost.UseKestrelDefaults();

var app = builder.Build();

app.UseExceptionMiddleware();
app.UseSwagger(new SwaggerOptions());
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("v1/swagger.json", "v1");
    options.RoutePrefix = "swagger";
});
app.UseRouting();
app.UseCorsDefaults();
app.MapControllers();

await app.RunAsync();
