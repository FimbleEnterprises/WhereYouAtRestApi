/*using FirebaseAdmin;
using FirebaseAdmin.Messaging;*/
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using WhereYouAt.AppCode;

namespace WhereYouAt.AppCode {
	public class FcmEngine {

		public enum TriStateResult {
			YES, NO, UNKNOWN
		}

		// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
		public class FcmResult {
			public const string NOT_REGISTERED_RESPONSE = "NotRegistered";
			public string error { get; set; }
		}

		public class FcmResults {
			public long multicast_id { get; set; }
			public int success { get; set; }
			public int failure { get; set; }
			public int canonical_ids { get; set; }
			public List<FcmResult> results { get; set; }

			public FcmResults () { }

			public FcmResults(string googleServerResponse) {
				FcmResults _results = (FcmResults) Newtonsoft.Json.JsonConvert.DeserializeObject<FcmResults>(googleServerResponse);
				this.multicast_id = _results.multicast_id;
				this.success = _results.success;
				this.failure = _results.failure;
				this.canonical_ids = _results.canonical_ids;
				this.results = _results.results;
			}

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

		public const string FCM_SERVER_KEY = "AAAAJdfHBKg:APA91bEemGxuR_f9tgEwv6enmgiiNfpt1BRKE5d-H41Ddm0sUPUQH96ElCLCyMZJgwLVQdMg1b07y5qhwDPosg7VW5t3Ql_RT2BypoyOWrtiJ_t7a2m6Z45Vnu9_PZ2So726hvphC7hF";
		public const string FCM_SENDER_KEY = "162533934248";
		public const string FCM_JSON = "{\r\n  \"type\": \"service_account\",\r\n  \"project_id\": \"where-you-at-88cb5\",\r\n  \"private_key_id\": \"763f5ed6e88fac2b2257a962c0bd3d2665a4499c\",\r\n  \"private_key\": \"-----BEGIN PRIVATE KEY-----\\nMIIEvQIBADANBgkqhkiG9w0BAQEFAASCBKcwggSjAgEAAoIBAQCzfin2zLxK6/iZ\\nmcfhNdnzHREEGMBiOwgsk+p9R1S8gsnAwdB4RRxrLg37a/mCC71D0W1MNCZBpfBF\\nkob+LcDRP2bXrPeHuPr0SkyznPcTBj2UiGWWFRBT7GErj7PrGIsxtA3V5ftpJRTm\\nUnfBckgcSs/tuW1Ti7KAXkAa7pRKgUXD0iooUcIaPntVkMhuM2eUJsoJMmcLTmB/\\njNYU1D1JIvxunKhFV548B8YgwfzcB5jt+Egay3HFHP0Q6V7/Jft0MEdeSvLfnCNI\\ntv34dELBJe2HdWcpRMzuc8PrBdLxgRMK+vEPnaX6RYaOS1CZhKQHf7JI8M9LAeWZ\\n1OsFKNcfAgMBAAECggEAEBhTbFK2Mlmv3xt6YD199QjkNQ6AdyPSpdIAXYop6lrS\\npeWQlgL3JhKTz7nCPou4Rak1OHdsqXJXIJtKXjx9ZRte/h6qdjx7dFsSlFXnlIh4\\nuSWfRl1OgNgEwUKPiu3r0rcnOR5NnwNjjmD9ZWv5VLV4qQPO7oZA/rXKNcqy+BiH\\nncEMW12AQ3wCCt4A+5AlJ/AONclGLYq2zb9KP1IkR+mQITq2U06KSHJnSxv1AzKi\\nnbb3kbP3OGfDUKuoyQNTIZtqaHkamfqMv8W6M1ULPA00uStjzzP8o859Yd7Wh/Pp\\npsJ0bVDmwDBAZtUKUt0hU0sHZFNE194nhyFNfkpIAQKBgQDiPIY2Nrgd3abHEQci\\nWSOgqaSkF/3vEEBDWI7vXqDly1MneZJM2qCPiYQDeBFqPZU16d+4iLA93vyTpLG9\\ndzq5cA+jbt1DPr7mBsNXipoMT9OaH8ufowjgR8slaoTnZSWqk8+apbacaFzgTIeO\\n/ajZKOmEFGsnwm24QxTNNzCNHwKBgQDLG1mRiJBG//iqPUZ7hsSHSGAiYIObqoN0\\nhI8jNRg0lNHGmvJFqfzVLm76Ya/mRBXXBWvEQG7cHqCc3X6S5NFRTzRquoHaIHrn\\nBQf685eJtvkEzXLKWDZa9ZKy/VBf2ovWW6Im/8GhD3+NSsq0dQ3Sk3haqqmsRpyC\\nX83iSZR2AQKBgA+jQ1un+J4H3ilQYf/bzXyjfT/icKj/pJGDI44kKlb38O1/l788\\nNXD2fmuG9x0y2Id7fP1SZWxBZ5AFCEwiW6rJtisD1MN7KNLZkNMSP4euVOAIZpjI\\nOirWi5IwhLNB6JKb2PPFlJPHGvReFdiqa+tmyo1Yo/eBlvfvmxZ8RBzdAoGASMeY\\nJOpGqOp4dKhYZVM5gTCSIj9raZCEYnBNylPwRIc/VGeZgRUUoF9vLRFsMpsbKF2s\\n7Doaf44KB6vm+0Q7LWOwaL5EYaFZ6QaIgYhgONz0BqCIDbHmaFmvI6xZ7L8ndLyJ\\nMmhbPheLoVm3oUqAlYB6lCKrgiwbUSx7Xib3qAECgYEA049MS5aKycVEK0R8maxi\\n+v3qCxTC+13BKtsnr6qHfJyvIn1f2Xy1l5LlaB4MUxBXc3zrrajXPPmvQ3K8ADzY\\ny/piRQdsC1IW002m1IS40d4efsqipZCEdGJT6kZZMrP4MWogeOq+7nWxF9qFG2Da\\nW3+YKLgCysJBqkXQrIdMdfg=\\n-----END PRIVATE KEY-----\\n\",\r\n  \"client_email\": \"firebase-adminsdk-q3982@where-you-at-88cb5.iam.gserviceaccount.com\",\r\n  \"client_id\": \"108648506301760800235\",\r\n  \"auth_uri\": \"https://accounts.google.com/o/oauth2/auth\",\r\n  \"token_uri\": \"https://oauth2.googleapis.com/token\",\r\n  \"auth_provider_x509_cert_url\": \"https://www.googleapis.com/oauth2/v1/certs\",\r\n  \"client_x509_cert_url\": \"https://www.googleapis.com/robot/v1/metadata/x509/firebase-adminsdk-q3982%40where-you-at-88cb5.iam.gserviceaccount.com\"\r\n}\r\n";

