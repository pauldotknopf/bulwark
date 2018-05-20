using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Bulwark.Integration.WebHook
{
    public static class Program
    {
        public static async Task<int> Main(string[] args)
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
                var webOnly = webOnlyOption.HasValue();
                var workerOnly = workerOnlyOption.HasValue();

                if (webOnly && workerOnly)
                {
                    Console.WriteLine("You must provide either --web-only or --worker-only...");
                    return -1;
                }

                if (!webOnly && !workerOnly)
                {
                    // We are running both web listener and worker.
                    var host = BuildWebHost();
                    await host.RunAsync();
                    return 0;
                }
                
                if (workerOnly)
                {
                    // Only running worker.
                    var host = BuildWorkerHost();
                    await host.RunAsync();
                }
                else
                {
                    // Only running web listener.
                    var host = BuildWebHost(false /*don't run worker*/);
                    await host.RunAsync();
                }

                return 0;
            });

            cliApp.Execute(args);
            
            Console.WriteLine("exiting...");

            return 0;
        }
        
        private static IHost BuildWorkerHost()
        {
            return new HostBuilder()
                .UseConsoleLifetime()
                .ConfigureAppConfiguration((builderContext, config) =>
                {
                    config.AddJsonFile("config.json");
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
                .ConfigureAppConfiguration((builderContext, config) =>
                {
                    config.AddJsonFile("config.json");
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