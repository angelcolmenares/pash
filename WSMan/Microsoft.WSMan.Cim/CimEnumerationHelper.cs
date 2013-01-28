using System;
using Microsoft.Management.Infrastructure.Native;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Xml;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.IO;

namespace Microsoft.WSMan.Cim
{
	internal static class CimEnumerationHelper
	{
		internal static NativeCimClass CreateClass (EndpointAddress address)
		{
			NativeCimClass item = new NativeCimClass();
			item.ClassName = GetHeaderValue<string>(address, "ClassName");
			item.Namespace = GetHeaderValue<string>(address, "NamespacePath");
			item.ServerName = GetHeaderValue<string>(address, "ServerName");
			item.Properties = GetHeaderValue<string>(address, "Properties");
			item.SystemProperties = GetHeaderValue<string>(address, "SystemProperties");
			item.Qualifiers = GetHeaderValue<string>(address, "Qualifiers");
			item.Methods = GetHeaderValue<string>(address, "Methods");
			return item;
		}
		
		internal static NativeCimInstance CreateInstance (EndpointAddress address)
		{
			NativeCimInstance item = new NativeCimInstance();
			item.ClassName = GetHeaderValue<string>(address, "ClassName");
			item.CimClassName = GetHeaderValue<string>(address, "CimClassName");
			item.Namespace = GetHeaderValue<string>(address, "NamespacePath");
			item.ServerName = GetHeaderValue<string>(address, "ServerName");
			item.Properties = GetHeaderValue<string>(address, "Properties");
			item.SystemProperties = GetHeaderValue<string>(address, "SystemProperties");
			item.Qualifiers = GetHeaderValue<string>(address, "Qualifiers");
			return item;
		}

		private static T GetHeaderValue<T> (EndpointAddress address, string name)
		{
			var header = address.Headers.FindHeader (name, CimNamespaces.CimNamespace);

			try {
				if (header != null) {
					//Serialize object to xml
					StringBuilder sb = new StringBuilder();
					var writer = XmlDictionaryWriter.CreateDictionaryWriter (XmlWriter.Create (sb));
					header.WriteAddressHeaderContents (writer);
					string text = sb.ToString ();
					StringReader reader = new StringReader(text);
					var xmlReader = XmlReader.Create (reader);
					xmlReader.MoveToElement ();
					object result = xmlReader.ReadElementContentAs (typeof(T), new XmlNamespaceManager(xmlReader.NameTable));

					return (T)result;

					/*
					var reader = header.GetAddressHeaderReader ();
					var resolver = new XmlNamespaceManager(reader.NameTable);
					while(reader.Read ())
					{
						if (reader.NodeType == XmlNodeType.Text)
						{
							string value = reader.Value;
						}
					}
					*/
				}
			}
			catch(Exception ex)
			{
				var msg = ex.Message;
			}
			return default(T);
		}
	}
}