		/*		public void sendFcmAsync() {
					// Create a list containing up to 500 registration tokens.
					// These registration tokens come from the client FCM SDKs.
					var registrationTokens = new List<string>()
					{
						"cI48-caQSbCSLK1mjD3jJF:APA91bHc_BBQo49LwuRc53b8VOc5wlw8CoAHveWVCBtp8NmDqneHvPIGUAEZjGlGJabKpxb30xmbXn2vYQ38fouGoX2dLNtOjW74uLtNYK7Kb4vOGfmDE7tp-1uaD-64I6xTD3MobR1x",
						// ...
						"dI48-caQSbCSLK1mjD3jJF:APA91bHc_BBQo49LwuRc53b8VOc5wlw8CoAHveWVCBtp8NmDqneHvPIGUAEZjGlGJabKpxb30xmbXn2vYQ38fouGoX2dLNtOjW74uLtNYK7Kb4vOGfmDE7tp-1uaD-64I6xTD3MobR1x",
					};
					var message = new MulticastMessage() {
						Tokens = registrationTokens,
						Data = new Dictionary<string, string>()
						{
							{ "score", "850" },
							{ "time", "2:45" },
						},
					};

					FirebaseApp.Create(new AppOptions() {
						Credential = Google.Apis.Auth.OAuth2.GoogleCredential.FromJson(FCM_JSON),
					});

					var response = FirebaseMessaging.DefaultInstance.SendMulticastAsync(message).Result;
					// See the BatchResponse reference documentation
					// for the contents of response.
					// Console.WriteLine($"{response.SuccessCount} messages were sent successfully");
				}*/

		public async Task<bool> NotifyAsync(string to, string title, string body) {
			try {
				// Get the server key from FCM console
				var serverKey = string.Format("key={0}", FCM_SERVER_KEY);

				// Get the sender id from FCM console
				var senderId = string.Format("id={0}", FCM_SENDER_KEY);

				var data = new {
					to, // Recipient device token
					notification = new { title, body }
				};

				// Using Newtonsoft.Json
				var jsonBody = JsonConvert.SerializeObject(data);

				using (var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://fcm.googleapis.com/fcm/send")) {
					httpRequest.Headers.TryAddWithoutValidation("Authorization", serverKey);
					httpRequest.Headers.TryAddWithoutValidation("Sender", senderId);
					httpRequest.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

					using (var httpClient = new HttpClient()) {
						HttpResponseMessage result = httpClient.SendAsync(httpRequest).Result;
						string resultstring = result.Content.ReadAsStringAsync().Result;

						FcmResults fcmResult = new FcmResults(resultstring);
						if (fcmResult.wasDeviceUnRegistered() == TriStateResult.YES) {
							// Do something to remove the now invalid FCM token from the server.
							MyDb db = new MyDb();
							db.RemoveFcmToken(to);
						}

						if (result.IsSuccessStatusCode) {
							return true;
						} else {
							// Use result.StatusCode to handle failure
							// Your custom error handler here
							MyDb.WriteLogLine($"Error sending notification. Status Code: {result.StatusCode}");
						}
					}
				}
			} catch (Exception ex) {
				MyDb.WriteLogLine($"Exception thrown in Notify Service: {ex}");
			}

			return false;
		}

	}
}