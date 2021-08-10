using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using WhereYouAt;
using System.Web.Http;
using static WhereYouAtRestApi.AppCode.Requests;
using WhereYouAtApi.AppCode;

namespace WhereYouAt.Controllers {
	public class ValuesController : ApiController {

		// OPERATIONS
		public const string CREATE_NEW_TRIP = "createnewtrip";
		public const string JOIN_TRIP = "jointrip";
		public const string UPSERT_USER = "upsertuser";
		public const string UPDATE_TRIP = "updatetrip";
		public const string UPSERT_FCMTOKEN = "upsertfcmtoken";
		public const string UPSERT_AVATAR = "upsertavatar";
		public const string LOCATION_UPDATE_REQUESTED = "locationupdaterequested";
		public const string LEAVE_TRIP = "leavetrip";
		public const string REQUEST_JOIN = "requestjoin";
		public const string TRIP_EXISTS = "tripexists";
		public const string SEND_MESSAGE = "sendmsg";
		public const string GET_TRIP_MESSAGES = "gettripmessages";

		// ERROR CODES
		public const string ERROR_TRIP_NOT_FOUND = "ERROR_TRIP_NOT_FOUND";

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
			Api.OperationResults operationResults = new Api.OperationResults();

			// Potential vars used in switch statement
			string userid, email, photourl, fcmtoken, displayname, locationJson, orderargument, requestingUser, msgsender, msgreceiver, msgbody, tripcode = null, base64Avatar = null;
			int limit, location_type;
			double latitude, longitude;
			double accuracy_meters;
			decimal velocity = 0;
			Backend.MyDb db = new Backend.MyDb();
			Api.OperationResult operationResult;

