using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;
using System.ComponentModel;
using Newtonsoft.Json;

namespace tapeworm_core {

    public class record{
		public  bool   is_comment          { get; set; }=false;
        public  bool   is_data             { get; set; }=false;
        public  bool   is_error            { get; set; }=false;
        public  bool   is_blank            { get; set; }=false;
		public  uint   line                { get; set; }=0;
        private string data                { get; set; }=String.Empty;
        private string comment             { get; set; }=String.Empty;
        public  object model               { get; set; }
        private string raw                 { get; set; }
        private string error               { get; set; }=String.Empty;
        private string uid                 { get; set; }
        [JsonIgnore]
        public   int?   key_hash            { get; set; }

        public record() {
			
        }

        public record(string uid,string key,uint line,string data,string file_path,string error_msg,bool err){
            this.uid         =uid;
            this.raw                =data;
			this.line               =line;
			this.error              =error_msg;
            this.is_error           =true;
		}

        public record(string uid,string key,uint line,string data,string file_path,bool pre_data=false){
            this.uid         =uid;
          this.raw=data;
			this.line=line;

            if(null==globals.models[uid] || null==globals.models[uid] .class_type) {
				throw new NullReferenceException("record does not have a valid type");
			}

            irecord_helper x =(irecord_helper) Activator.CreateInstance(globals.models[uid].class_type);
			this.model=x;
            data=data.Trim();

            //split data and comments ..HACK    
            int comment_index=data.IndexOf(globals.models[uid].comment_delimiter);
            if(pre_data) {
                comment_index=0;
            }
                
            if(comment_index==0) {
				is_comment=true;
                comment=data.Substring(comment_index).Trim();

				if(comment_index!=0)  {
					data=data.Substring(0,comment_index-1);
                    data=data.Trim();       //clean whitespace.
				} else {
					data=string.Empty;
				}
			}

			if(!String.IsNullOrWhiteSpace(data)) {
				 is_data=true;
            } else {
                if(!is_comment)  is_blank=true;
            }
			this.data=data;
            
			if(is_data){
                if(pass_regex(globals.models[uid].regex)){
				    set(key);	
				} else {
					throw new Exception(String.Format("RegEx Fail - Line:{0,6} {1,-30} - {2}",line,file_path,data));
				}
			}
		}

		public bool pass_regex(string regex){
            return true; 
			if(!String.IsNullOrWhiteSpace(regex)) {           //do a regex match on the data.
                Match match = Regex.Match(data,regex);        //line doesnt match regex. log error and skip
				if(!match.Success) {
					StringBuilder e=new StringBuilder();
                    //if groups are used we can highlight the correct areas....
					/*foreach(Group group in match.Groups) {
						
						if(!group.Success) {
							for(int i=0;i<group.Length;i++ ) { e.Append("X"); } 
						} else {
							for(int i=0;i<group.Length;i++ ) { e.Append(" "); } 
						}
					}
					error=String.Format("{0}\n",e.ToString());
                    */
					return false;
				}
            }
			return true;            //no regex or success 
		}

        public string get_data(string property_name) {
            object v1=((record_helper)model).get_value(property_name);
            if(null!=v1) return v1.ToString();
            return null;

            /*
            PropertyInfo property=globals.get_model_property(uid,property_name);
            if(property==null) return null;

            if(string.IsNullOrWhiteSpace(property_name)) return null;
            if(null==model || null== this.model_type) {
                throw new Exception(String.Format("model/type null {0}",property_name));
            }
            try{
                PropertyInfo pi=this.model_type.GetProperty(property_name);
                if(null==pi) {
                    throw new Exception(String.Format("property '{0}' does not exist on model",property_name));
                }
                object return_value=pi.GetValue(model);
                if(null==return_value) return String.Empty;
                else return return_value.ToString();
            } catch(Exception  ex){
                Console.WriteLine("Get Value:"+ex.Message);
                return null;
            }*/
        }

