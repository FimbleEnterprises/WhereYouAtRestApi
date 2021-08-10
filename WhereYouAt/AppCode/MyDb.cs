using WhereYouAt;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using WhereYouAt.Fcm;
using WhereYouAt.Google;
using WhereYouAtRestApi.AppCode;
using WhereYouAtApi.AppCode;

namespace WhereYouAt.Backend {
	public class MyDb {

		public const int LOCATION_TYPE_PASSIVE = 0;
		public const int LOCATION_TYPE_ACTIVE = 1;
		public const int LOCATION_TYPE_MISC1 = 2;
		public const int LOCATION_TYPE_MISC2 = 3;


		public static string GetConnectionString() {
			return System.Configuration.ConfigurationManager.ConnectionStrings["WhereYouAtDb"].ConnectionString;
		}

		public void GetAllTrips() {
			SqlConnection myConn = new SqlConnection(GetConnectionString());
			myConn.Open();
			SqlCommand cmd = new SqlCommand("SELECT * FROM [TripTable]", myConn);
			SqlDataAdapter da = new SqlDataAdapter(cmd);
			SqlCommandBuilder cb = new SqlCommandBuilder(da);

			DataSet ds = new DataSet();
			da.Fill(ds);

			if (ds.Tables[0].Rows.Count > 0) {
				foreach (DataRow row in ds.Tables[0].Rows) {
					Console.WriteLine(row["tripcode"].ToString());
				}
			}
			myConn.Close();
		}

		private DataRow GetTrip(string tripcode) {
			SqlConnection myConn = new SqlConnection(GetConnectionString());
			myConn.Open();
			SqlCommand cmd = new SqlCommand("SELECT * FROM [TripTable] where [tripcode] = @tripcode AND [validuntil] > @nowdatetime", myConn);
			cmd.Parameters.AddWithValue("@tripcode", tripcode);
			cmd.Parameters.AddWithValue("@nowdatetime", DateTime.UtcNow);
			SqlDataAdapter da = new SqlDataAdapter(cmd);

			DataSet ds = new DataSet();
			da.Fill(ds);
			myConn.Close();

			return ds.Tables[0].Rows[0];
		}

		/// <summary>
		/// Checks if a valid trip code exists in the TripTable.  Valid trips have a validUntil value that is greater than the current system date/time.
		/// </summary>
		/// <param name="tripcode"></param>
		/// <returns></returns>
		public bool TripCodeAlreadyExists(string tripcode) {
			SqlConnection myConn = new SqlConnection(GetConnectionString());
			myConn.Open();
			SqlCommand cmd = new SqlCommand("SELECT [tripcode] FROM [TripTable] WHERE [tripcode] = @code AND [validUntil] > @nowdatetime", myConn);
			cmd.Parameters.AddWithValue("@code", tripcode);
			cmd.Parameters.AddWithValue("@nowdatetime", DateTime.UtcNow);
			SqlDataAdapter da = new SqlDataAdapter(cmd);

			DataSet ds = new DataSet();
			da.Fill(ds);
			myConn.Close();
			return ds.Tables[0].Rows.Count > 0;
		}

		/// <summary>
		/// Returns the number of entries with the supplied trip code in the TripTable.
		/// </summary>
		/// <param name="tripcode"></param>
		/// <returns></returns>
		public int TripCodeCount(string tripcode) {
			SqlConnection myConn = new SqlConnection(GetConnectionString());
			myConn.Open();
			SqlCommand cmd = new SqlCommand("SELECT [tripcode] FROM [TripTable] WHERE [tripcode] = @code", myConn);
			cmd.Parameters.AddWithValue("@code", tripcode);
			SqlDataAdapter da = new SqlDataAdapter(cmd);

			DataSet ds = new DataSet();
			da.Fill(ds);
			myConn.Close();
			return ds.Tables[0].Rows.Count;
		}

		/// <summary>
		/// Returns the number of entries with the supplied trip code in the TripTable that are still valid.  This value should ideally be zero if creating a trip or one if querying one.
		/// </summary>
		/// <param name="tripcode"></param>
		/// <returns></returns>
		public int ValidTripCodeCount(string tripcode) {
			SqlConnection myConn = new SqlConnection(GetConnectionString());
			myConn.Open();
			SqlCommand cmd = new SqlCommand("SELECT [tripcode] FROM [TripTable] WHERE [tripcode] = @code AND [validUntil] > @nowdatetime", myConn);
			cmd.Parameters.AddWithValue("@code", tripcode);
			cmd.Parameters.AddWithValue("@nowdatetime", DateTime.UtcNow);
			SqlDataAdapter da = new SqlDataAdapter(cmd);

			DataSet ds = new DataSet();
			da.Fill(ds);
			myConn.Close();
			return ds.Tables[0].Rows.Count;
		}

		/// <summary>
		/// Generates a random string of characters of the given length.  Uses the the GetRandomFileName() method from System.IO which leverages the crypto library to ensure better random results.  
		/// </summary>
		/// <param name="length">The character count of the resultant string</param>
		/// <returns></returns>
		private string generateTripcode(int length) {
			string path = Path.GetRandomFileName();
			path = path.Replace(".", ""); // Remove period.
			return path.Substring(0, length);  // Return 8 character string
		}

