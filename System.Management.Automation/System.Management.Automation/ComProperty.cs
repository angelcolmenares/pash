namespace System.Management.Automation
{
    using System;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;
    using System.Text;

    internal class ComProperty
    {
        private System.Type cachedType;
        private int getterIndex;
        private bool hasGetter;
        private bool hasSetter;
        private bool hasSetterByRef;
        private bool isparameterizied;
        private string name;
        private int setterByRefIndex;
        private int setterIndex;
        private ITypeInfo typeInfo;

        internal ComProperty(ITypeInfo typeinfo, string name)
        {
            this.typeInfo = typeinfo;
            this.name = name;
        }

        internal string GetDefinition()
        {
            string str;
            IntPtr zero = IntPtr.Zero;
            try
            {
                this.typeInfo.GetFuncDesc(this.GetFuncDescIndex(), out zero);
                System.Runtime.InteropServices.ComTypes.FUNCDESC funcdesc = (System.Runtime.InteropServices.ComTypes.FUNCDESC) Marshal.PtrToStructure(zero, typeof(System.Runtime.InteropServices.ComTypes.FUNCDESC));
                str = ComUtil.GetMethodSignatureFromFuncDesc(this.typeInfo, funcdesc, !this.hasGetter);
            }
            finally
            {
                if (zero != IntPtr.Zero)
                {
                    this.typeInfo.ReleaseFuncDesc(zero);
                }
            }
            return str;
        }

        private int GetFuncDescIndex()
        {
            if (this.hasGetter)
            {
                return this.getterIndex;
            }
            if (this.hasSetter)
            {
                return this.setterIndex;
            }
            return this.setterByRefIndex;
        }

        internal object GetValue(object target)
        {
            System.Type type = target.GetType();
            try
            {
                return type.InvokeMember(this.name, BindingFlags.GetProperty | BindingFlags.IgnoreCase, null, target, null, CultureInfo.CurrentCulture);
            }
            catch (TargetInvocationException exception)
            {
                CommandProcessorBase.CheckForSevereException(exception.InnerException);
                COMException innerException = exception.InnerException as COMException;
                if ((innerException == null) || (innerException.ErrorCode != -2147352573))
                {
                    throw;
                }
            }
            catch (COMException exception3)
            {
                if (exception3.ErrorCode != -2147352570)
                {
                    throw;
                }
            }
            return null;
        }

        internal object GetValue(object target, object[] arguments)
        {
            System.Type type = target.GetType();
            try
            {
                object[] objArray;
                Collection<int> methods = new Collection<int> {
                    this.getterIndex
                };
                MethodInformation[] informationArray = ComUtil.GetMethodInformationArray(this.typeInfo, methods, false);
                MethodInformation methodInformation = Adapter.GetBestMethodAndArguments(this.Name, informationArray, arguments, out objArray);
                object obj2 = type.InvokeMember(this.name, BindingFlags.GetProperty | BindingFlags.IgnoreCase, null, target, objArray, ComUtil.GetModifiers(methodInformation.parameters), CultureInfo.CurrentCulture, null);
                Adapter.SetReferences(objArray, methodInformation, arguments);
                return obj2;
            }
            catch (TargetInvocationException exception)
            {
                CommandProcessorBase.CheckForSevereException(exception.InnerException);
                COMException innerException = exception.InnerException as COMException;
                if ((innerException == null) || (innerException.ErrorCode != -2147352573))
                {
                    throw;
                }
            }
            catch (COMException exception3)
            {
                if (exception3.ErrorCode != -2147352570)
                {
                    throw;
                }
            }
            return null;
        }

        internal void SetValue(object target, object setValue)
        {
            System.Type type = target.GetType();
            object[] args = new object[1];
            setValue = Adapter.PropertySetAndMethodArgumentConvertTo(setValue, this.Type, CultureInfo.InvariantCulture);
            args[0] = setValue;
            try
            {
                type.InvokeMember(this.name, BindingFlags.SetProperty | BindingFlags.IgnoreCase, null, target, args, CultureInfo.CurrentCulture);
            }
            catch (TargetInvocationException exception)
            {
                CommandProcessorBase.CheckForSevereException(exception.InnerException);
                COMException innerException = exception.InnerException as COMException;
                if ((innerException == null) || (innerException.ErrorCode != -2147352573))
                {
                    throw;
                }
            }
            catch (COMException exception3)
            {
                if (exception3.ErrorCode != -2147352570)
                {
                    throw;
                }
            }
        }

        internal void SetValue(object target, object setValue, object[] arguments)
        {
            object[] objArray;
            Collection<int> methods = new Collection<int> {
                (this.hasSetterByRef != null) ? this.setterByRefIndex : this.setterIndex
            };
            MethodInformation[] informationArray = ComUtil.GetMethodInformationArray(this.typeInfo, methods, true);
            MethodInformation methodInformation = Adapter.GetBestMethodAndArguments(this.Name, informationArray, arguments, out objArray);
            System.Type type = target.GetType();
            object[] args = new object[objArray.Length + 1];
            for (int i = 0; i < objArray.Length; i++)
            {
                args[i] = objArray[i];
            }
            args[objArray.Length] = Adapter.PropertySetAndMethodArgumentConvertTo(setValue, this.Type, CultureInfo.InvariantCulture);
            try
            {
                type.InvokeMember(this.name, BindingFlags.SetProperty | BindingFlags.IgnoreCase, null, target, args, ComUtil.GetModifiers(methodInformation.parameters), CultureInfo.CurrentCulture, null);
                Adapter.SetReferences(args, methodInformation, arguments);
            }
            catch (TargetInvocationException exception)
            {
                CommandProcessorBase.CheckForSevereException(exception.InnerException);
                COMException innerException = exception.InnerException as COMException;
                if ((innerException == null) || (innerException.ErrorCode != -2147352573))
                {
                    throw;
                }
            }
            catch (COMException exception3)
            {
                if (exception3.ErrorCode != -2147352570)
                {
                    throw;
                }
            }
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(this.GetDefinition());
            builder.Append(" ");
            if (this.hasGetter)
            {
                builder.Append("{get} ");
            }
            if (this.hasSetter)
            {
                builder.Append("{set} ");
            }
            if (this.hasSetterByRef)
            {
                builder.Append("{set by ref}");
            }
            return builder.ToString();
        }

        internal void UpdateFuncDesc(System.Runtime.InteropServices.ComTypes.FUNCDESC desc, int index)
        {
            switch (desc.invkind)
            {
                case System.Runtime.InteropServices.ComTypes.INVOKEKIND.INVOKE_PROPERTYGET:
                    this.hasGetter = true;
                    this.getterIndex = index;
                    if (desc.cParams <= 0)
                    {
                        break;
                    }
                    this.isparameterizied = true;
                    return;

                case (System.Runtime.InteropServices.ComTypes.INVOKEKIND.INVOKE_PROPERTYGET | System.Runtime.InteropServices.ComTypes.INVOKEKIND.INVOKE_FUNC):
                    break;

                case System.Runtime.InteropServices.ComTypes.INVOKEKIND.INVOKE_PROPERTYPUT:
                    this.hasSetter = true;
                    this.setterIndex = index;
                    if (desc.cParams <= 1)
                    {
                        break;
                    }
                    this.isparameterizied = true;
                    return;

                case System.Runtime.InteropServices.ComTypes.INVOKEKIND.INVOKE_PROPERTYPUTREF:
                    this.setterByRefIndex = index;
                    this.hasSetterByRef = true;
                    if (desc.cParams > 1)
                    {
                        this.isparameterizied = true;
                    }
                    break;

                default:
                    return;
            }
        }

        internal bool IsGettable
        {
            get
            {
                return this.hasGetter;
            }
        }

        internal bool IsParameterized
        {
            get
            {
                return this.isparameterizied;
            }
        }

        internal bool IsSettable
        {
            get
            {
                return (this.hasSetter | this.hasSetterByRef);
            }
        }

        internal string Name
        {
            get
            {
                return this.name;
            }
        }

        internal int ParamCount
        {
            get
            {
                return 0;
            }
        }

        internal System.Type Type
        {
            get
            {
                this.cachedType = null;
                if (this.cachedType == null)
                {
                    IntPtr zero = IntPtr.Zero;
                    try
                    {
                        this.typeInfo.GetFuncDesc(this.GetFuncDescIndex(), out zero);
                        System.Runtime.InteropServices.ComTypes.FUNCDESC funcdesc = (System.Runtime.InteropServices.ComTypes.FUNCDESC) Marshal.PtrToStructure(zero, typeof(System.Runtime.InteropServices.ComTypes.FUNCDESC));
                        if (this.hasGetter)
                        {
                            this.cachedType = ComUtil.GetTypeFromTypeDesc(funcdesc.elemdescFunc.tdesc);
                        }
                        else
                        {
                            ParameterInformation[] parameterInformation = ComUtil.GetParameterInformation(funcdesc, false);
                            this.cachedType = parameterInformation[0].parameterType;
                        }
                    }
                    finally
                    {
                        if (zero != IntPtr.Zero)
                        {
                            this.typeInfo.ReleaseFuncDesc(zero);
                        }
                    }
                }
                return this.cachedType;
            }
        }
    }
}

