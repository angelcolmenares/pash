using System;
using System.Collections.Generic;
using System.Management;
using System.ServiceModel;
using System.ServiceModel.Channels;
using Microsoft.Management.Infrastructure.Native;
using System.Text;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;

namespace Microsoft.WSMan.Cim
{
	public class CimEnumerator : IDisposable
	{
		private bool _isLocal;

		public CimEnumerator ()
		{

		}

		public CimEnumerator (bool isLocal)
		{
			_isLocal = isLocal;
		}

		public IEnumerable<object> Get (string username, string password, string nameSpace, string filter)
		{
			var context = new ManagementNamedValueCollection();
			ManagementScope scope = new ManagementScope(GetScopeString("localhost", nameSpace), new ConnectionOptions("en-US", username, password, "", ImpersonationLevel.Default, AuthenticationLevel.Default, true, context, TimeSpan.FromSeconds (60)));
			{
				ObjectQuery query = new ObjectQuery(filter);
				using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query, new EnumerationOptions(context, TimeSpan.FromSeconds (60), 4096, true, true, true, false, false, true, true)))
				{
					foreach(ManagementBaseObject obj in searcher.Get())
					{
						yield return ToEndointAddress(obj,  _isLocal);
					}
				}
			}
		}

		public int GetCount (string username, string password, string nameSpace, string filter)
		{
			var context = new ManagementNamedValueCollection();
			ManagementScope scope = new ManagementScope(GetScopeString("localhost", nameSpace), new ConnectionOptions("en-US", username, password, "", ImpersonationLevel.Default, AuthenticationLevel.Default, true, context, TimeSpan.FromSeconds (60)));
			{
				ObjectQuery query = new ObjectQuery(filter);
				using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query, new EnumerationOptions(context, TimeSpan.FromSeconds (60), 4096, true, true, true, false, false, true, true)))
				{
					return searcher.Get ().Count;
				}
			}
		}

		internal static string GetScopeString(string computer, string namespaceParameter)
		{
			StringBuilder stringBuilder = new StringBuilder("\\\\");
			stringBuilder.Append(computer);
			stringBuilder.Append("\\");
			stringBuilder.Append(namespaceParameter);
			return stringBuilder.ToString();
		}

		public static EndpointAddress ToEndointAddress (ManagementBaseObject obj, bool isLocal)
		{
			EndpointAddressBuilder builder = new EndpointAddressBuilder();
			builder.Identity = new X509CertificateEndpointIdentity (new X509Certificate2 ("powershell.pfx", "mono"));
			foreach(var header in GetAddressHeaders(obj, isLocal))
			{
				builder.Headers.Add (header);
			}
			builder.Uri = new Uri(CimNamespaces.CimNamespace + "/" + obj.ClassPath.ClassName);
			return builder.ToEndpointAddress ();
		}

		static AddressHeader CreateAddressHeader (string name, string ns, object obj)
		{
			//Serialize object to xml
			return AddressHeader.CreateAddressHeader (name, ns, obj);
		}

		static IEnumerable<AddressHeader> GetAddressHeaders (ManagementBaseObject obj, bool isLocal)
		{
			yield return CreateAddressHeader ("ClassName", CimNamespaces.CimNamespace, obj.ClassPath.ClassName);
			yield return CreateAddressHeader ("IsClass", CimNamespaces.CimNamespace, obj.ClassPath.IsClass);
			yield return CreateAddressHeader ("IsInstance", CimNamespaces.CimNamespace, obj.ClassPath.IsInstance);
			yield return CreateAddressHeader ("IsSingleton", CimNamespaces.CimNamespace, obj.ClassPath.IsSingleton);
			yield return CreateAddressHeader ("NamespacePath", CimNamespaces.CimNamespace, obj.ClassPath.NamespacePath);
			yield return CreateAddressHeader ("ServerName", CimNamespaces.CimNamespace, obj.ClassPath.Server);
			yield return CreateAddressHeader ("ObjectExits", CimNamespaces.CimNamespace, obj.ObjectExits);
			yield return CreateAddressHeader ("Qualifiers", CimNamespaces.CimNamespace, Serialize (obj.Qualifiers));
			yield return CreateAddressHeader ("Properties", CimNamespaces.CimNamespace, Serialize (obj.Properties, isLocal));
			yield return CreateAddressHeader ("SystemProperties", CimNamespaces.CimNamespace, SerializeSystem(obj.SystemProperties));

			ManagementObject o = obj as ManagementObject;
			if (o != null) {
				yield return CreateAddressHeader ("RelativePath", CimNamespaces.CimNamespace, o.Path.RelativePath);
				yield return CreateAddressHeader ("Path", CimNamespaces.CimNamespace, o.Path.Path);
			} else {
				yield return CreateAddressHeader ("Path", CimNamespaces.CimNamespace, obj.ClassPath.Path);
				yield return CreateAddressHeader ("RelativePath", CimNamespaces.CimNamespace, obj.ClassPath.RelativePath);
			}
			ManagementClass classObj = obj as ManagementClass;
			if (classObj != null) {
				yield return CreateAddressHeader ("CimClassName", CimNamespaces.CimNamespace, "Meta_Class");
				yield return CreateAddressHeader ("Methods", CimNamespaces.CimNamespace, Serialize (classObj.Methods, isLocal));
			}
			else {
				yield return CreateAddressHeader ("CimClassName", CimNamespaces.CimNamespace, obj.ClassPath.ClassName);
			}
		}

		static string Serialize (PropertyDataCollection properties, bool isLocal)
		{
			NativeCimProperties obj = new NativeCimProperties();
			foreach (var p in properties) {
				obj.Add(p.Name, p.Origin, p.IsArray, p.IsLocal, Transform(p.Type), p.Value);
			}
			obj.Add("PSShowComputerName", "", false, true, Microsoft.Management.Infrastructure.CimType.Boolean, true);
			obj.Add("PSComputerName", "", false, true, Microsoft.Management.Infrastructure.CimType.String, isLocal ? "localhost" : properties["__SERVER"].Value);
			return NativeCimPropertiesHelper.Serialize (obj);
		}

		static string SerializeMethodProperties(PropertyDataCollection properties, bool isLocal)
		{
			NativeCimProperties obj = new NativeCimProperties();
			foreach (var p in properties) {
				obj.Add(p.Name, p.Origin, p.IsArray, p.IsLocal, Transform(p.Type), p.Value);
			}
			return NativeCimPropertiesHelper.Serialize (obj);
		}


		static string SerializeSystem(PropertyDataCollection properties)
		{
			NativeCimProperties obj = new NativeCimProperties();
			foreach (var p in properties) {
				obj.Add(p.Name, p.Origin, p.IsArray, p.IsLocal, Transform(p.Type), p.Value);
			}
			return NativeCimPropertiesHelper.Serialize (obj);
		}

		private static Microsoft.Management.Infrastructure.CimType Transform (CimType type)
		{
			switch (type) {
			case CimType.None:
					return Microsoft.Management.Infrastructure.CimType.Unknown;
			case CimType.SInt16:
					return Microsoft.Management.Infrastructure.CimType.SInt16;
			case CimType.SInt32:
					return Microsoft.Management.Infrastructure.CimType.SInt32;
			case CimType.Real32:
					return Microsoft.Management.Infrastructure.CimType.Real32;
			case CimType.Real64:
					return Microsoft.Management.Infrastructure.CimType.Real64;
			case CimType.String:
					return Microsoft.Management.Infrastructure.CimType.String;
			case CimType.Boolean:
				return Microsoft.Management.Infrastructure.CimType.Boolean;
			case CimType.Object:
				return Microsoft.Management.Infrastructure.CimType.Instance;
			case CimType.SInt8:
				return Microsoft.Management.Infrastructure.CimType.SInt8;
			case CimType.UInt8:
				return Microsoft.Management.Infrastructure.CimType.UInt8;
			case CimType.UInt16:
				return Microsoft.Management.Infrastructure.CimType.UInt16;
			case CimType.UInt32:
				return Microsoft.Management.Infrastructure.CimType.UInt32;
			case CimType.SInt64:
				return Microsoft.Management.Infrastructure.CimType.SInt64;
			case CimType.UInt64:
				return Microsoft.Management.Infrastructure.CimType.UInt64;
			case CimType.DateTime:
				return Microsoft.Management.Infrastructure.CimType.DateTime;
			case CimType.Reference:
				return Microsoft.Management.Infrastructure.CimType.Reference;
			case CimType.Char16:
				return Microsoft.Management.Infrastructure.CimType.Char16;
			}

			return Microsoft.Management.Infrastructure.CimType.Unknown;
		}

		static string Serialize (QualifierDataCollection properties)
		{
			NativeCimQualifiers obj = new NativeCimQualifiers();
			foreach (var q in properties) {
				obj.Add(q.Name, q.IsAmended, q.IsLocal, q.IsOverridable, q.PropagatesToInstance, q.PropagatesToSubclass, q.Value);
			}
			return NativeCimQualifiersHelper.Serialize (obj);
		}

		static string Serialize (MethodDataCollection methods, bool isLocal)
		{
			NativeCimMethods obj = new NativeCimMethods();
			foreach (var m in methods) {
				obj.Add(m.Name, m.Origin, SerializeMethodProperties(m.InParameters.Properties, isLocal), SerializeMethodProperties(m.OutParameters.Properties, isLocal));
			}
			return NativeCimMethodsHelper.Serialize (obj);
		}

		#region IDisposable implementation		
		public void Dispose ()
		{

		}		
		#endregion
	}
}