		public enum OptionType {
			BOOL, INT, BIGINT, STRING, BLOB, MONEY, FLOAT, DATETIME
		}

		private static string getOptionsValueColumnName(OptionType type) {
			switch (type) {
				case OptionType.BIGINT:
					return "bigIntValue";
				case OptionType.INT:
					return "intValue";
				case OptionType.BOOL:
					return "boolValue";
				case OptionType.STRING:
					return "strValue";
				case OptionType.BLOB:
					return "blobValue";
				case OptionType.MONEY:
					return "moneyValue";
				case OptionType.FLOAT:
					return "floatValue";
				case OptionType.DATETIME:
					return "dtValue";
				default:
					return "strValue";
			}
		}

		/// <summary>
		/// Returns the appropriate column name from the options table based on the data type.
		/// </summary>
		/// <param name="optionName">The OptionName value</param>
		/// <param name="type">The data type of the option you are querying.</param>
		/// <returns>The column name</returns>
		public static object GetSingleOptionValue(string optionName, OptionType type) {
			SqlConnection myConn = new SqlConnection(GetConnectionString());
			myConn.Open();
			SqlCommand cmd = new SqlCommand("SELECT " + getOptionsValueColumnName(type) + " FROM [OptionsTable] WHERE [OptionName] = @name", myConn);
			cmd.Parameters.AddWithValue("@name", optionName);
			SqlDataAdapter da = new SqlDataAdapter(cmd);
			DataSet ds = new DataSet();
			da.Fill(ds);
			myConn.Close();
			return ds.Tables[0].Rows[0][0];
		}

		/// <summary>
		/// Calculates a trip valid until value using the DefaultTripValidUntilHours entry in the OptionsTable using the current system time.
		/// </summary>
		/// <returns></returns>
		public DateTime calculateTripValidUntil() {
			int hoursToAdd = (int)GetSingleOptionValue("DefaultTripValidUntilHours", OptionType.INT);
			DateTime validUntil = (DateTime.UtcNow.AddHours(hoursToAdd));
			return validUntil;
		}

		/// <summary>
		/// Calculates a trip valid until value using the DefaultTripValidUntilHours entry in the OptionsTable.
		/// </summary>
		/// <param name="tripCreationDate">The date to calculate from</param>
		/// <returns></returns>
		public DateTime calculateTripValidUntil(DateTime tripCreationDate) {
			int hoursToAdd = (int)GetSingleOptionValue("DefaultTripValidUntilHours", OptionType.INT);
			DateTime validUntil = (tripCreationDate.AddHours(hoursToAdd));
			return validUntil;
		}

		/// <summary>
		/// Generates a new entry in the trip table.  Entry is guaranteed to have a unique trip code.
		/// </summary>
		/// <param name="createdby"></param>
		/// <returns></returns>
		public Api.OperationResult CreateTrip(string createdby) {
			bool isunique = false;
			string potentialTripcode = "";

			// Create unique trip code then ensure that it doesn't already exist in the trip table.
			while (!isunique) {
				potentialTripcode = generateTripcode(4);
				isunique = !TripCodeAlreadyExists(potentialTripcode);
			}

			try {
				SqlConnection myConn = new SqlConnection(GetConnectionString());
				myConn.Open();
				SqlCommand cmd = new SqlCommand("SELECT * FROM [TripTable] WHERE 1=2", myConn);
				SqlDataAdapter da = new SqlDataAdapter(cmd);
				SqlCommandBuilder cb = new SqlCommandBuilder(da);
				DataSet ds = new DataSet();
				da.Fill(ds);
				DataRow dr = ds.Tables[0].NewRow();
				dr["tripcode"] = potentialTripcode;
				dr["createdon"] = DateTime.UtcNow;
				dr["validuntil"] = calculateTripValidUntil();
				dr["createdby"] = createdby;
				ds.Tables[0].Rows.Add(dr);
				da.Update(ds);
				myConn.Close();

				return new Api.OperationResult(true, "Trip was created.  Check operationSummary for the tripcode", potentialTripcode);

			} catch (Exception e) {
				return new Api.OperationResult(false, "Failed to create trip - see operationSummary for any messages", e.Message);
			}
		}

		/// <summary>
		/// Generates a new entry in the trip table.  Entry is guaranteed to have a unique trip code.
		/// </summary>
		/// <param name="createdby"></param>
		/// <returns></returns>
		public Api.OperationResult UpsertAvatar(string userid, string base64) {

			SqlConnection myConn = new SqlConnection(GetConnectionString());

			try {
				myConn.Open();
				SqlCommand cmd = new SqlCommand("SELECT * FROM [avatars] WHERE [userid] = @userid", myConn);
				SqlDataAdapter da = new SqlDataAdapter(cmd);
				SqlCommandBuilder cb = new SqlCommandBuilder(da);
				DataSet ds = new DataSet();
				da.Fill(ds);

				if (ds.Tables[0].Rows.Count == 0) {
					DataRow dr = ds.Tables[0].NewRow();
					dr["userid"] = userid;
					dr["base64"] = base64;
					ds.Tables[0].Rows.Add(dr);
				} else {
					ds.Tables[0].Rows[0]["userid"] = userid;
					ds.Tables[0].Rows[0]["base64"] = base64;
				}
				da.Update(ds);
				myConn.Close();

				return new Api.OperationResult(true, "avatar update/create");

			} catch (Exception e) {
				myConn.Close();
				return new Api.OperationResult(false, "avatar update/create", e.Message);
			}
		}

