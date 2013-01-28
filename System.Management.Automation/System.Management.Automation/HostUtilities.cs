namespace System.Management.Automation
{
    using Microsoft.PowerShell.Commands;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Management.Automation.Host;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Runspaces;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;

    internal static class HostUtilities
    {
        private static string checkForCommandInCurrentDirectoryScript = "\r\n        [System.Diagnostics.DebuggerHidden()]\r\n        param()\r\n\r\n        $foundSuggestion = $false\r\n        \r\n        if($lastError -and\r\n            ($lastError.Exception -is \"System.Management.Automation.CommandNotFoundException\"))\r\n        {\r\n            $escapedCommand = [System.Management.Automation.WildcardPattern]::Escape($lastError.TargetObject)\r\n            $foundSuggestion = @(Get-Command ($ExecutionContext.SessionState.Path.Combine(\".\", $escapedCommand)) -ErrorAction Ignore).Count -gt 0\r\n        }\r\n\r\n        $foundSuggestion\r\n        ";
        private static ArrayList suggestions = new ArrayList(new Hashtable[] { NewSuggestion(1, "Transactions", SuggestionMatchType.Command, "^Start-Transaction", SuggestionStrings.Suggestion_StartTransaction, true), NewSuggestion(2, "Transactions", SuggestionMatchType.Command, "^Use-Transaction", SuggestionStrings.Suggestion_UseTransaction, true), NewSuggestion(3, "General", SuggestionMatchType.Dynamic, ScriptBlock.Create(checkForCommandInCurrentDirectoryScript), ScriptBlock.Create(StringUtil.Format(SuggestionStrings.Suggestion_CommandExistsInCurrentDirectory, "$($lastError.TargetObject)", @".\$($lastError.TargetObject)")), true) });

        internal static PSCredential CredUIPromptForCredential(string caption, string message, string userName, string targetName, PSCredentialTypes allowedCredentialTypes, PSCredentialUIOptions options, IntPtr parentHWND)
        {
            if (string.IsNullOrEmpty(caption))
            {
                caption = CredUI.PromptForCredential_DefaultCaption;
            }
            if (string.IsNullOrEmpty(message))
            {
                message = CredUI.PromptForCredential_DefaultMessage;
            }
            if (caption.Length > 0x80)
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, CredUI.PromptForCredential_InvalidCaption, new object[] { 0x80 }));
            }
            if (message.Length > 0x400)
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, CredUI.PromptForCredential_InvalidMessage, new object[] { 0x400 }));
            }
            if ((userName != null) && (userName.Length > 0x201))
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, CredUI.PromptForCredential_InvalidUserName, new object[] { 0x201 }));
            }
            CREDUI_INFO structure = new CREDUI_INFO {
                pszCaptionText = caption,
                pszMessageText = message
            };
            StringBuilder pszUserName = new StringBuilder(userName, 0x201);
            StringBuilder pszPassword = new StringBuilder(0x100);
            bool flag = false;
            int pfSave = Convert.ToInt32(flag);
            structure.cbSize = Marshal.SizeOf(structure);
            structure.hwndParent = parentHWND;
            CREDUI_FLAGS dwFlags = CREDUI_FLAGS.DO_NOT_PERSIST;
            if ((allowedCredentialTypes & PSCredentialTypes.Domain) != PSCredentialTypes.Domain)
            {
                dwFlags |= CREDUI_FLAGS.GENERIC_CREDENTIALS;
                if ((options & PSCredentialUIOptions.AlwaysPrompt) == PSCredentialUIOptions.AlwaysPrompt)
                {
                    dwFlags |= CREDUI_FLAGS.ALWAYS_SHOW_UI;
                }
            }
            CredUIReturnCodes codes = CredUIReturnCodes.ERROR_INVALID_PARAMETER;
            if ((pszUserName.Length <= 0x201) && (pszPassword.Length <= 0x100))
            {
                codes = CredUIPromptForCredentials(ref structure, targetName, IntPtr.Zero, 0, pszUserName, 0x201, pszPassword, 0x100, ref pfSave, dwFlags);
            }
            if (codes == CredUIReturnCodes.NO_ERROR)
            {
                string str = null;
                if (pszUserName != null)
                {
                    str = pszUserName.ToString();
                }
                str = str.TrimStart(new char[] { '\\' });
                SecureString password = new SecureString();
                for (int i = 0; i < pszPassword.Length; i++)
                {
                    password.AppendChar(pszPassword[i]);
                    pszPassword[i] = '\0';
                }
                if (!string.IsNullOrEmpty(str))
                {
                    return new PSCredential(str, password);
                }
                return null;
            }
            return null;
        }

        [DllImport("credui", EntryPoint="CredUIPromptForCredentialsW", CharSet=CharSet.Unicode)]
        private static extern CredUIReturnCodes CredUIPromptForCredentials(ref CREDUI_INFO pUiInfo, string pszTargetName, IntPtr Reserved, int dwAuthError, StringBuilder pszUserName, int ulUserNameMaxChars, StringBuilder pszPassword, int ulPasswordMaxChars, ref int pfSave, CREDUI_FLAGS dwFlags);
        private static string GetAllUsersFolderPath(string shellId)
        {
            string applicationBase = string.Empty;
            try
            {
                applicationBase = Utils.GetApplicationBase(shellId);
            }
            catch (SecurityException)
            {
            }
            return applicationBase;
        }

        internal static PSObject GetDollarProfile(string allUsersAllHosts, string allUsersCurrentHost, string currentUserAllHosts, string currentUserCurrentHost)
        {
            PSObject obj2 = new PSObject(currentUserCurrentHost);
            obj2.Properties.Add(new PSNoteProperty("AllUsersAllHosts", allUsersAllHosts));
            obj2.Properties.Add(new PSNoteProperty("AllUsersCurrentHost", allUsersCurrentHost));
            obj2.Properties.Add(new PSNoteProperty("CurrentUserAllHosts", currentUserAllHosts));
            obj2.Properties.Add(new PSNoteProperty("CurrentUserCurrentHost", currentUserCurrentHost));
            return obj2;
        }

        internal static string GetFullProfileFileName(string shellId, bool forCurrentUser)
        {
            return GetFullProfileFileName(shellId, forCurrentUser, false);
        }

        internal static string GetFullProfileFileName(string shellId, bool forCurrentUser, bool useTestProfile)
        {
            string allUsersFolderPath = null;
            if (forCurrentUser)
            {
                allUsersFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), Utils.ProductNameForDirectory);
            }
            else
            {
                allUsersFolderPath = GetAllUsersFolderPath(shellId);
                if (string.IsNullOrEmpty(allUsersFolderPath))
                {
                    return "";
                }
            }
            string str2 = useTestProfile ? "profile_test.ps1" : "profile.ps1";
            if (!string.IsNullOrEmpty(shellId))
            {
                str2 = shellId + "_" + str2;
            }
            return (allUsersFolderPath = Path.Combine(allUsersFolderPath, str2));
        }

        internal static string GetMaxLines(string source, int maxLines)
        {
            if (string.IsNullOrEmpty(source))
            {
                return string.Empty;
            }
            StringBuilder builder = new StringBuilder();
            int num = 0;
            int num2 = 1;
            while (num < source.Length)
            {
                char ch = source[num];
                if (ch == '\n')
                {
                    num2++;
                }
                builder.Append(ch);
                if (num2 == maxLines)
                {
                    builder.Append("...");
                    break;
                }
                num++;
            }
            return builder.ToString();
        }

        public static PSCommand[] GetProfileCommands(string shellId)
        {
            return GetProfileCommands(shellId, false);
        }

        internal static PSCommand[] GetProfileCommands(string shellId, bool useTestProfile)
        {
            string str;
            string str2;
            string str3;
            string str4;
            PSObject obj2;
            List<PSCommand> list = new List<PSCommand>();
            GetProfileObjectData(shellId, useTestProfile, out str, out str2, out str3, out str4, out obj2);
            PSCommand item = new PSCommand();
            item.AddCommand("set-variable");
            item.AddParameter("Name", "profile");
            item.AddParameter("Value", obj2);
            item.AddParameter("Option", ScopedItemOptions.None);
            list.Add(item);
            string[] strArray = new string[] { str, str2, str3, str4 };
            foreach (string str5 in strArray)
            {
                if (File.Exists(str5))
                {
                    item = new PSCommand();
                    item.AddCommand(str5, false);
                    list.Add(item);
                }
            }
            return list.ToArray();
        }

        internal static void GetProfileObjectData(string shellId, bool useTestProfile, out string allUsersAllHosts, out string allUsersCurrentHost, out string currentUserAllHosts, out string currentUserCurrentHost, out PSObject dollarProfile)
        {
            allUsersAllHosts = GetFullProfileFileName(null, false, useTestProfile);
            allUsersCurrentHost = GetFullProfileFileName(shellId, false, useTestProfile);
            currentUserAllHosts = GetFullProfileFileName(null, true, useTestProfile);
            currentUserCurrentHost = GetFullProfileFileName(shellId, true, useTestProfile);
            dollarProfile = GetDollarProfile(allUsersAllHosts, allUsersCurrentHost, currentUserAllHosts, currentUserCurrentHost);
        }

        internal static string GetRemotePrompt(RemoteRunspace runspace, string basePrompt)
        {
            return string.Format(CultureInfo.InvariantCulture, "[{0}]: {1}", new object[] { runspace.ConnectionInfo.ComputerName, basePrompt });
        }

        internal static ArrayList GetSuggestion(Runspace runspace)
        {
            LocalRunspace runspace2 = runspace as LocalRunspace;
            if (runspace2 == null)
            {
                return new ArrayList();
            }
            bool questionMarkVariableValue = runspace2.ExecutionContext.QuestionMarkVariableValue;
            HistoryInfo[] infoArray = runspace2.History.GetEntries((long) (-1L), 1L, true);
            if (infoArray.Length == 0)
            {
                return new ArrayList();
            }
            HistoryInfo lastHistory = infoArray[0];
            ArrayList dollarErrorVariable = (ArrayList) runspace2.GetExecutionContext.DollarErrorVariable;
            object lastError = null;
            if (dollarErrorVariable.Count > 0)
            {
                lastError = dollarErrorVariable[0] as Exception;
                ErrorRecord errorRecord = null;
                if (lastError == null)
                {
                    errorRecord = dollarErrorVariable[0] as ErrorRecord;
                }
                else if (lastError is RuntimeException)
                {
                    errorRecord = ((RuntimeException) lastError).ErrorRecord;
                }
                if ((errorRecord != null) && (errorRecord.InvocationInfo != null))
                {
                    if (errorRecord.InvocationInfo.HistoryId == lastHistory.Id)
                    {
                        lastError = errorRecord;
                    }
                    else
                    {
                        lastError = null;
                    }
                }
            }
            Runspace defaultRunspace = null;
            bool flag2 = false;
            if (Runspace.DefaultRunspace != runspace)
            {
                defaultRunspace = Runspace.DefaultRunspace;
                flag2 = true;
                Runspace.DefaultRunspace = runspace;
            }
            ArrayList list2 = null;
            try
            {
                list2 = GetSuggestion(lastHistory, lastError, dollarErrorVariable);
            }
            finally
            {
                if (flag2)
                {
                    Runspace.DefaultRunspace = defaultRunspace;
                }
            }
            runspace2.ExecutionContext.QuestionMarkVariableValue = questionMarkVariableValue;
            return list2;
        }

        internal static ArrayList GetSuggestion(HistoryInfo lastHistory, object lastError, ArrayList errorList)
        {
            ArrayList list = new ArrayList();
            PSModuleInfo invocationModule = new PSModuleInfo(true);
            invocationModule.SessionState.PSVariable.Set("lastHistory", lastHistory);
            invocationModule.SessionState.PSVariable.Set("lastError", lastError);
            int count = 0;
            foreach (Hashtable hashtable in suggestions)
            {
                count = errorList.Count;
                if (LanguagePrimitives.IsTrue(hashtable["Enabled"]))
                {
                    SuggestionMatchType type = (SuggestionMatchType) LanguagePrimitives.ConvertTo(hashtable["MatchType"], typeof(SuggestionMatchType), CultureInfo.InvariantCulture);
                    if (type == SuggestionMatchType.Dynamic)
                    {
                        object obj2 = null;
                        ScriptBlock sb = hashtable["Rule"] as ScriptBlock;
                        if (sb == null)
                        {
                            hashtable["Enabled"] = false;
                            throw new ArgumentException(SuggestionStrings.RuleMustBeScriptBlock, "Rule");
                        }
                        try
                        {
                            obj2 = invocationModule.Invoke(sb, null);
                        }
                        catch (Exception exception)
                        {
                            CommandProcessorBase.CheckForSevereException(exception);
                            hashtable["Enabled"] = false;
                            continue;
                        }
                        if (LanguagePrimitives.IsTrue(obj2))
                        {
                            string suggestionText = GetSuggestionText(hashtable["Suggestion"], invocationModule);
                            if (!string.IsNullOrEmpty(suggestionText))
                            {
                                string str2 = string.Format(Thread.CurrentThread.CurrentCulture, "Suggestion [{0},{1}]: {2}", new object[] { (int) hashtable["Id"], (string) hashtable["Category"], suggestionText });
                                list.Add(str2);
                            }
                        }
                    }
                    else
                    {
                        string input = string.Empty;
                        switch (type)
                        {
                            case SuggestionMatchType.Command:
                                input = lastHistory.CommandLine;
                                break;

                            case SuggestionMatchType.Error:
                                if (lastError != null)
                                {
                                    Exception exception2 = lastError as Exception;
                                    if (exception2 != null)
                                    {
                                        input = exception2.Message;
                                    }
                                    else
                                    {
                                        input = lastError.ToString();
                                    }
                                }
                                break;

                            default:
                                hashtable["Enabled"] = false;
                                throw new ArgumentException(SuggestionStrings.InvalidMatchType, "MatchType");
                        }
                        if (Regex.IsMatch(input, (string) hashtable["Rule"], RegexOptions.IgnoreCase))
                        {
                            string str4 = GetSuggestionText(hashtable["Suggestion"], invocationModule);
                            if (!string.IsNullOrEmpty(str4))
                            {
                                string str5 = string.Format(Thread.CurrentThread.CurrentCulture, "Suggestion [{0},{1}]: {2}", new object[] { (int) hashtable["Id"], (string) hashtable["Category"], str4 });
                                list.Add(str5);
                            }
                        }
                    }
                    if (errorList.Count != count)
                    {
                        hashtable["Enabled"] = false;
                    }
                }
            }
            return list;
        }

        private static string GetSuggestionText(object suggestion, PSModuleInfo invocationModule)
        {
            if (!(suggestion is ScriptBlock))
            {
                return (string) LanguagePrimitives.ConvertTo(suggestion, typeof(string), Thread.CurrentThread.CurrentCulture);
            }
            ScriptBlock sb = (ScriptBlock) suggestion;
            object valueToConvert = null;
            try
            {
                valueToConvert = invocationModule.Invoke(sb, null);
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                return string.Empty;
            }
            return (string) LanguagePrimitives.ConvertTo(valueToConvert, typeof(string), Thread.CurrentThread.CurrentCulture);
        }

        internal static bool IsProcessInteractive(InvocationInfo invocationInfo)
        {
            if (invocationInfo.CommandOrigin == CommandOrigin.Runspace)
            {
                if (Process.GetCurrentProcess().MainWindowHandle == IntPtr.Zero)
                {
                    return false;
                }
                try
                {
                    Process currentProcess = Process.GetCurrentProcess();
                    TimeSpan span = (TimeSpan) (DateTime.Now - currentProcess.StartTime);
                    TimeSpan span2 = span - currentProcess.TotalProcessorTime;
                    if (span2.TotalSeconds > 2.0)
                    {
                        return true;
                    }
                }
                catch (Win32Exception)
                {
                    return false;
                }
            }
            return false;
        }

        private static Hashtable NewSuggestion(int id, string category, SuggestionMatchType matchType, ScriptBlock rule, ScriptBlock suggestion, bool enabled)
        {
            Hashtable hashtable = new Hashtable(StringComparer.CurrentCultureIgnoreCase);
            hashtable["Id"] = id;
            hashtable["Category"] = category;
            hashtable["MatchType"] = matchType;
            hashtable["Rule"] = rule;
            hashtable["Suggestion"] = suggestion;
            hashtable["Enabled"] = enabled;
            return hashtable;
        }

        private static Hashtable NewSuggestion(int id, string category, SuggestionMatchType matchType, string rule, string suggestion, bool enabled)
        {
            Hashtable hashtable = new Hashtable(StringComparer.CurrentCultureIgnoreCase);
            hashtable["Id"] = id;
            hashtable["Category"] = category;
            hashtable["MatchType"] = matchType;
            hashtable["Rule"] = rule;
            hashtable["Suggestion"] = suggestion;
            hashtable["Enabled"] = enabled;
            return hashtable;
        }

        internal static string RemoveGuidFromMessage(string message, out bool matchPattern)
        {
            matchPattern = false;
            if (!string.IsNullOrEmpty(message))
            {
                Match match = Regex.Match(message, @"^([\d\w]{8}\-[\d\w]{4}\-[\d\w]{4}\-[\d\w]{4}\-[\d\w]{12}:).*");
                if (match.Success)
                {
                    string str = match.Groups[1].Captures[0].Value;
                    message = message.Remove(0, str.Length);
                    matchPattern = true;
                }
            }
            return message;
        }

        internal static string RemoveIdentifierInfoFromMessage(string message, out bool matchPattern)
        {
            matchPattern = false;
            if (!string.IsNullOrEmpty(message))
            {
                Match match = Regex.Match(message, @"^([\d\w]{8}\-[\d\w]{4}\-[\d\w]{4}\-[\d\w]{4}\-[\d\w]{12}:\[.*\]:).*");
                if (match.Success)
                {
                    string str = match.Groups[1].Captures[0].Value;
                    message = message.Remove(0, str.Length);
                    matchPattern = true;
                }
            }
            return message;
        }

        [Flags]
        private enum CREDUI_FLAGS
        {
            ALWAYS_SHOW_UI = 0x80,
            COMPLETE_USERNAME = 0x800,
            DO_NOT_PERSIST = 2,
            EXCLUDE_CERTIFICATES = 8,
            EXPECT_CONFIRMATION = 0x20000,
            GENERIC_CREDENTIALS = 0x40000,
            INCORRECT_PASSWORD = 1,
            KEEP_USERNAME = 0x100000,
            PASSWORD_ONLY_OK = 0x200,
            PERSIST = 0x1000,
            REQUEST_ADMINISTRATOR = 4,
            REQUIRE_CERTIFICATE = 0x10,
            REQUIRE_SMARTCARD = 0x100,
            SERVER_CREDENTIAL = 0x4000,
            SHOW_SAVE_CHECK_BOX = 0x40,
            USERNAME_TARGET_CREDENTIALS = 0x80000,
            VALIDATE_USERNAME = 0x400
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct CREDUI_INFO
        {
            public int cbSize;
            public IntPtr hwndParent;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pszMessageText;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pszCaptionText;
            public IntPtr hbmBanner;
        }

        private enum CredUIReturnCodes
        {
            ERROR_CANCELLED = 0x4c7,
            ERROR_INSUFFICIENT_BUFFER = 0x7a,
            ERROR_INVALID_ACCOUNT_NAME = 0x523,
            ERROR_INVALID_FLAGS = 0x3ec,
            ERROR_INVALID_PARAMETER = 0x57,
            ERROR_NO_SUCH_LOGON_SESSION = 0x520,
            ERROR_NOT_FOUND = 0x490,
            NO_ERROR = 0
        }

        internal class DebuggerCommand
        {
            private string command;
            private bool executedByDebugger;
            private bool repeatOnEnter;
            private DebuggerResumeAction? resumeAction;

            public DebuggerCommand(string command, DebuggerResumeAction? action, bool repeatOnEnter, bool executedByDebugger)
            {
                this.resumeAction = action;
                this.command = command;
                this.repeatOnEnter = repeatOnEnter;
                this.executedByDebugger = executedByDebugger;
            }

            public string Command
            {
                get
                {
                    return this.command;
                }
            }

            public bool ExecutedByDebugger
            {
                get
                {
                    return this.executedByDebugger;
                }
            }

            public bool RepeatOnEnter
            {
                get
                {
                    return this.repeatOnEnter;
                }
            }

            public DebuggerResumeAction? ResumeAction
            {
                get
                {
                    return this.resumeAction;
                }
            }
        }

        internal class DebuggerCommandProcessor
        {
            private Dictionary<string, HostUtilities.DebuggerCommand> commandTable = new Dictionary<string, HostUtilities.DebuggerCommand>(StringComparer.OrdinalIgnoreCase);
            private const string ContinueCommand = "continue";
            private const string ContinueShortcut = "c";
            private const int DefaultListLineCount = 0x10;
            private const string GetStackTraceShortcut = "k";
            private HostUtilities.DebuggerCommand helpCommand;
            private const string HelpCommand = "h";
            private const string HelpShortcut = "?";
            private HostUtilities.DebuggerCommand lastCommand;
            private int lastLineDisplayed;
            private string[] lines;
            private HostUtilities.DebuggerCommand listCommand;
            private const string ListCommand = "list";
            private const string ListShortcut = "l";
            private const string StepCommand = "stepInto";
            private const string StepOutCommand = "stepOut";
            private const string StepOutShortcut = "o";
            private const string StepOverCommand = "stepOver";
            private const string StepOverShortcut = "v";
            private const string StepShortcut = "s";
            private const string StopCommand = "quit";
            private const string StopShortcut = "q";

            public DebuggerCommandProcessor()
            {
                this.commandTable["stepInto"] = this.commandTable["s"] = new HostUtilities.DebuggerCommand("stepInto", (DebuggerResumeAction)1, true, false);
                this.commandTable["stepOut"] = this.commandTable["o"] = new HostUtilities.DebuggerCommand("stepOut", (DebuggerResumeAction)2, false, false);
                this.commandTable["stepOver"] = this.commandTable["v"] = new HostUtilities.DebuggerCommand("stepOver", (DebuggerResumeAction)3, true, false);
                this.commandTable["continue"] = this.commandTable["c"] = new HostUtilities.DebuggerCommand("continue", (DebuggerResumeAction)0, false, false);
                this.commandTable["quit"] = this.commandTable["q"] = new HostUtilities.DebuggerCommand("quit", (DebuggerResumeAction)4, false, false);
                this.commandTable["k"] = new HostUtilities.DebuggerCommand("get-pscallstack", null, false, false);
                this.commandTable["h"] = this.commandTable["?"] = this.helpCommand = new HostUtilities.DebuggerCommand("h", null, false, true);
                this.commandTable["list"] = this.commandTable["l"] = this.listCommand = new HostUtilities.DebuggerCommand("list", null, true, true);
                this.commandTable[string.Empty] = new HostUtilities.DebuggerCommand(string.Empty, null, false, true);
            }

            private void DisplayHelp(PSHost host)
            {
                host.UI.WriteLine("");
                host.UI.WriteLine(StringUtil.Format(DebuggerStrings.StepHelp, "s", "stepInto"));
                host.UI.WriteLine(StringUtil.Format(DebuggerStrings.StepOverHelp, "v", "stepOver"));
                host.UI.WriteLine(StringUtil.Format(DebuggerStrings.StepOutHelp, "o", "stepOut"));
                host.UI.WriteLine("");
                host.UI.WriteLine(StringUtil.Format(DebuggerStrings.ContinueHelp, "c", "continue"));
                host.UI.WriteLine(StringUtil.Format(DebuggerStrings.StopHelp, "q", "quit"));
                host.UI.WriteLine("");
                host.UI.WriteLine(StringUtil.Format(DebuggerStrings.GetStackTraceHelp, "k"));
                host.UI.WriteLine("");
                host.UI.WriteLine(StringUtil.Format(DebuggerStrings.ListHelp, "l", "list"));
                host.UI.WriteLine(StringUtil.Format(DebuggerStrings.AdditionalListHelp1, new object[0]));
                host.UI.WriteLine(StringUtil.Format(DebuggerStrings.AdditionalListHelp2, new object[0]));
                host.UI.WriteLine(StringUtil.Format(DebuggerStrings.AdditionalListHelp3, new object[0]));
                host.UI.WriteLine("");
                host.UI.WriteLine(StringUtil.Format(DebuggerStrings.EnterHelp, new object[] { "stepInto", "stepOver", "list" }));
                host.UI.WriteLine("");
                host.UI.WriteLine(StringUtil.Format(DebuggerStrings.HelpCommandHelp, "?", "h"));
                host.UI.WriteLine("\n");
                host.UI.WriteLine(StringUtil.Format(DebuggerStrings.PromptHelp, new object[0]));
                host.UI.WriteLine("");
            }

            private void DisplayScript(PSHost host, InvocationInfo invocationInfo, Match match)
            {
                if (this.lines == null)
                {
                    string fullScript = invocationInfo.GetFullScript();
                    if (string.IsNullOrEmpty(fullScript))
                    {
                        host.UI.WriteErrorLine(StringUtil.Format(DebuggerStrings.NoSourceCode, new object[0]));
                        return;
                    }
                    this.lines = fullScript.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
                }
                int start = Math.Max(invocationInfo.ScriptLineNumber - 5, 1);
                if (match.Groups["start"].Value.Length > 0)
                {
                    try
                    {
                        start = int.Parse(match.Groups["start"].Value, CultureInfo.CurrentCulture.NumberFormat);
                    }
                    catch
                    {
                        host.UI.WriteErrorLine(StringUtil.Format(DebuggerStrings.BadStartFormat, this.lines.Length));
                        return;
                    }
                    if ((start <= 0) || (start > this.lines.Length))
                    {
                        host.UI.WriteErrorLine(StringUtil.Format(DebuggerStrings.BadStartFormat, this.lines.Length));
                        return;
                    }
                }
                int count = 0x10;
                if (match.Groups["count"].Value.Length > 0)
                {
                    try
                    {
                        count = int.Parse(match.Groups["count"].Value, CultureInfo.CurrentCulture.NumberFormat);
                    }
                    catch
                    {
                        host.UI.WriteErrorLine(StringUtil.Format(DebuggerStrings.BadCountFormat, this.lines.Length));
                        return;
                    }
                    if ((count <= 0) || (count > this.lines.Length))
                    {
                        host.UI.WriteErrorLine(StringUtil.Format(DebuggerStrings.BadCountFormat, this.lines.Length));
                        return;
                    }
                }
                this.DisplayScript(host, invocationInfo, start, count);
            }

            private void DisplayScript(PSHost host, InvocationInfo invocationInfo, int start, int count)
            {
                host.UI.WriteLine();
                for (int i = start; (i <= this.lines.Length) && (i < (start + count)); i++)
                {
                    host.UI.WriteLine((i == invocationInfo.ScriptLineNumber) ? string.Format(CultureInfo.CurrentCulture, "{0,5}:* {1}", new object[] { i, this.lines[i - 1] }) : string.Format(CultureInfo.CurrentCulture, "{0,5}:  {1}", new object[] { i, this.lines[i - 1] }));
                    this.lastLineDisplayed = i;
                }
                host.UI.WriteLine();
            }

            private HostUtilities.DebuggerCommand DoProcessCommand(PSHost host, string command, InvocationInfo invocationInfo)
            {
                if (((command.Length == 0) && (this.lastCommand != null)) && this.lastCommand.RepeatOnEnter)
                {
                    if (this.lastCommand == this.listCommand)
                    {
                        if (this.lastLineDisplayed < this.lines.Length)
                        {
                            this.DisplayScript(host, invocationInfo, this.lastLineDisplayed + 1, 0x10);
                        }
                        return this.listCommand;
                    }
                    command = this.lastCommand.Command;
                }
                Match match = new Regex(@"^l(ist)?(\s+(?<start>\S+))?(\s+(?<count>\S+))?$", RegexOptions.IgnoreCase).Match(command);
                if (match.Success)
                {
                    this.DisplayScript(host, invocationInfo, match);
                    return this.listCommand;
                }
                HostUtilities.DebuggerCommand command2 = null;
                if (!this.commandTable.TryGetValue(command, out command2))
                {
                    return new HostUtilities.DebuggerCommand(command, null, false, false);
                }
                if (command2 == this.helpCommand)
                {
                    this.DisplayHelp(host);
                }
                return command2;
            }

            public HostUtilities.DebuggerCommand ProcessCommand(PSHost host, string command, InvocationInfo invocationInfo)
            {
                return (this.lastCommand = this.DoProcessCommand(host, command, invocationInfo));
            }

            public void Reset()
            {
                this.lines = null;
            }
        }
    }
}

