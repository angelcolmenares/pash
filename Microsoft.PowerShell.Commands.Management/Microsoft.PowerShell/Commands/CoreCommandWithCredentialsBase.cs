using System;
using System.Collections.ObjectModel;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
	public class CoreCommandWithCredentialsBase : CoreCommandBase
	{
		private PSCredential credential;

		internal override CmdletProviderContext CmdletProviderContext
		{
			get
			{
				CmdletProviderContext cmdletProviderContext = new CmdletProviderContext(this, this.Credential);
				cmdletProviderContext.Force = this.Force;
				Collection<string> collection = SessionStateUtilities.ConvertArrayToCollection<string>(this.Include);
				Collection<string> strs = SessionStateUtilities.ConvertArrayToCollection<string>(this.Exclude);
				cmdletProviderContext.SetFilters(collection, strs, this.Filter);
				cmdletProviderContext.SuppressWildcardExpansion = this.SuppressWildcardExpansion;
				cmdletProviderContext.DynamicParameters = base.RetrievedDynamicParameters;
				this.stopContextCollection.Add(cmdletProviderContext);
				return cmdletProviderContext;
			}
		}

		[Credential]
		[Parameter(ValueFromPipelineByPropertyName=true)]
		public PSCredential Credential
		{
			get
			{
				return this.credential;
			}
			set
			{
				this.credential = value;
			}
		}

		public CoreCommandWithCredentialsBase()
		{
		}
	}
}