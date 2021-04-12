/*using FirebaseAdmin;
using FirebaseAdmin.Messaging;*/
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using WhereYouAt.AppCode;

namespace WhereYouAt.AppCode {
	public class FcmEngine {

		public const string FCM_SERVER_KEY = "	AAAA62YA-5w:APA91bF8GekkOCBKpoWSniDHgJwz0oi2OMLa4tb9hBaHdJDrx9WiUFQx5cvVp02TME0RObUGDvI5bDu85ukeWcShjAvzwnzpEGM7hojnjdHB5gcIgMNacQSmt8rDRhvCLPyNSIVrHTf2";
		public const string FCM_SENDER_KEY = "1011028655004";
		public const string FCM_JSON = "{\r\n  \"type\": \"service_account\",\r\n  \"project_id\": \"where-you-at-88cb5\",\r\n  \"private_key_id\": \"763f5ed6e88fac2b2257a962c0bd3d2665a4499c\",\r\n  \"private_key\": \"-----BEGIN PRIVATE KEY-----\\nMIIEvQIBADANBgkqhkiG9w0BAQEFAASCBKcwggSjAgEAAoIBAQCzfin2zLxK6/iZ\\nmcfhNdnzHREEGMBiOwgsk+p9R1S8gsnAwdB4RRxrLg37a/mCC71D0W1MNCZBpfBF\\nkob+LcDRP2bXrPeHuPr0SkyznPcTBj2UiGWWFRBT7GErj7PrGIsxtA3V5ftpJRTm\\nUnfBckgcSs/tuW1Ti7KAXkAa7pRKgUXD0iooUcIaPntVkMhuM2eUJsoJMmcLTmB/\\njNYU1D1JIvxunKhFV548B8YgwfzcB5jt+Egay3HFHP0Q6V7/Jft0MEdeSvLfnCNI\\ntv34dELBJe2HdWcpRMzuc8PrBdLxgRMK+vEPnaX6RYaOS1CZhKQHf7JI8M9LAeWZ\\n1OsFKNcfAgMBAAECggEAEBhTbFK2Mlmv3xt6YD199QjkNQ6AdyPSpdIAXYop6lrS\\npeWQlgL3JhKTz7nCPou4Rak1OHdsqXJXIJtKXjx9ZRte/h6qdjx7dFsSlFXnlIh4\\nuSWfRl1OgNgEwUKPiu3r0rcnOR5NnwNjjmD9ZWv5VLV4qQPO7oZA/rXKNcqy+BiH\\nncEMW12AQ3wCCt4A+5AlJ/AONclGLYq2zb9KP1IkR+mQITq2U06KSHJnSxv1AzKi\\nnbb3kbP3OGfDUKuoyQNTIZtqaHkamfqMv8W6M1ULPA00uStjzzP8o859Yd7Wh/Pp\\npsJ0bVDmwDBAZtUKUt0hU0sHZFNE194nhyFNfkpIAQKBgQDiPIY2Nrgd3abHEQci\\nWSOgqaSkF/3vEEBDWI7vXqDly1MneZJM2qCPiYQDeBFqPZU16d+4iLA93vyTpLG9\\ndzq5cA+jbt1DPr7mBsNXipoMT9OaH8ufowjgR8slaoTnZSWqk8+apbacaFzgTIeO\\n/ajZKOmEFGsnwm24QxTNNzCNHwKBgQDLG1mRiJBG//iqPUZ7hsSHSGAiYIObqoN0\\nhI8jNRg0lNHGmvJFqfzVLm76Ya/mRBXXBWvEQG7cHqCc3X6S5NFRTzRquoHaIHrn\\nBQf685eJtvkEzXLKWDZa9ZKy/VBf2ovWW6Im/8GhD3+NSsq0dQ3Sk3haqqmsRpyC\\nX83iSZR2AQKBgA+jQ1un+J4H3ilQYf/bzXyjfT/icKj/pJGDI44kKlb38O1/l788\\nNXD2fmuG9x0y2Id7fP1SZWxBZ5AFCEwiW6rJtisD1MN7KNLZkNMSP4euVOAIZpjI\\nOirWi5IwhLNB6JKb2PPFlJPHGvReFdiqa+tmyo1Yo/eBlvfvmxZ8RBzdAoGASMeY\\nJOpGqOp4dKhYZVM5gTCSIj9raZCEYnBNylPwRIc/VGeZgRUUoF9vLRFsMpsbKF2s\\n7Doaf44KB6vm+0Q7LWOwaL5EYaFZ6QaIgYhgONz0BqCIDbHmaFmvI6xZ7L8ndLyJ\\nMmhbPheLoVm3oUqAlYB6lCKrgiwbUSx7Xib3qAECgYEA049MS5aKycVEK0R8maxi\\n+v3qCxTC+13BKtsnr6qHfJyvIn1f2Xy1l5LlaB4MUxBXc3zrrajXPPmvQ3K8ADzY\\ny/piRQdsC1IW002m1IS40d4efsqipZCEdGJT6kZZMrP4MWogeOq+7nWxF9qFG2Da\\nW3+YKLgCysJBqkXQrIdMdfg=\\n-----END PRIVATE KEY-----\\n\",\r\n  \"client_email\": \"firebase-adminsdk-q3982@where-you-at-88cb5.iam.gserviceaccount.com\",\r\n  \"client_id\": \"108648506301760800235\",\r\n  \"auth_uri\": \"https://accounts.google.com/o/oauth2/auth\",\r\n  \"token_uri\": \"https://oauth2.googleapis.com/token\",\r\n  \"auth_provider_x509_cert_url\": \"https://www.googleapis.com/oauth2/v1/certs\",\r\n  \"client_x509_cert_url\": \"https://www.googleapis.com/robot/v1/metadata/x509/firebase-adminsdk-q3982%40where-you-at-88cb5.iam.gserviceaccount.com\"\r\n}\r\n";
		public const string VALIDATE_TOKEN_TITLE = "VALIDATE_TOKEN_TEST";
		public const string VALIDATE_TOKEN_BODY = "This is a test push notification to validate that the FCM token used for your device is valid - they can change without warning after all.  You should not be seeing this but if you are seeing this, then... Hi!  How the heck are ya?!";

