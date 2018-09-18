using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Emit;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Diagnostics;

namespace tapeworm_core  { 
    public class template {
        public Type class_type { get; set; }
        //public bool debug      { get; set; }
        /*
         * 
         * New hotness for security..
            var domain = AppDomain.CreateDomain("NewDomainName");
            var t = typeof(TypeIWantToLoad);
            var runnable = domain.CreateInstanceFromAndUnwrap(@"C:\myDll.dll", t.Name) as IRunnable;
            if (runnable == null) throw new Exception("broke");
            runnable.Run();

        */


        public bool load_assembly(string file_name,string ns,string class_name){
            if(!File.Exists(file_name)) {
                return false;
            }
            if(globals.debug) Console.WriteLine(string.Format("Loading: {0}",file_name));

            Assembly assembly = Assembly.LoadFile(file_name);
            class_type= assembly.GetType(ns+"."+class_name);
            return true;
        }
            

		public template(string file_name,string compile_dir,string source,string ns,string class_name,property[] types,bool build=false) {
            if(false==build) {
                if(load_assembly(file_name,ns,class_name)) {
                    return;
                }
            }
            if(globals.debug) Console.WriteLine(string.Format("Building: {0}",file_name));
            StringBuilder properties=new StringBuilder();
            StringBuilder properties_switch = new StringBuilder();
            StringBuilder set_properties = new StringBuilder();


            int max_name = 0;
			int max_type= 0;
			foreach (property item in types) {
				if (item.name.Length > max_name) max_name = item.name.Length;
			}
				max_name += 2;
				max_type = max_name + 6;
			foreach(property item in types) {
                
				string type=item.type;

				if (!string.IsNullOrWhiteSpace(item.bind_source)) {          //its a class
					string[] model = item.bind_source.Split(".");
					if (model.Length > 1) {
						//Console.WriteLine($"Complex object {type}");
						type = model[0];
					} else {
						//Console.WriteLine("Error converting complex object");
						type = item.bind_source;
					}
					item.has_default=false;
/*                    if(item.is_array) { 
                        set_properties.AppendLine(string.Format("             case \"{0,-" + max_name + "}: @{1}=value; break;", item.name + '"', create_set_value(type, item.name)));
                    } else {
                        set_properties.AppendLine(string.Format("             case \"{0,-" + max_name + "}: @{1}.Add(value); break;", item.name + '"', create_set_value(type, item.name)));
                    }
                    */
                } 
                set_properties.AppendLine(string.Format("             case \"{0,-" + max_name + "}: {1} break;", item.name + '"', create_set_value(type, item.name,item.is_array)));
                
                if (item.is_array)
				{
					type = "List<" + type + ">";
                    item.@default=$"new {type}();";
                    item.has_default=true;

                }
                properties_switch.AppendLine(string.Format("             case \"{0,-" + max_name + "}: return this.@{1}; ", item.name + '"', item.name));
                if (item.has_default) {
					properties.AppendLine(string.Format("      public {1," + max_type + "} @{0,-" + max_name + "} {{ get; set; }}={2}",item.name,type,item.@default));
                } else {
					properties.AppendLine(string.Format("      public {1," + max_type + "} @{0,-" + max_name + "} {{ get; set; }}",item.name,type));
                }
            }
            
            string class_template=string.Format(
@"
namespace {0}{{
  using System;
  using System.Runtime;
  using tapeworm_core;
  using System.Collections.Generic;

  public class {1} : record_helper {{
{2}
      public {1}(){{
      }}

      public override object get_value(string property){{
         switch(property) {{
{3}             default: return null;
         }}
      }}//end get_value
      public override bool set_value(string property,string value,bool is_array){{
         try{{
            switch(property) {{
{4}                default: break;
            }}
            return true;
          }} catch (Exception ex) {{ 
          //  throw new Exception(""Error setting value[{1}] :"" + ex.Message);
          }}
          return false;
      }}//end set_value
  }}
}}", ns,class_name,properties.ToString(),properties_switch.ToString(),set_properties.ToString());
//            Console.Write(class_template);
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(class_template);
            string assemblyName = Path.GetFileName(file_name);
			//            Console.WriteLine(assemblyName);
			//            Console.WriteLine(Path.GetFullPath(file_name));

			File.WriteAllText(source, class_template);

            List<MetadataReference>  references =new List<MetadataReference> ();CollectReferences();

            references.Add(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));
            references.Add(MetadataReference.CreateFromFile(typeof(Object).Assembly.Location));
            references.Add(MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location));
            references.Add(MetadataReference.CreateFromFile(typeof(tapeworm_core.irecord_helper).Assembly.Location));
            references.Add(MetadataReference.CreateFromFile(Assembly.Load(new AssemblyName("System.Runtime")).Location));
            references.Add(MetadataReference.CreateFromFile(Assembly.Load(new AssemblyName("mscorlib")).Location));

