using System;
using YamlDotNet.Core;
namespace tapeworm_core.@base {
    public class sql_parser {
        public sql_parser(string query) {
            string[] actions=new string[] { "select","insert","delete","update" };
            string[] targets=new string[] { "from" };
            string[] sort=new string[]    { "order by" };

            //split the query into tokens first...
            string [] tokens=query.Split(" ");
        }
    }
}
