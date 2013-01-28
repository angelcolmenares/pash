namespace System.Management.Automation.Internal
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Management.Automation;
    using System.Reflection;

    internal class GraphicalHostReflectionWrapper
    {
        private Assembly graphicalHostAssembly;
        private object graphicalHostHelperObject;
        private Type graphicalHostHelperType;

        private GraphicalHostReflectionWrapper()
        {
        }

        internal object CallMethod(string methodName, params object[] arguments)
        {
            return this.graphicalHostHelperType.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance).Invoke(this.graphicalHostHelperObject, arguments);
        }

        internal object CallStaticMethod(string methodName, params object[] arguments)
        {
            return this.graphicalHostHelperType.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, arguments);
        }

        internal static string EscapeBinding(string propertyName)
        {
            return propertyName.Replace("/", " ").Replace(".", " ");
        }

        internal static GraphicalHostReflectionWrapper GetGraphicalHostReflectionWrapper(PSCmdlet parentCmdlet, string graphicalHostHelperTypeName)
        {
            return GetGraphicalHostReflectionWrapper(parentCmdlet, graphicalHostHelperTypeName, parentCmdlet.CommandInfo.Name);
        }

        internal static GraphicalHostReflectionWrapper GetGraphicalHostReflectionWrapper(PSCmdlet parentCmdlet, string graphicalHostHelperTypeName, string featureName)
        {
            GraphicalHostReflectionWrapper wrapper = new GraphicalHostReflectionWrapper();
            if (IsInputFromRemoting(parentCmdlet))
            {
                ErrorRecord errorRecord = new ErrorRecord(new NotSupportedException(StringUtil.Format(HelpErrors.RemotingNotSupportedForFeature, featureName)), "RemotingNotSupported", ErrorCategory.InvalidOperation, parentCmdlet);
                parentCmdlet.ThrowTerminatingError(errorRecord);
            }
            AssemblyName assemblyRef = new AssemblyName {
                Name = "Microsoft.PowerShell.GraphicalHost",
                Version = new Version(3, 0, 0, 0),
                CultureInfo = new CultureInfo(string.Empty)
            };
            assemblyRef.SetPublicKeyToken(new byte[] { 0x31, 0xbf, 0x38, 0x56, 0xad, 0x36, 0x4e, 0x35 });
            try
            {
                wrapper.graphicalHostAssembly = Assembly.Load(assemblyRef);
            }
            catch (FileNotFoundException exception)
            {
                string message = StringUtil.Format(HelpErrors.GraphicalHostAssemblyIsNotFound, featureName, exception.Message);
                parentCmdlet.ThrowTerminatingError(new ErrorRecord(new NotSupportedException(message, exception), "ErrorLoadingAssembly", ErrorCategory.ObjectNotFound, assemblyRef));
            }
            catch (Exception exception2)
            {
                CommandProcessorBase.CheckForSevereException(exception2);
                parentCmdlet.ThrowTerminatingError(new ErrorRecord(exception2, "ErrorLoadingAssembly", ErrorCategory.ObjectNotFound, assemblyRef));
            }
            wrapper.graphicalHostHelperType = wrapper.graphicalHostAssembly.GetType(graphicalHostHelperTypeName);
            ConstructorInfo info = wrapper.graphicalHostHelperType.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[0], null);
            if (info != null)
            {
                wrapper.graphicalHostHelperObject = info.Invoke(new object[0]);
            }
            return wrapper;
        }

        internal object GetPropertyValue(string propertyName)
        {
            return this.graphicalHostHelperType.GetProperty(propertyName, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(this.graphicalHostHelperObject, new object[0]);
        }

        internal object GetStaticPropertyValue(string propertyName)
        {
            return this.graphicalHostHelperType.GetProperty(propertyName, BindingFlags.NonPublic | BindingFlags.Static).GetValue(null, new object[0]);
        }

        private static bool IsInputFromRemoting(PSCmdlet parentCmdlet)
        {
            return (parentCmdlet.SessionState.PSVariable.Get("PSSenderInfo") != null);
        }
    }
}

