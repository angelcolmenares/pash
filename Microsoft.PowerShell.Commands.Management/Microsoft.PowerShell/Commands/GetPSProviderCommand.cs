using System;
using System.Collections.ObjectModel;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
	[Cmdlet("Get", "PSProvider", HelpUri="http://go.microsoft.com/fwlink/?LinkID=113329")]
	[OutputType(new Type[] { typeof(ProviderInfo) })]
	public class GetPSProviderCommand : CoreCommandBase
	{
		private string[] provider;

		[Parameter(Position=0, ValueFromPipelineByPropertyName=true)]
		public string[] PSProvider
		{
			get
			{
				return this.provider;
			}
			set
			{
				if (value != null)
				{
					this.provider = value;
					return;
				}
				else
				{
					this.provider = new string[0];
					return;
				}
			}
		}

		public GetPSProviderCommand()
		{
			this.provider = new string[0];
		}

		protected override void ProcessRecord()
		{
			if (this.PSProvider == null || this.PSProvider != null && (int)this.PSProvider.Length == 0)
			{
				base.WriteObject(base.SessionState.Provider.GetAll(), true);
				return;
			}
			else
			{
				string[] pSProvider = this.PSProvider;
				for (int i = 0; i < (int)pSProvider.Length; i++)
				{
					string str = pSProvider[i];
					PSSnapinQualifiedName instance = PSSnapinQualifiedName.GetInstance(str);
					if (instance == null || !WildcardPattern.ContainsWildcardCharacters(instance.ShortName))
					{
						try
						{
							Collection<ProviderInfo> providerInfos = base.SessionState.Provider.Get(str);
							base.WriteObject(providerInfos, true);
						}
						catch (ProviderNotFoundException providerNotFoundException1)
						{
							ProviderNotFoundException providerNotFoundException = providerNotFoundException1;
							base.WriteError(new ErrorRecord(providerNotFoundException.ErrorRecord, providerNotFoundException));
						}
					}
					else
					{
						WildcardPattern wildcardPattern = new WildcardPattern(instance.ShortName, WildcardOptions.IgnoreCase);
						foreach (ProviderInfo all in base.SessionState.Provider.GetAll())
						{
							if (!all.IsMatch(wildcardPattern, instance))
							{
								continue;
							}
							base.WriteObject(all);
						}
					}
				}
				return;
			}
		}
	}
}