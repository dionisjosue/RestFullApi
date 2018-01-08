using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NLog.Web;

namespace Library.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
           
            
               var webHost = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    var env = hostingContext.HostingEnvironment;
                    config.AddJsonFile("appsettings.json", false, true)
                    .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true, true);
                    config.AddEnvironmentVariables();
                })
                .ConfigureLogging((hostingContext, loggerFactory) =>
                {
                    loggerFactory.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                    loggerFactory.AddConsole();
                    loggerFactory.AddDebug();
                })
                .UseStartup<Startup>()
                .Build();
            webHost.Run();

             
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                /*.ConfigureAppConfiguration((config) =>
                {
                    config.AddJsonFile("config.json",true,true)
                })*/
                .UseStartup<Startup>()
                .UseNLog()
                .Build();        
    }
}
