using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.IO;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;

using System.Linq;
using tapeworm_core.@base;


namespace tapeworm_core  { 
    public class table_base{ 
		public List<record>                 records             { get; set; }=new List<record>();
        [JsonIgnore]
		public List<string>                 errors              { get; set; }=new List<string>();
        //public string                       name                { get; set; }
        public string                       uid                 { get; set; }
        public stat                         stats               { get; set; }=new stat();
        [JsonIgnore]
        public bool                         is_loaded           { get; set; }=false;       
        [JsonIgnore]
        public string                       key                 { get; set; }

        public table_base(){
			
		}
		public table_base(config_file table_config){
            this.uid                =table_config.uid;
            stats.name              =table_config.name;
            stats.file_path         =table_config.path;
            stats.properties        =table_config.properties;
            stats.property_ordinals =table_config.property_ordinals;
		}

		public  stat stat(){
            if(!is_loaded)  load(); //TODO await
            return stats;
		}

        public async Task<report> report(options options) {
            try{
                if(!this.is_loaded) { 
                    await load(); 
                }
            } catch (Exception ex) {
                if(globals.debug) Console.WriteLine(string.Format("load:{0}",ex.Message));
                if(globals.debug) Console.WriteLine("model not defined in load");
                throw new Exception("failed to parse table");
            }
            stat report_stats =stats.Clone();
            report_stats.page       =options.page;
            report_stats.page_length=options.page_length;
            report_stats.returned   =0;
            report_stats.visible    =0;
            report_stats.errors     =0;
            report_stats.blanks     =0;
            report_stats.comments   =0;
            report_stats.records    =0;

            List<Tuple<string,string>> expressions =new List<Tuple<string, string>>();
            foreach(int[] expression in options.sort) {
                //int calculated_ordinal=report_stats.property_ordinals[expression.Key];
                if(globals.debug) Console.WriteLine(String.Format("SORT: {0}:{1}",expression[0],expression[1]));
                int calculated_ordinal=expression[0];
                string name=report_stats.properties[calculated_ordinal].name;
                string dir="";
                if(!report_stats.properties[calculated_ordinal].sort)  {               //we got a bad sort.. lets ignore it 
                    report_stats.errors++;
                    if(globals.debug) Console.WriteLine("Invalid Sort");
                    return new report(report_stats,null,"Invalid Sort");
                }
                if(expression[1]==0) dir="asc";
                if(expression[1]==1) dir="desc";
                if(expression[1]==1 || expression[1]==0) {
                    if(globals.debug) Console.WriteLine(String.Format("{2}.{0}:{1}",name,dir,"model"));
                    expressions.Add(new Tuple<string, string>("model."+name,dir));
                }
            }
            //if(expressions.Count>0) {

            List<Tuple<string,string,string,string>> filters=new   List<Tuple<string,string,string,string>>();
            if(!string.IsNullOrWhiteSpace(options.multi_search)) {
                foreach(property p in report_stats.properties) {
                    if(p.visible && p.multi_search) filters.Add(new Tuple<string,string,string,string>("model."+p.name,"=",options.multi_search,"multi"));
                }

            }

            List<record> computed_records_filter=records.NCDB_Where(filters).ToList();
            List<record> computed_records=computed_records_filter.NCDB_OrderBy(expressions).ToList();
            //}
            if(null!=computed_records) report_stats.records=(uint)computed_records.Count;

            uint target_start_index  =0;
            uint start_index         =0;
            if(report_stats.page_length==0) target_start_index=0;
            else                     target_start_index=report_stats.page*report_stats.page_length;
            //The length is variable based on what we are hiding...
            for(uint i=0;i<report_stats.records;i++) {
                record item=computed_records[(int)i];
 
                if(item.is_error)   report_stats.errors++;
                if(item.is_blank)   report_stats.blanks++;
                if(item.is_comment) report_stats.comments++;
                if((!globals.models[uid].errors_visible      && item.is_error )   ||
                   (!globals.models[uid].empty_lines_visible && item.is_blank )   ||
                   (!globals.models[uid].comments_visible    && item.is_comment)) continue;

                report_stats.visible++; 
                if(report_stats.visible==target_start_index)  start_index=i;
            }
            if(report_stats.page_length>0) {
                if(report_stats.visible<report_stats.page_length) report_stats.pages=1;
                report_stats.pages        =(report_stats.visible+ report_stats.page_length- 1) / report_stats.page_length;
                if(report_stats.pages>0 && report_stats.page>=report_stats.pages) report_stats.page=report_stats.pages-1;            //default to last page if past the end of the array
            } else {
                report_stats.pages=1;
            }
            if(report_stats.visible==0) report_stats.pages=0;
            try{


                List<record> page_of_computed_records=new List<record>();

                for(uint i=start_index;i<report_stats.records;i++){
                    record item=computed_records[(int)i];
                    if(null==item) {
                        if(globals.debug) Console.WriteLine("record null");
                        continue;
                    }

                    if((!globals.models[uid].errors_visible      && item.is_error )   ||
                       (!globals.models[uid].empty_lines_visible && item.is_blank )   ||
                       (!globals.models[uid].comments_visible    && item.is_comment)) continue;
                    if(report_stats.page_length>0 && report_stats.returned>=report_stats.page_length) break;
                    report_stats.returned++;
                    page_of_computed_records.Add(item);
                    report_stats.record_end=i;
                }

                report returned_report=new report(report_stats,page_of_computed_records);
                return returned_report;
            } catch (Exception ex) {
                if(globals.debug) Console.WriteLine(String.Format("report: record transfer failed. {0}",ex.Message));
                report returned_report=new report(report_stats,null);
                returned_report.stats.errors++;
                return returned_report;
            }
        }



