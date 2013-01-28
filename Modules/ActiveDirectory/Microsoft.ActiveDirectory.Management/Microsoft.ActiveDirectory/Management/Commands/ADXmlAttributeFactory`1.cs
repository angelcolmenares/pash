using Microsoft.ActiveDirectory;
using Microsoft.ActiveDirectory.Management;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public abstract class ADXmlAttributeFactory<T> : ADFactoryBase<T>
	where T : ADEntity, new()
	{
		private const string _debugCategory = "ADXmlAttributeFactory";

		internal ADXmlAttributeFactory()
		{
		}

		internal virtual T Construct(ADEntity directoryObj)
		{
			if (base.CmdletSessionInfo != null)
			{
				T sessionInfo = Activator.CreateInstance<T>();
				sessionInfo.IsSearchResult = true;
				sessionInfo.SessionInfo = directoryObj.SessionInfo;
				MappingTable<AttributeConverterEntry> item = ADFactoryBase<T>.AttributeTable[base.ConnectedStore];
				foreach (AttributeConverterEntry value in item.Values)
				{
					if (!value.IsExtendedConverterDefined)
					{
						continue;
					}
					value.InvokeToExtendedConverter(sessionInfo, directoryObj, base.CmdletSessionInfo);
				}
				sessionInfo.TrackChanges = true;
				return sessionInfo;
			}
			else
			{
				throw new ArgumentNullException(StringResources.SessionRequired);
			}
		}

		internal virtual IEnumerable<ADEntity> GetADEntityFromXmlAttribute(ADEntity sourceEntity, string xmlAttributeName, string xmlAttributeObjectTypeName)
		{
			IEnumerable<ADEntity> aDEntities;
			List<ADEntity> aDEntities1 = new List<ADEntity>();
			if (sourceEntity[xmlAttributeName] != null)
			{
				IEnumerator enumerator = sourceEntity[xmlAttributeName].GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						object current = enumerator.Current;
						XmlDocument xmlDocument = new XmlDocument();
						ADEntity aDEntity = new ADEntity();
						string str = current as string;
						aDEntity.SessionInfo = sourceEntity.SessionInfo;
						if (str == null)
						{
							continue;
						}
						try
						{
							xmlDocument.LoadXml(this.RemoveInvalidXmlChars(str));
						}
						catch (XmlException xmlException)
						{
							aDEntities = aDEntities1;
							return aDEntities;
						}
						if (xmlDocument.ChildNodes.Count == 1)
						{
							if (xmlDocument.FirstChild.Name.Equals(xmlAttributeObjectTypeName, StringComparison.OrdinalIgnoreCase))
							{
								IEnumerator enumerator1 = sourceEntity.PropertyNames.GetEnumerator();
								try
								{
									while (enumerator1.MoveNext())
									{
										string current1 = (string)enumerator1.Current;
										aDEntity.SetValue(current1, sourceEntity[current1]);
									}
								}
								finally
								{
									IDisposable disposable = enumerator1 as IDisposable;
									if (disposable != null)
									{
										disposable.Dispose();
									}
								}
								if (!aDEntity.Contains("sourceXmlAttribute"))
								{
									aDEntity.SetValue("sourceXmlAttribute", xmlAttributeName);
								}
								IEnumerator enumerator2 = xmlDocument.FirstChild.ChildNodes.GetEnumerator();
								try
								{
									while (enumerator2.MoveNext())
									{
										XmlNode xmlNodes = (XmlNode)enumerator2.Current;
										if (string.IsNullOrEmpty(xmlNodes.InnerText))
										{
											continue;
										}
										aDEntity.SetValue(xmlNodes.Name, xmlNodes.InnerText);
									}
								}
								finally
								{
									IDisposable disposable1 = enumerator2 as IDisposable;
									if (disposable1 != null)
									{
										disposable1.Dispose();
									}
								}
								aDEntities1.Add(aDEntity);
							}
							else
							{
								aDEntities = aDEntities1;
								return aDEntities;
							}
						}
						else
						{
							aDEntities = aDEntities1;
							return aDEntities;
						}
					}
					return aDEntities1;
				}
				finally
				{
					IDisposable disposable2 = enumerator as IDisposable;
					if (disposable2 != null)
					{
						disposable2.Dispose();
					}
				}
				return aDEntities;
			}
			else
			{
				return aDEntities1;
			}
		}

		internal virtual IEnumerable<T> GetExtendedObjectFromDirectoryObject(ADEntity directoryObject, string xmlAttributeName, string xmlAttributeObjectTypeName)
		{
			List<T> ts = new List<T>();
			foreach (ADEntity aDEntityFromXmlAttribute in this.GetADEntityFromXmlAttribute(directoryObject, xmlAttributeName, xmlAttributeObjectTypeName))
			{
				ts.Add(this.Construct(aDEntityFromXmlAttribute));
			}
			return this.ApplyClientSideFilter(ts);
		}

		private string RemoveInvalidXmlChars(string xml)
		{
			return xml.Replace("\0", "");
		}
	}
}