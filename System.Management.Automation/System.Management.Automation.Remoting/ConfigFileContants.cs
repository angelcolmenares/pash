namespace System.Management.Automation.Remoting
{
    using System;
    using System.Collections;
    using System.Management.Automation;
    using System.Management.Automation.Internal;

    internal static class ConfigFileContants
    {
        internal static readonly string AliasDefinitions = "AliasDefinitions";
        internal static readonly string AliasDescriptionToken = "Description";
        internal static readonly string AliasNameToken = "Name";
        internal static readonly string AliasOptionsToken = "Options";
        internal static readonly string AliasValueToken = "Value";
        internal static readonly string AssembliesToLoad = "AssembliesToLoad";
        internal static readonly string Author = "Author";
        internal static readonly string CompanyName = "CompanyName";
        internal static ConfigTypeEntry[] ConfigFileKeys = new ConfigTypeEntry[] { 
            new ConfigTypeEntry(SchemaVersion, new ConfigTypeEntry.TypeValidationCallback(ConfigFileContants.StringTypeValidationCallback)), new ConfigTypeEntry(Guid, new ConfigTypeEntry.TypeValidationCallback(ConfigFileContants.StringTypeValidationCallback)), new ConfigTypeEntry(Author, new ConfigTypeEntry.TypeValidationCallback(ConfigFileContants.StringTypeValidationCallback)), new ConfigTypeEntry(CompanyName, new ConfigTypeEntry.TypeValidationCallback(ConfigFileContants.StringTypeValidationCallback)), new ConfigTypeEntry(Copyright, new ConfigTypeEntry.TypeValidationCallback(ConfigFileContants.StringTypeValidationCallback)), new ConfigTypeEntry(Description, new ConfigTypeEntry.TypeValidationCallback(ConfigFileContants.StringTypeValidationCallback)), new ConfigTypeEntry(PowerShellVersion, new ConfigTypeEntry.TypeValidationCallback(ConfigFileContants.StringTypeValidationCallback)), new ConfigTypeEntry(SessionType, new ConfigTypeEntry.TypeValidationCallback(ConfigFileContants.ISSValidationCallback)), new ConfigTypeEntry(ModulesToImport, new ConfigTypeEntry.TypeValidationCallback(ConfigFileContants.StringOrHashtableArrayTypeValidationCallback)), new ConfigTypeEntry(AssembliesToLoad, new ConfigTypeEntry.TypeValidationCallback(ConfigFileContants.StringArrayTypeValidationCallback)), new ConfigTypeEntry(VisibleAliases, new ConfigTypeEntry.TypeValidationCallback(ConfigFileContants.StringArrayTypeValidationCallback)), new ConfigTypeEntry(VisibleCmdlets, new ConfigTypeEntry.TypeValidationCallback(ConfigFileContants.StringArrayTypeValidationCallback)), new ConfigTypeEntry(VisibleFunctions, new ConfigTypeEntry.TypeValidationCallback(ConfigFileContants.StringArrayTypeValidationCallback)), new ConfigTypeEntry(VisibleProviders, new ConfigTypeEntry.TypeValidationCallback(ConfigFileContants.StringArrayTypeValidationCallback)), new ConfigTypeEntry(AliasDefinitions, new ConfigTypeEntry.TypeValidationCallback(ConfigFileContants.AliasDefinitionsTypeValidationCallback)), new ConfigTypeEntry(FunctionDefinitions, new ConfigTypeEntry.TypeValidationCallback(ConfigFileContants.FunctionDefinitionsTypeValidationCallback)), 
            new ConfigTypeEntry(VariableDefinitions, new ConfigTypeEntry.TypeValidationCallback(ConfigFileContants.VariableDefinitionsTypeValidationCallback)), new ConfigTypeEntry(EnvironmentVariables, new ConfigTypeEntry.TypeValidationCallback(ConfigFileContants.HashtableTypeValiationCallback)), new ConfigTypeEntry(TypesToProcess, new ConfigTypeEntry.TypeValidationCallback(ConfigFileContants.StringArrayTypeValidationCallback)), new ConfigTypeEntry(FormatsToProcess, new ConfigTypeEntry.TypeValidationCallback(ConfigFileContants.StringArrayTypeValidationCallback)), new ConfigTypeEntry(LanguageMode, new ConfigTypeEntry.TypeValidationCallback(ConfigFileContants.LanugageModeValidationCallback)), new ConfigTypeEntry(ExecutionPolicy, new ConfigTypeEntry.TypeValidationCallback(ConfigFileContants.ExecutionPolicyValidationCallback)), new ConfigTypeEntry(ScriptsToProcess, new ConfigTypeEntry.TypeValidationCallback(ConfigFileContants.StringArrayTypeValidationCallback))
         };
        internal static readonly string Copyright = "Copyright";
        internal static readonly string Description = "Description";
        internal static readonly string EnvironmentVariables = "EnvironmentVariables";
        internal static readonly string ExecutionPolicy = "ExecutionPolicy";
        internal static readonly string FormatsToProcess = "FormatsToProcess";
        internal static readonly string FunctionDefinitions = "FunctionDefinitions";
        internal static readonly string FunctionNameToken = "Name";
        internal static readonly string FunctionOptionsToken = "Options";
        internal static readonly string FunctionValueToken = "ScriptBlock";
        internal static readonly string Guid = "GUID";
        internal static readonly string LanguageMode = "LanguageMode";
        internal static readonly string ModulesToImport = "ModulesToImport";
        internal static readonly string PowerShellVersion = "PowerShellVersion";
        internal static readonly string SchemaVersion = "SchemaVersion";
        internal static readonly string ScriptsToProcess = "ScriptsToProcess";
        internal static readonly string SessionType = "SessionType";
        internal static readonly string TypesToProcess = "TypesToProcess";
        internal static readonly string VariableDefinitions = "VariableDefinitions";
        internal static readonly string VariableNameToken = "Name";
        internal static readonly string VariableValueToken = "Value";
        internal static readonly string VisibleAliases = "VisibleAliases";
        internal static readonly string VisibleCmdlets = "VisibleCmdlets";
        internal static readonly string VisibleFunctions = "VisibleFunctions";
        internal static readonly string VisibleProviders = "VisibleProviders";

        private static bool AliasDefinitionsTypeValidationCallback(string key, object obj, PSCmdlet cmdlet, string path)
        {
            Hashtable[] hashtableArray = DISCPowerShellConfiguration.TryGetHashtableArray(obj);
            if (hashtableArray == null)
            {
                cmdlet.WriteVerbose(StringUtil.Format(RemotingErrorIdStrings.DISCTypeMustBeHashtableArray, key, path));
                return false;
            }
            foreach (Hashtable hashtable in hashtableArray)
            {
                if (!hashtable.ContainsKey(AliasNameToken))
                {
                    cmdlet.WriteVerbose(StringUtil.Format(RemotingErrorIdStrings.DISCTypeMustContainKey, new object[] { key, AliasNameToken, path }));
                    return false;
                }
                if (!hashtable.ContainsKey(AliasValueToken))
                {
                    cmdlet.WriteVerbose(StringUtil.Format(RemotingErrorIdStrings.DISCTypeMustContainKey, new object[] { key, AliasValueToken, path }));
                    return false;
                }
                foreach (string str in hashtable.Keys)
                {
                    if ((!string.Equals(str, AliasNameToken, StringComparison.OrdinalIgnoreCase) && !string.Equals(str, AliasValueToken, StringComparison.OrdinalIgnoreCase)) && (!string.Equals(str, AliasDescriptionToken, StringComparison.OrdinalIgnoreCase) && !string.Equals(str, AliasOptionsToken, StringComparison.OrdinalIgnoreCase)))
                    {
                        cmdlet.WriteVerbose(StringUtil.Format(RemotingErrorIdStrings.DISCTypeContainsInvalidKey, new object[] { str, key, path }));
                        return false;
                    }
                }
            }
            return true;
        }

        private static bool ExecutionPolicyValidationCallback(string key, object obj, PSCmdlet cmdlet, string path)
        {
            string str = obj as string;
            if (!string.IsNullOrEmpty(str))
            {
                try
                {
                    Enum.Parse(DISCUtils.ExecutionPolicyType, str, true);
                    return true;
                }
                catch (ArgumentException)
                {
                }
            }
            cmdlet.WriteVerbose(StringUtil.Format(RemotingErrorIdStrings.DISCTypeMustBeValidEnum, new object[] { key, DISCUtils.ExecutionPolicyType.FullName, LanguagePrimitives.EnumSingleTypeConverter.EnumValues(DISCUtils.ExecutionPolicyType), path }));
            return false;
        }

        private static bool FunctionDefinitionsTypeValidationCallback(string key, object obj, PSCmdlet cmdlet, string path)
        {
            Hashtable[] hashtableArray = DISCPowerShellConfiguration.TryGetHashtableArray(obj);
            if (hashtableArray == null)
            {
                cmdlet.WriteVerbose(StringUtil.Format(RemotingErrorIdStrings.DISCTypeMustBeHashtableArray, key, path));
                return false;
            }
            foreach (Hashtable hashtable in hashtableArray)
            {
                if (!hashtable.ContainsKey(FunctionNameToken))
                {
                    cmdlet.WriteVerbose(StringUtil.Format(RemotingErrorIdStrings.DISCTypeMustContainKey, new object[] { key, FunctionNameToken, path }));
                    return false;
                }
                if (!hashtable.ContainsKey(FunctionValueToken))
                {
                    cmdlet.WriteVerbose(StringUtil.Format(RemotingErrorIdStrings.DISCTypeMustContainKey, new object[] { key, FunctionValueToken, path }));
                    return false;
                }
                if (!(hashtable[FunctionValueToken] is ScriptBlock))
                {
                    cmdlet.WriteVerbose(StringUtil.Format(RemotingErrorIdStrings.DISCKeyMustBeScriptBlock, new object[] { FunctionValueToken, key, path }));
                    return false;
                }
                foreach (string str in hashtable.Keys)
                {
                    if ((!string.Equals(str, FunctionNameToken, StringComparison.OrdinalIgnoreCase) && !string.Equals(str, FunctionValueToken, StringComparison.OrdinalIgnoreCase)) && !string.Equals(str, FunctionOptionsToken, StringComparison.OrdinalIgnoreCase))
                    {
                        cmdlet.WriteVerbose(StringUtil.Format(RemotingErrorIdStrings.DISCTypeContainsInvalidKey, new object[] { str, key, path }));
                        return false;
                    }
                }
            }
            return true;
        }

        private static bool HashtableTypeValiationCallback(string key, object obj, PSCmdlet cmdlet, string path)
        {
            if (obj is Hashtable)
            {
                return true;
            }
            cmdlet.WriteVerbose(StringUtil.Format(RemotingErrorIdStrings.DISCTypeMustBeHashtable, key, path));
            return false;
        }

        private static bool ISSValidationCallback(string key, object obj, PSCmdlet cmdlet, string path)
        {
            string str = obj as string;
            if (!string.IsNullOrEmpty(str))
            {
                try
                {
                    Enum.Parse(typeof(System.Management.Automation.Remoting.SessionType), str, true);
                    return true;
                }
                catch (ArgumentException)
                {
                }
            }
            cmdlet.WriteVerbose(StringUtil.Format(RemotingErrorIdStrings.DISCTypeMustBeValidEnum, new object[] { key, typeof(System.Management.Automation.Remoting.SessionType).FullName, LanguagePrimitives.EnumSingleTypeConverter.EnumValues(typeof(System.Management.Automation.Remoting.SessionType)), path }));
            return false;
        }

        internal static bool IsValidKey(DictionaryEntry de, PSCmdlet cmdlet, string path)
        {
            bool flag = false;
            foreach (ConfigTypeEntry entry in ConfigFileKeys)
            {
                if (string.Equals(entry.Key, de.Key.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    flag = true;
                    if (entry.ValidationCallback(de.Key.ToString(), de.Value, cmdlet, path))
                    {
                        return true;
                    }
                }
            }
            if (!flag)
            {
                cmdlet.WriteVerbose(StringUtil.Format(RemotingErrorIdStrings.DISCInvalidKey, de.Key.ToString(), path));
            }
            return false;
        }

        private static bool LanugageModeValidationCallback(string key, object obj, PSCmdlet cmdlet, string path)
        {
            string str = obj as string;
            if (!string.IsNullOrEmpty(str))
            {
                try
                {
                    Enum.Parse(typeof(PSLanguageMode), str, true);
                    return true;
                }
                catch (ArgumentException)
                {
                }
            }
            cmdlet.WriteVerbose(StringUtil.Format(RemotingErrorIdStrings.DISCTypeMustBeValidEnum, new object[] { key, typeof(PSLanguageMode).FullName, LanguagePrimitives.EnumSingleTypeConverter.EnumValues(typeof(PSLanguageMode)), path }));
            return false;
        }

        private static bool StringArrayTypeValidationCallback(string key, object obj, PSCmdlet cmdlet, string path)
        {
            if (DISCPowerShellConfiguration.TryGetStringArray(obj) == null)
            {
                cmdlet.WriteVerbose(StringUtil.Format(RemotingErrorIdStrings.DISCTypeMustBeStringArray, key, path));
                return false;
            }
            return true;
        }

        private static bool StringOrHashtableArrayTypeValidationCallback(string key, object obj, PSCmdlet cmdlet, string path)
        {
            if (DISCPowerShellConfiguration.TryGetObjectsOfType<object>(obj, new Type[] { typeof(string), typeof(Hashtable) }) == null)
            {
                cmdlet.WriteVerbose(StringUtil.Format(RemotingErrorIdStrings.DISCTypeMustBeStringOrHashtableArrayInFile, key, path));
                return false;
            }
            return true;
        }

        private static bool StringTypeValidationCallback(string key, object obj, PSCmdlet cmdlet, string path)
        {
            if (obj is string)
            {
                return true;
            }
            cmdlet.WriteVerbose(StringUtil.Format(RemotingErrorIdStrings.DISCTypeMustBeString, key, path));
            return false;
        }

        private static bool VariableDefinitionsTypeValidationCallback(string key, object obj, PSCmdlet cmdlet, string path)
        {
            Hashtable[] hashtableArray = DISCPowerShellConfiguration.TryGetHashtableArray(obj);
            if (hashtableArray == null)
            {
                cmdlet.WriteVerbose(StringUtil.Format(RemotingErrorIdStrings.DISCTypeMustBeHashtableArray, key, path));
                return false;
            }
            foreach (Hashtable hashtable in hashtableArray)
            {
                if (!hashtable.ContainsKey(VariableNameToken))
                {
                    cmdlet.WriteVerbose(StringUtil.Format(RemotingErrorIdStrings.DISCTypeMustContainKey, new object[] { key, VariableNameToken, path }));
                    return false;
                }
                if (!hashtable.ContainsKey(VariableValueToken))
                {
                    cmdlet.WriteVerbose(StringUtil.Format(RemotingErrorIdStrings.DISCTypeMustContainKey, new object[] { key, VariableValueToken, path }));
                    return false;
                }
                foreach (string str in hashtable.Keys)
                {
                    if (!string.Equals(str, VariableNameToken, StringComparison.OrdinalIgnoreCase) && !string.Equals(str, VariableValueToken, StringComparison.OrdinalIgnoreCase))
                    {
                        cmdlet.WriteVerbose(StringUtil.Format(RemotingErrorIdStrings.DISCTypeContainsInvalidKey, new object[] { str, key, path }));
                        return false;
                    }
                }
            }
            return true;
        }
    }
}

