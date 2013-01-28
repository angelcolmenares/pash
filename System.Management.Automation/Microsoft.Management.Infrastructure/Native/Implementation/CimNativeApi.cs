using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace Microsoft.Management.Infrastructure.Native
{
	internal static class CimNativeApi
	{
		private static INativeCimHandler _apiHandler;
		private static INativeCimHandler _apiLocalHandler;
		private static readonly object _lock = new object();
		public const string WQLNamespace = "http://schemas.microsoft.com/wbem/wsman/1/WQL";

		private static INativeCimHandler Handler {
			get {
				if (_apiHandler == null) {
					Type type = Type.GetType ("Microsoft.WSMan.Cim.WSManNativeCimHandler, Microsoft.WSMan.Cim, Version=3.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756");
					if (type != null)
					{
						lock(_lock)
						{
							_apiHandler = (INativeCimHandler)Activator.CreateInstance (type, true);
						}
					}
				}
				return _apiHandler;
			}
		}

		private static INativeCimHandler LocalHandler {
			get {
				if (_apiLocalHandler == null) {
					Type type = Type.GetType ("Microsoft.WSMan.Cim.WSManNativeLocalCimHandler, Microsoft.WSMan.Cim, Version=3.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756");
					if (type != null)
					{
						lock(_lock)
						{
							_apiLocalHandler = (INativeCimHandler)Activator.CreateInstance (type, true);
						}
					}
				}
				return _apiLocalHandler;
			}
		}


		public static INativeCimHandler GetHandler (NativeDestinationOptions options)
		{
			if (string.IsNullOrEmpty (options.ServerName) || options.ServerName.Equals ("localhost", StringComparison.OrdinalIgnoreCase)) return LocalHandler;
			return Handler;
		}

		public static NativeCimInstance InvokeMethod (NativeDestinationOptions options, string namespaceName, string className, string methodName, NativeCimInstance instance, NativeCimInstance inSignature)
		{
			return GetHandler (options).InvokeMethod(namespaceName, className, methodName, instance, inSignature);
		}

		/// <summary>
		/// Queries the classes.
		/// </summary>
		/// <returns>
		/// The classes.
		/// </returns>
		/// <param name='namespaceName'>
		/// Namespace name.
		/// </param>
		/// <param name='queryDialect'>
		/// Query dialect.
		/// </param>
		/// <param name='queryExpression'>
		/// Query expression.
		/// </param>
		public static IEnumerable<NativeCimClass> QueryClasses(NativeDestinationOptions options, string namespaceName, string queryDialect, string queryExpression)
		{
			return GetHandler(options).QueryClasses (options, namespaceName, queryDialect, queryExpression);
		}

		/// <summary>
		/// Queries the instances.
		/// </summary>
		/// <returns>
		/// The instances.
		/// </returns>
		/// <param name='namespaceName'>
		/// Namespace name.
		/// </param>
		/// <param name='queryDialect'>
		/// Query dialect.
		/// </param>
		/// <param name='queryExpression'>
		/// Query expression.
		/// </param>
		/// <param name='keysOnly'>
		/// If set to <c>true</c> keys only.
		/// </param>
		public static IEnumerable<NativeCimInstance> QueryInstances(NativeDestinationOptions options, string namespaceName, string queryDialect, string queryExpression, bool keysOnly)
		{
			return GetHandler(options).QueryInstances (options, namespaceName, queryDialect, queryExpression, keysOnly);
		}

		public static string GetPassword (System.Security.SecureString s)
		{
			return ByteArrayToString (GetData (s));
		}
		
		private static string ByteArrayToString (byte[] data)
		{
			var ret = new System.Collections.Generic.List<byte> ();
			foreach (var b in data) {
				if (b != 0)
				{
					ret.Add(b);
				}
			}
			return System.Text.Encoding.UTF8.GetString (ret.ToArray ());
		}
		
		private static byte[] GetData(System.Security.SecureString s)
		{
			System.Reflection.FieldInfo fi = s.GetType().GetField ("data", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			return (byte[])fi.GetValue (s);
		}


		[StructLayout(LayoutKind.Sequential)]
		internal struct MarshalledObject : IDisposable
		{
			private IntPtr dataPtr;
			internal MarshalledObject(IntPtr dataPtr)
			{
				this.dataPtr = dataPtr;
			}
			
			internal IntPtr DataPtr
			{
				get
				{
					return this.dataPtr;
				}
			}

			internal static MarshalledObject Create<T>(T obj)
			{
				IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(T)));
				Marshal.StructureToPtr(obj, ptr, false);
				return new MarshalledObject { dataPtr = ptr };
			}
			
			internal static T FromPointer<T>(IntPtr ptr)
			{
				return (T)Marshal.PtrToStructure (ptr, typeof(T));
			}
			
			public void Dispose()
			{
				if (IntPtr.Zero != this.dataPtr)
				{
					Marshal.FreeHGlobal(this.dataPtr);
					this.dataPtr = IntPtr.Zero;
				}
			}
			
			public static implicit operator IntPtr(MarshalledObject obj)
			{
				return obj.dataPtr;
			}
		}

		public static NativeDestinationOptions GetDestinationOptions (SessionHandle handle)
		{
			NativeCimSession session = MarshalledObject.FromPointer<NativeCimSession> (handle.DangerousGetHandle ());
			NativeDestinationOptions options = new NativeDestinationOptions();
			if (session.DestinationOptions == IntPtr.Zero) {
				/* Setup Default Destination Options */
				options.ServerName = session.ServerName;
				options.DestinationPort = 5985;
			}
			else 
			{
				options = MarshalledObject.FromPointer<NativeDestinationOptions> (session.DestinationOptions);
				if (string.IsNullOrEmpty (options.ServerName)) options.ServerName = session.ServerName;
				if (options.DestinationPort == 0) options.DestinationPort = 5985;
			}
			return options;
		}
	}
}

