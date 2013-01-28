using System;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Provider;

namespace Microsoft.PowerShell.Commands
{
	public abstract class CoreCommandBase : PSCmdlet, IDynamicParameters
	{
		[TraceSource("NavigationCommands", "The namespace navigation tracer")]
		internal static PSTraceSource tracer;

		private bool suppressWildcardExpansion;

		private object dynamicParameters;

		internal Collection<CmdletProviderContext> stopContextCollection;

		private string filter;

		private string[] include;

		private string[] exclude;

		private bool force;

		internal virtual CmdletProviderContext CmdletProviderContext
		{
			get
			{
				CmdletProviderContext cmdletProviderContext = new CmdletProviderContext(this);
				cmdletProviderContext.Force = this.Force;
				Collection<string> collection = SessionStateUtilities.ConvertArrayToCollection<string>(this.Include);
				Collection<string> strs = SessionStateUtilities.ConvertArrayToCollection<string>(this.Exclude);
				cmdletProviderContext.SetFilters(collection, strs, this.Filter);
				cmdletProviderContext.SuppressWildcardExpansion = this.SuppressWildcardExpansion;
				cmdletProviderContext.DynamicParameters = this.RetrievedDynamicParameters;
				this.stopContextCollection.Add(cmdletProviderContext);
				return cmdletProviderContext;
			}
		}

		public virtual string[] Exclude
		{
			get
			{
				return this.exclude;
			}
			set
			{
				this.exclude = value;
			}
		}

		public virtual string Filter
		{
			get
			{
				return this.filter;
			}
			set
			{
				this.filter = value;
			}
		}

		public virtual SwitchParameter Force
		{
			get
			{
				return this.force;
			}
			set
			{
				this.force = value;
			}
		}

		public virtual string[] Include
		{
			get
			{
				return this.include;
			}
			set
			{
				this.include = value;
			}
		}

		protected virtual bool ProviderSupportsShouldProcess
		{
			get
			{
				return true;
			}
		}

		protected internal object RetrievedDynamicParameters
		{
			get
			{
				return this.dynamicParameters;
			}
		}

		public bool SupportsShouldProcess
		{
			get
			{
				bool providerSupportsShouldProcess = this.ProviderSupportsShouldProcess;
				object[] objArray = new object[1];
				objArray[0] = providerSupportsShouldProcess;
				CoreCommandBase.tracer.WriteLine("supportsShouldProcess = {0}", objArray);
				return providerSupportsShouldProcess;
			}
		}

		internal virtual SwitchParameter SuppressWildcardExpansion
		{
			get
			{
				return this.suppressWildcardExpansion;
			}
			set
			{
				this.suppressWildcardExpansion = value;
			}
		}

		static CoreCommandBase()
		{
			CoreCommandBase.tracer = PSTraceSource.GetTracer("NavigationCommands", "The namespace navigation tracer");
		}

		protected CoreCommandBase()
		{
			this.stopContextCollection = new Collection<CmdletProviderContext>();
			this.include = new string[0];
			this.exclude = new string[0];
		}

		protected bool DoesProviderSupportShouldProcess(string[] paths)
		{
			bool flag = true;
			if (paths != null && (int)paths.Length >= 0)
			{
				string[] strArrays = paths;
				int num = 0;
				while (num < (int)strArrays.Length)
				{
					string str = strArrays[num];
					ProviderInfo providerInfo = null;
					PSDriveInfo pSDriveInfo = null;
					base.SessionState.Path.GetUnresolvedProviderPathFromPSPath(str, this.CmdletProviderContext, out providerInfo, out pSDriveInfo);
					if (CmdletProviderManagementIntrinsics.CheckProviderCapabilities(ProviderCapabilities.ShouldProcess, providerInfo))
					{
						num++;
					}
					else
					{
						flag = false;
						break;
					}
				}
			}
			object[] objArray = new object[1];
			objArray[0] = flag;
			CoreCommandBase.tracer.WriteLine("result = {0}", objArray);
			return flag;
		}

		internal virtual object GetDynamicParameters(CmdletProviderContext context)
		{
			return null;
		}

		public object GetDynamicParameters()
		{
			CmdletProviderContext cmdletProviderContext = this.CmdletProviderContext;
			cmdletProviderContext.PassThru = false;
			try
			{
				this.dynamicParameters = this.GetDynamicParameters(cmdletProviderContext);
			}
			catch (ItemNotFoundException itemNotFoundException)
			{
				this.dynamicParameters = null;
			}
			catch (ProviderNotFoundException providerNotFoundException)
			{
				this.dynamicParameters = null;
			}
			catch (DriveNotFoundException driveNotFoundException)
			{
				this.dynamicParameters = null;
			}
			return this.dynamicParameters;
		}

		protected override void StopProcessing()
		{
			foreach (CmdletProviderContext cmdletProviderContext in this.stopContextCollection)
			{
				cmdletProviderContext.StopProcessing();
			}
		}
	}
}