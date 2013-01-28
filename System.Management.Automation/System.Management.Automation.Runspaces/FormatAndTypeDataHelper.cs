namespace System.Management.Automation.Runspaces
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IO;
    using System.Management.Automation;
    using System.Management.Automation.Host;
    using System.Management.Automation.Internal;
    using System.Reflection;
    using System.Text;

    internal static class FormatAndTypeDataHelper
    {
        private const string CannotFindRegistryKey = "CannotFindRegistryKey";
        private const string CannotFindRegistryKeyPath = "CannotFindRegistryKeyPath";
        private const string DuplicateFile = "DuplicateFile";
        private const string EntryShouldBeMshXml = "EntryShouldBeMshXml";
        private const string FileNotFound = "FileNotFound";
        internal const string ValidationException = "ValidationException";

        private static string GetAndCheckFullFileName(string psSnapinName, HashSet<string> fullFileNameSet, string baseFolder, string baseFileName, Collection<string> independentErrors, ref bool needToRemoveEntry)
        {
            string path = Path.IsPathRooted(baseFileName) ? baseFileName : Path.Combine(baseFolder, baseFileName);
            if (!File.Exists(path))
            {
                string item = StringUtil.Format(TypesXmlStrings.FileNotFound, psSnapinName, path);
                independentErrors.Add(item);
                return null;
            }
            if (fullFileNameSet.Contains(path))
            {
                needToRemoveEntry = true;
                return null;
            }
            if (!path.EndsWith(".ps1xml", StringComparison.OrdinalIgnoreCase))
            {
                string str3 = StringUtil.Format(TypesXmlStrings.EntryShouldBeMshXml, psSnapinName, path);
                independentErrors.Add(str3);
                return null;
            }
            fullFileNameSet.Add(path);
            return path;
        }

        private static string GetBaseFolder(RunspaceConfiguration runspaceConfiguration, Collection<string> independentErrors)
        {
            string shellPathFromRegistry = CommandDiscovery.GetShellPathFromRegistry(runspaceConfiguration.ShellId);
            if (shellPathFromRegistry == null)
            {
                return Path.GetDirectoryName(PsUtils.GetMainModule(Process.GetCurrentProcess()).FileName);
            }
            if (OSHelper.IsWindows) shellPathFromRegistry = Path.GetDirectoryName(shellPathFromRegistry);
            if (!Directory.Exists(shellPathFromRegistry))
            {
                string directoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string item = StringUtil.Format(TypesXmlStrings.CannotFindRegistryKeyPath, new object[] { shellPathFromRegistry, Utils.GetRegistryConfigurationPath(runspaceConfiguration.ShellId), @"\Path", directoryName });
                independentErrors.Add(item);
                shellPathFromRegistry = directoryName;
            }
            return shellPathFromRegistry;
        }

        internal static Collection<PSSnapInTypeAndFormatErrors> GetFormatAndTypesErrors(RunspaceConfiguration runspaceConfiguration, PSHost host, IEnumerable configurationEntryCollection, RunspaceConfigurationCategory category, Collection<string> independentErrors, Collection<int> entryIndicesToRemove)
        {
            Collection<PSSnapInTypeAndFormatErrors> collection = new Collection<PSSnapInTypeAndFormatErrors>();
            string baseFolder = GetBaseFolder(runspaceConfiguration, independentErrors);
            HashSet<string> fullFileNameSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            int item = -1;
            foreach (object obj2 in configurationEntryCollection)
            {
                string fileName;
                string str3;
                bool flag;
                item++;
                if (category == RunspaceConfigurationCategory.Types)
                {
                    TypeConfigurationEntry entry = (TypeConfigurationEntry) obj2;
                    fileName = entry.FileName;
                    str3 = (entry.PSSnapIn == null) ? runspaceConfiguration.ShellId : entry.PSSnapIn.Name;
                    if (fileName != null)
                    {
                        goto Label_00DD;
                    }
                    collection.Add(new PSSnapInTypeAndFormatErrors(str3, entry.TypeData, entry.IsRemove));
                    continue;
                }
                FormatConfigurationEntry entry2 = (FormatConfigurationEntry) obj2;
                fileName = entry2.FileName;
                str3 = (entry2.PSSnapIn == null) ? runspaceConfiguration.ShellId : entry2.PSSnapIn.Name;
                if (fileName == null)
                {
                    collection.Add(new PSSnapInTypeAndFormatErrors(str3, entry2.FormatData));
                    continue;
                }
            Label_00DD:
                flag = false;
                string xmlFileListFileName = GetAndCheckFullFileName(str3, fullFileNameSet, baseFolder, fileName, independentErrors, ref flag);
                if (xmlFileListFileName == null)
                {
                    if (flag)
                    {
                        entryIndicesToRemove.Add(item);
                    }
                }
                else if (xmlFileListFileName.EndsWith("filelist.ps1xml", StringComparison.OrdinalIgnoreCase))
                {
                    bool flag2;
                    foreach (string str5 in runspaceConfiguration.TypeTable.ReadFiles(str3, xmlFileListFileName, independentErrors, runspaceConfiguration.AuthorizationManager, host, out flag2))
                    {
                        string fullPath = GetAndCheckFullFileName(str3, fullFileNameSet, baseFolder, str5, independentErrors, ref flag);
                        if (fullPath != null)
                        {
                            collection.Add(new PSSnapInTypeAndFormatErrors(str3, fullPath));
                        }
                    }
                }
                else
                {
                    collection.Add(new PSSnapInTypeAndFormatErrors(str3, xmlFileListFileName));
                }
            }
            return collection;
        }

        internal static void ThrowExceptionOnError(string errorId, Collection<string> errors, RunspaceConfigurationCategory category)
        {
            if (errors.Count != 0)
            {
                StringBuilder builder = new StringBuilder();
                builder.Append('\n');
                foreach (string str in errors)
                {
                    builder.Append(str);
                    builder.Append('\n');
                }
                string message = "";
                if (category == RunspaceConfigurationCategory.Types)
                {
                    message = StringUtil.Format(ExtendedTypeSystem.TypesXmlError, builder.ToString());
                }
                else if (category == RunspaceConfigurationCategory.Formats)
                {
                    message = StringUtil.Format(FormatAndOutXmlLoadingStrings.FormatLoadingErrors, builder.ToString());
                }
                RuntimeException exception = new RuntimeException(message);
                exception.SetErrorId(errorId);
                throw exception;
            }
        }

        internal static void ThrowExceptionOnError(string errorId, Collection<string> independentErrors, Collection<PSSnapInTypeAndFormatErrors> PSSnapinFilesCollection, RunspaceConfigurationCategory category)
        {
            Collection<string> collection = new Collection<string>();
            if (independentErrors != null)
            {
                foreach (string str in independentErrors)
                {
                    collection.Add(str);
                }
            }
            foreach (PSSnapInTypeAndFormatErrors errors in PSSnapinFilesCollection)
            {
                foreach (string str2 in errors.Errors)
                {
                    collection.Add(str2);
                }
            }
            if (collection.Count != 0)
            {
                StringBuilder builder = new StringBuilder();
                builder.Append('\n');
                foreach (string str3 in collection)
                {
                    builder.Append(str3);
                    builder.Append('\n');
                }
                string message = "";
                if (category == RunspaceConfigurationCategory.Types)
                {
                    message = StringUtil.Format(ExtendedTypeSystem.TypesXmlError, builder.ToString());
                }
                else if (category == RunspaceConfigurationCategory.Formats)
                {
                    message = StringUtil.Format(FormatAndOutXmlLoadingStrings.FormatLoadingErrors, builder.ToString());
                }
                RuntimeException exception = new RuntimeException(message);
                exception.SetErrorId(errorId);
                throw exception;
            }
        }
    }
}

