namespace System.Management.Automation.Remoting
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Runtime.InteropServices;

    internal static class DISCUtils
    {
        internal static Type ExecutionPolicyType;

        internal static ExternalScriptInfo GetScriptInfoForFile(ExecutionContext context, string fileName, out string scriptName)
        {
            scriptName = Path.GetFileName(fileName);
            ExternalScriptInfo commandInfo = new ExternalScriptInfo(scriptName, fileName, context);
            if (!scriptName.EndsWith(".psd1", StringComparison.OrdinalIgnoreCase))
            {
                context.AuthorizationManager.ShouldRunInternal(commandInfo, CommandOrigin.Internal, context.EngineHostInterface);
                CommandDiscovery.VerifyPSVersion(commandInfo);
                commandInfo.SignatureChecked = true;
            }
            return commandInfo;
        }

        internal static Hashtable LoadConfigFile(ExecutionContext context, ExternalScriptInfo scriptInfo)
        {
            object obj2;
            object variableValue = context.GetVariableValue(SpecialVariables.PSScriptRootVarPath);
            object newValue = context.GetVariableValue(SpecialVariables.PSCommandPathVarPath);
            try
            {
                context.SetVariable(SpecialVariables.PSScriptRootVarPath, Path.GetDirectoryName(scriptInfo.Definition));
                context.SetVariable(SpecialVariables.PSCommandPathVarPath, scriptInfo.Definition);
                obj2 = PSObject.Base(scriptInfo.ScriptBlock.InvokeReturnAsIs(new object[0]));
            }
            finally
            {
                context.SetVariable(SpecialVariables.PSScriptRootVarPath, variableValue);
                context.SetVariable(SpecialVariables.PSCommandPathVarPath, newValue);
            }
            return (obj2 as Hashtable);
        }

        internal static void ValidateAbsolutePath(SessionState state, string key, string[] paths, string filePath)
        {
            if (paths != null)
            {
                foreach (string str2 in paths)
                {
                    string str;
                    if (!state.Path.IsPSAbsolute(str2, out str))
                    {
                        throw new InvalidOperationException(StringUtil.Format(RemotingErrorIdStrings.DISCPathsMustBeAbsolute, new object[] { key, str2, filePath }));
                    }
                }
            }
        }

        internal static void ValidateAbsolutePaths(SessionState state, Hashtable table, string filePath)
        {
            if (table.ContainsKey(ConfigFileContants.TypesToProcess))
            {
                ValidateAbsolutePath(state, ConfigFileContants.TypesToProcess, DISCPowerShellConfiguration.TryGetStringArray(table[ConfigFileContants.TypesToProcess]), filePath);
            }
            if (table.ContainsKey(ConfigFileContants.FormatsToProcess))
            {
                ValidateAbsolutePath(state, ConfigFileContants.FormatsToProcess, DISCPowerShellConfiguration.TryGetStringArray(table[ConfigFileContants.FormatsToProcess]), filePath);
            }
            if (table.ContainsKey(ConfigFileContants.ScriptsToProcess))
            {
                ValidateAbsolutePath(state, ConfigFileContants.ScriptsToProcess, DISCPowerShellConfiguration.TryGetStringArray(table[ConfigFileContants.ScriptsToProcess]), filePath);
            }
        }

        internal static void ValidateExtensions(Hashtable table, string filePath)
        {
            if (table.ContainsKey(ConfigFileContants.TypesToProcess))
            {
                ValidatePS1XMLExtension(ConfigFileContants.TypesToProcess, DISCPowerShellConfiguration.TryGetStringArray(table[ConfigFileContants.TypesToProcess]), filePath);
            }
            if (table.ContainsKey(ConfigFileContants.FormatsToProcess))
            {
                ValidatePS1XMLExtension(ConfigFileContants.FormatsToProcess, DISCPowerShellConfiguration.TryGetStringArray(table[ConfigFileContants.FormatsToProcess]), filePath);
            }
            if (table.ContainsKey(ConfigFileContants.ScriptsToProcess))
            {
                ValidatePS1OrPSM1Extension(ConfigFileContants.ScriptsToProcess, DISCPowerShellConfiguration.TryGetStringArray(table[ConfigFileContants.ScriptsToProcess]), filePath);
            }
        }

        private static void ValidatePS1OrPSM1Extension(string key, string[] paths, string filePath)
        {
            if (paths != null)
            {
                foreach (string str in paths)
                {
                    try
                    {
                        string extension = Path.GetExtension(str);
                        if (!extension.Equals(".ps1", StringComparison.OrdinalIgnoreCase) && !extension.Equals(".psm1", StringComparison.OrdinalIgnoreCase))
                        {
                            throw new InvalidOperationException(StringUtil.Format(RemotingErrorIdStrings.DISCInvalidExtension, new object[] { key, extension, string.Join(", ", new string[] { ".ps1", ".psm1" }) }));
                        }
                    }
                    catch (ArgumentException exception)
                    {
                        throw new InvalidOperationException(StringUtil.Format(RemotingErrorIdStrings.ErrorParsingTheKeyInPSSessionConfigurationFile, key, filePath), exception);
                    }
                }
            }
        }

        private static void ValidatePS1XMLExtension(string key, string[] paths, string filePath)
        {
            if (paths != null)
            {
                foreach (string str in paths)
                {
                    try
                    {
                        string extension = Path.GetExtension(str);
                        if (!extension.Equals(".ps1xml", StringComparison.OrdinalIgnoreCase))
                        {
                            throw new InvalidOperationException(StringUtil.Format(RemotingErrorIdStrings.DISCInvalidExtension, new object[] { key, extension, ".ps1xml" }));
                        }
                    }
                    catch (ArgumentException exception)
                    {
                        throw new InvalidOperationException(StringUtil.Format(RemotingErrorIdStrings.ErrorParsingTheKeyInPSSessionConfigurationFile, key, filePath), exception);
                    }
                }
            }
        }

        internal static bool VerifyConfigTable(Hashtable table, PSCmdlet cmdlet, string path)
        {
            bool flag = false;
            foreach (DictionaryEntry entry in table)
            {
                if (!ConfigFileContants.IsValidKey(entry, cmdlet, path))
                {
                    return false;
                }
                if (entry.Key.ToString().Equals(ConfigFileContants.SchemaVersion, StringComparison.OrdinalIgnoreCase))
                {
                    flag = true;
                }
            }
            if (!flag)
            {
                cmdlet.WriteVerbose(StringUtil.Format(RemotingErrorIdStrings.DISCMissingSchemaVersion, path));
                return false;
            }
            try
            {
                ValidateAbsolutePaths(cmdlet.SessionState, table, path);
                ValidateExtensions(table, path);
            }
            catch (InvalidOperationException exception)
            {
                cmdlet.WriteVerbose(exception.Message);
                return false;
            }
            return true;
        }
    }
}

