namespace System.Management.Automation
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Reflection;
    using System.Resources;
    using System.Threading;

    internal static class ResourceManagerCache
    {
        private static bool DFT_monitorFailingResourceLookup = true;
        private static Dictionary<string, Dictionary<string, ResourceManager>> resourceManagerCache = new Dictionary<string, Dictionary<string, ResourceManager>>(StringComparer.OrdinalIgnoreCase);
        private static object syncRoot = new object();

        internal static string FormatResourceString(string baseName, string resourceId, params object[] args)
        {
            if (string.IsNullOrEmpty(baseName))
            {
                throw PSTraceSource.NewArgumentException("baseName");
            }
            if (string.IsNullOrEmpty(resourceId))
            {
                throw PSTraceSource.NewArgumentException("resourceId");
            }
            string format = GetResourceString(Assembly.GetCallingAssembly(), baseName, resourceId);
            string str2 = null;
            if (format != null)
            {
                str2 = string.Format(Thread.CurrentThread.CurrentCulture, format, args);
            }
            return str2;
        }

        internal static string FormatResourceString(Assembly assembly, string baseName, string resourceId, params object[] args)
        {
            if (assembly == null)
            {
                throw PSTraceSource.NewArgumentNullException("assembly");
            }
            if (string.IsNullOrEmpty(baseName))
            {
                throw PSTraceSource.NewArgumentException("baseName");
            }
            if (string.IsNullOrEmpty(resourceId))
            {
                throw PSTraceSource.NewArgumentException("resourceId");
            }
            string format = GetResourceString(assembly, baseName, resourceId);
            string str2 = null;
            if (format != null)
            {
                str2 = string.Format(Thread.CurrentThread.CurrentCulture, format, args);
            }
            return str2;
        }

        internal static string FormatResourceStringUsingCulture(CultureInfo currentUICulture, CultureInfo currentCulture, string baseName, string resourceId, params object[] args)
        {
            if (currentUICulture == null)
            {
                currentUICulture = Thread.CurrentThread.CurrentUICulture;
            }
            if (currentCulture == null)
            {
                currentCulture = Thread.CurrentThread.CurrentCulture;
            }
            if (string.IsNullOrEmpty(baseName))
            {
                throw PSTraceSource.NewArgumentException("baseName");
            }
            if (string.IsNullOrEmpty(resourceId))
            {
                throw PSTraceSource.NewArgumentException("resourceId");
            }
            string format = GetResourceStringForUICulture(Assembly.GetCallingAssembly(), baseName, resourceId, currentUICulture);
            string str2 = null;
            if (format != null)
            {
                str2 = string.Format(currentCulture, format, args);
            }
            return str2;
        }

        internal static string FormatResourceStringUsingCulture(CultureInfo currentUICulture, CultureInfo currentCulture, Assembly assembly, string baseName, string resourceId, params object[] args)
        {
            if (currentUICulture == null)
            {
                currentUICulture = Thread.CurrentThread.CurrentUICulture;
            }
            if (currentCulture == null)
            {
                currentCulture = Thread.CurrentThread.CurrentCulture;
            }
            if (assembly == null)
            {
                throw PSTraceSource.NewArgumentNullException("assembly");
            }
            if (string.IsNullOrEmpty(baseName))
            {
                throw PSTraceSource.NewArgumentException("baseName");
            }
            if (string.IsNullOrEmpty(resourceId))
            {
                throw PSTraceSource.NewArgumentException("resourceId");
            }
            string format = GetResourceStringForUICulture(assembly, baseName, resourceId, currentUICulture);
            string str2 = null;
            if (format != null)
            {
                str2 = string.Format(currentCulture, format, args);
            }
            return str2;
        }

        internal static ResourceManager GetResourceManager(string baseName)
        {
            if (string.IsNullOrEmpty(baseName))
            {
                throw PSTraceSource.NewArgumentException("baseName");
            }
            return GetResourceManager(Assembly.GetCallingAssembly(), baseName);
        }

        internal static ResourceManager GetResourceManager(Assembly assembly, string baseName)
        {
            if (assembly == null)
            {
                throw PSTraceSource.NewArgumentNullException("assembly");
            }
            if (string.IsNullOrEmpty(baseName))
            {
                throw PSTraceSource.NewArgumentException("baseName");
            }
            baseName = assembly.GetName().Name + "." + baseName;
            ResourceManager manager = null;
            Dictionary<string, ResourceManager> dictionary = null;
            lock (syncRoot)
            {
                if (resourceManagerCache.ContainsKey(assembly.Location))
                {
                    dictionary = resourceManagerCache[assembly.Location];
                    if ((dictionary != null) && dictionary.ContainsKey(baseName))
                    {
                        manager = dictionary[baseName];
                    }
                }
            }
            if (manager == null)
            {
                manager = InitRMWithAssembly(baseName, assembly, null);
                if (dictionary != null)
                {
                    lock (syncRoot)
                    {
                        dictionary[baseName] = manager;
                        return manager;
                    }
                }
                Dictionary<string, ResourceManager> dictionary2 = new Dictionary<string, ResourceManager>();
                dictionary2[baseName] = manager;
                lock (syncRoot)
                {
                    resourceManagerCache[assembly.Location] = dictionary2;
                }
            }
            return manager;
        }

        internal static string GetResourceString(string baseName, string resourceId)
        {
            return GetResourceString(Assembly.GetCallingAssembly(), baseName, resourceId);
        }

        internal static string GetResourceString(Assembly assembly, string baseName, string resourceId)
        {
            if (assembly == null)
            {
                throw PSTraceSource.NewArgumentNullException("assembly");
            }
            if (string.IsNullOrEmpty(baseName))
            {
                throw PSTraceSource.NewArgumentException("baseName");
            }
            if (string.IsNullOrEmpty(resourceId))
            {
                throw PSTraceSource.NewArgumentException("resourceId");
            }
            string str = GetResourceManager(assembly, baseName).GetString(resourceId);
            if (string.IsNullOrEmpty(str))
            {
                bool flag1 = DFT_monitorFailingResourceLookup;
            }
            return str;
        }

        internal static string GetResourceStringForUICulture(string baseName, string resourceId, CultureInfo currentUICulture)
        {
            if (string.IsNullOrEmpty(baseName))
            {
                throw PSTraceSource.NewArgumentException("baseName");
            }
            if (string.IsNullOrEmpty(resourceId))
            {
                throw PSTraceSource.NewArgumentException("resourceId");
            }
            if (currentUICulture == null)
            {
                currentUICulture = Thread.CurrentThread.CurrentUICulture;
            }
            string str = GetResourceManager(Assembly.GetCallingAssembly(), baseName).GetString(resourceId, currentUICulture);
            if (string.IsNullOrEmpty(str))
            {
                bool flag1 = DFT_monitorFailingResourceLookup;
            }
            return str;
        }

        internal static string GetResourceStringForUICulture(Assembly assembly, string baseName, string resourceId, CultureInfo currentUICulture)
        {
            if (assembly == null)
            {
                throw PSTraceSource.NewArgumentNullException("assembly");
            }
            if (string.IsNullOrEmpty(baseName))
            {
                throw PSTraceSource.NewArgumentException("baseName");
            }
            if (string.IsNullOrEmpty(resourceId))
            {
                throw PSTraceSource.NewArgumentException("resourceId");
            }
            if (currentUICulture == null)
            {
                currentUICulture = Thread.CurrentThread.CurrentUICulture;
            }
            string str = GetResourceManager(assembly, baseName).GetString(resourceId, currentUICulture);
            if (string.IsNullOrEmpty(str))
            {
                bool flag1 = DFT_monitorFailingResourceLookup;
            }
            return str;
        }

        private static ResourceManager InitRMWithAssembly(string baseName, Assembly assemblyToUse, Type usingResourceSet)
        {
            if (((usingResourceSet != null) && (baseName != null)) && (assemblyToUse != null))
            {
                return new ResourceManager(baseName, assemblyToUse, usingResourceSet);
            }
            if (((usingResourceSet != null) && (baseName == null)) && (assemblyToUse == null))
            {
                return new ResourceManager(usingResourceSet);
            }
            if (((usingResourceSet != null) || (baseName == null)) || (assemblyToUse == null))
            {
                throw PSTraceSource.NewArgumentException("assemblyToUse");
            }
            return new ResourceManager(baseName, assemblyToUse);
        }

        internal static bool DFT_DoMonitorFailingResourceLookup
        {
            get
            {
                return DFT_monitorFailingResourceLookup;
            }
            set
            {
                DFT_monitorFailingResourceLookup = value;
            }
        }
    }
}

