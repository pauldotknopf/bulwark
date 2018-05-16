using Bulwark.Integration.Messages;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bulwark.Integration.WebHook
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            // ReSharper disable RedundantNameQualifier
            Bulwark.Services.Register(services);
            Bulwark.Integration.Services.Register(services);
            Bulwark.Integration.GitLab.Services.Register(services);
            // ReSharper restore RedundantNameQualifier
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseMvc(routes =>
            {
                routes.MapRoute("gitlab", "gitlab", new
                {
                    controller = "GitLab",
                    action = "Index"
                });
            });
        }
    }
}