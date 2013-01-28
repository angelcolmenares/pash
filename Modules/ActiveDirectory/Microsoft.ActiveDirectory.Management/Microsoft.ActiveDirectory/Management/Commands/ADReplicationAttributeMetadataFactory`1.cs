using Microsoft.ActiveDirectory.Management;
using System;
using System.Collections.Generic;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public class ADReplicationAttributeMetadataFactory<T> : ADXmlAttributeFactory<T>
	where T : ADReplicationAttributeMetadata, new()
	{
		private static AttributeConverterEntry[] ADMappingTable;

		private static AttributeConverterEntry[] ADAMMappingTable;

		internal static IDictionary<ADServerType, MappingTable<AttributeConverterEntry>> AttributeTable
		{
			get
			{
				return ADFactoryBase<T>.AttributeTable;
			}
		}

		static ADReplicationAttributeMetadataFactory()
		{
			AttributeConverterEntry[] attributeConverterEntry = new AttributeConverterEntry[13];
			attributeConverterEntry[0] = new AttributeConverterEntry(ADReplicationAttributeMetadataFactory<T>.ADReplicationAttributeMetadataPropertyMap.Server.PropertyName, ADReplicationAttributeMetadataFactory<T>.ADReplicationAttributeMetadataPropertyMap.Server.ADAttribute, TypeConstants.String, false, TypeAdapterAccess.Read, true, AttributeSet.Default, new ToExtendedFormatDelegate(ADReplicationUtil.ToExtendedServerFromSessionInfo), null, new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			attributeConverterEntry[1] = new AttributeConverterEntry(ADReplicationAttributeMetadataFactory<T>.ADReplicationAttributeMetadataPropertyMap.Object.PropertyName, ADReplicationAttributeMetadataFactory<T>.ADReplicationAttributeMetadataPropertyMap.Object.ADAttribute, TypeConstants.String, false, TypeAdapterAccess.Read, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), null, new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			attributeConverterEntry[2] = new AttributeConverterEntry(ADReplicationAttributeMetadataFactory<T>.ADReplicationAttributeMetadataPropertyMap.AttributeName.PropertyName, ADReplicationAttributeMetadataFactory<T>.ADReplicationAttributeMetadataPropertyMap.AttributeName.ADAttribute, TypeConstants.String, false, TypeAdapterAccess.Read, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), null, new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			attributeConverterEntry[3] = new AttributeConverterEntry(ADReplicationAttributeMetadataFactory<T>.ADReplicationAttributeMetadataPropertyMap.AttributeValue.PropertyName, ADReplicationAttributeMetadataFactory<T>.ADReplicationAttributeMetadataPropertyMap.AttributeValue.ADAttribute, TypeConstants.Object, false, TypeAdapterAccess.Read, true, AttributeSet.Default, new ToExtendedFormatDelegate(ADReplicationUtil.ToExtendedAttributeMetadataValue), null, new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			attributeConverterEntry[4] = new AttributeConverterEntry(ADReplicationAttributeMetadataFactory<T>.ADReplicationAttributeMetadataPropertyMap.Version.PropertyName, ADReplicationAttributeMetadataFactory<T>.ADReplicationAttributeMetadataPropertyMap.Version.ADAttribute, TypeConstants.Int, false, TypeAdapterAccess.Read, true, AttributeSet.Default, AttributeConverters.GetToExtendedFromStringConverter(new AttributeConverters.StringParserDelegate(AttributeConverters.ParseIntFromString)), null, new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			attributeConverterEntry[5] = new AttributeConverterEntry(ADReplicationAttributeMetadataFactory<T>.ADReplicationAttributeMetadataPropertyMap.LastOriginatingChangeUsn.PropertyName, ADReplicationAttributeMetadataFactory<T>.ADReplicationAttributeMetadataPropertyMap.LastOriginatingChangeUsn.ADAttribute, TypeConstants.Long, false, TypeAdapterAccess.Read, true, AttributeSet.Default, AttributeConverters.GetToExtendedFromStringConverter(new AttributeConverters.StringParserDelegate(AttributeConverters.ParseLongFromString)), null, new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			attributeConverterEntry[6] = new AttributeConverterEntry(ADReplicationAttributeMetadataFactory<T>.ADReplicationAttributeMetadataPropertyMap.LastOriginatingChangeTime.PropertyName, ADReplicationAttributeMetadataFactory<T>.ADReplicationAttributeMetadataPropertyMap.LastOriginatingChangeTime.ADAttribute, TypeConstants.DateTime, false, TypeAdapterAccess.Read, true, AttributeSet.Default, AttributeConverters.GetToExtendedFromStringConverter(new AttributeConverters.StringParserDelegate(AttributeConverters.ParseDateTimeFromString)), null, new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			attributeConverterEntry[7] = new AttributeConverterEntry(ADReplicationAttributeMetadataFactory<T>.ADReplicationAttributeMetadataPropertyMap.LastOriginatingChangeDirectoryServerInvocationId.PropertyName, ADReplicationAttributeMetadataFactory<T>.ADReplicationAttributeMetadataPropertyMap.LastOriginatingChangeDirectoryServerInvocationId.ADAttribute, TypeConstants.Guid, false, TypeAdapterAccess.Read, true, AttributeSet.Default, AttributeConverters.GetToExtendedFromStringConverter(new AttributeConverters.StringParserDelegate(AttributeConverters.ParseGuidFromString)), null, new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			attributeConverterEntry[8] = new AttributeConverterEntry(ADReplicationAttributeMetadataFactory<T>.ADReplicationAttributeMetadataPropertyMap.LastOriginatingChangeDirectoryServerIdentity.PropertyName, ADReplicationAttributeMetadataFactory<T>.ADReplicationAttributeMetadataPropertyMap.LastOriginatingChangeDirectoryServerIdentity.ADAttribute, TypeConstants.String, false, TypeAdapterAccess.Read, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), null, new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			attributeConverterEntry[9] = new AttributeConverterEntry(ADReplicationAttributeMetadataFactory<T>.ADReplicationAttributeMetadataPropertyMap.LocalChangeUsn.PropertyName, ADReplicationAttributeMetadataFactory<T>.ADReplicationAttributeMetadataPropertyMap.LocalChangeUsn.ADAttribute, TypeConstants.Long, false, TypeAdapterAccess.Read, true, AttributeSet.Default, AttributeConverters.GetToExtendedFromStringConverter(new AttributeConverters.StringParserDelegate(AttributeConverters.ParseLongFromString)), null, new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			attributeConverterEntry[10] = new AttributeConverterEntry(ADReplicationAttributeMetadataFactory<T>.ADReplicationAttributeMetadataPropertyMap.LastOriginatingDeleteTime.PropertyName, ADReplicationAttributeMetadataFactory<T>.ADReplicationAttributeMetadataPropertyMap.LastOriginatingDeleteTime.ADAttribute, TypeConstants.DateTime, false, TypeAdapterAccess.Read, true, AttributeSet.Default, AttributeConverters.GetToExtendedFromStringConverter(new AttributeConverters.StringParserDelegate(AttributeConverters.ParseDateTimeFromString)), null, new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			attributeConverterEntry[11] = new AttributeConverterEntry(ADReplicationAttributeMetadataFactory<T>.ADReplicationAttributeMetadataPropertyMap.FirstOriginatingCreateTime.PropertyName, ADReplicationAttributeMetadataFactory<T>.ADReplicationAttributeMetadataPropertyMap.FirstOriginatingCreateTime.ADAttribute, TypeConstants.DateTime, false, TypeAdapterAccess.Read, true, AttributeSet.Default, AttributeConverters.GetToExtendedFromStringConverter(new AttributeConverters.StringParserDelegate(AttributeConverters.ParseDateTimeFromString)), null, new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			attributeConverterEntry[12] = new AttributeConverterEntry(ADReplicationAttributeMetadataFactory<T>.ADReplicationAttributeMetadataPropertyMap.IsLinkValue.PropertyName, ADReplicationAttributeMetadataFactory<T>.ADReplicationAttributeMetadataPropertyMap.IsLinkValue.ADAttribute, TypeConstants.Bool, false, TypeAdapterAccess.Read, true, AttributeSet.Default, new ToExtendedFormatDelegate(ADReplicationUtil.ToExtendedIsLinkValue), null, new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			ADReplicationAttributeMetadataFactory<T>.ADMappingTable = attributeConverterEntry;
			ADReplicationAttributeMetadataFactory<T>.ADAMMappingTable = ADReplicationAttributeMetadataFactory<T>.ADMappingTable;
			ADFactoryBase<T>.RegisterMappingTable(ADReplicationAttributeMetadataFactory<T>.ADAMMappingTable, ADServerType.ADLDS);
			ADFactoryBase<T>.RegisterMappingTable(ADReplicationAttributeMetadataFactory<T>.ADMappingTable, ADServerType.ADDS);
		}

		public ADReplicationAttributeMetadataFactory()
		{
		}

		internal override IEnumerable<T> ApplyClientSideFilter(IEnumerable<T> objectList)
		{
			return base.ApplyClientSideFilter(this.ApplyPropertiesFilter(this.ApplyHighestUsnFilter(objectList)));
		}

		private IEnumerable<T> ApplyHighestUsnFilter(IEnumerable<T> objectList)
		{
			if (!base.CmdletSessionInfo.CmdletParameters.GetSwitchParameterBooleanValue("ShowAllLinkedValues"))
			{
				List<T> ts = new List<T>();
				Dictionary<string, T> strs = new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);
				foreach (T t in objectList)
				{
					if (t.IsLinkValue)
					{
						if (!strs.ContainsKey(t.AttributeName))
						{
							strs.Add(t.AttributeName, t);
						}
						else
						{
							if (this.CompareLinkValueStamp(t, strs[t.AttributeName]) <= 0)
							{
								continue;
							}
							strs[t.AttributeName] = t;
						}
					}
					else
					{
						ts.Add(t);
					}
				}
				foreach (T value in strs.Values)
				{
					ts.Add(value);
				}
				return ts;
			}
			else
			{
				return objectList;
			}
		}

		private IEnumerable<T> ApplyPropertiesFilter(IEnumerable<T> objectList)
		{
			if (base.CmdletSessionInfo.CmdletParameters.Contains("Properties"))
			{
				HashSet<string> strs = new HashSet<string>(base.CmdletSessionInfo.CmdletParameters["Properties"] as string[], StringComparer.OrdinalIgnoreCase);
				if (!strs.Contains("*"))
				{
					List<T> ts = new List<T>();
					foreach (T t in objectList)
					{
						if (!strs.Contains(t.AttributeName))
						{
							continue;
						}
						ts.Add(t);
					}
					return ts;
				}
				else
				{
					return objectList;
				}
			}
			else
			{
				return objectList;
			}
		}

		private int CompareLinkValueStamp(T metadata1, T metadata2)
		{
			if (metadata1.Version == 0 || metadata2.Version != 0)
			{
				if (metadata1.Version != 0 || metadata2.Version == 0)
				{
					if (metadata1.FirstOriginatingCreateTime <= metadata2.FirstOriginatingCreateTime)
					{
						if (metadata1.FirstOriginatingCreateTime >= metadata2.FirstOriginatingCreateTime)
						{
							if (metadata1.Version - metadata2.Version == 0)
							{
								if (metadata1.LastOriginatingChangeTime <= metadata2.LastOriginatingChangeTime)
								{
									if (metadata1.LastOriginatingChangeTime >= metadata2.LastOriginatingChangeTime)
									{
										Guid? lastOriginatingChangeDirectoryServerInvocationId = metadata1.LastOriginatingChangeDirectoryServerInvocationId;
										if (lastOriginatingChangeDirectoryServerInvocationId.HasValue)
										{
											Guid? nullable = metadata2.LastOriginatingChangeDirectoryServerInvocationId;
											if (nullable.HasValue)
											{
												Guid? lastOriginatingChangeDirectoryServerInvocationId1 = metadata1.LastOriginatingChangeDirectoryServerInvocationId;
												Guid value = lastOriginatingChangeDirectoryServerInvocationId1.Value;
												Guid? nullable1 = metadata2.LastOriginatingChangeDirectoryServerInvocationId;
												return value.CompareTo(nullable1.Value);
											}
										}
										Guid? lastOriginatingChangeDirectoryServerInvocationId2 = metadata1.LastOriginatingChangeDirectoryServerInvocationId;
										if (!lastOriginatingChangeDirectoryServerInvocationId2.HasValue)
										{
											Guid? nullable2 = metadata2.LastOriginatingChangeDirectoryServerInvocationId;
											if (!nullable2.HasValue)
											{
												return 0;
											}
											else
											{
												return -1;
											}
										}
										else
										{
											return 1;
										}
									}
									else
									{
										return -1;
									}
								}
								else
								{
									return 1;
								}
							}
							else
							{
								return metadata1.Version - metadata2.Version;
							}
						}
						else
						{
							return -1;
						}
					}
					else
					{
						return 1;
					}
				}
				else
				{
					return -1;
				}
			}
			else
			{
				return 1;
			}
		}

		private static class ADReplicationAttributeMetadataPropertyMap
		{
			public readonly static PropertyMapEntry Server;

			public readonly static PropertyMapEntry Object;

			public readonly static PropertyMapEntry AttributeName;

			public readonly static PropertyMapEntry AttributeValue;

			public readonly static PropertyMapEntry Version;

			public readonly static PropertyMapEntry LastOriginatingChangeUsn;

			public readonly static PropertyMapEntry LastOriginatingChangeTime;

			public readonly static PropertyMapEntry LastOriginatingChangeDirectoryServerInvocationId;

			public readonly static PropertyMapEntry LastOriginatingChangeDirectoryServerIdentity;

			public readonly static PropertyMapEntry LocalChangeUsn;

			public readonly static PropertyMapEntry LastOriginatingDeleteTime;

			public readonly static PropertyMapEntry FirstOriginatingCreateTime;

			public readonly static PropertyMapEntry IsLinkValue;

			static ADReplicationAttributeMetadataPropertyMap()
			{
				ADReplicationAttributeMetadataFactory<T>.ADReplicationAttributeMetadataPropertyMap.Server = new PropertyMapEntry("Server", string.Empty, string.Empty);
				ADReplicationAttributeMetadataFactory<T>.ADReplicationAttributeMetadataPropertyMap.Object = new PropertyMapEntry("Object", "distinguishedName", "distinguishedName");
				ADReplicationAttributeMetadataFactory<T>.ADReplicationAttributeMetadataPropertyMap.AttributeName = new PropertyMapEntry("AttributeName", "pszAttributeName", "pszAttributeName");
				ADReplicationAttributeMetadataFactory<T>.ADReplicationAttributeMetadataPropertyMap.AttributeValue = new PropertyMapEntry("AttributeValue", "pszObjectDn", "pszObjectDn");
				ADReplicationAttributeMetadataFactory<T>.ADReplicationAttributeMetadataPropertyMap.Version = new PropertyMapEntry("Version", "dwVersion", "dwVersion");
				ADReplicationAttributeMetadataFactory<T>.ADReplicationAttributeMetadataPropertyMap.LastOriginatingChangeUsn = new PropertyMapEntry("LastOriginatingChangeUsn", "usnOriginatingChange", "usnOriginatingChange");
				ADReplicationAttributeMetadataFactory<T>.ADReplicationAttributeMetadataPropertyMap.LastOriginatingChangeTime = new PropertyMapEntry("LastOriginatingChangeTime", "ftimeLastOriginatingChange", "ftimeLastOriginatingChange");
				ADReplicationAttributeMetadataFactory<T>.ADReplicationAttributeMetadataPropertyMap.LastOriginatingChangeDirectoryServerInvocationId = new PropertyMapEntry("LastOriginatingChangeDirectoryServerInvocationId", "uuidLastOriginatingDsaInvocationID", "uuidLastOriginatingDsaInvocationID");
				ADReplicationAttributeMetadataFactory<T>.ADReplicationAttributeMetadataPropertyMap.LastOriginatingChangeDirectoryServerIdentity = new PropertyMapEntry("LastOriginatingChangeDirectoryServerIdentity", "pszLastOriginatingDsaDN", "pszLastOriginatingDsaDN");
				ADReplicationAttributeMetadataFactory<T>.ADReplicationAttributeMetadataPropertyMap.LocalChangeUsn = new PropertyMapEntry("LocalChangeUsn", "usnLocalChange", "usnLocalChange");
				ADReplicationAttributeMetadataFactory<T>.ADReplicationAttributeMetadataPropertyMap.LastOriginatingDeleteTime = new PropertyMapEntry("LastOriginatingDeleteTime", "ftimeDeleted", "ftimeDeleted");
				ADReplicationAttributeMetadataFactory<T>.ADReplicationAttributeMetadataPropertyMap.FirstOriginatingCreateTime = new PropertyMapEntry("FirstOriginatingCreateTime", "ftimeCreated", "ftimeCreated");
				ADReplicationAttributeMetadataFactory<T>.ADReplicationAttributeMetadataPropertyMap.IsLinkValue = new PropertyMapEntry("IsLinkValue", "sourceXmlAttribute", "sourceXmlAttribute");
			}
		}
	}
}