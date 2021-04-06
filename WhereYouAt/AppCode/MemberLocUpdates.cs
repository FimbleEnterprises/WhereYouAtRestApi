using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RestApi.AppCode {

	public class MemberLocUpdate {
		public double createdOn { get; set; }
		public string displayName { get; set; }
		public string email { get; set; }
		public double lat { get; set; }
		public double lon { get; set; }
		public double accuracy_meters { get; set; }
		public double modifiedOn { get; set; }
		public string photoUrl { get; set; }
		public string tripcode { get; set; }
		public string userid { get; set; }
	}

	public class MemberLocUpdates {
		public List<MemberLocUpdate> list { get; set; } = new List<MemberLocUpdate>();
		public string tripcode { get; set; }

		public string toJson() {
			string json = Newtonsoft.Json.JsonConvert.SerializeObject(this);
			return json;
		}

	}

}