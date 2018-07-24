using System;
using System.Reflection;

namespace tapeworm_core {
    public static partial class query_extensions {
        public static object get_value(Type type,string property_name,object source) {
            if(null==type) return null;
            PropertyInfo pi=type.GetProperty(property_name);
            if(null==pi) {
                return null;
            }
            return pi.GetValue(source,null);
        }
        public static PropertyInfo get_property(Type type,string property_name) {
            if(null==type) return null;
            PropertyInfo pi=type.GetProperty(property_name);
            if(null==pi) {
                return null;
            }
            return pi;
        }
        public static Tuple<Type,Object> get_object_from_path(string property_path,object source,int depth=0){
            //if(globals.debug) Console.WriteLine(String.Format("Searching path ->  {0}:{1}",depth,property_path));
            if(string.IsNullOrWhiteSpace(property_path)) {
                if(globals.debug) Console.WriteLine(String.Format("Property path is invalid. {0}:{1}",depth,property_path));
                return new Tuple<Type,object>(null,null);
            }
            if(null==source) {
                if(globals.debug) Console.WriteLine(String.Format("Source null. {0}:{1}",depth,property_path));
                return new Tuple<Type,object>(null,null);
            }
            int node_index=property_path.IndexOf('.');
            string property_node;
            //if(globals.debug) Console.WriteLine(String.Format("Node break is {0}",node_index));
            if(node_index<0) {
                property_node=property_path;
                property_path=null;
                //if(globals.debug) Console.WriteLine(String.Format("Last node in property path. {0}:{1}",depth,property_node));
            } else {
                if(node_index<=1) {
                    if(globals.debug) Console.WriteLine(String.Format("Invalid node. {0}:{1}",depth,property_path));
                    return new Tuple<Type,object>(null,null);
                }
                property_node=property_path.Substring(0,node_index);
                property_path=property_path.Substring(node_index+1);
            }

            Type source_type=source.GetType();
            if(null==source_type) {
                if(globals.debug) Console.WriteLine(String.Format("source type not found. {0}:{1}",depth,property_node));
                return new Tuple<Type,object>(null,null);
            }
            PropertyInfo pi=source_type.GetProperty(property_node);
            if(null==pi) {
                Console.WriteLine(String.Format("PropertyInfo Null at depth {0}:{1} -> {2}",depth,property_node,source_type));
                return new Tuple<Type,object>(null,null);
            }
            object source_node=pi.GetValue(source);
            if(null==source_node) {
                // Console.WriteLine(String.Format("Found a NULL at depth {0}:{1}",depth,property_node));
                return new Tuple<Type,object>(pi.PropertyType,null);
            }
            if(property_path!=null) {
                return get_object_from_path(property_path,source_node,depth++); 
            } 
            //Console.WriteLine(String.Format("Found depth {0}:{1}",depth,property_node));
            return new Tuple<Type,object>(pi.PropertyType,source_node);
        }//end functions
    }//end class
}//en d namespace
