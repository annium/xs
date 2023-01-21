using Annium.Core.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Server.Host;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseServicePack<ServicePack>();
builder.Logging.ConfigureLoggingBridge();
builder.WebHost.UseKestrelDefaults();

var app = builder.Build();

app.UseExceptionMiddleware();
app.UseRouting();
app.UseCorsDefaults();
app.UseRequestLocalization("en", "ru");
app.MapControllers();

await app.RunAsync();