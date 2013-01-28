using Microsoft.ActiveDirectory;
using Microsoft.ActiveDirectory.Management;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public class ADObjectFactory<T> : ADFactory<T>
	where T : ADObject, new()
	{
		private readonly static IADOPathNode _structuralObjectFilter;

		private readonly static string _rDNPrefix;

		private readonly static string _structuralObjectClass;

		private readonly static string[] _identityLdapAttributes;

		private readonly static IdentityResolverDelegate[] _identityResolvers;

		private static AttributeConverterEntry[] ADMappingTable;

		private static AttributeConverterEntry[] ADAMMappingTable;

		internal static IDictionary<ADServerType, MappingTable<AttributeConverterEntry>> AttributeTable
		{
			get
			{
				return ADFactoryBase<T>.AttributeTable;
			}
		}

		internal virtual string[] IdentityLdapAttributes
		{
			get
			{
				return ADObjectFactory<T>._identityLdapAttributes;
			}
		}

		internal override IdentityResolverDelegate[] IdentityResolvers
		{
			get
			{
				return ADObjectFactory<T>._identityResolvers;
			}
		}

		internal override string RDNPrefix
		{
			get
			{
				return ADObjectFactory<T>._rDNPrefix;
			}
		}

		internal override string StructuralObjectClass
		{
			get
			{
				return ADObjectFactory<T>._structuralObjectClass;
			}
		}

		internal override IADOPathNode StructuralObjectFilter
		{
			get
			{
				return ADObjectFactory<T>._structuralObjectFilter;
			}
		}

		static ADObjectFactory()
		{
			ADObjectFactory<T>._structuralObjectFilter = ADOPathUtil.CreateFilterClause(ADOperator.Like, "objectClass", "*");
			ADObjectFactory<T>._rDNPrefix = "CN";
			ADObjectFactory<T>._structuralObjectClass = "top";
			ADObjectFactory<T>._identityLdapAttributes = new string[0];
			IdentityResolverDelegate[] customIdentityResolver = new IdentityResolverDelegate[2];
			customIdentityResolver[0] = IdentityResolverMethods.GetCustomIdentityResolver(new IdentityResolverDelegate(IdentityResolverMethods.DistinguishedNameIdentityResolver));
			customIdentityResolver[1] = IdentityResolverMethods.GetCustomIdentityResolver(new IdentityResolverDelegate(IdentityResolverMethods.GuidIdentityResolver));
			ADObjectFactory<T>._identityResolvers = customIdentityResolver;
			AttributeConverterEntry[] attributeConverterEntry = new AttributeConverterEntry[14];
			attributeConverterEntry[0] = new AttributeConverterEntry(ADObjectFactory<T>.ADObjectPropertyMap.DistinguishedName.PropertyName, ADObjectFactory<T>.ADObjectPropertyMap.DistinguishedName.ADAttribute, TypeConstants.String, false, TypeAdapterAccess.Read, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), null, new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[1] = new AttributeConverterEntry(ADObjectFactory<T>.ADObjectPropertyMap.ObjectClass.PropertyName, ADObjectFactory<T>.ADObjectPropertyMap.ObjectClass.ADAttribute, TypeConstants.String, false, TypeAdapterAccess.Read, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), null, new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[2] = new AttributeConverterEntry(ADObjectFactory<T>.ADObjectPropertyMap.CanonicalName.PropertyName, ADObjectFactory<T>.ADObjectPropertyMap.CanonicalName.ADAttribute, TypeConstants.String, false, TypeAdapterAccess.Read, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), null, new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[3] = new AttributeConverterEntry(ADObjectFactory<T>.ADObjectPropertyMap.GUID.PropertyName, ADObjectFactory<T>.ADObjectPropertyMap.GUID.ADAttribute, TypeConstants.Guid, false, TypeAdapterAccess.Read, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), null, new ToSearchFilterDelegate(SearchConverters.ToSearchGuid));
			attributeConverterEntry[4] = new AttributeConverterEntry(ADObjectFactory<T>.ADObjectPropertyMap.CN.PropertyName, ADObjectFactory<T>.ADObjectPropertyMap.CN.ADAttribute, TypeConstants.String, false, TypeAdapterAccess.Read, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[5] = new AttributeConverterEntry(ADObjectFactory<T>.ADObjectPropertyMap.CreationTimeStamp.PropertyName, ADObjectFactory<T>.ADObjectPropertyMap.CreationTimeStamp.ADAttribute, TypeConstants.DateTime, false, TypeAdapterAccess.Read, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedDateTimeFromDateTime), null, new ToSearchFilterDelegate(SearchConverters.ToSearchDateTimeUsingSchemaInfo));
			attributeConverterEntry[6] = new AttributeConverterEntry(ADObjectFactory<T>.ADObjectPropertyMap.Description.PropertyName, ADObjectFactory<T>.ADObjectPropertyMap.Description.ADAttribute, TypeConstants.String, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[7] = new AttributeConverterEntry(ADObjectFactory<T>.ADObjectPropertyMap.DisplayName.PropertyName, ADObjectFactory<T>.ADObjectPropertyMap.DisplayName.ADAttribute, TypeConstants.String, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[8] = new AttributeConverterEntry(ADObjectFactory<T>.ADObjectPropertyMap.IsDeleted.PropertyName, ADObjectFactory<T>.ADObjectPropertyMap.IsDeleted.ADAttribute, TypeConstants.Bool, false, TypeAdapterAccess.Read, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), null, new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[9] = new AttributeConverterEntry(ADObjectFactory<T>.ADObjectPropertyMap.LastKnownParent.PropertyName, ADObjectFactory<T>.ADObjectPropertyMap.LastKnownParent.ADAttribute, TypeConstants.String, false, TypeAdapterAccess.Read, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), null, new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[10] = new AttributeConverterEntry(ADObjectFactory<T>.ADObjectPropertyMap.ModifiedTimeStamp.PropertyName, ADObjectFactory<T>.ADObjectPropertyMap.ModifiedTimeStamp.ADAttribute, TypeConstants.DateTime, false, TypeAdapterAccess.Read, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedDateTimeFromDateTime), null, new ToSearchFilterDelegate(SearchConverters.ToSearchDateTimeUsingSchemaInfo));
			attributeConverterEntry[11] = new AttributeConverterEntry(ADObjectFactory<T>.ADObjectPropertyMap.ObjectCategory.PropertyName, ADObjectFactory<T>.ADObjectPropertyMap.ObjectCategory.ADAttribute, TypeConstants.String, true, TypeAdapterAccess.Read, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), null, new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[12] = new AttributeConverterEntry(ADObjectFactory<T>.ADObjectPropertyMap.Name.PropertyName, ADObjectFactory<T>.ADObjectPropertyMap.Name.ADAttribute, TypeConstants.String, false, TypeAdapterAccess.Read, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), null, new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[13] = new AttributeConverterEntry(ADObjectFactory<T>.ADObjectPropertyMap.ProtectedFromDeletion.PropertyName, ADObjectFactory<T>.ADObjectPropertyMap.ProtectedFromDeletion.ADAttribute, TypeConstants.Bool, false, TypeAdapterAccess.ReadWrite, true, AttributeSet.Extended, new ToExtendedFormatDelegate(ADObjectFactory<T>.ToExtendedProtectedFromDeletion), null, new ToSearchFilterDelegate(SearchConverters.ToSearchNotSupported));
			ADObjectFactory<T>.ADMappingTable = attributeConverterEntry;
			ADObjectFactory<T>.ADAMMappingTable = ADObjectFactory<T>.ADMappingTable;
			ADFactoryBase<T>.RegisterMappingTable(ADObjectFactory<T>.ADAMMappingTable, ADServerType.ADLDS);
			ADFactoryBase<T>.RegisterMappingTable(ADObjectFactory<T>.ADMappingTable, ADServerType.ADDS);
		}

		public ADObjectFactory()
		{
			base.PostCommitPipeline.InsertAtEnd(new ADFactory<T>.FactoryCommitSubroutine(this.ADObjectPostCommitFSRoutine));
			base.PreCommitPipeline.InsertAtEnd(new ADFactory<T>.FactoryCommitSubroutine(this.ADObjectPreCommitFSRoutine));
		}

		private bool ADObjectPostCommitFSRoutine(ADFactory<T>.DirectoryOperation operation, T instance, ADParameterSet parameters, ADObject directoryObj)
		{
			bool hasValue;
			if (operation != ADFactory<T>.DirectoryOperation.Create || !base.PropertyHasChange(ADObjectFactory<T>.ADObjectPropertyMap.ProtectedFromDeletion.PropertyName, instance, parameters, operation))
			{
				if (operation != ADFactory<T>.DirectoryOperation.Create || !ProtectedFromDeletionUtil.ShouldProtectByDefault(directoryObj.ObjectClass))
				{
					return false;
				}
				else
				{
					return ProtectedFromDeletionUtil.ProtectFromAccidentalDeletion(directoryObj, base.CmdletSessionInfo);
				}
			}
			else
			{
				bool? singleValueProperty = base.GetSingleValueProperty<bool?>(ADObjectFactory<T>.ADObjectPropertyMap.ProtectedFromDeletion.PropertyName, instance, parameters, operation);
				if (singleValueProperty.HasValue)
				{
					bool? nullable = singleValueProperty;
					if (!nullable.GetValueOrDefault())
					{
						hasValue = false;
					}
					else
					{
						hasValue = nullable.HasValue;
					}
					if (!hasValue)
					{
						return ProtectedFromDeletionUtil.UnprotectFromAccidentalDeletion(directoryObj, base.CmdletSessionInfo);
					}
					else
					{
						return ProtectedFromDeletionUtil.ProtectFromAccidentalDeletion(directoryObj, base.CmdletSessionInfo);
					}
				}
				else
				{
					return false;
				}
			}
		}

		private bool ADObjectPreCommitFSRoutine(ADFactory<T>.DirectoryOperation operation, T instance, ADParameterSet parameters, ADObject directoryObj)
		{
			bool hasValue;
			if (operation != ADFactory<T>.DirectoryOperation.Update || !base.PropertyHasChange(ADObjectFactory<T>.ADObjectPropertyMap.ProtectedFromDeletion.PropertyName, instance, parameters, operation))
			{
				return false;
			}
			else
			{
				bool? singleValueProperty = base.GetSingleValueProperty<bool?>(ADObjectFactory<T>.ADObjectPropertyMap.ProtectedFromDeletion.PropertyName, instance, parameters, operation);
				if (singleValueProperty.HasValue)
				{
					bool? nullable = singleValueProperty;
					if (!nullable.GetValueOrDefault())
					{
						hasValue = false;
					}
					else
					{
						hasValue = nullable.HasValue;
					}
					if (!hasValue)
					{
						return ProtectedFromDeletionUtil.UnprotectFromAccidentalDeletion(directoryObj, base.CmdletSessionInfo);
					}
					else
					{
						return ProtectedFromDeletionUtil.ProtectFromAccidentalDeletion(directoryObj, base.CmdletSessionInfo);
					}
				}
				else
				{
					return false;
				}
			}
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
				return aDOPathNodes;
			}
			else
			{
				throw new ArgumentNullException("identity");
			}
		}

		internal override IADOPathNode IdentitySearchConverter(object identity)
		{
			if (identity != null)
			{
				if ((int)this.IdentityLdapAttributes.Length <= 0)
				{
					return null;
				}
				else
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
									if (aDOPathNodes.Count != 1)
									{
										return ADOPathUtil.CreateAndClause(aDOPathNodes.ToArray());
									}
									else
									{
										return aDOPathNodes[0];
									}
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
						string str = identity as string;
						Guid? nullable1 = null;
						if (!Utils.TryParseGuid(str, out nullable1))
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
							Guid value1 = nullable1.Value;
							return ADOPathUtil.CreateFilterClause(ADOperator.Eq, "objectGuid", value1.ToByteArray());
						}
					}
				}
			}
			else
			{
				throw new ArgumentNullException("identity");
			}
		}

		internal static void ToExtendedProtectedFromDeletion(string extendedAttribute, string[] directoryAttributes, ADEntity userObj, ADEntity directoryObj, CmdletSessionInfo cmdletSessionInfo)
		{
			ADObject aDObject = directoryObj as ADObject;
			if (aDObject == null)
			{
				object[] type = new object[2];
				type[0] = "ToExtendedProtectedFromDeletion";
				type[1] = directoryObj.GetType();
				throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, StringResources.MethodNotSupportedForObjectType, type));
			}
			else
			{
				userObj.Add(extendedAttribute, ProtectedFromDeletionUtil.IsProtectedFromDeletion(aDObject, cmdletSessionInfo));
				return;
			}
		}

		private static class ADObjectPropertyMap
		{
			public readonly static PropertyMapEntry DistinguishedName;

			public readonly static PropertyMapEntry ObjectClass;

			public readonly static PropertyMapEntry CanonicalName;

			public readonly static PropertyMapEntry CN;

			public readonly static PropertyMapEntry CreationTimeStamp;

			public readonly static PropertyMapEntry Description;

			public readonly static PropertyMapEntry DisplayName;

			public readonly static PropertyMapEntry IsDeleted;

			public readonly static PropertyMapEntry LastKnownParent;

			public readonly static PropertyMapEntry ModifiedTimeStamp;

			public readonly static PropertyMapEntry ObjectCategory;

			public readonly static PropertyMapEntry Name;

			public readonly static PropertyMapEntry GUID;

			public readonly static PropertyMapEntry ProtectedFromDeletion;

			static ADObjectPropertyMap()
			{
				ADObjectFactory<T>.ADObjectPropertyMap.DistinguishedName = new PropertyMapEntry("DistinguishedName", "distinguishedName", "distinguishedName");
				ADObjectFactory<T>.ADObjectPropertyMap.ObjectClass = new PropertyMapEntry("ObjectClass", "objectClass", "objectClass");
				ADObjectFactory<T>.ADObjectPropertyMap.CanonicalName = new PropertyMapEntry("CanonicalName", "canonicalName", "canonicalName");
				ADObjectFactory<T>.ADObjectPropertyMap.CN = new PropertyMapEntry("CN", "cn", "cn");
				ADObjectFactory<T>.ADObjectPropertyMap.CreationTimeStamp = new PropertyMapEntry("Created", "createTimeStamp", "createTimeStamp");
				ADObjectFactory<T>.ADObjectPropertyMap.Description = new PropertyMapEntry("Description", "description", "description");
				ADObjectFactory<T>.ADObjectPropertyMap.DisplayName = new PropertyMapEntry("DisplayName", "displayName", "displayName");
				ADObjectFactory<T>.ADObjectPropertyMap.IsDeleted = new PropertyMapEntry("Deleted", "isDeleted", "isDeleted");
				ADObjectFactory<T>.ADObjectPropertyMap.LastKnownParent = new PropertyMapEntry("LastKnownParent", "lastKnownParent", "lastKnownParent");
				ADObjectFactory<T>.ADObjectPropertyMap.ModifiedTimeStamp = new PropertyMapEntry("Modified", "modifyTimeStamp", "modifyTimeStamp");
				ADObjectFactory<T>.ADObjectPropertyMap.ObjectCategory = new PropertyMapEntry("ObjectCategory", "objectCategory", "objectCategory");
				ADObjectFactory<T>.ADObjectPropertyMap.Name = new PropertyMapEntry("Name", "name", "name");
				ADObjectFactory<T>.ADObjectPropertyMap.GUID = new PropertyMapEntry("ObjectGUID", "objectGUID", "objectGUID");
				string[] strArrays = new string[4];
				strArrays[0] = "nTSecurityDescriptor";
				strArrays[1] = "sdRightsEffective";
				strArrays[2] = "instanceType";
				strArrays[3] = "isDeleted";
				string[] strArrays1 = new string[4];
				strArrays1[0] = "nTSecurityDescriptor";
				strArrays1[1] = "sdRightsEffective";
				strArrays1[2] = "instanceType";
				strArrays1[3] = "isDeleted";
				ADObjectFactory<T>.ADObjectPropertyMap.ProtectedFromDeletion = new PropertyMapEntry("ProtectedFromAccidentalDeletion", strArrays, strArrays1);
			}
		}
	}
}