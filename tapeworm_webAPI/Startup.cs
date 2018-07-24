using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using tapeworm_core;

namespace tapeworm_webAPI {
    public class Startup {

        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration      Configuration { get; }
        public IHostingEnvironment env           { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {
            try{
                services.AddOptions();
                services.AddCors(options =>
                {
                    options.AddPolicy("CorsPolicy",
                              builder => builder.AllowAnyOrigin()
                              .AllowAnyMethod()
                              .AllowAnyHeader()
                              .AllowCredentials() );
                });

                services.AddMvc(options =>
                {
                    options.FormatterMappings.SetMediaTypeMappingForFormat ("xml"   ,"application/xml");
                    options.FormatterMappings.SetMediaTypeMappingForFormat ("yaml"  ,"application/x-yaml");
                    options.FormatterMappings.SetMediaTypeMappingForFormat ("json"  ,"application/json");

                })
                        
                .AddXmlSerializerFormatters()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
              /*  services.Configure<ForwardedHeadersOptions>(options =>
                {
                    options.ForwardedHeaders = 
                        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                });*/
                //app.UseHttpsRedirection();

            } catch (Exception ex) {
                Console.WriteLine("Loading error: Startup -> ConfigureServices: "+ex.Message);
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app,IHostingEnvironment env) {
            try{
                this.env=env;

                if(env.IsDevelopment()) {
                    app.UseDeveloperExceptionPage();
                } else {
                    app.UseHsts();
                }
                app.UseCors("CorsPolicy");
                load_tapeworm();
                app.UseDefaultFiles();          //has to come before use static
                app.UseStaticFiles();
                app.UseMvc();
            } catch (Exception ex) {
                Console.WriteLine("Loading error: Startup -> Configure: "+ex.Message);
            }
        }

        public void load_tapeworm(){
            try{
                bool build=true;
                bool debug=true;
                //string root=Directory.GetCurrentDirectory();
                //string app_settings=root+"/"+$"appsettings.{env.EnvironmentName}.json";

                string config_dir=Configuration.GetSection("AppConfiguration")["config"];
                Console.WriteLine(String.Format("Configuration Directory: {0}",config_dir));

                Program.catalog.load(config_dir,build,debug);
            } catch (Exception ex) {
                Console.WriteLine("Loading error: "+ex.Message);
            }
        }
    }
}
