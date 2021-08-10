using RestApi.AppCode;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using WhereYouAt.AppCode;
using WhereYouAtRestApi.AppCode;

namespace WhereYouAt.AppCode {
	public class MyDb {

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

		public DataRow GetTrip(string tripcode) {
			SqlConnection myConn = new SqlConnection(GetConnectionString());
			myConn.Open();
			SqlCommand cmd = new SqlCommand("SELECT * FROM [TripTable] where [tripcode] = @tripcode AND [validuntil] > @nowdatetime", myConn);
			cmd.Parameters.AddWithValue("@tripcode", tripcode);
			cmd.Parameters.AddWithValue("@nowdatetime", DateTime.Now);
			SqlDataAdapter da = new SqlDataAdapter(cmd);
			SqlCommandBuilder cb = new SqlCommandBuilder(da);

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
			cmd.Parameters.AddWithValue("@nowdatetime", DateTime.Now);
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
			cmd.Parameters.AddWithValue("@nowdatetime", DateTime.Now);
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
			DateTime validUntil = (DateTime.Now.AddHours(hoursToAdd));
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
		public OperationResult CreateTrip(string createdby) {
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
				dr["createdon"] = DateTime.Now;
				dr["validuntil"] = calculateTripValidUntil();
				dr["createdby"] = createdby;
				ds.Tables[0].Rows.Add(dr);
				da.Update(ds);
				myConn.Close();

				return new OperationResult(true, "Trip was created.  Check operationSummary for the tripcode", potentialTripcode);

			} catch (Exception e) {
				return new OperationResult(false, "Failed to create trip - see operationSummary for any messages", e.Message);
			}
		}

		/// <summary>
		/// Generates a new entry in the trip table.  Entry is guaranteed to have a unique trip code.
		/// </summary>
		/// <param name="createdby"></param>
		/// <returns></returns>
		public OperationResult UpsertAvatar(string userid, string base64) {

			try {
				SqlConnection myConn = new SqlConnection(GetConnectionString());
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

				return new OperationResult(true, "avatar update/create");

			} catch (Exception e) {
				return new OperationResult(false, "avatar update/create", e.Message);
			}
		}

		/// <summary>
		/// Generates a new entry in the trip table.  Entry is guaranteed to have a unique trip code.
		/// </summary>
		/// <param name="createdby"></param>
		/// <returns></returns>
		public OperationResult CreateTrip(string createdby, bool testmode) {
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
				dr["createdon"] = DateTime.Now;
				dr["validuntil"] = calculateTripValidUntil();
				dr["createdby"] = createdby;
				ds.Tables[0].Rows.Add(dr);
				da.Update(ds);
				myConn.Close();

				return new OperationResult(true, "Trip was created.  Check operationSummary for the tripcode", potentialTripcode);

			} catch (Exception e) {
				return new OperationResult(false, "Failed to create trip - see operationSummary for any messages", e.Message);
			}
		}

		/// <summary>
		/// Generates a new entry in the trip table.  Entry is guaranteed to have a unique trip code.
		/// </summary>
		/// <param name="createdby"></param>
		/// <returns></returns>
		public OperationResult GetExistingTrip(string tripcode) {

			try {
				SqlConnection myConn = new SqlConnection(GetConnectionString());
				myConn.Open();
				SqlCommand cmd = new SqlCommand("SELECT * FROM [TripTable] WHERE [tripcode]=@tripcode", myConn);
				cmd.Parameters.AddWithValue("@tripcode", tripcode);
				SqlDataAdapter da = new SqlDataAdapter(cmd);
				SqlCommandBuilder cb = new SqlCommandBuilder(da);
				DataSet ds = new DataSet();
				da.Fill(ds);
				Trip trip = new Trip(tripcode);
				// TripMember tripMember = new TripMember();
				// trip.createdby = ds.Tables[0].Rows[0]["createdby"].ToString();
				/*dr["tripcode"] = potentialTripcode;
				dr["createdon"] = DateTime.Now;
				dr["validuntil"] = calculateTripValidUntil();
				dr["createdby"] = createdby;
				ds.Tables[0].Rows.Add(dr);
				da.Update(ds);*/
				myConn.Close();

				return new OperationResult(true, "Trip was created.  Check operationSummary for the tripcode", tripcode);

			} catch (Exception e) {
				return new OperationResult(false, "Failed to create trip - see operationSummary for any messages", e.Message);
			}
		}

		public OperationResult RemoveFcmToken(string fcmToken) {
			OperationResult operationResult = new OperationResult();

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

		public OperationResult UpdateTrip(string tripcode, string userid, double latitude, double longitude, double accuracy_meters) {
			try {
				SqlConnection myConn = new SqlConnection(GetConnectionString());
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
					dr["lat"] = latitude;
					dr["lon"] = longitude;
					dr["accuracy_meters"] = accuracy_meters;
					dr["userid"] = userid;
					dr["modifiedon"] = DateTime.Now;
					ds.Tables[0].Rows.Add(dr);
				} else {
					ds.Tables[0].Rows[0]["lat"] = latitude;
					ds.Tables[0].Rows[0]["lon"] = longitude;
					ds.Tables[0].Rows[0]["tripcode"] = tripcode;
					ds.Tables[0].Rows[0]["accuracy_meters"] = accuracy_meters; 
					ds.Tables[0].Rows[0]["modifiedon"] = DateTime.Now;
				}
				da.Update(ds);
				myConn.Close();
				return new OperationResult(true, "Updating existing trip", "Trip was updated by user: " + userid);
			} catch (Exception e) {
				return new OperationResult(false, "Updating existing trip", "Failed to update trip: " + e.Message);
			}
		}

