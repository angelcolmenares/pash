namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Eventing.Reader;
    using System.Globalization;
    using System.IO;
    using System.Management.Automation;
    using System.Net;
    using System.Reflection;
    using System.Resources;
    using System.Security.Principal;
    using System.Xml;

    [Cmdlet("Get", "WinEvent", DefaultParameterSetName="GetLogSet", HelpUri="http://go.microsoft.com/fwlink/?LinkID=138336")]
    public sealed class GetWinEventCommand : PSCmdlet
    {
        private List<string> _accumulatedFileNames = new List<string>();
        private List<string> _accumulatedLogNames = new List<string>();
        private List<string> _accumulatedProviderNames = new List<string>();
        private string _computerName = string.Empty;
        private PSCredential _credential = PSCredential.Empty;
        private string _filter = "*";
        private SwitchParameter _force;
        private string[] _listLog = new string[] { "*" };
        private string[] _listProvider = new string[] { "*" };
        private string[] _logName = new string[] { "*" };
        private StringCollection _logNamesMatchingWildcard;
        private long _maxEvents = -1L;
        private bool _oldest;
        private string[] _path;
        private string[] _providerName;
        private Dictionary<string, StringCollection> _providersByLogMap = new Dictionary<string, StringCollection>();
        private StringCollection _resolvedPaths = new StringCollection();
        private ResourceManager _resourceMgr;
        private Hashtable[] _selector;
        private static Assembly _systemCoreAssembly;
        private XmlDocument _xmlQuery;
        private const string filePrefix = "file://";
        private const string hashkey_data_lc = "data";
        private const string hashkey_endtime_lc = "endtime";
        private const string hashkey_id_lc = "id";
        private const string hashkey_keywords_lc = "keywords";
        private const string hashkey_level_lc = "level";
        private const string hashkey_logname_lc = "logname";
        private const string hashkey_path_lc = "path";
        private const string hashkey_providername_lc = "providername";
        private const string hashkey_starttime_lc = "starttime";
        private const string hashkey_userid_lc = "userid";
        private const long MAX_EVENT_BATCH = 100;
        private const string propClose = "]";
        private const string propOpen = "[";
        private const string queryCloser = "</Select></Query>";
        private const string queryListClose = "</QueryList>";
        private const string queryListOpen = "<QueryList>";
        private const string queryOpenerTemplate = "<Query Id=\"{0}\" Path=\"{1}\"><Select Path=\"{1}\">*";
        private const string queryTemplate = "<Query Id=\"{0}\" Path=\"{1}\"><Select Path=\"{1}\">{2}</Select></Query>";

        private void AccumulatePipelineFileNames()
        {
            this._accumulatedFileNames.AddRange(this._logName);
        }

        private void AccumulatePipelineLogNames()
        {
            this._accumulatedLogNames.AddRange(this._logName);
        }

        private void AccumulatePipelineProviderNames()
        {
            this._accumulatedProviderNames.AddRange(this._logName);
        }

        private void AddLogsForProviderToInternalMap(EventLogSession eventLogSession, string providerName)
        {
            try
            {
                ProviderMetadata metadata = new ProviderMetadata(providerName, eventLogSession, CultureInfo.CurrentCulture);
                foreach (EventLogLink link in metadata.LogLinks)
                {
                    if (!this._providersByLogMap.ContainsKey(link.LogName.ToLower(CultureInfo.InvariantCulture)))
                    {
                        EventLogConfiguration configuration = new EventLogConfiguration(link.LogName, eventLogSession);
                        if ((configuration.LogType == EventLogType.Debug) || (configuration.LogType == EventLogType.Analytical))
                        {
                            if (!this.Force.IsPresent)
                            {
                                continue;
                            }
                            this.ValidateLogName(link.LogName, eventLogSession);
                        }
                        base.WriteVerbose(string.Format(CultureInfo.InvariantCulture, this._resourceMgr.GetString("ProviderLogLink"), new object[] { providerName, link.LogName }));
                        StringCollection strings = new StringCollection();
                        strings.Add(providerName.ToLower(CultureInfo.InvariantCulture));
                        this._providersByLogMap.Add(link.LogName.ToLower(CultureInfo.InvariantCulture), strings);
                    }
                    else
                    {
                        StringCollection strings2 = this._providersByLogMap[link.LogName.ToLower(CultureInfo.InvariantCulture)];
                        if (!strings2.Contains(providerName.ToLower(CultureInfo.InvariantCulture)))
                        {
                            base.WriteVerbose(string.Format(CultureInfo.InvariantCulture, this._resourceMgr.GetString("ProviderLogLink"), new object[] { providerName, link.LogName }));
                            strings2.Add(providerName.ToLower(CultureInfo.InvariantCulture));
                        }
                    }
                }
            }
            catch (EventLogException exception)
            {
                Exception exception2 = new Exception(string.Format(CultureInfo.InvariantCulture, this._resourceMgr.GetString("ProviderMetadataUnavailable"), new object[] { providerName, exception.Message }), exception);
                base.WriteError(new ErrorRecord(exception2, "ProviderMetadataUnavailable", ErrorCategory.NotSpecified, null));
            }
        }

        private string AddProviderPredicatesToFilter(StringCollection providers)
        {
            if (providers.Count == 0)
            {
                return this._filter;
            }
            string str = this._filter;
            string str2 = this.BuildProvidersPredicate(providers);
            if (this._filter.Equals("*", StringComparison.OrdinalIgnoreCase))
            {
                return (str + "[" + str2 + "]");
            }
            int startIndex = this._filter.LastIndexOf(']');
            if (startIndex == -1)
            {
                return (str + "[" + str2 + "]");
            }
            return str.Insert(startIndex, " and " + str2);
        }

        protected override void BeginProcessing()
        {
            this._resourceMgr = new ResourceManager("GetEventResources", Assembly.GetExecutingAssembly());
            if (Environment.OSVersion.Version.Major < 6)
            {
                Exception exception = new Exception(this._resourceMgr.GetString("GetEventVistaPlusRequired"));
                base.ThrowTerminatingError(new ErrorRecord(exception, "GetEventVistaPlusRequired", ErrorCategory.NotImplemented, null));
            }
            try
            {
                AssemblyName assemblyRef = new AssemblyName {
                    Name = "System.Core",
                    Version = new Version("3.5.0.0"),
                    CultureInfo = new CultureInfo("")
                };
                assemblyRef.SetPublicKeyToken(new byte[] { 0xb7, 0x7a, 0x5c, 0x56, 0x19, 0x34, 0xe0, 0x89 });
                _systemCoreAssembly = Assembly.Load(assemblyRef);
            }
            catch (FileNotFoundException exception2)
            {
                FileNotFoundException exception3 = new FileNotFoundException(this._resourceMgr.GetString("GetEventDotNet35Required"), exception2);
                base.ThrowTerminatingError(new ErrorRecord(exception3, "GetEventDotNet35Required", ErrorCategory.NotInstalled, null));
            }
        }

        private string BuildAllProvidersPredicate()
        {
            if (this._providersByLogMap.Count == 0)
            {
                return "";
            }
            string str = "System/Provider[";
            List<string> list = new List<string>();
            foreach (string str2 in this._providersByLogMap.Keys)
            {
                for (int j = 0; j < this._providersByLogMap[str2].Count; j++)
                {
                    string item = this._providersByLogMap[str2][j].ToLower(CultureInfo.InvariantCulture);
                    if (!list.Contains(item))
                    {
                        list.Add(item);
                    }
                }
            }
            for (int i = 0; i < list.Count; i++)
            {
                str = str + "@Name='" + list[i] + "'";
                if (i < (list.Count - 1))
                {
                    str = str + " or ";
                }
            }
            return (str + "]");
        }

        private string BuildProvidersPredicate(StringCollection providers)
        {
            if (providers.Count == 0)
            {
                return "";
            }
            string str = "System/Provider[";
            for (int i = 0; i < providers.Count; i++)
            {
                str = str + "@Name='" + providers[i] + "'";
                if (i < (providers.Count - 1))
                {
                    str = str + " or ";
                }
            }
            return (str + "]");
        }

        private string BuildStructuredQuery(EventLogSession eventLogSession)
        {
            string str = "";
            switch (base.ParameterSetName)
            {
                case "ListLogSet":
                case "ListProviderSet":
                    break;

                case "GetProviderSet":
                {
                    str = "<QueryList>";
                    long num = 0;
                    foreach (string str2 in this._providersByLogMap.Keys)
                    {
                        string str3 = this.AddProviderPredicatesToFilter(this._providersByLogMap[str2]);
                        string str4 = string.Format(CultureInfo.InvariantCulture, "<Query Id=\"{0}\" Path=\"{1}\"><Select Path=\"{1}\">{2}</Select></Query>", new object[] { num++, str2, str3 });
                        str = str + str4;
                    }
                    str = str + "</QueryList>";
                    break;
                }
                case "GetLogSet":
                {
                    str = "<QueryList>";
                    long num2 = 0;
                    foreach (string str5 in this._logNamesMatchingWildcard)
                    {
                        string str6 = string.Format(CultureInfo.InvariantCulture, "<Query Id=\"{0}\" Path=\"{1}\"><Select Path=\"{1}\">{2}</Select></Query>", new object[] { num2++, str5, this._filter });
                        str = str + str6;
                    }
                    str = str + "</QueryList>";
                    break;
                }
                case "FileSet":
                {
                    str = "<QueryList>";
                    long num3 = 0;
                    foreach (string str7 in this._resolvedPaths)
                    {
                        string str8 = "file://" + str7;
                        string str9 = string.Format(CultureInfo.InvariantCulture, "<Query Id=\"{0}\" Path=\"{1}\"><Select Path=\"{1}\">{2}</Select></Query>", new object[] { num3++, str8, this._filter });
                        str = str + str9;
                    }
                    str = str + "</QueryList>";
                    break;
                }
                case "HashQuerySet":
                    str = this.BuildStructuredQueryFromHashTable(eventLogSession);
                    break;

                default:
                    base.WriteDebug(string.Format(CultureInfo.InvariantCulture, "Invalid parameter set name: {0}", new object[] { base.ParameterSetName }));
                    break;
            }
            base.WriteVerbose(string.Format(CultureInfo.InvariantCulture, this._resourceMgr.GetString("QueryTrace"), new object[] { str }));
            return str;
        }

        private string BuildStructuredQueryFromHashTable(EventLogSession eventLogSession)
        {
            string str = "";
            str = "<QueryList>";
            long num = 0;
            foreach (Hashtable hashtable in this._selector)
            {
                string xpathString = "";
                this.CheckHashTableForQueryPathPresence(hashtable);
                Dictionary<string, string> dictionary = new Dictionary<string, string>();
                if (hashtable.ContainsKey("logname"))
                {
                    List<string> logPatterns = new List<string>();
                    if (hashtable["logname"] is Array)
                    {
                        foreach (object obj2 in (Array) hashtable["logname"])
                        {
                            logPatterns.Add(obj2.ToString());
                        }
                    }
                    else
                    {
                        logPatterns.Add(hashtable["logname"].ToString());
                    }
                    this.FindLogNamesMatchingWildcards(eventLogSession, logPatterns);
                    foreach (string str3 in this._logNamesMatchingWildcard)
                    {
                        dictionary.Add(str3.ToLower(CultureInfo.InvariantCulture), string.Format(CultureInfo.InvariantCulture, "<Query Id=\"{0}\" Path=\"{1}\"><Select Path=\"{1}\">*", new object[] { num++, str3 }));
                    }
                }
                if (hashtable.ContainsKey("path"))
                {
                    if (hashtable["path"] is Array)
                    {
                        foreach (object obj3 in (Array) hashtable["path"])
                        {
                            foreach (string str4 in this.ValidateAndResolveFilePath(obj3.ToString()))
                            {
                                dictionary.Add("file://" + str4.ToLower(CultureInfo.InvariantCulture), string.Format(CultureInfo.InvariantCulture, "<Query Id=\"{0}\" Path=\"{1}\"><Select Path=\"{1}\">*", new object[] { num++, "file://" + str4 }));
                            }
                        }
                    }
                    else
                    {
                        foreach (string str5 in this.ValidateAndResolveFilePath(hashtable["path"].ToString()))
                        {
                            dictionary.Add("file://" + str5.ToLower(CultureInfo.InvariantCulture), string.Format(CultureInfo.InvariantCulture, "<Query Id=\"{0}\" Path=\"{1}\"><Select Path=\"{1}\">*", new object[] { num++, "file://" + str5 }));
                        }
                    }
                }
                if (hashtable.ContainsKey("providername"))
                {
                    List<string> providerPatterns = new List<string>();
                    if (hashtable["providername"] is Array)
                    {
                        foreach (object obj4 in (Array) hashtable["providername"])
                        {
                            providerPatterns.Add(obj4.ToString());
                        }
                    }
                    else
                    {
                        providerPatterns.Add(hashtable["providername"].ToString());
                    }
                    this.FindProvidersByLogForWildcardPatterns(eventLogSession, providerPatterns);
                    if (!hashtable.ContainsKey("path") && !hashtable.ContainsKey("logname"))
                    {
                        foreach (string str6 in this._providersByLogMap.Keys)
                        {
                            dictionary.Add(str6.ToLower(CultureInfo.InvariantCulture), string.Format(CultureInfo.InvariantCulture, "<Query Id=\"{0}\" Path=\"{1}\"><Select Path=\"{1}\">*", new object[] { num++, str6 }) + "[" + this.BuildProvidersPredicate(this._providersByLogMap[str6]));
                        }
                    }
                    else
                    {
                        List<string> list3 = new List<string>(dictionary.Keys);
                        bool flag = false;
                        foreach (string str9 in list3)
                        {
                            if (str9.StartsWith("file://", StringComparison.Ordinal))
                            {
								Dictionary<string, string> dictionary2 = dictionary;
								string str15= str9;
                                dictionary2[str15] = dictionary2[str15] + "[" + this.BuildAllProvidersPredicate();
                            }
                            else if (this._providersByLogMap.ContainsKey(str9))
                            {
								Dictionary<string, string> dictionary3 = dictionary;
								string str16 = str9;
                                string str10 = this.BuildProvidersPredicate(this._providersByLogMap[str9]);
                                dictionary3[str16] = dictionary3[str16] + "[" + str10;
                            }
                            else
                            {
                                base.WriteVerbose(string.Format(CultureInfo.InvariantCulture, this._resourceMgr.GetString("SpecifiedProvidersDontWriteToLog"), new object[] { str9 }));
                                dictionary.Remove(str9);
                                flag = true;
                            }
                        }
                        if (flag && (dictionary.Count == 0))
                        {
                            Exception exception = new Exception(string.Format(CultureInfo.InvariantCulture, this._resourceMgr.GetString("LogsAndProvidersDontOverlap"), new object[0]));
                            base.WriteError(new ErrorRecord(exception, "LogsAndProvidersDontOverlap", ErrorCategory.InvalidArgument, null));
                            continue;
                        }
                    }
                }
                if (dictionary.Count != 0)
                {
                    bool flag2 = false;
                    foreach (string str12 in hashtable.Keys)
                    {
                        string str13 = "";
                        switch (str12.ToLower(CultureInfo.InvariantCulture))
                        {
                            case "logname":
                            case "path":
                            case "providername":
                                break;

                            case "id":
                                str13 = this.HandleEventIdHashValue(hashtable[str12]);
                                if (str13.Length > 0)
                                {
                                    this.ExtendPredicate(ref xpathString);
                                    xpathString = xpathString + str13;
                                }
                                break;

                            case "level":
                                str13 = this.HandleLevelHashValue(hashtable[str12]);
                                if (str13.Length > 0)
                                {
                                    this.ExtendPredicate(ref xpathString);
                                    xpathString = xpathString + str13;
                                }
                                break;

                            case "keywords":
                                str13 = this.HandleKeywordHashValue(hashtable[str12]);
                                if (str13.Length > 0)
                                {
                                    this.ExtendPredicate(ref xpathString);
                                    xpathString = xpathString + str13;
                                }
                                break;

                            case "starttime":
                                if (!flag2)
                                {
                                    str13 = this.HandleStartTimeHashValue(hashtable[str12], hashtable);
                                    if (str13.Length > 0)
                                    {
                                        this.ExtendPredicate(ref xpathString);
                                        xpathString = xpathString + str13;
                                    }
                                    flag2 = true;
                                }
                                break;

                            case "endtime":
                                if (!flag2)
                                {
                                    str13 = this.HandleEndTimeHashValue(hashtable[str12], hashtable);
                                    if (str13.Length > 0)
                                    {
                                        this.ExtendPredicate(ref xpathString);
                                        xpathString = xpathString + str13;
                                    }
                                    flag2 = true;
                                }
                                break;

                            case "data":
                                str13 = this.HandleDataHashValue(hashtable[str12]);
                                if (str13.Length > 0)
                                {
                                    this.ExtendPredicate(ref xpathString);
                                    xpathString = xpathString + str13;
                                }
                                break;

                            case "userid":
                                str13 = this.HandleContextHashValue(hashtable[str12]);
                                if (str13.Length > 0)
                                {
                                    this.ExtendPredicate(ref xpathString);
                                    xpathString = xpathString + str13;
                                }
                                break;

                            default:
                                this.ExtendPredicate(ref xpathString);
                                xpathString = xpathString + string.Format(CultureInfo.InvariantCulture, "([EventData[Data[@Name='{0}']='{1}']] or [UserData/*/{0}='{1}'])", new object[] { str12, hashtable[str12] });
                                break;
                        }
                    }
                    foreach (string str14 in dictionary.Values)
                    {
                        str = str + str14;
                        if (str14.EndsWith("*", StringComparison.OrdinalIgnoreCase))
                        {
                            if (xpathString.Length != 0)
                            {
                                str = str + "[" + xpathString + "]";
                            }
                        }
                        else
                        {
                            if (xpathString.Length != 0)
                            {
                                str = str + " and " + xpathString;
                            }
                            str = str + "]";
                        }
                        str = str + "</Select></Query>";
                    }
                }
            }
            return (str + "</QueryList>");
        }

        private void CheckHashTableForQueryPathPresence(Hashtable hash)
        {
            bool flag = hash.ContainsKey("logname");
            bool flag2 = hash.ContainsKey("path");
            bool flag3 = hash.ContainsKey("providername");
            if ((!flag && !flag3) && !flag2)
            {
                Exception exception = new Exception(this._resourceMgr.GetString("LogProviderOrPathNeeded"));
                base.ThrowTerminatingError(new ErrorRecord(exception, "LogProviderOrPathNeeded", ErrorCategory.InvalidArgument, null));
            }
        }

        private void CheckHashTablesForNullValues()
        {
            foreach (Hashtable hashtable in this._selector)
            {
                foreach (string str in hashtable.Keys)
                {
                    object obj2 = hashtable[str];
                    if (obj2 == null)
                    {
                        string format = this._resourceMgr.GetString("NullNotAllowedInHashtable");
                        Exception exception = new Exception(string.Format(CultureInfo.InvariantCulture, format, new object[] { str }));
                        base.ThrowTerminatingError(new ErrorRecord(exception, "NullNotAllowedInHashtable", ErrorCategory.InvalidArgument, str));
                    }
                    else if (obj2 is Array)
                    {
                        IEnumerator enumerator2 = ((Array)obj2).GetEnumerator();
                        {
                            while (enumerator2.MoveNext())
                            {
                                if (enumerator2.Current == null)
                                {
                                    string str3 = this._resourceMgr.GetString("NullNotAllowedInHashtable");
                                    Exception exception2 = new Exception(string.Format(CultureInfo.InvariantCulture, str3, new object[] { str }));
                                    base.ThrowTerminatingError(new ErrorRecord(exception2, "NullNotAllowedInHashtable", ErrorCategory.InvalidArgument, str));
                                }
                            }
                        }
                    }
                }
            }
        }

        private EventLogSession CreateSession()
        {
            EventLogSession session = null;
            if (this._computerName == string.Empty)
            {
                this._computerName = "localhost";
                if (this._credential == PSCredential.Empty)
                {
                    return new EventLogSession();
                }
            }
            else if (this._credential == PSCredential.Empty)
            {
                return new EventLogSession(this._computerName);
            }
            NetworkCredential credential = (NetworkCredential) this._credential;
            session = new EventLogSession(this._computerName, credential.Domain, credential.UserName, this._credential.Password, SessionAuthentication.Default);
            credential.Password = "";
            return session;
        }

        protected override void EndProcessing()
        {
            string parameterSetName = base.ParameterSetName;
            if (parameterSetName != null)
            {
                if (!(parameterSetName == "GetLogSet"))
                {
                    if (!(parameterSetName == "FileSet"))
                    {
                        if (parameterSetName == "GetProviderSet")
                        {
                            this.ProcessGetProvider();
                        }
                        return;
                    }
                }
                else
                {
                    this.ProcessGetLog();
                    return;
                }
                this.ProcessFile();
            }
        }

        private void ExtendPredicate(ref string xpathString)
        {
            if (xpathString.Length != 0)
            {
                xpathString = xpathString + " and ";
            }
        }

        private void FindLogNamesMatchingWildcards(EventLogSession eventLogSession, IEnumerable<string> logPatterns)
        {
            if (this._logNamesMatchingWildcard == null)
            {
                this._logNamesMatchingWildcard = new StringCollection();
            }
            else
            {
                this._logNamesMatchingWildcard.Clear();
            }
            foreach (string str in logPatterns)
            {
                bool flag = false;
                foreach (string str2 in eventLogSession.GetLogNames())
                {
                    WildcardPattern pattern = new WildcardPattern(str, WildcardOptions.IgnoreCase);
                    if ((!WildcardPattern.ContainsWildcardCharacters(str) && str.Equals(str2, StringComparison.CurrentCultureIgnoreCase)) || pattern.IsMatch(str2))
                    {
                        EventLogConfiguration configuration;
                        try
                        {
                            configuration = new EventLogConfiguration(str2, eventLogSession);
                        }
                        catch (Exception exception)
                        {
                            Exception exception2 = new Exception(string.Format(CultureInfo.InvariantCulture, this._resourceMgr.GetString("LogInfoUnavailable"), new object[] { str2, exception.Message }), exception);
                            base.WriteError(new ErrorRecord(exception2, "LogInfoUnavailable", ErrorCategory.NotSpecified, null));
                            continue;
                        }
                        if ((configuration.LogType == EventLogType.Debug) || (configuration.LogType == EventLogType.Analytical))
                        {
                            if (WildcardPattern.ContainsWildcardCharacters(str) && !this.Force.IsPresent)
                            {
                                continue;
                            }
                            this.ValidateLogName(str2, eventLogSession);
                        }
                        if (!this._logNamesMatchingWildcard.Contains(str2.ToLower(CultureInfo.InvariantCulture)))
                        {
                            this._logNamesMatchingWildcard.Add(str2.ToLower(CultureInfo.InvariantCulture));
                        }
                        flag = true;
                    }
                }
                if (!flag)
                {
                    string format = this._resourceMgr.GetString("NoMatchingLogsFound");
                    Exception exception3 = new Exception(string.Format(CultureInfo.InvariantCulture, format, new object[] { this._computerName, str }));
                    base.WriteError(new ErrorRecord(exception3, "NoMatchingLogsFound", ErrorCategory.ObjectNotFound, str));
                }
            }
        }

        private void FindProvidersByLogForWildcardPatterns(EventLogSession eventLogSession, IEnumerable<string> providerPatterns)
        {
            this._providersByLogMap.Clear();
            foreach (string str in providerPatterns)
            {
                bool flag = false;
                foreach (string str2 in eventLogSession.GetProviderNames())
                {
                    WildcardPattern pattern = new WildcardPattern(str, WildcardOptions.IgnoreCase);
                    if ((!WildcardPattern.ContainsWildcardCharacters(str) && str.Equals(str2, StringComparison.CurrentCultureIgnoreCase)) || pattern.IsMatch(str2))
                    {
                        base.WriteVerbose(string.Format(CultureInfo.InvariantCulture, "Found matching provider: {0}", new object[] { str2 }));
                        this.AddLogsForProviderToInternalMap(eventLogSession, str2);
                        flag = true;
                    }
                }
                if (!flag)
                {
                    string format = this._resourceMgr.GetString("NoMatchingProvidersFound");
                    Exception exception = new Exception(string.Format(CultureInfo.InvariantCulture, format, new object[] { this._computerName, str }));
                    base.WriteError(new ErrorRecord(exception, "NoMatchingProvidersFound", ErrorCategory.ObjectNotFound, str));
                }
            }
        }

        private string HandleContextHashValue(object value)
        {
            SecurityIdentifier identifier = null;
            try
            {
                identifier = new SecurityIdentifier(value.ToString());
            }
            catch (ArgumentException)
            {
                base.WriteDebug(string.Format(CultureInfo.InvariantCulture, this._resourceMgr.GetString("InvalidSIDFormat"), new object[] { value }));
            }
            if (identifier == null)
            {
                try
                {
                    NTAccount account = new NTAccount(value.ToString());
                    identifier = (SecurityIdentifier) account.Translate(typeof(SecurityIdentifier));
                }
                catch (ArgumentException exception)
                {
                    Exception exception2 = new Exception(string.Format(CultureInfo.InvariantCulture, this._resourceMgr.GetString("InvalidContext"), new object[] { value.ToString() }), exception);
                    base.WriteError(new ErrorRecord(exception2, "InvalidContext", ErrorCategory.InvalidArgument, null));
                    return "";
                }
            }
            return string.Format(CultureInfo.InvariantCulture, "(System/Security[@UserID='{0}'])", new object[] { identifier.ToString() });
        }

        private string HandleDataHashValue(object value)
        {
            string str = "";
            if (value is Array)
            {
                Array array = (Array) value;
                str = str + "(";
                for (int i = 0; i < array.Length; i++)
                {
                    str = str + string.Format(CultureInfo.InvariantCulture, "(EventData/Data='{0}')", new object[] { array.GetValue(i).ToString() });
                    if (i < (array.Length - 1))
                    {
                        str = str + " or ";
                    }
                }
                return (str + ")");
            }
            return (str + string.Format(CultureInfo.InvariantCulture, "(EventData/Data='{0}')", new object[] { value }));
        }

        private string HandleEndTimeHashValue(object value, Hashtable hash)
        {
            string str = "";
            DateTime dt = new DateTime();
            if (!this.StringToDateTime(value.ToString(), ref dt))
            {
                return "";
            }
            dt = dt.ToUniversalTime();
            string str2 = dt.ToString("s", CultureInfo.InvariantCulture) + "." + dt.Millisecond.ToString("d3", CultureInfo.InvariantCulture) + "Z";
            if (hash.ContainsKey("starttime"))
            {
                DateTime time2 = new DateTime();
                if (!this.StringToDateTime(hash["starttime"].ToString(), ref time2))
                {
                    return "";
                }
                time2 = time2.ToUniversalTime();
                string str3 = time2.ToString("s", CultureInfo.InvariantCulture) + "." + time2.Millisecond.ToString("d3", CultureInfo.InvariantCulture) + "Z";
                return (str + string.Format(CultureInfo.InvariantCulture, "(System/TimeCreated[@SystemTime&gt;='{0}' and @SystemTime&lt;='{1}'])", new object[] { str3, str2 }));
            }
            return (str + string.Format(CultureInfo.InvariantCulture, "(System/TimeCreated[@SystemTime&lt;='{0}'])", new object[] { str2 }));
        }

        private string HandleEventIdHashValue(object value)
        {
            string str = "";
            if (value is Array)
            {
                Array array = (Array) value;
                str = str + "(";
                for (int i = 0; i < array.Length; i++)
                {
                    str = str + "(System/EventID=" + array.GetValue(i).ToString() + ")";
                    if (i < (array.Length - 1))
                    {
                        str = str + " or ";
                    }
                }
                return (str + ")");
            }
            object obj2 = str;
            return string.Concat(new object[] { obj2, "(System/EventID=", value, ")" });
        }

        private string HandleKeywordHashValue(object value)
        {
            long num = 0L;
            long keyLong = 0L;
            if (value is Array)
            {
                foreach (object obj2 in (Array) value)
                {
                    if (this.KeywordStringToInt64(obj2.ToString(), ref keyLong))
                    {
                        num |= keyLong;
                    }
                }
            }
            else
            {
                if (!this.KeywordStringToInt64(value.ToString(), ref keyLong))
                {
                    return "";
                }
                num |= keyLong;
            }
            return string.Format(CultureInfo.InvariantCulture, "System[band(Keywords,{0})]", new object[] { num });
        }

        private string HandleLevelHashValue(object value)
        {
            string str = "";
            if (value is Array)
            {
                Array array = (Array) value;
                str = str + "(";
                for (int i = 0; i < array.Length; i++)
                {
                    str = str + "(System/Level=" + array.GetValue(i).ToString() + ")";
                    if (i < (array.Length - 1))
                    {
                        str = str + " or ";
                    }
                }
                return (str + ")");
            }
            object obj2 = str;
            return string.Concat(new object[] { obj2, "(System/Level=", value, ")" });
        }

        private string HandleStartTimeHashValue(object value, Hashtable hash)
        {
            string str = "";
            DateTime dt = new DateTime();
            if (!this.StringToDateTime(value.ToString(), ref dt))
            {
                return "";
            }
            dt = dt.ToUniversalTime();
            string str2 = dt.ToString("s", CultureInfo.InvariantCulture) + "." + dt.Millisecond.ToString("d3", CultureInfo.InvariantCulture) + "Z";
            if (hash.ContainsKey("endtime"))
            {
                DateTime time2 = new DateTime();
                if (!this.StringToDateTime(hash["endtime"].ToString(), ref time2))
                {
                    return "";
                }
                time2 = time2.ToUniversalTime();
                string str3 = time2.ToString("s", CultureInfo.InvariantCulture) + "." + time2.Millisecond.ToString("d3", CultureInfo.InvariantCulture) + "Z";
                return (str + string.Format(CultureInfo.InvariantCulture, "(System/TimeCreated[@SystemTime&gt;='{0}' and @SystemTime&lt;='{1}'])", new object[] { str2, str3 }));
            }
            return (str + string.Format(CultureInfo.InvariantCulture, "(System/TimeCreated[@SystemTime&gt;='{0}'])", new object[] { str2 }));
        }

        private bool KeywordStringToInt64(string keyString, ref long keyLong)
        {
            try
            {
                keyLong = Convert.ToInt64(keyString, CultureInfo.InvariantCulture);
            }
            catch (Exception exception)
            {
                string format = this._resourceMgr.GetString("KeywordLongExpected");
                Exception exception2 = new Exception(string.Format(CultureInfo.InvariantCulture, format, new object[] { keyString }), exception);
                base.WriteError(new ErrorRecord(exception2, "KeywordLongExpected", ErrorCategory.InvalidArgument, null));
                return false;
            }
            return true;
        }

        private void ProcessFile()
        {
            EventLogSession eventLogSession = this.CreateSession();
            for (int i = 0; i < this._path.Length; i++)
            {
                foreach (string str in this.ValidateAndResolveFilePath(this._path[i]))
                {
                    this._resolvedPaths.Add(str);
                    base.WriteVerbose(string.Format(CultureInfo.InvariantCulture, "Found file {0}", new object[] { str }));
                }
            }
            EventLogQuery eventQuery = null;
            if (this._resolvedPaths.Count != 0)
            {
                if (this._resolvedPaths.Count > 1)
                {
                    string query = this.BuildStructuredQuery(eventLogSession);
                    eventQuery = new EventLogQuery(null, PathType.FilePath, query) {
                        TolerateQueryErrors = true
                    };
                }
                else
                {
                    eventQuery = new EventLogQuery(this._resolvedPaths[0], PathType.FilePath, this._filter);
                }
                eventQuery.Session = eventLogSession;
                eventQuery.ReverseDirection = !this._oldest;
                EventLogReader readerObj = new EventLogReader(eventQuery);
                if (readerObj != null)
                {
                    this.ReadEvents(readerObj);
                }
            }
        }

        private void ProcessFilterXml()
        {
            EventLogSession eventLogSession = this.CreateSession();
            if (!this.Oldest.IsPresent)
            {
                foreach (XmlNode node in this._xmlQuery.DocumentElement.SelectNodes("//Query//Select"))
                {
                    foreach (XmlAttribute attribute in node.Attributes)
                    {
                        if (attribute.Name.Equals("Path", StringComparison.OrdinalIgnoreCase))
                        {
                            string fileName = attribute.Value;
                            if (fileName.StartsWith("file://", StringComparison.OrdinalIgnoreCase))
                            {
                                this.TerminateForNonEvtxFileWithoutOldest(fileName);
                            }
                            this.ValidateLogName(fileName, eventLogSession);
                        }
                    }
                }
            }
            EventLogQuery eventQuery = new EventLogQuery(null, PathType.LogName, this._xmlQuery.InnerXml) {
                Session = eventLogSession,
                ReverseDirection = !this._oldest
            };
            EventLogReader readerObj = new EventLogReader(eventQuery);
            if (readerObj != null)
            {
                this.ReadEvents(readerObj);
            }
        }

        private void ProcessGetLog()
        {
            EventLogSession eventLogSession = this.CreateSession();
            this.FindLogNamesMatchingWildcards(eventLogSession, this._accumulatedLogNames);
            if (this._logNamesMatchingWildcard.Count != 0)
            {
                EventLogQuery query;
                if (this._logNamesMatchingWildcard.Count > 1)
                {
                    string str = this.BuildStructuredQuery(eventLogSession);
                    query = new EventLogQuery(null, PathType.LogName, str) {
                        TolerateQueryErrors = true
                    };
                }
                else
                {
                    query = new EventLogQuery(this._logNamesMatchingWildcard[0], PathType.LogName, this._filter);
                }
                query.Session = eventLogSession;
                query.ReverseDirection = !this._oldest;
                EventLogReader readerObj = new EventLogReader(query);
                if (readerObj != null)
                {
                    this.ReadEvents(readerObj);
                }
            }
        }

        private void ProcessGetProvider()
        {
            EventLogSession eventLogSession = this.CreateSession();
            this.FindProvidersByLogForWildcardPatterns(eventLogSession, this._providerName);
            if (this._providersByLogMap.Count != 0)
            {
                EventLogQuery eventQuery = null;
                if (this._providersByLogMap.Count > 1)
                {
                    string query = this.BuildStructuredQuery(eventLogSession);
                    eventQuery = new EventLogQuery(null, PathType.LogName, query) {
                        TolerateQueryErrors = true
                    };
                }
                else
                {
                    foreach (string str2 in this._providersByLogMap.Keys)
                    {
                        eventQuery = new EventLogQuery(str2, PathType.LogName, this.AddProviderPredicatesToFilter(this._providersByLogMap[str2]));
                        base.WriteVerbose(string.Format(CultureInfo.InvariantCulture, "Log {0} will be queried", new object[] { str2 }));
                    }
                }
                eventQuery.Session = eventLogSession;
                eventQuery.ReverseDirection = !this._oldest;
                EventLogReader readerObj = new EventLogReader(eventQuery);
                if (readerObj != null)
                {
                    this.ReadEvents(readerObj);
                }
            }
        }

        private void ProcessHashQuery()
        {
            this.CheckHashTablesForNullValues();
            EventLogSession eventLogSession = this.CreateSession();
            string str = this.BuildStructuredQuery(eventLogSession);
            if (str.Length != 0)
            {
                EventLogQuery eventQuery = new EventLogQuery(null, PathType.FilePath, str) {
                    Session = eventLogSession,
                    TolerateQueryErrors = true,
                    ReverseDirection = !this._oldest
                };
                EventLogReader readerObj = new EventLogReader(eventQuery);
                if (readerObj != null)
                {
                    this.ReadEvents(readerObj);
                }
            }
        }

        private void ProcessListLog()
        {
            EventLogSession session = this.CreateSession();
            foreach (string str in this._listLog)
            {
                bool flag = false;
                foreach (string str2 in session.GetLogNames())
                {
                    WildcardPattern pattern = new WildcardPattern(str, WildcardOptions.IgnoreCase);
                    if ((!WildcardPattern.ContainsWildcardCharacters(str) && (str.ToLower(CultureInfo.CurrentCulture) == str2.ToLower(CultureInfo.CurrentCulture))) || pattern.IsMatch(str2))
                    {
                        try
                        {
                            EventLogConfiguration configuration = new EventLogConfiguration(str2, session);
                            if ((this.Force.IsPresent || !WildcardPattern.ContainsWildcardCharacters(str)) || ((configuration.LogType != EventLogType.Debug) && (configuration.LogType != EventLogType.Analytical)))
                            {
                                EventLogInformation logInformation = session.GetLogInformation(str2, PathType.LogName);
                                PSObject sendToPipeline = new PSObject(configuration);
                                sendToPipeline.Properties.Add(new PSNoteProperty("FileSize", logInformation.FileSize));
                                sendToPipeline.Properties.Add(new PSNoteProperty("IsLogFull", logInformation.IsLogFull));
                                sendToPipeline.Properties.Add(new PSNoteProperty("LastAccessTime", logInformation.LastAccessTime));
                                sendToPipeline.Properties.Add(new PSNoteProperty("LastWriteTime", logInformation.LastWriteTime));
                                sendToPipeline.Properties.Add(new PSNoteProperty("OldestRecordNumber", logInformation.OldestRecordNumber));
                                sendToPipeline.Properties.Add(new PSNoteProperty("RecordCount", logInformation.RecordCount));
                                base.WriteObject(sendToPipeline);
                                flag = true;
                            }
                        }
                        catch (Exception exception)
                        {
                            Exception exception2 = new Exception(string.Format(CultureInfo.InvariantCulture, this._resourceMgr.GetString("LogInfoUnavailable"), new object[] { str2, exception.Message }), exception);
                            base.WriteError(new ErrorRecord(exception2, "LogInfoUnavailable", ErrorCategory.NotSpecified, null));
                        }
                    }
                }
                if (!flag)
                {
                    string format = this._resourceMgr.GetString("NoMatchingLogsFound");
                    Exception exception3 = new Exception(string.Format(CultureInfo.InvariantCulture, format, new object[] { this._computerName, str }));
                    base.WriteError(new ErrorRecord(exception3, "NoMatchingLogsFound", ErrorCategory.ObjectNotFound, null));
                }
            }
        }

        private void ProcessListProvider()
        {
            EventLogSession session = this.CreateSession();
            foreach (string str in this._listProvider)
            {
                bool flag = false;
                foreach (string str2 in session.GetProviderNames())
                {
                    WildcardPattern pattern = new WildcardPattern(str, WildcardOptions.IgnoreCase);
                    if ((!WildcardPattern.ContainsWildcardCharacters(str) && (str.ToLower(CultureInfo.CurrentCulture) == str2.ToLower(CultureInfo.CurrentCulture))) || pattern.IsMatch(str2))
                    {
                        try
                        {
                            ProviderMetadata sendToPipeline = new ProviderMetadata(str2, session, CultureInfo.CurrentCulture);
                            base.WriteObject(sendToPipeline);
                            flag = true;
                        }
                        catch (EventLogException exception)
                        {
                            Exception exception2 = new Exception(string.Format(CultureInfo.InvariantCulture, this._resourceMgr.GetString("ProviderMetadataUnavailable"), new object[] { str2, exception.Message }), exception);
                            base.WriteError(new ErrorRecord(exception2, "ProviderMetadataUnavailable", ErrorCategory.NotSpecified, null));
                        }
                    }
                }
                if (!flag)
                {
                    Exception exception3 = new Exception(string.Format(CultureInfo.InvariantCulture, this._resourceMgr.GetString("NoMatchingProvidersFound"), new object[] { this._computerName, str }));
                    base.WriteError(new ErrorRecord(exception3, "NoMatchingProvidersFound", ErrorCategory.ObjectNotFound, null));
                }
            }
        }

        protected override void ProcessRecord()
        {
            switch (base.ParameterSetName)
            {
                case "ListLogSet":
                    this.ProcessListLog();
                    return;

                case "ListProviderSet":
                    this.ProcessListProvider();
                    return;

                case "GetLogSet":
                    this.AccumulatePipelineLogNames();
                    return;

                case "FileSet":
                    this.AccumulatePipelineFileNames();
                    return;

                case "HashQuerySet":
                    this.ProcessHashQuery();
                    return;

                case "GetProviderSet":
                    this.AccumulatePipelineProviderNames();
                    return;

                case "XmlQuerySet":
                    this.ProcessFilterXml();
                    return;
            }
            base.WriteDebug(string.Format(CultureInfo.InvariantCulture, "Invalid parameter set name: {0}", new object[] { base.ParameterSetName }));
        }

        private void ReadEvents(EventLogReader readerObj)
        {
            long num = 0L;
            EventRecord record = null;
        Label_0005:
            try
            {
                record = readerObj.ReadEvent();
            }
            catch (Exception exception)
            {
                base.WriteError(new ErrorRecord(exception, exception.Message, ErrorCategory.NotSpecified, null));
                goto Label_0005;
            }
            if ((record != null) && ((this._maxEvents == -1L) || (num < this._maxEvents)))
            {
                PSObject sendToPipeline = new PSObject(record);
                string str = this._resourceMgr.GetString("NoEventMessage");
                try
                {
                    str = record.FormatDescription();
                }
                catch (Exception exception2)
                {
                    base.WriteError(new ErrorRecord(exception2, exception2.Message, ErrorCategory.NotSpecified, null));
                }
                sendToPipeline.Properties.Add(new PSNoteProperty("Message", str));
                base.WriteObject(sendToPipeline, true);
                num += 1L;
                goto Label_0005;
            }
            if (num == 0L)
            {
                Exception exception3 = new Exception(this._resourceMgr.GetString("NoMatchingEventsFound"));
                base.WriteError(new ErrorRecord(exception3, "NoMatchingEventsFound", ErrorCategory.ObjectNotFound, null));
            }
        }

        private bool StringToDateTime(string dtString, ref DateTime dt)
        {
            try
            {
                dt = DateTime.Parse(dtString, CultureInfo.CurrentCulture);
            }
            catch (FormatException exception)
            {
                string format = this._resourceMgr.GetString("DateTimeExpected");
                Exception exception2 = new Exception(string.Format(CultureInfo.InvariantCulture, format, new object[] { dtString }), exception);
                base.WriteError(new ErrorRecord(exception2, "DateTimeExpected", ErrorCategory.InvalidArgument, null));
                return false;
            }
            return true;
        }

        private void TerminateForNonEvtxFileWithoutOldest(string fileName)
        {
            if (!this.Oldest.IsPresent && (System.IO.Path.GetExtension(fileName).Equals(".etl", StringComparison.OrdinalIgnoreCase) || System.IO.Path.GetExtension(fileName).Equals(".evt", StringComparison.OrdinalIgnoreCase)))
            {
                string format = this._resourceMgr.GetString("SpecifyOldestForEtlEvt");
                Exception exception = new Exception(string.Format(CultureInfo.InvariantCulture, format, new object[] { fileName }));
                base.ThrowTerminatingError(new ErrorRecord(exception, "SpecifyOldestForEtlEvt", ErrorCategory.InvalidArgument, fileName));
            }
        }

        private StringCollection ValidateAndResolveFilePath(string path)
        {
            StringCollection strings = new StringCollection();
            Collection<PathInfo> resolvedPSPathFromPSPath = null;
            try
            {
                resolvedPSPathFromPSPath = base.SessionState.Path.GetResolvedPSPathFromPSPath(path);
            }
            catch (PSNotSupportedException exception)
            {
                base.WriteError(new ErrorRecord(exception, "", ErrorCategory.ObjectNotFound, path));
                return strings;
            }
            catch (System.Management.Automation.DriveNotFoundException exception2)
            {
                base.WriteError(new ErrorRecord(exception2, "", ErrorCategory.ObjectNotFound, path));
                return strings;
            }
            catch (ProviderNotFoundException exception3)
            {
                base.WriteError(new ErrorRecord(exception3, "", ErrorCategory.ObjectNotFound, path));
                return strings;
            }
            catch (ItemNotFoundException exception4)
            {
                base.WriteError(new ErrorRecord(exception4, "", ErrorCategory.ObjectNotFound, path));
                return strings;
            }
            catch (Exception exception5)
            {
                base.WriteError(new ErrorRecord(exception5, "", ErrorCategory.ObjectNotFound, path));
                return strings;
            }
            foreach (PathInfo info in resolvedPSPathFromPSPath)
            {
                if (info.Provider.Name != "FileSystem")
                {
                    string format = this._resourceMgr.GetString("NotAFileSystemPath");
                    Exception exception6 = new Exception(string.Format(CultureInfo.InvariantCulture, format, new object[] { path }));
                    base.WriteError(new ErrorRecord(exception6, "NotAFileSystemPath", ErrorCategory.InvalidArgument, path));
                }
                else if ((!System.IO.Path.GetExtension(info.Path).Equals(".evt", StringComparison.OrdinalIgnoreCase) && !System.IO.Path.GetExtension(info.Path).Equals(".evtx", StringComparison.OrdinalIgnoreCase)) && !System.IO.Path.GetExtension(info.Path).Equals(".etl", StringComparison.OrdinalIgnoreCase))
                {
                    if (!WildcardPattern.ContainsWildcardCharacters(path))
                    {
                        string str2 = this._resourceMgr.GetString("NotALogFile");
                        Exception exception7 = new Exception(string.Format(CultureInfo.InvariantCulture, str2, new object[] { info.ProviderPath }));
                        base.WriteError(new ErrorRecord(exception7, "NotALogFile", ErrorCategory.InvalidArgument, path));
                    }
                }
                else
                {
                    this.TerminateForNonEvtxFileWithoutOldest(info.ProviderPath);
                    strings.Add(info.ProviderPath.ToLower(CultureInfo.InvariantCulture));
                }
            }
            return strings;
        }

        private bool ValidateLogName(string logName, EventLogSession eventLogSession)
        {
            EventLogConfiguration configuration;
            try
            {
                configuration = new EventLogConfiguration(logName, eventLogSession);
            }
            catch (EventLogNotFoundException)
            {
                string format = this._resourceMgr.GetString("NoMatchingLogsFound");
                Exception exception = new Exception(string.Format(CultureInfo.InvariantCulture, format, new object[] { this._computerName, logName }));
                base.WriteError(new ErrorRecord(exception, "NoMatchingLogsFound", ErrorCategory.ObjectNotFound, logName));
                return false;
            }
            catch (Exception exception2)
            {
                Exception exception3 = new Exception(string.Format(CultureInfo.InvariantCulture, this._resourceMgr.GetString("LogInfoUnavailable"), new object[] { logName, exception2.Message }), exception2);
                base.WriteError(new ErrorRecord(exception3, "LogInfoUnavailable", ErrorCategory.NotSpecified, null));
                return false;
            }
            if (!this.Oldest.IsPresent && ((configuration.LogType == EventLogType.Debug) || (configuration.LogType == EventLogType.Analytical)))
            {
                string str3 = this._resourceMgr.GetString("SpecifyOldestForLog");
                Exception exception4 = new Exception(string.Format(CultureInfo.InvariantCulture, str3, new object[] { logName }));
                base.ThrowTerminatingError(new ErrorRecord(exception4, "SpecifyOldestForLog", ErrorCategory.InvalidArgument, logName));
            }
            return true;
        }

        [Parameter(ParameterSetName="ListProviderSet", HelpMessageBaseName="GetEventResources", HelpMessageResourceId="ComputerNameParamHelp"), Alias(new string[] { "Cn" }), Parameter(ParameterSetName="GetProviderSet", HelpMessageBaseName="GetEventResources", HelpMessageResourceId="ComputerNameParamHelp"), Parameter(ParameterSetName="ListLogSet", HelpMessageBaseName="GetEventResources", HelpMessageResourceId="ComputerNameParamHelp"), Parameter(ParameterSetName="GetLogSet", HelpMessageBaseName="GetEventResources", HelpMessageResourceId="ComputerNameParamHelp"), Parameter(ParameterSetName="HashQuerySet", HelpMessageBaseName="GetEventResources", HelpMessageResourceId="ComputerNameParamHelp"), Parameter(ParameterSetName="XmlQuerySet", HelpMessageBaseName="GetEventResources", HelpMessageResourceId="ComputerNameParamHelp"), ValidateNotNull]
        public string ComputerName
        {
            get
            {
                return this._computerName;
            }
            set
            {
                this._computerName = value;
            }
        }

        [Parameter(ParameterSetName="HashQuerySet"), Parameter(ParameterSetName="GetProviderSet"), Parameter(ParameterSetName="ListLogSet"), Credential, Parameter(ParameterSetName="FileSet"), Parameter(ParameterSetName="ListProviderSet"), Parameter(ParameterSetName="XmlQuerySet"), Parameter(ParameterSetName="GetLogSet")]
        public PSCredential Credential
        {
            get
            {
                return this._credential;
            }
            set
            {
                this._credential = value;
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Scope="member", Target="Microsoft.PowerShell.Commands.GetEvent.FilterHashtable", Justification="A string[] is required here because that is the type Powershell supports"), Parameter(Position=0, Mandatory=true, ValueFromPipeline=false, ValueFromPipelineByPropertyName=false, ParameterSetName="HashQuerySet", HelpMessageBaseName="GetEventResources")]
        public Hashtable[] FilterHashtable
        {
            get
            {
                return this._selector;
            }
            set
            {
                this._selector = value;
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes", Scope="member", Target="Microsoft.PowerShell.Commands.GetEvent.FilterXml", Justification="An XmlDocument is required here because that is the type Powershell supports"), Parameter(Position=0, Mandatory=true, ValueFromPipeline=false, ValueFromPipelineByPropertyName=false, ParameterSetName="XmlQuerySet", HelpMessageBaseName="GetEventResources")]
        public XmlDocument FilterXml
        {
            get
            {
                return this._xmlQuery;
            }
            set
            {
                this._xmlQuery = value;
            }
        }

        [ValidateNotNull, Parameter(ParameterSetName="FileSet", ValueFromPipeline=false, ValueFromPipelineByPropertyName=false, HelpMessageBaseName="GetEventResources"), Parameter(ParameterSetName="GetProviderSet", ValueFromPipeline=false, ValueFromPipelineByPropertyName=false, HelpMessageBaseName="GetEventResources"), Parameter(ParameterSetName="GetLogSet", ValueFromPipeline=false, ValueFromPipelineByPropertyName=false, HelpMessageBaseName="GetEventResources")]
        public string FilterXPath
        {
            get
            {
                return this._filter;
            }
            set
            {
                this._filter = value;
            }
        }

        [Parameter(ParameterSetName="GetProviderSet"), Parameter(ParameterSetName="GetLogSet"), Parameter(ParameterSetName="HashQuerySet"), Parameter(ParameterSetName="ListLogSet")]
        public SwitchParameter Force
        {
            get
            {
                return this._force;
            }
            set
            {
                this._force = value;
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Scope="member", Target="Microsoft.PowerShell.Commands.GetEvent.ListLog", Justification="A string[] is required here because that is the type Powershell supports"), Parameter(Position=0, Mandatory=true, ParameterSetName="ListLogSet", ValueFromPipeline=false, ValueFromPipelineByPropertyName=false, HelpMessageBaseName="GetEventResources", HelpMessageResourceId="ListLogParamHelp"), AllowEmptyCollection]
        public string[] ListLog
        {
            get
            {
                return this._listLog;
            }
            set
            {
                this._listLog = value;
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Scope="member", Target="Microsoft.PowerShell.Commands.GetEvent.ListProvider", Justification="A string[] is required here because that is the type Powershell supports"), AllowEmptyCollection, Parameter(Position=0, Mandatory=true, ParameterSetName="ListProviderSet", ValueFromPipeline=false, ValueFromPipelineByPropertyName=false, HelpMessageBaseName="GetEventResources", HelpMessageResourceId="ListProviderParamHelp")]
        public string[] ListProvider
        {
            get
            {
                return this._listProvider;
            }
            set
            {
                this._listProvider = value;
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Scope="member", Target="Microsoft.PowerShell.Commands.GetEvent.LogName", Justification="A string[] is required here because that is the type Powershell supports"), Parameter(Position=0, ParameterSetName="GetLogSet", ValueFromPipeline=true, ValueFromPipelineByPropertyName=true, HelpMessageBaseName="GetEventResources", HelpMessageResourceId="GetLogParamHelp")]
        public string[] LogName
        {
            get
            {
                return this._logName;
            }
            set
            {
                this._logName = value;
            }
        }

        [Parameter(ParameterSetName="GetLogSet", ValueFromPipeline=false, ValueFromPipelineByPropertyName=false, HelpMessageBaseName="GetEventResources", HelpMessageResourceId="MaxEventsParamHelp"), Parameter(ParameterSetName="XmlQuerySet", ValueFromPipeline=false, ValueFromPipelineByPropertyName=false, HelpMessageBaseName="GetEventResources", HelpMessageResourceId="MaxEventsParamHelp"), ValidateRange(1L, 0x7fffffffffffffffL), Parameter(ParameterSetName="GetProviderSet", ValueFromPipeline=false, ValueFromPipelineByPropertyName=false, HelpMessageBaseName="GetEventResources", HelpMessageResourceId="MaxEventsParamHelp"), Parameter(ParameterSetName="FileSet", ValueFromPipeline=false, ValueFromPipelineByPropertyName=false, HelpMessageBaseName="GetEventResources", HelpMessageResourceId="MaxEventsParamHelp"), Parameter(ParameterSetName="HashQuerySet", ValueFromPipeline=false, ValueFromPipelineByPropertyName=false, HelpMessageBaseName="GetEventResources", HelpMessageResourceId="MaxEventsParamHelp")]
        public long MaxEvents
        {
            get
            {
                return this._maxEvents;
            }
            set
            {
                this._maxEvents = value;
            }
        }

        [Parameter(ParameterSetName="GetLogSet"), Parameter(ParameterSetName="XmlQuerySet"), Parameter(ParameterSetName="GetProviderSet"), Parameter(ParameterSetName="FileSet"), Parameter(ParameterSetName="HashQuerySet")]
        public SwitchParameter Oldest
        {
            get
            {
                return this._oldest;
            }
            set
            {
                this._oldest = (bool) value;
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Scope="member", Target="Microsoft.PowerShell.Commands.GetEvent.Path", Justification="A string[] is required here because that is the type Powershell supports"), Parameter(Position=0, Mandatory=true, ParameterSetName="FileSet", ValueFromPipelineByPropertyName=true, HelpMessageBaseName="GetEventResources", HelpMessageResourceId="PathParamHelp"), Alias(new string[] { "PSPath" })]
        public string[] Path
        {
            get
            {
                return this._path;
            }
            set
            {
                this._path = value;
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Scope="member", Target="Microsoft.PowerShell.Commands.GetEvent.ProviderName", Justification="A string[] is required here because that is the type Powershell supports"), Parameter(Position=0, Mandatory=true, ParameterSetName="GetProviderSet", ValueFromPipelineByPropertyName=true, HelpMessageBaseName="GetEventResources", HelpMessageResourceId="GetProviderParamHelp")]
        public string[] ProviderName
        {
            get
            {
                return this._providerName;
            }
            set
            {
                this._providerName = value;
            }
        }
    }
}

