using System;
namespace tapeworm_core{
    public interface irecord_helper{
        object get_value(string property);
        bool   set_value(string property,string value,bool is_array);
    }
    public class record_helper:irecord_helper {
        public virtual object get_value(string property)
        {
            throw new NotImplementedException();
        }
        public virtual bool set_value(string property,string value, bool is_array)
        {
            throw new NotImplementedException();
        }

/*
            if (property.PropertyType==typeof(string)) {
					try{
                        property.SetValue(model, value, null);
					} catch {
                        throw new InvalidCastException(string.Format("Tokens: {3} Line: {2} string Error setting value for {0}:{1} ", property_name, value, line, tokens.Count()));
					}
                } else
                //IF					
				if(property.PropertyType==typeof(int?)) {
                    if(string.IsNullOrWhiteSpace(value)) { continue; }              //requires a value, skip
                    int x_int = 0;
                    if(Int32.TryParse(value,out x_int)) {
                            property.SetValue(model, x_int, null);
                    } else {
                        throw new InvalidCastException(string.Format("Tokens: {3} Line: {2} int Error setting value for {0}:{1} array-> {4}", property_name, value, line, tokens.Count()));
                    }
                } else
                if(property.PropertyType==typeof(int)) {
                    if(string.IsNullOrWhiteSpace(value)) { continue; }              //requires a value, skip
                    int x_int = 0;
                    if(Int32.TryParse(value,out x_int)) {
                        property.SetValue(model, x_int, null);
                    } else {
                         throw new InvalidCastException(string.Format("Tokens: {3} Line: {2} int Error setting value for {0}:{1} ", property_name, value, line, tokens.Count()));
                    }
                } else
 				//IF                    
                if(property.PropertyType==typeof(List<string>)) {
					if(string.IsNullOrWhiteSpace(value)) { continue; }              //requires a value, skip
                    if(string.IsNullOrEmpty(value)) {
                        return;
                    }
                                List<string> list_tokens = new List<string>(value.Split(globals.models[uid].delimiters.array));
                    try{
                        property.SetValue(this.model, list_tokens, null);
} catch {
                         throw new InvalidCastException(string.Format("Tokens: {3} Line: {2} List<string> Error setting value for {0}:{1} ", property_name, value, line, tokens.Count()));
                    }
                } else
				//IF                    
				if(property.PropertyType==typeof(List<int>)) {
						if(string.IsNullOrWhiteSpace(value)) { continue; }              //requires a value, skip
                    if(string.IsNullOrEmpty(value)) {
                        return;
                    }
                    List<string> list_tokens = new List<string>(value.Split(globals.models[uid].delimiters.array));
List<int> list_ints = new List<int>();
					foreach(string list_token in list_tokens) {
						int x_int = 0;
                        if(Int32.TryParse(value,out x_int)) {
							list_ints.Add(x_int);
                        } else {
                            throw  new InvalidCastException(string.Format("Tokens: {3} Line: {2} List<int> Error setting value for {0}:{1} array-> {4}", property_name, value, line, tokens.Count()));
                        }
                        try{
                            property.SetValue(this.model, list_tokens, null);
} catch {
                            throw new InvalidCastException(string.Format("Tokens: {3} Line: {2} List<int> Error setting value for {0}:{1} ", property_name, value, line, tokens.Count()));
                        }
                    }
                }*/

//    }

    }
}
