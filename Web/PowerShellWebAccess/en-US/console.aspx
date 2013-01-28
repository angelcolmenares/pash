<%@ Page Language="C#" EnableSessionState="ReadOnly" AutoEventWireup="true" CodeBehind="console.aspx.cs" Inherits="Microsoft.Management.PowerShellWebAccess.Primitives.Console" %>
<%@ Import Namespace="Microsoft.Management.PowerShellWebAccess.Primitives" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.1//EN"
    "http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd"> 
 
<html xmlns="http://www.w3.org/1999/xhtml" xml:lang="en"> 
<head>	
    <title>Unix PowerShell Web Access</title>
    <meta http-equiv="Content-type" content="application/xhtml+xml"/> 
    <meta content="text/html; charset=UTF-8" http-equiv="Content-Type"/> 
    <meta content="en" name="language"/> 
    <meta content="en" http-equiv="Content-Language"/> 
    <meta http-equiv="X-UA-Compatible" content="IE=8"/> 
    <meta http-equiv="X-XSS-Protection" content="0"/> 
    <link rel="shortcut icon" href="../images/powershell_ico.ico" type="image/x-icon" /> 
    <link rel="apple-touch-icon" href="../images/powershell_ico.ico" type="image/x-icon" /> 
    <link rel="stylesheet" href="../css/site.css" type="text/css" media="screen" charset="utf-8"/>
    <meta name="viewport" content="width=device-width; initial-scale=1.0; minimum-scale=1.0; maximum-scale=1.0; user-scalable=no" />

    <style type="text/css" id="antiClickjack">body { display:none !important; }</style> 
    <script type="text/javascript">
        if (self === top) {
            var antiClickjack = document.getElementById("antiClickjack")
            antiClickjack.parentNode.removeChild(antiClickjack);
        } else {
            top.location = self.location;
        }
    </script>
</head>

<body class="console" onload="bootstrap();">
    <form id="mainForm" runat="server">
        <asp:scriptmanager ID="ScriptManager" runat="server" EnablePageMethods="true" />
        <div id="console" style="display: none">            
            <div id="output" class="output"></div>
            <textarea id="inputarea" class="input" disabled="disabled" rows="1" cols="1"></textarea>
            <input id="passwordTextBox" type="password" class="secure" value="" style="display:none" />
            <div class="status-bar" style="display:none;">
                <div id="percentComplete" class="percentage"><span></span></div>
                <div id="timeBar" class="time"><id id="timeRemaining">Time remaining </id><span></span></div>
                <div id="progress" class="progress"><div class="bar">&nbsp;</div></div>
                <pre></pre>
            </div>
            <pre class="error" style="display:none;"></pre>
            <div class="footer">
              <ul class="commands">
                  <li class="play first"><input id="submit" type="button" value="Submit" class="enabled" title="Submit (Enter)" /></li>
                  <li class="stop"><input id="cancel" type="button" value="Cancel" disabled="disabled" class="disabled" title="Cancel (CTRL-Q)"/></li>  
                  <li class="tab">
                    <button id="tabbutton" type="button" title="Tab Complete (TAB)" class="enabled">
                        <img id="tabCompletionIcon" src="../images/tab_completion.png" alt="Tab Complete"/>
                    </button>
                    </li>
                  <li id="history" class="label">History:</li>
                  <li class="history up">
                    <button id="historyup" type="button" title="Previous Command (Up Arrow)" disabled="disabled" class="disabled">
                        <img id="historyUpDisabledIcon" src="../images/up_disabled.png" alt="Previous Command (Up Arrow)"/>
                        <img id="historyUpEnabledIcon" src="../images/up.png" alt="Previous Command" style="display:none;"/>
                    </button>
                    </li>
                  <li class="history down">
                    <button id="historydown" type="button" title="Next Command (Down Arrow)" disabled="disabled" class="disabled">
                        <img id="historyDownDisabledIcon" src="../images/down_disabled.png" alt="Next Command (Down Arrow)"/>
                        <img id="historyDownEnabledIcon" src="../images/down.png" alt="Next Command" style="display:none;"/>
                    </button>
                    </li>
              </ul>
              <ul class="actions">
                  <li class="label machine-name" id="machinename"><id id="connectedTo">Connected to: </id><strong>...</strong></li>
                  <li class="logout"><input id="logoff" type="button" value="Sign Out" /></li>
              </ul>
              <div style="clear:both;"></div>
            </div>
        </div>
        <asp:HiddenField ID="sessionKey" runat="server"/>
    </form>
    <div id="timeout" style="display:none;">
        <h2 id="warning">Warning</h2>
        <div class="content">
            <p id="timedOutInactivity">This inactive session will time out in </p>
            <p id="countdown" class="countdown">&nbsp;</p>
            <p id="remainSignedIn">Do you want to remain signed in?</p>
            <p class="actions">
                <input id="timeoutkeepalive" type="button" value="Yes"  class="keep-alive" />
                <input id="timeoutlogout" type="button" value="No" class="logout" />
            </p>
        </div>
    </div>

    <script type="text/javascript" src="./powershell.console.ui.resources.js"></script>
    <script type="text/javascript" src="../scripts/system.common.js"></script>
    <script type="text/javascript" src="../scripts/powershell.console.ui.types.js"></script>
    <script type="text/javascript" src="../scripts/powershell.console.ui.extentions.js"></script>
    <script type="text/javascript" src="../scripts/powershell.console.ui.js"></script>
</body>
</html>
