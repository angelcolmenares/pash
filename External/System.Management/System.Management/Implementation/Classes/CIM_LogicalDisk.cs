using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace System.Management.Classes
{
	[Guid("075e8f60-8bf5-43ce-adbc-262392d95655")]
	[MetaImplementation(typeof(CIM_LogicalDisk_MetaImplementation))]
	internal abstract class CIM_LogicalDisk : CIM_ManagedSystemElement
	{
		public CIM_LogicalDisk ()
		{

		}

		protected override void RegisterProperies()
		{
			base.RegisterProperies ();
			RegisterProperty ("DeviceId", CimType.String, 0);
			RegisterProperty ("Size", CimType.UInt64, 0);
			RegisterProperty ("FreeSpace", CimType.UInt64, 0);
			RegisterProperty ("VolumeLabel", CimType.String, 0);
			RegisterProperty ("IsReady", CimType.Boolean, 0);
			RegisterProperty ("DriveFormat", CimType.String, 0);
			RegisterProperty ("DriveType", CimType.String, 0);
		}
		
		public override string PathField {
			get { return "DeviceId"; }
		}

		public override System.Collections.Generic.IEnumerable<object> Get (string strQuery)
		{
			return Get (System.IO.DriveInfo.GetDrives (), strQuery);
		}

		protected override IUnixWbemClassHandler Build(object nativeObj)
		{
			var drive = (System.IO.DriveInfo)nativeObj;
			var ret = base.Build(nativeObj)
				.WithProperty ("DeviceId", drive.Name)
					.WithProperty ("Size", (ulong)drive.TotalSize)
					.WithProperty ("FreeSpace", (ulong)drive.AvailableFreeSpace)
					.WithProperty ("VolumeLabel", drive.VolumeLabel)
					.WithProperty ("IsReady", drive.IsReady)
					.WithProperty ("DriveFormat", drive.DriveFormat)
					.WithProperty ("DriveType", drive.DriveType.ToString ());
			
			return ret;
		}

		public string DeviceId { get { return GetPropertyAs<string> ("DeviceId"); } }
		public ulong Size { get { return GetPropertyAs<ulong> ("Size"); } }
		public ulong FreeSpace { get { return GetPropertyAs<ulong> ("FreeSpace"); } }
		public string VolumeLabel { get { return GetPropertyAs<string> ("VolumeLabel"); } }
		public bool IsReady { get { return GetPropertyAs<bool> ("IsReady"); } }
		public string DriveFormat { get { return GetPropertyAs<string> ("DriveFormat"); } }
		public string DriveType { get { return GetPropertyAs<string> ("DriveType"); } }

	}

	internal class CIM_LogicalDisk_MetaImplementation : CIM_LogicalDisk
	{
		protected override QueryParser Parser { 
			get { return new QueryParser<CIM_LogicalDisk_MetaImplementation> (); } 
		}

		protected override bool IsMetaImplementation
		{
			get { return true; }
		}
	}
}

