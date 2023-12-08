using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Steven.FastDFS.Demo.Provider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Steven.FastDFS.Demo.DependencyInjection
{
    public class FastDFSBuilder : IFastDFSBuilder
    {
        public IServiceCollection Services { get; }
        public IConfiguration Configuration { get; }

        public FastDFSBuilder(IServiceCollection services, IConfiguration configurationRoot)
        {
            Configuration = configurationRoot;
            Services = services;
            // 单例方式注入到IOC
            Services.TryAddSingleton<FastDFSProvider>();
        }
    }
}
