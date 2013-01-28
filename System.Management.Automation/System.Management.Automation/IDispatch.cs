namespace System.Management.Automation
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;

    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("00020400-0000-0000-c000-000000000046")]
    internal interface IDispatch
    {
        [PreserveSig]
        int GetTypeInfoCount(out int info);
        [PreserveSig]
        int GetTypeInfo(int iTInfo, int lcid, out ITypeInfo ppTInfo);
        [PreserveSig]
        int GetIDsOfNames([In] ref Guid iid_null, [In, MarshalAs(UnmanagedType.LPWStr)] string[] rgszNames, int cNames, int lcid, [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=2)] int[] rgDispId);
        [PreserveSig]
        int Invoke(int dispIdMember, [In] ref Guid iid_null, int lcid, int wFlags, [In, MarshalAs(UnmanagedType.LPArray)] System.Runtime.InteropServices.ComTypes.DISPPARAMS[] pDispParams, [MarshalAs(UnmanagedType.Struct)] out object pVarResult, ref System.Runtime.InteropServices.ComTypes.EXCEPINFO pExcepInfo, [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=0, SizeConst=1)] int[] puArgErr);
    }
}

