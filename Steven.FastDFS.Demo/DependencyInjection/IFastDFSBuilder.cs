using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Steven.FastDFS.Demo.DependencyInjection
{
    public interface IFastDFSBuilder
    {
        IServiceCollection Services { get; }

        IConfiguration Configuration { get; }
    }
}