			foreach (property item in types) {
				if (!string.IsNullOrWhiteSpace(item.bind_source)) {          //its a class
					string[] model = item.bind_source.Split(".");
					if (model.Length > 1) {
						references.Add(MetadataReference.CreateFromFile(Path.Combine(new string[] { compile_dir, model[0] + ".dll" })));
					} else {
						references.Add(MetadataReference.CreateFromFile(Path.Combine(new string[] { compile_dir, item.bind_source + ".dll" })));
					}
				}
			}

            CSharpCompilation compilation = CSharpCompilation.Create(
            assemblyName,
            syntaxTrees: new[] { syntaxTree },              references: references.ToArray(),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            EmitResult result = compilation.Emit(Path.GetFullPath(file_name));
                if (!result.Success) {
                    IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic => 
                                                                    diagnostic.IsWarningAsError || 
                                                                    diagnostic.Severity == DiagnosticSeverity.Error);

                    foreach (Diagnostic diagnostic in failures) {
                    if(globals.debug) Console.Error.WriteLine("{0}: {1}", diagnostic.Id, diagnostic.GetMessage());
                    }
                } else {
                    if(!load_assembly(file_name,ns,class_name)) {
                        throw new Exception(string.Format("Cannot load assembly {0}",file_name));
                    }
                }
         }//end function


