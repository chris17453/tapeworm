using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Collections.Immutable;
using YamlDotNet.Serialization;
//using tapeworm_core.queryable.sortable;

namespace tapeworm_core{

    public class config_summary{
        public string group                 { get; set; }
        public string entity                { get; set; }
        public string name                  { get; set; }
        public string display               { get; set; }
        public int    ordinal               { get; set; }
        public string uid                   { get; set; }
        public bool   active                { get; set; }

        public config_summary(config_file config) {
            this.name    =config.name;
            this.entity  =config.entity;
            this.display =config.display;
            this.group   =config.group;
            this.ordinal =config.ordinal;
            this.uid     =config.uid;
            this.active  =config.active;
        }
    }


    public class config_file{
        public string       name                  { get; set; }
        public string       display               { get; set; }
        public string       entity                { get; set; }
        public string       group                 { get; set; }
        public int          ordinal               { get; set; }=-1;
        public string       uid                   { get; set; }
        public string       path                  { get; set; }
        public string       regex                 { get; set; }="";
//        public string       field_delimiter       { get; set; }=",";
//        public string       array_delimiter       { get; set; }="|";
//        public string       comment_delimiter     { get; set; }="#";
        public delimiters   delimiters            { get; set; }     
        public string       key                   { get; set; }
        public bool         comments_visible      { get; set; }=false;
        public uint         data_starts_on_line   { get; set; }=1;
        public bool         errors_visible        { get; set; }=true;
        public bool         empty_lines_visible   { get; set; }=false;
        public Type         class_type            { get; set; }
        public bool         multi_search          { get; set; }=true;
        public bool         active                { get; set; }=false;
        public string       mode                  { get; set; }="simple";
        public string       source                { get; set; }
        public property[]   properties            { get; set; }


        public config_file(){
        }
        public  Dictionary<int,int>  property_ordinals { 
            get {
                property[] sorted=sorted=properties.ToArray();
                var order_by_expresisons = new List<Tuple<string, string>>();
                order_by_expresisons.Add(new Tuple<string,string>("ordinal"        ,"asc"));
                property[] sorted_properties=properties.NCDB_OrderBy(order_by_expresisons).ToArray();

                Dictionary<int,int> sorted_dictionary=new Dictionary<int, int>();
                
                foreach(property p in sorted_properties){
                    if(p.visible) {
                        sorted_dictionary[p.ordinal]=p.data_ordinal;
                    }
                }
                return sorted_dictionary;
            }
        }

        public void update_ordinals(){
            if(null==properties || !properties.Any()) return;
            int properties_count=properties.Count();
            if(globals.debug) Console.WriteLine(String.Format("Display Ordinal Mapping"));
            if(globals.debug) Console.WriteLine(String.Format("Property Count {0}",properties_count));

            bool[] has_ordinal=new bool [properties_count];

            for(int i=0;i<properties_count;i++) has_ordinal[i]=false;         //hash lookup emptied

            for(int i=0;i<properties_count;i++) {
                
                properties[i].data_ordinal=i;
                int ordinal_value=properties[i].ordinal;

                if(true==properties[i].visible && ordinal_value>-1 && ordinal_value<properties_count) {
                    has_ordinal[ordinal_value]=true;
                } else { 
                    properties[i].ordinal=-1; 
                }
            }
            //for(int i=0;i<properties_count;i++) { Console.WriteLine(String.Format("{0},{1},{2}",i,has_ordinal[i],properties[i].ordinal)); }

            for(int oi=0;oi<properties_count;oi++) {
                if(globals.debug) Console.Write(String.Format("property:[{0,2}] ",oi));
                if(true==properties[oi].visible && properties[oi].ordinal==-1) {
                            for(int i=0;i<properties_count;i++) {
                                if(false==has_ordinal[i]){
                                    if(globals.debug) Console.WriteLine(String.Format(" NEEDS {0,2} - {1} ",i,properties[oi].name));
                                    properties[oi].ordinal=i;
                                    has_ordinal[i]=true;
                                    break;
                                }//end if
                            }//end inner hash lookup
                } else { //end if ordinal
                    if(globals.debug) Console.WriteLine(String.Format(" HAS   {0,2} - {1}",properties[oi].ordinal,properties[oi].name));
                }
            }//end main loop
        }//end function

        public static config_file load_yaml(string file){
            if(string.IsNullOrWhiteSpace(file)) {
                Console.WriteLine("Cant Load, file is empty.");
                return null;
            }
            Console.WriteLine($"Loading: {file}");
            string yaml_text = File.ReadAllText(file);
            var input        = new StringReader(yaml_text);
            Deserializer deserializer=new DeserializerBuilder().IgnoreUnmatchedProperties().Build();
            return deserializer.Deserialize<config_file>(input);
        }//end load_yaml

        public static bool save_yaml(config_file config,string file){
            try{
                if(config==null) {
                    Console.WriteLine($"Cant save, config is null.");
                    return false;
                }
                Console.WriteLine($"Saving: {file}");
                using(TextWriter text_writer=new StreamWriter(file)) {
                    var serializer = new Serializer();
                    serializer.Serialize(text_writer, config);
                    return true;
                }
            } catch(Exception ex){
                Console.WriteLine($"Error saving: {ex.Message}");
            }
            return false;

        }//end load_yaml

    }//end config class

    public class property{
        //
        public string                    name              { get; set; }      
        public string                    bind_src          { get; set; }      
        public string[]                  bind_target       { get; set; }      
        public string                    display           { get; set; }=String.Empty;
					                     
        //type	                         
        public string                    type              { get; set; }="string";
        public string                    @default          { get; set; }=String.Empty;
        public bool                      is_array          { get; set; }=false;
        public bool                      has_default       { get; set; }=false;
					                     
        //display	                     
        public int                       ordinal           { get; set; }=-1;
        public bool                      visible           { get; set; }=true;
        public bool                      fixed_width       { get; set; }=true;
        public int                       width             { get; set; }=100;
        public int                       max_width         { get; set; }=0;
        public int                       min_width         { get; set; }=0;
        public bool                      overflow          { get; set; }=false;
					                     
        //search	                     
        public bool                      search            { get; set; }=false;
        public bool                      multi_search      { get; set; }=false;
					                     
        //sort	                         
        public bool                      sort              { get; set; }=false;
        public int                       sort_ordinal      { get; set; }=0;
        public bool                      sort_default      { get; set; }=false;
        public bool                      sort_default_asc  { get; set; }=false;
					                     
        //other	                         
        public int                       data_ordinal      { get; set; }=-1;
        public bool                      export            { get; set; }=false;
        public Dictionary<string,string> options           { get; set; }
    }
}
