using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WhereYouAtRestApi.AppCode {
	public class Requests {

		public class Argument {
			public string name { get; set; }
			public object value { get; set; }

			public Argument() { }

			public Argument(string name, string value) {
				this.name = name;
				this.value = value;
			}
		}

		public class Request {
			public Request() { }
			public string Function { get; set; }
			public List<Argument> Arguments { get; set; } = new List<Argument>();
		}

	}
}