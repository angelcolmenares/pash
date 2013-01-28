using Microsoft.ActiveDirectory;
using Microsoft.ActiveDirectory.Management;
using System;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public abstract class ADGetDomainCmdletBase<P, F, O> : ADGetCmdletBase<P, F, O>
	where P : ADParameterSet, new()
	where F : ADFactory<O>, new()
	where O : ADEntity, new()
	{
		public ADGetDomainCmdletBase()
		{
			base.ProcessRecordPipeline.InsertAtStart(new CmdletSubroutine(this.ADGetDomainCmdletBaseCalculateIdentityCSRoutine));
		}

		private bool ADGetDomainCmdletBaseCalculateIdentityCSRoutine()
		{
			bool hasValue;
			bool flag;
			if (this._cmdletParameters["Identity"] == null)
			{
				string defaultNamingContext = null;
				string item = this._cmdletParameters["Server"] as string;
				ADCurrentDomainType? nullable = (ADCurrentDomainType?)(this._cmdletParameters["Current"] as ADCurrentDomainType?);
				if (!nullable.HasValue)
				{
					if (item != null || ProviderUtils.IsCurrentDriveAD(base.SessionState))
					{
						ADRootDSE rootDSE = this.GetRootDSE();
						defaultNamingContext = rootDSE.DefaultNamingContext;
					}
					else
					{
						nullable = new ADCurrentDomainType?(ADCurrentDomainType.LoggedOnUser);
					}
				}
				ADCurrentDomainType? nullable1 = nullable;
				if (nullable1.GetValueOrDefault() != ADCurrentDomainType.LocalComputer)
				{
					hasValue = false;
				}
				else
				{
					hasValue = nullable1.HasValue;
				}
				if (!hasValue)
				{
					ADCurrentDomainType? nullable2 = nullable;
					if (nullable2.GetValueOrDefault() != ADCurrentDomainType.LoggedOnUser)
					{
						flag = false;
					}
					else
					{
						flag = nullable2.HasValue;
					}
					if (!flag)
					{
						if (nullable.HasValue)
						{
							throw new ArgumentException("Current");
						}
					}
					else
					{
						defaultNamingContext = base.EffectiveDomainName;
					}
				}
				else
				{
					defaultNamingContext = ADDomainUtil.GetLocalComputerDomain();
					if (defaultNamingContext == null)
					{
						throw new ArgumentException(StringResources.CouldNotDetermineLocalComputerDomain);
					}
				}
				if (defaultNamingContext != null)
				{
					this._cmdletParameters["Identity"] = this.ConstructObjectFromIdentity(defaultNamingContext);
				}
				return true;
			}
			else
			{
				return true;
			}
		}

		protected internal abstract O ConstructObjectFromIdentity(string currentDomain);

		protected internal override string GetDefaultPartitionPath()
		{
			return this.GetRootDSE().DefaultNamingContext;
		}

		internal override ADSessionInfo GetSessionInfo()
		{
			return ADDomainUtil.ConstructSessionFromIdentity<P, ADDomain>(this, base.GetSessionInfo(), true);
		}
	}
}