using Microsoft.ActiveDirectory;
using Microsoft.ActiveDirectory.Management;
using System;
using System.Globalization;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Get", "ADTrust", HelpUri="http://go.microsoft.com/fwlink/?LinkId=216369", DefaultParameterSetName="Filter")]
	public class GetADTrust : ADGetCmdletBase<GetADTrustParameterSet, ADTrustFactory<ADTrust>, ADTrust>
	{
		public GetADTrust()
		{
			base.ProcessRecordPipeline.InsertAtStart(new CmdletSubroutine(this.ADGetADTrustCmdletProcessInputObjectCSRoutine));
		}

		private bool ADGetADTrustCmdletProcessInputObjectCSRoutine()
		{
			if (this._cmdletParameters.Contains("InputObject"))
			{
				object item = this._cmdletParameters["InputObject"];
				if (item as PSObject != null)
				{
					item = ((PSObject)item).BaseObject;
				}
				ADEntity aDEntity = item as ADEntity;
				string str = item as string;
				if (aDEntity == null)
				{
					if (str == null)
					{
						object[] type = new object[2];
						type[0] = item.GetType();
						type[1] = "InputObject";
						base.WriteErrorBuffered(this.ConstructErrorRecord(new ParameterBindingException(string.Format(CultureInfo.CurrentCulture, StringResources.UnsupportedParameterType, type))));
						return false;
					}
					else
					{
						this._cmdletParameters["Identity"] = new ADTrust(str);
					}
				}
				else
				{
					if (aDEntity.IsSearchResult)
					{
						if (aDEntity as ADForest != null || aDEntity as ADDomain != null || aDEntity as ADClaimTransformPolicy != null)
						{
							CmdletSessionInfo cmdletSessionInfo = this.GetCmdletSessionInfo();
							ADTrustFactory<ADTrust> aDTrustFactory = new ADTrustFactory<ADTrust>();
							IADOPathNode structuralObjectFilter = aDTrustFactory.StructuralObjectFilter;
							if (aDEntity as ADForest != null || aDEntity as ADDomain != null)
							{
								if (!this._cmdletParameters.Contains("Server"))
								{
									ADSessionInfo aDSessionInfo = new ADSessionInfo(aDEntity["Name"].Value as string);
									if (aDEntity as ADForest != null)
									{
										aDSessionInfo.SetDefaultPort(LdapConstants.LDAP_GC_PORT);
									}
									if (cmdletSessionInfo.ADSessionInfo != null)
									{
										aDSessionInfo.Credential = cmdletSessionInfo.ADSessionInfo.Credential;
									}
									cmdletSessionInfo.ADRootDSE = null;
									cmdletSessionInfo.ADSessionInfo = aDSessionInfo;
									using (ADObjectSearcher aDObjectSearcher = new ADObjectSearcher(aDSessionInfo))
									{
										cmdletSessionInfo.ADRootDSE = aDObjectSearcher.GetRootDSE();
										cmdletSessionInfo.ADRootDSE.SessionInfo = aDSessionInfo;
									}
									this.SetPipelinedSessionInfo(aDSessionInfo);
								}
								else
								{
									base.WriteErrorBuffered(this.ConstructErrorRecord(new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.ServerParameterNotSupported, new object[0]))));
									return false;
								}
							}
							else
							{
								IADOPathNode aDOPathNode = ADOPathUtil.CreateFilterClause(ADOperator.Eq, "msDS-EgressClaimsTransformationPolicy", aDEntity["DistinguishedName"].Value as string);
								IADOPathNode aDOPathNode1 = ADOPathUtil.CreateFilterClause(ADOperator.Eq, "msDS-IngressClaimsTransformationPolicy", aDEntity["DistinguishedName"].Value as string);
								IADOPathNode[] aDOPathNodeArray = new IADOPathNode[2];
								aDOPathNodeArray[0] = aDOPathNode;
								aDOPathNodeArray[1] = aDOPathNode1;
								structuralObjectFilter = ADOPathUtil.CreateOrClause(aDOPathNodeArray);
							}
							base.BuildPropertySet();
							this._factory.SetCmdletSessionInfo(cmdletSessionInfo);
							base.ValidateParameters();
							base.OutputSearchResults(structuralObjectFilter);
							return false;
						}
						else
						{
							if (aDEntity as ADTrust == null)
							{
								if (string.Compare(this._factory.StructuralObjectClass, aDEntity["ObjectClass"].Value as string, StringComparison.OrdinalIgnoreCase) == 0)
								{
									this._cmdletParameters["Identity"] = new ADTrust((string)aDEntity["DistinguishedName"].Value);
								}
								else
								{
									object[] objArray = new object[2];
									objArray[0] = aDEntity.GetType();
									objArray[1] = "InputObject";
									base.WriteErrorBuffered(this.ConstructErrorRecord(new ParameterBindingException(string.Format(CultureInfo.CurrentCulture, StringResources.UnsupportedParameterType, objArray))));
									return false;
								}
							}
							else
							{
								this._cmdletParameters["Identity"] = (ADTrust)aDEntity;
							}
						}
					}
					else
					{
						base.WriteErrorBuffered(this.ConstructErrorRecord(new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.OnlySearchResultsSupported, new object[0]))));
						return false;
					}
				}
			}
			return true;
		}
	}
}