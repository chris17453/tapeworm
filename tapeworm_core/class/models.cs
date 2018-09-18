using System;
using System.Collections.Generic;
using System.IO;
using YamlDotNet.Serialization;
using System.Linq.Expressions;

namespace tapeworm_core
{

	public class models
	{

		public models()
		{
		}

		public List<config_file> load(string folder_path, bool build)
		{
			List<config_file> config_files = new List<config_file>();
			string[] files = Directory.GetFiles(folder_path+"/simple", "*.yaml", SearchOption.AllDirectories);
			foreach (string file in files){
				config_file cf=thing(file, folder_path,build);
				if (cf != null) config_files.Add(cf);
		    }
			files = Directory.GetFiles(folder_path + "/complex", "*.yaml", SearchOption.AllDirectories);
            foreach (string file in files)            {
                config_file cf = thing(file, folder_path, build);
                if (cf != null) config_files.Add(cf);
            }
		    System.GC.Collect();
            return config_files;
        }
    	public config_file thing(string file,string folder_path,bool build){
            if (globals.debug) Console.WriteLine(String.Format("Loading: {0}", file));
            config_file cf = config_file.load_yaml(file);
            if (cf == null) {
                if (globals.debug) Console.WriteLine(String.Format("bad configuration file -> {0}", file));
                return null;
            }
            if (globals.models.ContainsKey(cf.uid)) {
                if (globals.debug) Console.WriteLine(String.Format("Skipping model load -> {0}", cf.uid));
                return globals.models[cf.uid];
            }
            if (globals.debug) Console.WriteLine(String.Format("model load -> {0}, active: {1}", cf.uid, cf.active));
            string ns = "model";
            string dir = folder_path + "/compiled";
            string source_dir = folder_path + "/source";
            if (!Directory.Exists(dir)) {
                Directory.CreateDirectory(dir);
            }
            string dll_path = String.Format("{0}/{1}.dll", dir, cf.uid); // Path.GetFileNameWithoutExtension(file)
            string source_path = String.Format("{0}/{1}.cs", source_dir, cf.uid); // Path.GetFileNameWithoutExtension(file));
            template template = new template(dll_path, dir, source_path, ns, cf.name, cf.properties, build);
            cf.update_ordinals();
            globals.models[cf.uid] = cf;
            globals.add_model_properties(cf.uid, template.class_type);
            cf.class_type = template.class_type;
			return cf;

	}
 
    }//end class
}//end namespace
