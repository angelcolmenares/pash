using Microsoft.ActiveDirectory;
using Microsoft.ActiveDirectory.Management;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Principal;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public class ADDefaultDomainPasswordPolicyFactory<T> : ADFactory<T>
	where T : ADDefaultDomainPasswordPolicy, new()
	{
		private const string _debugCategory = "ADDefaultDomainPasswordPolicyFactory";

		private static IADOPathNode _ddppStructuralFilter;

		private static string[] _ddppIdentityLdapAttributes;

		private static AttributeConverterEntry[] ADMappingTable;

		internal static IDictionary<ADServerType, MappingTable<AttributeConverterEntry>> AttributeTable
		{
			get
			{
				return ADFactoryBase<T>.AttributeTable;
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
				return "domainDNS";
			}
		}

		internal override IADOPathNode StructuralObjectFilter
		{
			get
			{
				return ADDefaultDomainPasswordPolicyFactory<T>._ddppStructuralFilter;
			}
		}

		static ADDefaultDomainPasswordPolicyFactory()
		{
			ADDefaultDomainPasswordPolicyFactory<T>._ddppStructuralFilter = null;
			ADDefaultDomainPasswordPolicyFactory<T>._ddppIdentityLdapAttributes = null;
			AttributeConverterEntry[] attributeConverterEntry = new AttributeConverterEntry[10];
			attributeConverterEntry[0] = new AttributeConverterEntry(ADDefaultDomainPasswordPolicyFactory<T>.ADDefaultDomainPasswordPolicyPropertyMap.DistinguishedName.PropertyName, ADDefaultDomainPasswordPolicyFactory<T>.ADDefaultDomainPasswordPolicyPropertyMap.DistinguishedName.ADAttribute, TypeConstants.String, true, TypeAdapterAccess.Read, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), null);
			attributeConverterEntry[1] = new AttributeConverterEntry(ADDefaultDomainPasswordPolicyFactory<T>.ADDefaultDomainPasswordPolicyPropertyMap.LockoutDuration.PropertyName, ADDefaultDomainPasswordPolicyFactory<T>.ADDefaultDomainPasswordPolicyPropertyMap.LockoutDuration.ADAttribute, TypeConstants.TimeSpan, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedNoExpirationTimeSpan), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryNegativeTimeSpan), null);
			attributeConverterEntry[2] = new AttributeConverterEntry(ADDefaultDomainPasswordPolicyFactory<T>.ADDefaultDomainPasswordPolicyPropertyMap.LockoutObservationWindow.PropertyName, ADDefaultDomainPasswordPolicyFactory<T>.ADDefaultDomainPasswordPolicyPropertyMap.LockoutObservationWindow.ADAttribute, TypeConstants.TimeSpan, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedNoExpirationTimeSpan), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryNegativeTimeSpan), null);
			attributeConverterEntry[3] = new AttributeConverterEntry(ADDefaultDomainPasswordPolicyFactory<T>.ADDefaultDomainPasswordPolicyPropertyMap.LockoutThreshold.PropertyName, ADDefaultDomainPasswordPolicyFactory<T>.ADDefaultDomainPasswordPolicyPropertyMap.LockoutThreshold.ADAttribute, TypeConstants.Int, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), null);
			attributeConverterEntry[4] = new AttributeConverterEntry(ADDefaultDomainPasswordPolicyFactory<T>.ADDefaultDomainPasswordPolicyPropertyMap.MaxPasswordAge.PropertyName, ADDefaultDomainPasswordPolicyFactory<T>.ADDefaultDomainPasswordPolicyPropertyMap.MaxPasswordAge.ADAttribute, TypeConstants.TimeSpan, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedNoExpirationTimeSpan), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryNoExpirationTimeSpan), null);
			attributeConverterEntry[5] = new AttributeConverterEntry(ADDefaultDomainPasswordPolicyFactory<T>.ADDefaultDomainPasswordPolicyPropertyMap.MinPasswordAge.PropertyName, ADDefaultDomainPasswordPolicyFactory<T>.ADDefaultDomainPasswordPolicyPropertyMap.MinPasswordAge.ADAttribute, TypeConstants.TimeSpan, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedNoExpirationTimeSpan), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryNegativeTimeSpan), null);
			attributeConverterEntry[6] = new AttributeConverterEntry(ADDefaultDomainPasswordPolicyFactory<T>.ADDefaultDomainPasswordPolicyPropertyMap.MinPasswordLength.PropertyName, ADDefaultDomainPasswordPolicyFactory<T>.ADDefaultDomainPasswordPolicyPropertyMap.MinPasswordLength.ADAttribute, TypeConstants.Int, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), null);
			attributeConverterEntry[7] = new AttributeConverterEntry(ADDefaultDomainPasswordPolicyFactory<T>.ADDefaultDomainPasswordPolicyPropertyMap.PasswordHistoryCount.PropertyName, ADDefaultDomainPasswordPolicyFactory<T>.ADDefaultDomainPasswordPolicyPropertyMap.PasswordHistoryCount.ADAttribute, TypeConstants.Int, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), null);
			attributeConverterEntry[8] = new AttributeConverterEntry(ADDefaultDomainPasswordPolicyFactory<T>.ADDefaultDomainPasswordPolicyPropertyMap.ComplexityEnabled.PropertyName, ADDefaultDomainPasswordPolicyFactory<T>.ADDefaultDomainPasswordPolicyPropertyMap.ComplexityEnabled.ADAttribute, TypeConstants.Bool, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Default, new ToExtendedFormatDelegate(ADDefaultDomainPasswordPolicyFactory<T>.ToExtendedPasswordProperties), new ToDirectoryFormatDelegate(ADDefaultDomainPasswordPolicyFactory<T>.ToDirectoryPasswordProperties), null);
			attributeConverterEntry[9] = new AttributeConverterEntry(ADDefaultDomainPasswordPolicyFactory<T>.ADDefaultDomainPasswordPolicyPropertyMap.ReversibleEncryptionEnabled.PropertyName, ADDefaultDomainPasswordPolicyFactory<T>.ADDefaultDomainPasswordPolicyPropertyMap.ReversibleEncryptionEnabled.ADAttribute, TypeConstants.Bool, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Default, new ToExtendedFormatDelegate(ADDefaultDomainPasswordPolicyFactory<T>.ToExtendedPasswordProperties), new ToDirectoryFormatDelegate(ADDefaultDomainPasswordPolicyFactory<T>.ToDirectoryPasswordProperties), null);
			ADDefaultDomainPasswordPolicyFactory<T>.ADMappingTable = attributeConverterEntry;
			ADDefaultDomainPasswordPolicyFactory<T>._ddppStructuralFilter = ADOPathUtil.CreateFilterClause(ADOperator.Eq, "objectClass", "domainDNS");
			string[] strArrays = new string[3];
			strArrays[0] = "distinguishedName";
			strArrays[1] = "objectSid";
			strArrays[2] = "objectGUID";
			ADDefaultDomainPasswordPolicyFactory<T>._ddppIdentityLdapAttributes = strArrays;
			ADFactoryBase<T>.RegisterMappingTable(ADDefaultDomainPasswordPolicyFactory<T>.ADMappingTable, ADServerType.ADDS);
		}

		public ADDefaultDomainPasswordPolicyFactory()
		{
		}

		internal override ADObject GetDirectoryObjectFromIdentity(T identityObj, string searchRoot, bool showDeleted)
		{
			ADObject aDSessionInfo;
			if (base.CmdletSessionInfo != null)
			{
				string defaultNamingContext = base.CmdletSessionInfo.ADRootDSE.DefaultNamingContext;
				ADObjectSearcher aDObjectSearcher = SearchUtility.BuildSearcher(base.CmdletSessionInfo.ADSessionInfo, defaultNamingContext, ADSearchScope.Base);
				using (aDObjectSearcher)
				{
					AttributeSetRequest attributeSetRequest = this.ConstructAttributeSetRequest(null);
					aDObjectSearcher.Properties.AddRange(attributeSetRequest.DirectoryAttributes);
					IADOPathNode[] structuralObjectFilter = new IADOPathNode[2];
					structuralObjectFilter[0] = this.StructuralObjectFilter;
					structuralObjectFilter[1] = this.BuildIdentityFilter(identityObj);
					aDObjectSearcher.Filter = ADOPathUtil.CreateAndClause(structuralObjectFilter);
					DebugLogger.LogInfo("ADDefaultDomainPasswordPolicyFactory", string.Format("GetDirectoryObjectFromIdentity: Searching for identity using filter: {0} searchbase: {1}", aDObjectSearcher.Filter.GetLdapFilterString(), aDObjectSearcher.SearchRoot));
					aDSessionInfo = aDObjectSearcher.FindOne();
					if (aDSessionInfo == null)
					{
						DebugLogger.LogInfo("ADDefaultDomainPasswordPolicyFactory", string.Format("GetDirectoryObjectFromIdentity: Identity not found.", new object[0]));
						object[] str = new object[2];
						str[0] = identityObj.ToString();
						str[1] = aDObjectSearcher.SearchRoot;
						throw new ADIdentityNotFoundException(string.Format(CultureInfo.CurrentCulture, StringResources.IdentityNotFound, str));
					}
				}
				if (!aDSessionInfo.ObjectClass.Equals("domainDNS", StringComparison.OrdinalIgnoreCase))
				{
					object[] objArray = new object[2];
					objArray[0] = identityObj.ToString();
					objArray[1] = defaultNamingContext;
					throw new ADIdentityNotFoundException(string.Format(CultureInfo.CurrentCulture, StringResources.IdentityNotFound, objArray));
				}
				else
				{
					aDSessionInfo.TrackChanges = true;
					aDSessionInfo.SessionInfo = base.CmdletSessionInfo.ADSessionInfo;
					return aDSessionInfo;
				}
			}
			else
			{
				throw new ArgumentNullException(StringResources.SessionRequired);
			}
		}

		internal override T GetExtendedObjectFromIdentity(T identityObj, string identityQueryPath, ICollection<string> propertiesToFetch, bool showDeleted)
		{
			ADObject aDObject;
			ADDefaultDomainPasswordPolicy aDDefaultDomainPasswordPolicy = identityObj;
			if (aDDefaultDomainPasswordPolicy != null)
			{
				if (base.CmdletSessionInfo != null)
				{
					AttributeSetRequest attributeSetRequest = this.ConstructAttributeSetRequest(propertiesToFetch);
					IADOPathNode[] structuralObjectFilter = new IADOPathNode[2];
					structuralObjectFilter[0] = this.StructuralObjectFilter;
					structuralObjectFilter[1] = this.BuildIdentityFilter(identityObj);
					IADOPathNode aDOPathNode = ADOPathUtil.CreateAndClause(structuralObjectFilter);
					string defaultNamingContext = base.CmdletSessionInfo.ADRootDSE.DefaultNamingContext;
					ADSearchScope aDSearchScope = ADSearchScope.Base;
					ADObjectSearcher aDObjectSearcher = SearchUtility.BuildSearcher(base.CmdletSessionInfo.ADSessionInfo, defaultNamingContext, aDSearchScope, showDeleted);
					using (aDObjectSearcher)
					{
						aDObjectSearcher.Filter = aDOPathNode;
						aDObjectSearcher.Properties.AddRange(attributeSetRequest.DirectoryAttributes);
						DebugLogger.LogInfo("ADDefaultDomainPasswordPolicyFactory", string.Format("ADFactory: GetExtendedObjectFromIdentity: Searching for identity using filter: {0} searchbase: {1} scope: {2}", aDObjectSearcher.Filter.GetLdapFilterString(), aDObjectSearcher.SearchRoot, aDObjectSearcher.Scope));
						aDObject = aDObjectSearcher.FindOne();
						if (aDObject == null)
						{
							DebugLogger.LogInfo("ADDefaultDomainPasswordPolicyFactory", string.Format("ADFactory: GetExtendedObjectFromIdentity: Identity not found", new object[0]));
							object[] str = new object[2];
							str[0] = identityObj.ToString();
							str[1] = aDObjectSearcher.SearchRoot;
							throw new ADIdentityNotFoundException(string.Format(CultureInfo.CurrentCulture, StringResources.IdentityNotFound, str));
						}
					}
					T aDSessionInfo = this.Construct(aDObject, attributeSetRequest);
					aDSessionInfo.SessionInfo = base.CmdletSessionInfo.ADSessionInfo;
					return aDSessionInfo;
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

		internal override IADOPathNode IdentitySearchConverter(object identity)
		{
			if (identity != null)
			{
				if (identity as ADDefaultDomainPasswordPolicy != null)
				{
					ADDefaultDomainPasswordPolicy aDDefaultDomainPasswordPolicy = (ADDefaultDomainPasswordPolicy)identity;
					if (aDDefaultDomainPasswordPolicy.DistinguishedName == null)
					{
						while (identity as ADDefaultDomainPasswordPolicy != null)
						{
							identity = ((ADDefaultDomainPasswordPolicy)identity).Identity;
						}
					}
					else
					{
						identity = aDDefaultDomainPasswordPolicy.DistinguishedName;
					}
				}
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
					if (identity as string == null)
					{
						if (!(identity is Guid))
						{
							if (identity as ADObject == null)
							{
								throw new ArgumentException(string.Format(StringResources.SearchConverterUnrecognizedObjectType, identity.GetType()));
							}
							else
							{
								ADObject aDObject = identity as ADObject;
								List<IADOPathNode> aDOPathNodes = new List<IADOPathNode>(2);
								if (!string.IsNullOrEmpty(aDObject.DistinguishedName))
								{
									aDOPathNodes.Add(ADOPathUtil.CreateFilterClause(ADOperator.Eq, "distinguishedName", aDObject.DistinguishedName));
								}
								Guid? objectGuid = aDObject.ObjectGuid;
								if (objectGuid.HasValue)
								{
									Guid? nullable = aDObject.ObjectGuid;
									Guid value = nullable.Value;
									aDOPathNodes.Add(ADOPathUtil.CreateFilterClause(ADOperator.Eq, "objectGuid", value.ToByteArray()));
								}
								if (aDOPathNodes.Count != 0)
								{
									return ADOPathUtil.CreateAndClause(aDOPathNodes.ToArray());
								}
								else
								{
									throw new ArgumentException(StringResources.SearchConverterIdentityAttributeNotSet);
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
						Guid? nullable1 = null;
						if (!Utils.TryParseGuid(str, out nullable1))
						{
							List<IADOPathNode> aDOPathNodes1 = new List<IADOPathNode>((int)ADDefaultDomainPasswordPolicyFactory<T>._ddppIdentityLdapAttributes.Length);
							string[] strArrays = ADDefaultDomainPasswordPolicyFactory<T>._ddppIdentityLdapAttributes;
							for (int i = 0; i < (int)strArrays.Length; i++)
							{
								string str2 = strArrays[i];
								aDOPathNodes1.Add(ADOPathUtil.CreateFilterClause(ADOperator.Eq, str2, str));
							}
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
							Guid value1 = nullable1.Value;
							return ADOPathUtil.CreateFilterClause(ADOperator.Eq, "objectGuid", value1.ToByteArray());
						}
					}
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

		internal static void ToDirectoryPasswordProperties(string extendedAttribute, string[] directoryAttributes, ADPropertyValueCollection extendedData, ADEntity directoryObj, CmdletSessionInfo cmdletSessionInfo)
		{
			if (!directoryObj.Contains("pwdProperties"))
			{
				throw new NotSupportedException();
			}
			else
			{
				int value = (int)directoryObj["pwdProperties"].Value;
				bool item = (bool)extendedData[0];
				int bit = PasswordPropertiesUtil.StringToBit(extendedAttribute);
				if (PasswordPropertiesUtil.IsInverseBit(bit))
				{
					item = !item;
				}
				if (!item)
				{
					value = value & ~bit;
				}
				else
				{
					value = value | bit;
				}
				directoryObj.SetValue("pwdProperties", value);
				return;
			}
		}

		internal static void ToExtendedPasswordProperties(string extendedAttribute, string[] directoryAttributes, ADEntity userObj, ADEntity directoryObj, CmdletSessionInfo cmdletSessionInfo)
		{
			bool flag;
			if (directoryObj.Contains(directoryAttributes[0]))
			{
				int value = (int)directoryObj[directoryAttributes[0]].Value;
				int bit = PasswordPropertiesUtil.StringToBit(extendedAttribute);
				if (!PasswordPropertiesUtil.IsInverseBit(bit))
				{
					flag = (value & bit) != 0;
				}
				else
				{
					flag = (value & bit) == 0;
				}
				userObj.Add(extendedAttribute, flag);
				return;
			}
			else
			{
				return;
			}
		}

		internal static class ADDefaultDomainPasswordPolicyPropertyMap
		{
			internal readonly static PropertyMapEntry DistinguishedName;

			internal readonly static PropertyMapEntry LockoutDuration;

			internal readonly static PropertyMapEntry LockoutObservationWindow;

			internal readonly static PropertyMapEntry LockoutThreshold;

			internal readonly static PropertyMapEntry MaxPasswordAge;

			internal readonly static PropertyMapEntry MinPasswordAge;

			internal readonly static PropertyMapEntry MinPasswordLength;

			internal readonly static PropertyMapEntry PasswordHistoryCount;

			internal readonly static PropertyMapEntry ComplexityEnabled;

			internal readonly static PropertyMapEntry ReversibleEncryptionEnabled;

			static ADDefaultDomainPasswordPolicyPropertyMap()
			{
				ADDefaultDomainPasswordPolicyFactory<T>.ADDefaultDomainPasswordPolicyPropertyMap.DistinguishedName = new PropertyMapEntry("DistinguishedName", "distinguishedName", null);
				ADDefaultDomainPasswordPolicyFactory<T>.ADDefaultDomainPasswordPolicyPropertyMap.LockoutDuration = new PropertyMapEntry("LockoutDuration", "lockoutDuration", null);
				ADDefaultDomainPasswordPolicyFactory<T>.ADDefaultDomainPasswordPolicyPropertyMap.LockoutObservationWindow = new PropertyMapEntry("LockoutObservationWindow", "lockoutObservationWindow", null);
				ADDefaultDomainPasswordPolicyFactory<T>.ADDefaultDomainPasswordPolicyPropertyMap.LockoutThreshold = new PropertyMapEntry("LockoutThreshold", "lockoutThreshold", null);
				ADDefaultDomainPasswordPolicyFactory<T>.ADDefaultDomainPasswordPolicyPropertyMap.MaxPasswordAge = new PropertyMapEntry("MaxPasswordAge", "maxPwdAge", null);
				ADDefaultDomainPasswordPolicyFactory<T>.ADDefaultDomainPasswordPolicyPropertyMap.MinPasswordAge = new PropertyMapEntry("MinPasswordAge", "minPwdAge", null);
				ADDefaultDomainPasswordPolicyFactory<T>.ADDefaultDomainPasswordPolicyPropertyMap.MinPasswordLength = new PropertyMapEntry("MinPasswordLength", "minPwdLength", null);
				ADDefaultDomainPasswordPolicyFactory<T>.ADDefaultDomainPasswordPolicyPropertyMap.PasswordHistoryCount = new PropertyMapEntry("PasswordHistoryCount", "pwdHistoryLength", null);
				ADDefaultDomainPasswordPolicyFactory<T>.ADDefaultDomainPasswordPolicyPropertyMap.ComplexityEnabled = new PropertyMapEntry("ComplexityEnabled", "pwdProperties", null);
				ADDefaultDomainPasswordPolicyFactory<T>.ADDefaultDomainPasswordPolicyPropertyMap.ReversibleEncryptionEnabled = new PropertyMapEntry("ReversibleEncryptionEnabled", "pwdProperties", null);
			}
		}
	}
}