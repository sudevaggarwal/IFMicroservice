using DatabaseAccess;
using DatabaseAccess.Models;
using GenricComp.Extentions;
using GenricCompBA;
using GenricCompDA;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GenricComp
{
    public class Startup
    {
        internal readonly string MyAllowSpeificOrigins = "_myAllowSpecificOrigins";
        public readonly IConfiguration _configuration;
        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

       

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCorsPolic(_configuration, MyAllowSpeificOrigins);
            services.AddTransient<IDaHalper, DaHalper>();
            services.AddTransient<IGCDA, GCDA>();
            services.AddTransient<IGCBA, GCBA>();
            services.Configure<ConnectionString>(_configuration.GetSection("ConnectionString"));
            services.AddControllers();
            var isSwaggerEnable = _configuration.GetValue<bool>("ConnectionString:isSwaggerEnable");
            if (isSwaggerEnable)
                services.AddSwaggerGen();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            var isSwaggerEnable = _configuration.GetValue<bool>("ConnectionString:isSwaggerEnable");
            if (isSwaggerEnable)
                app.UseSwaggerDocumentation();
          //  app.UseCors(MyAllowSpeificOrigins);
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseRouting();
            
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
