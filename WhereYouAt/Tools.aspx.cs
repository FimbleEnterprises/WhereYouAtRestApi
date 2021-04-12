using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WhereYouAtApi {
	public partial class Tools : System.Web.UI.Page {
		protected void Page_Load(object sender, EventArgs e) {

		}

		protected void btnValidateFcmToken_Click(object sender, EventArgs e) {
			validateTokenAsync();
		}

		async System.Threading.Tasks.Task validateTokenAsync() {
			WhereYouAt.AppCode.FcmEngine fcm = new WhereYouAt.AppCode.FcmEngine();
			WhereYouAt.AppCode.FcmEngine.FcmResults response = await fcm.ValidateToken(txtFcmToken.Text);

			if (response.wasSuccessful()) {
				lblFcmTokenResult.Text = "Success";
			} else {
				lblFcmTokenResult.Text = response.results[0].error;
			}
		}

	}
}