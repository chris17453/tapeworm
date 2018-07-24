/*

Experimental yaml builder. Did conversions by hand.

*/


/*using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Internal;

namespace tapeworm{

    public class column{
        public string name     { get; set; }
        public string display  { get; set; }
        public string sorting  { get; set; }
        public int    ordinal  { get; set; }
        public string type     { get; set; }
        public string @default { get; set; }
    }

    public class table{
        public Dictionary<string,string> headers  {get; set; }=new Dictionary<string, string>();
        public string       name          { get; set; }
        public string       path          { get; set; }
        public string       regex         { get; set; }
        public string       description   { get; set; }
        public string       id            { get; set; }
        public int          fields        { get; set; }
        public List<column> columns       { get; set; }=new List<column>();
    }
    public class db {
        public List<table> tables { get; set; }=new List<table>();
    }
    public class generate_mapping {
        string folder_path {get; set; }
        public generate_mapping(string folder_path) {
            this.folder_path=folder_path;
        }
        public db build_db() {
            string[] files = Directory.GetFiles(folder_path, "*", SearchOption.TopDirectoryOnly);
            db new_db=new db();
            foreach(string file in files) {
                string name=file.ToLower().Replace(".","");
                table t=map_file(string.Format("{0},{1}",folder_path,file),file,name);
                if(null!=t) {
                    new_db.tables.Add(t);
                }
            }
            return new_db;
        }

        public table map_file(string path,string file,string name) {
//             The Rules......
//             comments in header
//             beginning of file is header until a non # is seen
//             everything before a : in a headder is a field, everything after is the value
//
//             #=comment, but only on character 0
//             :=field seperator
//             ,=array seperator
//             blank=ignored
//            /
            if(name.IndexOf(".txt")>=0) return null;                                        //skip text files
        
            table t=new table();
            int[] fields=new int[256];
            for(int i=0;i<256;i++) fields[i]=0;

            const Int32 BufferSize = 128;
            using (var fileStream = File.OpenRead(file))

            using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, BufferSize)) {
                String line;
                bool is_header     =true;
                bool is_comment    =true;
                bool is_blank      =true;
                bool is_data       =false;
                bool proccess_this =false;
                string description="";
                string regex      ="";
                string id         ="";

                string key="";
                string value="";
                while ((line = streamReader.ReadLine()) != null){
                    if(line.Length>0 && line[0]=='#') is_comment=true; else is_comment=false;
                    if(string.IsNullOrWhiteSpace(line)) is_blank=true; else is_blank=false;
                    if(!is_comment && is_header) is_header=false;
                    if(!is_comment && !is_blank) is_data=true; else is_data=false;

                    //do the attributes
                    if(is_header) {
                        string[] tokens=line.Split(':',2);
                        if(tokens.Count()>1) {                //Got a split
                            string key1=tokens[0].Substring(1).Trim();
                            value=tokens[1].Trim();
                            if(!string.IsNullOrWhiteSpace(key1) && !string.IsNullOrWhiteSpace(value)) {
                                key=key1;
                                if(!t.headers.ContainsKey(key1)) {
                                    t.headers.Add(key1,value);
                                } else {
                                    t.headers[key1]+=value;
                                }
                            } else {
                                if(t.headers.Count==0) continue;
                                t.headers[key]+=line.Substring(1).Trim();
                            }

                        } else {
                            if(t.headers.Count==0) continue;
                            t.headers[key]+=line.Substring(1).Trim();
                        }
                        
                    }

                    //Do the data
                    if(!is_header) {
                        if(is_data) {
                            if(line.Trim()=="fi") return null;                                      ///skip stuff with bash code.
                            if(line.IndexOf("export")==0) return null;                               ///skip stuff with bash code.

                            string[] tokens=line.Split(":");
                            if(tokens.Count()>0 && tokens.Count()<256)  fields[tokens.Count()]++;
                        }
                    }
                }//end loop of reading file

                //post analysis
                int field_count=0;
                for(int i=0;i<256;i++) {
                    if(fields[i]>field_count) field_count=i;
                }

                t.name=file;
                t.path=path;
                t.name=name;
                t.regex=regex;
                t.description=description;
                t.id=id;
                t.fields=field_count;
                if(field_count==0) return null;


                for(int i=0;i<field_count;i++) {
                    column c=new column();
                    c.name="field_"+i.ToString();
                    c.ordinal=i;
                    t.columns.Add(c);
                }



            }
            return t;
        }//end map 
    }//end class
}//end namespace
*/