		/// <summary>
		/// Generates a new entry in the trip table.  Entry is guaranteed to have a unique trip code.
		/// </summary>
		/// <param name="createdby"></param>
		/// <returns></returns>
		public Api.OperationResult CreateTrip(string createdby, bool testmode) {
			bool isunique = false;
			string potentialTripcode = "";

			if (testmode) {
				potentialTripcode = "0000";
			} else {
				// Create unique trip code then ensure that it doesn't already exist in the trip table.
				while (!isunique) {
					potentialTripcode = generateTripcode(4);
					isunique = !TripCodeAlreadyExists(potentialTripcode);
				}
			}

			SqlConnection myConn = new SqlConnection(GetConnectionString());

			try {
				myConn.Open();
				SqlCommand cmd = new SqlCommand("SELECT * FROM [TripTable] WHERE 1=2", myConn);
				SqlDataAdapter da = new SqlDataAdapter(cmd);
				SqlCommandBuilder cb = new SqlCommandBuilder(da);
				DataSet ds = new DataSet();
				da.Fill(ds);
				DataRow dr = ds.Tables[0].NewRow();
				dr["tripcode"] = potentialTripcode;
				dr["createdon"] = DateTime.UtcNow;
				dr["createdby"] = createdby;
				ds.Tables[0].Rows.Add(dr);
				da.Update(ds);
				myConn.Close();

				return new Api.OperationResult(true, "Trip was created.  Check operationSummary for the tripcode", potentialTripcode);

			} catch (Exception e) {
				myConn.Close();
				return new Api.OperationResult(false, "Failed to create trip - see operationSummary for any messages", e.Message);
			}
		}

		public Api.OperationResult RemoveFcmToken(string fcmToken) {
			Api.OperationResult operationResult = new Api.OperationResult();

			SqlConnection myConn = new SqlConnection(GetConnectionString());
			myConn.Open();
			SqlCommand cmd = new SqlCommand("SELECT * FROM [fcmtokens] where [fcmtoken] = @fcmtoken", myConn);
			cmd.Parameters.AddWithValue("@fcmtoken", fcmToken);
			SqlDataAdapter da = new SqlDataAdapter(cmd);
			SqlCommandBuilder cb = new SqlCommandBuilder(da);
			DataSet ds = new DataSet();
			da.Fill(ds);

			try {
				ds.Tables[0].Rows[0].Delete();
				da.Update(ds);
				myConn.Close();
				operationResult.wasSuccessful = true;
			} catch (Exception e) {
				operationResult.wasSuccessful = false;
				operationResult.result = e.Message;
			}

			return operationResult;
		}

		/// <summary>
		/// Adds or updates a record in the tripentries table.  Much like the UpdateTrip method this 
		/// does the same but omits the location parameters - this would be used by new users joining a 
		/// trip and before they likely have any reliable location data.
		/// </summary>
		/// <param name="tripcode"></param>
		/// <param name="userid"></param>
		/// <returns>An OperationResult object that will really only contain the userid of the user that prompted the update (or an error if applicable).</returns>
		public Api.OperationResult JoinTrip(string tripcode, string userid) {
			SqlConnection myConn = new SqlConnection(GetConnectionString());
			try {
				myConn.Open();
				SqlCommand cmd = new SqlCommand("SELECT * FROM [TripEntries] where [userid] = @userid", myConn);
				cmd.Parameters.AddWithValue("@userid", userid);
				SqlDataAdapter da = new SqlDataAdapter(cmd);
				SqlCommandBuilder cb = new SqlCommandBuilder(da);
				DataSet ds = new DataSet();
				da.Fill(ds);
				if (ds.Tables[0].Rows.Count == 0) {
					DataRow dr = ds.Tables[0].NewRow();
					dr["tripcode"] = tripcode;
					dr["userid"] = userid;
					dr["modifiedon"] = DateTime.UtcNow;
					ds.Tables[0].Rows.Add(dr);
				} else {
					ds.Tables[0].Rows[0]["tripcode"] = tripcode;
					ds.Tables[0].Rows[0]["modifiedon"] = DateTime.UtcNow;
				}
				int id = da.Update(ds);

				Api.TripReport locUpdates = GetTripUpdateReport(tripcode, userid);

				myConn.Close();
				return new Api.OperationResult(true, "Updating existing trip", locUpdates);
			} catch (Exception e) {
				myConn.Close();
				return new Api.OperationResult(false, "Updating existing trip", "Failed to join trip: " + e.Message);
			}
		}

