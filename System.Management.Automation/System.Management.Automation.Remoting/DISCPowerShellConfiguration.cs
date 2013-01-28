namespace System.Management.Automation.Remoting
{
    using Microsoft.PowerShell.Commands;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Runspaces;

    internal sealed class DISCPowerShellConfiguration : PSSessionConfiguration
    {
        private string configFile;
        private Hashtable configHash;

        internal DISCPowerShellConfiguration(string configFile)
        {
            this.configFile = configFile;
            Runspace defaultRunspace = Runspace.DefaultRunspace;
            try
            {
                string str;
                Runspace.DefaultRunspace = RunspaceFactory.CreateRunspace();
                Runspace.DefaultRunspace.Open();
                ExternalScriptInfo scriptInfo = DISCUtils.GetScriptInfoForFile(Runspace.DefaultRunspace.ExecutionContext, configFile, out str);
                this.configHash = DISCUtils.LoadConfigFile(Runspace.DefaultRunspace.ExecutionContext, scriptInfo);
                Runspace.DefaultRunspace.Close();
            }
            catch (PSSecurityException exception)
            {
                PSInvalidOperationException exception2 = new PSInvalidOperationException(StringUtil.Format(RemotingErrorIdStrings.InvalidPSSessionConfigurationFilePath, configFile), exception);
                exception2.SetErrorId("InvalidPSSessionConfigurationFilePath");
                throw exception2;
            }
            finally
            {
                Runspace.DefaultRunspace = defaultRunspace;
            }
        }

        private SessionStateAliasEntry CreateSessionStateAliasEntry(Hashtable alias)
        {
            string str = TryGetValue(alias, ConfigFileContants.AliasNameToken);
            if (string.IsNullOrEmpty(str))
            {
                return null;
            }
            string str2 = TryGetValue(alias, ConfigFileContants.AliasValueToken);
            if (string.IsNullOrEmpty(str2))
            {
                return null;
            }
            string description = TryGetValue(alias, ConfigFileContants.AliasDescriptionToken);
            ScopedItemOptions none = ScopedItemOptions.None;
            string str4 = TryGetValue(alias, ConfigFileContants.AliasOptionsToken);
            if (!string.IsNullOrEmpty(str4))
            {
                none = (ScopedItemOptions) Enum.Parse(typeof(ScopedItemOptions), str4, true);
            }
            return new SessionStateAliasEntry(str, str2, description, none, SessionStateEntryVisibility.Public);
        }

        private SessionStateFunctionEntry CreateSessionStateFunctionEntry(Hashtable function)
        {
            string str = TryGetValue(function, ConfigFileContants.FunctionNameToken);
            if (string.IsNullOrEmpty(str))
            {
                return null;
            }
            string str2 = TryGetValue(function, ConfigFileContants.FunctionValueToken);
            if (string.IsNullOrEmpty(str2))
            {
                return null;
            }
            ScopedItemOptions none = ScopedItemOptions.None;
            string str3 = TryGetValue(function, ConfigFileContants.FunctionOptionsToken);
            if (!string.IsNullOrEmpty(str3))
            {
                none = (ScopedItemOptions) Enum.Parse(typeof(ScopedItemOptions), str3, true);
            }
            ScriptBlock scriptBlock = ScriptBlock.Create(str2);
            scriptBlock.LanguageMode = 0;
            return new SessionStateFunctionEntry(str, str2, none, SessionStateEntryVisibility.Private, scriptBlock, null);
        }

        private SessionStateVariableEntry CreateSessionStateVariableEntry(Hashtable variable)
        {
            string str = TryGetValue(variable, ConfigFileContants.VariableNameToken);
            if (string.IsNullOrEmpty(str))
            {
                return null;
            }
            string str2 = TryGetValue(variable, ConfigFileContants.VariableValueToken);
            if (string.IsNullOrEmpty(str2))
            {
                return null;
            }
            string description = TryGetValue(variable, ConfigFileContants.AliasDescriptionToken);
            ScopedItemOptions none = ScopedItemOptions.None;
            string str4 = TryGetValue(variable, ConfigFileContants.AliasOptionsToken);
            if (!string.IsNullOrEmpty(str4))
            {
                none = (ScopedItemOptions) Enum.Parse(typeof(ScopedItemOptions), str4, true);
            }
            return new SessionStateVariableEntry(str, str2, description, none, new Collection<Attribute>(), SessionStateEntryVisibility.Public);
        }

        public override InitialSessionState GetInitialSessionState(PSSenderInfo senderInfo)
        {
            InitialSessionState state = null;
            bool flag = false;
            string str = TryGetValue(this.configHash, ConfigFileContants.SessionType);
            SessionType type = SessionType.Default;
            bool flag2 = this.IsNonDefaultVisibiltySpecified(ConfigFileContants.VisibleCmdlets);
            bool flag3 = this.IsNonDefaultVisibiltySpecified(ConfigFileContants.VisibleFunctions);
            bool flag4 = this.IsNonDefaultVisibiltySpecified(ConfigFileContants.VisibleAliases);
            bool flag5 = this.IsNonDefaultVisibiltySpecified(ConfigFileContants.VisibleProviders);
            if (!string.IsNullOrEmpty(str))
            {
                type = (SessionType) Enum.Parse(typeof(SessionType), str, true);
                switch (type)
                {
                    case SessionType.Empty:
                        state = InitialSessionState.Create();
                        goto Label_00AD;

                    case SessionType.RestrictedRemoteServer:
                        state = InitialSessionState.CreateRestricted(SessionCapabilities.RemoteServer);
                        if (flag5)
                        {
                            InitialSessionState state2 = InitialSessionState.CreateDefault2();
                            state.Providers.Add(state2.Providers);
                        }
                        goto Label_00AD;
                }
                state = InitialSessionState.CreateDefault2();
            }
            else
            {
                state = InitialSessionState.CreateDefault2();
            }
        Label_00AD:
            if (this.configHash.ContainsKey(ConfigFileContants.AssembliesToLoad))
            {
                string[] strArray = TryGetStringArray(this.configHash[ConfigFileContants.AssembliesToLoad]);
                if (strArray != null)
                {
                    foreach (string str2 in strArray)
                    {
                        state.Assemblies.Add(new SessionStateAssemblyEntry(str2));
                    }
                }
            }
            if (this.configHash.ContainsKey(ConfigFileContants.ModulesToImport))
            {
                object[] objArray = TryGetObjectsOfType<object>(this.configHash[ConfigFileContants.ModulesToImport], new Type[] { typeof(string), typeof(Hashtable) });
                if ((this.configHash[ConfigFileContants.ModulesToImport] != null) && (objArray == null))
                {
                    PSInvalidOperationException exception = new PSInvalidOperationException(StringUtil.Format(RemotingErrorIdStrings.DISCTypeMustBeStringOrHashtableArray, ConfigFileContants.ModulesToImport));
                    exception.SetErrorId("InvalidModulesToImportKeyEntries");
                    throw exception;
                }
                if (objArray != null)
                {
                    Collection<ModuleSpecification> modules = new Collection<ModuleSpecification>();
                    foreach (object obj2 in objArray)
                    {
                        ModuleSpecification item = null;
                        string str4 = obj2 as string;
                        if (!string.IsNullOrEmpty(str4))
                        {
                            item = new ModuleSpecification(str4);
                        }
                        else
                        {
                            Hashtable moduleSpecification = obj2 as Hashtable;
                            if (moduleSpecification != null)
                            {
                                item = new ModuleSpecification(moduleSpecification);
                            }
                        }
                        if (item != null)
                        {
                            if (string.Equals(InitialSessionState.CoreModule, item.Name, StringComparison.OrdinalIgnoreCase))
                            {
                                if (type == SessionType.Empty)
                                {
                                    state.ImportCorePSSnapIn();
                                }
                            }
                            else
                            {
                                modules.Add(item);
                            }
                        }
                    }
                    state.ImportPSModule(modules);
                }
            }
            if (this.configHash.ContainsKey(ConfigFileContants.AliasDefinitions))
            {
                Hashtable[] hashtableArray = TryGetHashtableArray(this.configHash[ConfigFileContants.AliasDefinitions]);
                if (hashtableArray != null)
                {
                    foreach (Hashtable hashtable2 in hashtableArray)
                    {
                        SessionStateAliasEntry entry = this.CreateSessionStateAliasEntry(hashtable2);
                        if (entry != null)
                        {
                            state.Commands.Add(entry);
                        }
                    }
                }
            }
            if (this.configHash.ContainsKey(ConfigFileContants.FunctionDefinitions))
            {
                Hashtable[] hashtableArray2 = TryGetHashtableArray(this.configHash[ConfigFileContants.FunctionDefinitions]);
                if (hashtableArray2 != null)
                {
                    foreach (Hashtable hashtable3 in hashtableArray2)
                    {
                        SessionStateFunctionEntry entry2 = this.CreateSessionStateFunctionEntry(hashtable3);
                        if (entry2 != null)
                        {
                            state.Commands.Add(entry2);
                        }
                    }
                }
            }
            if (this.configHash.ContainsKey(ConfigFileContants.VariableDefinitions))
            {
                Hashtable[] hashtableArray3 = TryGetHashtableArray(this.configHash[ConfigFileContants.VariableDefinitions]);
                if (hashtableArray3 != null)
                {
                    foreach (Hashtable hashtable4 in hashtableArray3)
                    {
                        if (!hashtable4.ContainsKey(ConfigFileContants.VariableValueToken) || !(hashtable4[ConfigFileContants.VariableValueToken] is ScriptBlock))
                        {
                            SessionStateVariableEntry entry3 = this.CreateSessionStateVariableEntry(hashtable4);
                            if (entry3 != null)
                            {
                                state.Variables.Add(entry3);
                            }
                        }
                    }
                }
            }
            if (this.configHash.ContainsKey(ConfigFileContants.TypesToProcess))
            {
                string[] strArray2 = TryGetStringArray(this.configHash[ConfigFileContants.TypesToProcess]);
                if (strArray2 != null)
                {
                    foreach (string str5 in strArray2)
                    {
                        if (!string.IsNullOrEmpty(str5))
                        {
                            state.Types.Add(new SessionStateTypeEntry(str5));
                        }
                    }
                }
            }
            if (this.configHash.ContainsKey(ConfigFileContants.FormatsToProcess))
            {
                string[] strArray3 = TryGetStringArray(this.configHash[ConfigFileContants.FormatsToProcess]);
                if (strArray3 != null)
                {
                    foreach (string str6 in strArray3)
                    {
                        if (!string.IsNullOrEmpty(str6))
                        {
                            state.Formats.Add(new SessionStateFormatEntry(str6));
                        }
                    }
                }
            }
            if ((flag2 || flag3) || (flag4 || flag5))
            {
                flag = true;
            }
            if (flag)
            {
                state.Variables.Add(new SessionStateVariableEntry("PSModuleAutoLoadingPreference", PSModuleAutoLoadingPreference.None, string.Empty, ScopedItemOptions.None));
                if (type == SessionType.Default)
                {
                    state.ImportPSCoreModule(InitialSessionState.EngineModules.ToArray<string>());
                }
                if (!flag2)
                {
                    state.Commands.Remove("Import-Module", typeof(SessionStateCmdletEntry));
                }
                if (!flag4)
                {
                    state.Commands.Remove("ipmo", typeof(SessionStateAliasEntry));
                }
            }
            return state;
        }

        private bool IsNonDefaultVisibiltySpecified(string configFileKey)
        {
            if (!this.configHash.ContainsKey(configFileKey))
            {
                return false;
            }
            string[] strArray = TryGetStringArray(this.configHash[configFileKey]);
            return ((strArray != null) && (strArray.Length != 0));
        }

        internal static Hashtable[] TryGetHashtableArray(object hashObj)
        {
            Hashtable hashtable = hashObj as Hashtable;
            if (hashtable != null)
            {
                return new Hashtable[] { hashtable };
            }
            Hashtable[] hashtableArray = hashObj as Hashtable[];
            if (hashtableArray == null)
            {
                object[] objArray = hashObj as object[];
                if (objArray == null)
                {
                    return hashtableArray;
                }
                hashtableArray = new Hashtable[objArray.Length];
                for (int i = 0; i < hashtableArray.Length; i++)
                {
                    Hashtable hashtable2 = objArray[i] as Hashtable;
                    if (hashtable2 == null)
                    {
                        return null;
                    }
                    hashtableArray[i] = hashtable2;
                }
            }
            return hashtableArray;
        }

        internal static T[] TryGetObjectsOfType<T>(object hashObj, IEnumerable<Type> types) where T: class
        {
            object[] objs = hashObj as object[];
            if (objs == null)
            {
                object obj2 = hashObj;
                if (obj2 != null)
                {
                    foreach (Type type in types)
                    {
                        if (obj2.GetType().Equals(type))
                        {
                            return new T[] { (obj2 as T) };
                        }
                    }
                }
                return null;
            }
            T[] localArray = new T[objs.Length];
            for (int i = 0; i < objs.Length; i++)
            {
                int i1 = i;
                if (types.Any<Type>(type => objs[i1].GetType().Equals(type)))
                {
                    localArray[i] = objs[i] as T;
                }
                else
                {
                    return null;
                }
            }
            return localArray;
        }

        internal static string[] TryGetStringArray(object hashObj)
        {
            object[] objArray = hashObj as object[];
            if (objArray == null)
            {
                object obj2 = hashObj;
                if (obj2 != null)
                {
                    return new string[] { obj2.ToString() };
                }
                return null;
            }
            string[] strArray = new string[objArray.Length];
            for (int i = 0; i < objArray.Length; i++)
            {
                strArray[i] = objArray[i].ToString();
            }
            return strArray;
        }

        internal static string TryGetValue(Hashtable table, string key)
        {
            if (table.ContainsKey(key))
            {
                return table[key].ToString();
            }
            return string.Empty;
        }

        internal Hashtable ConfigHash
        {
            get
            {
                return this.configHash;
            }
        }
    }
}

