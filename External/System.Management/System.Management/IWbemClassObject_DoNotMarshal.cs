using System;
using System.Runtime.InteropServices;

namespace System.Management
{
	[Guid("DC12A681-737F-11CF-884D-00AA004B2E24")]
	[InterfaceType(1)]
	[TypeLibType(0x200)]
	internal interface IWbemClassObject_DoNotMarshal
	{
		object NativeObject { get; }

		int BeginEnumeration_(int lEnumFlags);

		int BeginMethodEnumeration_(int lEnumFlags);

		int Clone_(out IWbemClassObject_DoNotMarshal ppCopy);

		int CompareTo_(int lFlags, IWbemClassObject_DoNotMarshal pCompareTo);

		int Delete_(string wszName);

		int DeleteMethod_(string wszName);

		int EndEnumeration_();

		int EndMethodEnumeration_();

		int Get_(string wszName, int lFlags, out object pVal, out int pType, out int plFlavor);

		int GetMethod_(string wszName, int lFlags, out IWbemClassObject_DoNotMarshal ppInSignature, out IWbemClassObject_DoNotMarshal ppOutSignature);

		int GetMethodOrigin_(string wszMethodName, out string pstrClassName);

		int ExecuteMethod_ (string wszMethodName, object ppInSignature, out object ppOutSignature);

		int GetMethodQualifierSet_(string wszMethod, out IWbemQualifierSet_DoNotMarshal ppQualSet);

		int GetNames_(string wszQualifierName, int lFlags, ref object pQualifierVal, out string[] pNames);

		int GetObjectText_(int lFlags, out string pstrObjectText);

		int GetPropertyOrigin_(string wszName, out string pstrClassName);

		int GetPropertyQualifierSet_(string wszProperty, out IWbemQualifierSet_DoNotMarshal ppQualSet);

		int GetQualifierSet_(out IWbemQualifierSet_DoNotMarshal ppQualSet);

		int InheritsFrom_(string strAncestor);

		int Next_(int lFlags, out string strName, out object pVal, out int pType, out int plFlavor);

		int NextMethod_(int lFlags, out string pstrName, out IWbemClassObject_DoNotMarshal ppInSignature, out IWbemClassObject_DoNotMarshal ppOutSignature);

		int Put_(string wszName, int lFlags, ref object pVal, int Type);

		int PutMethod_(string wszName, int lFlags, IWbemClassObject_DoNotMarshal pInSignature, IWbemClassObject_DoNotMarshal pOutSignature);

		int SpawnDerivedClass_(int lFlags, out IWbemClassObject_DoNotMarshal ppNewClass);

		int SpawnInstance_(int lFlags, out IWbemClassObject_DoNotMarshal ppNewInstance);
	}
}