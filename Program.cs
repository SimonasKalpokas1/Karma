using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Karma.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Karma
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .ConfigureLogging((hostBuilderContext, logging) =>
                {
                    logging.AddFileLogger(options =>
                    {
                        hostBuilderContext.Configuration.GetSection("Logging").GetSection("LogFile").GetSection("Options").Bind(options);
                    });
                });
    }
}
