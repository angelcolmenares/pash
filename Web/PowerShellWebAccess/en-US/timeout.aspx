<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="timeout.aspx.cs" Inherits="Microsoft.Management.PowerShellWebAccess.Primitives.Timeout" MasterPageFile="~/FormLayout.Master"  %>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <h2 id="loggedOutInactivity">You have been signed out of Microsoft PowerShell Web Access due to inactivity.</h2>
    <p id="closeWindow">For added security, close the window.<br /><br />Alternatively, you can return to the  <a id="logonLink" href="./logon.aspx">sign-in page.</a></p>
</asp:Content>