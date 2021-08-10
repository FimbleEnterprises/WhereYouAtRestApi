<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Tools.aspx.cs" Inherits="WhereYouAtApi.Tools" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <style>
        .title {
            font-family:Arial;
            font-size:large;
        }

        .labels {
            font-family:Algerian
        }

        .result_label {
            font-family:Arial;
            color:tomato;
            font-size:small;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <p class="title">
            Admin tools and utilities for Where You At.
        </p>
        <div class="labels"> 
            <asp:Label ID="Label1" runat="server" Text="Validate FCM Token"></asp:Label>
            <br />
            <asp:TextBox ID="txtFcmToken" runat="server" ToolTip="FCM token to validate"></asp:TextBox>&nbsp&nbsp<asp:Button ID="btnValidateFcmToken" runat="server" Text="Validate" OnClick="btnValidateFcmToken_Click" />
        &nbsp;
            <asp:Label ID="lblFcmTokenResult" runat="server" Text="" CssClass="result_label"></asp:Label>
        </div>
        <div class="labels"> 
            <asp:Label ID="Label2" runat="server" Text="Get all userids"></asp:Label>
            <br />
            <asp:TextBox ID="txtGetUsersByEmail" runat="server" ToolTip="Email address to search"></asp:TextBox>&nbsp&nbsp<asp:Button ID="btnGetUseridsByEmail" runat="server" Text="Get ids" OnClick="btnGetUseridsByEmail_Click" />
        &nbsp;
            <asp:Label ID="lblGetUsersByEmailResults" runat="server" Text="" CssClass="result_label"></asp:Label>
        </div>
    </form>
</body>
</html>
