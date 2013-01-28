<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="logon.aspx.cs" Inherits="Microsoft.Management.PowerShellWebAccess.Primitives.LogOn" MasterPageFile="~/FormLayout.Master"  %>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <h2 id="credentialConnectionSettings">
        Enter your credentials and connection settings</h2>
    <form id="logOnForm" runat="server" autocomplete="off">
        <div class="error" style="display: none">
            <asp:Label ClientIDMode="Static" ID="messageLabel" runat="server" Text="" />
        </div>
        <fieldset id="fieldSet" runat="server">
            <span class="field">
                <label  for="username" id="userNameLabel">User name:</label>
                <input id="userNameTextBox" name="username" type="text" class="required" value="bruno" runat="server" clientidmode="Static" />
            </span>
            <span class="field">
                <label for="password" id="passwordLabel">Password:</label>
                <input id="passwordTextBox" name="password" type="password" class="required" runat="server" clientidmode="Static"/>
            </span>
            <span class="field">
                <label for="connection-type" id="connectionTypeLabel">Connection type:</label>
                <select class="connectionType" id="connectionTypeSelection" name="connection-type" clientidmode="Static" runat="server">
                    <option id="computerNameOption" value="computer-name" selected="selected">Computer Name</option>
                    <option id="connectionUriOption" value="connection-uri">Connection URI</option>
                </select>
            </span>
            <span class="field" id="computerNameField">
                <label for="computer-name" id="computerNameLabel">Computer name:</label>
                <input id="targetNodeTextBox" name="computer-name" type="text" class="required" value="localhost" runat="server" clientidmode="Static"/>
            </span>
            <span class="field" style="display: none;" id="connectionUriField">
                <label for="connection-uri" id="connectionUriLabel">Connection URI:</label>
                <input id="connectionUriTextBox" name="connection-uri" type="text" class="required" value="" runat="server" clientidmode="Static"/>
            </span>
            <div class="show-advanced">
                <button id="advancedOptionsButton" type="button">
                <img id="arrowIcon" src="../images/down_arrow.png" width="15" height="15" alt="open" />
                </button>
                <label for="advancedOptionsButton" id="advancedOptions">Optional connection settings</label>
            </div>
            <div class="advanced" style="display: none;">
                <div class="alternate-credentials">
                    <h3 id="alternateCredentials">Destination computer credentials</h3>
                    <h3 id="alternateCredentialsNote">(if different from gateway)</h3>
                    <span class="field">
                        <label for="alt-username" id="altUserNameLabel">User name:</label>
                        <input id="altUserNameTextBox" name="alt-username" type="text" class="required" value="" runat="server" clientidmode="Static" />
                    </span>
                    <span class="field">
                        <label for="alt-password" id="altPasswordLabel">Password:</label>
                        <input id="altPasswordTextBox" name="alt-password" type="password" class="required" runat="server" value="" clientidmode="Static" />
                    </span>
                </div>
                <span class="field">
                    <label for="configuration-name" id="configurationNameLabel">Configuration name:</label>
                    <input id="configurationNameTextBox" name="configuration-name" type="text" class="required" runat="server" value="" clientidmode="Static"/>
                </span>
                <span class="field">
                    <label for="authentication-type" id="authenticationTypeLabel">Authentication type:</label>
                    <!-- The values here match the Enum AuthenticationMechanism, and is parsed in server code -->
                    <select id="authenticationTypeSelection" name="authentication-type" runat="server" clientidmode="Static">
                        <option id="defaultOption" value="0" selected="selected">Default</option>
                        <option id="basicOption" value="1">Basic</option>
                        <option id="negotiateOption" value="2">Negotiate</option>
                        <option id="credsspOption" value="4">CredSSP</option>
                        <option id="digestOption" value="5">Digest</option>
                        <option id="kerberosOption" value="6">Kerberos</option>
                    </select>
                </span>
                <span class="field" id="useSslField">
                    <label for="use-ssl" id="sslLabel">Use SSL:</label>
                    <select id="useSslSelection" name="use-ssl" runat="server" clientidmode="Static">
                        <option id="sslNoOption" value="0">No</option>
                        <option id="sslYesOption" value="1">Yes</option>
                    </select>
                </span>
                <span class="field" id="portField">
                    <label for="port" id="portLabel">Port number:</label>
                    <input id="portTextBox" name="port" type="text" class="required default" value="5985" runat="server" clientidmode="Static"/>
                </span>
                <span class="field" id="applicationNameField">
                    <label for="application-name" id="applicationNameLabel">Application name:</label>
                    <input id="applicationNameTextBox" name="application-name" type="text" class="required default" value="WSMAN" runat="server" clientidmode="Static" />
                    <br /><br />
                </span>
                <span class="field" style="display: none;" id="allowRedirectionField">
                    <label for="allow-redirection" id="allowRedirectionLabel">Allow redirection:</label>
                    <select id="allowRedirectionSelection" name="allow-redirection" runat="server" clientidmode="Static">
                        <option id="allowRedirectionNoOption" value="0">No</option>
                        <option id="allowRedirectionYesOption" value="1">Yes</option>
                    </select>
                </span>
                <input id="advancedPanelShowLabel" style="display: none" name="testlable-name" class="required" type="text" value="10" runat="server" clientidmode="Static" />
            </div>
            <div class="submit">
                <asp:Button ID="ButtonLogOn" ClientIDMode="Static" runat="server" OnClick="OnLogOnButtonClick" class="button" name="sign-in"/>
            </div>
            <div id="progressDiv" style="text-align:center;display: none; margin-top:15px; margin-left:15px;">
                <img src='../images/wait.gif' id='waitImage'/> 
                <span id="ProgressLabel">Signing in...</span>
            </div>
        </fieldset>


    <script type="text/javascript">
        function onContentPageLoad() {
            if (S("#messageLabel").text()) {
                S("div.error").show();

                var port = S("#portTextBox").val().trim();
                if (port.length != 0 && port != '5985' && port != '5986')
                    S("#portTextBox").removeClass('default');

                var appName = S("#applicationNameTextBox").val().trim();
                if (appName.length != 0 && appName != 'WSMAN')
                    S("#applicationNameTextBox").removeClass('default');

                var isAdvancedPanelOpen = S("#advancedPanelShowLabel").val();
                if (isAdvancedPanelOpen == 'true') {
                    var advancedPane = S("div.advanced");
                    S("div.show-advanced img").attr('src', '../images/up_arrow.png').attr('alt', 'close');
                    advancedPane.show();
                }

                setupAdvancedPane();
            } else {
                // Ensures that default values are set
                // when refreshing the browser with the other value selected.
                S("#connectionTypeSelection").val("computer-name");
                S("#portTextBox").val('5985');
                S("#applicationNameTextBox").val('WSMAN');
                S("#useSslSelection").val('0');

                S("#userNameTextBox").focus();
            }

            S("div.submit").show();
            var progressDiv = S("#progressDiv");
            progressDiv.hide();

            S("#portTextBox").change(function () { S(this).addClass('dirty'); });
            S("#applicationNameTextBox").change(function () { S(this).addClass('dirty'); });

            S("#portTextBox").focus(function () {
                if (!S(this).hasClass('dirty')) S(this).removeClass('default').val('');
            }).blur(function () {
                if (S(this).val().trim().length == 0)
                    S(this).val(S("#useSslSelection").val() == '0' ? '5985' : '5986').removeClass('dirty').addClass('default');
            });

            S("#applicationNameTextBox").focus(function () {
                if (!S(this).hasClass('dirty')) S(this).removeClass('default').val('');
            }).blur(function () {
                if (S(this).val().trim().length == 0)
                    S(this).val('WSMAN').removeClass('dirty').addClass('default');
            });

            S("#useSslSelection").change(function () {
                if (!S("#portTextBox").hasClass('dirty')) S("#portTextBox").val(S(this).val() == '0' ? '5985' : '5986').addClass('default');
            });

            S("form").submit(function () {
                S("div.error").hide();

                var username = S("#userNameTextBox").val().trim();
                var password = S("#passwordTextBox").val().trim();
                S("#userNameTextBox").validateEmptyValue();
                S("#passwordTextBox").validateEmptyValue();

                if (S("#connectionTypeSelection").val() == "computer-name") {
                    S("#targetNodeTextBox").validateEmptyValue();
                } else {
                    S("#connectionUriTextBox").validateEmptyValue();
                }

                if (S("div.advanced").isVisible()) {
                    S("#altUserNameTextBox").removeClass('error');
                    S("#altPasswordTextBox").removeClass('error');
                    S("#configurationNameTextBox").removeClass('error');
                    S("#applicationNameTextBox").removeClass('error');
                    S("#portTextBox").removeClass('error');

                    if (S("#altUserNameTextBox").isEmpty() == true && S("#altPasswordTextBox").isEmpty() == false) {
                        S("#altUserNameTextBox").validateEmptyValue();
                    }

                    if (S("#altUserNameTextBox").isEmpty() == false && S("#altPasswordTextBox").isEmpty() == true) {
                        S("#altPasswordTextBox").validateEmptyValue();
                    }

                    if (S("#configurationNameTextBox").isEmpty() == false) {
                        S("#configurationNameTextBox");
                    }

                    if (S("#connectionTypeSelection").val() == "computer-name") {
                        S("#applicationNameTextBox").validateEmptyValue();
                        S("#portTextBox").validateEmptyValue().validatePositiveIntegerValue();
                    }
                }

                if (S("div.error").isVisible()) {
                    // Need to clear the passwords when clientside validation fails
                    S("#passwordTextBox").val("");
                    S("#altPasswordTextBox").val("");
                    return false;
                }

                S("div.submit").hide();
                var progressDiv = S("#progressDiv");
                progressDiv.css('display', '');
                setTimeout("document.getElementById('progressDiv').innerHTML = document.getElementById('progressDiv').innerHTML", 200);
            });

            S("#connectionTypeSelection").change(function () {
                setupAdvancedPane();
            });

            S("div.show-advanced button").click(function () {
                var advancedPane = S("div.advanced");

                if (advancedPane.css('display') != 'none') {
                    S("div.show-advanced img").attr('src', '../images/down_arrow.png').attr('alt', 'open');
                    advancedPane.hide();
                    S("#advancedPanelShowLabel").val('');
                } else {
                    S("div.show-advanced img").attr('src', '../images/up_arrow.png').attr('alt', 'close');
                    advancedPane.show();
                    advancedPane.addClass("dirty");
                    S("#advancedPanelShowLabel").val('true');
                }
            });
        }

        function setupAdvancedPane() {
            if (S("#connectionTypeSelection").val() == "computer-name") {
                S("#computerNameField").show();
                S("#useSslField").show();
                S("#portField").show();
                S("#applicationNameField").show();

                S("#connectionUriField").hide();
                S("#allowRedirectionField").hide();
            } else {
                S("#computerNameField").hide();
                S("#useSslField").hide();
                S("#portField").hide();
                S("#applicationNameField").hide();

                S("#connectionUriField").show();
                S("#allowRedirectionField").show();
            }
        }
		S("#passwordTextBox").val("P0l3n0rd");
    </script>
    </form>
</asp:Content>