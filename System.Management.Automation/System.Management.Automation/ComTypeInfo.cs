namespace System.Management.Automation
{
    using System;
    using System.Collections.Generic;
    using System.Management.Automation.Internal;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;

    internal class ComTypeInfo
    {
        private Guid guid = Guid.Empty;
        private Dictionary<string, ComMethod> methods;
        private Dictionary<string, ComProperty> properties;
        private ITypeInfo typeinfo;

        internal ComTypeInfo(ITypeInfo info)
        {
            this.typeinfo = info;
            this.properties = new Dictionary<string, ComProperty>(StringComparer.OrdinalIgnoreCase);
            this.methods = new Dictionary<string, ComMethod>(StringComparer.OrdinalIgnoreCase);
            if (this.typeinfo != null)
            {
                this.Initialize();
            }
        }

        private void AddMethod(string strName, int index)
        {
            ComMethod method = null;
            if (this.methods.ContainsKey(strName))
            {
                method = this.methods[strName];
            }
            else
            {
                method = new ComMethod(this.typeinfo, strName);
                this.methods[strName] = method;
            }
            if (method != null)
            {
                method.AddFuncDesc(index);
            }
        }

        private void AddProperty(string strName, System.Runtime.InteropServices.ComTypes.FUNCDESC funcdesc, int index)
        {
            ComProperty property = null;
            if (this.properties.ContainsKey(strName))
            {
                property = this.properties[strName];
            }
            else
            {
                property = new ComProperty(this.typeinfo, strName);
                this.properties[strName] = property;
            }
            if (property != null)
            {
                property.UpdateFuncDesc(funcdesc, index);
            }
        }

        internal static ComTypeInfo GetDispatchTypeInfo(object comObject)
        {
            ComTypeInfo info = null;
            System.Management.Automation.IDispatch dispatch = comObject as System.Management.Automation.IDispatch;
            if (dispatch == null)
            {
                return info;
            }
            ITypeInfo ppTInfo = null;
            dispatch.GetTypeInfo(0, 0, out ppTInfo);
            if (ppTInfo == null)
            {
                return info;
            }
            System.Runtime.InteropServices.ComTypes.TYPEATTR typeAttr = GetTypeAttr(ppTInfo);
            if (typeAttr.typekind == System.Runtime.InteropServices.ComTypes.TYPEKIND.TKIND_INTERFACE)
            {
                ppTInfo = GetDispatchTypeInfoFromCustomInterfaceTypeInfo(ppTInfo);
            }
            if (typeAttr.typekind == System.Runtime.InteropServices.ComTypes.TYPEKIND.TKIND_COCLASS)
            {
                ppTInfo = GetDispatchTypeInfoFromCoClassTypeInfo(ppTInfo);
            }
            return new ComTypeInfo(ppTInfo);
        }

        internal static ITypeInfo GetDispatchTypeInfoFromCoClassTypeInfo(ITypeInfo typeinfo)
        {
            int cImplTypes = GetTypeAttr(typeinfo).cImplTypes;
            ITypeInfo ppTI = null;
            for (int i = 0; i < cImplTypes; i++)
            {
                int num2;
                typeinfo.GetRefTypeOfImplType(i, out num2);
                typeinfo.GetRefTypeInfo(num2, out ppTI);
                System.Runtime.InteropServices.ComTypes.TYPEATTR typeAttr = GetTypeAttr(ppTI);
                if (typeAttr.typekind == System.Runtime.InteropServices.ComTypes.TYPEKIND.TKIND_DISPATCH)
                {
                    return ppTI;
                }
                if (((short) (typeAttr.wTypeFlags & System.Runtime.InteropServices.ComTypes.TYPEFLAGS.TYPEFLAG_FDUAL)) != 0)
                {
                    ppTI = GetDispatchTypeInfoFromCustomInterfaceTypeInfo(ppTI);
                    if (GetTypeAttr(ppTI).typekind == System.Runtime.InteropServices.ComTypes.TYPEKIND.TKIND_DISPATCH)
                    {
                        return ppTI;
                    }
                }
            }
            return null;
        }

        internal static ITypeInfo GetDispatchTypeInfoFromCustomInterfaceTypeInfo(ITypeInfo typeinfo)
        {
            ITypeInfo ppTI = null;
            try
            {
                int num;
                typeinfo.GetRefTypeOfImplType(-1, out num);
                typeinfo.GetRefTypeInfo(num, out ppTI);
            }
            catch (COMException exception)
            {
                if (exception.ErrorCode != -2147319765)
                {
                    throw;
                }
            }
            return ppTI;
        }

        [ArchitectureSensitive]
        internal static System.Runtime.InteropServices.ComTypes.FUNCDESC GetFuncDesc(ITypeInfo typeinfo, int index)
        {
            IntPtr ptr;
            typeinfo.GetFuncDesc(index, out ptr);
            System.Runtime.InteropServices.ComTypes.FUNCDESC funcdesc = (System.Runtime.InteropServices.ComTypes.FUNCDESC) Marshal.PtrToStructure(ptr, typeof(System.Runtime.InteropServices.ComTypes.FUNCDESC));
            typeinfo.ReleaseFuncDesc(ptr);
            return funcdesc;
        }

        [ArchitectureSensitive]
        internal static System.Runtime.InteropServices.ComTypes.TYPEATTR GetTypeAttr(ITypeInfo typeinfo)
        {
            IntPtr ptr;
            typeinfo.GetTypeAttr(out ptr);
            System.Runtime.InteropServices.ComTypes.TYPEATTR typeattr = (System.Runtime.InteropServices.ComTypes.TYPEATTR) Marshal.PtrToStructure(ptr, typeof(System.Runtime.InteropServices.ComTypes.TYPEATTR));
            typeinfo.ReleaseTypeAttr(ptr);
            return typeattr;
        }

        private void Initialize()
        {
            if (this.typeinfo != null)
            {
                System.Runtime.InteropServices.ComTypes.TYPEATTR typeAttr = GetTypeAttr(this.typeinfo);
                this.guid = typeAttr.guid;
                for (int i = 0; i < typeAttr.cFuncs; i++)
                {
                    string nameFromFuncDesc;
                    System.Runtime.InteropServices.ComTypes.FUNCDESC funcDesc = GetFuncDesc(this.typeinfo, i);
                    if ((funcDesc.wFuncFlags & 1) != 1)
                    {
                        nameFromFuncDesc = ComUtil.GetNameFromFuncDesc(this.typeinfo, funcDesc);
                        switch (funcDesc.invkind)
                        {
                            case System.Runtime.InteropServices.ComTypes.INVOKEKIND.INVOKE_FUNC:
                                this.AddMethod(nameFromFuncDesc, i);
                                break;

                            case System.Runtime.InteropServices.ComTypes.INVOKEKIND.INVOKE_PROPERTYGET:
                            case System.Runtime.InteropServices.ComTypes.INVOKEKIND.INVOKE_PROPERTYPUT:
                            case System.Runtime.InteropServices.ComTypes.INVOKEKIND.INVOKE_PROPERTYPUTREF:
                                goto Label_0075;
                        }
                    }
                    continue;
                Label_0075:
                    this.AddProperty(nameFromFuncDesc, funcDesc, i);
                }
            }
        }

        public string Clsid
        {
            get
            {
                return this.guid.ToString();
            }
        }

        public Dictionary<string, ComMethod> Methods
        {
            get
            {
                return this.methods;
            }
        }

        public Dictionary<string, ComProperty> Properties
        {
            get
            {
                return this.properties;
            }
        }
    }
}

