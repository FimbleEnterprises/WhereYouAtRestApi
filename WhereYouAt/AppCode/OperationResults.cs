using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WhereYouAt.Api {
	public class OperationResults {
		public List<OperationResult> allResults = new List<OperationResult>();

		public OperationResults() { }

		public OperationResults(OperationResult result) {
			this.allResults.Add(result);
		}

		public string toJson() {
			try {
				return Newtonsoft.Json.JsonConvert.SerializeObject(this);
			} catch (Exception e) {
				return "Failed to convert results to json: " + e.Message;
			}
		}
	}

	public class OperationResult {
		public bool wasSuccessful;
		public string operationSummary;
		public object result;

		public OperationResult() { }

		public OperationResult(bool wasSuccessful, string operationSummary, object result) {
			this.wasSuccessful = wasSuccessful;
			this.operationSummary = operationSummary;
			this.result = result;
		}

		public OperationResult(bool wasSuccessful, string operationSummary) {
			this.wasSuccessful = wasSuccessful;
			this.operationSummary = operationSummary;
			this.result = "";
		}
	}

	public class FcmUpsertResult {
		public string userid;
		public string fcmToken;
		public bool wasSuccessful;

		public FcmUpsertResult(bool wasSuccessful, string userid, string fcmtoken) {
			this.userid = userid;
			this.fcmToken = fcmtoken;
			this.wasSuccessful = wasSuccessful;
		}

		public string ToJson() {
			return Newtonsoft.Json.JsonConvert.SerializeObject(this);
		}
	}

}