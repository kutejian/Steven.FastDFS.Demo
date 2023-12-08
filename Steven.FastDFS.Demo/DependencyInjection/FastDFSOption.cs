using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Steven.FastDFS.Demo.DependencyInjection
{
    public class FastDFSOptions
    {
        public List<FastDFSAddress> AddressList { get; set; }
    }

    public class FastDFSAddress
    {
        public string Host { get; set; }
        public int Port { get; set; }
    }
}
