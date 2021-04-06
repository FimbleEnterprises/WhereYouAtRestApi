using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WhereYouAtRestApi.AppCode {

	public class Trip {
		public string tripcode;
		public DateTime createdon;
		public TripMember createdby;
		public List<TripMember> members = new List<TripMember>();

		public Trip(string tripcode) {
			this.tripcode = tripcode;
		}

		public Trip(string tripcode, string email) {
			this.tripcode = tripcode;
			this.members.Add(new TripMember(email));
		}

		public string toJson() {
			return Newtonsoft.Json.JsonConvert.SerializeObject(this);
		}

	}

	public class TripMember {
		public string email;

		public TripMember(string email) {
			this.email = email;
		}

	}
}