		/// <summary>
		/// Removes all entries for the specified user from the TripEntries table.
		/// </summary>
		/// <param name="tripcode"></param>
		/// <param name="userid"></param>
		/// <returns>An OperationResult object containing a MemberLocUpdates object that should contain zero references to the specified user.</returns>
		public Api.OperationResult RemoveAllUserEntriesForTrip(string userid, string tripcode) {
			SqlConnection myConn = new SqlConnection(GetConnectionString());
			try {
				myConn.Open();
				SqlCommand cmd = new SqlCommand("SELECT * FROM [TripEntries] where [userid] = @userid AND [tripcode] = @tripcode", myConn);
				cmd.Parameters.AddWithValue("@userid", userid);
				cmd.Parameters.AddWithValue("@tripcode", tripcode);
				SqlDataAdapter da = new SqlDataAdapter(cmd);
				SqlCommandBuilder cb = new SqlCommandBuilder(da);
				DataSet ds = new DataSet();
				da.Fill(ds);
				if (ds.Tables[0].Rows.Count > 0) {
					for (int i = 0; i < ds.Tables[0].Rows.Count; i++) {
						ds.Tables[0].Rows[i].Delete();
					}
				}
				int id = da.Update(ds);

				Api.TripReport locUpdates = GetTripUpdateReport(tripcode, userid);

				myConn.Close();
				return new Api.OperationResult(true, "Removed user from existing trip", locUpdates);
			} catch (Exception e) {
				myConn.Close();
				return new Api.OperationResult(false, "Removed user from existing trip", "Failed to leave trip: " + e.Message);
			}
		}

		/// <summary>
		/// Removes all entries for the specified user from the TripEntries table.
		/// </summary>
		/// <param name="tripcode"></param>
		/// <param name="userid"></param>
		/// <returns>An OperationResult object containing a MemberLocUpdates object that should contain zero references to the specified user.</returns>
		public Api.OperationResult RemoveAllUserEntries(string userid) {
			SqlConnection myConn = new SqlConnection(GetConnectionString());
			try {
				myConn.Open();
				SqlCommand cmd = new SqlCommand("SELECT * FROM [TripEntries] where [userid] = @userid", myConn);
				cmd.Parameters.AddWithValue("@userid", userid);
				SqlDataAdapter da = new SqlDataAdapter(cmd);
				SqlCommandBuilder cb = new SqlCommandBuilder(da);
				DataSet ds = new DataSet();
				da.Fill(ds);
				if (ds.Tables[0].Rows.Count > 0) {
					for (int i = 0; i < ds.Tables[0].Rows.Count; i++) {
						ds.Tables[0].Rows[i].Delete();
					}
				}
				int id = da.Update(ds);
				
				myConn.Close();
				return new Api.OperationResult(true, "Removed user from existing trip", null);
			} catch (Exception e) {
				myConn.Close();
				return new Api.OperationResult(false, "Removed user from existing trip", "Failed to leave trip: " + e.Message);
			}
		}

		/// <summary>
		/// Returns a list of GoogleUsers associated with a tripcode.
		/// </summary>
		/// <param name="tripcode"></param>
		/// <param name="userid"></param>
		/// <returns>A List of GoogleUser objects</returns>
		public List<User> GetTripMembers(string tripcode) {
			List<User> users = new List<User>();
			SqlConnection myConn = new SqlConnection(GetConnectionString());
			try {
				myConn.Open();
				SqlCommand cmd = new SqlCommand("SELECT a1.userid, a2.email, a2.displayname, a2.photourl FROM [dbo].[TripEntries] as a1 join [dbo].[Users] as a2 on a1.userid = a2.userid where a1.[tripcode] = @tripcode", myConn);
				cmd.Parameters.AddWithValue("@tripcode", tripcode);
				SqlDataAdapter da = new SqlDataAdapter(cmd);
				SqlCommandBuilder cb = new SqlCommandBuilder(da);
				DataSet ds = new DataSet();
				da.Fill(ds);
				foreach (DataRow row in ds.Tables[0].Rows) {
					User user = new User();
					user.id = row["userid"].ToString();
					user.email = row["email"].ToString();
					user.fullname = row["displayname"].ToString();
					if (row["photorul"] != null) {
						user.photourl = row["photourl"].ToString();
					}
					users.Add(user);
				}
				myConn.Close();
				return users;
			} catch (Exception e) {
				myConn.Close();
				return null;
			}
		}

