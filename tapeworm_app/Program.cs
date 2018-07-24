using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using tapeworm_core;
using System.Security.Policy;

namespace tapeworm {
    public class Program {

        public static void Main(string[] arguments) {
            Task<int> return_value=preform_operations(arguments);

            //return_value.Result;

        }//end function

        static async Task<int> preform_operations(string[] arguments) {
            catalog cc=new catalog();
            options options=args.process(arguments);

            bool debug=false;


            string app_settings=String.Empty;

            app_settings="appsettings.json";

                
            var builder = new ConfigurationBuilder();
            IConfiguration configuration = builder.Build();
            string config_dir=configuration.GetSection("AppConfiguration")["config"];

            if(options.verbosity>0) debug=true;
            catalog c=new catalog(config_dir,options.build,debug);

            if(options.error>0) {
                Console.WriteLine("Exiting due to errors");
                return 1;
            }
            if(options.help) {               //nothing addidional is processed if the help scrren is shown
                return 0; 
            }
            //do the thing. logic belongs in the main dll
            string output=(string)await c.perform_operation(options);
            Console.WriteLine(output);
            return 0;
        }
    }//end class
}//end namespace