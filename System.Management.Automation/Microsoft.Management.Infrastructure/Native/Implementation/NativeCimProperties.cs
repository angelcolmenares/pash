using System;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.Management.Infrastructure.Native
{
	[Serializable]
	public class NativeCimProperties : List<NativeCimProperty>
	{
		public void Add(string name, string origin, bool isArray, bool isLocal, CimType type, object value)
		{
			Add(new NativeCimProperty { Name = name, Origin = origin, IsArray = isArray, Type = type, Value = value, IsLocal = isLocal });
		}
	}

	[Serializable]
	public class NativeCimProperty
	{
		public bool IsArray
		{
			get;set;
		}

		public bool IsLocal
		{
			get;set;
		}

		public string Origin
		{
			get;set;
		}

		public string Name
		{
			get;set;
		}

		public object Value
		{
			get;set;
		}

		public CimType Type
		{
			get;set;
		}
	}

	internal static class NativeCimPropertiesHelper
	{
		//private static readonly XmlSerializer serializer = CreateSerializer();

		static XmlSerializer CreateSerializer ()
		{
			var obj = new XmlSerializer(typeof(NativeCimProperties), new Type[] { typeof(List<string>), typeof(string[]) });

			FieldInfo backgroundGeneration = typeof(XmlSerializer).GetField ("backgroundGeneration", BindingFlags.NonPublic | BindingFlags.Static);
			backgroundGeneration.SetValue (null, false);

			return obj;
		}

		public static string Serialize (NativeCimProperties obj)
		{
			XmlSerializer serializer = CreateSerializer();
			StringBuilder sb = new StringBuilder();
			StringWriter writer = new StringWriter(sb);
			serializer.Serialize (writer, obj);
			return sb.ToString ();
		}

		public static NativeCimProperties Deserialize(string target)
		{
			XmlSerializer serializer = CreateSerializer();
			StringReader reader = new StringReader(target);
			NativeCimProperties obj = (NativeCimProperties)serializer.Deserialize(reader);
			return obj;
		}
	}
}

