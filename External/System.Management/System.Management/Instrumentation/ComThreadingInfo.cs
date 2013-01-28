using System;
using System.Runtime.InteropServices;

namespace System.Management.Instrumentation
{
	internal class ComThreadingInfo
	{
		private Guid IID_IUnknown;

		private ComThreadingInfo.APTTYPE apartmentType;

		private ComThreadingInfo.THDTYPE threadType;

		private Guid logicalThreadId;

		public ComThreadingInfo.APTTYPE ApartmentType
		{
			get
			{
				return this.apartmentType;
			}
		}

		public static ComThreadingInfo Current
		{
			get
			{
				return new ComThreadingInfo();
			}
		}

		public Guid LogicalThreadId
		{
			get
			{
				return this.logicalThreadId;
			}
		}

		public ComThreadingInfo.THDTYPE ThreadType
		{
			get
			{
				return this.threadType;
			}
		}

		private ComThreadingInfo()
		{
			this.IID_IUnknown = new Guid("00000000-0000-0000-C000-000000000046");
			ComThreadingInfo.IComThreadingInfo comThreadingInfo = (ComThreadingInfo.IComThreadingInfo)ComThreadingInfo.CoGetObjectContext(ref this.IID_IUnknown);
			this.apartmentType = comThreadingInfo.GetCurrentApartmentType();
			this.threadType = comThreadingInfo.GetCurrentThreadType();
			this.logicalThreadId = comThreadingInfo.GetCurrentLogicalThreadId();
		}

		/*
		[DllImport("ole32.dll", CharSet=CharSet.None)]
		private static extern object CoGetObjectContext(ref Guid riid);
		*/

		private static object CoGetObjectContext (ref Guid riid)
		{
			return null;
		}


		public override string ToString()
		{
			return string.Format("{{{0}}} - {1} - {2}", this.LogicalThreadId, this.ApartmentType, this.ThreadType);
		}

		public enum APTTYPE
		{
			APTTYPE_CURRENT = -1,
			APTTYPE_STA = 0,
			APTTYPE_MTA = 1,
			APTTYPE_NA = 2,
			APTTYPE_MAINSTA = 3
		}

		[Guid("000001ce-0000-0000-C000-000000000046")]
		[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		private interface IComThreadingInfo
		{
			ComThreadingInfo.APTTYPE GetCurrentApartmentType();

			Guid GetCurrentLogicalThreadId();

			ComThreadingInfo.THDTYPE GetCurrentThreadType();

			void SetCurrentLogicalThreadId(Guid rguid);
		}

		public enum THDTYPE
		{
			THDTYPE_BLOCKMESSAGES,
			THDTYPE_PROCESSMESSAGES
		}
	}
}