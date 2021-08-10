using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WhereYouAt.Backend;
using WhereYouAt.Google;

namespace WhereYouAtApi.AppCode {
	public class UserMessage {

		public User sender { get; set; }
		public User receiver { get; set; }
		public string messageBody { get; set; }
		public string tripcode { get; set; }
		public long createdonutc { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
		public List<User> acknowledgedby { get; set; }

		public UserMessage() { }

		public UserMessage(User sender, string msg, string tripcode) {
			this.sender = sender;
			this.messageBody = msg;
			this.tripcode = tripcode;
		}

		public bool AppendToDb() {
			MyDb db = new MyDb();
			return db.AppendUserMessage(this);
		}

		public string toJson() {
			return Newtonsoft.Json.JsonConvert.SerializeObject(this);
		}

	}
}