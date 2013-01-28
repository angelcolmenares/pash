using Microsoft.Build.Evaluation;
using Microsoft.Build.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation.Tracing;
using System.Runtime.InteropServices;
using System.Xaml;
using System.Xml;

namespace Microsoft.PowerShell.Workflow
{
	internal class WorkflowRuntimeCompilation
	{
		private const string Template_Project = "<?xml version=\"1.0\" encoding=\"utf-8\"?><Project ToolsVersion=\"4.0\" DefaultTargets=\"Build\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\"><PropertyGroup><Configuration Condition=\" '$(Configuration)' == '' \">Release</Configuration><Platform Condition=\" '$(Platform)' == '' \">AnyCPU</Platform><ProductVersion>10.0</ProductVersion><SchemaVersion>2.0</SchemaVersion><OutputType>Library</OutputType><AppDesignerFolder>Properties</AppDesignerFolder><TargetFrameworkVersion>v4.0</TargetFrameworkVersion><TargetFrameworkProfile></TargetFrameworkProfile><FileAlignment>512</FileAlignment></PropertyGroup><PropertyGroup Condition=\" '$(Configuration)|$(Platform)' == 'Debug|AnyCPU' \"><DebugSymbols>true</DebugSymbols><DebugType>full</DebugType><Optimize>false</Optimize><OutputPath>bin\\Debug\\</OutputPath><DefineConstants>DEBUG;TRACE</DefineConstants><ErrorReport>prompt</ErrorReport><WarningLevel>4</WarningLevel></PropertyGroup><PropertyGroup Condition=\" '$(Configuration)|$(Platform)' == 'Release|AnyCPU' \"><DebugType>pdbonly</DebugType><Optimize>true</Optimize><OutputPath>bin\\Release\\</OutputPath><DefineConstants>TRACE</DefineConstants><ErrorReport>prompt</ErrorReport><WarningLevel>4</WarningLevel></PropertyGroup><ItemGroup><Reference Include=\"Microsoft.CSharp\" /><Reference Include=\"System\" /><Reference Include=\"System.Activities\" /><Reference Include=\"System.Core\" /><Reference Include=\"System.Data\" /><Reference Include=\"System.ServiceModel\" /><Reference Include=\"System.ServiceModel.Activities\" /><Reference Include=\"System.Xaml\" /><Reference Include=\"System.Xml\" /><Reference Include=\"System.Xml.Linq\" /><Reference Include=\"System.Management\" /><Reference Include=\"System.Management.Automation\" /><Reference Include=\"Microsoft.PowerShell.Workflow.ServiceCore\" /></ItemGroup><Import Project=\"$(MSBuildToolsPath)\\Microsoft.CSharp.targets\" /><!-- To modify your build process, add your task inside one of the targets below and uncomment it.  Other similar extension points exist, see Microsoft.Common.targets. <Target Name=\"BeforeBuild\"></Target><Target Name=\"AfterBuild\"></Target>--></Project>";

		private readonly static PowerShellTraceSource Tracer;

		private string _projectRoot;

		private static object _syncObject;

		internal string AssemblyName
		{
			get;
			set;
		}

		internal string AssemblyPath
		{
			get;
			set;
		}

		internal string BuildLogPath
		{
			get;
			set;
		}

		internal bool BuildReturnedCode
		{
			get;
			set;
		}

		internal string ProjectFilePath
		{
			get;
			set;
		}

		internal string ProjectFolderPath
		{
			get;
			set;
		}

		internal string ProjectName
		{
			get;
			set;
		}

		static WorkflowRuntimeCompilation()
		{
			WorkflowRuntimeCompilation.Tracer = PowerShellTraceSourceFactory.GetTraceSource();
			WorkflowRuntimeCompilation._syncObject = new object();
		}

