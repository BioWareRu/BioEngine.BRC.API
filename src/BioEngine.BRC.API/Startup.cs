﻿using BioEngine.Core.API;
using BioEngine.Core.API.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BioEngine.BRC.Api
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
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddScoped(typeof(BaseControllerContext<>));
            services.AddTransient<RequestParams>();

            services.AddCors(options =>
            {
                // Define one or more CORS policies
                options.AddPolicy("allorigins",
                    corsBuilder =>
                    {
                        corsBuilder.AllowAnyHeader().AllowAnyOrigin().AllowAnyMethod().AllowCredentials();
                    });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseAuthentication();
            app.UseCors("allorigins");
            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}