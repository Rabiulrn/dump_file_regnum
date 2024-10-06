//using RegnumServices;
//using Serilog;
//var builder = Host.CreateApplicationBuilder(args);
//builder.Services.AddHostedService<RegWorkServ>();
//builder.Services.AddSerilog();
////builder.Services.AddSystemd(); for Linux
//builder.Services.AddWindowsService();
//var host = builder.Build();
//var configsetting = new ConfigurationBuilder().
//	AddJsonFile("appsettings.json").Build();
//Log.Logger = new LoggerConfiguration()
//	.MinimumLevel.Debug()
//	.MinimumLevel.Override("microsoft", Serilog.Events.LogEventLevel.Warning)
//	.Enrich.FromLogContext()
//	.WriteTo.File(configsetting["Logging:Logpath"])
//	.CreateLogger();
//host.Run();
using RegnumServices;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

// Add RegWorkServ as a hosted service
builder.Services.AddHostedService<RegWorkServ>();

// Configure Serilog
var configsetting = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.File(configsetting["Logging:Logpath"])  // Path from appsettings.json
    .CreateLogger();

builder.Services.AddLogging(loggingBuilder =>
    loggingBuilder.AddSerilog(dispose: true));  // Dispose Serilog properly when done

// Optional: Add Windows or Linux-specific services if needed
builder.Services.AddWindowsService();  // For Windows Service deployment
// builder.Services.AddSystemd();  // Uncomment if running on Linux

var host = builder.Build();

// Run the host, which will only execute RegWorkServ
await host.RunAsync();
