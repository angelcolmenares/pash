using Microsoft.ActiveDirectory;
using Microsoft.ActiveDirectory.Management;
using System;
using System.Globalization;
using System.Management;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Get", "ADForest", HelpUri="http://go.microsoft.com/fwlink/?LinkId=219305", DefaultParameterSetName="Current")]
	public class GetADForest : ADGetCmdletBase<GetADForestParameterSet, ADForestFactory<ADForest>, ADForest>
	{
		public GetADForest()
		{
			base.ProcessRecordPipeline.InsertAtStart(new CmdletSubroutine(this.GetADForestCalculateIdentityCSRoutine));
		}

		private bool GetADForestCalculateIdentityCSRoutine()
		{
			bool hasValue;
			bool flag;
			string value = null;
			string item = this._cmdletParameters["Server"] as string;
			ADCurrentForestType? nullable = (ADCurrentForestType?)(this._cmdletParameters["Current"] as ADCurrentForestType?);
			if (this._cmdletParameters["Identity"] == null)
			{
				if (!nullable.HasValue)
				{
					if (item != null || ProviderUtils.IsCurrentDriveAD(base.SessionState))
					{
						ADRootDSE rootDSE = this.GetRootDSE();
						string str = string.Concat("CN=Partitions,", rootDSE.ConfigurationNamingContext);
						ADObjectSearcher aDObjectSearcher = SearchUtility.BuildSearcher(this.GetSessionInfo(), str, ADSearchScope.OneLevel);
						using (aDObjectSearcher)
						{
							IADOPathNode[] aDOPathNodeArray = new IADOPathNode[2];
							aDOPathNodeArray[0] = ADOPathUtil.CreateFilterClause(ADOperator.Eq, "objectClass", "crossRef");
							aDOPathNodeArray[1] = ADOPathUtil.CreateFilterClause(ADOperator.Eq, "nCName", rootDSE.RootDomainNamingContext);
							aDObjectSearcher.Filter = ADOPathUtil.CreateAndClause(aDOPathNodeArray);
							aDObjectSearcher.Properties.Add("dnsRoot");
							ADObject aDObject = aDObjectSearcher.FindOne();
							if (aDObject != null)
							{
								value = aDObject["dnsRoot"].Value as string;
							}
							if (value == null)
							{
								object[] rootDomainNamingContext = new object[1];
								rootDomainNamingContext[0] = rootDSE.RootDomainNamingContext;
								throw new ADIdentityResolutionException(string.Format(CultureInfo.CurrentCulture, StringResources.CouldNotFindForestIdentity, rootDomainNamingContext));
							}
						}
					}
					else
					{
						nullable = new ADCurrentForestType?(ADCurrentForestType.LoggedOnUser);
					}
				}
				ADCurrentForestType? nullable1 = nullable;
				if (nullable1.GetValueOrDefault() != ADCurrentForestType.LocalComputer)
				{
					hasValue = false;
				}
				else
				{
					hasValue = nullable1.HasValue;
				}
				if (!hasValue)
				{
					ADCurrentForestType? nullable2 = nullable;
					if (nullable2.GetValueOrDefault() != ADCurrentForestType.LoggedOnUser)
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
						value = base.EffectiveDomainName;
					}
				}
				else
				{
					SelectQuery selectQuery = new SelectQuery(WMIConstants.ComputerSystem);
					ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher(selectQuery);
					ManagementObjectCollection managementObjectCollections = managementObjectSearcher.Get();
					ManagementObjectCollection.ManagementObjectEnumerator enumerator = managementObjectCollections.GetEnumerator();
					using (enumerator)
					{
						if (enumerator.MoveNext())
						{
							ManagementObject current = (ManagementObject)enumerator.Current;
							value = current[WMIConstants.Domain] as string;
						}
					}
					if (value == null)
					{
						throw new ArgumentException(StringResources.CouldNotDetermineLocalComputerDomain);
					}
				}
				if (value != null)
				{
					this._cmdletParameters["Identity"] = new ADForest(value);
				}
				return true;
			}
			else
			{
				return true;
			}
		}

		protected internal override string GetDefaultPartitionPath()
		{
			return this.GetRootDSE().RootDomainNamingContext;
		}

		internal override ADSessionInfo GetSessionInfo()
		{
			ADSessionInfo aDSessionInfo;
			try
			{
				aDSessionInfo = ADDomainUtil.ConstructSessionFromIdentity<GetADForestParameterSet, ADForest>(this, base.GetSessionInfo(), false);
			}
			catch (ArgumentException argumentException1)
			{
				ArgumentException argumentException = argumentException1;
				object[] item = new object[1];
				item[0] = argumentException.Data["IdentityData"];
				throw new ADIdentityNotFoundException(string.Format(CultureInfo.CurrentCulture, StringResources.CouldNotFindForestIdentity, item));
			}
			return aDSessionInfo;
		}
	}
}