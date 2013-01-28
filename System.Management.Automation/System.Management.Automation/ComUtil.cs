namespace System.Management.Automation
{
    using System;
    using System.Collections.ObjectModel;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;
    using System.Text;

    internal class ComUtil
    {
        private static ComMethodInformation GetMethodInformation(System.Runtime.InteropServices.ComTypes.FUNCDESC funcdesc, bool skipLastParameter)
        {
            Type typeFromTypeDesc = GetTypeFromTypeDesc(funcdesc.elemdescFunc.tdesc);
            ParameterInformation[] parameterInformation = GetParameterInformation(funcdesc, skipLastParameter);
            bool hasoptional = false;
            foreach (ParameterInformation information in parameterInformation)
            {
                if (information.isOptional)
                {
                    hasoptional = true;
                    break;
                }
            }
            return new ComMethodInformation(false, hasoptional, parameterInformation, typeFromTypeDesc);
        }

        internal static ComMethodInformation[] GetMethodInformationArray(ITypeInfo typeInfo, Collection<int> methods, bool skipLastParameters)
        {
            int count = methods.Count;
            int num2 = 0;
            ComMethodInformation[] informationArray = new ComMethodInformation[count];
            foreach (int num3 in methods)
            {
                IntPtr ptr;
                typeInfo.GetFuncDesc(num3, out ptr);
                System.Runtime.InteropServices.ComTypes.FUNCDESC funcdesc = (System.Runtime.InteropServices.ComTypes.FUNCDESC) Marshal.PtrToStructure(ptr, typeof(System.Runtime.InteropServices.ComTypes.FUNCDESC));
                informationArray[num2++] = GetMethodInformation(funcdesc, skipLastParameters);
                typeInfo.ReleaseFuncDesc(ptr);
            }
            return informationArray;
        }

        internal static string GetMethodSignatureFromFuncDesc(ITypeInfo typeinfo, System.Runtime.InteropServices.ComTypes.FUNCDESC funcdesc, bool isPropertyPut)
        {
            StringBuilder builder = new StringBuilder();
            string nameFromFuncDesc = GetNameFromFuncDesc(typeinfo, funcdesc);
            if (!isPropertyPut)
            {
                builder.Append(GetStringFromTypeDesc(typeinfo, funcdesc.elemdescFunc.tdesc) + " ");
            }
            builder.Append(nameFromFuncDesc);
            builder.Append(" (");
            IntPtr lprgelemdescParam = funcdesc.lprgelemdescParam;
            int num = Marshal.SizeOf(typeof(System.Runtime.InteropServices.ComTypes.ELEMDESC));
            for (int i = 0; i < funcdesc.cParams; i++)
            {
                IntPtr ptr2;
                System.Runtime.InteropServices.ComTypes.ELEMDESC elemdesc = new System.Runtime.InteropServices.ComTypes.ELEMDESC();
                int num3 = i * num;
                if (IntPtr.Size == 4)
                {
                    ptr2 = (IntPtr) (lprgelemdescParam.ToInt32() + num3);
                }
                else
                {
                    ptr2 = (IntPtr) (lprgelemdescParam.ToInt64() + num3);
                }
                elemdesc = (System.Runtime.InteropServices.ComTypes.ELEMDESC) Marshal.PtrToStructure(ptr2, typeof(System.Runtime.InteropServices.ComTypes.ELEMDESC));
                string stringFromTypeDesc = GetStringFromTypeDesc(typeinfo, elemdesc.tdesc);
                if ((i == 0) && isPropertyPut)
                {
                    builder.Insert(0, stringFromTypeDesc + " ");
                }
                else
                {
                    builder.Append(stringFromTypeDesc);
                    if (i < (funcdesc.cParams - 1))
                    {
                        builder.Append(", ");
                    }
                }
            }
            builder.Append(")");
            return builder.ToString();
        }

        internal static ParameterModifier[] GetModifiers(ParameterInformation[] parameters)
        {
            int length = parameters.Length;
            if (parameters.Length == 0)
            {
                return null;
            }
            ParameterModifier modifier = new ParameterModifier(length);
            for (int i = 0; i < length; i++)
            {
                modifier[i] = parameters[i].isByRef;
            }
            return new ParameterModifier[] { modifier };
        }

        internal static string GetNameFromFuncDesc(ITypeInfo typeinfo, System.Runtime.InteropServices.ComTypes.FUNCDESC funcdesc)
        {
            string str;
            string str2;
            string str3;
            int num;
            typeinfo.GetDocumentation(funcdesc.memid, out str, out str2, out num, out str3);
            return str;
        }

        internal static ParameterInformation[] GetParameterInformation(System.Runtime.InteropServices.ComTypes.FUNCDESC funcdesc, bool skipLastParameter)
        {
            int cParams = funcdesc.cParams;
            if (skipLastParameter)
            {
                cParams--;
            }
            ParameterInformation[] informationArray = new ParameterInformation[cParams];
            IntPtr lprgelemdescParam = funcdesc.lprgelemdescParam;
            int num2 = Marshal.SizeOf(typeof(System.Runtime.InteropServices.ComTypes.ELEMDESC));
            for (int i = 0; i < cParams; i++)
            {
                IntPtr ptr2;
                bool isOptional = false;
                System.Runtime.InteropServices.ComTypes.ELEMDESC elemdesc = new System.Runtime.InteropServices.ComTypes.ELEMDESC();
                int num4 = i * num2;
                if (IntPtr.Size == 4)
                {
                    ptr2 = (IntPtr) (lprgelemdescParam.ToInt32() + num4);
                }
                else
                {
                    ptr2 = (IntPtr) (lprgelemdescParam.ToInt64() + num4);
                }
                elemdesc = (System.Runtime.InteropServices.ComTypes.ELEMDESC) Marshal.PtrToStructure(ptr2, typeof(System.Runtime.InteropServices.ComTypes.ELEMDESC));
                Type typeFromTypeDesc = GetTypeFromTypeDesc(elemdesc.tdesc);
                object defaultValue = null;
                if (((short) (elemdesc.desc.paramdesc.wParamFlags & (System.Runtime.InteropServices.ComTypes.PARAMFLAG.PARAMFLAG_NONE | System.Runtime.InteropServices.ComTypes.PARAMFLAG.PARAMFLAG_FOPT))) != 0)
                {
                    isOptional = true;
                    defaultValue = Type.Missing;
                }
                else
                {
                    isOptional = false;
                }
                bool isByRef = false;
                if (((short) (elemdesc.desc.paramdesc.wParamFlags & (System.Runtime.InteropServices.ComTypes.PARAMFLAG.PARAMFLAG_NONE | System.Runtime.InteropServices.ComTypes.PARAMFLAG.PARAMFLAG_FOUT))) != 0)
                {
                    isByRef = true;
                }
                informationArray[i] = new ParameterInformation(typeFromTypeDesc, isOptional, defaultValue, isByRef);
            }
            return informationArray;
        }

        private static string GetStringFromCustomType(ITypeInfo typeinfo, IntPtr refptr)
        {
            ITypeInfo info;
            int hRef = (int) ((long) refptr);
            typeinfo.GetRefTypeInfo(hRef, out info);
            if (info != null)
            {
                string str;
                string str2;
                string str3;
                int num2;
                info.GetDocumentation(-1, out str, out str2, out num2, out str3);
                return str;
            }
            return "UnknownCustomtype";
        }

        private static string GetStringFromTypeDesc(ITypeInfo typeinfo, System.Runtime.InteropServices.ComTypes.TYPEDESC typedesc)
        {
            if (typedesc.vt == 0x1a)
            {
                System.Runtime.InteropServices.ComTypes.TYPEDESC typedesc2 = (System.Runtime.InteropServices.ComTypes.TYPEDESC) Marshal.PtrToStructure(typedesc.lpValue, typeof(System.Runtime.InteropServices.ComTypes.TYPEDESC));
                return GetStringFromTypeDesc(typeinfo, typedesc2);
            }
            if (typedesc.vt == 0x1b)
            {
                System.Runtime.InteropServices.ComTypes.TYPEDESC typedesc3 = (System.Runtime.InteropServices.ComTypes.TYPEDESC) Marshal.PtrToStructure(typedesc.lpValue, typeof(System.Runtime.InteropServices.ComTypes.TYPEDESC));
                return ("SAFEARRAY(" + GetStringFromTypeDesc(typeinfo, typedesc3) + ")");
            }
            if (typedesc.vt == 0x1d)
            {
                return GetStringFromCustomType(typeinfo, typedesc.lpValue);
            }
            switch (((VarEnum) typedesc.vt))
            {
                case VarEnum.VT_EMPTY:
                    return "";

                case VarEnum.VT_I2:
                    return "short";

                case VarEnum.VT_I4:
                case VarEnum.VT_INT:
                case VarEnum.VT_HRESULT:
                    return "int";

                case VarEnum.VT_R4:
                    return "float";

                case VarEnum.VT_R8:
                    return "double";

                case VarEnum.VT_CY:
                    return "currency";

                case VarEnum.VT_DATE:
                    return "Date";

                case VarEnum.VT_BSTR:
                case VarEnum.VT_LPSTR:
                case VarEnum.VT_LPWSTR:
                    return "string";

                case VarEnum.VT_DISPATCH:
                    return "IDispatch";

                case VarEnum.VT_BOOL:
                    return "bool";

                case VarEnum.VT_VARIANT:
                    return "Variant";

                case VarEnum.VT_UNKNOWN:
                    return "IUnknown";

                case VarEnum.VT_DECIMAL:
                    return "decimal";

                case VarEnum.VT_I1:
                    return "char";

                case VarEnum.VT_UI1:
                    return "byte";

                case VarEnum.VT_UI2:
                    return "ushort";

                case VarEnum.VT_UI4:
                case VarEnum.VT_UINT:
                    return "int";

                case VarEnum.VT_I8:
                    return "int64";

                case VarEnum.VT_UI8:
                    return "int64";

                case VarEnum.VT_VOID:
                    return "void";

                case VarEnum.VT_CLSID:
                    return "clsid";

                case VarEnum.VT_ARRAY:
                    return "object[]";
            }
            return "Unknown!";
        }

        internal static Type GetTypeFromTypeDesc(System.Runtime.InteropServices.ComTypes.TYPEDESC typedesc)
        {
            switch (((VarEnum) typedesc.vt))
            {
                case VarEnum.VT_I2:
                    return typeof(short);

                case VarEnum.VT_I4:
                case VarEnum.VT_INT:
                case VarEnum.VT_HRESULT:
                    return typeof(int);

                case VarEnum.VT_R4:
                    return typeof(float);

                case VarEnum.VT_R8:
                    return typeof(double);

                case VarEnum.VT_CY:
                case VarEnum.VT_DECIMAL:
                    return typeof(decimal);

                case VarEnum.VT_DATE:
                    return typeof(DateTime);

                case VarEnum.VT_BSTR:
                case VarEnum.VT_LPSTR:
                case VarEnum.VT_LPWSTR:
                    return typeof(string);

                case VarEnum.VT_BOOL:
                    return typeof(bool);

                case VarEnum.VT_I1:
                    return typeof(sbyte);

                case VarEnum.VT_UI1:
                    return typeof(byte);

                case VarEnum.VT_UI2:
                    return typeof(ushort);

                case VarEnum.VT_UI4:
                case VarEnum.VT_UINT:
                    return typeof(int);

                case VarEnum.VT_I8:
                    return typeof(long);

                case VarEnum.VT_UI8:
                    return typeof(ulong);

                case VarEnum.VT_VOID:
                    return typeof(void);

                case VarEnum.VT_CLSID:
                    return typeof(Guid);

                case VarEnum.VT_ARRAY:
                    return typeof(object[]);
            }
            return typeof(object);
        }
    }
}

