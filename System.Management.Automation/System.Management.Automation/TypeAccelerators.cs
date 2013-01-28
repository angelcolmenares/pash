namespace System.Management.Automation
{
    using System;
    using System.Collections.Generic;
    using System.Management.Automation.Runspaces;

    internal static class TypeAccelerators
    {
        private static Dictionary<string, Type> allTypeAccelerators = null;
        internal static Dictionary<string, Type> builtinTypeAccelerators = new Dictionary<string, Type>(0x40, StringComparer.OrdinalIgnoreCase);
        internal static Dictionary<string, Type> userTypeAccelerators = new Dictionary<string, Type>(0x40, StringComparer.OrdinalIgnoreCase);

        static TypeAccelerators()
        {
            foreach (KeyValuePair<Type, string[]> pair in CoreTypes.Items.Value)
            {
                if (pair.Value != null)
                {
                    foreach (string str in pair.Value)
                    {
                        builtinTypeAccelerators.Add(str, pair.Key);
                    }
                }
            }
            builtinTypeAccelerators.Add("scriptblock", typeof(ScriptBlock));
            builtinTypeAccelerators.Add("type", typeof(Type));
            builtinTypeAccelerators.Add("psmoduleinfo", typeof(PSModuleInfo));
            builtinTypeAccelerators.Add("powershell", typeof(PowerShell));
            builtinTypeAccelerators.Add("runspacefactory", typeof(RunspaceFactory));
            builtinTypeAccelerators.Add("runspace", typeof(Runspace));
        }

        public static void Add(string typeName, Type type)
        {
            lock (LanguagePrimitives.stringToTypeCache)
            {
                userTypeAccelerators[typeName] = type;
                if (allTypeAccelerators != null)
                {
                    allTypeAccelerators[typeName] = type;
                }
                LanguagePrimitives.stringToTypeCache.AddOrReplace(typeName, type);
            }
        }

        internal static void FillCache(Dictionary<string, Type> cache)
        {
            foreach (KeyValuePair<string, Type> pair in builtinTypeAccelerators)
            {
                cache.Add(pair.Key, pair.Value);
            }
            foreach (KeyValuePair<string, Type> pair2 in userTypeAccelerators)
            {
                cache.Add(pair2.Key, pair2.Value);
            }
        }

        internal static string FindBuiltinAccelerator(Type type)
        {
            foreach (KeyValuePair<string, Type> pair in builtinTypeAccelerators)
            {
                if (pair.Value.Equals(type))
                {
                    return pair.Key;
                }
            }
            return null;
        }

        public static bool Remove(string typeName)
        {
            lock (LanguagePrimitives.stringToTypeCache)
            {
                userTypeAccelerators.Remove(typeName);
                if (allTypeAccelerators != null)
                {
                    allTypeAccelerators.Remove(typeName);
                }
                return LanguagePrimitives.stringToTypeCache.Remove(typeName);
            }
        }

        public static Dictionary<string, Type> Get
        {
            get
            {
                if (allTypeAccelerators == null)
                {
                    allTypeAccelerators = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
                    FillCache(allTypeAccelerators);
                }
                return allTypeAccelerators;
            }
        }
    }
}

