using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace System.Management.Classes
{
	[Guid("9ce7daf0-2af7-4a89-9eac-da61b7811dd8")]
	internal class UNIX_OperatingSystem : CIM_OperatingSystem
	{
		public UNIX_OperatingSystem ()
		{

		}

		protected override void RegisterProperies()
		{
			base.RegisterProperies ();
			RegisterProperty ("ComputerName", CimType.String, 0);
			RegisterProperty ("Description", CimType.String, 0);
		}

		
		protected override QueryParser Parser { 
			get { return new QueryParser<UNIX_ComputerSystem> (); } 
		}

		public override string PathField {
			get { return "ComputerName"; }
		}

		protected override IUnixWbemClassHandler Build (object nativeObj)
		{
			return base.Build (nativeObj).WithProperty (PathField, System.Net.Dns.GetHostName ())
				.WithProperty ("Description", OSHelper.GetComputerDescription().ToString ());;
		}

		public override IUnixWbemClassHandler InvokeMethod (string methodName, IUnixWbemClassHandler obj)
		{
			return base.InvokeMethod (methodName, obj);
		} 


	}
}

