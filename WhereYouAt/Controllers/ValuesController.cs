using Newtonsoft.Json;
using RestApi.AppCode;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WhereYouAt.AppCode;
using WhereYouAtRestApi.AppCode;
using static WhereYouAtRestApi.AppCode.Requests;

namespace WhereYouAt.Controllers {
	public class ValuesController : ApiController {

		public const string CREATE_NEW_TRIP = "createnewtrip";
		public const string UPSERT_USER = "upsertuser";
		public const string UPDATE_TRIP = "updatetrip";
		public const string UPSERT_FCMTOKEN = "upsertfcmtoken";
		public const string UPSERT_AVATAR = "upsertavatar";

		// GET api/values 2 ss
		public IEnumerable<string> Get() {
			return new string[] { "value1", "value2" };
		}

		// GET api/values/5
		public string Get(int id) {
			return "value supplied: " + id;
		}

		// POST api/values
		public HttpResponseMessage PerformAction([FromBody] Request value) {

			HttpResponseMessage response = new HttpResponseMessage();
			OperationResults operationResults = new OperationResults();

			// Potential vars used in switch statement
			string userid, email, photourl, fcmtoken, displayname, tripcode, base64Avatar;
			double latitude, longitude;
			double accuracy_meters;
			bool isTestMode = false;

			try {
				switch (value.Function.ToLower()) {
					case CREATE_NEW_TRIP:
						userid = (string)value.Arguments[0].value;
						if (value.Arguments[1] != null) {
							isTestMode = (bool)value.Arguments[1].value;
						}
						operationResults.allResults.Add(CreateTrip(userid, isTestMode));
						response = Request.CreateResponse(HttpStatusCode.OK, operationResults);
						return response;
					case UPSERT_USER:
						userid = (string)value.Arguments[0].value;
						email = (string)value.Arguments[1].value;
						photourl = "";
						fcmtoken = null;
						if (value.Arguments[2] != null) {
							photourl = (string)value.Arguments[2].value;
						}
						displayname = (string)value.Arguments[3].value;
						operationResults.allResults.Add(UpsertUser(userid, email, photourl, displayname));
						response = Request.CreateResponse(HttpStatusCode.OK, operationResults);
						return response;
					case UPSERT_FCMTOKEN:
						userid = (string)value.Arguments[0].value;
						fcmtoken = (string)value.Arguments[1].value;
						operationResults.allResults.Add(UpsertFcmToken(userid, fcmtoken));
						return Request.CreateResponse(HttpStatusCode.OK, operationResults);
					case UPSERT_AVATAR:
						userid = (string)value.Arguments[0].value;
						base64Avatar = (string)value.Arguments[1].value;
						operationResults.allResults.Add(UpsertFcmToken(userid, base64Avatar));
						return Request.CreateResponse(HttpStatusCode.OK, operationResults);
					case UPDATE_TRIP:
						userid = (string)value.Arguments[0].value;
						tripcode = (string)value.Arguments[1].value;
						latitude = (double)value.Arguments[2].value;
						longitude = (double)value.Arguments[3].value;
						accuracy_meters = (double)value.Arguments[4].value;

						OperationResult updateDbResult = UpdateTrip(userid, tripcode, latitude, longitude, accuracy_meters);
						operationResults.allResults.Add(updateDbResult);

						if (updateDbResult.wasSuccessful) {

							// Get all trip member's fcm ids
							MyDb myDb = new MyDb();
							List<string> tokens = myDb.GetTripMembersFcmToken(tripcode);
							FcmEngine fcm = new FcmEngine();
							MemberLocUpdates locUpdates = myDb.GetTripUpdateReport(tripcode, userid);

							// Send out fcm notifications to all members
							foreach (string token in tokens) {
								// discard results as we cannot be bothered to await them

								_ = fcm.NotifyAsync(token, "<title here>", locUpdates.toJson());
							}

						}

						return Request.CreateResponse(HttpStatusCode.OK, operationResults);
					default:
						response = Request.CreateErrorResponse(HttpStatusCode.BadRequest, "No argument supplied");
						return response;
				}
			} catch (Exception e) {
				response = Request.CreateErrorResponse(HttpStatusCode.BadRequest, e.Message);
				return response;
			}
		}

		// PUT api/values/5
		public void Put(int id, [FromBody] string value) {

		}

		// DELETE api/values/5
		public void Delete(int id) {

		}

		public OperationResult CreateTrip(string username, bool isTestmode) {
			try {
				MyDb db = new MyDb();
				OperationResult operationResult = db.CreateTrip(username, isTestmode);
				if (operationResult.wasSuccessful) {
					string tripcode = operationResult.result;
					MyDb.WriteLogLine("Created a new trip: " + tripcode);
					Trip newTrip = new Trip(tripcode);
					OperationResult result = new OperationResult(true, "Create new trip", newTrip.toJson());
					return result;
				}
				return operationResult;
			} catch (Exception e) {
				MyDb.WriteLogLine(e.Message);
				return new OperationResult(false, "Create new trip", e.Message);
			}
		}

		/// <summary>
		/// Updates or creates a user in the base users table
		/// </summary>
		/// <param name="userid">Unique Google userid</param>
		/// <param name="email">Email address</param>
		/// <param name="photourl">Url to the user's public Google profile picture</param>
		/// <param name="displayname">User's display name</param>
		/// <returns></returns>
		public OperationResult UpsertUser(string userid, string email, string photourl, string displayname) {
			MyDb mydb = new MyDb();
			// Update/create user in the base users table
			return mydb.UpsertUser(userid, email, photourl, displayname);
		}

		/// <summary>
		/// Updates or creates a record in the fcmtokens table
		/// </summary>
		/// <param name="userid">Unique Google userid</param>
		/// <param name="fcmtoken">Device-specific FCM token as generated by the user's device</param>
		/// <returns></returns>
		public OperationResult UpsertFcmToken(string userid, string fcmtoken) {
			MyDb myDb = new MyDb();
			// Update/create record in the fcmtokens table
			return myDb.UpsertFcmToken(userid, fcmtoken);
		}

		/// <summary>
		/// Updates an existing trip in the database.
		/// </summary>
		/// <param name="userid">The user initiating the update</param>
		/// <param name="tripcode">The trip to update</param>
		/// <param name="latitude">User's latitude</param>
		/// <param name="longitude">User's longitude</param>
		/// <returns>An OperationResult object with a success/failure result boolean and some strings summarizing the operation.</returns>
		public OperationResult UpdateTrip(string userid, string tripcode, double latitude, double longitude, double accuracy_meters) {
			try {
				MyDb db = new MyDb();
				OperationResult result = db.UpdateTrip(tripcode, userid, latitude, longitude, accuracy_meters);
				return result;
			} catch (Exception e) {
				MyDb.WriteLogLine(e.Message);
				return new OperationResult(false, "Updating trip", e.Message);
			}
		}
		public OperationResult UpsertAvatar(string userid, string base64) {
			try {
				MyDb mydb = new MyDb();
				OperationResult result = mydb.UpsertAvatar(userid, base64);
				return result;
			} catch (Exception e) {
				return new OperationResult(false, "upsert avatar", e.Message);
			}
		}
	}
}
