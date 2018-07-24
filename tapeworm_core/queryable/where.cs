using System;
using System.Collections.Generic;
using System.Linq;

namespace tapeworm_core { 
    public static partial class query_extensions{
        public static IEnumerable<T> NCDB_Where<T>(this IEnumerable<T> data,List<Tuple<string,string, string,string>> expressisons){
            if(!data.Any()) return data;

            if(globals.debug) Console.WriteLine("WHERE:");

            if (null==expressisons)  {                          //no expresisons return data in curent order
                if(globals.debug) Console.WriteLine("No filter, Expressions null");
                return data;
            }
            int number_of_expresisons=expressisons.Count;
            if(number_of_expresisons<= 0){                      //no expresisons return data in curent order
                if(globals.debug) Console.WriteLine(String.Format("No Expressions to filter. Records:{0}",data.Count()));
                return data;
            }

            IEnumerable<T>        filtered_query= from item in data select item;


            Func<T,bool> p =  delegate(T arg) {
                for (int i=0;i<number_of_expresisons;i++){
                    int    index=i;
                    string path=expressisons[index].Item1;
                    string value=expressisons[index].Item3;
                    string operation=expressisons[index].Item2;
                    Tuple<Type,object> path_object=get_object_from_path(path,arg);
                    //Console.WriteLine(String.Format("{0},{1},{2},{3}",expressisons[i].Item1,expressisons[i].Item2,expressisons[i].Item3,expressisons[i].Item4));
                    if(expressisons[index].Item4=="multi") {
                        switch(operation){
                            case "=": if(path_object.Item2==null && value=="null") return true;
                                if(path_object.Item2!=null && path_object.Item2.ToString().Contains(value,StringComparison.OrdinalIgnoreCase)){
                                    //Console.WriteLine(String.Format("{0},{1},{2},{3}:{4}",expressisons[i].Item1,expressisons[i].Item2,expressisons[i].Item3,expressisons[i].Item4,path_object.Item2.ToString()));
                                    return true;
                                }
                                      break;
                            default: return false;
                        }
                    } else {
                    /*    switch(operation){
                            case "=": if(path_object.Item2==null && value!="null") return false;
                                      if(path_object.Item2.ToString()!=value)      return false;
                                      break;
                            default: return false;
                        }
                        */
                    }
                }
                return false;
            };
            return  filtered_query .Where(p);
        }

       

    }
}
