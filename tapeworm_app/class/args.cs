using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Options;
using Microsoft.EntityFrameworkCore.Internal;
using tapeworm_core;
namespace tapeworm {
    public static class args {


        public static options process(string[] arg_vars) {
            options o=new options();
            Dictionary<string,string> cur        =null;
            List<string> errors=new List<string>();

			var p = new OptionSet () {  
                "usage:",
                "tapeworm   --type {CONFIG}" ,
                "           --operation {OPERATION}" ,
                "           --sort {FIELD}:{VALUE}" ,
                "           --filter {FIELD}:{VALUE}" ,
                "           --multi-search {VALUE}" ,
                "           --format {FORMAT}" ,
                "           --hide_comments" ,
                "           --hide_blank" ,
                "           --format {FORMAT}" ,
                "           --page {PAGE}" ,
                "           --page_length {PAGE_LENGTH}" ,
                "           --verbosity {LIMIT} ",
                "           --build",
                "Examples:",
                "   tapeworm  -t xmachines -o report -i 1 -l 15 -f raw -v 15",
                "   tapeworm  -t machines -o edit -pfield:value -f raw -v 15",
                "   tapeworm  -t networks -o stats -f raw --v 15",
                "options:",
                { "t=|type=",      "The configuration to parse and function to perform \n" +
                                   "{CONFIG_TYPE}=("+String.Join(", ",o.types.ToArray())+")" 
                                   ,v => {  
                                            o.type=v.ToLower();
                                            //if(o.types.IndexOf(v.ToLower())>=0) o.type=v.ToLower();
                                            //else                                  throw new OptionException("type invalid: "+v,"-t"); 
                                        }},
                { "o=|operation=",      "The operation to perform \n" +
                                          "{OPERATION}=("+String.Join(", ",o.operations.ToArray())+")\n"  +                                      
                                          "stats  reports the line, entity and error count in each configuration file.\n"+
                                          "report outputs the configuration data"
                                        ,v => {   if(o.operations.IndexOf(v.ToLower())>=0) o.operation=v.ToLower();
                                                 else                                      throw new OptionException("operation invalid: "+v,"-o");
                                            }},
/*  TODO              { "p|parameters",  "parameter to filter by. \n" +
                                   "key:value,key:value"                                                                 
                                   ,v => { cur=o.parameters; }},
*/                "pagination:",
                { "i=|page=",       "paginated output: \n" +
                                   "{PAGE}=The page view starting with 0, default=0",
                                   (uint v)=> {  o.page=v; o.paginate=true; }},
                { "l=|page_length=","paginated output: \n" +
                                   "{PAGE_LENGTH}=The number of records to return, default=10.",
                                   (uint v)=> {  o.page_length=v;  o.paginate=true; }},
                "display",
                { "f=|format=",    "The data format for return data\n" +
                                   "{FORMAT}=("+String.Join(", ",o.formats.ToArray())+"), default=raw"                                                        
                                    , v => { 
                                        if(o.formats.IndexOf(v.ToLower())>=0) o.format=v.ToLower();
                                        else                                  throw new OptionException("format invalid: "+v,"-f");
                                    }
                },
/*                { "hc|hide_comments" ,"do not display comments in output"                                     , v =>   o.hide_comments=true },
                { "hb|hide_blanks"   ,"do not display blank lines in output"                                  , v =>   o.hide_blanks=true },
                { "he|hide_errors"   ,"do not display errored records in output"                              , v =>   o.hide_errors=true },
  */
                { "b|build"          ,"(re)build object models"                                               , v =>   o.build=true },
                "debugging",
                { "v:|verbosity:",  "increase debug message verbosity\n" +
                                   " {LIMIT}=(max lines of error output, default=10)"                            
                                   , (uint? v) => { 
                                        o.verbosity=1;  
                                      if(null!=v) o.max_error=(uint)v;
                                    }
                },
                "other",
                { "h|help"        ," show this message and exit"                                             , v =>   o.help = v != null },
                { "<>"          , v => {    if(null==cur) return;                                       //skip floating data
                                            string[] tokens = v.Split(new[] {','}); 
                                            foreach(string token in tokens) {
                                                string[] values = token.Split(new[]{'=', ':'}, 2);                            
                                                if(null!=values && values.Count()==2) {                    
                                                    cur.Add (values [0], values [1]);
                                                }
                                            }
                                        } 
                }
                };

            List<string> extra;
            try {
				extra = p.Parse (arg_vars);
            }
            catch (OptionException e) {
                errors.Add(" -"+e.Message);
                o.error++;
            }
            if(string.IsNullOrEmpty(o.type)) {
                errors.Add(" -Missing type");
                o.error++;
            }
            if(string.IsNullOrEmpty(o.operation)) {
                errors.Add(" -Missing Operation");
                o.error++;
            }
              
            if(o.help) {
				p.WriteOptionDescriptions(Console.Out);
            }

            if(o.verbosity>0) {
                Console.WriteLine("\n\nVerbosity enabled");
                if(errors.Count!=0 || o.error!=0) {
                    Console.WriteLine("\nErrors");
                    Console.WriteLine(String.Join("\n",errors.ToArray()));
                }
                Console.WriteLine("----------------------------------------------------------");
                Console.WriteLine("Execution Variables");
                Console.WriteLine("----------------------------------------------------------");
                Console.WriteLine(String.Format("  Type:          {0}",o.type));
                Console.WriteLine(String.Format("  Format:        {0}",o.format));
                Console.WriteLine(String.Format("  Operation:     {0}",o.operation));
//                Console.WriteLine(String.Format("  Params:        {0}",string.Join(",",o.parameters.Select(x => x.Key + ":" + x.Value).ToArray()) ));
                Console.WriteLine(String.Format("  Verbosity:     {0}",o.verbosity));
                Console.WriteLine(String.Format("  Max Errors     {0}",o.max_error));
                Console.WriteLine(String.Format("  Paginate:      {0}",o.paginate));
                Console.WriteLine(String.Format("  Page:          {0}",o.page));
                Console.WriteLine(String.Format("  Page Size:     {0}",o.page_length));
                Console.WriteLine(String.Format("  Help:          {0}",o.help));
                Console.WriteLine("----------------------------------------------------------\n");
            }
			return o;
		}//end func
    }//end class
}//end namespace
