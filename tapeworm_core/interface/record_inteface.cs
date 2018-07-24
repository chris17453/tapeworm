using System;
namespace tapeworm_core{
    public interface irecord_helper{
        object get_value(string property);
    }
    public class record_helper:irecord_helper {
        public virtual object get_value(string property) {
            throw new NotImplementedException();
        }
    }
}
