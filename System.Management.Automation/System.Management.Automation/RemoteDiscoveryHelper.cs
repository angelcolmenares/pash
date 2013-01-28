namespace System.Management.Automation
{
    using Microsoft.Management.Infrastructure;
    using Microsoft.Management.Infrastructure.Options;
    using Microsoft.PowerShell;
    using Microsoft.PowerShell.Commands;
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Management.Automation.Language;
    using System.Management.Automation.Runspaces;
    using System.Net;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Xml;

    internal class RemoteDiscoveryHelper
    {
        private static readonly ConditionalWeakTable<PSModuleInfo, object> _moduleInfoToSession = new ConditionalWeakTable<PSModuleInfo, object>();
        private static readonly int BlockingCollectionCapacity = 0x3e8;
        private const string DiscoveryProviderAssociationClass = "PS_ModuleToModuleFile";
        private const string DiscoveryProviderFileClass = "PS_ModuleFile";
        private const string DiscoveryProviderModuleClass = "PS_Module";
        private const string DiscoveryProviderNamespace = "root/Microsoft/Windows/Powershellv3";
        private const string DiscoveryProviderNotFoundErrorId = "DiscoveryProviderNotFound";
        private static readonly string[] ManifestEntriesToKeepAsString = new string[] { "GUID", "Author", "CompanyName", "Copyright", "ModuleVersion", "Description", "HelpInfoURI" };
        private static readonly string[] ManifestEntriesToKeepAsStringArray = new string[] { "FunctionsToExport", "VariablesToExport", "AliasesToExport", "CmdletsToExport" };

        internal static void AssociatePSModuleInfoWithSession(PSModuleInfo moduleInfo, PSSession psSession)
        {
            AssociatePSModuleInfoWithSession(moduleInfo, psSession);
        }

        private static void AssociatePSModuleInfoWithSession(PSModuleInfo moduleInfo, object weaklyTypedSession)
        {
            _moduleInfoToSession.Add(moduleInfo, weaklyTypedSession);
        }

        internal static void AssociatePSModuleInfoWithSession(PSModuleInfo moduleInfo, CimSession cimSession, Uri resourceUri, string cimNamespace)
        {
            AssociatePSModuleInfoWithSession(moduleInfo, new Tuple<CimSession, Uri, string>(cimSession, resourceUri, cimNamespace));
        }

        internal static Hashtable ConvertCimModuleFileToManifestHashtable(CimModuleFile cimModuleFile, string temporaryModuleManifestPath, ModuleCmdletBase cmdlet, ref bool containedErrors)
        {
            ScriptBlockAst ast = null;
            if (!containedErrors)
            {
                System.Management.Automation.Language.Token[] tokenArray;
                ParseError[] errorArray;
                ast = Parser.ParseInput(cimModuleFile.FileData, temporaryModuleManifestPath, out tokenArray, out errorArray);
                if ((ast == null) || ((errorArray != null) && (errorArray.Length > 0)))
                {
                    containedErrors = true;
                }
            }
            Hashtable hashtable = null;
            if (!containedErrors)
            {
                ScriptBlock scriptBlock = new ScriptBlock(ast, false);
                hashtable = cmdlet.LoadModuleManifestData(temporaryModuleManifestPath, scriptBlock, ModuleCmdletBase.ModuleManifestMembers, 0, ref containedErrors);
            }
            return hashtable;
        }

        private static void CopyParameterFromCmdletToPowerShell(Cmdlet cmdlet, PowerShell powerShell, string parameterName)
        {
            object obj2;
            Func<CommandParameter, bool> predicate = null;
            if (cmdlet.MyInvocation.BoundParameters.TryGetValue(parameterName, out obj2))
            {
                CommandParameter item = new CommandParameter(parameterName, obj2);
                foreach (Command command in powerShell.Commands.Commands)
                {
                    if (predicate == null)
                    {
                        predicate = existingParameter => existingParameter.Name.Equals(parameterName, StringComparison.OrdinalIgnoreCase);
                    }
                    if (!command.Parameters.Any<CommandParameter>(predicate))
                    {
                        command.Parameters.Add(item);
                    }
                }
            }
        }

        internal static CimSession CreateCimSession(string computerName, PSCredential credential, string authentication, CancellationToken cancellationToken, PSCmdlet cmdlet)
        {
            CimSessionOptions sessionOptions = new CimSessionOptions();
            CimCredential cimCredentials = GetCimCredentials(authentication, credential);
            if (cimCredentials != null)
            {
                sessionOptions.AddDestinationCredentials(cimCredentials);
            }
            return CimSession.Create(computerName, sessionOptions);
        }

        internal static void DispatchModuleInfoProcessing(PSModuleInfo moduleInfo, Action localAction, Action<CimSession, Uri, string> cimSessionAction, Action<PSSession> psSessionAction)
        {
            object obj2;
            if (!_moduleInfoToSession.TryGetValue(moduleInfo, out obj2))
            {
                localAction();
            }
            else
            {
                Tuple<CimSession, Uri, string> tuple = obj2 as Tuple<CimSession, Uri, string>;
                if (tuple != null)
                {
                    cimSessionAction(tuple.Item1, tuple.Item2, tuple.Item3);
                }
                else
                {
                    PSSession session = obj2 as PSSession;
                    if (session != null)
                    {
                        psSessionAction(session);
                    }
                }
            }
        }

        private static IEnumerable<T> EnumerateWithCatch<T>(IEnumerable<T> enumerable, Action<Exception> exceptionHandler)
        {
            IEnumerator<T> enumerator = null;
            try
            {
                enumerator = enumerable.GetEnumerator();
            }
            catch (Exception exception)
            {
                exceptionHandler(exception);
            }
            if (enumerator != null)
            {
                using (enumerator)
                {
                    bool iteratorVariable1 = false;
                    do
                    {
                        try
                        {
                            iteratorVariable1 = false;
                            iteratorVariable1 = enumerator.MoveNext();
                        }
                        catch (Exception exception2)
                        {
                            exceptionHandler(exception2);
                        }
                        if (iteratorVariable1)
                        {
                            T current = default(T);
                            bool iteratorVariable3 = false;
                            try
                            {
                                current = enumerator.Current;
                                iteratorVariable3 = true;
                            }
                            catch (Exception exception3)
                            {
                                exceptionHandler(exception3);
                            }
                            if (iteratorVariable3)
                            {
                                yield return current;
                            }
                            else
                            {
                                goto Label_011D;
                            }
                        }
                    }
                    while (iteratorVariable1);
                }
            }
        Label_011D:
            yield break;
        }

        private static CimCredential GetCimCredentials(PasswordAuthenticationMechanism authenticationMechanism, PSCredential credential)
        {
            NetworkCredential networkCredential = credential.GetNetworkCredential();
            return new CimCredential(authenticationMechanism, networkCredential.Domain, networkCredential.UserName, networkCredential.SecurePassword);
        }

        private static CimCredential GetCimCredentials(string authentication, PSCredential credential)
        {
            if ((authentication == null) || authentication.Equals("Default", StringComparison.OrdinalIgnoreCase))
            {
                if (credential == null)
                {
                    return null;
                }
                return GetCimCredentials(PasswordAuthenticationMechanism.Default, credential);
            }
            if (authentication.Equals("Basic", StringComparison.OrdinalIgnoreCase))
            {
                if (credential == null)
                {
                    throw GetExceptionWhenAuthenticationRequiresCredential(authentication);
                }
                return GetCimCredentials(PasswordAuthenticationMechanism.Basic, credential);
            }
            if (authentication.Equals("Negotiate", StringComparison.OrdinalIgnoreCase))
            {
                if (credential == null)
                {
                    return new CimCredential(ImpersonatedAuthenticationMechanism.Negotiate);
                }
                return GetCimCredentials(PasswordAuthenticationMechanism.Negotiate, credential);
            }
            if (authentication.Equals("CredSSP", StringComparison.OrdinalIgnoreCase))
            {
                if (credential == null)
                {
                    throw GetExceptionWhenAuthenticationRequiresCredential(authentication);
                }
                return GetCimCredentials(PasswordAuthenticationMechanism.CredSsp, credential);
            }
            if (authentication.Equals("Digest", StringComparison.OrdinalIgnoreCase))
            {
                if (credential == null)
                {
                    throw GetExceptionWhenAuthenticationRequiresCredential(authentication);
                }
                return GetCimCredentials(PasswordAuthenticationMechanism.Digest, credential);
            }
            if (!authentication.Equals("Kerberos", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentOutOfRangeException("authentication");
            }
            if (credential == null)
            {
                return new CimCredential(ImpersonatedAuthenticationMechanism.Kerberos);
            }
            return GetCimCredentials(PasswordAuthenticationMechanism.Kerberos, credential);
        }

        internal static IEnumerable<CimModule> GetCimModules(CimSession cimSession, Uri resourceUri, string cimNamespace, IEnumerable<string> moduleNamePatterns, bool onlyManifests, Cmdlet cmdlet, CancellationToken cancellationToken)
        {
            moduleNamePatterns = moduleNamePatterns ?? ((IEnumerable<string>) new string[] { "*" });
            HashSet<string> iteratorVariable0 = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            IEnumerable<CimModule> iteratorVariable1 = moduleNamePatterns.SelectMany(moduleNamePattern => GetCimModules(cimSession, resourceUri, cimNamespace, moduleNamePattern, onlyManifests, cmdlet, cancellationToken));
            foreach (CimModule iteratorVariable2 in iteratorVariable1)
            {
                if (iteratorVariable0.Contains(iteratorVariable2.ModuleName))
                {
                    continue;
                }
                iteratorVariable0.Add(iteratorVariable2.ModuleName);
                yield return iteratorVariable2;
            }
        }

        private static IEnumerable<CimModule> GetCimModules(CimSession cimSession, Uri resourceUri, string cimNamespace, string moduleNamePattern, bool onlyManifests, Cmdlet cmdlet, CancellationToken cancellationToken)
        {
            Func<CimModule, CimModule> selector = null;
            WildcardPattern wildcardPattern = new WildcardPattern(moduleNamePattern, WildcardOptions.CultureInvariant | WildcardOptions.IgnoreCase);
            string optionValue = WildcardPatternToDosWildcardParser.Parse(wildcardPattern);
            CimOperationOptions options = new CimOperationOptions {
                CancellationToken = new CancellationToken?(cancellationToken)
            };
            options.SetCustomOption("PS_ModuleNamePattern", optionValue, false);
            if (resourceUri != null)
            {
                options.ResourceUri = resourceUri;
            }
            if (string.IsNullOrEmpty(cimNamespace) && (resourceUri == null))
            {
                cimNamespace = "root/Microsoft/Windows/Powershellv3";
            }
            IEnumerable<CimModule> source = from cimInstance in cimSession.EnumerateInstances(cimNamespace, "PS_Module", options)
                select new CimModule(cimInstance) into cimModule
                where wildcardPattern.IsMatch(cimModule.ModuleName)
                select cimModule;
            if (!onlyManifests)
            {
                if (selector == null)
                {
                    selector = delegate (CimModule cimModule) {
                        cimModule.FetchAllModuleFiles(cimSession, cimNamespace, options);
                        return cimModule;
                    };
                }
                source = source.Select<CimModule, CimModule>(selector);
            }
            return EnumerateWithCatch<CimModule>(source, delegate (Exception exception) {
                ErrorRecord errorRecord = GetErrorRecordForRemoteDiscoveryProvider(exception);
                if (!cmdlet.MyInvocation.ExpectingInput && (((-1 != errorRecord.FullyQualifiedErrorId.IndexOf("DiscoveryProviderNotFound", StringComparison.OrdinalIgnoreCase)) || cancellationToken.IsCancellationRequested) || ((exception is OperationCanceledException) || !cimSession.TestConnection())))
                {
                    cmdlet.ThrowTerminatingError(errorRecord);
                }
                cmdlet.WriteError(errorRecord);
            });
        }

        internal static ErrorRecord GetErrorRecordForProcessingOfCimModule(Exception innerException, string moduleName)
        {
            return new ErrorRecord(new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, Modules.RemoteDiscoveryFailedToProcessRemoteModule, new object[] { moduleName, innerException.Message }), innerException), innerException.GetType().Name, ErrorCategory.NotSpecified, moduleName);
        }

        private static ErrorRecord GetErrorRecordForRemoteDiscoveryProvider(Exception innerException)
        {
            CimException exception = innerException as CimException;
            if ((exception != null) && (((exception.NativeErrorCode == NativeErrorCode.InvalidNamespace) || (exception.NativeErrorCode == NativeErrorCode.InvalidClass)) || ((exception.NativeErrorCode == NativeErrorCode.MethodNotFound) || (exception.NativeErrorCode == NativeErrorCode.MethodNotAvailable))))
            {
                return new ErrorRecord(new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, Modules.RemoteDiscoveryProviderNotFound, new object[] { innerException.Message }), innerException), "DiscoveryProviderNotFound", ErrorCategory.NotImplemented, null);
            }
            return new ErrorRecord(new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, Modules.RemoteDiscoveryFailureFromDiscoveryProvider, new object[] { innerException.Message }), innerException), "DiscoveryProviderFailure", ErrorCategory.NotSpecified, null);
        }

        private static ErrorRecord GetErrorRecordForRemotePipelineInvocation(Exception innerException, string errorMessageTemplate)
        {
            Exception exception = new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, errorMessageTemplate, new object[] { innerException.Message }), innerException);
            RemoteException exception2 = innerException as RemoteException;
            ErrorRecord record = (exception2 != null) ? exception2.ErrorRecord : null;
            string errorId = (record != null) ? record.FullyQualifiedErrorId : innerException.GetType().Name;
            return new ErrorRecord(exception, errorId, (record != null) ? record.CategoryInfo.Category : ErrorCategory.NotSpecified, null);
        }

        private static ErrorRecord GetErrorRecordForRemotePipelineInvocation(ErrorRecord innerErrorRecord, string errorMessageTemplate)
        {
            string str;
            if ((innerErrorRecord.ErrorDetails != null) && (innerErrorRecord.ErrorDetails.Message != null))
            {
                str = innerErrorRecord.ErrorDetails.Message;
            }
            else if ((innerErrorRecord.Exception != null) && (innerErrorRecord.Exception.Message != null))
            {
                str = innerErrorRecord.Exception.Message;
            }
            else
            {
                str = innerErrorRecord.ToString();
            }
            string message = string.Format(CultureInfo.InvariantCulture, errorMessageTemplate, new object[] { str });
            ErrorRecord record = new ErrorRecord(innerErrorRecord, null);
            ErrorDetails details = new ErrorDetails(message);
            record.ErrorDetails = details;
            return record;
        }

        private static Exception GetExceptionWhenAuthenticationRequiresCredential(string authentication)
        {
            throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, RemotingErrorIdStrings.AuthenticationMechanismRequiresCredential, new object[] { authentication }));
        }

        internal static string GetModulePath(string remoteModuleName, Version remoteModuleVersion, string computerName, Runspace localRunspace)
        {
            computerName = computerName ?? string.Empty;
            string str = Regex.Replace(remoteModuleName, "[^a-zA-Z0-9]", "");
            string str2 = Regex.Replace(computerName, "[^a-zA-Z0-9]", "");
            string str3 = string.Format(CultureInfo.InvariantCulture, "remoteIpMoProxy_{0}_{1}_{2}_{3}", new object[] { str.Substring(0, Math.Min(str.Length, 100)), remoteModuleVersion, str2.Substring(0, Math.Min(str2.Length, 100)), RuntimeHelpers.GetHashCode(localRunspace) });
            return Path.Combine(Path.GetTempPath(), str3);
        }

        private static T GetPropertyValue<T>(CimInstance cimInstance, string propertyName, T defaultValue)
        {
            CimProperty property = cimInstance.CimInstanceProperties[propertyName];
            if (property != null)
            {
                object obj2 = property.Value;
                if (obj2 is T)
                {
                    return (T) obj2;
                }
                if (!(obj2 is string))
                {
                    return defaultValue;
                }
                string s = (string) obj2;
                try
                {
                    if (typeof(T).Equals(typeof(bool)))
                    {

                        return (T)LanguagePrimitives.ConvertTo<T>(XmlConvert.ToBoolean(s));
                    }
                    if (typeof(T).Equals(typeof(ushort)))
                    {
                        return (T)LanguagePrimitives.ConvertTo<T>(ushort.Parse(s, CultureInfo.InvariantCulture));
                    }
                    if (!typeof(T).Equals(typeof(byte[])))
                    {
                        return defaultValue;
                    }
                    byte[] second = Convert.FromBase64String(s);
                    byte[] bytes = BitConverter.GetBytes((int) (second.Length + 4));
                    if (BitConverter.IsLittleEndian)
                    {
                        Array.Reverse(bytes);
                    }
                    return (T)LanguagePrimitives.ConvertTo<T>(bytes.Concat<byte>(second).ToArray<byte>());
                }
                catch (Exception)
                {
                    return defaultValue;
                }
            }
            return defaultValue;
        }

        private static EventHandler<DataAddedEventArgs> GetStreamForwarder<T>(Action<T> forwardingAction, bool swallowInvalidOperationExceptions = false)
        {
            return delegate (object sender, DataAddedEventArgs eventArgs) {
                PSDataCollection<T> datas = (PSDataCollection<T>) sender;
                foreach (T local in datas.ReadAll())
                {
                    try
                    {
                        forwardingAction(local);
                    }
                    catch (InvalidOperationException)
                    {
                        if (!swallowInvalidOperationExceptions)
                        {
                            throw;
                        }
                    }
                }
            };
        }

        private static void HandleErrorFromPipeline(Cmdlet cmdlet, ErrorRecord errorRecord, PowerShell powerShell)
        {
            if (!cmdlet.MyInvocation.ExpectingInput && (((powerShell.Runspace != null) && (powerShell.Runspace.RunspaceStateInfo.State != RunspaceState.Opened)) || ((powerShell.RunspacePool != null) && (powerShell.RunspacePool.RunspacePoolStateInfo.State != RunspacePoolState.Opened))))
            {
                cmdlet.ThrowTerminatingError(errorRecord);
            }
            cmdlet.WriteError(errorRecord);
        }

        private static IEnumerable<PSObject> InvokeNestedPowerShell(PowerShell powerShell, CancellationToken cancellationToken, PSCmdlet cmdlet, PSInvocationSettings invocationSettings, string errorMessageTemplate)
        {
            EventHandler<DataAddedEventArgs> streamForwarder = GetStreamForwarder<ErrorRecord>(delegate (ErrorRecord errorRecord) {
                errorRecord = GetErrorRecordForRemotePipelineInvocation(errorRecord, errorMessageTemplate);
                HandleErrorFromPipeline(cmdlet, errorRecord, powerShell);
            }, false);
            powerShell.Streams.Error.DataAdded += streamForwarder;
            using (cancellationToken.Register(new Action(powerShell.Stop)))
            {
                foreach (PSObject iteratorVariable1 in powerShell.Invoke<PSObject>(null, invocationSettings))
                {
                    yield return iteratorVariable1;
                }
            }
        }

        internal static IEnumerable<PSObject> InvokePowerShell(PowerShell powerShell, CancellationToken cancellationToken, PSCmdlet cmdlet, string errorMessageTemplate)
        {
            CopyParameterFromCmdletToPowerShell(cmdlet, powerShell, "ErrorAction");
            CopyParameterFromCmdletToPowerShell(cmdlet, powerShell, "WarningAction");
            CopyParameterFromCmdletToPowerShell(cmdlet, powerShell, "Verbose");
            CopyParameterFromCmdletToPowerShell(cmdlet, powerShell, "Debug");
            PSInvocationSettings invocationSettings = new PSInvocationSettings {
                Host = cmdlet.Host
            };
            IEnumerable<PSObject> enumerable = powerShell.IsNested ? InvokeNestedPowerShell(powerShell, cancellationToken, cmdlet, invocationSettings, errorMessageTemplate) : InvokeTopLevelPowerShell(powerShell, cancellationToken, cmdlet, invocationSettings, errorMessageTemplate);
            return EnumerateWithCatch<PSObject>(enumerable, delegate (Exception exception) {
                ErrorRecord errorRecord = GetErrorRecordForRemotePipelineInvocation(exception, errorMessageTemplate);
                HandleErrorFromPipeline(cmdlet, errorRecord, powerShell);
            });
        }

        private static IEnumerable<PSObject> InvokeTopLevelPowerShell(PowerShell powerShell, CancellationToken cancellationToken, PSCmdlet cmdlet, PSInvocationSettings invocationSettings, string errorMessageTemplate)
        {

            Action<PSObject> action = null;
            Action<ErrorRecord> action1 = null;
            Action<WarningRecord> action2 = null;
            Action<VerboseRecord> action3 = null;
            Action<DebugRecord> action4 = null;
            AsyncCallback asyncCallback = null;
            using (BlockingCollection<Func<PSCmdlet, IEnumerable<PSObject>>> funcs = new BlockingCollection<Func<PSCmdlet, IEnumerable<PSObject>>>(RemoteDiscoveryHelper.BlockingCollectionCapacity))
            {
                PSDataCollection<PSObject> pSObjects = new PSDataCollection<PSObject>();
                if (action == null)
                {
                    action = (PSObject output) => funcs.Add((PSCmdlet argument0) =>
                    {
                        PSObject[] pSObjectArray = new PSObject[1];
                        pSObjectArray[0] = output;
                        return pSObjectArray;
                    }
                    );
                }
                EventHandler<DataAddedEventArgs> streamForwarder = RemoteDiscoveryHelper.GetStreamForwarder<PSObject>(action, true);
                if (action1 == null)
                {
                    action1 = (ErrorRecord errorRecord) => funcs.Add((PSCmdlet c) =>
                    {
                        errorRecord = RemoteDiscoveryHelper.GetErrorRecordForRemotePipelineInvocation(errorRecord, errorMessageTemplate);
                        RemoteDiscoveryHelper.HandleErrorFromPipeline(c, errorRecord, powerShell);
                        return Enumerable.Empty<PSObject>();
                    }
                    );
                }
                EventHandler<DataAddedEventArgs> eventHandler = RemoteDiscoveryHelper.GetStreamForwarder<ErrorRecord>(action1, true);
                if (action2 == null)
                {
                    action2 = (WarningRecord warningRecord) => funcs.Add((PSCmdlet c) =>
                    {
                        c.WriteWarning(warningRecord.Message);
                        return Enumerable.Empty<PSObject>();
                    }
                    );
                }
                EventHandler<DataAddedEventArgs> streamForwarder1 = RemoteDiscoveryHelper.GetStreamForwarder<WarningRecord>(action2, true);
                if (action3 == null)
                {
                    action3 = (VerboseRecord verboseRecord) => funcs.Add((PSCmdlet c) =>
                    {
                        c.WriteVerbose(verboseRecord.Message);
                        return Enumerable.Empty<PSObject>();
                    }
                    );
                }
                EventHandler<DataAddedEventArgs> eventHandler1 = RemoteDiscoveryHelper.GetStreamForwarder<VerboseRecord>(action3, true);
                if (action4 == null)
                {
                    action4 = (DebugRecord debugRecord) => funcs.Add((PSCmdlet c) =>
                    {
                        c.WriteDebug(debugRecord.Message);
                        return Enumerable.Empty<PSObject>();
                    }
                    );
                }
                EventHandler<DataAddedEventArgs> streamForwarder2 = RemoteDiscoveryHelper.GetStreamForwarder<DebugRecord>(action4, true);
                pSObjects.DataAdded += streamForwarder;
                powerShell.Streams.Error.DataAdded += eventHandler;
                powerShell.Streams.Warning.DataAdded += streamForwarder1;
                powerShell.Streams.Verbose.DataAdded += eventHandler1;
                powerShell.Streams.Debug.DataAdded += streamForwarder2;
                try
                {
                    PowerShell powerShell1 = powerShell;
                    object obj = null;
                    PSDataCollection<PSObject> pSObjects1 = pSObjects;
                    PSInvocationSettings pSInvocationSetting = invocationSettings;
                    if (asyncCallback == null)
                    {
                        asyncCallback = (IAsyncResult param0) =>
                        {
                            try
                            {
                                funcs.CompleteAdding();
                            }
                            catch (InvalidOperationException invalidOperationException)
                            {
                            }
                        }
                        ;
                    }
                    IAsyncResult asyncResult = powerShell1.BeginInvoke<PSObject, PSObject>((PSDataCollection<PSObject>)obj, pSObjects1, pSInvocationSetting, asyncCallback, null);
                    CancellationTokenRegistration cancellationTokenRegistration = cancellationToken.Register(new Action(powerShell.Stop));
                    try
                    {
                        try
                        {
                            foreach (Func<PSCmdlet, IEnumerable<PSObject>> func in funcs)
                            {
                                IEnumerator<PSObject> enumerator = func(cmdlet).GetEnumerator();
                                using (enumerator)
                                {
                                    while (enumerator.MoveNext())
                                    {
                                        yield return enumerator.Current;
                                    }
                                }
                            }
                        }
                        finally
                        {
                            funcs.CompleteAdding();
                            powerShell.EndInvoke(asyncResult);
                        }
                    }
                    finally
                    {
                        cancellationTokenRegistration.Dispose();
                    }
                }
                finally
                {
                    pSObjects.DataAdded -= streamForwarder;
                    powerShell.Streams.Error.DataAdded -= eventHandler;
                    powerShell.Streams.Warning.DataAdded -= streamForwarder1;
                    powerShell.Streams.Verbose.DataAdded -= eventHandler1;
                    powerShell.Streams.Debug.DataAdded -= streamForwarder2;
                }
            }
        }

        private static Collection<string> RehydrateHashtableKeys(PSObject pso, string propertyName)
        {
            DeserializingTypeConverter.RehydrationFlags flags = DeserializingTypeConverter.RehydrationFlags.MissingPropertyOk | DeserializingTypeConverter.RehydrationFlags.NullValueOk;
            Hashtable hashtable = DeserializingTypeConverter.GetPropertyValue<Hashtable>(pso, propertyName, flags);
            if (hashtable == null)
            {
                return new Collection<string>();
            }
            return new Collection<string>((from k in hashtable.Keys.Cast<object>()
                where k != null
                select k.ToString() into s
                where s != null
                select s).ToList<string>());
        }

        internal static PSModuleInfo RehydratePSModuleInfo(PSObject deserializedModuleInfo)
        {
            DeserializingTypeConverter.RehydrationFlags flags = DeserializingTypeConverter.RehydrationFlags.MissingPropertyOk | DeserializingTypeConverter.RehydrationFlags.NullValueOk;
            string name = DeserializingTypeConverter.GetPropertyValue<string>(deserializedModuleInfo, "Name", flags);
            string path = DeserializingTypeConverter.GetPropertyValue<string>(deserializedModuleInfo, "Path", flags);
            PSModuleInfo info = new PSModuleInfo(name, path, null, null);
            info.SetGuid(DeserializingTypeConverter.GetPropertyValue<Guid>(deserializedModuleInfo, "Guid", flags));
            info.SetModuleType(DeserializingTypeConverter.GetPropertyValue<ModuleType>(deserializedModuleInfo, "ModuleType", flags));
            info.SetVersion(DeserializingTypeConverter.GetPropertyValue<Version>(deserializedModuleInfo, "Version", flags));
            info.AccessMode = DeserializingTypeConverter.GetPropertyValue<ModuleAccessMode>(deserializedModuleInfo, "AccessMode", flags);
            info.Author = DeserializingTypeConverter.GetPropertyValue<string>(deserializedModuleInfo, "Author", flags);
            info.ClrVersion = DeserializingTypeConverter.GetPropertyValue<Version>(deserializedModuleInfo, "ClrVersion", flags);
            info.CompanyName = DeserializingTypeConverter.GetPropertyValue<string>(deserializedModuleInfo, "CompanyName", flags);
            info.Copyright = DeserializingTypeConverter.GetPropertyValue<string>(deserializedModuleInfo, "Copyright", flags);
            info.Description = DeserializingTypeConverter.GetPropertyValue<string>(deserializedModuleInfo, "Description", flags);
            info.DotNetFrameworkVersion = DeserializingTypeConverter.GetPropertyValue<Version>(deserializedModuleInfo, "DotNetFrameworkVersion", flags);
            info.PowerShellHostName = DeserializingTypeConverter.GetPropertyValue<string>(deserializedModuleInfo, "PowerShellHostName", flags);
            info.PowerShellHostVersion = DeserializingTypeConverter.GetPropertyValue<Version>(deserializedModuleInfo, "PowerShellHostVersion", flags);
            info.PowerShellVersion = DeserializingTypeConverter.GetPropertyValue<Version>(deserializedModuleInfo, "PowerShellVersion", flags);
            info.ProcessorArchitecture = DeserializingTypeConverter.GetPropertyValue<ProcessorArchitecture>(deserializedModuleInfo, "ProcessorArchitecture", flags);
            info.DeclaredAliasExports = RehydrateHashtableKeys(deserializedModuleInfo, "ExportedAliases");
            info.DeclaredCmdletExports = RehydrateHashtableKeys(deserializedModuleInfo, "ExportedCmdlets");
            info.DeclaredFunctionExports = RehydrateHashtableKeys(deserializedModuleInfo, "ExportedFunctions");
            info.DeclaredVariableExports = RehydrateHashtableKeys(deserializedModuleInfo, "ExportedVariables");
            return info;
        }

        internal static Hashtable RewriteManifest(Hashtable originalManifest)
        {
            return RewriteManifest(originalManifest, null, null, null);
        }

        internal static Hashtable RewriteManifest(Hashtable originalManifest, IEnumerable<string> nestedModules, IEnumerable<string> typesToProcess, IEnumerable<string> formatsToProcess)
        {
            nestedModules = nestedModules ?? ((IEnumerable<string>) new string[0]);
            typesToProcess = typesToProcess ?? ((IEnumerable<string>) new string[0]);
            formatsToProcess = formatsToProcess ?? ((IEnumerable<string>) new string[0]);
            Hashtable hashtable = new Hashtable(StringComparer.OrdinalIgnoreCase);
            hashtable["NestedModules"] = nestedModules;
            hashtable["TypesToProcess"] = typesToProcess;
            hashtable["FormatsToProcess"] = formatsToProcess;
            foreach (DictionaryEntry entry in originalManifest)
            {
                if (ManifestEntriesToKeepAsString.Contains<string>(entry.Key as string, StringComparer.OrdinalIgnoreCase))
                {
                    string str = (string) LanguagePrimitives.ConvertTo(entry.Value, typeof(string), CultureInfo.InvariantCulture);
                    hashtable[entry.Key] = str;
                }
                else if (ManifestEntriesToKeepAsStringArray.Contains<string>(entry.Key as string, StringComparer.OrdinalIgnoreCase))
                {
                    string[] strArray = (string[]) LanguagePrimitives.ConvertTo(entry.Value, typeof(string[]), CultureInfo.InvariantCulture);
                    hashtable[entry.Key] = strArray;
                }
            }
            return hashtable;
        }

        
        internal enum CimFileCode
        {
            Unknown,
            PsdV1,
            TypesV1,
            FormatV1,
            CmdletizationV1
        }

        internal class CimModule
        {
            private readonly CimInstance _baseObject;
            private List<RemoteDiscoveryHelper.CimModuleFile> _moduleFiles;

            internal CimModule(CimInstance baseObject)
            {
                this._baseObject = baseObject;
            }

            internal void FetchAllModuleFiles(CimSession cimSession, string cimNamespace, CimOperationOptions operationOptions)
            {
                IEnumerable<RemoteDiscoveryHelper.CimModuleFile> source = from i in cimSession.EnumerateAssociatedInstances(cimNamespace, this._baseObject, "PS_ModuleToModuleFile", "PS_ModuleFile", "Antecedent", "Dependent", operationOptions) select new CimModuleImplementationFile(i);
                this._moduleFiles = source.ToList<RemoteDiscoveryHelper.CimModuleFile>();
            }

            public bool IsPsCimModule
            {
                get
                {
                    return (RemoteDiscoveryHelper.GetPropertyValue<ushort>(this._baseObject, "ModuleType", 0) == 1);
                }
            }

            public RemoteDiscoveryHelper.CimModuleFile MainManifest
            {
                get
                {
                    return new CimModuleManifestFile(this.ModuleName + ".psd1", RemoteDiscoveryHelper.GetPropertyValue<byte[]>(this._baseObject, "moduleManifestFileData", new byte[0]));
                }
            }

            public IEnumerable<RemoteDiscoveryHelper.CimModuleFile> ModuleFiles
            {
                get
                {
                    return this._moduleFiles;
                }
            }

            public string ModuleName
            {
                get
                {
                    return Path.GetFileName(RemoteDiscoveryHelper.GetPropertyValue<string>(this._baseObject, "ModuleName", string.Empty));
                }
            }

            private class CimModuleImplementationFile : RemoteDiscoveryHelper.CimModuleFile
            {
                private readonly CimInstance _baseObject;

                internal CimModuleImplementationFile(CimInstance baseObject)
                {
                    this._baseObject = baseObject;
                }

                public override string FileName
                {
                    get
                    {
                        return Path.GetFileName(RemoteDiscoveryHelper.GetPropertyValue<string>(this._baseObject, "FileName", string.Empty));
                    }
                }

                internal override byte[] RawFileDataCore
                {
                    get
                    {
                        return RemoteDiscoveryHelper.GetPropertyValue<byte[]>(this._baseObject, "FileData", new byte[0]);
                    }
                }
            }

            private class CimModuleManifestFile : RemoteDiscoveryHelper.CimModuleFile
            {
                private readonly string _fileName;
                private readonly byte[] _rawFileData;

                internal CimModuleManifestFile(string fileName, byte[] rawFileData)
                {
                    this._fileName = fileName;
                    this._rawFileData = rawFileData;
                }

                public override string FileName
                {
                    get
                    {
                        return this._fileName;
                    }
                }

                internal override byte[] RawFileDataCore
                {
                    get
                    {
                        return this._rawFileData;
                    }
                }
            }

            private enum DiscoveredModuleType : ushort
            {
                Cim = 1,
                Unknown = 0
            }
        }

        internal abstract class CimModuleFile
        {
            private string _fileData;

            protected CimModuleFile()
            {
            }

            public RemoteDiscoveryHelper.CimFileCode FileCode
            {
                get
                {
                    if (this.FileName.EndsWith(".psd1", StringComparison.OrdinalIgnoreCase))
                    {
                        return RemoteDiscoveryHelper.CimFileCode.PsdV1;
                    }
                    if (this.FileName.EndsWith(".cdxml", StringComparison.OrdinalIgnoreCase))
                    {
                        return RemoteDiscoveryHelper.CimFileCode.CmdletizationV1;
                    }
                    if (this.FileName.EndsWith(".types.ps1xml", StringComparison.OrdinalIgnoreCase))
                    {
                        return RemoteDiscoveryHelper.CimFileCode.TypesV1;
                    }
                    if (this.FileName.EndsWith(".format.ps1xml", StringComparison.OrdinalIgnoreCase))
                    {
                        return RemoteDiscoveryHelper.CimFileCode.FormatV1;
                    }
                    return RemoteDiscoveryHelper.CimFileCode.Unknown;
                }
            }

            public string FileData
            {
                get
                {
                    if (this._fileData == null)
                    {
                        using (MemoryStream stream = new MemoryStream(this.RawFileData))
                        {
                            using (StreamReader reader = new StreamReader(stream, true))
                            {
                                this._fileData = reader.ReadToEnd();
                            }
                        }
                    }
                    return this._fileData;
                }
            }

            public abstract string FileName { get; }

            public byte[] RawFileData
            {
                get
                {
                    return this.RawFileDataCore.Skip<byte>(4).ToArray<byte>();
                }
            }

            internal abstract byte[] RawFileDataCore { get; }
        }
    }
}

