namespace System.Management.Automation.Internal
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Security;

    internal class CabinetExtractorFactory
    {
        private static string assemblyFile = (Utils.GetApplicationBase(Utils.DefaultPowerShellShellID) + @"\pspluginwkr-v3.dll");
        private static ICabinetExtractorLoader cabinetLoader;
        internal static ICabinetExtractor EmptyExtractor = new EmptyCabinetExtractor();

        static CabinetExtractorFactory()
        {
            Assembly assembly = null;
            try
            {
                assembly = Assembly.LoadFrom(assemblyFile);
            }
            catch (ArgumentNullException)
            {
            }
            catch (FileNotFoundException)
            {
            }
            catch (FileLoadException)
            {
            }
            catch (BadImageFormatException)
            {
            }
            catch (SecurityException)
            {
            }
            catch (ArgumentException)
            {
            }
            catch (PathTooLongException)
            {
            }
            if (assembly != null)
            {
                cabinetLoader = (ICabinetExtractorLoader) assembly.GetType("System.Management.Automation.Internal.CabinetExtractorLoader").GetMethod("GetInstance").Invoke(null, null);
            }
        }

        internal static ICabinetExtractor GetCabinetExtractor()
        {
            if (cabinetLoader != null)
            {
                return cabinetLoader.GetCabinetExtractor();
            }
            return EmptyExtractor;
        }
    }
}

