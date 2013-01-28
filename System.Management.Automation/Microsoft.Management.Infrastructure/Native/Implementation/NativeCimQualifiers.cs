using System;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace Microsoft.Management.Infrastructure.Native
{
	[Serializable]
	public class NativeCimQualifiers : List<NativeCimQualifier>
	{
		public void Add(string name, bool isAmmended, bool isLocal, bool isOverridable, bool propagateToInstance, bool propagateToSuperClass, object value)
		{
			Add(new NativeCimQualifier { Name = name, IsAmmended = isAmmended, IsOverridable = isOverridable, PropagatesToInstance = propagateToInstance, PropagatesToSuperClass = propagateToSuperClass, Value = value, IsLocal = isLocal });
		}
	}
	
	[Serializable]
	public class NativeCimQualifier
	{
		public bool IsAmmended
		{
			get;set;
		}
		
		public bool IsLocal
		{
			get;set;
		}

		public bool IsOverridable
		{
			get;set;
		}

		public bool PropagatesToInstance
		{
			get;set;
		}

		public bool PropagatesToSuperClass
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
	}
	
	internal static class NativeCimQualifiersHelper
	{
		private static readonly XmlSerializer serializer = new XmlSerializer(typeof(NativeCimQualifiers), new Type[] { typeof(List<string>), typeof(string[]) });
		
		
		public static string Serialize (NativeCimQualifiers obj)
		{
			StringBuilder sb = new StringBuilder();
			StringWriter writer = new StringWriter(sb);
			serializer.Serialize (writer, obj);
			return sb.ToString ();
		}
		
		public static NativeCimQualifiers Deserialize(string target)
		{
			StringReader reader = new StringReader(target);
			NativeCimQualifiers obj = (NativeCimQualifiers)serializer.Deserialize(reader);
			return obj;
		}
	}
}

