using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Steven.FastDFS.Demo.DependencyInjection
{
    /// <summary>
    /// 依赖注入的扩展类
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        public static IFastDFSBuilder AddFastDFS(this IServiceCollection services)
        {
            var configuration = services.BuildServiceProvider().GetRequiredService<IConfiguration>();
            // 添加FastDFS的json文件解析
            services.Configure<FastDFSOptions>(configuration.GetSection("FastDFSOptions"));
            return new FastDFSBuilder(services, configuration);
        }

    }
}
