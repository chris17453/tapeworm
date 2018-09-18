using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using YamlDotNet;
using System.Reflection.PortableExecutable;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Immutable;
using System.Xml.Linq;

namespace tapeworm_core  { 
    public class catalog {
        List<table_base> tables { get; set; }=new List<table_base>();
        public string config_folder { get; set; }
        public bool   build         { get; set; } =false;
        public bool   debug         { get; set; } =false;

        //direct indexer
        public table_base this[string key] {
            get{ 
                foreach(table_base t in tables){
                    if(t.uid==key) return t;
                }
                return null;
            }
        }

		//constructor
        public catalog() {
        }

        public catalog(string config_folder,bool build=false,bool debug=false){
            this.config_folder=config_folder;
            this.debug=debug;
            this.build=build;
            load();
        }

        public void load(string config_folder,bool build=false,bool debug=false){
            this.config_folder=config_folder;
            this.debug=debug;
            this.build=build;
            load();
        }

        public catalog reload(){
            catalog c=new catalog(config_folder,build,debug);
            return c;
        }

        public void load(){
            if(globals.debug) Console.WriteLine("Loading");

            List<config_file> model_files =new models().load(config_folder,build);
			globals.debug=debug;

            foreach(config_file item in model_files) {
                bool results=add(item);
            }
		}
        public bool add(config_file table_config) {
            if(null!=this[table_config.uid]) {
                if(globals.debug) Console.WriteLine(String.Format("table already exists: {0}",table_config.name));
                return false;                      //already exists.
            }
            if(globals.debug) Console.WriteLine(string.Format("Adding Table {0}",table_config.name));
            tables.Add(new table_base(table_config));
            return true;
		}

        public bool update(){
            foreach(table_base instance in tables) {
                bool results=instance.update();
                if(results==false) {
                    throw new Exception("Failed to update");  //TODO name verbose
                }
            }
			return true;
		}

		public List<stat> stats(options options){
            List<stat> stats =new List<stat>();
            foreach(table_base instance in tables) {
                stat s=instance.stat();
                 stats.Add(s);
            }
            return stats;
		}//end stats

        //This is what changes the text output
        public async Task<string> optupt_converter(string format,object obj){
            if(null==obj) {
                if(globals.debug) Console.WriteLine(String.Format("Output Converter: No Object"));
                return "";
            }
            string output=String.Empty;
            TextWriter text_writer=new StringWriter();
            switch(format) {
                case "none"    : break;
                case "raw"     : await text_writer.WriteAsync(obj.ToString()); break;
                case "json"    : await text_writer.WriteAsync(JsonConvert.SerializeObject(obj)); break;
                case "yaml"    : var serializer = new YamlDotNet.Serialization.Serializer();                                //TODO awync yaml
                                 serializer.Serialize(text_writer,obj,obj.GetType());
                                 break;
            }
            return text_writer.ToString();
        }

        public async Task<object> perform_operation(options options,bool return_object=false) {
            string header=String.Empty;
            object obj=null;

            table_base t=this[options.uid];
            switch(options.operation.ToLower()) {
                /***ON SINGULAR TABLE****/
                case "report": if(null==t) throw new Exception("Table did not load properly, Invalid type-report");
                               obj=await t.report(options); 
                               break;
                case "list"  :  obj=globals.get_list();
                               break;
                case "config": if(globals.models.ContainsKey(options.uid)) obj=globals.models[options.uid];
                else throw new Exception(String.Format("Config: Model not in global cache. {0}",options.uid));
                               break;
                case "stat"  : if(null==t) throw new Exception("Table did not load properly, Invalid type");
                               obj=t.stat  ();  
                               break;
                
                /***ON ALL TABLES****/
//                case "stats" : obj=stats(options);     break;
                
                default      : return "Unknown operation"; 
            }
            if(return_object) return obj;
            if(options.format=="raw") header=options.ToString();
            string base_string=await optupt_converter(options.format,obj);
            return header+base_string;
        }

        public async Task<report> get_report(options options){
            table_base t=this[options.uid];
            if(null==t) throw new Exception("Invalid type");
             return await t.report(options); 
        }
    }
}
