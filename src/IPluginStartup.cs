using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace OpenFaaS
{
    public interface IPluginStartup
    {
        void ConfigureServices( IServiceCollection services );
        void Configure( IApplicationBuilder app, IWebHostEnvironment env );
    }
}
