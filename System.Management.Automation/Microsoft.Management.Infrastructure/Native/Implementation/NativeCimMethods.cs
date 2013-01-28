using System;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace Microsoft.Management.Infrastructure.Native
{
	[Serializable]
	public class NativeCimMethods : List<NativeCimMethod>
	{
		public void Add(string name, string origin, string inSignature, string outSignature)
		{
			Add(new NativeCimMethod { Name = name, Origin = origin, InSignature = inSignature, OutSignature = outSignature });
		}
	}

	[Serializable]
	public class NativeCimMethod
	{
		public string InSignature { get; set; }
		
		public string OutSignature { get; set; }

		public string Origin
		{
			get;set;
		}

		public string Name
		{
			get;set;
		}


	}

	internal static class NativeCimMethodsHelper
	{
		private static readonly XmlSerializer serializer = new XmlSerializer(typeof(NativeCimMethods));


		public static string Serialize (NativeCimMethods obj)
		{
			StringBuilder sb = new StringBuilder();
			StringWriter writer = new StringWriter(sb);
			serializer.Serialize (writer, obj);
			return sb.ToString ();
		}

		public static NativeCimMethods Deserialize(string target)
		{
			StringReader reader = new StringReader(target);
			NativeCimMethods obj = (NativeCimMethods)serializer.Deserialize(reader);
			return obj;
		}
	}
}

