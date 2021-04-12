using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using WhereYouAt.AppCode;

namespace WhereYouAtApi {
	public partial class Tools : System.Web.UI.Page {

		protected void Page_Load(object sender, EventArgs e) {

		}

		protected void btnValidateFcmToken_Click(object sender, EventArgs e) {
			validateTokenAsync();
		}

		async System.Threading.Tasks.Task validateTokenAsync() {
			FcmEngine fcm = new WhereYouAt.AppCode.FcmEngine();
			FcmEngine.FcmResults results = await fcm.ValidateToken(txtFcmToken.Text);
		}

		protected void btnGetUseridsByEmail_Click(object sender, EventArgs e) {
			MyDb db = new MyDb();
			List<string> userids = db.GetAllFcmsByUserEmail(txtGetUsersByEmail.Text);
			foreach (string token in userids) {
				lblGetUsersByEmailResults.Text += token + "<br />";
			}
		}

	}
}