		/// <summary>
		/// Returns a GoogleUser by their google id.
		/// </summary>
		/// <param name="userid"></param>
		/// <returns>A List of GoogleUser objects</returns>
		public User GetGoogleUser(string userid) {
			List<User> users = new List<User>();
			SqlConnection myConn = new SqlConnection(GetConnectionString());
			try {
				myConn.Open();
				SqlCommand cmd = new SqlCommand("SELECT * FROM [dbo].[Users] WHERE [userid] = @userid", myConn);
				cmd.Parameters.AddWithValue("@userid", userid);
				SqlDataAdapter da = new SqlDataAdapter(cmd);
				SqlCommandBuilder cb = new SqlCommandBuilder(da);
				DataSet ds = new DataSet();
				da.Fill(ds);
				if (ds.Tables[0].Rows.Count > 0) {
					User user = new User();
					user.id = ds.Tables[0].Rows[0]["userid"].ToString();
					user.email = ds.Tables[0].Rows[0]["email"].ToString();
					user.fullname = ds.Tables[0].Rows[0]["displayname"].ToString();
					if (ds.Tables[0].Rows[0]["photourl"] != null) {
						user.photourl = ds.Tables[0].Rows[0]["photourl"].ToString();
					}
					return user;
				}
				return null;
			} catch (Exception e) {
				return null;
			} finally {
				myConn.Close();
			}
		}

		/// <summary>
		/// Does nothing more than check if there are rows in the tripentries table containing the specified tripcode.
		/// </summary>
		/// <param name="tripcode"></param>
		/// <returns></returns>
		public bool TripExists(string tripcode) {
			SqlConnection myConn = new SqlConnection(GetConnectionString());
			try {
				myConn.Open();
				SqlCommand cmd = new SqlCommand("SELECT * FROM [dbo].[TripEntries] WHERE [tripcode] = @tripcode", myConn);
				cmd.Parameters.AddWithValue("@tripcode", tripcode);
				SqlDataAdapter da = new SqlDataAdapter(cmd);
				SqlCommandBuilder cb = new SqlCommandBuilder(da);
				DataSet ds = new DataSet();
				da.Fill(ds);
				return ds.Tables[0].Rows.Count > 0;
			} catch (Exception e) {
				return false;
			}
			finally {
				myConn.Close();
			}
		}

		/// <summary>
		/// Adds or updates a record in the tripentries table.
		/// </summary>
		/// <param name="tripcode"></param>
		/// <param name="userid"></param>
		/// <param name="latitude"></param>
		/// <param name="longitude"></param>
		/// <param name="accuracy_meters"></param>
		/// <returns>An OperationResult object that will really only contain the userid of the user that prompted the update (or an error if applicable).</returns>
		public Api.OperationResult UpdateTrip(string tripcode, string userid, int location_type, string locationJson = null) {
			SqlConnection myConn = new SqlConnection(GetConnectionString());
			try {
				myConn.Open();
				SqlCommand cmd = new SqlCommand("SELECT * FROM [TripEntries] where [userid] = @userid", myConn);
				cmd.Parameters.AddWithValue("@userid", userid);
				SqlDataAdapter da = new SqlDataAdapter(cmd);
				SqlCommandBuilder cb = new SqlCommandBuilder(da);
				DataSet ds = new DataSet();
				da.Fill(ds);
				if (ds.Tables[0].Rows.Count == 0) {
					DataRow dr = ds.Tables[0].NewRow();
					dr["tripcode"] = tripcode;
					dr["userid"] = userid;
					dr["modifiedon"] = DateTime.UtcNow;
					dr["locationtype"] = location_type;
					dr["location"] = locationJson;
					ds.Tables[0].Rows.Add(dr);
				} else {
					ds.Tables[0].Rows[0]["tripcode"] = tripcode;
					ds.Tables[0].Rows[0]["modifiedon"] = DateTime.UtcNow;
					ds.Tables[0].Rows[0]["locationtype"] = location_type;
					ds.Tables[0].Rows[0]["location"] = locationJson;
				}
				da.Update(ds);

				Api.TripReport locUpdates = GetTripUpdateReport(tripcode, userid);

				myConn.Close();
				return new Api.OperationResult(true, "Updating existing trip", locUpdates);
			} catch (Exception e) {
				myConn.Close();
				return new Api.OperationResult(false, "Updating existing trip", "Failed to update trip: " + e.Message);
			}
		}

		/// <summary>
		/// Updates an existing or creates a new entry the fcmtokens table.
		/// </summary>
		/// <param name="userid">The userid to associate with the fcm token</param>
		/// <param name="fcmtoken">The user's unique, device-specific fcm token as generated by the device.</param>
		/// <returns>An OperationResult object with its constituent properties.</returns>
		public Api.OperationResult UpsertFcmToken(string userid, string fcmtoken) {
			SqlConnection myConn = new SqlConnection(GetConnectionString());
			try {
				myConn.Open();
				SqlCommand cmd = new SqlCommand("SELECT * FROM [fcmtokens] where [userid] = @userid and [fcmtoken] = @fcmtoken", myConn);
				cmd.Parameters.AddWithValue("@userid", userid);
				cmd.Parameters.AddWithValue("@fcmtoken", fcmtoken);
				SqlDataAdapter da = new SqlDataAdapter(cmd);
				SqlCommandBuilder cb = new SqlCommandBuilder(da);
				DataSet ds = new DataSet();
				da.Fill(ds);
				if (ds.Tables[0].Rows.Count == 0) {
					DataRow dr = ds.Tables[0].NewRow();
					dr["userid"] = userid;
					dr["fcmtoken"] = fcmtoken;
					dr["createdon"] = DateTime.UtcNow;
					dr["modifiedon"] = DateTime.UtcNow;
					ds.Tables[0].Rows.Add(dr);
				} else {
					ds.Tables[0].Rows[0]["userid"] = userid;
					ds.Tables[0].Rows[0]["fcmtoken"] = fcmtoken;
					ds.Tables[0].Rows[0]["createdon"] = DateTime.UtcNow;
					ds.Tables[0].Rows[0]["modifiedon"] = DateTime.UtcNow;
				}
				da.Update(ds);
				myConn.Close();
			} catch (Exception e) {
				myConn.Close();
				return new Api.OperationResult(false, "Upserting FCM token", e.Message);
			}
			Api.FcmUpsertResult fcmUpsertResult = new Api.FcmUpsertResult(true, userid, fcmtoken);
			return new Api.OperationResult(true, "Upserting FCM token", fcmUpsertResult.ToJson());
		}