		internal WorkflowRuntimeCompilation()
		{
			if (!WorkflowRuntimeCompilation.IsRunningOnProcessorArchitectureARM())
			{
				Guid guid = Guid.NewGuid();
				this.ProjectName = string.Concat("Workflow_", guid.ToString("N"));
				this._projectRoot = Path.Combine(Path.GetTempPath(), string.Concat("PSWorkflowCompilation\\", this.ProjectName));
				this.ProjectFolderPath = Path.Combine(this._projectRoot, "Project");
				this.ProjectFilePath = Path.Combine(this.ProjectFolderPath, "RuntimeProject.csproj");
				this.BuildLogPath = Path.Combine(this.ProjectFolderPath, "Build.Log");
				this.AssemblyName = this.ProjectName;
				this.AssemblyPath = Path.Combine(this._projectRoot, string.Concat(this.ProjectName, ".dll"));
				return;
			}
			else
			{
				WorkflowRuntimeCompilation.Tracer.WriteMessage("The workflow Calling workflow is not supported so throwing the exception.");
				throw new NotSupportedException(Resources.WFCallingWFNotSupported);
			}
		}

		internal void Compile(List<string> dependentWorkflows, Dictionary<string, string> requiredAssemblies)
		{
			string str = null;
			DirectoryInfo directoryInfo = new DirectoryInfo(this.ProjectFolderPath);
			directoryInfo.Create();
			List<string> strs = new List<string>();
			try
			{
				foreach (string dependentWorkflow in dependentWorkflows)
				{
					string str1 = Path.Combine(this.ProjectFolderPath, string.Concat(Path.GetRandomFileName(), ".xaml"));
					File.WriteAllText(str1, dependentWorkflow);
					strs.Add(str1);
				}
				File.WriteAllText(this.ProjectFilePath, "<?xml version=\"1.0\" encoding=\"utf-8\"?><Project ToolsVersion=\"4.0\" DefaultTargets=\"Build\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\"><PropertyGroup><Configuration Condition=\" '$(Configuration)' == '' \">Release</Configuration><Platform Condition=\" '$(Platform)' == '' \">AnyCPU</Platform><ProductVersion>10.0</ProductVersion><SchemaVersion>2.0</SchemaVersion><OutputType>Library</OutputType><AppDesignerFolder>Properties</AppDesignerFolder><TargetFrameworkVersion>v4.0</TargetFrameworkVersion><TargetFrameworkProfile></TargetFrameworkProfile><FileAlignment>512</FileAlignment></PropertyGroup><PropertyGroup Condition=\" '$(Configuration)|$(Platform)' == 'Debug|AnyCPU' \"><DebugSymbols>true</DebugSymbols><DebugType>full</DebugType><Optimize>false</Optimize><OutputPath>bin\\Debug\\</OutputPath><DefineConstants>DEBUG;TRACE</DefineConstants><ErrorReport>prompt</ErrorReport><WarningLevel>4</WarningLevel></PropertyGroup><PropertyGroup Condition=\" '$(Configuration)|$(Platform)' == 'Release|AnyCPU' \"><DebugType>pdbonly</DebugType><Optimize>true</Optimize><OutputPath>bin\\Release\\</OutputPath><DefineConstants>TRACE</DefineConstants><ErrorReport>prompt</ErrorReport><WarningLevel>4</WarningLevel></PropertyGroup><ItemGroup><Reference Include=\"Microsoft.CSharp\" /><Reference Include=\"System\" /><Reference Include=\"System.Activities\" /><Reference Include=\"System.Core\" /><Reference Include=\"System.Data\" /><Reference Include=\"System.ServiceModel\" /><Reference Include=\"System.ServiceModel.Activities\" /><Reference Include=\"System.Xaml\" /><Reference Include=\"System.Xml\" /><Reference Include=\"System.Xml.Linq\" /><Reference Include=\"System.Management\" /><Reference Include=\"System.Management.Automation\" /><Reference Include=\"Microsoft.PowerShell.Workflow.ServiceCore\" /></ItemGroup><Import Project=\"$(MSBuildToolsPath)\\Microsoft.CSharp.targets\" /><!-- To modify your build process, add your task inside one of the targets below and uncomment it.  Other similar extension points exist, see Microsoft.Common.targets. <Target Name=\"BeforeBuild\"></Target><Target Name=\"AfterBuild\"></Target>--></Project>");
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				WorkflowRuntimeCompilation.Tracer.TraceException(exception);
				throw;
			}
			using (ProjectCollection projectCollection = new ProjectCollection())
			{
				Project project = projectCollection.LoadProject(this.ProjectFilePath);
				project.SetProperty("AssemblyName", this.AssemblyName);
				HashSet<string> strs1 = new HashSet<string>();
				foreach (string str2 in strs)
				{
					project.AddItem("XamlAppDef", str2);
					XamlXmlReader xamlXmlReader = new XamlXmlReader(XmlReader.Create(str2), new XamlSchemaContext());
					using (xamlXmlReader)
					{
						while (xamlXmlReader.Read())
						{
							if (xamlXmlReader.NodeType != XamlNodeType.NamespaceDeclaration)
							{
								continue;
							}
							string lowerInvariant = xamlXmlReader.Namespace.Namespace.ToLowerInvariant();
							if (lowerInvariant.IndexOf("assembly=", StringComparison.OrdinalIgnoreCase) <= -1)
							{
								continue;
							}
							List<string> strs2 = new List<string>();
							strs2.Add("assembly=");
							string[] strArrays = lowerInvariant.Split(strs2.ToArray(), StringSplitOptions.RemoveEmptyEntries);
							if ((int)strArrays.Length <= 1 || string.IsNullOrEmpty(strArrays[1]))
							{
								continue;
							}
							if (requiredAssemblies == null || requiredAssemblies.Count <= 0 || !requiredAssemblies.TryGetValue(strArrays[1], out str))
							{
								strs1.Add(strArrays[1]);
							}
							else
							{
								strs1.Add(str);
							}
						}
					}
				}
				foreach (string str3 in strs1)
				{
					project.AddItem("Reference", str3);
				}
				project.Save(this.ProjectFilePath);
				FileLogger fileLogger = new FileLogger();
				fileLogger.Parameters = string.Concat("logfile=", this.BuildLogPath);
				this.BuildReturnedCode = false;
				lock (WorkflowRuntimeCompilation._syncObject)
				{
					this.BuildReturnedCode = project.Build(fileLogger);
				}
				fileLogger.Shutdown();
				if (this.BuildReturnedCode)
				{
					string str4 = Path.Combine(this.ProjectFolderPath, string.Concat("obj\\Release\\", this.ProjectName, ".dll"));
					if (File.Exists(str4))
					{
						File.Move(str4, this.AssemblyPath);
					}
					try
					{
						Directory.Delete(this.ProjectFolderPath, true);
					}
					catch (Exception exception3)
					{
						Exception exception2 = exception3;
						WorkflowRuntimeCompilation.Tracer.TraceException(exception2);
					}
				}
			}
		}

