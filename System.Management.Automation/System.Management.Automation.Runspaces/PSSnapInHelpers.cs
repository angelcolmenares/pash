namespace System.Management.Automation.Runspaces
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Provider;
    using System.Reflection;
    using System.Runtime.InteropServices;

    internal static class PSSnapInHelpers
    {
        private static Lazy<ConcurrentDictionary<Assembly, bool>> _assembliesWithModuleInitializerCache = new Lazy<ConcurrentDictionary<Assembly, bool>>();
        private static Lazy<ConcurrentDictionary<Assembly, Dictionary<string, SessionStateCmdletEntry>>> _cmdletCache = new Lazy<ConcurrentDictionary<Assembly, Dictionary<string, SessionStateCmdletEntry>>>();
        private static Lazy<ConcurrentDictionary<Assembly, Dictionary<string, SessionStateProviderEntry>>> _providerCache = new Lazy<ConcurrentDictionary<Assembly, Dictionary<string, SessionStateProviderEntry>>>();
        private static PSTraceSource _PSSnapInTracer = PSTraceSource.GetTracer("PSSnapInLoadUnload", "Loading and unloading mshsnapins", false);

        internal static void AnalyzePSSnapInAssembly(Assembly assembly, string name, PSSnapInInfo psSnapInInfo, PSModuleInfo moduleInfo, bool isModuleLoad, out Dictionary<string, SessionStateCmdletEntry> cmdlets, out Dictionary<string, SessionStateProviderEntry> providers, out string helpFile)
        {
            Type[] assemblyTypes;
            helpFile = null;
            if (assembly == null)
            {
                throw new ArgumentNullException("assembly");
            }
            cmdlets = null;
            providers = null;
            if (_cmdletCache.Value.ContainsKey(assembly))
            {
                cmdlets = new Dictionary<string, SessionStateCmdletEntry>(_cmdletCache.Value.Count, StringComparer.OrdinalIgnoreCase);
                Dictionary<string, SessionStateCmdletEntry> dictionary = _cmdletCache.Value[assembly];
                foreach (string str in dictionary.Keys)
                {
                    SessionStateCmdletEntry entry = dictionary[str];
                    if ((entry.PSSnapIn == null) && (psSnapInInfo != null))
                    {
                        entry.SetPSSnapIn(psSnapInInfo);
                    }
                    SessionStateCmdletEntry entry2 = (SessionStateCmdletEntry) entry.Clone();
                    cmdlets[str] = entry2;
                }
            }
            if (_providerCache.Value.ContainsKey(assembly))
            {
                providers = new Dictionary<string, SessionStateProviderEntry>(_providerCache.Value.Count, StringComparer.OrdinalIgnoreCase);
                Dictionary<string, SessionStateProviderEntry> dictionary2 = _providerCache.Value[assembly];
                foreach (string str2 in dictionary2.Keys)
                {
                    SessionStateProviderEntry entry3 = dictionary2[str2];
                    if ((entry3.PSSnapIn == null) && (psSnapInInfo != null))
                    {
                        entry3.SetPSSnapIn(psSnapInInfo);
                    }
                    SessionStateProviderEntry entry4 = (SessionStateProviderEntry) entry3.Clone();
                    providers[str2] = entry4;
                }
            }
            if ((cmdlets != null) || (providers != null))
            {
                if (!_assembliesWithModuleInitializerCache.Value.ContainsKey(assembly))
                {
                    _PSSnapInTracer.WriteLine("Returning cached cmdlet and provider entries for {0}", new object[] { assembly.Location });
                }
                else
                {
                    _PSSnapInTracer.WriteLine("Executing IModuleAssemblyInitializer.Import for {0}", new object[] { assembly.Location });
                    assemblyTypes = GetAssemblyTypes(assembly, name);
                    ExecuteModuleInitializer(assembly, assemblyTypes, isModuleLoad);
                }
            }
            else
            {
                _PSSnapInTracer.WriteLine("Analyzing assembly {0} for cmdlet and providers", new object[] { assembly.Location });
                helpFile = GetHelpFile(assembly.Location);
                assemblyTypes = GetAssemblyTypes(assembly, name);
                ExecuteModuleInitializer(assembly, assemblyTypes, isModuleLoad);
                Type type = null;
                Type type2 = null;
                foreach (Type type3 in assemblyTypes)
                {
                    if ((type3.IsPublic || type3.IsNestedPublic) && !type3.IsAbstract)
                    {
                        if (IsCmdletClass(type3) && HasDefaultConstructor(type3))
                        {
                            type = type3;
                            CmdletAttribute customAttribute = GetCustomAttribute<CmdletAttribute>(type3);
                            if (customAttribute != null)
                            {
                                string cmdletName = GetCmdletName(customAttribute);
                                if (!string.IsNullOrEmpty(cmdletName))
                                {
                                    if ((cmdlets != null) && cmdlets.ContainsKey(cmdletName))
                                    {
                                        string errorMessageFormat = StringUtil.Format(ConsoleInfoErrorStrings.PSSnapInDuplicateCmdlets, cmdletName, name);
                                        _PSSnapInTracer.TraceError(errorMessageFormat, new object[0]);
                                        throw new PSSnapInException(name, errorMessageFormat);
                                    }
                                    SessionStateCmdletEntry entry5 = new SessionStateCmdletEntry(cmdletName, type3, helpFile);
                                    if (psSnapInInfo != null)
                                    {
                                        entry5.SetPSSnapIn(psSnapInInfo);
                                    }
                                    if (cmdlets == null)
                                    {
                                        cmdlets = new Dictionary<string, SessionStateCmdletEntry>(StringComparer.OrdinalIgnoreCase);
                                    }
                                    cmdlets.Add(cmdletName, entry5);
                                    _PSSnapInTracer.WriteLine("{0} from type {1} is added as a cmdlet. ", new object[] { cmdletName, type3.FullName });
                                }
                            }
                        }
                        else if (IsProviderClass(type3) && HasDefaultConstructor(type3))
                        {
                            type2 = type3;
                            CmdletProviderAttribute providerAttribute = GetCustomAttribute<CmdletProviderAttribute>(type3);
                            if (providerAttribute != null)
                            {
                                string providerName = GetProviderName(providerAttribute);
                                if (!string.IsNullOrEmpty(providerName))
                                {
                                    if ((providers != null) && providers.ContainsKey(providerName))
                                    {
                                        string str6 = StringUtil.Format(ConsoleInfoErrorStrings.PSSnapInDuplicateProviders, providerName, psSnapInInfo.Name);
                                        _PSSnapInTracer.TraceError(str6, new object[0]);
                                        throw new PSSnapInException(psSnapInInfo.Name, str6);
                                    }
                                    SessionStateProviderEntry entry6 = new SessionStateProviderEntry(providerName, type3, helpFile);
                                    entry6.SetPSSnapIn(psSnapInInfo);
                                    if (moduleInfo != null)
                                    {
                                        entry6.SetModule(moduleInfo);
                                    }
                                    if (providers == null)
                                    {
                                        providers = new Dictionary<string, SessionStateProviderEntry>(StringComparer.OrdinalIgnoreCase);
                                    }
                                    providers.Add(providerName, entry6);
                                    _PSSnapInTracer.WriteLine("{0} from type {1} is added as a provider. ", new object[] { providerName, type3.FullName });
                                }
                            }
                        }
                    }
                }
                if (((providers == null) || (providers.Count == 0)) && ((cmdlets == null) || (cmdlets.Count == 0)))
                {
                    try
                    {
                        if (type != null)
                        {
                            ConstructorInfo constructor = type.GetConstructor(Type.EmptyTypes);
                            if (constructor != null)
                            {
                                constructor.Invoke(null);
                            }
                        }
                        if (type2 != null)
                        {
                            ConstructorInfo info2 = type2.GetConstructor(Type.EmptyTypes);
                            if (info2 != null)
                            {
                                info2.Invoke(null);
                            }
                        }
                    }
                    catch (TargetInvocationException exception)
                    {
                        throw exception.InnerException;
                    }
                }
                if (cmdlets != null)
                {
                    Dictionary<string, SessionStateCmdletEntry> dictionary3 = new Dictionary<string, SessionStateCmdletEntry>(cmdlets.Count, StringComparer.OrdinalIgnoreCase);
                    foreach (KeyValuePair<string, SessionStateCmdletEntry> pair in cmdlets)
                    {
                        dictionary3[pair.Key] = (SessionStateCmdletEntry) pair.Value.Clone();
                    }
                    _cmdletCache.Value[assembly] = dictionary3;
                }
                if (providers != null)
                {
                    Dictionary<string, SessionStateProviderEntry> dictionary4 = new Dictionary<string, SessionStateProviderEntry>(providers.Count, StringComparer.OrdinalIgnoreCase);
                    foreach (KeyValuePair<string, SessionStateProviderEntry> pair2 in providers)
                    {
                        dictionary4[pair2.Key] = (SessionStateProviderEntry) pair2.Value.Clone();
                    }
                    _providerCache.Value[assembly] = providers;
                }
            }
        }

        private static void ExecuteModuleInitializer(Assembly assembly, Type[] assemblyTypes, bool isModuleLoad)
        {
            foreach (Type type in assemblyTypes)
            {
                if ((type.IsPublic || type.IsNestedPublic) && ((!type.IsAbstract && isModuleLoad) && (typeof(IModuleAssemblyInitializer).IsAssignableFrom(type) && !type.Equals(typeof(IModuleAssemblyInitializer)))))
                {
                    _assembliesWithModuleInitializerCache.Value[assembly] = true;
                    (Activator.CreateInstance(type, true) as IModuleAssemblyInitializer).OnImport();
                }
            }
        }

        private static Type[] GetAssemblyTypes(Assembly assembly, string name)
        {
            Type[] exportedTypes = null;
            try
            {
                exportedTypes = assembly.GetExportedTypes();
            }
            catch (ReflectionTypeLoadException exception)
            {
                string errorMessageFormat = exception.Message + "\nLoader Exceptions: \n";
                if (exception.LoaderExceptions != null)
                {
                    foreach (Exception exception2 in exception.LoaderExceptions)
                    {
                        errorMessageFormat = errorMessageFormat + "\n" + exception2.Message;
                    }
                }
                _PSSnapInTracer.TraceError(errorMessageFormat, new object[0]);
                throw new PSSnapInException(name, errorMessageFormat);
            }
            return exportedTypes;
        }

        private static string GetCmdletName(CmdletAttribute cmdletAttribute)
        {
            string verbName = cmdletAttribute.VerbName;
            string nounName = cmdletAttribute.NounName;
            return (verbName + "-" + nounName);
        }

        private static T GetCustomAttribute<T>(Type decoratedType) where T: Attribute
        {
            object[] customAttributes = decoratedType.GetCustomAttributes(typeof(T), false);
            if (customAttributes.Length == 0)
            {
                return default(T);
            }
            return (T) customAttributes[0];
        }

        private static string GetHelpFile(string assemblyPath)
        {
            return (Path.GetFileName(assemblyPath) + "-Help.xml");
        }

        private static string GetProviderName(CmdletProviderAttribute providerAttribute)
        {
            return providerAttribute.ProviderName;
        }

        private static bool HasDefaultConstructor(Type type)
        {
            return (type.GetConstructor(Type.EmptyTypes) != null);
        }

        private static bool IsCmdletClass(Type type)
        {
            if (type == null)
            {
                return false;
            }
            return type.IsSubclassOf(typeof(Cmdlet));
        }

        internal static bool IsModuleAssemblyInitializerClass(Type type)
        {
            if (type == null)
            {
                return false;
            }
            return type.IsSubclassOf(typeof(IModuleAssemblyInitializer));
        }

        private static bool IsProviderClass(Type type)
        {
            if (type == null)
            {
                return false;
            }
            return type.IsSubclassOf(typeof(CmdletProvider));
        }

        internal static Assembly LoadPSSnapInAssembly(PSSnapInInfo psSnapInInfo, out Dictionary<string, SessionStateCmdletEntry> cmdlets, out Dictionary<string, SessionStateProviderEntry> providers)
        {
            Assembly assembly = null;
            cmdlets = null;
            providers = null;
            _PSSnapInTracer.WriteLine("Loading assembly from GAC. Assembly Name: {0}", new object[] { psSnapInInfo.AssemblyName });
            try
            {
                assembly = Assembly.Load(psSnapInInfo.AssemblyName);
            }
            catch (FileLoadException exception)
            {
                _PSSnapInTracer.TraceWarning("Not able to load assembly {0}: {1}", new object[] { psSnapInInfo.AssemblyName, exception.Message });
            }
            catch (BadImageFormatException exception2)
            {
                _PSSnapInTracer.TraceWarning("Not able to load assembly {0}: {1}", new object[] { psSnapInInfo.AssemblyName, exception2.Message });
            }
            catch (FileNotFoundException exception3)
            {
                _PSSnapInTracer.TraceWarning("Not able to load assembly {0}: {1}", new object[] { psSnapInInfo.AssemblyName, exception3.Message });
            }
            if (assembly == null)
            {
                _PSSnapInTracer.WriteLine("Loading assembly from path: {0}", new object[] { psSnapInInfo.AssemblyName });
                try
                {
                    Assembly assembly2 = Assembly.ReflectionOnlyLoadFrom(psSnapInInfo.AbsoluteModulePath);
                    if (assembly2 == null)
                    {
                        return null;
                    }
                    if (!string.Equals(assembly2.FullName, psSnapInInfo.AssemblyName, StringComparison.OrdinalIgnoreCase))
                    {
                        string errorMessageFormat = StringUtil.Format(ConsoleInfoErrorStrings.PSSnapInAssemblyNameMismatch, psSnapInInfo.AbsoluteModulePath, psSnapInInfo.AssemblyName);
                        _PSSnapInTracer.TraceError(errorMessageFormat, new object[0]);
                        throw new PSSnapInException(psSnapInInfo.Name, errorMessageFormat);
                    }
                    assembly = Assembly.LoadFrom(psSnapInInfo.AbsoluteModulePath);
                }
                catch (FileLoadException exception4)
                {
                    _PSSnapInTracer.TraceError("Not able to load assembly {0}: {1}", new object[] { psSnapInInfo.AssemblyName, exception4.Message });
                    throw new PSSnapInException(psSnapInInfo.Name, exception4.Message);
                }
                catch (BadImageFormatException exception5)
                {
                    _PSSnapInTracer.TraceError("Not able to load assembly {0}: {1}", new object[] { psSnapInInfo.AssemblyName, exception5.Message });
                    throw new PSSnapInException(psSnapInInfo.Name, exception5.Message);
                }
                catch (FileNotFoundException exception6)
                {
                    _PSSnapInTracer.TraceError("Not able to load assembly {0}: {1}", new object[] { psSnapInInfo.AssemblyName, exception6.Message });
                    throw new PSSnapInException(psSnapInInfo.Name, exception6.Message);
                }
            }
            return assembly;
        }
    }
}