		public enum TriStateResult {
			YES, NO, UNKNOWN
		}

		// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
		public class FcmResult {
			public const string NOT_REGISTERED_RESPONSE = "NotRegistered";
			public string error { get; set; }
		}

		/// <summary>
		/// A container for parsing Google responses.  I haven't messed with this much other than deserializing the returned JSON.
		/// I can confirm that sending a single FCM with an invalid token returns a result at index 0 flagged as such. 
		/// </summary>
		public class FcmResults {
			public long multicast_id { get; set; }
			public int success { get; set; }
			public int failure { get; set; }
			public int canonical_ids { get; set; }
			public List<FcmResult> results { get; set; }

			public FcmResults () { }

			/// <summary>
			/// A constructor that hasn't been massaged much at all - really just a deserializes the Google server response.
			/// </summary>
			/// <param name="googleServerResponse">The web request response from Google's servers</param>
			public FcmResults(string googleServerResponse) {
				FcmResults _results = (FcmResults) Newtonsoft.Json.JsonConvert.DeserializeObject<FcmResults>(googleServerResponse);
				this.multicast_id = _results.multicast_id;
				this.success = _results.success;
				this.failure = _results.failure;
				this.canonical_ids = _results.canonical_ids;
				this.results = _results.results;
			}

			/// <summary>
			/// Helper method for determining basic success/failure.
			/// </summary>
			/// <returns></returns>
			public bool wasSuccessful() {
				return this.success == 1;
			}

			/// <summary>
			/// Determines if the supplied FCM token is valid and registered.
			/// </summary>
			/// <returns>Yes, no or unknown</returns>
			public TriStateResult wasDeviceUnRegistered() {
				try {
					if (this.results[0].error.Equals(FcmResult.NOT_REGISTERED_RESPONSE)) {
						return TriStateResult.YES;
					} else {
						return TriStateResult.NO;
					}
				} catch (Exception e) {
					return TriStateResult.UNKNOWN;
				}
			}

		}

		/// <summary>
		/// Sends an FCM with a pass/fail response.  Failures are written to the database but do not return any failure info to the caller.
		/// </summary>
		/// <param name="to">A valid recipient FCM token</param>
		/// <param name="title">Message title</param>
		/// <param name="body">Message body</param>
		/// <returns>Pass/Fail boolean response.</returns>
		public async Task<FcmResults> NotifyAsync(string recipient, string title, string body) {

			// Our server's identity and access key
			string SERVER_API_KEY = FCM_SERVER_KEY;
			var SENDER_ID = FCM_SENDER_KEY;

			// Construct the request 
			WebRequest tRequest;
			tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send");
			tRequest.Method = "post";
			tRequest.ContentType = "application/json";
			tRequest.Headers.Add(string.Format("Authorization: key={0}", SERVER_API_KEY));
			tRequest.Headers.Add(string.Format("Sender: id={0}", SENDER_ID));

			// Construct a quick FCM object per Google's FCM requirements
			var data = new {
				to = recipient,
				notification = new {
					body = body,
					title = title,
					icon = "myicon"
				}
			};

			// Convert that quick FCM object to json for Google consumption
			var json = JsonConvert.SerializeObject(data);

			// Create a byte array from our json and measure its length
			Byte[] byteArray = Encoding.UTF8.GetBytes(json);
			tRequest.ContentLength = byteArray.Length;

			// Construct a data stream within our web request 
			Stream dataStream = tRequest.GetRequestStream();
			dataStream.Write(byteArray, 0, byteArray.Length);
			dataStream.Close();

			// Make the request
			WebResponse tResponse = tRequest.GetResponse();

			// Use the data stream afor the response
			dataStream = tResponse.GetResponseStream();

			// Read the response and convert it to a string
			StreamReader tReader = new StreamReader(dataStream);
			String sResponseFromServer = tReader.ReadToEnd();

			// Clean up
			tReader.Close();
			dataStream.Close();
			tResponse.Close();

			// Create our custom response object from the response string and return it
			FcmResults results = new FcmResults(sResponseFromServer);
			return results;
		}

