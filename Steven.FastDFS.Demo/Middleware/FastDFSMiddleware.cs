using FastDFS.Client;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Steven.FastDFS.Demo.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Steven.FastDFS.Demo.Middleware
{
    /// <summary>
    /// 初始化FastDFS的中间件
    /// </summary>
    public class FastDFSMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly FastDFSOptions _options;
        //如果需要其他服务，可以自己加
        public FastDFSMiddleware(RequestDelegate next, IOptions<FastDFSOptions> options)
        {
            _options = options.Value;
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            List<IPEndPoint> trackerPoints = new();
            var addressList = _options.AddressList;
            foreach (var address in addressList)
            {
                string host = address.Host;
                int port = address.Port;
                Console.WriteLine($"Host:{host}====Port:{port}");
                trackerPoints.Add(new IPEndPoint(IPAddress.Parse(host), port));
            }
            // 初始化FastDFS对象
            ConnectionManager.Initialize(trackerPoints);
            await _next(context);
        }
    }
}