		public List<string> GetTripMembersFcmToken(string tripcode) {
			List<string> tokens = new List<string>();
			SqlConnection myConn = new SqlConnection(GetConnectionString());
			myConn.Open();
			SqlCommand cmd = new SqlCommand("select fcmtoken from dbo.TripEntries as a1 left join dbo.fcmtokens as a2 on a1.userid = " +
				"a2.userid where [tripcode] = @tripcode", myConn);
			cmd.Parameters.AddWithValue("@tripcode", tripcode);
			SqlDataAdapter da = new SqlDataAdapter(cmd);
			SqlCommandBuilder cb = new SqlCommandBuilder(da);
			DataSet ds = new DataSet();
			da.Fill(ds);
			foreach (DataRow row in ds.Tables[0].Rows) {
				tokens.Add(row["fcmtoken"].ToString());
			}
			myConn.Close();
			return tokens;
		}

		/// <summary>
		/// Reads all fcmtoken entries for the specified user's google id.  Returns null on error or an empty list (but not null) if none are found.
		/// </summary>
		/// <param name="userid">The Google userid of the user to search</param>
		/// <returns>A list of strings each containing an FCM token string and nothing else.</returns>
		public Dictionary<string, string> GetAllFcmsByUserid(string userid) {

			Dictionary<string, string> tokens = new Dictionary<string, string>();
			SqlConnection myConn = new SqlConnection(GetConnectionString());

			try {
				myConn.Open();
				SqlCommand cmd = new SqlCommand("SELECT a2.[displayname], a1.[createdon], a1.[fcmtoken] FROM [fcmtokens] as a1 join [Users] as a2 on a1.[userid] = a2.[userid] where a1.[userid] = @userid", myConn);
				cmd.Parameters.AddWithValue("@userid", userid);
				SqlDataAdapter da = new SqlDataAdapter(cmd);
				SqlCommandBuilder cb = new SqlCommandBuilder(da);
				DataSet ds = new DataSet();
				da.Fill(ds);
				foreach (DataRow row in ds.Tables[0].Rows) {
					string displayname = row["displayname"].ToString();
					string token = row["fcmtoken"].ToString();
					tokens.Add(token, displayname);
				}
				myConn.Close();
				return tokens;
			} catch (Exception e) {
				return null;
			} finally {
				myConn.Close();
			}
		}

		/// <summary>
		/// Creates a new row in the Messages table.  No validation is performed nor duplicate detection etc..
		/// </summary>
		/// <param name="message">A populated UserMessage object</param>
		/// <returns>True if the record was created in the Messages table.</returns>
		public bool AppendUserMessage(UserMessage message) {
			SqlConnection myConn = new SqlConnection(GetConnectionString());
			try {
				myConn.Open();
				SqlCommand cmd = new SqlCommand("SELECT * FROM [Messages] where 1=2", myConn);
				cmd.Parameters.AddWithValue("@tripcode", message.tripcode);
				cmd.Parameters.AddWithValue("@json", message.toJson());
				SqlDataAdapter da = new SqlDataAdapter(cmd);
				SqlCommandBuilder cb = new SqlCommandBuilder(da);
				DataSet ds = new DataSet();
				da.Fill(ds);
				if (ds.Tables[0].Rows.Count == 0) {
					DataRow dr = ds.Tables[0].NewRow();
					dr["tripcode"] = message.tripcode;
					dr["json"] = message.toJson();
					ds.Tables[0].Rows.Add(dr);
				}
				da.Update(ds);
				myConn.Close();
			} catch (Exception e) {
				myConn.Close();
				return false;
			}
			return true;
		}

