using Microsoft.PowerShell.Workflow;
using System;
using System.IO;
using System.Reflection;
using System.Security;

namespace Microsoft.PowerShell.Activities
{
	internal class DefaultWorkflowHost : PSWorkflowHost
	{
		private readonly Assembly _serviceCore;

		private readonly PSActivityHostController _activityHostController;

		private readonly RunspaceProvider _runspaceProvider;

		private readonly static DefaultWorkflowHost _instance;

		private readonly RunspaceProvider _localRunspaceProvider;

		internal static DefaultWorkflowHost Instance
		{
			get
			{
				return DefaultWorkflowHost._instance;
			}
		}

		public override RunspaceProvider LocalRunspaceProvider
		{
			get
			{
				return this._localRunspaceProvider;
			}
		}

		public override PSActivityHostController PSActivityHostController
		{
			get
			{
				return this._activityHostController;
			}
		}

		public override RunspaceProvider RemoteRunspaceProvider
		{
			get
			{
				return this._runspaceProvider;
			}
		}

		static DefaultWorkflowHost()
		{
			DefaultWorkflowHost._instance = new DefaultWorkflowHost();
		}

		private DefaultWorkflowHost()
		{
			try
			{
				this._serviceCore = Assembly.Load("Microsoft.PowerShell.Workflow.ServiceCore, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35, processorArchitecture=MSIL");
			}
			catch (ArgumentNullException argumentNullException)
			{
			}
			catch (FileNotFoundException fileNotFoundException)
			{
			}
			catch (FileLoadException fileLoadException)
			{
			}
			catch (BadImageFormatException badImageFormatException)
			{
			}
			catch (SecurityException securityException)
			{
			}
			catch (ArgumentException argumentException)
			{
			}
			catch (PathTooLongException pathTooLongException)
			{
			}
			Type type = this._serviceCore.GetType("Microsoft.PowerShell.Workflow.PSWorkflowRuntime");
			PropertyInfo property = type.GetProperty("Instance", BindingFlags.Static | BindingFlags.NonPublic);
			object value = property.GetValue(null, null);
			property = value.GetType().GetProperty("Configuration");
			PSWorkflowConfigurationProvider pSWorkflowConfigurationProvider = (PSWorkflowConfigurationProvider)property.GetValue(value, null);
			pSWorkflowConfigurationProvider.Populate("\r\n                <PrivateData>\r\n                    <Param Name='AllowedActivity' Value='PSDefaultActivities' />\r\n                </PrivateData>\r\n", "Microsoft.PowerShell.Workflow");
			property = value.GetType().GetProperty("PSActivityHostController", BindingFlags.Instance | BindingFlags.Public);
			this._activityHostController = (PSActivityHostController)property.GetValue(value, null);
			property = value.GetType().GetProperty("RemoteRunspaceProvider", BindingFlags.Instance | BindingFlags.Public);
			this._runspaceProvider = (RunspaceProvider)property.GetValue(value, null);
			property = value.GetType().GetProperty("LocalRunspaceProvider", BindingFlags.Instance | BindingFlags.Public);
			this._localRunspaceProvider = (RunspaceProvider)property.GetValue(value, null);
		}

		internal void ResetLocalRunspaceProvider()
		{
			MethodInfo method = this._localRunspaceProvider.GetType().GetMethod("Reset", BindingFlags.Instance | BindingFlags.NonPublic);
			method.Invoke(this._localRunspaceProvider, new object[0]);
		}
	}
}