		/// <summary>
		/// Updates an existing or creates a new entry the fcmtokens table.
		/// </summary>
		/// <param name="userid">The userid to associate with the fcm token</param>
		/// <param name="fcmtoken">The user's unique, device-specific fcm token as generated by the device.</param>
		/// <returns>An OperationResult object with its constituent properties.</returns>
		public OperationResult UpsertFcmToken(string userid, string fcmtoken) {
			try {
				SqlConnection myConn = new SqlConnection(GetConnectionString());
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
					dr["createdon"] = DateTime.Now;
					dr["modifiedon"] = DateTime.Now;
					ds.Tables[0].Rows.Add(dr);
				} else {
					ds.Tables[0].Rows[0]["userid"] = userid;
					ds.Tables[0].Rows[0]["fcmtoken"] = fcmtoken;
					ds.Tables[0].Rows[0]["createdon"] = DateTime.Now;
					ds.Tables[0].Rows[0]["modifiedon"] = DateTime.Now;
				}
				da.Update(ds);
				myConn.Close();
			} catch (Exception e) {
				return new OperationResult(false, "Upserting FCM token", e.Message);
			}
			FcmUpsertResult fcmUpsertResult = new FcmUpsertResult(true, userid, fcmtoken);
			return new OperationResult(true, "Upserting FCM token", fcmUpsertResult.ToJson());
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
				SqlCommand cmd = new SqlCommand("SELECT a2.[displayname], a1.[createdon], a1.[fcmtoken] FROM [fcmtokens] as a1 join [Users] as a2 on a1.[userid] = [a2.userid] where a1.[userid] = @userid", myConn);
				cmd.Parameters.AddWithValue("@userid", userid);
				SqlDataAdapter da = new SqlDataAdapter(cmd);
				SqlCommandBuilder cb = new SqlCommandBuilder(da);
				DataSet ds = new DataSet();
				da.Fill(ds);
				foreach (DataRow row in ds.Tables[0].Rows) {
					string displayname = row["displayname"].ToString();
					string token = row["fcmtoken"].ToString();
					tokens.Add(displayname, token);
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

		public MemberLocUpdates GetTripUpdateReport(string tripcode, string userid) {
			OperationResult operationResult = new OperationResult();

			SqlConnection myConn = new SqlConnection(GetConnectionString());
			myConn.Open();
			SqlCommand cmd = new SqlCommand("" +
				"SELECT " +
					"a1.[tripcode], " +
					"a1.[lat], " +
					"a1.[lon], " +
					"a1.[modifiedon], " +
					"a1.[accuracy_meters], " +
					"a1.[userid], " +
					"a2.[displayname], " +
					"a2.[email], " +
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

			MemberLocUpdates updates = new MemberLocUpdates();
			updates.tripcode = tripcode;

			foreach (DataRow row in ds.Tables[0].Rows) {
				MemberLocUpdate memberLoc = new MemberLocUpdate();
				memberLoc.tripcode = row["tripcode"].ToString();
				memberLoc.lat = Convert.ToDouble(row["lat"]);
				memberLoc.lon = Convert.ToDouble(row["lon"]);
				memberLoc.accuracy_meters = Convert.ToDouble(row["accuracy_meters"]);
				DateTime mOn = (DateTime)row["modifiedon"];
				memberLoc.modifiedOn = mOn.ToOADate();
				memberLoc.createdOn = mOn.ToOADate();
				memberLoc.userid = row["userid"].ToString();
				memberLoc.displayName = row["displayname"].ToString();
				memberLoc.email = row["email"].ToString();
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
		public OperationResult UpsertUser(string userid, string email, string photourl, string displayname) {

			OperationResult operationResult = new OperationResult();

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
				dr["createdon"] = DateTime.Now;
				dr["modifiedon"] = DateTime.Now;
				ds.Tables[0].Rows.Add(dr);
			} else {
				ds.Tables[0].Rows[0]["userid"] = userid;
				ds.Tables[0].Rows[0]["email"] = email;
				ds.Tables[0].Rows[0]["photourl"] = photourl;
				ds.Tables[0].Rows[0]["displayname"] = displayname;
				ds.Tables[0].Rows[0]["modifiedon"] = DateTime.Now;
			}

			try {
				da.Update(ds);
				myConn.Close();
				return new OperationResult(true, "Upserting user (" + userid + ")", "");
			} catch (Exception e0) {
				myConn.Close();
				return new OperationResult(false, "Upserting user (" + userid + ")", e0.Message);
			}
		}

		public static void WriteLogLine(string value) {
			SqlConnection myConn = new SqlConnection(GetConnectionString());
			myConn.Open();
			SqlCommand cmd = new SqlCommand("SELECT * FROM [DebugLogging] WHERE 1=2", myConn);
			SqlDataAdapter da = new SqlDataAdapter(cmd);
			SqlCommandBuilder cb = new SqlCommandBuilder(da);
			DataSet ds = new DataSet();
			da.Fill(ds);
			DataRow dr = ds.Tables[0].NewRow();
			dr["strValue"] = value;
			ds.Tables[0].Rows.Add(dr);
			da.Update(ds);
			myConn.Close();
		}

	}
}