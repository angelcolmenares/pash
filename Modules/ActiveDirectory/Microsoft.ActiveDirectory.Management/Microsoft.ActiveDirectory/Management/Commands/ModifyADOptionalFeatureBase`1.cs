using Microsoft.ActiveDirectory;
using Microsoft.ActiveDirectory.Management;
using Microsoft.ActiveDirectory.Management.Provider;
using System;
using System.Globalization;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public class ModifyADOptionalFeatureBase<P> : ADSetCmdletBase<P, ADOptionalFeatureFactory<ADOptionalFeature>, ADOptionalFeature>
	where P : ADParameterSet, new()
	{
		protected ModifyADOptionalFeatureBase<P>.ModifyADOptionalFeatureAction _action;

		protected ModifyADOptionalFeatureBase(ModifyADOptionalFeatureBase<P>.ModifyADOptionalFeatureAction action)
		{
			this._action = (ModifyADOptionalFeatureBase<P>.ModifyADOptionalFeatureAction)action;
			base.ProcessRecordPipeline.Clear();
			base.ProcessRecordPipeline.InsertAtStart(new CmdletSubroutine(this.ModifyADOptionalFeatureBaseProcessCSRoutine));
		}

		protected internal override string GetDefaultPartitionPath()
		{
			return this.GetRootDSE().ConfigurationNamingContext;
		}

		protected internal override string GetDefaultQueryPath()
		{
			return this.GetRootDSE().ConfigurationNamingContext;
		}

		private bool ModifyADOptionalFeatureBaseProcessCSRoutine()
		{
			string distinguishedName;
			string str;
			string str1 = null;
			bool flag;
			if (this._cmdletParameters.Contains("Identity"))
			{
				ADOptionalFeature item = this._cmdletParameters["Identity"] as ADOptionalFeature;
				ADOptionalFeatureScope aDOptionalFeatureScope = (ADOptionalFeatureScope)this._cmdletParameters["Scope"];
				ADEntity aDEntity = this._cmdletParameters["Target"] as ADEntity;
				this.SetPipelinedSessionInfo(item.SessionInfo);
				CmdletSessionInfo cmdletSessionInfo = this.GetCmdletSessionInfo();
				this._factory.SetCmdletSessionInfo(cmdletSessionInfo);
				if (aDOptionalFeatureScope != ADOptionalFeatureScope.Domain)
				{
					if (aDOptionalFeatureScope != ADOptionalFeatureScope.ForestOrConfigurationSet)
					{
						distinguishedName = null;
					}
					else
					{
						string item1 = this._cmdletParameters["Server"] as string;
						ADRootDSE rootDSE = this.GetRootDSE();
						if (rootDSE.ServerType != ADServerType.ADDS)
						{
							cmdletSessionInfo = this.GetCmdletSessionInfo();
							ADObjectFactory<ADObject> aDObjectFactory = new ADObjectFactory<ADObject>();
							aDObjectFactory.SetCmdletSessionInfo(cmdletSessionInfo);
							ADObject aDObject = new ADObject();
							aDObject.Identity = aDEntity.Identity;
							ADObject directoryObjectFromIdentity = aDObjectFactory.GetDirectoryObjectFromIdentity(aDObject, rootDSE.ConfigurationNamingContext, false);
							string str2 = X500Path.StripX500Whitespace(directoryObjectFromIdentity.DistinguishedName);
							if (string.Compare(rootDSE.ConfigurationNamingContext, str2, StringComparison.InvariantCultureIgnoreCase) == 0)
							{
								distinguishedName = string.Concat("CN=Partitions,", directoryObjectFromIdentity.DistinguishedName);
							}
							else
							{
								object[] objArray = new object[1];
								objArray[0] = aDEntity.Identity.ToString();
								throw new ADIdentityNotFoundException(string.Format(CultureInfo.CurrentCulture, StringResources.ConfigSetNotFound, objArray));
							}
						}
						else
						{
							ADRootDSE aDRootDSE = null;
							if (item1 != null)
							{
								aDRootDSE = rootDSE;
							}
							if (aDEntity as ADForest == null)
							{
								str = ADDomainUtil.DiscoverDCFromIdentity<ADForest>(aDEntity.Identity, out str1);
							}
							else
							{
								str = ADDomainUtil.DiscoverDCFromIdentity<ADForest>(aDEntity, out str1);
							}
							if (str != null)
							{
								ADSessionInfo sessionInfo = this.GetSessionInfo();
								sessionInfo.Server = str1;
								this.SetPipelinedSessionInfo(sessionInfo);
								if (aDRootDSE != null)
								{
									ADRootDSE rootDSE1 = this.GetRootDSE();
									if (rootDSE1.RootDomainNamingContext == aDRootDSE.RootDomainNamingContext)
									{
										sessionInfo.Server = item1;
										this.SetPipelinedSessionInfo(sessionInfo);
									}
									else
									{
										throw new ADIdentityNotFoundException();
									}
								}
								base.TargetOperationMasterRole(ADOperationMasterRole.DomainNamingMaster);
								cmdletSessionInfo = this.GetCmdletSessionInfo();
								this._factory.SetCmdletSessionInfo(cmdletSessionInfo);
								ADForestFactory<ADForest> aDForestFactory = new ADForestFactory<ADForest>();
								aDForestFactory.SetCmdletSessionInfo(cmdletSessionInfo);
								ADForest aDForest = new ADForest(str1);
								ADObject directoryObjectFromIdentity1 = aDForestFactory.GetDirectoryObjectFromIdentity(aDForest, this.GetRootDSE().DefaultNamingContext, false);
								distinguishedName = directoryObjectFromIdentity1.DistinguishedName;
							}
							else
							{
								object[] defaultNamingContext = new object[2];
								defaultNamingContext[0] = aDEntity.Identity.ToString();
								defaultNamingContext[1] = this.GetRootDSE().DefaultNamingContext;
								throw new ADIdentityNotFoundException(string.Format(CultureInfo.CurrentCulture, StringResources.IdentityNotFound, defaultNamingContext));
							}
						}
					}
				}
				else
				{
					ADDomainFactory<ADDomain> aDDomainFactory = new ADDomainFactory<ADDomain>();
					aDDomainFactory.SetCmdletSessionInfo(cmdletSessionInfo);
					ADDomain aDDomain = new ADDomain();
					if (aDEntity as ADObject == null)
					{
						aDDomain = new ADDomain(aDEntity.Identity as string);
					}
					else
					{
						aDDomain.Identity = aDEntity;
					}
					ADObject aDObject1 = aDDomainFactory.GetDirectoryObjectFromIdentity(aDDomain, this.GetRootDSE().DefaultNamingContext, false);
					distinguishedName = aDObject1.DistinguishedName;
				}
				this.ValidateParameters();
				item = this._factory.GetExtendedObjectFromIdentity(item, this.GetDefaultPartitionPath(), null, false);
				Guid? featureGUID = item.FeatureGUID;
				string str3 = featureGUID.ToString();
				if (this._action != ModifyADOptionalFeatureBase<P>.ModifyADOptionalFeatureAction.Enable)
				{
					if (this._action != ModifyADOptionalFeatureBase<P>.ModifyADOptionalFeatureAction.Disable)
					{
						throw new NotImplementedException(this._action.ToString());
					}
					else
					{
						flag = false;
					}
				}
				else
				{
					if (!item.IsDisableable)
					{
						base.WriteWarning(string.Format(StringResources.EnablingIsIrreversible, item.Name, distinguishedName));
					}
					flag = true;
				}
				if (base.ShouldProcessOverride(item.Name, this._action.ToString()))
				{
					using (ADTopologyManagement aDTopologyManagement = new ADTopologyManagement(cmdletSessionInfo.ADSessionInfo))
					{
						aDTopologyManagement.ChangeOptionalFeature(distinguishedName, flag, str3);
					}
					return true;
				}
				else
				{
					return false;
				}
			}
			else
			{
				object[] objArray1 = new object[1];
				objArray1[0] = "Identity,Instance";
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.ParameterRequiredMultiple, objArray1));
			}
		}

		protected enum ModifyADOptionalFeatureAction
		{
			Enable,
			Disable
		}
	}
}