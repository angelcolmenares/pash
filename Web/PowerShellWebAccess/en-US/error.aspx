<%@ Page Title="" Language="C#" MasterPageFile="~/FormLayout.Master" AutoEventWireup="true" CodeBehind="error.aspx.cs" Inherits="Microsoft.Management.PowerShellWebAccess.Primitives.Error"%>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <h2 id="pageCannotDisplayed">The page cannot be displayed</h2>
    <div id="intenalServerError" class="page-error">Internal Server Error</div>
    <p id="problemPage">There is a problem with the page you are trying to open.  The page cannot be displayed.</p>
    <p id="closeWindow">For added security, close the window.<br />Alternatively, you can go back to the <a id="logonLink" href="./logon.aspx">sign-in page.</a></p>
    <div class="push"></div>
</asp:Content>
