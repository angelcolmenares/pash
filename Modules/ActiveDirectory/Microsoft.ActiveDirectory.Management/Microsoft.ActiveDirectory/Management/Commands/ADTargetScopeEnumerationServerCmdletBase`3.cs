using Microsoft.ActiveDirectory;
using Microsoft.ActiveDirectory.Management;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Management.Automation;
using System.Security.Authentication;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public abstract class ADTargetScopeEnumerationServerCmdletBase<P, F, O> : ADFactoryCmdletBase<P, F, O>, IDynamicParameters, IADErrorTarget
	where P : ADParameterSet, new()
	where F : ADXmlAttributeFactory<O>, new()
	where O : ADEntity, new()
	{
		private const string _debugCategory = "ADTargetScopeEnumerationServerCmdletBase";

		internal LinkedList<ADSessionInfo> _sessionPipe;

		private ADObjectFactory<ADObject> _sharedADOFactory;

		private CmdletSessionInfo _currentCmdletSessionInfo;

		protected virtual bool UseGCPortIfAvailable
		{
			get
			{
				return true;
			}
		}

		public ADTargetScopeEnumerationServerCmdletBase()
		{
			base.BeginProcessPipeline.InsertAtEnd(new CmdletSubroutine(this.ADTargetScopeEnumerationServerBeginCSRoutine));
			base.ProcessRecordPipeline.InsertAtEnd(new CmdletSubroutine(this.ADTargetScopeEnumerationServerPreProcessPipelineCSRoutine));
			base.ProcessRecordPipeline.InsertAtEnd(new CmdletSubroutine(this.ADTargetScopeEnumerationServerPreProcessTargetCSRoutine));
			base.ProcessRecordPipeline.InsertAtEnd(new CmdletSubroutine(this.ADTargetScopeEnumerationServerProcessCSRoutine));
		}

		private void AddSessionFromSiteDN(string siteDN)
		{
			IADOPathNode aDOPathNode = ADOPathUtil.CreateFilterClause(ADOperator.Eq, "objectClass", "server");
			List<string> strs = new List<string>();
			strs.Add("dNSHostName");
			ICollection<string> strs1 = strs;
			this._sharedADOFactory.SetCmdletSessionInfo(this.GetCmdletSessionInfo());
			int? nullable = null;
			int? nullable1 = null;
			IEnumerable<ADObject> extendedObjectFromFilter = this._sharedADOFactory.GetExtendedObjectFromFilter(aDOPathNode, string.Concat("CN=Servers,", siteDN), ADSearchScope.OneLevel, strs1, nullable, nullable1, false);
			foreach (ADObject aDObject in extendedObjectFromFilter)
			{
				this._sessionPipe.AddLast(new ADSessionInfo(aDObject["dNSHostName"].Value as string));
			}
		}

		private void AddSessionsFromConnectedForest()
		{
			IADOPathNode aDOPathNode = ADOPathUtil.CreateFilterClause(ADOperator.Eq, "objectClass", "server");
			List<string> strs = new List<string>();
			strs.Add("dNSHostName");
			ICollection<string> strs1 = strs;
			this._sharedADOFactory.SetCmdletSessionInfo(this.GetCmdletSessionInfo());
			int? nullable = null;
			int? nullable1 = null;
			IEnumerable<ADObject> extendedObjectFromFilter = this._sharedADOFactory.GetExtendedObjectFromFilter(aDOPathNode, string.Concat("CN=Sites,", this.GetCmdletSessionInfo().ADRootDSE.ConfigurationNamingContext), ADSearchScope.Subtree, strs1, nullable, nullable1, false);
			foreach (ADObject aDObject in extendedObjectFromFilter)
			{
				this._sessionPipe.AddLast(new ADSessionInfo(aDObject["dNSHostName"].Value as string));
			}
		}

		private void AddSessionsFromDomain(ADDomain domain)
		{
			foreach (string replicaDirectoryServer in domain.ReplicaDirectoryServers)
			{
				this._sessionPipe.AddLast(new ADSessionInfo(replicaDirectoryServer));
			}
			foreach (string readOnlyReplicaDirectoryServer in domain.ReadOnlyReplicaDirectoryServers)
			{
				this._sessionPipe.AddLast(new ADSessionInfo(readOnlyReplicaDirectoryServer));
			}
		}

		private void AddSessionsMatchingServerName(string serverName)
		{
			if (serverName == null || serverName == "*")
			{
				this.AddSessionsFromConnectedForest();
				return;
			}
			else
			{
				this._sessionPipe.AddLast(new ADSessionInfo(serverName));
				return;
			}
		}

		private void AddSessionsMatchingSiteName(string siteName)
		{
			bool flag = false;
			if (siteName == null || siteName == "*")
			{
				this.AddSessionsFromConnectedForest();
				return;
			}
			else
			{
				IADOPathNode[] aDOPathNodeArray = new IADOPathNode[2];
				IADOPathNode[] aDOPathNodeArray1 = new IADOPathNode[2];
				aDOPathNodeArray1[0] = ADOPathUtil.CreateFilterClause(ADOperator.Like, "distinguishedName", siteName);
				aDOPathNodeArray1[1] = ADOPathUtil.CreateFilterClause(ADOperator.Like, "name", siteName);
				aDOPathNodeArray[0] = ADOPathUtil.CreateOrClause(aDOPathNodeArray1);
				aDOPathNodeArray[1] = ADOPathUtil.CreateFilterClause(ADOperator.Eq, "objectClass", "site");
				IADOPathNode aDOPathNode = ADOPathUtil.CreateAndClause(aDOPathNodeArray);
				List<string> strs = new List<string>();
				strs.Add("distinguishedName");
				ICollection<string> strs1 = strs;
				this._sharedADOFactory.SetCmdletSessionInfo(this.GetCmdletSessionInfo());
				int? nullable = null;
				int? nullable1 = null;
				IEnumerable<ADObject> extendedObjectFromFilter = this._sharedADOFactory.GetExtendedObjectFromFilter(aDOPathNode, string.Concat("CN=Sites,", this.GetCmdletSessionInfo().ADRootDSE.ConfigurationNamingContext), ADSearchScope.OneLevel, strs1, nullable, nullable1, false);
				foreach (ADObject aDObject in extendedObjectFromFilter)
				{
					this.AddSessionFromSiteDN(aDObject.DistinguishedName);
					flag = true;
				}
				if (!flag)
				{
					object[] item = new object[2];
					item[0] = siteName;
					item[1] = this._cmdletParameters["Scope"];
					base.WriteErrorBuffered(this.ConstructErrorRecord(new ADIdentityNotFoundException(string.Format(CultureInfo.CurrentCulture, StringResources.NoMatchingResultsForTarget, item))));
				}
				return;
			}
		}

		private bool ADTargetScopeEnumerationServerBeginCSRoutine()
		{
			this._sessionPipe = new LinkedList<ADSessionInfo>();
			this._sharedADOFactory = new ADObjectFactory<ADObject>();
			if (!this._cmdletParameters.ContainsKey("Scope"))
			{
				this._cmdletParameters["Scope"] = ADScopeType.Server;
			}
			return true;
		}

		private bool ADTargetScopeEnumerationServerPreProcessPipelineCSRoutine()
		{
			if (!this._cmdletParameters.ContainsKey("Target"))
			{
				ADScopeType? item = (ADScopeType?)(this._cmdletParameters["Scope"] as ADScopeType?);
				if (item.Value != ADScopeType.Server)
				{
					CmdletSessionInfo cmdletSessionInfo = this.GetCmdletSessionInfo();
					ADDomainController aDDomainController = new ADDomainController(cmdletSessionInfo.ADRootDSE.DNSHostName);
					ADDomainControllerFactory<ADDomainController> aDDomainControllerFactory = new ADDomainControllerFactory<ADDomainController>();
					aDDomainControllerFactory.SetCmdletSessionInfo(cmdletSessionInfo);
					ADDomainController extendedObjectFromIdentity = aDDomainControllerFactory.GetExtendedObjectFromIdentity(aDDomainController, null, null, false);
					ADScopeType? nullable = (ADScopeType?)(this._cmdletParameters["Scope"] as ADScopeType?);
					ADScopeType valueOrDefault = nullable.GetValueOrDefault();
					if (nullable.HasValue)
					{
						switch (valueOrDefault)
						{
							case ADScopeType.Domain:
							{
								string[] domain = new string[1];
								domain[0] = extendedObjectFromIdentity.Domain;
								this._cmdletParameters["Target"] = domain;
								break;
							}
							case ADScopeType.Forest:
							{
								string[] forest = new string[1];
								forest[0] = extendedObjectFromIdentity.Forest;
								this._cmdletParameters["Target"] = forest;
								break;
							}
							case ADScopeType.Site:
							{
								string[] site = new string[1];
								site[0] = extendedObjectFromIdentity.Site;
								this._cmdletParameters["Target"] = site;
								break;
							}
						}
					}
				}
				else
				{
					throw new ADException(StringResources.ServerTargetParameterNotSpecified);
				}
			}
			return true;
		}

		private bool ADTargetScopeEnumerationServerPreProcessTargetCSRoutine()
		{
			object baseObject;
			ADSessionInfo sessionInfo = this.GetSessionInfo();
			string item = this._cmdletParameters["Server"] as string;
			object[] objArray = this._cmdletParameters["Target"] as object[];
			for (int i = 0; i < (int)objArray.Length; i++)
			{
				object obj = objArray[i];
				if (obj as PSObject == null)
				{
					baseObject = obj;
				}
				else
				{
					baseObject = ((PSObject)obj).BaseObject;
				}
				string str = baseObject as string;
				ADEntity aDEntity = baseObject as ADEntity;
				if (aDEntity == null)
				{
					ADScopeType? nullable = (ADScopeType?)(this._cmdletParameters["Scope"] as ADScopeType?);
					ADScopeType valueOrDefault = nullable.GetValueOrDefault();
					if (nullable.HasValue)
					{
						switch (valueOrDefault)
						{
							case ADScopeType.Server:
							{
								this.AddSessionsMatchingServerName(str);
								break;
							}
							case ADScopeType.Domain:
							{
								if (item == null)
								{
									ADDiscoverableService[] aDDiscoverableServiceArray = new ADDiscoverableService[1];
									aDDiscoverableServiceArray[0] = ADDiscoverableService.ADWS;
									ADMinimumDirectoryServiceVersion? nullable1 = null;
									ADEntity aDEntity1 = DomainControllerUtil.DiscoverDomainController(null, str, aDDiscoverableServiceArray, ADDiscoverDomainControllerOptions.ReturnDnsName, nullable1);
									this.SetPipelinedSessionInfo(new ADSessionInfo(aDEntity1["HostName"].Value as string));
								}
								ADDomainFactory<ADDomain> aDDomainFactory = new ADDomainFactory<ADDomain>();
								aDDomainFactory.SetCmdletSessionInfo(this.GetCmdletSessionInfo());
								ADDomain extendedObjectFromIdentity = aDDomainFactory.GetExtendedObjectFromIdentity(new ADDomain(str), this.GetRootDSE().DefaultNamingContext);
								this.AddSessionsFromDomain(extendedObjectFromIdentity);
								if (item != null)
								{
									break;
								}
								this.SetPipelinedSessionInfo(sessionInfo);
								break;
							}
							case ADScopeType.Forest:
							{
								if (item != null)
								{
									ADForestFactory<ADForest> aDForestFactory = new ADForestFactory<ADForest>();
									aDForestFactory.SetCmdletSessionInfo(this.GetCmdletSessionInfo());
									try
									{
										aDForestFactory.GetDirectoryObjectFromIdentity(new ADForest(str), null);
									}
									catch (ADIdentityNotFoundException aDIdentityNotFoundException1)
									{
										ADIdentityNotFoundException aDIdentityNotFoundException = aDIdentityNotFoundException1;
										base.WriteError(this.ConstructErrorRecord(aDIdentityNotFoundException));
										break;
									}
								}
								else
								{
									ADDiscoverableService[] aDDiscoverableServiceArray1 = new ADDiscoverableService[1];
									aDDiscoverableServiceArray1[0] = ADDiscoverableService.ADWS;
									ADMinimumDirectoryServiceVersion? nullable2 = null;
									ADEntity aDEntity2 = DomainControllerUtil.DiscoverDomainController(null, str, aDDiscoverableServiceArray1, ADDiscoverDomainControllerOptions.ReturnDnsName, nullable2);
									this.SetPipelinedSessionInfo(new ADSessionInfo(aDEntity2["HostName"].Value as string));
								}
								this.AddSessionsFromConnectedForest();
								if (item != null)
								{
									break;
								}
								this.SetPipelinedSessionInfo(sessionInfo);
								break;
							}
							case ADScopeType.Site:
							{
								this.AddSessionsMatchingSiteName(str);
								break;
							}
						}
					}
				}
				else
				{
					if (aDEntity.IsSearchResult)
					{
						this.SetPipelinedSessionInfo(aDEntity.SessionInfo);
						if (aDEntity as ADForest == null)
						{
							if (aDEntity as ADDomain == null)
							{
								if (aDEntity as ADDirectoryServer == null)
								{
									if (aDEntity as ADReplicationSite == null)
									{
										object[] type = new object[2];
										type[0] = aDEntity.GetType();
										type[1] = "Target";
										base.WriteErrorBuffered(this.ConstructErrorRecord(new ParameterBindingException(string.Format(CultureInfo.CurrentCulture, StringResources.UnsupportedParameterType, type))));
									}
									else
									{
										ADReplicationSite aDReplicationSite = (ADReplicationSite)aDEntity;
										this.AddSessionFromSiteDN(aDReplicationSite.DistinguishedName);
									}
								}
								else
								{
									ADDirectoryServer aDDirectoryServer = (ADDirectoryServer)aDEntity;
									this._sessionPipe.AddLast(new ADSessionInfo(aDDirectoryServer["HostName"].Value as string));
								}
							}
							else
							{
								ADDomain aDDomain = (ADDomain)aDEntity;
								this.AddSessionsFromDomain(aDDomain);
							}
						}
						else
						{
							this.AddSessionsFromConnectedForest();
						}
						this.SetPipelinedSessionInfo(sessionInfo);
					}
					else
					{
						base.WriteErrorBuffered(this.ConstructErrorRecord(new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.OnlySearchResultsSupported, new object[0]))));
					}
				}
			}
			return true;
		}

		private bool ADTargetScopeEnumerationServerProcessCSRoutine()
		{
			bool hasValue;
			this._currentCmdletSessionInfo = this.GetCmdletSessionInfo();
			foreach (ADSessionInfo credential in this._sessionPipe)
			{
				if (this._currentCmdletSessionInfo.ADSessionInfo != null)
				{
					credential.Credential = this._currentCmdletSessionInfo.ADSessionInfo.Credential;
				}
				this._currentCmdletSessionInfo.ADRootDSE = null;
				this._currentCmdletSessionInfo.ADSessionInfo = credential;
				try
				{
					using (ADObjectSearcher aDObjectSearcher = new ADObjectSearcher(credential))
					{
						this._currentCmdletSessionInfo.ADRootDSE = aDObjectSearcher.GetRootDSE();
						this._currentCmdletSessionInfo.ADRootDSE.SessionInfo = credential;
					}
					if (this.UseGCPortIfAvailable && !credential.ConnectedToGC && !credential.UsingExplicitPort)
					{
						bool? globalCatalogReady = this._currentCmdletSessionInfo.ADRootDSE.GlobalCatalogReady;
						if (!globalCatalogReady.GetValueOrDefault())
						{
							hasValue = false;
						}
						else
						{
							hasValue = globalCatalogReady.HasValue;
						}
						if (hasValue)
						{
							credential.SetEffectivePort(LdapConstants.LDAP_GC_PORT);
						}
					}
					this._currentCmdletSessionInfo.ConnectedADServerType = this.GetConnectedStore();
					this._currentCmdletSessionInfo.DefaultQueryPath = this.GetDefaultQueryPath();
					this._currentCmdletSessionInfo.DefaultCreationPath = this.GetDefaultCreationPath();
					this._currentCmdletSessionInfo.DefaultPartitionPath = this.GetDefaultPartitionPath();
					this.PerServerProcessRecord();
				}
				catch (ADServerDownException aDServerDownException1)
				{
					ADServerDownException aDServerDownException = aDServerDownException1;
					base.WriteErrorBuffered(this.ConstructErrorRecord(aDServerDownException));
				}
				catch (AuthenticationException authenticationException1)
				{
					AuthenticationException authenticationException = authenticationException1;
					base.WriteErrorBuffered(this.ConstructErrorRecord(authenticationException));
				}
			}
			return true;
		}

		internal override CmdletSessionInfo GetCmdletSessionInfo()
		{
			if (this._currentCmdletSessionInfo == null)
			{
				return base.GetCmdletSessionInfo();
			}
			else
			{
				return this._currentCmdletSessionInfo;
			}
		}

		internal override ADServerType GetConnectedStore()
		{
			return this.GetRootDSE().ServerType;
		}

		protected internal override ADRootDSE GetRootDSE()
		{
			if (this._currentCmdletSessionInfo == null || this._currentCmdletSessionInfo.ADRootDSE == null)
			{
				return base.GetRootDSE();
			}
			else
			{
				return this._currentCmdletSessionInfo.ADRootDSE;
			}
		}

		internal override ADSessionInfo GetSessionInfo()
		{
			if (this._currentCmdletSessionInfo == null || this._currentCmdletSessionInfo.ADSessionInfo == null)
			{
				return base.GetSessionInfo();
			}
			else
			{
				return this._currentCmdletSessionInfo.ADSessionInfo;
			}
		}

		object Microsoft.ActiveDirectory.Management.Commands.IADErrorTarget.CurrentIdentity(Exception e)
		{
			return this.GetSessionInfo().Server;
		}

		internal abstract void PerServerProcessRecord();
	}
}