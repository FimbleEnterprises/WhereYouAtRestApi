using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WhereYouAt.Google {
	public class User {
		public string id { get; set; }
		public string email { get; set; }
		public string fullname { get; set; }
		public string photourl { get; set; }

		public string ToJson() {
			return Newtonsoft.Json.JsonConvert.SerializeObject(this);
		}

		public static User FromJson(string json) {
			return Newtonsoft.Json.JsonConvert.DeserializeObject<User>(json);
		}

	}
}