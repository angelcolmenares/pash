using Microsoft.ActiveDirectory;
using Microsoft.ActiveDirectory.Management;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Security.Principal;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public class ADDomainFactory<T> : ADPartitionFactory<T>
	where T : ADDomain, new()
	{
		private static IADOPathNode _domainStructuralFilter;

		private static string[] _domainIdentityLdapAttributes;

		private static string _domainStructuralObjectClass;

		private string _debugCategory;

		private static AttributeConverterEntry[] ADMappingTable;

		internal static IDictionary<ADServerType, MappingTable<AttributeConverterEntry>> AttributeTable
		{
			get
			{
				return ADPartitionFactory<T>.AttributeTable;
			}
		}

		internal override string[] IdentityLdapAttributes
		{
			get
			{
				return ADDomainFactory<T>._domainIdentityLdapAttributes;
			}
		}

		internal override string RDNPrefix
		{
			get
			{
				return "DC";
			}
		}

		internal override string StructuralObjectClass
		{
			get
			{
				return ADDomainFactory<T>._domainStructuralObjectClass;
			}
		}

		internal override IADOPathNode StructuralObjectFilter
		{
			get
			{
				return ADDomainFactory<T>._domainStructuralFilter;
			}
		}

		static ADDomainFactory()
		{
			ADDomainFactory<T>._domainStructuralFilter = null;
			ADDomainFactory<T>._domainIdentityLdapAttributes = null;
			ADDomainFactory<T>._domainStructuralObjectClass = "domainDNS";
			AttributeConverterEntry[] attributeConverterEntry = new AttributeConverterEntry[18];
			attributeConverterEntry[0] = new AttributeConverterEntry(ADDomainFactory<T>.ADDomainPropertyMap.AllowedDNSSuffixes.PropertyName, ADDomainFactory<T>.ADDomainPropertyMap.AllowedDNSSuffixes.ADAttribute, TypeConstants.String, true, TypeAdapterAccess.ReadWrite, false, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedMultivalueObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryMultivalueObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[1] = new AttributeConverterEntry(ADDomainFactory<T>.ADDomainPropertyMap.LastLogonReplicationInterval.PropertyName, ADDomainFactory<T>.ADDomainPropertyMap.LastLogonReplicationInterval.ADAttribute, TypeConstants.TimeSpan, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryDaysFromTimeSpan), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[2] = new AttributeConverterEntry(ADDomainFactory<T>.ADDomainPropertyMap.ManagedBy.PropertyName, ADDomainFactory<T>.ADDomainPropertyMap.ManagedBy.ADAttribute, TypeConstants.ADPrincipal, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryFromADObjectToDN<ADPrincipalFactory<ADPrincipal>, ADPrincipal>), new ToSearchFilterDelegate(SearchConverters.ToSearchFromADObjectToDN<ADPrincipalFactory<ADPrincipal>, ADPrincipal>));
			attributeConverterEntry[3] = new AttributeConverterEntry(ADDomainFactory<T>.ADDomainPropertyMap.DomainMode.PropertyName, ADDomainFactory<T>.ADDomainPropertyMap.DomainMode.ADAttribute, TypeConstants.ADDomainMode, true, TypeAdapterAccess.Read, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObjectWithCast<int>), new ToSearchFilterDelegate(SearchConverters.ToSearchEnum<ADDomainMode>));
			attributeConverterEntry[4] = new AttributeConverterEntry(ADDomainFactory<T>.ADDomainPropertyMap.DomainSID.PropertyName, ADDomainFactory<T>.ADDomainPropertyMap.DomainSID.ADAttribute, TypeConstants.SID, false, TypeAdapterAccess.Read, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[5] = new AttributeConverterEntry(ADDomainFactory<T>.ADDomainPropertyMap.LinkedGroupPolicyObjects.PropertyName, ADDomainFactory<T>.ADDomainPropertyMap.LinkedGroupPolicyObjects.ADAttribute, TypeConstants.String, false, TypeAdapterAccess.Read, false, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedMultivalueObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryMultivalueObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[6] = new AttributeConverterEntry(ADDomainFactory<T>.ADDomainPropertyMap.ChildDomains.PropertyName, ADDomainFactory<T>.ADDomainPropertyMap.ChildDomains.ADAttribute, TypeConstants.ADDomain, false, TypeAdapterAccess.Read, false, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedMultivalueObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryMultivalueObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[7] = new AttributeConverterEntry(ADDomainFactory<T>.ADDomainPropertyMap.ComputersContainer.PropertyName, ADDomainFactory<T>.ADDomainPropertyMap.ComputersContainer.ADAttribute, TypeConstants.String, false, TypeAdapterAccess.Read, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[8] = new AttributeConverterEntry(ADDomainFactory<T>.ADDomainPropertyMap.DomainControllersContainer.PropertyName, ADDomainFactory<T>.ADDomainPropertyMap.DomainControllersContainer.ADAttribute, TypeConstants.String, false, TypeAdapterAccess.Read, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[9] = new AttributeConverterEntry(ADDomainFactory<T>.ADDomainPropertyMap.ForeignSecurityPrincipalsContainer.PropertyName, ADDomainFactory<T>.ADDomainPropertyMap.ForeignSecurityPrincipalsContainer.ADAttribute, TypeConstants.String, false, TypeAdapterAccess.Read, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[10] = new AttributeConverterEntry(ADDomainFactory<T>.ADDomainPropertyMap.Forest.PropertyName, ADDomainFactory<T>.ADDomainPropertyMap.Forest.ADAttribute, TypeConstants.ADForest, false, TypeAdapterAccess.Read, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[11] = new AttributeConverterEntry(ADDomainFactory<T>.ADDomainPropertyMap.InfrastructureMaster.PropertyName, ADDomainFactory<T>.ADDomainPropertyMap.InfrastructureMaster.ADAttribute, TypeConstants.ADDomainController, false, TypeAdapterAccess.Read, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[12] = new AttributeConverterEntry(ADDomainFactory<T>.ADDomainPropertyMap.NetBIOSName.PropertyName, ADDomainFactory<T>.ADDomainPropertyMap.NetBIOSName.ADAttribute, TypeConstants.String, false, TypeAdapterAccess.Read, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[13] = new AttributeConverterEntry(ADDomainFactory<T>.ADDomainPropertyMap.PDCEmulator.PropertyName, ADDomainFactory<T>.ADDomainPropertyMap.PDCEmulator.ADAttribute, TypeConstants.ADDomainController, false, TypeAdapterAccess.Read, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[14] = new AttributeConverterEntry(ADDomainFactory<T>.ADDomainPropertyMap.ParentDomain.PropertyName, ADDomainFactory<T>.ADDomainPropertyMap.ParentDomain.ADAttribute, TypeConstants.ADDomain, false, TypeAdapterAccess.Read, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[15] = new AttributeConverterEntry(ADDomainFactory<T>.ADDomainPropertyMap.RIDMaster.PropertyName, ADDomainFactory<T>.ADDomainPropertyMap.RIDMaster.ADAttribute, TypeConstants.ADDomainController, false, TypeAdapterAccess.Read, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[16] = new AttributeConverterEntry(ADDomainFactory<T>.ADDomainPropertyMap.SystemsContainer.PropertyName, ADDomainFactory<T>.ADDomainPropertyMap.SystemsContainer.ADAttribute, TypeConstants.String, false, TypeAdapterAccess.Read, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[17] = new AttributeConverterEntry(ADDomainFactory<T>.ADDomainPropertyMap.UsersContainer.PropertyName, ADDomainFactory<T>.ADDomainPropertyMap.UsersContainer.ADAttribute, TypeConstants.String, false, TypeAdapterAccess.Read, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			ADDomainFactory<T>.ADMappingTable = attributeConverterEntry;
			ADDomainFactory<T>._domainStructuralFilter = ADOPathUtil.CreateFilterClause(ADOperator.Eq, "objectClass", ADDomainFactory<T>._domainStructuralObjectClass);
			string[] strArrays = new string[3];
			strArrays[0] = "distinguishedName";
			strArrays[1] = "objectSid";
			strArrays[2] = "objectGUID";
			ADDomainFactory<T>._domainIdentityLdapAttributes = strArrays;
			ADFactoryBase<T>.RegisterMappingTable(ADDomainFactory<T>.ADMappingTable, ADServerType.ADDS);
		}

		public ADDomainFactory()
		{
			this._debugCategory = "ADDomainFactory";
			base.PreCommitPipeline.InsertAtEnd(new ADFactory<T>.FactoryCommitSubroutine(this.ADDomainPreCommitFSRoutine));
		}

		private bool ADDomainPreCommitFSRoutine(ADFactory<T>.DirectoryOperation operation, T instance, ADParameterSet parameters, ADObject directoryObj)
		{
			bool flag = false;
			if (operation == ADFactory<T>.DirectoryOperation.Update && base.PropertyHasChange(ADDomainFactory<T>.ADDomainPropertyMap.DomainMode.PropertyName, instance, parameters, operation))
			{
				ADDomainMode? singleValueProperty = base.GetSingleValueProperty<ADDomainMode?>(ADDomainFactory<T>.ADDomainPropertyMap.DomainMode.PropertyName, instance, parameters, operation);
				if (singleValueProperty.HasValue)
				{
					ADDomainMode value = singleValueProperty.Value;
					switch (value)
					{
						case ADDomainMode.Windows2000Domain:
						{
							Win32Exception win32Exception = new Win32Exception(50);
							throw new NotSupportedException(win32Exception.Message);
						}
						case ADDomainMode.Windows2003InterimDomain:
						{
							if (base.CmdletSessionInfo.ADRootDSE.DomainFunctionality == ADDomainMode.Windows2000Domain && ADDomainFactory<T>.GetNTMixedDomainMode(base.CmdletSessionInfo) == 1)
							{
								break;
							}
							directoryObj["ntMixedDomain"].Value = 1;
							flag = true;
							break;
						}
						case ADDomainMode.Windows2003Domain:
						{
							bool flag1 = false;
							if (base.CmdletSessionInfo.ADRootDSE.DomainFunctionality != ADDomainMode.Windows2000Domain)
							{
								if (base.CmdletSessionInfo.ADRootDSE.DomainFunctionality == ADDomainMode.Windows2003InterimDomain)
								{
									flag1 = true;
								}
							}
							else
							{
								if (ADDomainFactory<T>.GetNTMixedDomainMode(base.CmdletSessionInfo) != 0)
								{
									flag1 = true;
								}
							}
							if (!flag1)
							{
								break;
							}
							ADDomainFactory<T>.UpdateNTMixedDomainMode(base.CmdletSessionInfo, 0);
							break;
						}
					}
				}
				else
				{
					return flag;
				}
			}
			return flag;
		}

		internal override T Construct(ADEntity directoryObj, AttributeSetRequest requestedAttributes)
		{
			requestedAttributes = base.ConstructAttributeSetRequest(null);
			return base.Construct(directoryObj, requestedAttributes);
		}

		internal override AttributeSetRequest ConstructAttributeSetRequest(ICollection<string> requestedExtendedAttr)
		{
			return new AttributeSetRequest(false);
		}

		internal override T GetExtendedObjectFromDN(string distinguishedName, ICollection<string> propertiesToFetch, bool showDeleted)
		{
			ADObject domain = null;
			if (string.Compare(distinguishedName, base.CmdletSessionInfo.ADRootDSE.DefaultNamingContext, StringComparison.OrdinalIgnoreCase) == 0)
			{
				using (ADTopologyManagement aDTopologyManagement = new ADTopologyManagement(base.CmdletSessionInfo.ADSessionInfo))
				{
					domain = aDTopologyManagement.GetDomain();
				}
				if (domain == null)
				{
					DebugLogger.LogInfo(this._debugCategory, string.Format("GetExtendedObjectFromIdentity: No objects returned from custom action", new object[0]));
				}
			}
			if (domain == null)
			{
				DebugLogger.LogInfo(this._debugCategory, string.Format("ADFactory: GetExtendedObjectFromDN: Identity not found", new object[0]));
				T t = default(T);
				return t;
			}
			else
			{
				T aDSessionInfo = this.Construct(domain, null);
				aDSessionInfo.SessionInfo = base.CmdletSessionInfo.ADSessionInfo;
				return aDSessionInfo;
			}
		}

		internal override T GetExtendedObjectFromIdentity(T identityObj, string identityQueryPath, ICollection<string> propertiesToFetch, bool showDeleted)
		{
			AttributeSetRequest attributeSetRequest;
			string distinguishedName;
			ADObject domain;
			if (!identityObj.IsSearchResult)
			{
				ADObject aDObject = identityObj;
				if (aDObject != null)
				{
					if (base.CmdletSessionInfo != null)
					{
						attributeSetRequest = this.ConstructAttributeSetRequest(null);
						attributeSetRequest.DirectoryAttributes.Add("distinguishedName");
						IADOPathNode aDOPathNode = this.BuildIdentityFilter(identityObj);
						IADOPathNode[] structuralObjectFilter = new IADOPathNode[2];
						structuralObjectFilter[0] = aDOPathNode;
						structuralObjectFilter[1] = this.StructuralObjectFilter;
						aDOPathNode = ADOPathUtil.CreateAndClause(structuralObjectFilter);
						string str = identityQueryPath;
						ADSearchScope aDSearchScope = ADSearchScope.Subtree;
						bool flag = false;
						ADObjectSearcher aDObjectSearcher = SearchUtility.BuildSearcher(base.CmdletSessionInfo.ADSessionInfo, str, aDSearchScope, showDeleted);
						using (aDObjectSearcher)
						{
							aDObjectSearcher.Filter = aDOPathNode;
							aDObjectSearcher.Properties.AddRange(attributeSetRequest.DirectoryAttributes);
							DebugLogger.LogInfo(this._debugCategory, string.Format("ADFactory: GetExtendedObjectFromIdentity: Searching for identity using filter: {0} searchbase: {1} scope: {2}", aDObjectSearcher.Filter.GetLdapFilterString(), aDObjectSearcher.SearchRoot, aDObjectSearcher.Scope));
							ADObject aDObject1 = aDObjectSearcher.FindOne(out flag);
							if (aDObject1 != null)
							{
								if (!flag)
								{
									distinguishedName = aDObject1.DistinguishedName;
								}
								else
								{
									throw new ADMultipleMatchingIdentitiesException(StringResources.MultipleMatches);
								}
							}
							else
							{
								DebugLogger.LogInfo(this._debugCategory, string.Format("ADFactory: GetExtendedObjectFromIdentity: Identity not found", new object[0]));
								object[] searchRoot = new object[2];
								searchRoot[0] = identityObj.ToString();
								searchRoot[1] = aDObjectSearcher.SearchRoot;
								throw new ADIdentityNotFoundException(string.Format(CultureInfo.CurrentCulture, StringResources.IdentityNotFound, searchRoot));
							}
						}
					}
					else
					{
						throw new ArgumentNullException(StringResources.SessionRequired);
					}
				}
				else
				{
					object[] type = new object[2];
					type[0] = "GetExtendedObjectFromIdentity";
					type[1] = identityObj.GetType();
					throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, StringResources.MethodNotSupportedForObjectType, type));
				}
			}
			else
			{
				distinguishedName = identityObj.DistinguishedName;
			}
			if (string.Compare(distinguishedName, base.CmdletSessionInfo.ADRootDSE.DefaultNamingContext, StringComparison.OrdinalIgnoreCase) == 0)
			{
				using (ADTopologyManagement aDTopologyManagement = new ADTopologyManagement(base.CmdletSessionInfo.ADSessionInfo))
				{
					domain = aDTopologyManagement.GetDomain();
				}
				if (domain != null)
				{
					domain.SessionInfo = base.CmdletSessionInfo.ADSessionInfo;
					domain.IsSearchResult = true;
					attributeSetRequest = base.ConstructAttributeSetRequest(propertiesToFetch);
					return this.Construct(domain, attributeSetRequest);
				}
				else
				{
					DebugLogger.LogInfo(this._debugCategory, string.Format("GetExtendedObjectFromIdentity: No objects returned from custom action", new object[0]));
					throw new ADIdentityNotFoundException(string.Format(CultureInfo.CurrentCulture, StringResources.ObjectNotFound, new object[0]));
				}
			}
			else
			{
				throw new ADIdentityNotFoundException(string.Format(CultureInfo.CurrentCulture, StringResources.ObjectNotFound, new object[0]));
			}
		}

		private static int GetNTMixedDomainMode(CmdletSessionInfo cmdletSessionInfo)
		{
			int value;
			ADObjectSearcher aDObjectSearcher = SearchUtility.BuildSearcher(cmdletSessionInfo.ADSessionInfo, cmdletSessionInfo.ADRootDSE.DefaultNamingContext, ADSearchScope.Base);
			using (aDObjectSearcher)
			{
				aDObjectSearcher.Filter = ADOPathUtil.CreateFilterClause(ADOperator.Like, "objectClass", "*");
				aDObjectSearcher.Properties.Add("ntMixedDomain");
				ADObject aDObject = aDObjectSearcher.FindOne();
				value = (int)aDObject["ntMixedDomain"].Value;
			}
			return value;
		}

		internal override IADOPathNode IdentitySearchConverter(object identity)
		{
			if (identity != null)
			{
				string str = identity as string;
				if (str != null)
				{
					string str1 = ADDomainUtil.FindDomainNCHead(str, base.CmdletSessionInfo);
					if (str1 != null)
					{
						return ADOPathUtil.CreateFilterClause(ADOperator.Eq, "distinguishedName", str1);
					}
				}
				SecurityIdentifier securityIdentifier = identity as SecurityIdentifier;
				if (securityIdentifier == null)
				{
					IADOPathNode aDOPathNode = base.IdentitySearchConverter(identity);
					return aDOPathNode;
				}
				else
				{
					return ADDomainUtil.CreateSidFilterClause(securityIdentifier);
				}
			}
			else
			{
				throw new ArgumentNullException("Identity");
			}
		}

		private static void UpdateNTMixedDomainMode(CmdletSessionInfo cmdletSessionInfo, int ntMixedDomainMode)
		{
			ADObject aDObject = new ADObject();
			aDObject.DistinguishedName = cmdletSessionInfo.ADRootDSE.DefaultNamingContext;
			using (ADActiveObject aDActiveObject = new ADActiveObject(cmdletSessionInfo.ADSessionInfo, aDObject))
			{
				aDObject.TrackChanges = true;
				aDObject.Add("ntMixedDomain", ntMixedDomainMode);
				aDActiveObject.Update();
			}
		}

		internal static class ADDomainPropertyMap
		{
			internal readonly static PropertyMapEntry LastLogonReplicationInterval;

			internal readonly static PropertyMapEntry AllowedDNSSuffixes;

			internal readonly static PropertyMapEntry ManagedBy;

			internal readonly static PropertyMapEntry DomainMode;

			internal readonly static PropertyMapEntry DomainSID;

			internal readonly static PropertyMapEntry LinkedGroupPolicyObjects;

			internal readonly static PropertyMapEntry ChildDomains;

			internal readonly static PropertyMapEntry ComputersContainer;

			internal readonly static PropertyMapEntry DomainControllersContainer;

			internal readonly static PropertyMapEntry ForeignSecurityPrincipalsContainer;

			internal readonly static PropertyMapEntry Forest;

			internal readonly static PropertyMapEntry InfrastructureMaster;

			internal readonly static PropertyMapEntry NetBIOSName;

			internal readonly static PropertyMapEntry PDCEmulator;

			internal readonly static PropertyMapEntry ParentDomain;

			internal readonly static PropertyMapEntry RIDMaster;

			internal readonly static PropertyMapEntry SystemsContainer;

			internal readonly static PropertyMapEntry UsersContainer;

			static ADDomainPropertyMap()
			{
				ADDomainFactory<T>.ADDomainPropertyMap.LastLogonReplicationInterval = new PropertyMapEntry("LastLogonReplicationInterval", "msDS-LogonTimeSyncInterval", null);
				ADDomainFactory<T>.ADDomainPropertyMap.AllowedDNSSuffixes = new PropertyMapEntry("AllowedDNSSuffixes", "msDS-AllowedDNSSuffixes", null);
				ADDomainFactory<T>.ADDomainPropertyMap.ManagedBy = new PropertyMapEntry("ManagedBy", "managedBy", null);
				ADDomainFactory<T>.ADDomainPropertyMap.DomainMode = new PropertyMapEntry("DomainMode", "msDS-Behavior-Version", null);
				ADDomainFactory<T>.ADDomainPropertyMap.DomainSID = new PropertyMapEntry("DomainSID", "objectSid", null);
				ADDomainFactory<T>.ADDomainPropertyMap.LinkedGroupPolicyObjects = new PropertyMapEntry("LinkedGroupPolicyObjects", "LinkedGroupPolicyObjects", "LinkedGroupPolicyObjects");
				ADDomainFactory<T>.ADDomainPropertyMap.ChildDomains = new PropertyMapEntry("ChildDomains", "ChildDomains", "ChildDomains");
				ADDomainFactory<T>.ADDomainPropertyMap.ComputersContainer = new PropertyMapEntry("ComputersContainer", "ComputersContainer", "ComputersContainer");
				ADDomainFactory<T>.ADDomainPropertyMap.DomainControllersContainer = new PropertyMapEntry("DomainControllersContainer", "DomainControllersContainer", "DomainControllersContainer");
				ADDomainFactory<T>.ADDomainPropertyMap.ForeignSecurityPrincipalsContainer = new PropertyMapEntry("ForeignSecurityPrincipalsContainer", "ForeignSecurityPrincipalsContainer", "ForeignSecurityPrincipalsContainer");
				ADDomainFactory<T>.ADDomainPropertyMap.Forest = new PropertyMapEntry("Forest", "Forest", "Forest");
				ADDomainFactory<T>.ADDomainPropertyMap.InfrastructureMaster = new PropertyMapEntry("InfrastructureMaster", "InfrastructureMaster", "InfrastructureMaster");
				ADDomainFactory<T>.ADDomainPropertyMap.NetBIOSName = new PropertyMapEntry("NetBIOSName", "NetBIOSName", "NetBIOSName");
				ADDomainFactory<T>.ADDomainPropertyMap.PDCEmulator = new PropertyMapEntry("PDCEmulator", "PDCEmulator", "PDCEmulator");
				ADDomainFactory<T>.ADDomainPropertyMap.ParentDomain = new PropertyMapEntry("ParentDomain", "ParentDomain", "ParentDomain");
				ADDomainFactory<T>.ADDomainPropertyMap.RIDMaster = new PropertyMapEntry("RIDMaster", "RIDMaster", "RIDMaster");
				ADDomainFactory<T>.ADDomainPropertyMap.SystemsContainer = new PropertyMapEntry("SystemsContainer", "SystemsContainer", "SystemsContainer");
				ADDomainFactory<T>.ADDomainPropertyMap.UsersContainer = new PropertyMapEntry("UsersContainer", "UsersContainer", "UsersContainer");
			}
		}
	}
}