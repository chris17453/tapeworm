using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;


namespace tapeworm_core  { 
	public interface flat_interface {
		string        regex          { get; }
		List<record>  records        { get; set; }
		List<string>  errors         { get; set; }
        stat          stat();
        report        report(options options);
        bool          load();
		bool          save();
        bool          update();
    }
}
