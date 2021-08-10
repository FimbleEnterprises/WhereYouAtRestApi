using System;
using System.Collections.Generic;

namespace WhereYouAt {
	public partial class Tools : System.Web.UI.Page {

		protected void Page_Load(object sender, EventArgs e) {

		}

		protected void btnValidateFcmToken_Click(object sender, EventArgs e) {
			//validateTokenAsync();
		}

		/*async System.Threading.Tasks.Task validateTokenAsync() {
			Fcm.Engine fcm = new Fcm.Engine();
			Fcm.Engine.FcmResults results = await fcm.ValidateToken(txtFcmToken.Text);
		}

		protected void btnGetUseridsByEmail_Click(object sender, EventArgs e) {
			Backend.MyDb db = new Backend.MyDb();
			List<string> userids = db.GetAllFcmsByUserEmail(txtGetUsersByEmail.Text);
			foreach (string token in userids) {
				lblGetUsersByEmailResults.Text += token + "<br />";
			}
		}*/

		protected void btnTestFcm_Click(object sender, EventArgs e) {

			Backend.MyDb db = new Backend.MyDb();
			db.JoinTrip("5555", "116830684150145127689");
			
			/*Backend.MyDb db = new Backend.MyDb();
			Fcm.Engine fcm = new Fcm.Engine();
			Fcm.Message message = new Fcm.Message();
			message.to = "c1dEguZRS36udp5fp8FowC:APA91bEWFDHA-J8-PBV7s7EWlbGOjqK_p3xb0dqxQeWad8AKBiOf_3YX7cZNVzZX499dWb7TruQY-UllsrXv1FDbGbBZ_JCvPw09PNV_Du2nVa-eCX8-e0_Z1Nst2tWBJzb6xGBgfhvi";
			Fcm.Payload payload = new Fcm.Payload();
			payload.obj = db.GetTripUpdateReport("0000", "116830684150145127689");
			payload.title = "test fcm title";
			payload.sentOn = DateTime.Now;
			payload.opcode = Fcm.Payload.OP_CODE_JOINED_TRIP;
			message.data = payload;
			fcm.NotifyAsync(message);*/
		}
	}
}