        public bool update(){
            return false;
        }

		public bool save() {
			try{
                string file_path=globals.models[uid].path;
				DateTime time=DateTime.Now;
                string backup_file_path=string.Format("{0}.bak.{1},",file_path,time.Ticks);
                System.IO.File.Copy(file_path,backup_file_path, true);
                
                if(globals.debug) Console.WriteLine(file_path);
				using(FileStream fs = new FileStream(file_path, FileMode.Create, FileAccess.Write, FileShare.None)){
        			StreamWriter sw= new  StreamWriter(fs);  
					int index=0;
        			foreach(record record in records) {
						sw.WriteLine(record.ToString());
						index++;
        			}
					sw.Flush();                                                 //because this is obvious.
                }
    			return true;
			} catch (Exception ex) {
				if(globals.debug)  Console.WriteLine(ex.Message);
                return false;
			}
        }

        public bool duplicate_record_exists(int? hash1) {
            if(null==records || records.Count==0) return false;             //nothing exists yet..
            if(string.IsNullOrWhiteSpace(key)) return false;                //no key is set
            foreach(record record in records){
                if(record.is_data) {
                    if(hash1==record.key_hash) return true;
                }
            }
            return false;
        }

        public async Task<bool> load() {
            try{
                string file_path=globals.models[uid].path;
                if(!globals.models.ContainsKey(uid) || globals.models[uid].class_type==null) {
                    Console.WriteLine("model not defined in load");
                    return false;
                }
                stats.lines=0;
                stats.load_start=DateTime.Now.Ticks;
                string line;  
                uint index=0;
                if(globals.debug) {
                    Console.WriteLine($"Reading: {file_path}" );
                }
                if(!File.Exists(file_path)) {
                    Console.WriteLine($"Data file does not exist: {file_path}" );
                    return false;
                }
                System.IO.StreamReader file = new System.IO.StreamReader(file_path);
                if(null==file) {
                    Console.WriteLine($"Cannot open file: {file_path}" );
                    return false;
                }

                bool has_key=!string.IsNullOrWhiteSpace(key);

                if(globals.debug) {
                    Console.WriteLine($"Key is '{key}' : {has_key}" );
                }

                while((line =await file.ReadLineAsync()) != null) {  
        				index++;

        				record record=null;
        				try{
                            if(globals.models[uid].data_starts_on_line>index) {
                                record=new record(uid,key,index,line,file_path,true);
                                Console.WriteLine(String.Format("Skipping Line: {0}",index));
                            } else {
                                record=new record(uid,key,index,line,file_path,false);
                            }
                         } catch (Exception ex) {
                            record=new record(uid,key,index,line,file_path,ex.Message,true);
                            errors.Add(ex.Message);
                            if(globals.debug) {
                                //  if(errors.Count<debug_limit) {
                                Console.WriteLine("table parsing error: "+ex.Message);
                                //  }
                            }
                        }
                    /* TODO DUPE ERR
                        try{         
                            if(has_key && record.is_data) { 
                                int? hash=record.key_hash;
                                if(null==hash) {
                                    //  throw new Exception(string.Format("Key '{0}': is null ",key));
                                }
                                if(null!=records && records.Count>0) {
                                    record dupe=records.Find((r) =>r.key_hash==hash);
                                    if(null!=dupe) {
                                        record.is_error=true;
                                        //  throw new Exception(string.Format("Duplicate record on key {0}:{1}",key,hash));                               
                                    }
                                }
                            }
                        } catch (Exception ex) {
                            if(globals.debug) {
                            Console.WriteLine("table parsing error-dupe lookup: "+ex.Message);
                        }
                        */

                       records.Add(record);	
        			}
                    //records.Sort((record x, record y) => x.line.CompareTo(y.line)); //order the list.. if usint parallel-ize
                    is_loaded=true;
        			file.Close();
                stats.lines=index;
                if(globals.debug) Console.WriteLine("Parsed");
                stats.load_end=DateTime.Now.Ticks;
    			return true;
            } catch (Exception ex){
                is_loaded=false;
                stats.load_end=DateTime.Now.Ticks;
                Console.WriteLine("Loading data: "+ex.Message);
				return false;
			}
            
        }//end load

       
    }//end class
}//end namespace
