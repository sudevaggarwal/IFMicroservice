using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GenricComp.Extentions
{
    public static class AddCorsPolicy
    {
        public static IServiceCollection AddCorsPolic(this IServiceCollection services,IConfiguration configuration,string policyName = "defalutIF",string allowedOriginsAppKey = "AllowedOrigins")
        {
            if (string.IsNullOrEmpty(policyName))
            {
                throw new ArgumentNullException(nameof(policyName), $"{nameof(policyName)} can't be null be empty");
            }
            if (string.IsNullOrEmpty(allowedOriginsAppKey))
            {
                throw new ArgumentNullException(nameof(allowedOriginsAppKey), $"{nameof(allowedOriginsAppKey)} can't be empty");
            }
            return services.AddCors(options =>
            {
                options.AddPolicy(policyName,
                    builder =>
                    {
                        var allowedHosts = configuration.GetValue<string>(allowedOriginsAppKey);
                        if (!string.IsNullOrEmpty(allowedHosts))
                        {
                            var allowedHostsArray = allowedHosts.Split(',');
                            builder.WithOrigins(allowedHostsArray);
                        }
                        builder.AllowAnyHeader().AllowAnyMethod();
                    });
            });
        }
    }
}
