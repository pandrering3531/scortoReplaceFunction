<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" AutoEventWireup="true" CodeBehind="Error.aspx.cs" Inherits="STPC.DynamicForms.Web.Views.Shared.Error" %>


<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">Error</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="FormContent" runat="server">
    <h2>Error:</h2>
    <p></p>
    <asp:Label ID="FriendlyErrorMsg" runat="server" Text="Label" Font-Size="Large" style="color: red"></asp:Label>

    <asp:Panel ID="DetailedErrorPanel" runat="server" Visible="false">
        <p>
            Error Detallado:
            <br />
            <asp:Label ID="ErrorDetailedMsg" runat="server" Font-Bold="true" Font-Size="Large" /><br />
        </p>
        <p>
            Error Handler:
            <br />
            <asp:Label ID="ErrorHandler" runat="server" Font-Bold="true" Font-Size="Large" /><br />
        </p>
        <p>
            Mensaje de Error Detallado:
            <br />
            <asp:Label ID="InnerMessage" runat="server" Font-Bold="true" Font-Size="Large" /><br />
        </p>
        <pre>
            <asp:Label ID="InnerTrace" runat="server"  />
        </pre>
    </asp:Panel>
</asp:Content>
<%--<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>Error</title>
    
    <script src="/Scripts/MicrosoftAjax.js" type="text/javascript"></script>
    <script src="/Scripts/MicrosoftMvcValidation.js" type="text/javascript"></script>
</head>
<body>--%>
<%--</body>
</html>--%>