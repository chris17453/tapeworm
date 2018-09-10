using System;
namespace tapeworm_core {
    public class delimiters {
        public char    array   { get; set; }=',';
        public char    field   { get; set; }=':';
        public char[]  comment { get; set; }=new char[] { '#','-','[' }; //TODO Dynamic delimiters
        public delimiters() {
        }
        public delimiters(char array,char field,char[] comment) {
            this.array  =array;
            this.field  =field;
            this.comment=comment;
        }
    }
}