		/// <summary>
		/// This sends an FCM notification like any other just with a fixed title and body.  Its purpose is to validate a user's FCM token by reading the 
		/// server response after sending the message.
		/// </summary>
		/// <param name="to">The FCM token string to validate</param>
		/// <returns>Not quite sure yet...</returns>
		public async Task<FcmResults> ValidateToken(string recipient) {

			// Our server's identity and access key
			string SERVER_API_KEY = FCM_SERVER_KEY;
			var SENDER_ID = FCM_SENDER_KEY;

			// Construct the request 
			WebRequest tRequest;
			tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send");
			tRequest.Method = "post";
			tRequest.ContentType = "application/json";
			tRequest.Headers.Add(string.Format("Authorization: key={0}", SERVER_API_KEY));
			tRequest.Headers.Add(string.Format("Sender: id={0}", SENDER_ID));

			// Construct a quick FCM object per Google's FCM requirements
			var data = new {
				to = recipient,
				notification = new {
					body = VALIDATE_TOKEN_BODY,
					title = VALIDATE_TOKEN_TITLE,
					icon = "myicon"
				}
			};

			// Convert that quick FCM object to json for Google consumption
			var json = JsonConvert.SerializeObject(data);

			// Create a byte array from our json and measure its length
			Byte[] byteArray = Encoding.UTF8.GetBytes(json);
			tRequest.ContentLength = byteArray.Length;

			// Construct a data stream within our web request 
			Stream dataStream = tRequest.GetRequestStream();
			dataStream.Write(byteArray, 0, byteArray.Length);
			dataStream.Close();

			// Make the request
			WebResponse tResponse = tRequest.GetResponse();

			// Use the data stream afor the response
			dataStream = tResponse.GetResponseStream();

			// Read the response and convert it to a string
			StreamReader tReader = new StreamReader(dataStream);
			String sResponseFromServer = tReader.ReadToEnd();

			// Clean up
			tReader.Close();
			dataStream.Close();
			tResponse.Close();

			// Create our custom response object from the response string and return it
			FcmResults results = new FcmResults(sResponseFromServer);
			return results;
		}

	}

	// FcmMessage myDeserializedClass = JsonConvert.DeserializeObject<FcmMessage>(myJsonResponse); 
	public class Payload {
		public string title { get; set; }
		public string body { get; set; }
	}

	public class Message {
		public string token { get; set; }
		public Payload notification { get; set; }
	}

	public class FcmContainer {
		public Message message { get; set; }

		/// <summary>
		/// Constructs a FCM message that Google's servers can parse.  
		/// </summary>
		/// <param name="to">Recipient's fcm token.</param>
		/// <param name="title">A title for the message</param>
		/// <param name="body">The body of the messsage</param>
		/// <returns>JSON that Google's FCM server can parse and can be included in the web request to Google.</returns>
		public static string ConstructAsJson(string to, string title, string body) {
			// Build an object that can be deserialized into JSON that Google's FCM server can understand.
			FcmContainer fcmContainer = new FcmContainer();

			// 
			Payload payload = new Payload();
			payload.title = title;
			payload.body = body;

			Message msg = new Message();
			msg.token = to;
			msg.notification = payload;

			fcmContainer.message = msg;

			return JsonConvert.SerializeObject(fcmContainer);
		}

		/// <summary>
		/// Constructs a FCM message in a format described by Google for FCM messages.
		/// </summary>
		/// <param name="to">Recipient's fcm token.</param>
		/// <param name="title">A title for the message</param>
		/// <param name="body">The body of the messsage</param>
		/// <returns>An object with properties structured in such a way as to conform to Google's FCM message requests.  This is unlikely
		/// to be used in this form - this would typically be serialized to JSON and included in a web request.</returns>
		public static FcmContainer ConstructAsObject(string to, string title, string body) {
			// Build an object that can be deserialized into JSON that Google's FCM server can understand.
			FcmContainer fcmContainer = new FcmContainer();

			// 
			Payload payload = new Payload();
			payload.title = title;
			payload.body = body;

			Message msg = new Message();
			msg.token = to;
			msg.notification = payload;

			fcmContainer.message = msg;

			return fcmContainer;
		}

	}

}