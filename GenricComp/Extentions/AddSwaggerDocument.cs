using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GenricComp.Extentions
{
    public static class SwaggerServiceExtension {
        //public static IServiceCollection AddSwaggerDocument(this IServiceCollection services, string title = "Insights AccessControl API", string version = "v1", string description = "API for InsightFirst")
        //{
        //    services.AddSwaggerGen(c =>
        //    {
        //        c.SwaggerDoc("v1", new OpenApiInfo
        //        {
        //            Title = title,
        //            Version = version,
        //            Description = description,
        //            Contact = new OpenApiContact
        //            {
        //                Name = "sudev"
        //            }
        //        });
        //        //c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        //        //{
        //        //    Name = "Authorization",
        //        //    Type = SecuritySchemeType.ApiKey,
        //        //    Scheme = "Bearer",
        //        //    BearerFormat = "JWT",
        //        //    In = ParameterLocation.Header
        //        //});
        //        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        //    {{
        //        new OpenApiSecurityScheme
        //        {
        //            Reference = new OpenApiReference
        //            {
        //                Type = ReferenceType.SecurityScheme,
        //                Id = "Bearer"
        //            }
        //        },
        //        Array.Empty<string>() }
        //    });
        //    });
        //    return services;
        //}
        public static IApplicationBuilder UseSwaggerDocumentation(this IApplicationBuilder app, string name = "Insight")
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", name);
            });
            return app;
        }
    }



}
