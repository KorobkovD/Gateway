using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

if (builder.Environment.IsDevelopment())
{
    builder.Host.UseSerilog((_, configuration) => configuration.WriteTo.Console()
                                                               .Enrich.FromLogContext()
                                                               .Enrich.WithThreadId()
                                                               .Enrich.WithProcessId()
                                                               .Enrich.WithHttpRequestId());
}
else
{
    var logFilePath = Path.Combine(AppContext.BaseDirectory, "logs", "gateway_.log");
    builder.Host.UseSerilog((_, configuration) =>
                                configuration.WriteTo.Console()
                                             .WriteTo.File(logFilePath, retainedFileCountLimit: 7,
                                                           rollingInterval: RollingInterval.Day,
                                                           restrictedToMinimumLevel: LogEventLevel.Information,
                                                           outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff z} [{Level:u3}] {IPAddress} [{RequestId}] {SourceContext}: {Message:lj}{NewLine}{Exception}")
                                             .Enrich.FromLogContext()
                                             .Enrich.WithThreadId()
                                             .Enrich.WithProcessId()
                                             .Enrich.WithHttpRequestId());
}

builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
builder.Services.AddOcelot(builder.Configuration);

var app = builder.Build();
app.UseHttpsRedirection();
app.MapControllers();
app.UseSerilogRequestLogging();
app.UseRouting();

app.UseEndpoints(endpoints => {
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Ping}");
});

app.UseOcelot().Wait();
app.Run();