			try {
				// ------------------------------------------------------------------------------------
				switch (value.Function.ToLower()) {
					// ------------------------------------------------------------------------------------


					// ------------------------------------------------------------------------------------
					case CREATE_NEW_TRIP:
						// ------------------------------------------------------------------------------------

						userid = (string)value.Arguments[0].value;

						try {

							db = new Backend.MyDb();
							operationResult = db.CreateTrip(userid);
							if (operationResult.wasSuccessful) {
								tripcode = operationResult.result.ToString();
								Backend.MyDb.WriteLogLine("Created a new trip: " + tripcode);
								Api.Trip newTrip = new Api.Trip(tripcode);
								// Add the requesting user as a trip member
								string creatingUsersEmail = db.GetEmailByUserid(userid);
								Api.TripMember creatingUser = new Api.TripMember(creatingUsersEmail);
								newTrip.members.Add(creatingUser);
								newTrip.createdon = DateTime.UtcNow;
								newTrip.createdby = creatingUser;

								Api.TripReport locUpdates = null;

								try {
									locUpdates = (Api.TripReport)db.UpdateTrip(tripcode, userid, Backend.MyDb.LOCATION_TYPE_ACTIVE, null).result;
								} catch (Exception e2) { }

								operationResult = new Api.OperationResult(true, "Create new trip", locUpdates);

								// Send an FCM with a trip report payload to trip members if the db update was successful (which will be one member but still).
								if (operationResult.wasSuccessful) {
									List<Google.User> users = db.GetTripMembers(tripcode);
									if (users != null) {
										Fcm.Engine fcmEnginecreateNewTrip = new Fcm.Engine();
										foreach (WhereYouAt.Google.User u in users) {
											Dictionary<string, string> fcms = db.GetAllFcmsByUserid(u.id);
											if (fcms != null) {
												foreach (string key in fcms.Keys) {
													Fcm.Message message = new Fcm.Message();
													message.to = key;
													message.data = new Fcm.Payload();
													message.data.obj = locUpdates;
													message.data.opcode = Fcm.Payload.OP_CODE_CREATED_TRIP;
													message.data.title = "Created a trip ya fuck.";
													fcmEnginecreateNewTrip.NotifyAsync(message);
												}
											}
										}
									}
								}

								operationResults.allResults.Add(operationResult);

								response = Request.CreateResponse(HttpStatusCode.OK, operationResults);
								return response;
							}
						} catch (Exception e) {
							Backend.MyDb.WriteLogLine(e.Message);
							operationResults.allResults.Add(new Api.OperationResult(false, "Creating trip", e.Message));
						}

						response = Request.CreateResponse(HttpStatusCode.OK, operationResults);
						return response;

					// ------------------------------------------------------------------------------------
					case JOIN_TRIP:
						// ------------------------------------------------------------------------------------

						userid = (string)value.Arguments[0].value;
						tripcode = (string)value.Arguments[1].value;

						// If tripcode not found in the tripentries table return a failed operation.
						if (!db.TripExists(tripcode)) {
							operationResult = new Api.OperationResult(false, ERROR_TRIP_NOT_FOUND);
							operationResults.allResults.Add(operationResult);
							response = Request.CreateResponse(HttpStatusCode.OK, operationResults);
						}

						// The OperationResult returned from the JoinTrip method has a result value that can be cast to a MemberLocUpdates object.
						operationResult = db.JoinTrip(tripcode, userid);

						if (operationResult.wasSuccessful) {
							// Cast the OperationResult.result property to a MemberLocUpdates object.
							Api.TripReport locUpdates = (Api.TripReport)operationResult.result;

							if (locUpdates != null) {
								// For each member in the MemberLocUpdates list send an FCM with the MemberLocUpdates object to each of their tokens on file
								foreach (Api.MemberLocUpdate l in locUpdates.list) {
									Dictionary<string, string> fcms = db.GetAllFcmsByUserid(l.userid);
									if (fcms != null) {
										foreach (string key in fcms.Keys) {
											Fcm.Message message = new Fcm.Message();
											message.to = key;
											Fcm.Payload payload = new Fcm.Payload();
											payload.title = "A user has joined the group!";
											payload.opcode = Fcm.Payload.OP_CODE_USER_JOINED_TRIP;
											payload.obj = operationResult.result;
											message.data = payload;
											Fcm.Engine engine = new Fcm.Engine();
											engine.NotifyAsync(message);
											Backend.MyDb.WriteLogLine("Sent fcm payload!");
										}
									}
								}
							}
						}
						operationResults.allResults.Add(operationResult);
						response = Request.CreateResponse(HttpStatusCode.OK, operationResults);
						return response;

					// ------------------------------------------------------------------------------------
					case REQUEST_JOIN:
						// ------------------------------------------------------------------------------------
						userid = (string)value.Arguments[0].value;
						tripcode = (string)value.Arguments[1].value;

						Dictionary<string, string> dicTokens = db.GetAllFcmsByUserid(userid);
						foreach (string token in dicTokens.Keys) {
							Fcm.Engine engine = new Fcm.Engine();
							engine.SendJoinRequest(token, "Request to join trip " + tripcode, "Someone has requested that you show them where you are at!");
						}
						operationResult = new Api.OperationResult(true, "sweet");
						operationResults.allResults.Add(operationResult);
						return Request.CreateResponse(HttpStatusCode.OK, operationResults);

					// ------------------------------------------------------------------------------------
					case UPSERT_USER:
						// ------------------------------------------------------------------------------------

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

					// ------------------------------------------------------------------------------------
					case UPSERT_FCMTOKEN:
						// ------------------------------------------------------------------------------------

						userid = (string)value.Arguments[0].value;
						fcmtoken = (string)value.Arguments[1].value;
						operationResults.allResults.Add(UpsertFcmToken(userid, fcmtoken));
						return Request.CreateResponse(HttpStatusCode.OK, operationResults);

					// ------------------------------------------------------------------------------------
					case LEAVE_TRIP:
						// ------------------------------------------------------------------------------------

						userid = (string)value.Arguments[0].value;
						tripcode = (string)value.Arguments[1].value;
						db = new Backend.MyDb();

						operationResult = db.RemoveAllUserEntries(userid);
						operationResults.allResults.Add(operationResult);

						if (operationResult.wasSuccessful) {
							// Check if the removal was successful and if so, send FCMs to all members reporting that this douchebag left.
							List<string> tokens = db.GetTripMembersFcmToken(tripcode);
							Fcm.Engine engine = new Fcm.Engine();
							foreach (string token in tokens) {
								Fcm.Message message = new Fcm.Message();
								Fcm.Payload payload = new Fcm.Payload();
								payload.opcode = Fcm.Payload.OP_CODE_USER_LEFT_TRIP;
								payload.title = "Someone left the group";
								payload.obj = db.GetGoogleUser(userid);
								message.data = payload;
								message.to = token;
								engine.NotifyAsync(message);
							}
						}

						return Request.CreateResponse(HttpStatusCode.OK, operationResults);

					// ------------------------------------------------------------------------------------
					case SEND_MESSAGE:
						// ------------------------------------------------------------------------------------

						msgsender = (string)value.Arguments[0].value;
						msgbody = (string)value.Arguments[1].value;
						tripcode = (string)value.Arguments[2].value;

						// Quick validation of message
						if (msgsender == null || msgbody == null) {
							operationResults.allResults.Add(new Api.OperationResult(false, "Sender or message body was not supplied"));
							return Request.CreateResponse(HttpStatusCode.OK, operationResults);
						}

						// Get user objects for sender and receiver
						Google.User sendingUser = db.GetGoogleUser(msgsender);

						// Validate sending user exists.
						if (sendingUser == null) {
							operationResults.allResults.Add(new Api.OperationResult(false, "Sending user object was not found in the database"));
							return Request.CreateResponse(HttpStatusCode.OK, operationResults);
						}

						// Get all trip member's fcm tokens
						List<string> allTokens = db.GetTripMembersFcmToken(tripcode);

						// Construct the UserMessage object and append it to the Messages table.  If this fails, return a fail and send zero FCMs.
						UserMessage usermsg = new UserMessage(sendingUser, msgbody, tripcode);
						bool appendResult = usermsg.AppendToDb();
						if (appendResult == false) {
							operationResults.allResults.Add(new Api.OperationResult(false, "Failed to append message to the Messages table - This was a server error with our shit!"));
							return Request.CreateResponse(HttpStatusCode.OK, operationResults);
						}

						List<UserMessage> allMessages = db.GetAllUserMessages(tripcode, 100, "asc");

						// Construct the FCM message as well as its payload (which will be the last 100 messages in this trip)
						Fcm.Engine fcmEngine = new Fcm.Engine();
						try {
							foreach (string token in allTokens) {
								Fcm.Message message = new Fcm.Message();
								Fcm.Payload payload = new Fcm.Payload();
								payload.opcode = Fcm.Payload.OP_CODE_USER_MESSAGE;
								payload.title = sendingUser.fullname + " sent a message";
								payload.obj = allMessages;
								message.data = payload;
								message.to = token;
								fcmEngine.NotifyAsync(message);
							}
						} catch (Exception exMsgSending) {
							operationResults.allResults.Add(new Api.OperationResult(false, exMsgSending.Message));
							return Request.CreateResponse(HttpStatusCode.OK, operationResults);
						}

						// Construct and return successfull operation results.
						operationResults.allResults.Add(new Api.OperationResult(true, "No obvious sending FCMs"));
						return Request.CreateResponse(HttpStatusCode.OK, operationResults);

					// ------------------------------------------------------------------------------------
					case GET_TRIP_MESSAGES:
						// ------------------------------------------------------------------------------------
						tripcode = (string)value.Arguments[0].value;
						String strLimit = (string) value.Arguments[1].value;
						limit = int.Parse(strLimit);
						orderargument = (string)value.Arguments[2].value;
						db = new Backend.MyDb();
						List<UserMessage> messages = db.GetAllUserMessages(tripcode, limit, orderargument);
						operationResults.allResults.Add(new Api.OperationResult(true, "get all trip messages", messages));
						return Request.CreateResponse(HttpStatusCode.OK, operationResults);

					// ------------------------------------------------------------------------------------
					case UPSERT_AVATAR:
					// ------------------------------------------------------------------------------------

						userid = (string)value.Arguments[0].value;
						base64Avatar = (string)value.Arguments[1].value;
						operationResults.allResults.Add(UpsertFcmToken(userid, base64Avatar));
						return Request.CreateResponse(HttpStatusCode.OK, operationResults);

					// ------------------------------------------------------------------------------------
					case UPDATE_TRIP:
					// ------------------------------------------------------------------------------------

						userid = (string)value.Arguments[0].value;
						tripcode = (string)value.Arguments[1].value;
						string ltype = (string)value.Arguments[2].value;
						location_type = int.Parse(ltype);
						if (value.Arguments[3] != null) {
							locationJson = (string)value.Arguments[3].value;
						} else {
							locationJson = null;
						}
						/*latitude = (double)value.Arguments[2].value;
						longitude = (double)value.Arguments[3].value;
						accuracy_meters = (double)value.Arguments[4].value;
						if (value.Arguments[6] != null) {
							string strVel = value.Arguments[6].value.ToString();
							if (strVel != null) {
								velocity = Decimal.Parse(strVel);
							}
						} else {
							velocity = 0;
						}*/

						// Update the actual database with the user's loc.
						Api.OperationResult updateDbResult = new Api.OperationResult();
						try {
							db = new Backend.MyDb();
							updateDbResult = db.UpdateTrip(tripcode, userid, location_type, locationJson);
							// updateDbResult = db.UpdateTrip(tripcode, userid, latitude, longitude, accuracy_meters, location_type, velocity, locationJson);
							operationResults.allResults.Add(updateDbResult);
						} catch (Exception e) {
							Backend.MyDb.WriteLogLine(e.Message);
							operationResults.allResults.Add(new Api.OperationResult(false, "Updating trip", e.Message));
						}

						// If db update worked then assemble a MemberLocs object to use as the payload in an FCM message.
						if (updateDbResult.wasSuccessful) {
							// Get all trip member's fcm ids
							Backend.MyDb myDb = new Backend.MyDb();
							List<string> tokens = myDb.GetTripMembersFcmToken(tripcode);
							List<Google.User> tripmembers = myDb.GetTripMembers(tripcode);
							Fcm.Engine fcm = new Fcm.Engine();
							Api.TripReport locUpdates = myDb.GetTripUpdateReport(tripcode, userid);

							// Send out fcm notifications to all members
							foreach (string token in tokens) {
								// Build the FCM object
								Fcm.Message message = new Fcm.Message();
								message.to = token;
								// Give the FCM an operation code, title and a serialized payload
								Fcm.Payload data = new Fcm.Payload();
								data.title = "Location was updated by " + locUpdates.GetUserFromMemberLocs(userid).fullname;
								data.opcode = Fcm.Payload.OP_CODE_TRIP_UPDATED;
								data.obj = locUpdates.toJson();
								message.data = data;
								_ = fcm.NotifyAsync(message);
							}

						}

						// -= Respond to the rest api caller. =-
						// The caller will also soon receive an FCM (as they are part of the group) which will be redundant as 
						// this response contains (packaged differently) the same payload as that enroute FCM.
						return Request.CreateResponse(HttpStatusCode.OK, operationResults);

					// ------------------------------------------------------------------------------------
					case LOCATION_UPDATE_REQUESTED:
					// ------------------------------------------------------------------------------------

						userid = (string)value.Arguments[0].value;
						tripcode = (string)value.Arguments[1].value;
						requestingUser = (string)value.Arguments[2].value;

						// Get all trip member's fcm ids
						Backend.MyDb myDb2 = new Backend.MyDb();
						Dictionary<string, string> tokens2 = myDb2.GetAllFcmsByUserid(userid);
						Fcm.Engine fcm2 = new Fcm.Engine();

						foreach (string token in tokens2.Keys) {
							Fcm.Message message = new Fcm.Message();
							Fcm.Payload payload = new Fcm.Payload(Fcm.Payload.OP_CODE_LOCATION_REQUESTED, myDb2.GetGoogleUser(userid));
							message.to = token;
							message.data = payload;
							message.SendAsync();
						}

						operationResults = new Api.OperationResults(new Api.OperationResult(true, "Requested location updates via FCM"));
						return Request.CreateResponse(HttpStatusCode.OK, operationResults);

					// ------------------------------------------------------------------------------------
					case TRIP_EXISTS:
					// ------------------------------------------------------------------------------------

						/*userid = (string)value.Arguments[0].value;
						tripcode = (string)value.Arguments[1].value;
						requestingUser = (string)value.Arguments[2].value;

						// Get all trip member's fcm ids
						Backend.MyDb myDb2 = new Backend.MyDb();
						Dictionary<string, string> tokens2 = myDb2.GetAllFcmsByUserid(userid);
						Fcm.Engine fcm2 = new Fcm.Engine();

						foreach (string token in tokens2.Keys) {
							Fcm.Message message = new Fcm.Message();
							Fcm.Payload payload = new Fcm.Payload(Fcm.Payload.OP_CODE_LOCATION_REQUESTED, myDb2.GetGoogleUser(userid));
							message.to = token;
							message.data = payload;
							message.SendAsync();
						}

						operationResults = new Api.OperationResults(new Api.OperationResult(true, "Requested location updates via FCM"));*/
						return Request.CreateResponse(HttpStatusCode.OK, operationResults);

					// ------------------------------------------------------------------------------------
					default:
					// ------------------------------------------------------------------------------------
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

		/// <summary>
		/// Creates a trip, adds the user to it and finally constructs a MemberLocUpdates object and assigns it 
		/// to the OperationResult object's "result" property.  An FCM will be sent to all tokens on file for each
		/// user in the MemberLocUpdates object also containing the MemberLocUpdates object.
		/// </summary>
		/// <param name="userid"></param>
		/// <param name="isTestmode"></param>
		/// <returns>An OperationResult object with a "result" property that can be cast to a MemberLocUpdates object.</returns>
		/*public Api.OperationResult CreateTrip(string userid, bool isTestmode) {
			try {
				Backend.MyDb db = new Backend.MyDb();
				Api.OperationResult operationResult = db.CreateTrip(userid, isTestmode);
				if (operationResult.wasSuccessful) {
					string tripcode = operationResult.result.ToString();
					Backend.MyDb.WriteLogLine("Created a new trip: " + tripcode);
					Api.Trip newTrip = new Api.Trip(tripcode);
					// Add the requesting user as a trip member
					string creatingUsersEmail = db.GetEmailByUserid(userid);
					Api.TripMember creatingUser = new Api.TripMember(creatingUsersEmail);
					newTrip.members.Add(creatingUser);
					newTrip.createdon = DateTime.Now;
					newTrip.createdby = creatingUser;

					Api.TripReport locUpdates = null;

					try {
						// locUpdates = (Api.TripReport)UpdateTrip(userid, tripcode, 0, 0, 0, LOCATION_TYPE_PASSIVE).result;
					} catch (Exception e2) { }

					operationResult = new Api.OperationResult(true, "Create new trip", locUpdates);

					// Send an FCM with a trip report payload to trip members (which will be one member but still).
					if (operationResult.wasSuccessful) {
						List<Google.User> users = db.GetTripMembers(tripcode);
						if (users != null) {
							Fcm.Engine fcmEngine = new Fcm.Engine();
							foreach (WhereYouAt.Google.User u in users) {
								Dictionary<string, string> fcms = db.GetAllFcmsByUserid(u.id);
								if (fcms != null) {
									foreach (string key in fcms.Keys) {
										Fcm.Message message = new Fcm.Message();
										message.to = key;
										message.data = new Fcm.Payload();
										message.data.obj = locUpdates;
										message.data.opcode = Fcm.Payload.OP_CODE_CREATED_TRIP;
										message.data.title = "Created a trip ya fuck.";
									}
								}
							}
						}
					}

					return operationResult;
				}
				return operationResult;
			} catch (Exception e) {
				Backend.MyDb.WriteLogLine(e.Message);
				return new Api.OperationResult(false, "Create new trip", e.Message);
			}
		}*/
		
		/// <summary>
		/// Adds the user to the trip and constructs a MemberLocUpdates object which is attached to the result property of 
		/// an OperationResult object.
		/// </summary>
		/// <param name="userid"></param>
		/// <param name="tripcode"></param>
		/// <returns>An OperationResult object whose "result" property can be cast to a MemberLocUpdates object.</returns>
		/*public Api.OperationResult JoinTrip(string userid, string tripcode) {
			Backend.MyDb db = new Backend.MyDb();

			// The OperationResult returned from the JoinTrip method has a result value that can be cast to a MemberLocUpdates object.
			Api.OperationResult operationResult = db.JoinTrip(tripcode, userid);

			if (operationResult.wasSuccessful) {
				// Cast the OperationResult.result property to a MemberLocUpdates object.
				Api.TripReport locUpdates = (Api.TripReport)operationResult.result;
				
				if (locUpdates != null) {
					// For each member in the MemberLocUpdates list send an FCM with the MemberLocUpdates object to each of their tokens on file
					foreach (Api.MemberLocUpdate l in locUpdates.list) {
						Dictionary<string, string> fcms = db.GetAllFcmsByUserid(l.userid);
						if (fcms != null) {
							foreach (string key in fcms.Keys) {
								Fcm.Message message = new Fcm.Message();
								message.to = key;
								Fcm.Payload payload = new Fcm.Payload();
								payload.title = "A user has joined the group!";
								payload.opcode = Fcm.Payload.OP_CODE_USER_JOINED_TRIP;
								payload.obj = operationResult.result;
								message.data = payload;
								Fcm.Engine engine = new Fcm.Engine();
								engine.NotifyAsync(message);
								Backend.MyDb.WriteLogLine("Sent fcm payload!");
							}
						}
					}
				}
			}
			return operationResult;
		}*/

		/// <summary>
		/// Updates or creates a user in the base users table
		/// </summary>
		/// <param name="userid">Unique Google userid</param>
		/// <param name="email">Email address</param>
		/// <param name="photourl">Url to the user's public Google profile picture</param>
		/// <param name="displayname">User's display name</param>
		/// <returns></returns>
		public Api.OperationResult UpsertUser(string userid, string email, string photourl, string displayname) {
			Backend.MyDb mydb = new Backend.MyDb();
			// Update/create user in the base users table
			return mydb.UpsertUser(userid, email, photourl, displayname);
		}

		/// <summary>
		/// Updates or creates a record in the fcmtokens table
		/// </summary>
		/// <param name="userid">Unique Google userid</param>
		/// <param name="fcmtoken">Device-specific FCM token as generated by the user's device</param>
		/// <returns></returns>
		public Api.OperationResult UpsertFcmToken(string userid, string fcmtoken) {
			Backend.MyDb myDb = new Backend.MyDb();
			// Update/create record in the fcmtokens table
			return myDb.UpsertFcmToken(userid, fcmtoken);
		}

		public Api.OperationResult UpsertAvatar(string userid, string base64) {
			try {
				Backend.MyDb mydb = new Backend.MyDb();
				Api.OperationResult result = mydb.UpsertAvatar(userid, base64);
				return result;
			} catch (Exception e) {
				return new Api.OperationResult(false, "upsert avatar", e.Message);
			}
		}
	}
}
