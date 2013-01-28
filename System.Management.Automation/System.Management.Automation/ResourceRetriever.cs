namespace System.Management.Automation
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Resources;

    internal class ResourceRetriever : MarshalByRefObject
    {
        private static string GetString(Stream stream, string resourceID)
        {
            ResourceReader reader = new ResourceReader(stream);
            foreach (DictionaryEntry entry in reader)
            {
                if (string.Equals(resourceID, (string) entry.Key, StringComparison.OrdinalIgnoreCase))
                {
                    return (string) entry.Value;
                }
            }
            return null;
        }

        internal string GetStringResource(string assemblyName, string modulePath, string baseName, string resourceID)
        {
            string str = null;
            string str2;
            Assembly assembly = LoadAssembly(assemblyName, modulePath);
            if (assembly == null)
            {
                return str;
            }
            CultureInfo currentUICulture = CultureInfo.CurrentUICulture;
            Stream manifestResourceStream = null;
        Label_001B:
            str2 = baseName;
            if (!string.IsNullOrEmpty(currentUICulture.Name))
            {
                str2 = str2 + "." + currentUICulture.Name;
            }
            str2 = str2 + ".resources";
            manifestResourceStream = assembly.GetManifestResourceStream(str2);
            if ((manifestResourceStream == null) && !string.IsNullOrEmpty(currentUICulture.Name))
            {
                currentUICulture = currentUICulture.Parent;
                goto Label_001B;
            }
            if (manifestResourceStream != null)
            {
                str = GetString(manifestResourceStream, resourceID);
            }
            return str;
        }

        private static Assembly LoadAssembly(string assemblyName, string modulePath)
        {
            Assembly assembly = null;
            AssemblyName name = new AssemblyName(assemblyName);
            string directoryName = Path.GetDirectoryName(modulePath);
            string fileName = Path.GetFileName(modulePath);
            CultureInfo currentUICulture = CultureInfo.CurrentUICulture;
            while (true)
            {
                assembly = LoadAssemblyForCulture(currentUICulture, name, directoryName, fileName);
                if ((assembly != null) || string.IsNullOrEmpty(currentUICulture.Name))
                {
                    return assembly;
                }
                currentUICulture = currentUICulture.Parent;
            }
        }

		private static bool isAssemblyCultureEnabled  = false;

        private static Assembly LoadAssemblyForCulture(CultureInfo culture, AssemblyName assemblyName, string moduleBase, string moduleFile)
        {
            Assembly assembly = null;
			if (isAssemblyCultureEnabled) assemblyName.CultureInfo = culture;
            try
            {
                assembly = Assembly.ReflectionOnlyLoad(assemblyName.FullName);
            }
            catch (FileLoadException)
            {
            }
            catch (BadImageFormatException)
            {
            }
            catch (FileNotFoundException)
            {
            }
            if (assembly == null)
            {
                string name = assemblyName.Name;
                try
                {
                    assemblyName.Name = name + ".resources";
                    assembly = Assembly.ReflectionOnlyLoad(assemblyName.FullName);
                }
                catch (FileLoadException)
                {
                }
                catch (BadImageFormatException)
                {
                }
                catch (FileNotFoundException)
                {
                }
                if (assembly != null)
                {
                    return assembly;
                }
                assemblyName.Name = name;
                string path = Path.Combine(Path.Combine(moduleBase, culture.Name), moduleFile);
                if (!File.Exists(path))
                {
                    return assembly;
                }
                try
                {
                    assembly = Assembly.ReflectionOnlyLoadFrom(path);
                }
                catch (FileLoadException)
                {
                }
                catch (BadImageFormatException)
                {
                }
                catch (FileNotFoundException)
                {
                }
            }
            return assembly;
        }
    }
}

