using System;
using System.Collections.Generic;
using System.Text;
            
namespace tapeworm_core  { 
    public class report {
        public string          msg      { get; set; }
        public stat            stats    { get; set; }
        public List<record>    records  { get; set; }
        public report() {
        }
        public report(stat stats,List<record> records,string msg=null) {
            this.stats=stats;
            this.records=records;
            this.msg=msg;
        }

        public override string ToString() {
            StringBuilder output=new StringBuilder();

            output.Append(stats.ToString());
            output.Append(String.Format("Msg: {0}",msg));
            if(null!=records) {
                foreach(record record in records) {
                    output.AppendLine(record.ToString());
                }
            }
            return output.ToString();
        }
    }
}
