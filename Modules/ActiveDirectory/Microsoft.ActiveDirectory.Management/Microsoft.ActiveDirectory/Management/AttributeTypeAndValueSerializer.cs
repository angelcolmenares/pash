using System;
using System.Collections;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Xml;

namespace Microsoft.ActiveDirectory.Management
{
	internal sealed class AttributeTypeAndValueSerializer
	{
		private AttributeTypeAndValueSerializer()
		{
		}

		private static string FormatAttributeName(string prefix, string attribute)
		{
			return XmlUtility.AddPrefix(prefix, attribute);
		}

		private static void InternalSerialize(XmlDictionaryWriter writer, ChangeOperation ChangeOperation, string ns, string property, object value)
		{
			string str = writer.LookupPrefix(ns);
			writer.LookupPrefix("http://schemas.microsoft.com/2008/1/ActiveDirectory");
			if (ChangeOperation != ChangeOperation.None)
			{
				writer.WriteStartElement("Change", "http://schemas.microsoft.com/2006/11/IdentityManagement/DirectoryAccess");
				ChangeOperation changeOperation = ChangeOperation;
				switch (changeOperation)
				{
					case ChangeOperation.Add:
					{
						writer.WriteAttributeString("Operation", "add");
						break;
					}
					case ChangeOperation.Delete:
					{
						writer.WriteAttributeString("Operation", "delete");
						break;
					}
					case ChangeOperation.Replace:
					{
						writer.WriteAttributeString("Operation", "replace");
						break;
					}
				}
			}
			else
			{
				writer.WriteStartElement("AttributeTypeAndValue", "http://schemas.microsoft.com/2006/11/IdentityManagement/DirectoryAccess");
			}
			writer.WriteElementString("AttributeType", "http://schemas.microsoft.com/2006/11/IdentityManagement/DirectoryAccess", AttributeTypeAndValueSerializer.FormatAttributeName(str, property));
			if (value != null)
			{
				if (value as ICollection == null)
				{
					writer.WriteStartElement("AttributeValue", "http://schemas.microsoft.com/2006/11/IdentityManagement/DirectoryAccess");
					if (value as DirectoryAttributeModification == null)
					{
						ADValueSerializer.Serialize(writer, value);
					}
					else
					{
						DirectoryAttributeModification directoryAttributeModification = (DirectoryAttributeModification)value;
						ADValueSerializer.Serialize(writer, directoryAttributeModification[0]);
					}
					writer.WriteEndElement();
				}
				else
				{
					ICollection collections = (ICollection)value;
					if (collections.Count > 0)
					{
						writer.WriteStartElement("AttributeValue", "http://schemas.microsoft.com/2006/11/IdentityManagement/DirectoryAccess");
						foreach (object obj in collections)
						{
							ADValueSerializer.Serialize(writer, obj);
						}
						writer.WriteEndElement();
					}
				}
			}
			writer.WriteEndElement();
		}

		public static void Serialize(XmlDictionaryWriter writer, DirectoryAttribute attribute)
		{
			AttributeTypeAndValueSerializer.InternalSerialize(writer, ChangeOperation.None, AttributeNs.LookupNs(attribute.Name, SyntheticAttributeOperation.Write), attribute.Name, attribute);
		}

		public static void Serialize(XmlDictionaryWriter writer, ChangeOperation ChangeOperation, DirectoryAttributeModification attribute)
		{
			AttributeTypeAndValueSerializer.InternalSerialize(writer, ChangeOperation, AttributeNs.LookupNs(attribute.Name, SyntheticAttributeOperation.Write), attribute.Name, attribute);
		}

		public static void Serialize(XmlDictionaryWriter writer, DirectoryAttributeModification attribute)
		{
			ChangeOperation changeOperation = ChangeOperation.None;
			DirectoryAttributeOperation operation = attribute.Operation;
			switch (operation)
			{
				case DirectoryAttributeOperation.Add:
				{
					changeOperation = ChangeOperation.Add;
					break;
				}
				case DirectoryAttributeOperation.Delete:
				{
					changeOperation = ChangeOperation.Delete;
					break;
				}
				case DirectoryAttributeOperation.Replace:
				{
					changeOperation = ChangeOperation.Replace;
					break;
				}
			}
			AttributeTypeAndValueSerializer.InternalSerialize(writer, changeOperation, AttributeNs.LookupNs(attribute.Name, SyntheticAttributeOperation.Write), attribute.Name, attribute);
		}

		public static void Serialize(XmlDictionaryWriter writer, string ns, string property, object value)
		{
			AttributeTypeAndValueSerializer.InternalSerialize(writer, ChangeOperation.None, ns, property, value);
		}

		public static void Serialize(XmlDictionaryWriter writer, ChangeOperation ChangeOperation, string ns, string property, object value)
		{
			AttributeTypeAndValueSerializer.InternalSerialize(writer, ChangeOperation, ns, property, value);
		}

		public static void Serialize(XmlDictionaryWriter writer, ChangeOperation ChangeOperation, IList<DirectoryAttributeModification> attributes)
		{
			foreach (DirectoryAttributeModification attribute in attributes)
			{
				AttributeTypeAndValueSerializer.InternalSerialize(writer, ChangeOperation, AttributeNs.LookupNs(attribute.Name, SyntheticAttributeOperation.Write), attribute.Name, attribute);
			}
		}
	}
}