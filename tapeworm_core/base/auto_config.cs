using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Reflection.Metadata.Ecma335;

namespace tapeworm_core{
    public class auto_config {
        public auto_config(string data_dir,string config_dir,delimiters delimiters) {
            string[] files = Directory.GetFiles(data_dir, "*", SearchOption.TopDirectoryOnly);
            Console.WriteLine("File Count"+files.Length.ToString());
            int index=0;
            string[] excluded=new string[]{ 
                ".ksh",".sh",".bat",".json",".yaml",".rpm",".zip",".tar",".bak",".tgz",".gz",".7z",".bak"

            };

            foreach(string file in files) {
                index++;
                string base_file=Path.GetFileName(file);
                string dest_file=Path.Combine(new string[] {config_dir,base_file+".tapeworm.yaml"});
             
                if(file[0]=='.') {
                    Console.WriteLine($"Skipping private . - {file}");
                    continue;
                }
                foreach(string exclude in excluded) {
                    if(file.IndexOf(exclude)>=0) {
                        Console.WriteLine($"Skipping {exclude} - {file}");
                        continue;
                    }
                }

                Console.WriteLine("--");
                Console.WriteLine("File index:"+index.ToString());
                Console.WriteLine(file);
                try{
                    build_config(file,dest_file,delimiters);
                } catch(Exception ex) {
                    Console.WriteLine($"Failed on {file}");
                }
            }
        }

        public bool is_comment(string line,delimiters delimiters){
            int comment_index=-1;
            foreach(char delimiter in delimiters.comment) {
                comment_index=line.IndexOf(delimiter);
                if(comment_index==0) break; //stop trying once a comment delimiter is found
            }

            if(comment_index==0) {
                //Console.WriteLine("COMMENT: " + line);
                return true;
            }
            return false;
        }
        public void build_config(string filename,string dest_file,delimiters delimiters){
            config_file cf=new config_file();

            cf.delimiters=delimiters;
            cf.comments_visible=false;
            cf.empty_lines_visible=false;
            cf.data_starts_on_line=0;
            cf.display=filename;
            cf.active=true;
            cf.errors_visible=false;
            cf.group="";
            cf.multi_search=true;
            cf.name=Path.GetFileName(filename);
            cf.path=filename;
              

            int max_fields=0;
            string[] lines=File.ReadAllLines(filename);
            if(lines.Length==0) {
                Console.WriteLine("Empty file");
                return;
            }
            List<property> properties=new List<property>();
            int control=0;
            //loop all lines and get the maximum column/field count.
            if(!is_comment(lines[0],delimiters)) {
                Console.WriteLine("No comment on first line. ");
                return ;
            }
            foreach(string line in lines) {
                if(string.IsNullOrWhiteSpace(line)) {           //skip whitespace
                    continue;
                }
                if(is_comment(line,delimiters)) {               //skip comments
                    continue;       
                }
                foreach(char c in line) if(c==0) control++;

                string[] temp_fields=line.Split(new char[] { delimiters.field } ); //TODO comment delim
                if(temp_fields.Length>max_fields) max_fields=temp_fields.Length;
            }
            if(control>5) {
                Console.WriteLine("More than 5 control characters. Skipping. ");
                return;
            }

            if(max_fields==0) {
                Console.WriteLine("No properties. Not saving.");
                return;
            }


            //create properties in config 
            for(int i=0;i<max_fields;i++){
                property property=new property();
                property.name           ="field"+i.ToString();
                property.display         =String.Empty;
                property.type            ="string";
                property.@default        =String.Empty;
                property.is_array        =false;
                property.has_default     =false;
                property.ordinal         =i;
                property.visible         =true;
                property.fixed_width     =true;
                property.width           =100;
                property.max_width       =0;
                property.min_width       =0;
                property.overflow        =false;
                property.search          =true;
                property.multi_search    =true;
                property.sort            =true;
                property.sort_ordinal    =0;
                property.sort_default    =false;
                property.sort_default_asc=false;
                property.data_ordinal    =i;
                property.export          =true;
                //property.options           { get; set; }
                properties.Add(property);
            }
            cf.properties=properties.ToArray();

            //determine property type 
            foreach(string line in lines) {
                if(string.IsNullOrWhiteSpace(line)) {           //skip whitespace
                    continue;
                }
                if(is_comment(line,delimiters)) {               //skip comments
                    continue;       
                }

                string[] temp_fields=line.Split(new char[] { delimiters.field } ); //TODO comment

                int index=0;
                foreach(string temp_field in temp_fields) {
                    if(temp_field.IndexOf(delimiters.array.ToString())>=0) {
                        properties[index].is_array=true;
                    }
                    index++;
                }
            }  
            cf.update_ordinals();
           
            config_file.save_yaml(cf,dest_file);
        }
    }
}
