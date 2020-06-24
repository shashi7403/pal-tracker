using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Steeltoe.CloudFoundry.Connector.MySql.EFCore;
using Steeltoe.Management.CloudFoundry;
using Steeltoe.Management.Endpoint.CloudFoundry;
using Steeltoe.Common.HealthChecks;
using Steeltoe.Management.Endpoint;
using Steeltoe.Management.Hypermedia;
 using Steeltoe.Management.Endpoint.Info;

namespace PalTracker
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
            services.AddCloudFoundryActuators(Configuration, MediaTypeVersion.V2, ActuatorContext.ActuatorAndCloudFoundry);
            services.AddControllers();
            services.AddDbContext<TimeEntryContext>(options => options.UseMySql(Configuration));
            services.AddSingleton<IOperationCounter<TimeEntry>, OperationCounter<TimeEntry>>();
            services.AddSingleton<IInfoContributor, TimeEntryInfoContributor>();

           var message = Configuration.GetValue<string>("WELCOME_MESSAGE");

           var port = Configuration.GetValue<string>("PORT");
           var memoorylimit = Configuration.GetValue<string>("MEMORY_LIMIT");
           var cfinstanceIndex = Configuration.GetValue<string>("CF_INSTANCE_INDEX");
           var cfInstancAdder = Configuration.GetValue<string>("CF_INSTANCE_ADDR");
           if (string.IsNullOrEmpty(message))
           {
               throw new ApplicationException("WELCOME_MESSAGE not configured.");
           }
           services.AddSingleton(sp => new WelcomeMessage(message));
           services.AddSingleton(sp => new CloudFoundryInfo(port,memoorylimit,cfinstanceIndex,cfInstancAdder));
           //services.AddSingleton<ITimeEntryRepository, InMemoryTimeEntryRepository>();
           services.AddScoped<ITimeEntryRepository, MySqlTimeEntryRepository>();
           services.AddScoped<IHealthContributor, TimeEntryHealthContributor>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseCloudFoundryActuators(MediaTypeVersion.V2, ActuatorContext.ActuatorAndCloudFoundry);            app.UseHttpsRedirection();
            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                 name : "Default",
                 pattern : "{controller=Home}/{action=Index}/{id?}"              
                                 
                );
            });
    
        }
    }
}
