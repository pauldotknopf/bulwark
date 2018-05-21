using System;
using System.IO;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace Bulwark.Integration.WebHook
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            var cliApp = new CommandLineApplication();

            cliApp.HelpOption("-? | -h | --help");
            
            var webOnlyOption = cliApp.Option("--web-only",
                "Run only the web listener.",
                CommandOptionType.NoValue);

            var workerOnlyOption = cliApp.Option("--worker-only",
                "Run only the worker.",
                CommandOptionType.NoValue);

            cliApp.OnExecute(async () =>
            {
                PrepLogger();
                
                var webOnly = webOnlyOption.HasValue();
                var workerOnly = workerOnlyOption.HasValue();

                if (webOnly && workerOnly)
                {
                    Log.Logger.Error("You must provide either --web-only or --worker-only, not both");
                    return -1;
                }

                if (!webOnly && !workerOnly)
                {
                    // We are running both web listener and worker.
                    Log.Logger.Information("Running in both worker-only and web-only mode");
                    var host = BuildWebHost();
                    await host.RunAsync();
                    return 0;
                }
                
                if (workerOnly)
                {
                    // Only running worker.
                    Log.Logger.Information("Running in worker-only mode");
                    var host = BuildWorkerHost();
                    await host.RunAsync();
                }
                else
                {
                    // Only running web listener.
                    Log.Logger.Information("Running in web-only mode.");
                    var host = BuildWebHost(false /*don't run worker*/);
                    await host.RunAsync();
                }

                return 0;
            });

            return cliApp.Execute(args);
        }

        private static void PrepLogger()
        {
            var host = WebHost.CreateDefaultBuilder()
                .ConfigureAppConfiguration((_, c) =>
                {
                    c.AddJsonFile("config.json", true);
                })
                .Configure(app => {})
                .Build();

            var logSection = host.Services.GetRequiredService<IConfiguration>().GetSection("Logging");
            var minimalLevel = logSection.GetValue("MinimumLevel", LogEventLevel.Information);
            var logFile = logSection.GetValue<string>("File");
            var isRolling = logSection.GetValue("Rolling", false);

            var loggerConfiguration = new LoggerConfiguration()
                .MinimumLevel.Is(minimalLevel)
                .WriteTo.Console();

            if (!string.IsNullOrEmpty(logFile))
            {
                if (!Path.IsPathRooted(logFile))
                {
                    logFile = Path.Combine(Directory.GetCurrentDirectory(), logFile);
                }

                if (isRolling)
                {
                    loggerConfiguration.WriteTo.RollingFile(logFile);
                }
                else
                {
                    loggerConfiguration.WriteTo.File(logFile);
                }
            }
            
            Log.Logger = loggerConfiguration.CreateLogger();
            
            Log.Logger.Error("This is an error");
            Log.Logger.Information("This is info");
            Log.Logger.Debug("This is debug");
        }
        
        private static IHost BuildWorkerHost()
        {
            var environmentName = new ConfigurationBuilder()
                .AddEnvironmentVariables("ASPNETCORE_")
                .Build()
                .GetValue(HostDefaults.EnvironmentKey, Microsoft.Extensions.Hosting.EnvironmentName.Production);
            
            return new HostBuilder()
                .UseConsoleLifetime()
                .ConfigureLogging(loggingBuilder =>
                {
                    loggingBuilder.ClearProviders();
                    loggingBuilder.AddSerilog(dispose: true);
                })
                .UseEnvironment(environmentName)
                .UseContentRoot(Directory.GetCurrentDirectory())
                .ConfigureAppConfiguration((builderContext, config) =>
                {
                    config.AddEnvironmentVariables("ASPNETCORE_");
                    config.SetFileProvider(builderContext.HostingEnvironment.ContentRootFileProvider);
                    config.AddJsonFile("appsettings.json", true);
                    config.AddJsonFile($"appsettings.{builderContext.HostingEnvironment.EnvironmentName}.json", true);
                    config.AddJsonFile("config.json", true);
                })
                .ConfigureServices((context, services) =>
                {
                    // ReSharper disable RedundantNameQualifier
                    Bulwark.Services.Register(services);
                    Bulwark.Integration.Services.Register(services, context.Configuration);
                    Bulwark.Integration.GitLab.Services.Register(services, context.Configuration);
                    // ReSharper restore RedundantNameQualifier
                    services.AddSingleton<IHostedService, WorkerService>();
                })
                .Build();
        }

        private static IWebHost BuildWebHost(bool runWorker = true)
        {
            return WebHost.CreateDefaultBuilder()
                .ConfigureLogging(loggingBuilder =>
                {
                    loggingBuilder.ClearProviders();
                    loggingBuilder.AddSerilog(dispose: true);
                })
                .ConfigureAppConfiguration((builderContext, config) =>
                {
                    config.AddJsonFile("config.json", true);
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddMvc();
                    // ReSharper disable RedundantNameQualifier
                    Bulwark.Services.Register(services);
                    Bulwark.Integration.Services.Register(services, context.Configuration);
                    Bulwark.Integration.GitLab.Services.Register(services, context.Configuration);
                    // ReSharper restore RedundantNameQualifier
                    if(runWorker)
                        services.AddSingleton<IHostedService, WorkerService>();
                })
                .Configure(app =>
                {
                    app.UseMvc(routes =>
                    {
                        routes.MapRoute("gitlab", "gitlab", new
                        {
                            controller = "GitLab",
                            action = "Index"
                        });
                    });
                })
                .UseUrls("http://*:5000")
                .Build();
        }
    }
}