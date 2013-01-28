using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Management.Infrastructure.Internal;

namespace Microsoft.Management.Infrastructure.Native
{
	internal class InstanceMethods
	{
		internal static ValueType maxValidCimTimestamp;

		static InstanceMethods()
		{
			DateTime dateTime = new DateTime();
			ValueType valueType = dateTime;
			//new DateTime(0x270f, 12, 31, 23, 59, 59, 0x3e7, DateTimeKind.Utc)
			InstanceMethods.maxValidCimTimestamp = valueType;
		}

		private InstanceMethods()
		{

		}

		

		internal static void ThrowIfMismatchedType(MiType type, object managedValue)
		{
			
		}

		public static MiResult GetServerName (Microsoft.Management.Infrastructure.Native.InstanceHandle instanceHandle, out string str)
		{
			IntPtr ptr = instanceHandle.DangerousGetHandle ();
			NativeCimInstance instance = CimNativeApi.MarshalledObject.FromPointer<NativeCimInstance>(ptr);
			str = instance.ServerName;
			return MiResult.OK;
		}

		public static MiResult SetServerName (Microsoft.Management.Infrastructure.Native.InstanceHandle handle, string serverName)
		{
			IntPtr ptr = handle.DangerousGetHandle ();
			NativeCimInstance instance = CimNativeApi.MarshalledObject.FromPointer<NativeCimInstance>(ptr);
			instance.ServerName = serverName;
			return MiResult.OK;
		}

		public static MiResult SetNamespace (Microsoft.Management.Infrastructure.Native.InstanceHandle handle, string @namespace)
		{
			IntPtr ptr = handle.DangerousGetHandle ();
			NativeCimInstance instance = CimNativeApi.MarshalledObject.FromPointer<NativeCimInstance>(ptr);
			instance.Namespace = @namespace;
			return MiResult.OK;
		}

		public static MiResult GetClass (Microsoft.Management.Infrastructure.Native.InstanceHandle instanceHandle, out ClassHandle classHandle)
		{
			MiResult result = MiResult.OK;
			IntPtr ptr = instanceHandle.DangerousGetHandle ();
			NativeCimInstance instance = CimNativeApi.MarshalledObject.FromPointer<NativeCimInstance> (ptr);
			classHandle = null;
			if (!string.IsNullOrEmpty (instance.ClassName)) {
				var options = CimNativeApi.GetDestinationOptions (new SessionHandle(instance.SessionHandle));
				var classObj = CimNativeApi.QueryClasses (options, instance.Namespace, CimNativeApi.WQLNamespace, "SELECT * FROM Meta_Class WHERE ClassName = '" + instance.ClassName + "'").FirstOrDefault ();
				classObj.SessionHandle = instance.SessionHandle;
				IntPtr classPtr = CimNativeApi.MarshalledObject.Create<NativeCimClass>(classObj);
				classHandle = new ClassHandle (classPtr, true);
			}
			return result;
		}

		public static MiResult GetClassName (Microsoft.Management.Infrastructure.Native.InstanceHandle instanceHandle, out string str1)
		{
			IntPtr ptr = instanceHandle.DangerousGetHandle ();
			NativeCimInstance instance = CimNativeApi.MarshalledObject.FromPointer<NativeCimInstance>(ptr);
			str1 = instance.CimClassName;
			return MiResult.OK;
		}

		public static MiResult GetNamespace (Microsoft.Management.Infrastructure.Native.InstanceHandle instanceHandle, out string str2)
		{
			IntPtr ptr = instanceHandle.DangerousGetHandle ();
			NativeCimInstance instance = CimNativeApi.MarshalledObject.FromPointer<NativeCimInstance>(ptr);
			str2 = instance.Namespace;
			return MiResult.OK;
		}

		public static MiResult GetElementCount (InstanceHandle handle, out int num)
		{
			NativeCimInstance instance = CimNativeApi.MarshalledObject.FromPointer<NativeCimInstance> (handle.DangerousGetHandle ());
			var properties = NativeCimPropertiesHelper.Deserialize (PropertiesOrSystem(instance));
			num = properties.Count;
			return MiResult.OK;
		}

		private static string PropertiesOrSystem (NativeCimInstance instance)
		{
			if (instance.CimClassName.Equals ("meta_class", StringComparison.OrdinalIgnoreCase)) 
			{
				return instance.SystemProperties;
			}
			return instance.Properties;
		}


