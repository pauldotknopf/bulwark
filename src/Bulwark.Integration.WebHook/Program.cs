using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Bulwark.Integration.GitLab;
using LibGit2Sharp;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Bulwark.Integration.WebHook
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        private static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((builderContext, config) =>
                {
                    config.AddJsonFile("config.json", true /*optional*/);
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddMvc();
                    // ReSharper disable RedundantNameQualifier
                    Bulwark.Services.Register(services);
                    Bulwark.Integration.Services.Register(services, context.Configuration);
                    Bulwark.Integration.GitLab.Services.Register(services, context.Configuration);
                    // ReSharper restore RedundantNameQualifier
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