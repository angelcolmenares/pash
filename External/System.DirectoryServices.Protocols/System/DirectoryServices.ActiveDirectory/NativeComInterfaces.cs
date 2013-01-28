using System;
using System.Runtime.InteropServices;
using System.Security;

namespace System.DirectoryServices.ActiveDirectory
{
	[ComVisible(false)]
	[SuppressUnmanagedCodeSecurity]
	internal sealed class NativeComInterfaces
	{
		internal const int ADS_SETTYPE_DN = 4;

		internal const int ADS_FORMAT_X500_DN = 7;

		internal const int ADS_ESCAPEDMODE_ON = 2;

		internal const int ADS_ESCAPEDMODE_OFF_EX = 4;

		internal const int ADS_FORMAT_LEAF = 11;

		public NativeComInterfaces()
		{
		}

		[Guid("C8F93DD0-4AE0-11CF-9E73-00AA004A5691")]
		[InterfaceType(ComInterfaceType.InterfaceIsDual)]
		internal interface IAdsClass
		{
			bool Abstract
			{
				get;
				set;
			}

			string ADsPath
			{
				get;
			}

			object AuxDerivedFrom
			{
				[SuppressUnmanagedCodeSecurity]
				get;
				set;
			}

			bool Auxiliary
			{
				get;
				set;
			}

			string Class
			{
				get;
			}

			string CLSID
			{
				get;
				set;
			}

			bool Container
			{
				get;
				set;
			}

			object Containment
			{
				get;
				set;
			}

			object DerivedFrom
			{
				get;
				set;
			}

			string GUID
			{
				get;
			}

			int HelpFileContext
			{
				get;
				set;
			}

			string HelpFileName
			{
				get;
				set;
			}

			object MandatoryProperties
			{
				[SuppressUnmanagedCodeSecurity]
				get;
				set;
			}

			string Name
			{
				get;
			}

			object NamingProperties
			{
				get;
				set;
			}

			string OID
			{
				[SuppressUnmanagedCodeSecurity]
				get;
				set;
			}

			object OptionalProperties
			{
				[SuppressUnmanagedCodeSecurity]
				get;
				set;
			}

			string Parent
			{
				get;
			}

			object PossibleSuperiors
			{
				[SuppressUnmanagedCodeSecurity]
				get;
				set;
			}

			string PrimaryInterface
			{
				get;
			}

			string Schema
			{
				get;
			}

			object Get(string bstrName);

			object GetEx(string bstrName);

			void GetInfo();

			void GetInfoEx(object vProperties, int lnReserved);

			void Put(string bstrName, object vProp);

			void PutEx(int lnControlCode, string bstrName, object vProp);

			object Qualifiers();

			void SetInfo();
		}

		[Guid("D592AED4-F420-11D0-A36E-00C04FB950DC")]
		[InterfaceType(ComInterfaceType.InterfaceIsDual)]
		internal interface IAdsPathname
		{
			int EscapedMode
			{
				get;
				[SuppressUnmanagedCodeSecurity]
				set;
			}

			void AddLeafElement(string bstrLeafElement);

			object CopyPath();

			string GetElement(int lnElementIndex);

			[SuppressUnmanagedCodeSecurity]
			string GetEscapedElement(int lnReserved, string bstrInStr);

			int GetNumElements();

			void RemoveLeafElement();

			[SuppressUnmanagedCodeSecurity]
			string Retrieve(int lnFormatType);

			[SuppressUnmanagedCodeSecurity]
			int Set(string bstrADsPath, int lnSetType);

			int SetDisplayType(int lnDisplayType);
		}

		[Guid("C8F93DD3-4AE0-11CF-9E73-00AA004A5691")]
		[InterfaceType(ComInterfaceType.InterfaceIsDual)]
		internal interface IAdsProperty
		{
			string ADsPath
			{
				get;
			}

			string Class
			{
				get;
			}

			string GUID
			{
				get;
			}

			int MaxRange
			{
				[SuppressUnmanagedCodeSecurity]
				get;
				set;
			}

			int MinRange
			{
				[SuppressUnmanagedCodeSecurity]
				get;
				set;
			}

			bool MultiValued
			{
				[SuppressUnmanagedCodeSecurity]
				get;
				set;
			}

			string Name
			{
				get;
			}

			string OID
			{
				[SuppressUnmanagedCodeSecurity]
				get;
				set;
			}

			string Parent
			{
				get;
			}

			string Schema
			{
				get;
			}

			string Syntax
			{
				get;
				set;
			}

			object Get(string bstrName);

			object GetEx(string bstrName);

			void GetInfo();

			void GetInfoEx(object vProperties, int lnReserved);

			void Put(string bstrName, object vProp);

			void PutEx(int lnControlCode, string bstrName, object vProp);

			object Qualifiers();

			void SetInfo();
		}

		[Guid("080d0d78-f421-11d0-a36e-00c04fb950dc")]
		internal class Pathname
		{
			public extern Pathname();
		}
	}
}