using Microsoft.Management.Odata;
using System;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace Microsoft.Management.Odata.Common
{
	internal static class TypeLoader
	{
		internal static Assembly LoadAssembly(string assemblyName)
		{
			TraceHelper.Current.DebugMessage(string.Concat("Trying to load assembly ", assemblyName, " from GAC"));
			Assembly assembly = null;
			try
			{
				if (assemblyName.IndexOf (".dll", StringComparison.OrdinalIgnoreCase) == -1)
				{
					assembly = Assembly.Load(assemblyName);
				}
				else {
					assembly = Assembly.LoadFile (assemblyName);
				}
			}
			catch (FileLoadException fileLoadException1)
			{
				FileLoadException fileLoadException = fileLoadException1;
				object[] traceMessage = new object[2];
				traceMessage[0] = assemblyName;
				traceMessage[1] = fileLoadException.ToTraceMessage("Exception");
				TraceHelper.Current.DebugMessage(string.Format(CultureInfo.CurrentCulture, Resources.AssemblLoadFromGACFailed, traceMessage));
			}
			catch (BadImageFormatException badImageFormatException1)
			{
				BadImageFormatException badImageFormatException = badImageFormatException1;
				object[] objArray = new object[2];
				objArray[0] = assemblyName;
				objArray[1] = badImageFormatException.ToTraceMessage("Exception");
				TraceHelper.Current.DebugMessage(string.Format(CultureInfo.CurrentCulture, Resources.AssemblLoadFromGACFailed, objArray));
			}
			catch (FileNotFoundException fileNotFoundException1)
			{
				FileNotFoundException fileNotFoundException = fileNotFoundException1;
				object[] traceMessage1 = new object[2];
				traceMessage1[0] = assemblyName;
				traceMessage1[1] = fileNotFoundException.ToTraceMessage("Exception");
				TraceHelper.Current.DebugMessage(string.Format(CultureInfo.CurrentCulture, Resources.AssemblLoadFromGACFailed, traceMessage1));
			}
			if (assembly == null)
			{
				string baseBinDirectory = Utils.GetBaseBinDirectory(assemblyName);
				string str = Path.Combine(baseBinDirectory, assemblyName);
				string[] strArrays = new string[6];
				strArrays[0] = "Attempt to load assembly ";
				strArrays[1] = assemblyName;
				strArrays[2] = " failed from GAC.\nNow trying to load assembly from application base ";
				strArrays[3] = baseBinDirectory;
				strArrays[4] = " \nAssembly full path ";
				strArrays[5] = str;
				TraceHelper.Current.DebugMessage(string.Concat(strArrays));
				try
				{
					assembly = Assembly.LoadFrom(str);
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					object[] objArray1 = new object[2];
					objArray1[0] = assemblyName;
					objArray1[1] = exception.ToTraceMessage("Exception");
					TraceHelper.Current.DebugMessage(string.Format(CultureInfo.CurrentCulture, Resources.AssemblyLoadFailed, objArray1));
					throw;
				}
			}
			TraceHelper.Current.DebugMessage(string.Concat("Loaded assembly ", assemblyName, " successfully."));
			return assembly;
		}

		public static Type LoadType(string assemblyName, string typeName)
		{
			return TypeLoader.LoadAssembly(assemblyName).GetType(typeName);
		}
	}
}