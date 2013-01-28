<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="logout.aspx.cs" Inherits="Microsoft.Management.PowerShellWebAccess.Primitives.Logout" MasterPageFile="~/FormLayout.Master" %>
<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <h2 id="successfullyLoggedOut">You have successfully signed out.</h2>
    <p id="closeWindow">For added security close the window.<br /><br />Alternatively, you can return to the <a id="logonLink" href="./logon.aspx">sign-in page.</a></p>
    <div class="push"></div>
</asp:Content>
