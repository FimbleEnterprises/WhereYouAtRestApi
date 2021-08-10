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

namespace InsideEdge.Backend {
	public class MyDb {

		public const string DB_TABLE_AVAILABLE_PRODUCTS = "INSIDE_EDGE_AVAILABLE_PRODUCTS";
		public const string DB_TABLE_COLORS = "INSIDE_EDGE_COLORS";
		public const string DB_TABLE_CUSTOMERS = "INSIDE_EDGE_CUSTOMERS";
		public const string DB_TABLE_MANUFACTURERS = "INSIDE_EDGE_MANUFACTURERS";
		public const string DB_TABLE_QUOTES = "INSIDE_EDGE_QUOTES";
		public const string DB_TABLE_SIZES = "INSIDE_EDGE_SIZES";
		public const string DB_TABLE_STYLES = "INSIDE_EDGE_STYLES";
		public const string DB_TABLE_TYPES = "INSIDE_EDGE_TYPES";


		public static string GetConnectionString() {
			return System.Configuration.ConfigurationManager.ConnectionStrings["WhereYouAt_DatabaseConnectionString"].ConnectionString;
		}

		/// <summary>
		/// Creates a new entry in the products table.  Does precisely ZERO duplicate detection - you have been warned!
		/// </summary>
		/// <param name="manufacturerid"></param>
		/// <param name="typeid"></param>
		/// <param name="styleid"></param>
		/// <param name="colorid"></param>
		/// <param name="sizeid"></param>
		/// <returns>True if row was created, false if it wasn't (for any reason)</returns>
		public bool CreateNewProduct(string manufacturerid, string typeid, string styleid, string colorid, string sizeid) {

			SqlConnection myConn = new SqlConnection(GetConnectionString());

			try {
				myConn.Open();
				SqlCommand cmd = new SqlCommand("SELECT * FROM [" + DB_TABLE_AVAILABLE_PRODUCTS +"] WHERE 1=2", myConn);
				SqlDataAdapter da = new SqlDataAdapter(cmd);
				SqlCommandBuilder cb = new SqlCommandBuilder(da);
				DataSet ds = new DataSet();
				da.Fill(ds);

				if (ds.Tables[0].Rows.Count == 0) {
					DataRow dr = ds.Tables[0].NewRow();
					dr["manufacturerid"] = manufacturerid;
					dr["styleid"] = styleid;
					dr["typeid"] = typeid;
					dr["colorid"] = colorid;
					dr["sizeid"] = sizeid;
					ds.Tables[0].Rows.Add(dr);
				} else {
					// Not bothering with duplicate detection at this time
				}

				da.Update(ds);
				myConn.Close();

				return true;

			} catch (Exception e) {
				myConn.Close();
				return false;
			} finally {
				myConn.Close();
			}
		}

		/// <summary>
		/// Creates a new entry in the specified table.
		/// </summary>
		/// <param name="tablename">The name of the SQL table to update</param>
		/// <param name="entry">The [name] value to add.</param>
		/// <param name="entryidcolumnname">The name of the column that houses the entry's unique id (e.g. a part number)</param>
		/// <param name="entryid">The unique id for the row to be created.</param>
		/// <returns></returns>
		public bool CreateNewEntry(string tablename, string entry, string entryid) {

			SqlConnection myConn = new SqlConnection(GetConnectionString());

			try {
				myConn.Open();
				SqlCommand cmd = new SqlCommand("SELECT * FROM [" + tablename + "] WHERE [value] = @entryid" , myConn);
				cmd.Parameters.AddWithValue("@entryid", entryid);
				SqlDataAdapter da = new SqlDataAdapter(cmd);
				SqlCommandBuilder cb = new SqlCommandBuilder(da);
				DataSet ds = new DataSet();
				da.Fill(ds);

				if (ds.Tables[0].Rows.Count == 0) {
					DataRow dr = ds.Tables[0].NewRow();
					dr["name"] = entry;
					dr["value"] = entryid;
					ds.Tables[0].Rows.Add(dr);
				} else {
					return false;
				}

				da.Update(ds);
				myConn.Close();
				return true;

			} catch (Exception e) {
				myConn.Close();
				return false;
			} finally {
				myConn.Close();
			}
		}

