﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Steeltoe.CloudFoundry.Connector.MySql.EFCore;
using Steeltoe.Management.CloudFoundry;
using Steeltoe.Common.HealthChecks;

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
            services.AddMvc();
            services.AddSingleton(sp => new WelcomeMessage(
                Configuration.GetValue<string>("WELCOME_MESSAGE", "WELCOME_MESSAGE not configured.")
            ));
            

            services.AddSingleton(cf => new CloudFoundryInfo(
                Configuration.GetValue<string>("PORT", "N/A"), 
                Configuration.GetValue<string>("MEMORY_LIMIT", "N/A"), 
                Configuration.GetValue<string>("CF_INSTANCE_INDEX", "N/A"), 
                Configuration.GetValue<string>("CF_INSTANCE_ADDR", "N/A")
            ));

            services.AddScoped<ITimeEntryRepository, MySqlTimeEntryRepository>();
            services.AddSingleton<IHealthContributor, TimeEntryHealthContributor>();

            services.AddDbContext<TimeEntryContext>(options => options.UseMySql(Configuration));     

            services.AddCloudFoundryActuators(Configuration);       
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
            app.UseCloudFoundryActuators(); // Needs to occur after mvc to ensure that our controllers mapping take precedence over the actuator endpoints.
        }
    }
}
