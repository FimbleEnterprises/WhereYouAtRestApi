using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WhereYouAt.Api {

	public class MemberLocUpdate {
		public DateTime createdOn { get; set; }
		public string displayName { get; set; }
		public string email { get; set; }
		public double lat { get; set; }
		public double lon { get; set; }
		public double accuracy_meters { get; set; }
		public DateTime modifiedOn { get; set; }
		public string photoUrl { get; set; }
		public string tripcode { get; set; }
		public string userid { get; set; }
		public double insertid { get; set; }
		public string locationtype { get; set; }
		public string location { get; set; }
		public decimal velocity { get; set; }
	}

	public class TripReport {
		public List<MemberLocUpdate> list { get; set; } = new List<MemberLocUpdate>();
		public string tripcode { get; set; }
		public string initiatedby { get; set; }
		public DateTime initiatedon { get; set; }

		public string toJson() {
			string json = Newtonsoft.Json.JsonConvert.SerializeObject(this);
			return json;
		}

		/// <summary>
		/// Loops through the MemberLoc objects in teh list and if a userid matches the supplied id
		/// it constructs and returns a GoogleUser object from the matched MemberLocUpdate object
		/// </summary>
		/// <param name="userid"></param>
		/// <returns></returns>
		public WhereYouAt.Google.User GetUserFromMemberLocs(string userid) {
			foreach (MemberLocUpdate item in this.list) {
				if (item.userid == userid) {
					Google.User user = new Google.User();
					user.email = item.email;
					user.fullname = item.displayName;
					user.photourl = item.photoUrl;
					user.id = item.userid;
					return user;
				}
			}
			return null;
		}

	}

}