		/// <summary>
		/// Returns all UserMessage objects stored in teh database for the specified trip.
		/// </summary>
		/// <param name="tripcode">The tripcode the messages are associated with.</param>
		/// <param name="limit">The record limit to return (cannot exceed 100 or will be set to 100).</param>
		/// <param name="orderargument">Either "ASC" or "DESC" will default to "DESC" if nothing or an invalid argument is supplied.</param>
		/// <returns>A list of UserMessage objects.</returns>
		public List<UserMessage> GetAllUserMessages(string tripcode, int limit, string orderargument) {

			// Quick validations - limit to 100 records and ensure a sorting argument is valid
			if (limit > 100) { limit = 100; }
			if ((!orderargument.ToLower().Equals("asc")) || (!orderargument.ToLower().Equals("desc"))) {
				orderargument = "desc";
			}

			SqlConnection myConn = new SqlConnection(GetConnectionString());
			List<UserMessage> messages = new List<UserMessage>();

			try {
				myConn.Open();
				SqlCommand cmd = new SqlCommand("SELECT TOP " + limit + " * from [Messages] where [tripcode] = @tripcode ORDER BY id " + orderargument, myConn);
				cmd.Parameters.AddWithValue("@tripcode", tripcode);
				SqlDataAdapter da = new SqlDataAdapter(cmd);
				SqlCommandBuilder cb = new SqlCommandBuilder(da);
				DataSet ds = new DataSet();
				da.Fill(ds);
				foreach (DataRow row in ds.Tables[0].Rows) {
					string json = row["json"].ToString();
					UserMessage message = Newtonsoft.Json.JsonConvert.DeserializeObject<UserMessage>(json);
					messages.Add(message);
				}
				return messages;
			} catch (Exception e) {
				return null;
			}
			finally {
				myConn.Close();
			}
		}

		/// <summary>
		/// Reads all fcmtoken entries for the specified user's email address.  Returns null on error or an empty list (but not null) if none are found.
		/// </summary>
		/// <param name="email">The Google userid of the user to search</param>
		/// <returns>A list of strings each containing an FCM token string and nothing else.</returns>
		public List<string> GetAllFcmsByUserEmail(string email) {

			SqlConnection myConn = new SqlConnection(GetConnectionString());
			List<string> tokens = new List<string>();

			try {
				myConn.Open();
				SqlCommand cmd = new SqlCommand("SELECT a2.[displayname], a1.[createdon], a1.[fcmtoken] FROM [fcmtokens] as a1 join [Users] as a2 on a1.[userid] = a2.[userid] where a2.[email] = @email", myConn);
				cmd.Parameters.AddWithValue("@email", email);
				SqlDataAdapter da = new SqlDataAdapter(cmd);
				SqlCommandBuilder cb = new SqlCommandBuilder(da);
				DataSet ds = new DataSet();
				da.Fill(ds);
				foreach (DataRow row in ds.Tables[0].Rows) {
					string token = row["fcmtoken"].ToString();
					tokens.Add(token);
				}
				return tokens;
			} catch (Exception e) {
				return null;
			} finally {
				myConn.Close();
			}
		}

		/// <summary>
		/// Reads all fcmtoken entries for the specified user's email address.  Returns null on error or an empty list (but not null) if none are found.
		/// </summary>
		/// <param name="email">The Google userid of the user to search</param>
		/// <returns>A list of strings each containing an FCM token string and nothing else.</returns>
		public List<string> GetUseridByUserEmail(string email) {

			List<string> userids = new List<string>();
			SqlConnection myConn = new SqlConnection(GetConnectionString());

			try {
				myConn.Open();
				SqlCommand cmd = new SqlCommand("SELECT a1.[userid] from [Users] where a1.[email] = @email", myConn);
				cmd.Parameters.AddWithValue("@email", email);
				SqlDataAdapter da = new SqlDataAdapter(cmd);
				SqlCommandBuilder cb = new SqlCommandBuilder(da);
				DataSet ds = new DataSet();
				da.Fill(ds);
				foreach (DataRow row in ds.Tables[0].Rows) {
					string userid = row["userid"].ToString();
					userids.Add(userid);
				}
				return userids;
			} catch (Exception e) {
				return null;
			} finally {
				myConn.Close();
			}
		}

		/// <summary>
		/// Reads all fcmtoken entries for the specified user's email address.  Returns null on error or an empty list (but not null) if none are found.
		/// </summary>
		/// <param name="email">The Google userid of the user to search</param>
		/// <returns>A list of strings each containing an FCM token string and nothing else.</returns>
		public string GetEmailByUserid(string userid) {

			List<string> userids = new List<string>();
			SqlConnection myConn = new SqlConnection(GetConnectionString());

			try {
				myConn.Open();
				SqlCommand cmd = new SqlCommand("SELECT [email] from [Users] where [userid] = @userid", myConn);
				cmd.Parameters.AddWithValue("@userid", userid);
				SqlDataAdapter da = new SqlDataAdapter(cmd);
				SqlCommandBuilder cb = new SqlCommandBuilder(da);
				DataSet ds = new DataSet();
				da.Fill(ds);
				string email = ds.Tables[0].Rows[0]["email"].ToString();
				return email;
			} catch (Exception e) {
				return null;
			}
			finally {
				myConn.Close();
			}
		}

