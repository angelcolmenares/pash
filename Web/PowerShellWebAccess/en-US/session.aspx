<%@ Page Title="" Language="C#" MasterPageFile="~/FormLayout.Master" AutoEventWireup="true" CodeBehind="session.aspx.cs" Inherits="Microsoft.Management.PowerShellWebAccess.Primitives.Session" %>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <h2 id="existingSession">A Microsoft PowerShell session is already running.</h2>

    <p id="newSession">
    Multiple Microsoft PowerShell connections are supported only by creating a new browser session. This is not the same as a new tab or window. 
    Your browser might allow you to start more than one session. In Internet Explorer, for example, you can use the New Session command on the 
    File menu. Browsers on mobile devices might not allow you to create a new session. 
    </p>

    <p id="closeWindow">
    You can close this window and continue working in the active Microsoft PowerShell session. Otherwise, if you are finished working in this Windows 
    PowerShell session, click Sign Out.
    </p>
    
    <br />

    <input type="button" value="Sign Out" class="button" name="log-off" id="log-off" />

    <div class="push"></div>
</asp:Content>
