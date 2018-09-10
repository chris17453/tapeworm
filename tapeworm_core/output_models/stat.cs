using System;
using System.Collections.Generic;
using System.Text;

namespace tapeworm_core  { 
    public class stat {             
        public string                name                { get; set; } 
        public string                file_path           { get; set; } 
        public uint                  records             { get; set; } 
        public uint                  lines               { get; set; } 
        public uint                  visible             { get; set; } 
        public uint                  errors              { get; set; } 
        public uint                  blanks              { get; set; } 
        public uint                  comments            { get; set; } 
        public uint                  page                { get; set; }
        public uint                  page_length         { get; set; }
        public uint                  returned            { get; set; }
        public uint                  pages               { get; set; }
        public uint                  record_start        { get; set; }
        public uint                  record_end          { get; set; }
        public string                time                { get; set; }=DateTime.Now.ToLongTimeString();
        public long                  load_time           { get; set; }=0;
        public long                  load_start          { get; set; }=0;
        public long                  load_end            { get; set; }=0;
        public property[]            properties          { get; set; }
        public Dictionary<int,int>   property_ordinals   { get; set; }

         


        public stat() {
        }
        /*public stat(string name,string file_path,uint records,uint errors,uint blanks,uint comments,uint visible,uint page,uint page_length,uint pages,uint returned,uint record_start,uint record_end) {
            this.name          =name;
            this.file_path     =file_path;
            this.records       =records;
            this.errors        =errors;
            this.blanks        =blanks;
            this.comments      =comments;
            this.visible       =visible;
            this.page          =page;
            this.page_length   =page_length;
            this.pages         =pages;
            this.returned      =returned;
            this.record_start  =record_start;
            this.record_end    =record_end;
        }*/

        public override string ToString() {
            load_time=load_end-load_start;
            StringBuilder output=new StringBuilder();
            output.AppendLine(string.Format("__[stats]________________________"));
            output.AppendLine(String.Format(" Time               : {0}",time));
            output.AppendLine(String.Format(" Load Time          : {0}",load_time));
            output.AppendLine(String.Format(" Type               : {0}",name));
            output.AppendLine(String.Format(" Path               : {0}",file_path));
            output.AppendLine(String.Format(" Lines in file      : {0}",lines));
            output.AppendLine(String.Format(" Records (all)      : {0}",records));
            output.AppendLine(String.Format(" Visible            : {0}",visible));
            output.AppendLine(String.Format(" Blanks             : {0}",blanks));
            output.AppendLine(String.Format(" Comments           : {0}",comments));
            output.AppendLine(String.Format(" Errors             : {0}",errors));
            output.AppendLine(String.Format(" Pages              : {0}",pages));
            output.AppendLine(String.Format(" Page               : {0}",page));
            output.AppendLine(String.Format(" Page Length        : {0}",page_length));
            output.AppendLine(String.Format(" Returned           : {0}",returned));
            output.AppendLine(String.Format(" Records            : {0}-{1}",record_start,record_end));
            output.AppendLine(String.Format(" Field Delimiter    : {0}",globals.models[name].delimiters.field));
            output.AppendLine(String.Format(" Array Delimiter    : {0}",globals.models[name].delimiters.array));
            foreach(char delimiter in globals.models[name].delimiters.comment) {
                output.AppendLine(String.Format(" Commment Delemiter : {0}",delimiter.ToString()));
            }
            output.AppendLine(String.Format(" First row of data  : {0}",globals.models[name].data_starts_on_line));
            output.AppendLine(string.Format("________________________________"));
            output.AppendLine("");

            return output.ToString();
        }

        public stat  Clone() {
            stat clone=new stat();

            clone.name              =this.name;
            clone.file_path         =this.file_path;
            clone.records           =this.records;
            clone.lines             =this.lines;
            clone.visible           =this.visible;
            clone.errors            =this.errors;
            clone.blanks            =this.blanks; 
            clone.comments          =this.comments;
            clone.page              =this.page;
            clone.page_length       =this.page_length;
            clone.returned          =this.returned;
            clone.pages             =this.pages;
            clone.record_start      =this.record_start;
            clone.record_end        =this.record_end;
            //clone.time              =new string(this.time.ToCharArray());
            clone.load_time         =this.load_time;   
            clone.load_start        =this.load_start;
            clone.load_end          =this.load_end;
								    
            clone.properties        =this.properties;
            clone.property_ordinals =this.property_ordinals; 

            return clone;
        }
    }
}