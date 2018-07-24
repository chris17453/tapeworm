using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.ComponentModel.Design;
using System.Xml.Linq;

namespace tapeworm_core{
    public static class globals{
        public static List<string>                      types       { get; set; }=new List<string>();
        public static bool                              debug       { get; set; }=true;
        public static Dictionary<string,PropertyInfo>   properties  { get; set; }=new Dictionary<string, PropertyInfo>();
        public static Dictionary<string,config_file>    models      { get; set; }=new Dictionary<string,config_file>();



        public static void add_model_properties(string uid,Type type){
            PropertyInfo[] properties_list=type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            int ordinal=0;
            if(string.IsNullOrWhiteSpace(uid)) {
                throw new Exception(string.Format("Model does not have a name"));
            }
            foreach(PropertyInfo property in properties_list){
                if(null==property || string.IsNullOrWhiteSpace(property.Name)) continue;
                string key=uid+'.'+property.Name;
                properties[key]=property;
                key=uid+'.'+ordinal;
                properties[key]=property;
                ordinal++;
            }            
        }

        public static PropertyInfo get_model_property(string uid,string property_name){
            string key=uid+'.'+property_name;
            if(properties.ContainsKey(key)) return properties[key];
            return null;
        }
        public static PropertyInfo get_model_property(string uid,int ordinal){
            string key=uid+'.'+ordinal.ToString();
            if(properties.ContainsKey(key)) return properties[key];
            return null;
        }
       
        public static List<config_summary> get_list(){
            List<KeyValuePair<string,config_file>> model_list=globals.models.Where(x=>x.Value.active==true).ToList();
            model_list.Sort(delegate (KeyValuePair<String,config_file> x,KeyValuePair<String,config_file> y) {

                if(x.Value.ordinal > y.Value.ordinal) return 1;
                if(x.Value.ordinal < y.Value.ordinal) return -1;


                int gc = string.Compare(x.Value.group,y.Value.group,StringComparison.OrdinalIgnoreCase);
                if(gc != 0) return gc;
                int nc = string.Compare(x.Value.name,y.Value.name,StringComparison.OrdinalIgnoreCase);
                if(nc != 0) return nc;
                return 0;
            });        

            List<config_summary> return_list=new List<config_summary>();
            foreach(KeyValuePair<string,config_file> _model in model_list) { return_list.Add(new config_summary(_model.Value)); }
            return return_list;
        }


    }//end class
}//end namespace
