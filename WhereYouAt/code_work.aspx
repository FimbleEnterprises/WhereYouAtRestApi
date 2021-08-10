<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="code_work.aspx.cs" Inherits="InsideEdge.InsideEdgeHome" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Quotes</title>
    <link href="styles/styles.css" rel="stylesheet" />
    <link rel="stylesheet" type="text/css" href="styles/modal-loading.css"/>
	<link rel="stylesheet" type="text/css" href="styles/modal-loading-animate.css" />
    <script src="https://code.jquery.com/jquery-3.3.1.min.js"></script>
	<script type="text/javascript" src="Scripts/modal-loading.js"></script>
    </head>
<body>
    <form id="form1" runat="server" defaultbutton="btnSearch">
        <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
        <asp:UpdatePanel ID="UpdatePanel1" runat="server">
            <ContentTemplate>
                <asp:Panel ID="pnlAll" runat="server" DefaultButton="btnSearch">
                <asp:Panel ID="pnlSomething" runat="server" DefaultButton="btnSearch">
                    <table>
                        <tr>
                            <td style="width: 90%">
                                <p class="label_top_title">Available Products</p>
                            </td>
                            <td>
                                <asp:Panel ID="pnlSearch" DefaultButton="btnSearch" runat="server">
                                    <asp:TextBox ID="txtSearch" runat="server"></asp:TextBox>
                                    <asp:Button ID="btnSearch" Width="75px" CssClass="myButton" runat="server" Text="Search" OnClick="btnSearch_Click" />
                                </asp:Panel>
                            </td>
                        </tr>
                    </table>
                </asp:Panel>
                <asp:Panel ID="Panel1" CssClass="myGridClass" Width="100%" runat="server">
                    <asp:SqlDataSource ID="sqlds_AvailableProducts" runat="server" ConnectionString="<%$ ConnectionStrings:WhereYouAt_DatabaseConnectionString %>" 
                        SelectCommand="SELECT a1.productid as ProductID, a2.[name] as Manufacturer, a3.[name] as Type, a4.[name] as Style, a5.[name] as Color, a6.[name] as Size
  FROM [dbo].INSIDE_EDGE_AVAILABLE_PRODUCTS as a1 
	inner join [dbo].[INSIDE_EDGE_MANUFACTURERS] as a2 on a1.manufacturerid = a2.value 
	inner join [dbo].[INSIDE_EDGE_TYPES] as a3 on a1.typeid = a3.value
	inner join [dbo].[INSIDE_EDGE_STYLES] as a4 on a1.styleid = a4.value 
	inner join [dbo].[INSIDE_EDGE_COLORS] as a5 on a1.colorid = a5.value 
	inner join [dbo].[INSIDE_EDGE_SIZES] as a6 on a1.sizeid = a6.value">
                    </asp:SqlDataSource>
                    <asp:GridView ID="GridView1" Width="100%" runat="server" CellPadding="4" DataSourceID="sqlds_AvailableProducts" AllowSorting="True"
                        OnRowCommand="GridView1_RowCommand"
                        OnSelectedIndexChanged="GridView1_SelectedIndexChanged" AutoGenerateColumns="False" BackColor="White" BorderColor="#CC9966" BorderStyle="None" BorderWidth="1px">
                        <Columns>
                            <asp:ButtonField CommandName="REMOVE" Text="delete" />
                            <asp:BoundField DataField="ProductID" HeaderText="ProductID" Visible="true" SortExpression="ProductID" />
                            <asp:BoundField DataField="Manufacturer" HeaderText="Manufacturer" SortExpression="Manufacturer" />
                            <asp:BoundField DataField="Type" HeaderText="Type" SortExpression="Type" />
                            <asp:BoundField DataField="Style" HeaderText="Style" SortExpression="Style" />
                            <asp:BoundField DataField="Color" HeaderText="Color" SortExpression="Color" />
                            <asp:BoundField DataField="Size" HeaderText="Size" SortExpression="Size" />
                        </Columns>
                        <FooterStyle BackColor="#FFFFCC" ForeColor="#330099" />
                        <HeaderStyle BackColor="#990000" Font-Bold="True" ForeColor="#FFFFCC" />
                        <PagerStyle BackColor="#FFFFCC" ForeColor="#330099" HorizontalAlign="Center" />
                        <RowStyle BackColor="White" ForeColor="#330099" />
                        <SelectedRowStyle BackColor="#FFCC66" Font-Bold="True" ForeColor="#663399" />
                        <SortedAscendingCellStyle BackColor="#FEFCEB" />
                        <SortedAscendingHeaderStyle BackColor="#AF0101" />
                        <SortedDescendingCellStyle BackColor="#F6F0C0" />
                        <SortedDescendingHeaderStyle BackColor="#7E0000" />
                    </asp:GridView>
               </asp:Panel>
                <br />
               <hr />
                <br />
                <asp:Panel runat="server" ID="pnlAddShit" CssClass="label_title">
                    <table>
                        <tr>
                            <td>
                                <p >Create new available product</p>
                            </td>
                            <td>

                            </td>
                            <td>
                                
                                <asp:Button ID="btnAddNewProduct" runat="server" CssClass="myButton" OnClick="btnAddNewProduct_Click" Text="Add Product" />
                                
                            </td>
                        </tr>
                    </table>
                    <table >
                        <tr class="label_title">
                            <td>Manufacturer</td>
                            <td>Type</td>
                            <td>Style</td>
                            <td>Color</td>
                            <td>Size</td>
                        </tr>
                        <tr>
                            <td>
                                <table>
                                    <tr>
                                        <td>
                                            <asp:DropDownList ID="ddl_Manufacturer" runat="server" DataSourceID="sqlds_Manufacturer" DataTextField="name" DataValueField="value"></asp:DropDownList>
                                            <br />
                                            <asp:LinkButton ID="lnkRemoveManuf" runat="server" OnClick="lnkRemoveManuf_Click" CssClass="tinyLnkbtn">Remove Manufacturer</asp:LinkButton>
                                   
                                        </td>
                                    </tr>
                                </table>
                                <hr />
                                <div>
                                    <table>
                                        <tr>
                                            <td>
                                                <asp:Label ID="Label2" runat="server" Text="Name/ID" CssClass="label_title"></asp:Label>
                                                <br />
                                                <asp:TextBox ID="txtAddManuf" ToolTip="Enter new manufacturer" runat="server"></asp:TextBox>
                                                <br />
                                                <asp:TextBox ID="txtAddManufId" ToolTip="Enter a unique id for the entry" runat="server"></asp:TextBox>
                                                <br />
                                                <asp:LinkButton ID="lnkAddManuf" runat="server" OnClick="lnkAddManuf_Click" CssClass="tinyLnkbtn">Add New Manufacturer</asp:LinkButton>
                                            </td>
                                        </tr>
                                    </table>
                                </div>
                            </td>
                            <td>
                                <table>
                                    <tr>
                                        <td>
                                            <asp:DropDownList ID="ddl_Type" runat="server" DataSourceID="sqlds_Type" DataTextField="name" DataValueField="value"></asp:DropDownList>
                                            <br />
                                            <asp:LinkButton ID="lnkRemoveType" runat="server" OnClick="lnkRemoveType_Click" CssClass="tinyLnkbtn">Remove Type</asp:LinkButton>
                                        </td>
                                    </tr>
                                </table>
                                <hr />
                                <div>
                                    <table>
                                        <tr>
                                            <td>
                                                <asp:Label ID="Label4" runat="server" Text="Name/ID" CssClass="label_title"></asp:Label>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td>
                                                <asp:TextBox ID="txtAddType" ToolTip="Enter new manufacturer" runat="server"></asp:TextBox>
                                                <br />
                                                <asp:TextBox ID="txtAddTypeId" ToolTip="Enter a unique id for the entry" runat="server"></asp:TextBox>
                                                <br />
                                                <asp:LinkButton ID="lnkAddType" runat="server" OnClick="lnkAddType_Click" CssClass="tinyLnkbtn">Add New Type</asp:LinkButton>
                                        </tr>
                                    </table>
                                </div>
                            </td>
                            <td>
                                <table>
                                    <tr>
                                        <td>
                                            <asp:DropDownList ID="ddl_Style" runat="server" DataSourceID="sqlds_Style" DataTextField="name" DataValueField="value"></asp:DropDownList>
                                            <br />
                                            <asp:LinkButton ID="lnkRemoveStyle" runat="server" OnClick="lnkRemoveStyle_Click" CssClass="tinyLnkbtn">Remove Style</asp:LinkButton>
                                        </td>
                                    </tr>
                                </table>
                                <hr />
                                <div>
                                    <table>
                                        <tr>
                                            <td>
                                                <asp:Label ID="Label6" runat="server" Text="Name/ID" CssClass="label_title"></asp:Label>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td>
                                                <asp:TextBox ID="txtAddStyle" ToolTip="Enter new style" runat="server"></asp:TextBox>
                                                <br />
                                                <asp:TextBox ID="txtAddStyleId" ToolTip="Enter a unique id for the entry" runat="server"></asp:TextBox>
                                                <br />
                                                <asp:LinkButton ID="lnkAddStyle" runat="server" OnClick="lnkAddStyle_Click" CssClass="tinyLnkbtn">Add New Style</asp:LinkButton>
                                            </td>
                                        </tr>
                                    </table>
                                </div>
                            </td>
                            <td>
                                <table>
                                    <tr>
                                        <td>
                                            <asp:DropDownList ID="ddl_Color" runat="server" DataSourceID="sqlds_Color" DataTextField="name" DataValueField="value"></asp:DropDownList>
                                            <br />
                                            <asp:LinkButton ID="lnkRemoveColor" runat="server" OnClick="lnkRemoveColor_Click" CssClass="tinyLnkbtn">Remove Color</asp:LinkButton>
                                        </td>
                                    </tr>
                                </table>
                                <hr />
                                <div>
                                    <table>
                                        <tr>
                                            <td>
                                                <asp:Label ID="Label10" runat="server" Text="Name/ID" CssClass="label_title"></asp:Label>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td>
                                                <asp:TextBox ID="txtAddColor" ToolTip="Enter new color" runat="server"></asp:TextBox>
                                                <br />
                                                <asp:TextBox ID="txtAddColorId" ToolTip="Enter a unique id for the entry" runat="server"></asp:TextBox>
                                                <br />
                                                <asp:LinkButton ID="lnkAddColor" runat="server" OnClick="lnkAddColor_Click" CssClass="tinyLnkbtn">Add New Color</asp:LinkButton>
                                            </td>
                                        </tr>
                                    </table>
                                </div>
                            </td>
                            <td>
                                <table>
                                    <tr>
                                        <td>
                                            <asp:DropDownList ID="ddl_Size" runat="server" DataSourceID="sqlds_Size" DataTextField="name" DataValueField="value"></asp:DropDownList>
                                            <br />
                                            <asp:LinkButton ID="lnkRemoveSize" runat="server" OnClick="lnkRemoveSize_Click" CssClass="tinyLnkbtn">Remove Size</asp:LinkButton>
                                        </td>
                                    </tr>
                                </table>
                                <hr />
                                <div>
                                    <table>
                                        <tr>
                                            <td>
                                                <asp:Label ID="Label8" runat="server" Text="Name/ID" CssClass="label_title"></asp:Label>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td>
                                                <asp:TextBox ID="txtAddSize" ToolTip="Enter new manufacturer" runat="server"></asp:TextBox>
                                                <br />
                                                <asp:TextBox ID="txtAddSizeId" ToolTip="Enter a unique id for the entry" runat="server"></asp:TextBox>
                                                <br />
                                                <asp:LinkButton ID="lnkAddSize" runat="server" OnClick="lnkAddSize_Click" CssClass="tinyLnkbtn">Add New Size</asp:LinkButton>
                                            </td>
                                        </tr>
                                    </table>
                                </div>
                            </td>
                            <td>
                                &nbsp;</td>
                            <td>
                                
                            </td>
                        </tr>
                    </table>
                </asp:Panel>

                <div style="width:100%">
                    <asp:SqlDataSource ID="sqlds_Manufacturer" runat="server" ConnectionString="<%$ ConnectionStrings:WhereYouAt_DatabaseConnectionString %>" SelectCommand="SELECT [value], [name] FROM [INSIDE_EDGE_MANUFACTURERS] ORDER BY [name]"></asp:SqlDataSource>
                    <asp:SqlDataSource ID="sqlds_Type" runat="server" ConnectionString="<%$ ConnectionStrings:WhereYouAt_DatabaseConnectionString %>" SelectCommand="SELECT [value], [name] FROM [INSIDE_EDGE_TYPES] ORDER BY [name]"></asp:SqlDataSource>
                    <asp:SqlDataSource ID="sqlds_Style" runat="server" ConnectionString="<%$ ConnectionStrings:WhereYouAt_DatabaseConnectionString %>" SelectCommand="SELECT [value], [name] FROM [INSIDE_EDGE_STYLES] ORDER BY [name]"></asp:SqlDataSource>
                    <asp:SqlDataSource ID="sqlds_Color" runat="server" ConnectionString="<%$ ConnectionStrings:WhereYouAt_DatabaseConnectionString %>" SelectCommand="SELECT [value], [name] FROM [INSIDE_EDGE_COLORS] ORDER BY [name]"></asp:SqlDataSource>
                    <asp:SqlDataSource ID="sqlds_Size" runat="server" ConnectionString="<%$ ConnectionStrings:WhereYouAt_DatabaseConnectionString %>" SelectCommand="SELECT [value], [name] FROM [INSIDE_EDGE_SIZES] ORDER BY [name]"></asp:SqlDataSource>
                </div>
                    </asp:Panel>
            </ContentTemplate>
        </asp:UpdatePanel>
        <asp:UpdateProgress ID="UpdateProgress1" DisplayAfter="350" AssociatedUpdatePanelID="UpdatePanel1" runat="server">
            <ProgressTemplate>
                <asp:Label ID="Label1" runat="server" Text="Hey!  I'm fuckin working here!"></asp:Label>
            </ProgressTemplate>
        </asp:UpdateProgress>
    </form>
</body>
</html>
