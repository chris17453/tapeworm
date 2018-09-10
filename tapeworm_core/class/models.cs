using System;
using System.Collections.Generic;
using System.IO;
using YamlDotNet.Serialization;
using System.Linq.Expressions;

namespace tapeworm_core  { 

    public class models{

        public models(){
        }

        public List<config_file> load(string folder_path,bool build) {
            List<config_file> config_files=new List<config_file>();
            string[] files = Directory.GetFiles(folder_path, "*.yaml", SearchOption.AllDirectories);
            //return config_files;
             foreach(string file in files) {
                if(globals.debug) Console.WriteLine(String.Format("Loading: {0}",file));
                config_file cf=config_file.load_yaml(file);
                if(cf==null) {
                    if(globals.debug) Console.WriteLine(String.Format("bad configuration file -> {0}",file));
                    continue;
                }
                if(globals.models.ContainsKey(cf.uid)) {
                    if(globals.debug) Console.WriteLine(String.Format("Skipping model load -> {0}",cf.uid));
                    config_files.Add(globals.models[cf.uid]);
                    continue;
                }
                if(globals.debug) Console.WriteLine(String.Format("model load -> {0}, active: {1}",cf.uid,cf.active));
                string ns="model";
                string dir=folder_path+"/compiled";
                if(!Directory.Exists(dir)) {
                    Directory.CreateDirectory(dir);
                }
                string dll_path=String.Format("{0}/{1}.dll",dir,Path.GetFileNameWithoutExtension(file));
                template template=new template(dll_path,ns,cf.name,cf.properties,build);
                cf.update_ordinals();
                globals.models[cf.uid]=cf;
                globals.add_model_properties(cf.uid,template.class_type);
                cf.class_type=template.class_type;
                config_files.Add(cf);
            }
            System.GC.Collect();
            return config_files;
        }

 
    }//end class
}//end namespace
