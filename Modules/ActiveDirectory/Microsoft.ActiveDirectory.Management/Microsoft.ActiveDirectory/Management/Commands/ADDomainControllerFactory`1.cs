using Microsoft.ActiveDirectory;
using Microsoft.ActiveDirectory.Management;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Security.Principal;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public class ADDomainControllerFactory<T> : ADDirectoryServerFactory<T>
	where T : ADDomainController, new()
	{
		private const string _debugCategory = "ADDomainControllerFactory";

		private const string GROUP_RID_CONTROLLERS = "516";

		private const string GROUP_RID_READONLY_CONTROLLERS = "521";

		private readonly static IADOPathNode _domainControllerComputerObjectFilter;

		private readonly static string[] _domainControllerDefaultAttributes;

		private readonly static IADOPathNode _domainControllerStructuralFilter;

		private readonly static string[] _identityLdapAttributes;

		private static AttributeConverterEntry[] ADMappingTable;

		internal static IDictionary<ADServerType, MappingTable<AttributeConverterEntry>> AttributeTable
		{
			get
			{
				return ADDirectoryServerFactory<T>.AttributeTable;
			}
		}

		internal virtual string[] IdentityLdapAttributes
		{
			get
			{
				return ADDomainControllerFactory<T>._identityLdapAttributes;
			}
		}

		internal override string RDNPrefix
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		internal override string StructuralObjectClass
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		internal override IADOPathNode StructuralObjectFilter
		{
			get
			{
				return ADDomainControllerFactory<T>._domainControllerStructuralFilter;
			}
		}

		static ADDomainControllerFactory()
		{
			IADOPathNode[] aDOPathNodeArray = new IADOPathNode[2];
			aDOPathNodeArray[0] = ADOPathUtil.CreateFilterClause(ADOperator.Eq, "objectClass", "computer");
			IADOPathNode[] aDOPathNodeArray1 = new IADOPathNode[2];
			aDOPathNodeArray1[0] = ADOPathUtil.CreateFilterClause(ADOperator.Eq, "primaryGroupID", "516");
			aDOPathNodeArray1[1] = ADOPathUtil.CreateFilterClause(ADOperator.Eq, "primaryGroupID", "521");
			aDOPathNodeArray[1] = ADOPathUtil.CreateOrClause(aDOPathNodeArray1);
			ADDomainControllerFactory<T>._domainControllerComputerObjectFilter = ADOPathUtil.CreateAndClause(aDOPathNodeArray);
			string[] strArrays = new string[7];
			strArrays[0] = "name";
			strArrays[1] = "distinguishedName";
			strArrays[2] = "objectClass";
			strArrays[3] = "objectGUID";
			strArrays[4] = "dNSHostName";
			strArrays[5] = "serverReference";
			strArrays[6] = "serverReferenceBL";
			ADDomainControllerFactory<T>._domainControllerDefaultAttributes = strArrays;
			IADOPathNode[] aDOPathNodeArray2 = new IADOPathNode[3];
			aDOPathNodeArray2[0] = ADOPathUtil.CreateFilterClause(ADOperator.Eq, "objectClass", "nTDSDSA");
			aDOPathNodeArray2[1] = ADOPathUtil.CreateFilterClause(ADOperator.Eq, "objectClass", "server");
			aDOPathNodeArray2[2] = ADDomainControllerFactory<T>._domainControllerComputerObjectFilter;
			ADDomainControllerFactory<T>._domainControllerStructuralFilter = ADOPathUtil.CreateOrClause(aDOPathNodeArray2);
			string[] strArrays1 = new string[5];
			strArrays1[0] = "distinguishedName";
			strArrays1[1] = "objectGUID";
			strArrays1[2] = "dNSHostName";
			strArrays1[3] = "objectSid";
			strArrays1[4] = "sAMAccountName";
			ADDomainControllerFactory<T>._identityLdapAttributes = strArrays1;
			AttributeConverterEntry[] attributeConverterEntry = new AttributeConverterEntry[10];
			attributeConverterEntry[0] = new AttributeConverterEntry("IsReadOnly", "IsReadOnly", TypeConstants.Bool, false, TypeAdapterAccess.Read, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			attributeConverterEntry[1] = new AttributeConverterEntry("IsGlobalCatalog", "IsGlobalCatalog", TypeConstants.Bool, false, TypeAdapterAccess.Read, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			attributeConverterEntry[2] = new AttributeConverterEntry("Enabled", "Enabled", TypeConstants.Bool, false, TypeAdapterAccess.Read, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			attributeConverterEntry[3] = new AttributeConverterEntry("OperatingSystemVersion", "OSVersion", TypeConstants.String, false, TypeAdapterAccess.Read, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			attributeConverterEntry[4] = new AttributeConverterEntry("OperatingSystem", "OSName", TypeConstants.String, false, TypeAdapterAccess.Read, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			attributeConverterEntry[5] = new AttributeConverterEntry("OperatingSystemServicePack", "OSServicepack", TypeConstants.String, false, TypeAdapterAccess.Read, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			attributeConverterEntry[6] = new AttributeConverterEntry("OperatingSystemHotfix", "OSHotFix", TypeConstants.String, false, TypeAdapterAccess.Read, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			attributeConverterEntry[7] = new AttributeConverterEntry("ComputerObjectDN", "ComputerDN", TypeConstants.String, false, TypeAdapterAccess.Read, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			attributeConverterEntry[8] = new AttributeConverterEntry("Domain", "Domain", TypeConstants.String, false, TypeAdapterAccess.Read, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			attributeConverterEntry[9] = new AttributeConverterEntry("Forest", "Forest", TypeConstants.String, false, TypeAdapterAccess.Read, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			ADDomainControllerFactory<T>.ADMappingTable = attributeConverterEntry;
			ADFactoryBase<T>.RegisterMappingTable(ADDomainControllerFactory<T>.ADMappingTable, ADServerType.ADDS);
		}

		public ADDomainControllerFactory()
		{
		}

		internal virtual List<IADOPathNode> BuildIdentityFilterListFromString(string identity)
		{
			if (identity != null)
			{
				List<IADOPathNode> aDOPathNodes = new List<IADOPathNode>((int)this.IdentityLdapAttributes.Length);
				string[] identityLdapAttributes = this.IdentityLdapAttributes;
				for (int i = 0; i < (int)identityLdapAttributes.Length; i++)
				{
					string str = identityLdapAttributes[i];
					if (str != "distinguishedName")
					{
						aDOPathNodes.Add(ADOPathUtil.CreateFilterClause(ADOperator.Eq, str, identity));
					}
					else
					{
						string str1 = Utils.EscapeDNForFilter(identity);
						aDOPathNodes.Add(ADOPathUtil.CreateFilterClause(ADOperator.Eq, str, str1));
					}
				}
				return ADComputerUtil.BuildComputerSamAccountNameIdentityFilter(identity, aDOPathNodes);
			}
			else
			{
				throw new ArgumentNullException("identity");
			}
		}

		internal override string GenerateObjectName(ADParameterSet parameters)
		{
			throw new NotSupportedException();
		}

		internal IEnumerable<T> GetAllDomainControllers(ICollection<string> propertiesToFetch)
		{
			IEnumerable<T> ts;
			string defaultNamingContext = base.CmdletSessionInfo.ADRootDSE.DefaultNamingContext;
			ADSearchScope aDSearchScope = ADSearchScope.Subtree;
			IADOPathNode aDOPathNode = ADDomainControllerFactory<T>._domainControllerComputerObjectFilter;
			ADObjectSearcher aDObjectSearcher = SearchUtility.BuildSearcher(base.CmdletSessionInfo.ADSessionInfo, defaultNamingContext, aDSearchScope);
			using (aDObjectSearcher)
			{
				aDObjectSearcher.Filter = aDOPathNode;
				aDObjectSearcher.Properties.AddRange(ADDomainControllerFactory<T>._domainControllerDefaultAttributes);
				List<ADObject> aDObjects = new List<ADObject>();
				foreach (ADObject aDObject in aDObjectSearcher.FindAll())
				{
					aDObjects.Add(aDObject);
				}
				if (aDObjects.Count != 0)
				{
					List<string> strs = new List<string>();
					foreach (ADObject aDObject1 in aDObjects)
					{
						string value = aDObject1["serverReferenceBL"].Value as string;
						if (value == null)
						{
							DebugLogger.LogInfo("ADDomainControllerFactory", string.Format("Could  not find property: {0} for identity: {1}", "serverReferenceBL", aDObject1.DistinguishedName));
						}
						else
						{
							strs.Add(string.Concat("CN=NTDS Settings,", value));
						}
					}
					if (strs.Count <= 0)
					{
						ts = null;
					}
					else
					{
						using (ADTopologyManagement aDTopologyManagement = new ADTopologyManagement(base.CmdletSessionInfo.ADSessionInfo))
						{
							ADEntity[] domainController = aDTopologyManagement.GetDomainController(strs.ToArray());
							if (domainController == null || (int)domainController.Length == 0)
							{
								ts = new List<T>();
							}
							else
							{
								List<T> ts1 = new List<T>();
								AttributeSetRequest attributeSetRequest = this.ConstructAttributeSetRequest(propertiesToFetch);
								ADEntity[] aDEntityArray = domainController;
								for (int i = 0; i < (int)aDEntityArray.Length; i++)
								{
									ADEntity aDEntity = aDEntityArray[i];
									T aDSessionInfo = this.Construct(aDEntity, attributeSetRequest);
									aDSessionInfo.SessionInfo = base.CmdletSessionInfo.ADSessionInfo;
									ts1.Add(aDSessionInfo);
								}
								ts = ts1;
							}
						}
					}
				}
				else
				{
					ts = new List<T>();
				}
			}
			return ts;
		}

		internal override ADObject GetDirectoryObjectFromIdentity(T identityObj, string searchRoot, bool showDeleted)
		{
			return base.GetDirectoryObjectFromIdentity(identityObj, searchRoot, showDeleted);
		}

		internal ADObject GetDirectoryObjectFromIdentity(T identityObj, string searchRoot, ICollection<string> propertiesToFetch)
		{
			ADObject aDObject = null;
			ADObject aDObject1 = null;
			ADObject aDObject2 = null;
			HashSet<string> strs = new HashSet<string>(ADDomainControllerFactory<T>._domainControllerDefaultAttributes, StringComparer.OrdinalIgnoreCase);
			if (propertiesToFetch != null)
			{
				strs.UnionWith(propertiesToFetch);
			}
			string[] strArrays = new string[strs.Count];
			strs.CopyTo(strArrays);
			string nTDSSettingsDN = base.ResolveIdentityToNTDSSettingsDN(identityObj, strArrays, true, out aDObject, out aDObject1, out aDObject2);
			if (nTDSSettingsDN != null)
			{
				ADObject aDSessionInfo = null;
				if (aDObject == null)
				{
					string value = null;
					if (aDObject1 == null)
					{
						string str = nTDSSettingsDN.Substring("CN=NTDS Settings,".Length);
						ADObjectSearcher aDObjectSearcher = SearchUtility.BuildSearcher(base.CmdletSessionInfo.ADSessionInfo, str, ADSearchScope.Base);
						using (aDObjectSearcher)
						{
							aDObjectSearcher.Filter = ADOPathUtil.CreateFilterClause(ADOperator.Like, "objectClass", "*");
							string[] strArrays1 = new string[1];
							strArrays1[0] = "serverReference";
							aDObjectSearcher.Properties.AddRange(strArrays1);
							ADObject aDObject3 = aDObjectSearcher.FindOne();
							if (aDObject3 != null)
							{
								value = aDObject3["serverReference"].Value as string;
							}
						}
					}
					else
					{
						value = aDObject1["serverReference"].Value as string;
					}
					if (value != null)
					{
						ADObjectSearcher aDObjectSearcher1 = SearchUtility.BuildSearcher(base.CmdletSessionInfo.ADSessionInfo, value, ADSearchScope.Base);
						using (aDObjectSearcher1)
						{
							aDObjectSearcher1.Filter = ADOPathUtil.CreateFilterClause(ADOperator.Like, "objectClass", "*");
							aDObjectSearcher1.Properties.AddRange(strArrays);
							aDSessionInfo = aDObjectSearcher1.FindOne();
							if (aDSessionInfo == null)
							{
								DebugLogger.LogInfo("ADDomainControllerFactory", string.Format("GetDirectoryObjectFromIdentity: Identity not found. Filter used: {0}", value));
								object[] objArray = new object[2];
								objArray[0] = nTDSSettingsDN;
								objArray[1] = aDObjectSearcher1.SearchRoot;
								throw new ADIdentityNotFoundException(string.Format(CultureInfo.CurrentCulture, StringResources.IdentityNotFound, objArray));
							}
						}
					}
					else
					{
						object[] identity = new object[1];
						identity[0] = identityObj.Identity;
						throw new ADIdentityNotFoundException(string.Format(CultureInfo.CurrentCulture, StringResources.DirectoryServerNotFound, identity));
					}
				}
				else
				{
					aDSessionInfo = aDObject;
				}
				aDSessionInfo.TrackChanges = true;
				aDSessionInfo.SessionInfo = base.CmdletSessionInfo.ADSessionInfo;
				return aDSessionInfo;
			}
			else
			{
				DebugLogger.LogInfo("ADDomainControllerFactory", string.Format("GetDirectoryObjectFromIdentity: NTDS Settings DN for the given directory server identity not found", new object[0]));
				object[] identity1 = new object[1];
				identity1[0] = identityObj.Identity;
				throw new ADIdentityNotFoundException(string.Format(CultureInfo.CurrentCulture, StringResources.DirectoryServerNotFound, identity1));
			}
		}

		internal override T GetExtendedObjectFromIdentity(T identityObj, string identityQueryPath, ICollection<string> propertiesToFetch, bool showDeleted)
		{
			ADObject aDObject = null;
			ADObject aDObject1 = null;
			ADObject aDObject2 = null;
			T t;
			string nTDSSettingsDN = base.ResolveIdentityToNTDSSettingsDN(identityObj, null, true, out aDObject, out aDObject1, out aDObject2);
			if (nTDSSettingsDN != null)
			{
				using (ADTopologyManagement aDTopologyManagement = new ADTopologyManagement(base.CmdletSessionInfo.ADSessionInfo))
				{
					string[] strArrays = new string[1];
					strArrays[0] = nTDSSettingsDN;
					ADEntity[] domainController = aDTopologyManagement.GetDomainController(strArrays);
					if (domainController == null || (int)domainController.Length == 0)
					{
						DebugLogger.LogInfo("ADDomainControllerFactory", string.Format("GetExtendedObjectFromIdentity: No objects returned from custom action", new object[0]));
						throw new ADIdentityNotFoundException(string.Format(CultureInfo.CurrentCulture, StringResources.ObjectNotFound, new object[0]));
					}
					else
					{
						AttributeSetRequest attributeSetRequest = this.ConstructAttributeSetRequest(propertiesToFetch);
						T aDSessionInfo = this.Construct(domainController[0], attributeSetRequest);
						aDSessionInfo.SessionInfo = base.CmdletSessionInfo.ADSessionInfo;
						t = aDSessionInfo;
					}
				}
				return t;
			}
			else
			{
				DebugLogger.LogInfo("ADDomainControllerFactory", string.Format("GetExtendedObjectFromIdentity: Could not find the DC's NTDS Settings DN", new object[0]));
				object[] identity = new object[1];
				identity[0] = identityObj.Identity;
				throw new ADIdentityNotFoundException(string.Format(CultureInfo.CurrentCulture, StringResources.DirectoryServerNotFound, identity));
			}
		}

		internal override IADOPathNode IdentitySearchConverter(object identity)
		{
			IADOPathNode aDOPathNode;
			if (identity != null)
			{
				string str = identity as string;
				if (str == null)
				{
					SecurityIdentifier securityIdentifier = identity as SecurityIdentifier;
					if (securityIdentifier == null)
					{
						if (!(identity is Guid))
						{
							ADDomainController aDDomainController = identity as ADDomainController;
							if (aDDomainController == null)
							{
								ADObject aDObject = identity as ADObject;
								if (aDObject == null)
								{
									throw new ArgumentException(string.Format(StringResources.SearchConverterUnrecognizedObjectType, identity.GetType()));
								}
								else
								{
									ADComputer aDComputer = new ADComputer(aDObject);
									ADDomainControllerFactory<T>.ADComputerPseudoFactory aDComputerPseudoFactory = new ADDomainControllerFactory<T>.ADComputerPseudoFactory();
									aDComputerPseudoFactory.SetCmdletSessionInfo(base.CmdletSessionInfo);
									return aDComputerPseudoFactory.BuildIdentityFilter(aDComputer);
								}
							}
							else
							{
								IADOPathNode aDOPathNode1 = base.IdentitySearchConverter(identity);
								List<IADOPathNode> aDOPathNodes = new List<IADOPathNode>();
								if (aDDomainController["ComputerDN"] != null && aDDomainController["ComputerDN"].Value != null)
								{
									aDOPathNodes.Add(ADOPathUtil.CreateFilterClause(ADOperator.Eq, "distinguishedName", Utils.EscapeDNForFilter(aDDomainController["ComputerDN"].Value as string)));
								}
								aDOPathNodes.Add(aDOPathNode1);
								if (aDOPathNodes.Count != 1)
								{
									return ADOPathUtil.CreateOrClause(aDOPathNodes.ToArray());
								}
								else
								{
									return aDOPathNodes[0];
								}
							}
						}
						else
						{
							Guid guid = (Guid)identity;
							return ADOPathUtil.CreateFilterClause(ADOperator.Eq, "objectGuid", guid.ToByteArray());
						}
					}
					else
					{
						byte[] numArray = new byte[securityIdentifier.BinaryLength];
						securityIdentifier.GetBinaryForm(numArray, 0);
						return ADOPathUtil.CreateFilterClause(ADOperator.Eq, "objectSid", numArray);
					}
				}
				else
				{
					Guid? nullable = null;
					IPAddress pAddress = null;
					if (!Utils.TryParseGuid(str, out nullable))
					{
						if (!IPAddress.TryParse(str, out pAddress))
						{
							List<IADOPathNode> aDOPathNodes1 = this.BuildIdentityFilterListFromString(str);
							if (aDOPathNodes1.Count <= 1)
							{
								return aDOPathNodes1[0];
							}
							else
							{
								return ADOPathUtil.CreateOrClause(aDOPathNodes1.ToArray());
							}
						}
						else
						{
							try
							{
								IPHostEntry hostEntry = Dns.GetHostEntry(pAddress);
								aDOPathNode = ADOPathUtil.CreateFilterClause(ADOperator.Eq, "dNSHostName", hostEntry.HostName);
							}
							catch (SocketException socketException1)
							{
								SocketException socketException = socketException1;
								object[] objArray = new object[1];
								objArray[0] = pAddress;
								throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.CannotResolveIPAddressToHostName, objArray), socketException);
							}
							return aDOPathNode;
						}
					}
					else
					{
						Guid value = nullable.Value;
						return ADOPathUtil.CreateFilterClause(ADOperator.Eq, "objectGuid", value.ToByteArray());
					}
				}
			}
			else
			{
				throw new ArgumentNullException("identity");
			}
		}

		private class ADComputerPseudoFactory : ADComputerFactory<ADComputer>
		{
			private readonly static string[] _identityLdapAttributes;

			internal override string[] IdentityLdapAttributes
			{
				get
				{
					return ADDomainControllerFactory<T>.ADComputerPseudoFactory._identityLdapAttributes;
				}
			}

			static ADComputerPseudoFactory()
			{
				string[] strArrays = new string[4];
				strArrays[0] = "sAMAccountName";
				strArrays[1] = "distinguishedName";
				strArrays[2] = "objectSid";
				strArrays[3] = "objectGUID";
				ADDomainControllerFactory<T>.ADComputerPseudoFactory._identityLdapAttributes = strArrays;
			}

			public ADComputerPseudoFactory()
			{
			}

			internal override List<IADOPathNode> BuildIdentityFilterListFromString(string identity)
			{
				List<IADOPathNode> aDOPathNodes = base.BuildIdentityFilterListFromString(identity);
				return ADComputerUtil.BuildComputerSamAccountNameIdentityFilter(identity, aDOPathNodes);
			}
		}
	}
}