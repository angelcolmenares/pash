namespace System.Management.Automation
{
    using Microsoft.PowerShell.Commands;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Management.Automation.Runspaces;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Threading;

    internal class AnalysisCache
    {
        private const string AnalysisMutex = "PowerShell_CommandAnalysis_Lock";
        private const string CommandAnalysisFolder = @"Microsoft\Windows\PowerShell\CommandAnalysis\";
        private const string DataFileBase = "PowerShell_AnalysisCacheEntry_";
        private static bool disableDiskBasedCache = false;
        private const string IndexFile = "PowerShell_AnalysisCacheIndex";
        private static Dictionary<string, Dictionary<string, List<CommandTypes>>> itemCache = new Dictionary<string, Dictionary<string, List<CommandTypes>>>(StringComparer.OrdinalIgnoreCase);
        private static HashSet<string> modulesBeingAnalyzed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private static AnalysisCacheIndex savedCacheIndex = null;

        private static void Cache<T>(string basePath, string path, T value)
        {
            bool flag = false;
            ModuleIntrinsics.Tracer.WriteLine("Caching " + value + " to disk.", new object[0]);
            AnalysisCacheIndex cacheIndex = GetCacheIndex(basePath);
            if (cacheIndex == null)
            {
                ModuleIntrinsics.Tracer.WriteLine("Could not get cache index. Returning.", new object[0]);
            }
            else
            {
                AnalysisCacheIndexEntry entry;
                ModuleIntrinsics.Tracer.WriteLine("Got cache index.", new object[0]);
                if (!cacheIndex.Entries.TryGetValue(path, out entry))
                {
                    entry = new AnalysisCacheIndexEntry();
                    string str = Guid.NewGuid().ToString();
                    entry.Path = "PowerShell_AnalysisCacheEntry_" + str;
                    flag = true;
                    ModuleIntrinsics.Tracer.WriteLine("Item not already in cache. Caching to " + entry.Path + ", need to update index.", new object[0]);
                }
                DateTime lastWriteTime = new FileInfo(path).LastWriteTime;
                if (entry.LastWriteTime != lastWriteTime)
                {
                    ModuleIntrinsics.Tracer.WriteLine(string.Concat(new object[] { "LastWriteTime for ", path, " + has changed. Old: ", entry.LastWriteTime, ", new: ", lastWriteTime, ". Need to update index." }), new object[0]);
                    entry.LastWriteTime = lastWriteTime;
                    flag = true;
                }
                string str2 = entry.Path;
                ModuleIntrinsics.Tracer.WriteLine("Caching to " + str2, new object[0]);
                if (flag)
                {
                    cacheIndex = GetCacheIndexFromDisk(basePath);
                    cacheIndex.Entries[path] = entry;
                }
                try
                {
                    if (savedCacheIndex != null)
                    {
                        savedCacheIndex.Entries[path] = entry;
                    }
                    SerializeToFile(value, str2);
                }
                catch (IOException exception)
                {
                    ModuleIntrinsics.Tracer.WriteLine("Couldn't serialize file due to IOException - " + exception.ToString(), new object[0]);
                    disableDiskBasedCache = true;
                }
                catch (UnauthorizedAccessException exception2)
                {
                    ModuleIntrinsics.Tracer.WriteLine("Couldn't serialize file due to UnauthorizedAccessException - " + exception2.ToString(), new object[0]);
                    disableDiskBasedCache = true;
                }
                if (flag && !disableDiskBasedCache)
                {
                    ModuleIntrinsics.Tracer.WriteLine("Serializing index.", new object[0]);
                    SaveCacheIndex(cacheIndex);
                    savedCacheIndex = null;
                }
            }
        }

        internal static void CacheExportedCommands(PSModuleInfo module, bool force, System.Management.Automation.ExecutionContext context)
        {
            string basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Microsoft\Windows\PowerShell\CommandAnalysis\");
            ModuleIntrinsics.Tracer.WriteLine("Requested caching for " + module.Name + " at " + basePath, new object[0]);
            if (!force)
            {
                Dictionary<string, List<CommandTypes>> dictionary = GetExportedCommands(module.Path, true, context);
                if (((dictionary != null) && (module.ExportedCommands != null)) && (dictionary.Count == module.ExportedCommands.Count))
                {
                    ModuleIntrinsics.Tracer.WriteLine("Existing cached info up-to-date. Skipping.", new object[0]);
                    return;
                }
            }
            ModuleIntrinsics.Tracer.WriteLine("Obtaining mutex PowerShell_CommandAnalysis_Lock for caching.", new object[0]);
            using (Mutex mutex = new Mutex(false, "PowerShell_CommandAnalysis_Lock"))
            {
                mutex.WaitOne();
                try
                {
                    Dictionary<string, List<CommandTypes>> dictionary2 = new Dictionary<string, List<CommandTypes>>(StringComparer.OrdinalIgnoreCase);
                    if (module.ExportedCommands != null)
                    {
                        ModuleIntrinsics.Tracer.WriteLine("Caching " + module.ExportedCommands.Count + " commands", new object[0]);
                        foreach (CommandInfo info in module.ExportedCommands.Values)
                        {
                            if (!dictionary2.ContainsKey(info.Name))
                            {
                                ModuleIntrinsics.Tracer.WriteLine("Caching " + info.Name, new object[0]);
                                dictionary2[info.Name] = new List<CommandTypes>();
                            }
                            dictionary2[info.Name].Add(info.CommandType);
                        }
                        Cache<Dictionary<string, List<CommandTypes>>>(basePath, module.Path, dictionary2);
                        lock (itemCache)
                        {
                            itemCache[module.Path] = dictionary2;
                        }
                    }
                }
                finally
                {
                    ModuleIntrinsics.Tracer.WriteLine("Releasing mutex after caching.", new object[0]);
                    mutex.ReleaseMutex();
                }
            }
        }

        private static void CleanAnalysisCacheStore(AnalysisCacheIndex cacheIndex)
        {
            try
            {
                ModuleIntrinsics.Tracer.WriteLine("Entering CleanAnalysisCacheStore.", new object[0]);
                string str = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Microsoft\Windows\PowerShell\CommandAnalysis\");
                List<string> list = new List<string>();
                HashSet<string> set = new HashSet<string>();
                foreach (string str2 in cacheIndex.Entries.Keys)
                {
                    AnalysisCacheIndexEntry entry = cacheIndex.Entries[str2];
                    string path = Path.Combine(str, entry.Path);
                    ModuleIntrinsics.Tracer.WriteLine("Cache index contains " + path, new object[0]);
                    if (!File.Exists(str2))
                    {
                        ModuleIntrinsics.Tracer.WriteLine("Module + " + str2 + " no longer exists. Deleting its index entry.", new object[0]);
                        File.Delete(path);
                        list.Add(str2);
                    }
                    else
                    {
                        set.Add(path);
                    }
                }
                foreach (string str4 in list)
                {
                    cacheIndex.Entries.Remove(str4);
                }
                ModuleIntrinsics.Tracer.WriteLine("Searching for files with no cache entries.", new object[0]);
                foreach (string str5 in Directory.EnumerateFiles(str, "PowerShell_AnalysisCacheEntry_*"))
                {
                    if (!set.Contains(str5))
                    {
                        ModuleIntrinsics.Tracer.WriteLine("Found stale file: " + str5, new object[0]);
                        File.Delete(str5);
                    }
                }
                ModuleIntrinsics.Tracer.WriteLine("Saving cache index.", new object[0]);
                cacheIndex.LastMaintenance = DateTime.Now;
                SaveCacheIndex(cacheIndex);
            }
            catch (IOException exception)
            {
                ModuleIntrinsics.Tracer.WriteLine("Got an IO exception during cache maintenance: " + exception.ToString(), new object[0]);
                disableDiskBasedCache = true;
            }
            catch (UnauthorizedAccessException exception2)
            {
                ModuleIntrinsics.Tracer.WriteLine("Got an UnauthorizedAccessException during cache maintenance: " + exception2.ToString(), new object[0]);
                disableDiskBasedCache = true;
            }
        }

        private static T DeserializeFromFile<T>(string basePath, string path)
        {
            T local = default(T);
            if (disableDiskBasedCache)
            {
                ModuleIntrinsics.Tracer.WriteLine("Skipping deserialization from file: " + Path.Combine(basePath, path) + " - disk-based caching is diabled.", new object[0]);
                return local;
            }
            ModuleIntrinsics.Tracer.WriteLine("Deserializing " + typeof(T).FullName + " from file: " + Path.Combine(basePath, path), new object[0]);
            if (Directory.Exists(basePath))
            {
                using (FileStream stream = new FileStream(Path.Combine(basePath, path), FileMode.Open))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    local = (T) formatter.Deserialize(stream);
                }
            }
            ModuleIntrinsics.Tracer.WriteLine("Deserializing complete.", new object[0]);
            return local;
        }

        public static Dictionary<string, List<CommandTypes>> Get(string basePath, string path)
        {
            AnalysisCacheIndex cacheIndex = null;
            ModuleIntrinsics.Tracer.WriteLine("Getting analysis cache entry for " + path + ".", new object[0]);
            cacheIndex = GetCacheIndex(basePath);
            if (cacheIndex == null)
            {
                ModuleIntrinsics.Tracer.WriteLine("Could not get cache index. Returning.", new object[0]);
                return null;
            }
            AnalysisCacheIndexEntry entry = null;
            if (cacheIndex.Entries.TryGetValue(path, out entry))
            {
                ModuleIntrinsics.Tracer.WriteLine("Found cache entry for " + path, new object[0]);
                if (new FileInfo(path).LastWriteTime == entry.LastWriteTime)
                {
                    ModuleIntrinsics.Tracer.WriteLine("LastWriteTime is current.", new object[0]);
                    try
                    {
                        ModuleIntrinsics.Tracer.WriteLine("Deserializing from " + Path.Combine(basePath, entry.Path), new object[0]);
                        return DeserializeFromFile<Dictionary<string, List<CommandTypes>>>(basePath, entry.Path);
                    }
                    catch (Exception exception)
                    {
                        ModuleIntrinsics.Tracer.WriteLine("Got an exception deserializing: " + exception.ToString(), new object[0]);
                        CommandProcessorBase.CheckForSevereException(exception);
                        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("PSDisableModuleAutoloadingCacheMaintenance")))
                        {
                            ModuleIntrinsics.Tracer.WriteLine("Cleaning cache store.", new object[0]);
                            CleanAnalysisCacheStore(cacheIndex);
                        }
                        ModuleIntrinsics.Tracer.WriteLine("Returning NULL due to exception.", new object[0]);
                        return null;
                    }
                }
                ModuleIntrinsics.Tracer.WriteLine("Returning NULL - LastWriteTime does not match.", new object[0]);
                return null;
            }
            ModuleIntrinsics.Tracer.WriteLine("Returning NULL - not cached.", new object[0]);
            return null;
        }

        private static AnalysisCacheIndex GetCacheIndex(string basePath)
        {
            AnalysisCacheIndex savedCacheIndex = null;
            if (AnalysisCache.savedCacheIndex != null)
            {
                ModuleIntrinsics.Tracer.WriteLine("Found in-memory cache entry.", new object[0]);
                if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("PSDisableModuleAutoLoadingMemoryCache")))
                {
                    savedCacheIndex = AnalysisCache.savedCacheIndex;
                }
            }
            if (savedCacheIndex == null)
            {
                ModuleIntrinsics.Tracer.WriteLine("No in-memory entry. Getting cache index.", new object[0]);
                savedCacheIndex = GetCacheIndexFromDisk(basePath);
                AnalysisCache.savedCacheIndex = savedCacheIndex;
            }
            return savedCacheIndex;
        }

        public static AnalysisCacheIndex GetCacheIndexFromDisk(string basePath)
        {
            AnalysisCacheIndex cacheIndex = null;
            bool flag = false;
            try
            {
                ModuleIntrinsics.Tracer.WriteLine("Deserializing cache index from " + Path.Combine(basePath, "PowerShell_AnalysisCacheIndex"), new object[0]);
                cacheIndex = DeserializeFromFile<AnalysisCacheIndex>(basePath, "PowerShell_AnalysisCacheIndex");
            }
            catch (Exception exception)
            {
                ModuleIntrinsics.Tracer.WriteLine("Got an exception deserializing index: " + exception.ToString(), new object[0]);
                CommandProcessorBase.CheckForSevereException(exception);
                flag = true;
            }
            if (cacheIndex == null)
            {
                ModuleIntrinsics.Tracer.WriteLine("Creating new index, couldn't get one from disk.", new object[0]);
                cacheIndex = new AnalysisCacheIndex {
                    LastMaintenance = DateTime.Now
                };
            }
            if (cacheIndex.Entries == null)
            {
                cacheIndex.Entries = new Dictionary<string, AnalysisCacheIndexEntry>(StringComparer.OrdinalIgnoreCase);
            }
            if (!flag)
            {
                TimeSpan span = (TimeSpan) (DateTime.Now - cacheIndex.LastMaintenance);
                if (span.TotalDays <= 7.0)
                {
                    return cacheIndex;
                }
            }
            if (flag)
            {
                ModuleIntrinsics.Tracer.WriteLine("Cleaning analysis store because it was corrupted.", new object[0]);
            }
            else
            {
                ModuleIntrinsics.Tracer.WriteLine("Cleaning analysis store for its 7-day maintenance window. Last maintenance was " + cacheIndex.LastMaintenance, new object[0]);
            }
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("PSDisableModuleAutoloadingCacheMaintenance")))
            {
                CleanAnalysisCacheStore(cacheIndex);
            }
            return cacheIndex;
        }

        internal static Dictionary<string, List<CommandTypes>> GetExportedCommands(string modulePath, bool testOnly, System.Management.Automation.ExecutionContext context)
        {
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("PSDisableModuleAutoLoadingMemoryCache")))
            {
                AnalysisCacheIndexEntry entry = null;
                if ((itemCache.ContainsKey(modulePath) && (savedCacheIndex != null)) && savedCacheIndex.Entries.TryGetValue(modulePath, out entry))
                {
                    lock (itemCache)
                    {
                        if (itemCache.ContainsKey(modulePath))
                        {
                            DateTime lastWriteTime = new FileInfo(modulePath).LastWriteTime;
                            if ((lastWriteTime == entry.LastWriteTime) && (itemCache[modulePath] != null))
                            {
                                return itemCache[modulePath];
                            }
                            ModuleIntrinsics.Tracer.WriteLine(string.Concat(new object[] { "Cache entry for ", modulePath, " was out of date. Cached on ", entry.LastWriteTime, ", last updated on ", lastWriteTime, ". Re-analyzing." }), new object[0]);
                            itemCache.Remove(modulePath);
                        }
                    }
                }
            }
            string basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Microsoft\Windows\PowerShell\CommandAnalysis\");
            ModuleIntrinsics.Tracer.WriteLine("Entering mutex PowerShell_CommandAnalysis_Lock", new object[0]);
            Dictionary<string, List<CommandTypes>> dictionary = null;
            using (Mutex mutex = new Mutex(false, "PowerShell_CommandAnalysis_Lock"))
            {
                mutex.WaitOne();
                try
                {
                    string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(modulePath);
                    dictionary = Get(basePath, modulePath);
                    if (!testOnly && (dictionary == null))
                    {
                        try
                        {
                            if (modulesBeingAnalyzed.Contains(modulePath))
                            {
                                ModuleIntrinsics.Tracer.WriteLine(modulePath + " is already being analyzed. Exiting.", new object[0]);
                                return null;
                            }
                            ModuleIntrinsics.Tracer.WriteLine("Registering " + modulePath + " for analysis.", new object[0]);
                            modulesBeingAnalyzed.Add(modulePath);
                            CommandInfo commandInfo = new CmdletInfo("Get-Module", typeof(GetModuleCommand), null, null, context);
                            Command command = new Command(commandInfo);
                            ModuleIntrinsics.Tracer.WriteLine("Listing modules.", new object[0]);
                            PowerShell.Create(RunspaceMode.CurrentRunspace).AddCommand(command).AddParameter("List", true).AddParameter("Name", fileNameWithoutExtension).AddParameter("ErrorAction", ActionPreference.Ignore).AddParameter("WarningAction", ActionPreference.Ignore).AddParameter("Verbose", false).AddParameter("Debug", false).Invoke<PSModuleInfo>();
                        }
                        catch (Exception exception)
                        {
                            ModuleIntrinsics.Tracer.WriteLine("Module analysis generated an exception: " + exception.ToString(), new object[0]);
                            CommandProcessorBase.CheckForSevereException(exception);
                        }
                        finally
                        {
                            ModuleIntrinsics.Tracer.WriteLine("Unregistering " + modulePath + " for analysis.", new object[0]);
                            modulesBeingAnalyzed.Remove(modulePath);
                        }
                        dictionary = Get(basePath, modulePath);
                    }
                    if (dictionary != null)
                    {
                        lock (itemCache)
                        {
                            ModuleIntrinsics.Tracer.WriteLine("Caching " + dictionary.Count + " exported commands.", new object[0]);
                            itemCache[modulePath] = dictionary;
                            goto Label_037E;
                        }
                    }
                    ModuleIntrinsics.Tracer.WriteLine("Detected an error while retrieving exported commands.", new object[0]);
                }
                finally
                {
                    ModuleIntrinsics.Tracer.WriteLine("Releasing mutex.", new object[0]);
                    mutex.ReleaseMutex();
                }
            }
        Label_037E:
            if (dictionary != null)
            {
                ModuleIntrinsics.Tracer.WriteLine("Returning " + dictionary.Count + " exported commands.", new object[0]);
                return dictionary;
            }
            ModuleIntrinsics.Tracer.WriteLine("Returning NULL for exported commands", new object[0]);
            return dictionary;
        }

        private static void SaveCacheIndex(AnalysisCacheIndex index)
        {
            try
            {
                ModuleIntrinsics.Tracer.WriteLine("Serializing index to PowerShell_AnalysisCacheIndex", new object[0]);
                savedCacheIndex = index;
                SerializeToFile(index, "PowerShell_AnalysisCacheIndex");
            }
            catch (IOException exception)
            {
                ModuleIntrinsics.Tracer.WriteLine("Got an IO exception saving cache index: " + exception.ToString(), new object[0]);
                disableDiskBasedCache = true;
            }
            catch (UnauthorizedAccessException exception2)
            {
                ModuleIntrinsics.Tracer.WriteLine("Got an unauthorized access exception saving cache index: " + exception2.ToString(), new object[0]);
                disableDiskBasedCache = true;
            }
        }

        private static void SerializeToFile(object value, string path)
        {
            string str = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Microsoft\Windows\PowerShell\CommandAnalysis\");
            string str2 = Path.Combine(str, path);
            if (disableDiskBasedCache)
            {
                ModuleIntrinsics.Tracer.WriteLine("Skipping serialization of " + value.ToString() + " to file: " + str2 + " - disk-based caching is diabled.", new object[0]);
            }
            else
            {
                ModuleIntrinsics.Tracer.WriteLine("Serializing " + value.ToString() + " to file: " + str2, new object[0]);
                if (!Directory.Exists(str))
                {
                    ModuleIntrinsics.Tracer.WriteLine("Root directory does not exist. Creating.", new object[0]);
                    Directory.CreateDirectory(str);
                }
                using (FileStream stream = new FileStream(str2, FileMode.OpenOrCreate))
                {
                    new BinaryFormatter().Serialize(stream, value);
                    stream.Flush();
                }
                ModuleIntrinsics.Tracer.WriteLine("Serializing complete.", new object[0]);
            }
        }
    }
}