		/// <summary>
		/// Edits an existing product in the available products table.  All columns are optional and if ommitted will be ignored when updating the row.
		/// </summary>
		/// <param name="productid"></param>
		/// <param name="manufacturerid"></param>
		/// <param name="typeid"></param>
		/// <param name="styleid"></param>
		/// <param name="colorid"></param>
		/// <param name="sizeid"></param>
		/// <returns></returns>
		public bool EditExistingProduct(string productid, string manufacturerid = null, string typeid = null, string styleid = null, string colorid = null, string sizeid = null) {

			SqlConnection myConn = new SqlConnection(GetConnectionString());

			try {
				myConn.Open();
				SqlCommand cmd = new SqlCommand("SELECT * FROM [" + DB_TABLE_AVAILABLE_PRODUCTS + "] WHERE [productid] = @productid", myConn);
				cmd.Parameters.AddWithValue("@productid", productid);
				SqlDataAdapter da = new SqlDataAdapter(cmd);
				SqlCommandBuilder cb = new SqlCommandBuilder(da);
				DataSet ds = new DataSet();
				da.Fill(ds);

				if (ds.Tables[0].Rows.Count == 1) {
					if (manufacturerid != null) ds.Tables[0].Rows[0]["manufacturerid"] = manufacturerid;
					if (styleid != null) ds.Tables[0].Rows[0]["styleid"] = styleid;
					if (typeid != null) ds.Tables[0].Rows[0]["typeid"] = typeid;
					if (colorid != null) ds.Tables[0].Rows[0]["coloridid"] = colorid;
					if (sizeid != null) ds.Tables[0].Rows[0]["sizeid"] = sizeid;

					da.Update(ds);
					myConn.Close();

					return true;
				}

				// Either the productid was not found or multiple rows were found with that productid (in which case, you kinda got bigger problems, dawg).
				return false;

			} catch (Exception e) {
				myConn.Close();
				return false;
			} finally {
				myConn.Close();
			}
		}

		/// <summary>
		/// Deletes the row with the supplied productid.  If two rows are found (this should never happen) the first one returned is deleted.
		/// </summary>
		/// <param name="productid">The product id to find and kill.</param>
		/// <returns></returns>
		public bool RemoveExistingProduct(string productid) {

			SqlConnection myConn = new SqlConnection(GetConnectionString());
			myConn.Open();
			SqlCommand cmd = new SqlCommand("SELECT * FROM [" + DB_TABLE_AVAILABLE_PRODUCTS + "] where [productid] = @productid", myConn);
			cmd.Parameters.AddWithValue("@productid", productid);
			SqlDataAdapter da = new SqlDataAdapter(cmd);
			SqlCommandBuilder cb = new SqlCommandBuilder(da);
			DataSet ds = new DataSet();
			da.Fill(ds);

			try {
				ds.Tables[0].Rows[0].Delete();
				da.Update(ds);
				myConn.Close();
				return true;
			} catch (Exception e) {
				return false;
			} finally {
				myConn.Close();
			}
		}

		/// <summary>
		/// Deletes the row with the supplied productid.  If two rows are found (this should never happen) the first one returned is deleted.
		/// </summary>
		/// <param name="productid">The product id to find and kill.</param>
		/// <returns></returns>
		public bool RemoveEntry(string tablename, String entryid) {

			SqlConnection myConn = new SqlConnection(GetConnectionString());
			myConn.Open();
			SqlCommand cmd = new SqlCommand("SELECT * FROM [" + tablename + "] where [value] = @entryid", myConn);
			cmd.Parameters.AddWithValue("@entryid", entryid);
			SqlDataAdapter da = new SqlDataAdapter(cmd);
			SqlCommandBuilder cb = new SqlCommandBuilder(da);
			DataSet ds = new DataSet();
			da.Fill(ds);

			try {
				ds.Tables[0].Rows[0].Delete();
				da.Update(ds);
				myConn.Close();
				return true;
			} catch (Exception e) {
				return false;
			} finally {
				myConn.Close();
			}
		}

		public DataSet SearchAvailableProducts(string terms) {

			SqlConnection myConn = new SqlConnection(GetConnectionString());

			try {
				myConn.Open();

				SqlCommand cmd = new SqlCommand(
					"SELECT " +
						"a1.productid as ProductID, " +
						"a2.[name] as Manufacturer, " +
						"a3.[name] as Type, " +
						"a4.[name] as Style, " +
						"a5.[name] as Color, " +
						"a6.[name] as Size " +
					"FROM [dbo].INSIDE_EDGE_AVAILABLE_PRODUCTS as a1 " +
						"inner join [dbo].[INSIDE_EDGE_MANUFACTURERS] as a2 on a1.manufacturerid = a2.value  " +
						"inner join [dbo].[INSIDE_EDGE_TYPES] as a3 on a1.typeid = a3.value " +
						"inner join [dbo].[INSIDE_EDGE_STYLES] as a4 on a1.styleid = a4.value  " +
						"inner join [dbo].[INSIDE_EDGE_COLORS] as a5 on a1.colorid = a5.value " +
						"inner join [dbo].[INSIDE_EDGE_SIZES] as a6 on a1.sizeid = a6.value " +
					"WHERE " +
						"a2.[name] like '%' + @man + '%' or " +
						"a3.[name] like '%' + @type + '%' or " +
						"a4.[name] like '%' + @style + '%' or " +
						"a5.[name] like '%' + @color + '%' or " +
						"a6.[name] like '%' + @size + '%'", myConn);

				cmd.Parameters.AddWithValue("@man", terms);
				cmd.Parameters.AddWithValue("@type", terms);
				cmd.Parameters.AddWithValue("@style", terms);
				cmd.Parameters.AddWithValue("@color", terms);
				cmd.Parameters.AddWithValue("@size", terms);

				SqlDataAdapter da = new SqlDataAdapter(cmd);
				DataSet ds = new DataSet();
				da.Fill(ds);
				return ds;

			} catch (Exception e) {
				myConn.Close();
				return null;
			} finally {
				myConn.Close();
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