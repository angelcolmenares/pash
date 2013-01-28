using System;
using System.Globalization;
using System.Management.Automation;
using System.Management.Automation.Remoting;
using System.Management.Automation.Runspaces;
using System.Management.Automation.Tracing;
using System.Threading;

namespace Microsoft.PowerShell.Workflow
{
	public class PSWorkflowSessionConfiguration : PSSessionConfiguration
	{
		private const int ModulesNotLoaded = 0;

		private const int ModulesLoaded = 1;

		private readonly static InitialSessionState InitialSessionState;

		private static int _modulesLoaded;

		static PSWorkflowSessionConfiguration()
		{
			PSWorkflowSessionConfiguration.InitialSessionState = InitialSessionState.CreateRestricted(SessionCapabilities.RemoteServer | SessionCapabilities.WorkflowServer | SessionCapabilities.Language);
			PSWorkflowSessionConfiguration._modulesLoaded = 0;
		}

		public PSWorkflowSessionConfiguration()
		{
		}

		public override InitialSessionState GetInitialSessionState(PSSenderInfo senderInfo)
		{
			throw new NotImplementedException();
		}

		public override InitialSessionState GetInitialSessionState(PSSessionConfigurationData sessionConfigurationData, PSSenderInfo senderInfo, string configProviderId)
		{
			Tracer tracer = new Tracer();
			tracer.Correlate();
			if (sessionConfigurationData != null)
			{
				if (senderInfo != null)
				{
					if (!string.IsNullOrEmpty(configProviderId))
					{
						if (Interlocked.CompareExchange(ref PSWorkflowSessionConfiguration._modulesLoaded, 1, 0) == 0)
						{
							try
							{
								PSWorkflowConfigurationProvider configuration = WorkflowJobSourceAdapter.GetInstance().GetPSWorkflowRuntime().Configuration;
								if (configuration != null)
								{
									configuration.Populate(sessionConfigurationData.PrivateData, configProviderId, senderInfo);
									if (sessionConfigurationData.ModulesToImport != null)
									{
										foreach (string modulesToImport in sessionConfigurationData.ModulesToImport)
										{
											PSWorkflowSessionConfiguration.InitialSessionState.ImportPSModulesFromPath(modulesToImport);
										}
									}
								}
								else
								{
									throw new InvalidOperationException("PSWorkflowConfigurationProvider is null");
								}
							}
							catch (Exception exception)
							{
								Interlocked.CompareExchange(ref PSWorkflowSessionConfiguration._modulesLoaded, 0, 1);
								throw;
							}
						}
						if (configProviderId.ToLower(CultureInfo.InvariantCulture).Equals("http://schemas.microsoft.com/powershell/microsoft.windows.servermanagerworkflows"))
						{
							PSSessionConfigurationData.IsServerManager = true;
						}
						return PSWorkflowSessionConfiguration.InitialSessionState;
					}
					else
					{
						throw new ArgumentNullException("configProviderId");
					}
				}
				else
				{
					throw new ArgumentNullException("senderInfo");
				}
			}
			else
			{
				throw new ArgumentNullException("sessionConfigurationData");
			}
		}
	}
}