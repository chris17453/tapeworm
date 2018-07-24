(function ( $ ) {
    $.fn.tapeworm = function( options ) {
		var settings = $.extend({
		   controller_name     : "tapeworm",
		   api_target          : "/api/",                       //the controller url for the webAPI
		   success_function    : null,
		   cancel_function     : null,
		   dialog_function     : null,                          //for a custom message call back
		   api_error           :"API failure",					//generic api error message
		   no_results_msg      :"No results found",
		   use_zebra_rows      : true
		}, options );

		var id;
		var JWT;
		var element;
		
		//********************************************************************************************
		//Init of internal plugin
		//********************************************************************************************
		var tapeworm= function(element,options){
			this.options          =options;                              //options for the plugin
			this.api_target       =options.api_target;                   //the controller url for the webAPI
			this.controller_name  =options.controller_name;              //the controller name on the webapi
			this.element          =element;                              //the curent html element
			this.id=this.build_uid( 'tapeworm');
			this.data_stream_uid  ="et_machines";
			this.JWT="";
			this.build();                                                //create the html and inject it into the element.
			this.get_list();											 //get the list of methods and groups
			this.bind();                                                 //attach events to newly created HTML
			//this.load(options.path,options.filters);                     //load and populate the initial data for the file browser.
		};
        //user defined functions
		
		tapeworm.prototype.build_uid=function(hash="X"){
			hash=hash.replace(/[\W_]+/g,"");
			 var uid=hash+"_"+ Math.random().toString(36).substr(2, 9);
			 return uid;
		}
        tapeworm.prototype.build=function(){
			$(this.element).html('');
	    }
        tapeworm.prototype.bind=function (){
            var self=this;
            $(element).on("remove",$.proxy(this.destroy, this)); //listen for a kill "this plugin" event 
			$(document).on("click",".load_report",function(e) {  
				var data_stream_uid=$(e.currentTarget).data("uid");
					$(".load_report").removeClass('active');
					$(e.currentTarget).addClass('active');
					$(self.element).datareport({api_target:self.api_target,fetch_method:'fetch',config_method:'config','uid':data_stream_uid});
				});
		}
		tapeworm.prototype.unbind=function(){
            //clear variables
			$(document).off("click",".load_report");
		}
        tapeworm.prototype.destroy=function(){
            this.unbind();
            $(this.element).html('');       //cleanup injected HTML.
		}
        tapeworm.prototype.close=function(){
            this.destroy();
		}
		tapeworm.prototype.err=function(data){
			$(".tapeworm-message").show();
			$(".tapeworm-message alert-alert-danger").html(data);
            //alert(data);
		}
        tapeworm.prototype.call_api =function(method,success_func,err_func,da){
            if(da!==undefined && da!==null)  {
                this.data_object=da;
            } else this.data_object={};
            $.ajax({type       : "POST", 
					url        : this.api_target + "/" + method, 
                    contentType: "application/json; charset=utf-8",
                    dataType   : "json" ,
					data       : JSON.stringify(this.data_object),
					headers    : {  'Access-Control-Allow-Credentials' : true,
									'Access-Control-Allow-Origin':'*',
									'Access-Control-Allow-Methods':'POST',
									'Access-Control-Allow-Headers':'application/json'
						},
                    success    : function(results){ if(success_func){ success_func(results);} }, 
                    error      : function(results){ if(err_func)    { err_func(results);    } else this.err(results); } 
                    });
        }
        tapeworm.prototype.get_list=function(){
			var self=this;
			var data_object={ format:"json" }
			var err_func=alert;
			var uid="";
			var success_func=function(data){
				var x=data;	
				var data_streams="",old_entity="",old_group="";
				var in_ul=false;
				for(i in data) {
					if(old_entity!=data[i].entity) {
						if(in_ul) {
							data_streams+=`</div></ul>`;
							data_streams+=`</div></ul>`;
							in_ul=false;
						}
						uid=self.build_uid("entity_");
						data_streams+=`<h6 class="sidebar-heading d-flex px-3 mt-4 mb-1 text-muted">
						  <a class="d-flex align-items-left text-muted" onclick="$('#${uid}').toggle();">
						  <span data-feather="globe"></span><span class="pl-2">${data[i].entity}</span>
						  </a>
						</h6><div class="entity_border" id="${uid}">`;
						new_ul=true;
					}
					if(old_group!=data[i].group){
						if(in_ul) {
							data_streams+=`</ul></div>`;
							in_ul=false;
						}
						
						uid=self.build_uid("grp_");
						data_streams+=`<h6 class="sidebar-heading d-flex px-3 mt-4 mb-1 text-muted">
						  <a class="d-flex align-items-left text-muted" href="#"  onclick="$('#${uid}').toggle();">
						  <span data-feather="folder"></span><span class="pl-2">${data[i].group}</span>
						  </a>
						</h6><div class="group_border" id="${uid}">`;
						new_ul=true;
					}
					if(new_ul){
						new_ul=false;
						data_streams+=`<ul class="nav flex-column">`;
						in_ul=true;
					}
					
					data_streams+=`<li class="nav-item"><a class="load_report nav-link" data-uid="${data[i].uid}"><span data-feather="file"></span>${data[i].display}<span class="sr-only"></span></a></li>`;
					old_entity=data[i].entity;
					old_group =data[i].group;
				}
				
				
				var nav=`<ul class="nav flex-column">
					  <li class="nav-item">
						<a class="nav-link active" href="#active ">
						  <span data-feather="home"></span>
						  Dashboard <span class="sr-only">(current)</span>
						</a>
					  </li>
					</ul>
					<ul class="nav flex-column">
					${data_streams}
					</ul>

					`
				$(".sidebar-sticky").html(nav);
				feather.replace();
			}
			this.call_api("list",success_func,err_func,data_object);
        }
        tapeworm.prototype.build_api_url=function(){
			var url           ="/operation.json";
			
			return this.api_target+url;
			//return "http://localhost:5000/operation/";
			//return          beforeSend: function(xhr){xhr.setRequestHeader('X-Test-Header', 'test-value');},
		}

	    tapeworm.prototype.obj_to_string=function(obj){
            return  JSON.stringify(obj);
        }
        return this.each(function() {
            new tapeworm(this,settings);
        });
    };

}( jQuery ));           