		public static MiResult GetElement_GetIndex (InstanceHandle handle, string propertyName, out int num)
		{
			NativeCimInstance instance = CimNativeApi.MarshalledObject.FromPointer<NativeCimInstance> (handle.DangerousGetHandle ());
			var properties = NativeCimPropertiesHelper.Deserialize (PropertiesOrSystem(instance));
			num = -1;
			int i = 0;
			foreach(var element in properties) {

				if (element.Name == propertyName)
				{
					num = i;
					break;
				}
				i++;
			}
			return MiResult.OK;
		}

		public static MiResult AddElement (InstanceHandle handle, string name, object obj, object par, MiFlags miFlags)
		{
			NativeCimInstance instance = CimNativeApi.MarshalledObject.FromPointer<NativeCimInstance>(handle.DangerousGetHandle ());
			NativeCimProperties properties = NativeCimPropertiesHelper.Deserialize (instance.Properties);

			CimType type = CimConverter.GetCimType (obj.GetType ());
			properties.Add (new NativeCimProperty { Name = name, Type = type, Origin = "client", IsArray = false, IsLocal = false, Value = obj });
			instance.Properties = NativeCimPropertiesHelper.Serialize (properties);
			handle.DangerousSetHandle ((IntPtr)CimNativeApi.MarshalledObject.Create<NativeCimInstance>(instance));
			return MiResult.OK;
		}

		public static MiResult GetElementAt_GetType (InstanceHandle handle, int _index, out MiType miType)
		{
			NativeCimInstance instance = CimNativeApi.MarshalledObject.FromPointer<NativeCimInstance> (handle.DangerousGetHandle ());
			var properties = NativeCimPropertiesHelper.Deserialize (PropertiesOrSystem(instance));
			int i = 0;
			miType = MiType.Boolean;
			foreach (var element in properties) {
				if (i == _index) {
					Type type = element.Value.GetType ();
					miType = CimConverter.GetCimType (type).ToMiType();
					break;
				}
				i++;
			}
			return MiResult.OK;
		}

		public static MiResult SetElementAt_SetNotModifiedFlag (InstanceHandle handle, int _index, bool flag)
		{
			return MiResult.OK;
		}

		public static MiResult GetElementAt_GetFlags (InstanceHandle handle, int _index, out MiFlags miFlag)
		{
			miFlag = MiFlags.PROPERTY;
			return MiResult.OK;
		}

		public static MiResult GetElementAt_GetName (InstanceHandle handle, int _index, out string str)
		{
			NativeCimInstance instance = CimNativeApi.MarshalledObject.FromPointer<NativeCimInstance> (handle.DangerousGetHandle ());
			var properties = NativeCimPropertiesHelper.Deserialize (PropertiesOrSystem(instance));
			int i = 0;
			str = null;
			foreach (var element in properties) {
				if (i == _index) {
					str = element.Name;
					break;
				}
				i++;
			}
			return MiResult.OK;
		}

		public static MiResult GetElementAt_GetValue (InstanceHandle handle, int _index, out object obj)
		{
			NativeCimInstance instance = CimNativeApi.MarshalledObject.FromPointer<NativeCimInstance> (handle.DangerousGetHandle ());
			obj = null;
			if (_index != -1) {
				var properties = NativeCimPropertiesHelper.Deserialize (PropertiesOrSystem(instance));
				var element = properties.ElementAtOrDefault (_index);
				obj = element.Value;
			}
			return MiResult.OK;
		}

		public static MiResult SetElementAt_SetValue (InstanceHandle handle, int _index, object obj)
		{
			NativeCimInstance instance = CimNativeApi.MarshalledObject.FromPointer<NativeCimInstance> (handle.DangerousGetHandle ());
			var properties = NativeCimPropertiesHelper.Deserialize (PropertiesOrSystem(instance));
			properties.ElementAt(_index).Value = obj;
			instance.Properties = NativeCimPropertiesHelper.Serialize (properties);
			handle.DangerousSetHandle ((IntPtr)CimNativeApi.MarshalledObject.Create<NativeCimInstance>(instance));
			return MiResult.OK;
		}

		public static MiResult ClearElementAt (InstanceHandle handle, int _index)
		{
			NativeCimInstance instance = CimNativeApi.MarshalledObject.FromPointer<NativeCimInstance> (handle.DangerousGetHandle ());
			var properties = NativeCimPropertiesHelper.Deserialize (PropertiesOrSystem(instance));
			properties.ElementAt(_index).Value = null; 
			instance.Properties = NativeCimPropertiesHelper.Serialize (properties);
			handle.DangerousSetHandle ((IntPtr)CimNativeApi.MarshalledObject.Create<NativeCimInstance>(instance));
			return MiResult.OK;
		}
	}
}