		internal static bool IsRunningOnProcessorArchitectureARM()
		{
			WorkflowRuntimeCompilation.NativeMethods.SYSTEM_INFO sYSTEMINFO = new WorkflowRuntimeCompilation.NativeMethods.SYSTEM_INFO();
			WorkflowRuntimeCompilation.NativeMethods.GetSystemInfo(ref sYSTEMINFO);
			return sYSTEMINFO.wProcessorArchitecture == 5;
		}

		private static class NativeMethods
		{
			internal const ushort PROCESSOR_ARCHITECTURE_INTEL = 0;

			internal const ushort PROCESSOR_ARCHITECTURE_ARM = 5;

			internal const ushort PROCESSOR_ARCHITECTURE_IA64 = 6;

			internal const ushort PROCESSOR_ARCHITECTURE_AMD64 = 9;

			internal const ushort PROCESSOR_ARCHITECTURE_UNKNOWN = 0xffff;

			[DllImport("kernel32.dll", CharSet=CharSet.None)]
			internal static extern void GetSystemInfo(ref WorkflowRuntimeCompilation.NativeMethods.SYSTEM_INFO lpSystemInfo);

			internal struct SYSTEM_INFO
			{
				public ushort wProcessorArchitecture;

				public ushort wReserved;

				public uint dwPageSize;

				public IntPtr lpMinimumApplicationAddress;

				public IntPtr lpMaximumApplicationAddress;

				public UIntPtr dwActiveProcessorMask;

				public uint dwNumberOfProcessors;

				public uint dwProcessorType;

				public uint dwAllocationGranularity;

				public ushort wProcessorLevel;

				public ushort wProcessorRevision;

			}
		}
	}
}