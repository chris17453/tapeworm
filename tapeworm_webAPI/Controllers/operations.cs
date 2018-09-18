using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using tapeworm_core;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Cors;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
using Microsoft.CodeAnalysis;

namespace tapeworm_webAPI.Controllers {
//[EnableCors("SiteCorsPolicy")]
    [Route("api")]
    [ApiController]
    public class queryController:ControllerBase {
        // GET api/values

        private IConfiguration _configuration;

        public queryController(IConfiguration Configuration){
            _configuration = Configuration;
        }


        public class operation_json {
            public string                       uid                 { get; set; }
            public string                       format              { get; set; }="json";
            public bool                         paginate            { get; set; }=true;
            public uint                         page                { get; set; }=0;
            public uint                         page_length         { get; set; }=10;
            public bool                         reload              { get; set; }=false;
            public string                       multi_search        { get; set; }
            public int[][]                      sort                { get; set; }
            public List<KeyValuePair<int,int>>  filter              { get; set; }

            public operation_json(){
            }

        }
        [HttpPost("/fetch"), FormatFilter]
        public async Task<object> operation_POST(operation_json request) {
                if(request.reload)  Program.catalog=Program.catalog.reload();
                options options=new options();
                options.uid          =request.uid; 
                options.multi_search =request.multi_search; 
                options.format       =request.format; 
                options.operation    ="report"; 
                options.paginate     =true; 
                options.page         =request.page; 
                options.page_length  =request.page_length; 
                options.paginate     =request.paginate;
                options.build        =false;  
                options.debug        =Program.catalog.debug;
                options.sort         =request.sort;
                //options.filter       =request.filter;
                if(options.debug) options.verbosity++;
                object output=await Program.catalog.perform_operation(options,true);
                return output;
        }

        [HttpPost("/list")]
        public async Task<object> list_POST() {
            options options=new options();
            options.operation    ="list"; 
            options.debug        =Program.catalog.debug;
            if(options.debug) options.verbosity++;
            object output=await Program.catalog.perform_operation(options,true);
            return output;
        }
        [HttpPost("/config")]
        public async Task<ActionResult<object>> config_POST(options options) {
            options.operation    ="config"; 
            options.debug        =Program.catalog.debug;
            if(options.debug) options.verbosity++;
            ActionResult<object> output=await Program.catalog.perform_operation(options,true);
            return output;
        }




        //http://localhost:5000/operation/machines/format/raw/paginate/true/page/9/length/110/comments/false/blanks/false/errors/false/reload/false/
        [HttpGet("/operation/{type}/format/{format}/paginate/{paginate}/page/{page}/length/{page_length}/reload/{reload}", Name = "Operation")]
        public ActionResult<string> operation(string type,
                                string operation="report",
                                string format="json",
                                bool paginate=false,
                                uint page=0,
                                uint page_length=10,
                                bool reload=false) {
            if(reload) Program.catalog=Program.catalog.reload();
            options options=new options();
            options.type         =type; 
            options.format       =format; 
            options.operation    =operation; 
            options.paginate     =true; 
            options.page         =page; 
            options.page_length  =page_length; 
            options.paginate     =paginate;
            options.build        =false;  
            options.debug        =Program.catalog.debug;
            if(options.debug) options.verbosity++;
            Task<object> output=Program.catalog.perform_operation(options);
            return (string)output.Result;
        }
        //http://localhost:5000/inventory/machines/format/yaml/paginate/true/page/0/length/0/comments/true/blanks/true/errors/true/reload/false/
        [HttpGet("/inventory/{type}/format/{format}/paginate/{paginate}/page/{page}/length/{page_length}/reload/{reload}", Name = "Inventory")]
        [HttpGet]
        public ActionResult<string> inventory(string type,
                                              string operation="report",
                                              string format="none",
                                              bool paginate=false,
                                              uint page=0,
                                              uint page_length=10,
                                              bool reload=false) {
            if(reload) Program.catalog=Program.catalog.reload();

            options options=new options();

            options.type         =type; 
            options.format       =format; 
            options.operation    =operation; 
            options.paginate     =false; 
            options.page         =page; 
            options.page_length  =page_length; 
            options.paginate     =paginate;
            options.build        =false;  
            options.debug        =Program.catalog.debug;
            if(options.debug) options.verbosity++;



            Task<report> r=Program.catalog.get_report(options);
            r.RunSynchronously();
            Dictionary<string,object> inventory    =new Dictionary<string, object>();
            Dictionary<string,object> hosts        =new Dictionary<string, object>();
            Dictionary<string,object> group        =new Dictionary<string, object>();
            Dictionary<string,object> _meta        =new Dictionary<string, object>();
            Dictionary<string,object> hostvars     =new Dictionary<string, object>();
            Dictionary<string,object> all          =new Dictionary<string, object>();
            Dictionary<string,object> ungrouped    =new Dictionary<string, object>();
            Dictionary<string,object> all_children =new Dictionary<string, object>();
            //List<string> groups=new List<string>();
            foreach(record record in r.Result.records){
                if(record.is_data) {
                    string node=record.get_data("node");
                    string metagroup=record.get_data("metagroup");
                    string workgroup=record.get_data("workgroup");
                    //if(groups.IndexOf(metagroup)<0) groups.Add(metagroup);
                    if(!hosts.ContainsKey(node)) {
                        hosts.Add(node,record.model);
                    }
                }
            }
            group.Add("hosts"      ,hosts);
            inventory.Add("group",group);
            //all_children.AddRange(groups);
           // all_children.Add("ungrouped",new object());
           // all.Add("children",all_children);

            //_meta.Add("hostvars"    ,hostvars);
            //inventory.Add("_meta"    ,_meta);
            //inventory.Add("all"      ,all);
            //inventory.Add("ungrouped",ungrouped);

            Task<String> output=Program.catalog.optupt_converter(options.format,inventory); 
            output.RunSynchronously();
            return (string)output.Result;
        }
    }
}