		public Api.TripReport GetTripUpdateReport(string tripcode, string userid) {
			Api.OperationResult operationResult = new Api.OperationResult();

			SqlConnection myConn = new SqlConnection(GetConnectionString());
			myConn.Open();
			SqlCommand cmd = new SqlCommand("" +
				"SELECT " +
					"a1.[id]," +
					"a1.[tripcode], " +
					"a1.[modifiedon], " +
					"a1.[userid], " +
					"a2.[displayname], " +
					"a2.[email], " +
					"a1.[locationtype], " +
					"a1.[location], " +
					"a2.[photourl] " +
				"FROM [dbo].[TripEntries] as a1 join [dbo].[Users] as a2 on a1.userid = a2.userid " +
				"WHERE [tripcode] = @tripcode"
				, myConn);
			cmd.Parameters.AddWithValue("@userid", userid);
			cmd.Parameters.AddWithValue("@tripcode", tripcode);
			SqlDataAdapter da = new SqlDataAdapter(cmd);
			SqlCommandBuilder cb = new SqlCommandBuilder(da);
			DataSet ds = new DataSet();
			da.Fill(ds);

			Api.TripReport updates = new Api.TripReport();
			updates.tripcode = tripcode; // The group's code
			updates.initiatedby = userid; // The user initiating this report
			updates.initiatedon = DateTime.UtcNow; // Now, bitch!

			// Read each user's location from the database coresponding to this trip, construct 
			// MemberLocUpdate objects and add them to an array.  This will ultimately be serialized 
			// and sent to the user as a paylod in an FCM message.
			foreach (DataRow row in ds.Tables[0].Rows) {
				Api.MemberLocUpdate memberLoc = new Api.MemberLocUpdate();
				memberLoc.tripcode = row["tripcode"].ToString();
				DateTime mOn = (DateTime)row["modifiedon"];
				memberLoc.modifiedOn = mOn.ToUniversalTime();
				memberLoc.createdOn = mOn.ToUniversalTime();
				memberLoc.userid = row["userid"].ToString();
				memberLoc.displayName = row["displayname"].ToString();
				memberLoc.email = row["email"].ToString();
				memberLoc.location = row["location"].ToString();
				memberLoc.locationtype = row["locationtype"].ToString();
				if (row["photourl"] != null) {
					memberLoc.photoUrl = row["photourl"].ToString();
				}
				updates.list.Add(memberLoc);
			}

			myConn.Close();
			return updates;

		}

		/// <summary>
		/// Creates or updates a user in the base users table - does not update other tables even if 
		/// they are directly related to this base entry (e.g. The FCM token table is not updated etc.).  
		/// </summary>
		/// <param name="userid">The user's unique Google id</param>
		/// <param name="email">The user's email address</param>
		/// <param name="photourl">NULLABLE The url to the user's public Google account profile picture.</param>
		/// <param name="displayname">The user's display name</param>
		/// <returns>An OperationResult object with its constituent properties.</returns>
		public Api.OperationResult UpsertUser(string userid, string email, string photourl, string displayname) {

			Api.OperationResult operationResult = new Api.OperationResult();

			SqlConnection myConn = new SqlConnection(GetConnectionString());
			myConn.Open();
			SqlCommand cmd = new SqlCommand("SELECT * FROM [Users] where [userid] = @userid", myConn);
			cmd.Parameters.AddWithValue("@userid", userid);
			SqlDataAdapter da = new SqlDataAdapter(cmd);
			SqlCommandBuilder cb = new SqlCommandBuilder(da);
			DataSet ds = new DataSet();
			da.Fill(ds);
			if (ds.Tables[0].Rows.Count == 0) {
				DataRow dr = ds.Tables[0].NewRow();
				dr["userid"] = userid;
				dr["email"] = email;
				dr["photourl"] = photourl;
				dr["displayname"] = displayname;
				dr["createdon"] = DateTime.UtcNow;
				dr["modifiedon"] = DateTime.UtcNow;
				ds.Tables[0].Rows.Add(dr);
			} else {
				ds.Tables[0].Rows[0]["userid"] = userid;
				ds.Tables[0].Rows[0]["email"] = email;
				ds.Tables[0].Rows[0]["photourl"] = photourl;
				ds.Tables[0].Rows[0]["displayname"] = displayname;
				ds.Tables[0].Rows[0]["modifiedon"] = DateTime.UtcNow;
			}

			try {
				da.Update(ds);
				myConn.Close();
				return new Api.OperationResult(true, "Upserting user (" + userid + ")", "");
			} catch (Exception e0) {
				myConn.Close();
				return new Api.OperationResult(false, "Upserting user (" + userid + ")", e0.Message);
			}
		}

		public static void WriteLogLine(string value) {
			SqlConnection myConn = new SqlConnection(GetConnectionString());
			try {
				
				myConn.Open();
				SqlCommand cmd = new SqlCommand("SELECT * FROM [DebugLogging] WHERE 1=2", myConn);
				SqlDataAdapter da = new SqlDataAdapter(cmd);
				SqlCommandBuilder cb = new SqlCommandBuilder(da);
				DataSet ds = new DataSet();
				da.Fill(ds);
				DataRow dr = ds.Tables[0].NewRow();
				dr["strValue"] = value;
				dr["createdon"] = DateTime.UtcNow;
				ds.Tables[0].Rows.Add(dr);
				da.Update(ds);
				myConn.Close();
			} catch (Exception fuckyou) {
				myConn.Close();
			}
		}

	}
}