        public void set(string key) {
            string[] tokens=data.Split(globals.models[uid].field_delimiter);
            /*
            if(null==properties || properties.Count()==0) { 
				throw new Exception(string.Format("Properties  are invalid on object in set data"));
            }*/

            for(int index=0;index<tokens.Count(); index++) {
                string value=tokens[index];
                PropertyInfo property=globals.get_model_property(uid,index);
                if(property==null) continue;
                string property_name=property.Name;
                if(!String.IsNullOrWhiteSpace(key) && property_name==key) {
                    this.key_hash=tokens[index].GetHashCode();
                }

                /*
                if(index>=properties.Count()) break;                    //dont go out of index range

				string property_name = properties[index].Name;
                string value=tokens[index];
                //IF
                */
				if(property.PropertyType==typeof(string)) {
					try{
                        property.SetValue(model, value, null);
					} catch {
                        throw new InvalidCastException(string.Format("Tokens: {3} Line: {2} string Error setting value for {0}:{1} ",property_name,value,line,tokens.Count()));
					}
                } else
                //IF					
				if(property.PropertyType==typeof(int?)) {
                    if(string.IsNullOrWhiteSpace(value)) { continue; }              //requires a value, skip
                    int x_int=0;
                    if(Int32.TryParse(value,out x_int)) {
                            property.SetValue(model,x_int, null);
                    } else {
                        throw new InvalidCastException(string.Format("Tokens: {3} Line: {2} int Error setting value for {0}:{1} array-> {4}",property_name,value,line,tokens.Count()));
                    }
                } else
                if(property.PropertyType==typeof(int)) {
                    if(string.IsNullOrWhiteSpace(value)) { continue; }              //requires a value, skip
                    int x_int=0;
                    if(Int32.TryParse(value,out x_int)) {
                        property.SetValue(model,x_int, null);
                    } else {
                         throw new InvalidCastException(string.Format("Tokens: {3} Line: {2} int Error setting value for {0}:{1} ",property_name,value,line,tokens.Count()));
                    }
                } else
 				//IF                    
                if(property.PropertyType==typeof(List<string>)) {
					if(string.IsNullOrWhiteSpace(value)) { continue; }              //requires a value, skip
                    if(string.IsNullOrEmpty(value)) {
                        return;
                    }
                                List<string>list_tokens=new List<string>(value.Split(globals.models[uid].array_delimiter));
                    try{
                        property.SetValue(this.model, list_tokens, null);
                    } catch {
                         throw new InvalidCastException(string.Format("Tokens: {3} Line: {2} List<string> Error setting value for {0}:{1} ",property_name,value,line,tokens.Count()));
                    }
                } else
				//IF                    
				if(property.PropertyType==typeof(List<int>)) {
						if(string.IsNullOrWhiteSpace(value)) { continue; }              //requires a value, skip
                    if(string.IsNullOrEmpty(value)) {
                        return;
                    }
                    List<string>list_tokens=new List<string>(value.Split(globals.models[uid].array_delimiter));
					List<int> list_ints=new List<int>();
					foreach(string list_token in list_tokens) {
						int x_int=0;
                        if(Int32.TryParse(value,out x_int)) {
							list_ints.Add(x_int);
                        } else {
                            throw  new InvalidCastException(string.Format("Tokens: {3} Line: {2} List<int> Error setting value for {0}:{1} array-> {4}",property_name,value,line,tokens.Count()));
                        }
                        try{
                            property.SetValue(this.model, list_tokens, null);
                        } catch {
                            throw new InvalidCastException(string.Format("Tokens: {3} Line: {2} List<int> Error setting value for {0}:{1} ",property_name,value,line,tokens.Count()));
                        }
                    }
                }
            }//end loop
		}//end function

		public string token_array_to_string(string[] tokens,PropertyInfo [] pi){
			StringBuilder o=new StringBuilder();
			for(int i=0;i<tokens.Length;i++) {
				string name="";
				if(i<=pi.Count()) name=pi[i].Name;
				o.Append(String.Format("{0},{1}=\'{2}\',",i,name,tokens[i]));
			}
			return o.ToString();
		}

		public override string ToString() {
			if(error.Any()) {
                return String.Format("#error#{0}",raw);
            }

			if(!is_data &&  !is_comment) return String.Empty;
   			List<string> output=new List<string>();
			Type t=model.GetType();
			PropertyInfo[] properties=t.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			foreach(PropertyInfo property in properties) {
				
				object value=property.GetValue(model);
				if(value==null) {
					output.Add("");
				} else if(property.PropertyType == typeof(List<string>)){
					string[] array=((List<string>)value).ToArray();
                    output.Add(String.Join(globals.models[uid].array_delimiter,array));
				} else if(property.PropertyType == typeof(List<int>)){
                    int[] array=((List<int>)value).ToArray();
                    output.Add(String.Join(globals.models[uid].array_delimiter,array));
                } else 	{
					output.Add(value.ToString());
				}

			}

			if(is_comment && is_data) {
                return String.Format("{0,-80}:{1}",String.Join(globals.models[uid].field_delimiter,output.ToArray()),comment);
			} 
			if(is_comment && !is_data) {
                return String.Format("{0}",comment);
            }
		    if(!is_comment && is_data) {
			    return String.Format("{0}",String.Join(":",output.ToArray()));
            }
			return "";
		}
    }
}



/*
public class FastMethodInfo
{
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Reflection;
    private delegate object ReturnValueDelegate(object instance, object[] arguments);
    private delegate void VoidDelegate(object instance, object[] arguments);

    public FastMethodInfo(MethodInfo methodInfo)
    {
        var instanceExpression = Expression.Parameter(typeof(object), "instance");
        var argumentsExpression = Expression.Parameter(typeof(object[]), "arguments");
        var argumentExpressions = new List<Expression>();
        var parameterInfos = methodInfo.GetParameters();
        for (var i = 0; i < parameterInfos.Length; ++i)
        {
            var parameterInfo = parameterInfos[i];
            argumentExpressions.Add(Expression.Convert(Expression.ArrayIndex(argumentsExpression, Expression.Constant(i)), parameterInfo.ParameterType));
        }
        var callExpression = Expression.Call(!methodInfo.IsStatic ? Expression.Convert(instanceExpression, methodInfo.ReflectedType) : null, methodInfo, argumentExpressions);
        if (callExpression.Type == typeof(void))
        {
            var voidDelegate = Expression.Lambda<VoidDelegate>(callExpression, instanceExpression, argumentsExpression).Compile();
            Delegate = (instance, arguments) => { voidDelegate(instance, arguments); return null; };
        }
        else
            Delegate = Expression.Lambda<ReturnValueDelegate>(Expression.Convert(callExpression, typeof(object)), instanceExpression, argumentsExpression).Compile();
    }

    private ReturnValueDelegate Delegate { get; }

    public object Invoke(object instance, params object[] arguments)
    {
        return Delegate(instance, arguments);
    }
}
*/