        public string create_set_value(string type,string name,bool is_array) {
            string o="";
            if(is_array==false) { 
                switch (type) {
                    //value types
                    case "sbyte"          : o=$"this.@{name}=Convert.ToSByte(value);   "; break;
                    case "short"          : o=$"this.@{name}=Convert.ToInt16(value);   "; break;
                    case "int"            : o=$"this.@{name}=Convert.ToInt32(value);   "; break;
                    case "long"           : o=$"this.@{name}=Convert.ToInt64(value);   "; break;
                    case "byte"           : o=$"this.@{name}=Convert.ToByte(value);    "; break;
                    case "ushort"         : o=$"this.@{name}=Convert.ToUInt16(value);  "; break;
                    case "uint"           : o=$"this.@{name}=Convert.ToUInt32(value);  "; break;
                    case "ulong"          : o=$"this.@{name}=Convert.ToUInt64(value);  "; break;
                    case "decimal"        : o=$"this.@{name}=Convert.ToDecimal(value); "; break;
                    case "char"           : o=$"this.@{name}=Convert.ToChar(value);    "; break;
                    case "bool"           : o=$"this.@{name}=Convert.ToBoolean(value); "; break;
                    //nullable            
                    case "string"         : o=$"this.@{name}=value; break;"; break;
                    //nullable vlauetypes
                    case "sbyte?"         : o=$"if(null==value) {{ this.@{name}=null; }} else {{ this.@{name}=Convert.ToSByte(value);  }} "; break;
                    case "short?"         : o=$"if(null==value) {{ this.@{name}=null; }} else {{ this.@{name}=Convert.ToInt16(value);  }} "; break;
                    case "int?"           : o=$"if(null==value) {{ this.@{name}=null; }} else {{ this.@{name}=Convert.ToInt32(value);  }} "; break;
                    case "long?"          : o=$"if(null==value) {{ this.@{name}=null; }} else {{ this.@{name}=Convert.ToInt64(value);  }} "; break;
                    case "byte?"          : o=$"if(null==value) {{ this.@{name}=null; }} else {{ this.@{name}=Convert.ToByte(value);   }} "; break;
                    case "ushort?"        : o=$"if(null==value) {{ this.@{name}=null; }} else {{ this.@{name}=Convert.ToUInt16(value); }} "; break;
                    case "uint?"          : o=$"if(null==value) {{ this.@{name}=null; }} else {{ this.@{name}=Convert.ToUInt32(value); }} "; break;
                    case "ulong?"         : o=$"if(null==value) {{ this.@{name}=null; }} else {{ this.@{name}=Convert.ToUInt64(value); }} "; break;
                    case "decimal?"       : o=$"if(null==value) {{ this.@{name}=null; }} else {{ this.@{name}=Convert.ToDecimal(value);}} "; break;
                    case "char?"          : o=$"if(null==value) {{ this.@{name}=null; }} else {{ this.@{name}=Convert.ToChar(value);   }} "; break;
                    case "bool?"          : o=$"if(null==value) {{ this.@{name}=null; }} else {{ this.@{name}=Convert.ToBoolean(value);}} "; break;
                }
            }
            if (is_array == true) { 
                switch (type) {
                    //List types 
                    case "sbyte"    : o=$"if(null==this.@{name}) return false; this.@{name}.Add(Convert.ToSByte  (value )); "; break;
                    case "short"    : o=$"if(null==this.@{name}) return false; this.@{name}.Add(Convert.ToInt16  (value )); "; break;
                    case "int"      : o=$"if(null==this.@{name}) return false; this.@{name}.Add(Convert.ToInt32  (value )); "; break;
                    case "long"     : o=$"if(null==this.@{name}) return false; this.@{name}.Add(Convert.ToInt64  (value )); "; break;
                    case "byte"     : o=$"if(null==this.@{name}) return false; this.@{name}.Add(Convert.ToByte   (value )); "; break;
                    case "ushort"   : o=$"if(null==this.@{name}) return false; this.@{name}.Add(Convert.ToUInt16 (value )); "; break;
                    case "uint"     : o=$"if(null==this.@{name}) return false; this.@{name}.Add(Convert.ToUInt32 (value )); "; break;
                    case "ulong"    : o=$"if(null==this.@{name}) return false; this.@{name}.Add(Convert.ToUInt64 (value )); "; break;
                    case "decimal"  : o=$"if(null==this.@{name}) return false; this.@{name}.Add(Convert.ToDecimal(value )); "; break;
                    case "char"     : o=$"if(null==this.@{name}) return false; this.@{name}.Add(Convert.ToChar   (value )); "; break;
                    case "bool"     : o=$"if(null==this.@{name}) return false; this.@{name}.Add(Convert.ToBoolean(value )); "; break;
                    //nullable
                    case "string"   : o=$"if(null==this.@{name}) return false; @{name}.Add(value);  break;";break;
                    //nullable vlauetypes
                    case "sbyte?"   : o=$"if(null==value || null==this.@{name}) {{ this.@{name}.Add(null); }} else {{ this.@{name}.Add(Convert.ToSByte  (value )); }} "; break;
                    case "short?"   : o=$"if(null==value || null==this.@{name}) {{ this.@{name}.Add(null); }} else {{ this.@{name}.Add(Convert.ToInt16  (value )); }} "; break;
                    case "int?"     : o=$"if(null==value || null==this.@{name}) {{ this.@{name}.Add(null); }} else {{ this.@{name}.Add(Convert.ToInt32  (value )); }} "; break;
                    case "long?"    : o=$"if(null==value || null==this.@{name}) {{ this.@{name}.Add(null); }} else {{ this.@{name}.Add(Convert.ToInt64  (value )); }} "; break;
                    case "byte?"    : o=$"if(null==value || null==this.@{name}) {{ this.@{name}.Add(null); }} else {{ this.@{name}.Add(Convert.ToByte   (value )); }} "; break;
                    case "ushort?"  : o=$"if(null==value || null==this.@{name}) {{ this.@{name}.Add(null); }} else {{ this.@{name}.Add(Convert.ToUInt16 (value )); }} "; break;
                    case "uint?"    : o=$"if(null==value || null==this.@{name}) {{ this.@{name}.Add(null); }} else {{ this.@{name}.Add(Convert.ToUInt32 (value )); }} "; break;
                    case "ulong?"   : o=$"if(null==value || null==this.@{name}) {{ this.@{name}.Add(null); }} else {{ this.@{name}.Add(Convert.ToUInt64 (value )); }} "; break;
                    case "decimal?" : o=$"if(null==value || null==this.@{name}) {{ this.@{name}.Add(null); }} else {{ this.@{name}.Add(Convert.ToDecimal(value )); }} "; break;
                    case "char?"    : o=$"if(null==value || null==this.@{name}) {{ this.@{name}.Add(null); }} else {{ this.@{name}.Add(Convert.ToChar   (value )); }} "; break;
                    case "bool?"    : o=$"if(null==value || null==this.@{name}) {{ this.@{name}.Add(null); }} else {{ this.@{name}.Add(Convert.ToBoolean(value )); }} "; break;
                    default : //o=$"{name}=value; break; "; 
                        break;
                }
            }
            return o;
        }

        private static List<MetadataReference> CollectReferences(){
            var assemblies = new HashSet<Assembly>();
            Collect(Assembly.Load(new AssemblyName("netstandard")));
            //Collect(typeof(Uri).Assembly);
            var result = new List<MetadataReference>(assemblies.Count);
            foreach (var assembly in assemblies){
                result.Add(MetadataReference.CreateFromFile(assembly.Location));
            }
            return result;

            void Collect(Assembly assembly){
                if (!assemblies.Add(assembly)){
                    return;
                }
                var referencedAssemblyNames = assembly.GetReferencedAssemblies();
                foreach (var assemblyName in referencedAssemblyNames){
                    var loadedAssembly = Assembly.Load(assemblyName);
                    assemblies.Add(loadedAssembly);
                }
            }
        }//

    }//end class
}//end namespace

