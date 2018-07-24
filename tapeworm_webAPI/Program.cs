using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using tapeworm_core;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal;

namespace tapeworm_webAPI {
    public class Program {
        public static catalog catalog { get; set; }=new catalog();
        public static void Main(string[] args) {
            BuildWebHost().Run();
        }

        public static IWebHost BuildWebHost(){
            string root=Directory.GetCurrentDirectory();
            Console.WriteLine(String.Format("Working Directory: {0}",root));

            WebHostBuilder wb=new WebHostBuilder();

            wb.UseContentRoot(root);
            wb.UseKestrel();
            wb.ConfigureAppConfiguration((builderContext, config) => {
                IHostingEnvironment env = builderContext.HostingEnvironment;
                string app_settings=$"appsettings.{env.EnvironmentName}.json";
                Console.WriteLine($"Loading Configuration: "+app_settings);
                config.AddJsonFile(app_settings, optional: true, reloadOnChange: true);
            });

            wb.UseIISIntegration();
            wb.UseDefaultServiceProvider((context, options) => {
                    options.ValidateScopes = context.HostingEnvironment.IsDevelopment();
            });
            wb.UseStartup<Startup>();
            wb.UseUrls("http://0.0.0.0:5001");
            return wb.Build();
        }

    }//end class
}//end namespace
