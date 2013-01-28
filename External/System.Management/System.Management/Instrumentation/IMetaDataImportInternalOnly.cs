using System;
using System.Runtime.InteropServices;
using System.Text;

namespace System.Management.Instrumentation
{
	[Guid("7DAC8207-D3AE-4c75-9B67-92801A497D44")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[TypeLibType(TypeLibTypeFlags.FRestricted)]
	internal interface IMetaDataImportInternalOnly
	{
		void f1();

		void f2();

		void f3();

		void f4();

		void f5();

		void f6();

		void f7();

		void GetScopeProps(StringBuilder szName, int cchName, out int pchName, out Guid pmvid);
	}
}