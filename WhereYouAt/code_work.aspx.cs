using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Windows.Forms;
using Control = System.Web.UI.Control;
using TextBox = System.Web.UI.WebControls.TextBox;

namespace InsideEdge {
	public partial class InsideEdgeHome : System.Web.UI.Page {
		protected void Page_Load(object sender, EventArgs e) {
			if (!IsPostBack) {
				txtSearch.Text = "";
				GridView1.DataSourceID = "sqlds_AvailableProducts";
				GridView1.DataBind();
			}
		}

		protected void btnAddNewProduct_Click(object sender, EventArgs e) {
			InsideEdge.Backend.MyDb db = new Backend.MyDb();

			bool wasSuccessful = db.CreateNewProduct(ddl_Manufacturer.SelectedValue, ddl_Type.SelectedValue
				, ddl_Style.SelectedValue, ddl_Color.SelectedValue, ddl_Size.SelectedValue);

			if (wasSuccessful) {
				GridView1.DataSourceID = null;
				GridView1.DataSourceID = "sqlds_AvailableProducts";
				GridView1.DataBind();
			}
		}

		protected void GridView1_SelectedIndexChanged(object sender, EventArgs e) {

		}

		protected void GridView1_RowCommand(object sender, GridViewCommandEventArgs e) {
			if (e.CommandName.Equals("REMOVE")) {
				try {
					int index = Int32.Parse((string)e.CommandArgument);
					string productid = GridView1.Rows[index].Cells[1].Text;

					InsideEdge.Backend.MyDb db = new Backend.MyDb();
					bool wasSuccessful = db.RemoveExistingProduct(productid);

					if (wasSuccessful) {
						GridView1.DataSourceID = null;
						GridView1.DataSourceID = "sqlds_AvailableProducts";
						GridView1.DataBind();
					}
				} catch (Exception ex) { }
			}
		}

		protected void lnkAddManuf_Click(object sender, EventArgs e) {
			Backend.MyDb db = new Backend.MyDb();
			bool wasSuccessful = db.CreateNewEntry(Backend.MyDb.DB_TABLE_MANUFACTURERS, txtAddManuf.Text, txtAddManufId.Text);
			if (wasSuccessful) {
				sqlds_Manufacturer.DataBind();
				ddl_Manufacturer.DataSource = null;
				ddl_Manufacturer.DataSourceID = "sqlds_Manufacturer";
				txtAddManuf.Text = "";
				txtAddManufId.Text = "";
			}
		}

		protected void lnkAddType_Click(object sender, EventArgs e) {
			Backend.MyDb db = new Backend.MyDb();
			bool wasSuccessful = db.CreateNewEntry(Backend.MyDb.DB_TABLE_TYPES, txtAddType.Text, txtAddTypeId.Text);
			if (wasSuccessful) {
				sqlds_Type.DataBind();
				ddl_Type.DataSource = null;
				ddl_Type.DataSourceID = "sqlds_Type";
				txtAddType.Text = "";
				txtAddTypeId.Text = "";
			}
		}

		protected void lnkAddStyle_Click(object sender, EventArgs e) {
			Backend.MyDb db = new Backend.MyDb();
			bool wasSuccessful = db.CreateNewEntry(Backend.MyDb.DB_TABLE_STYLES, txtAddStyle.Text, txtAddStyleId.Text);
			if (wasSuccessful) {
				sqlds_Style.DataBind();
				ddl_Style.DataSource = null;
				ddl_Style.DataSourceID = "sqlds_Style";
				txtAddStyle.Text = "";
				txtAddStyleId.Text = "";
			}
		}

		protected void lnkAddColor_Click(object sender, EventArgs e) {
			Backend.MyDb db = new Backend.MyDb();
			bool wasSuccessful = db.CreateNewEntry(Backend.MyDb.DB_TABLE_COLORS, txtAddColor.Text, txtAddColorId.Text);
			if (wasSuccessful) {
				sqlds_Color.DataBind();
				ddl_Color.DataSource = null;
				ddl_Color.DataSourceID = "sqlds_Color";
				txtAddColor.Text = "";
				txtAddColorId.Text = "";
			}
		}

		protected void lnkAddSize_Click(object sender, EventArgs e) {
			Backend.MyDb db = new Backend.MyDb();
			bool wasSuccessful = db.CreateNewEntry(Backend.MyDb.DB_TABLE_SIZES, txtAddSize.Text, txtAddSizeId.Text);
			if (wasSuccessful) {
				sqlds_Size.DataBind();
				ddl_Size.DataSource = null;
				ddl_Size.DataSourceID = "sqlds_Size";
				txtAddSizeId.Text = "";
				txtAddSize.Text = "";
			}
		}

		protected void lnkRemoveManuf_Click(object sender, EventArgs e) {
			Backend.MyDb db = new Backend.MyDb();
			bool wasSuccessful = db.RemoveEntry(Backend.MyDb.DB_TABLE_MANUFACTURERS, ddl_Manufacturer.SelectedValue);
			if (wasSuccessful) {
				sqlds_Manufacturer.DataBind();
				ddl_Manufacturer.DataSource = null;
				ddl_Manufacturer.DataSourceID = "sqlds_Manufacturer";
			}
		}

		protected void lnkRemoveType_Click(object sender, EventArgs e) {
			Backend.MyDb db = new Backend.MyDb();
			bool wasSuccessful = db.RemoveEntry(Backend.MyDb.DB_TABLE_TYPES, ddl_Type.SelectedValue);
			if (wasSuccessful) {
				sqlds_Type.DataBind();
				ddl_Type.DataSource = null;
				ddl_Type.DataSourceID = "sqlds_Type";
			}
		}

		protected void lnkRemoveStyle_Click(object sender, EventArgs e) {
			Backend.MyDb db = new Backend.MyDb();
			bool wasSuccessful = db.RemoveEntry(Backend.MyDb.DB_TABLE_STYLES, ddl_Style.SelectedValue);
			if (wasSuccessful) {
				sqlds_Style.DataBind();
				ddl_Style.DataSource = null;
				ddl_Style.DataSourceID = "sqlds_Style";
			}
		}

		protected void lnkRemoveColor_Click(object sender, EventArgs e) {
			Backend.MyDb db = new Backend.MyDb();
			bool wasSuccessful = db.RemoveEntry(Backend.MyDb.DB_TABLE_COLORS, ddl_Color.SelectedValue);
			if (wasSuccessful) {
				sqlds_Color.DataBind();
				ddl_Color.DataSource = null;
				ddl_Color.DataSourceID = "sqlds_Color";
			}
		}

		protected void lnkRemoveSize_Click(object sender, EventArgs e) {
			Backend.MyDb db = new Backend.MyDb();
			bool wasSuccessful = db.RemoveEntry(Backend.MyDb.DB_TABLE_SIZES, ddl_Size.SelectedValue);
			if (wasSuccessful) {
				sqlds_Size.DataBind();
				ddl_Size.DataSource = null;
				ddl_Size.DataSourceID = "sqlds_Size";
			}
		}

		protected void btnSearch_Click(object sender, EventArgs e) {
			if (txtSearch.Text.Length == 0) {
				GridView1.DataSource = null;
				GridView1.DataSourceID = "sqlds_AvailableProducts";
				GridView1.DataBind();
			} else {
				GridView1.DataSourceID = null;
				GridView1.DataSource = new Backend.MyDb().SearchAvailableProducts(txtSearch.Text);
				GridView1.DataBind();
			}
		}
	}
}