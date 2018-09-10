using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace tapeworm_core  { 
    public class options{ 
        public string                         uid               { get; set; }="";
        public string                         type              { get; set; }="";
        public string                         format            { get; set; }="raw";
        public string                         operation         { get; set; }="";
        public bool                           help              { get; set; }=false;
        public bool                           debug             { get; set; }=false;     //debug
        public uint                           verbosity         { get; set; }=0;         //debug
        public uint                           max_error         { get; set; }=10;        //debug
        public bool                           paginate          { get; set; }=false;     //pagination
        public uint                           page              { get; set; }=0;         //pagination
        public uint                           page_length       { get; set; }=10;        //pagination
        public int[][]                        sort              { get; set; }
        public string                         multi_search      { get; set; }
        public List<KeyValuePair<int,string>> filter            { get; set; }=new List<KeyValuePair<int, string>>();
        public uint                           error             { get; set; }=0;
        public List<string>                   types             { get; set; }=globals.types; //loaded from?
        public string[]                       operations        { get; set; }=new string[] { "report","stats" };    // TODO "add","edit","delete","edit","search",
        public string[]                       formats           { get; set; }=new string[] { "none","raw","json","yaml" };    
        public string                         profile           { get; set; }="flat_config";    
        public bool                           build             { get; set; }=false;
        public string                         data_dir          { get; set; }
        public string                         config_dir        { get; set; }
        public delimiters                     delimiters        { get; set; }=new delimiters();

        public override string ToString() {
            StringBuilder output=new StringBuilder();
            output.AppendLine(string.Format("__[options]______________________"));
            output.AppendLine(string.Format(" type         :{0}",type         ));
            output.AppendLine(string.Format(" format       :{0}",format       ));
            output.AppendLine(string.Format(" operation    :{0}",operation    ));
            output.AppendLine(string.Format(" help         :{0}",help         ));
            output.AppendLine(string.Format(" debug        :{0}",debug        ));
            output.AppendLine(string.Format(" verbosity    :{0}",verbosity    ));
            output.AppendLine(string.Format(" max_error    :{0}",max_error    ));
            output.AppendLine(string.Format(" paginate     :{0}",paginate     ));
            output.AppendLine(string.Format(" page         :{0}",page         ));
            output.AppendLine(string.Format(" page_length  :{0}",page_length  ));
//            output.AppendLine(string.Format(" parameters   :{0}",string.Join(",",parameters.Select(x => string.Format("{0}{1}{2}", x.Key, ':', x.Value)))));
            output.AppendLine(string.Format(" error        :{0}",error        ));
            output.AppendLine(string.Format(" types        :{0}",string.Join(",",types.ToArray())));
            output.AppendLine(string.Format(" operations   :{0}",string.Join(",",operations.ToArray())));
            output.AppendLine(string.Format(" formats      :{0}",string.Join(",",formats.ToArray())));
            output.AppendLine(string.Format(" profile      :{0}",profile     ));
            output.AppendLine(string.Format(" build        :{0}",build       ));
            output.AppendLine(string.Format(" data_dir     :{0}",data_dir    ));
            output.AppendLine(string.Format(" config_dir   :{0}",config_dir  ));
            output.AppendLine(string.Format("_________________________________"));
            return output.ToString();
        }
    }
}
