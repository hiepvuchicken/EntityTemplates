<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ReportsCR.aspx.cs" Inherits="WebTest.Reports.ReportsCR" %>

<%@ Register Assembly="CrystalDecisions.Web, Version=13.0.3500.0, Culture=neutral, PublicKeyToken=692fbea5521e1304" Namespace="CrystalDecisions.Web" TagPrefix="CR" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    
    <form id="form1" runat="server">
       <%-- <asp:Button ID="btnPrint" runat="server" Text="Print Directly" OnClientClick="Print()"></asp:Button>--%>
        <div id="dvReport">
            <%--<table style="text-align: center; padding-top: 50px; font-family: Verdana,tahoma,calibri; font-size: 18px; font-weight: bold; width: 100%;">
                
            </table>--%>
            <CR:CrystalReportViewer ID="crViewer" runat="server" AutoDataBind="true" HasCrystalLogo="False" HasPrintButton="True" HasDrillUpButton="False" HasDrilldownTabs ="False" PrintMode="ActiveX" HasExportButton="False" HasToggleGroupTreeButton="False" HasToggleParameterPanelButton="False" ToolPanelView="None" Height="50px" Width="350px"/>
        </div>
    </form>
    <%--<script type="text/javascript">
        function Print() {
            var dvReport = document.getElementById("dvReport");
            var frame1 = dvReport.getElementsByTagName("iframe")[0];
            if (navigator.appName.indexOf("Internet Explorer") != -1) {
                frame1.name = frame1.id;
                window.frames[frame1.id].focus();
                window.frames[frame1.id].print();
            }
            else {
                var frameDoc = frame1.contentWindow ? frame1.contentWindow : frame1.contentDocument.document ? frame1.contentDocument.document : frame1.contentDocument;
                frameDoc.print();
            }
        }
    </script>--%>
</body>
</html>
