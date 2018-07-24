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
            

        public template(string file_name,string ns,string class_name,property[] types,bool build=false) {
            if(false==build) {
                if(load_assembly(file_name,ns,class_name)) {
                    return;
                }
            }
            if(globals.debug) Console.WriteLine(string.Format("Building: {0}",file_name));
            StringBuilder properties=new StringBuilder();
            StringBuilder properties_switch=new StringBuilder();
            foreach(property item in types) {
                string type=item.type;
                if(item.is_array) {
                    type="List<"+type+">";
                }
                properties_switch.AppendLine(string.Format("          case \"{0}\": return this.@{0}; ",item.name));
                if(item.has_default) {
                    properties.AppendLine(string.Format("      public {1,15} @{0,-30} {{ get; set; }}={2}",item.name,type,item.@default));
                } else {
                    properties.AppendLine(string.Format("      public {1,15} @{0,-30} {{ get; set; }}",item.name,type));
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
{3}
            default: return null;
        }}
    }}
}}
}}",ns,class_name,properties.ToString(),properties_switch.ToString());
//            Console.Write(class_template);
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(class_template);
            string assemblyName = Path.GetFileName(file_name);
//            Console.WriteLine(assemblyName);
//            Console.WriteLine(Path.GetFullPath(file_name));


            List<MetadataReference>  references =new List<MetadataReference> ();CollectReferences();

            references.Add(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));
            references.Add(MetadataReference.CreateFromFile(typeof(Object).Assembly.Location));
            references.Add(MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location));
            references.Add(MetadataReference.CreateFromFile(typeof(tapeworm_core.irecord_helper).Assembly.Location));
            references.Add(MetadataReference.CreateFromFile(Assembly.Load(new AssemblyName("System.Runtime")).Location));
            references.Add(MetadataReference.CreateFromFile(Assembly.Load(new AssemblyName("mscorlib")).Location));

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

