<%--<%@ Page Title="Reportes" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" AutoEventWireup="true" CodeBehind="Report.aspx.cs" Inherits="STPC.DynamicForms.Web.RT.Reports.Report" %>--%>

<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Report.aspx.cs" Inherits="STPC.DynamicForms.Web.RT.Reports.Report" %>

<%@ Register Assembly="Microsoft.ReportViewer.WebForms, Version=11.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91" Namespace="Microsoft.Reporting.WebForms" TagPrefix="rsweb" %>

<%--<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">Reportes</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="FormContent" runat="server">	 --%>
<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
            <rsweb:ReportViewer ID="ReportViewerCtrl" runat="server" AsyncRendering="false" 
                InteractivityPostBackMode="AlwaysAsynchronous" ShowParameterPrompts="false" ProcessingMode="Remote" Width="908px" Height="908px">
            </rsweb:ReportViewer>
        </div>
    </form>
</body>
</html>
<%--</asp:Content>--%>

<%--<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Report.aspx.cs" Inherits="STPC.DynamicForms.Web.RT.Reports.Report" %>

<%@ Register Assembly="Microsoft.ReportViewer.WebForms, Version=11.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91" Namespace="Microsoft.Reporting.WebForms" TagPrefix="rsweb" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	 <title></title>
</head>
<body>

</html>--%>