using System;
using System.Collections.Generic;
using System.Linq;

namespace tapeworm_core { 
    public static partial class query_extensions{
        public static IEnumerable<T> NCDB_OrderBy<T>(this IEnumerable<T> data, List<Tuple<string, string>> order_by_expresisons){
            if (null==order_by_expresisons)  {              //no expresisons return data in curent order
                if(globals.debug) Console.WriteLine("Expressions null");
                return data;
            }
            int number_of_expresisons=order_by_expresisons.Count;
            if(number_of_expresisons<= 0){                  //no expresisons return data in curent order
                if(globals.debug) Console.WriteLine(String.Format("No Expressions to sort. Records:{0}",data.Count()));
                return data;
            }
            if(!data.Any()) return data;

            IEnumerable<T> query = from item in data select item;
            IOrderedEnumerable<T> orderedQuery = null;
            Type class_type=typeof(T);
            //hard code for 2 levels. split out later for recursive.
            bool first=true;
            for (int i=0;i<number_of_expresisons;i++){
                var index = i;
                object item_a=data.First();

                Tuple<Type,object> path_object=get_object_from_path(order_by_expresisons[i].Item1,item_a);
                if(path_object.Item1==null) {
                    Console.WriteLine(String.Format("Object was not found {0}",order_by_expresisons[i].Item1));
                    //continue;
                } else {
                   // Console.WriteLine(String.Format("Object NOT null {0}",order_by_expresisons[i].Item1));
                }



                Func<T, object> expression = delegate (T item) {
                    Tuple<Type,object> delegate_path_object=get_object_from_path(order_by_expresisons[index].Item1,item);
                    return delegate_path_object.Item2;
                };
                                                         
                if (order_by_expresisons[index].Item2 == "asc"){
                    if(first){
                        orderedQuery = query.OrderBy(expression);
                        first=false;
                    } else {
                        orderedQuery=orderedQuery.ThenBy(expression);
                    }
                } else {
                    if(first){
                        orderedQuery = query.OrderByDescending(expression);
                        first=false;
                    } else {
                        orderedQuery = orderedQuery.ThenByDescending(expression);
                    }
                }
            }
            if(first) return data;
            query = orderedQuery;
            return query;
        }
    }
}


