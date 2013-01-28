namespace System.Management.Automation
{
    using System;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Management.Automation.Internal;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;

    internal class ComMethod
    {
        private Collection<int> methods = new Collection<int>();
        private string name;
        private ITypeInfo typeInfo;

        internal ComMethod(ITypeInfo typeinfo, string name)
        {
            this.typeInfo = typeinfo;
            this.name = name;
        }

        internal void AddFuncDesc(int index)
        {
            this.methods.Add(index);
        }

        internal object InvokeMethod(PSMethod method, object[] arguments)
        {
            Type type = method.baseObject.GetType();
            BindingFlags invokeAttr = BindingFlags.InvokeMethod | BindingFlags.IgnoreCase;
            try
            {
                object[] objArray;
                ComMethodInformation[] informationArray = ComUtil.GetMethodInformationArray(this.typeInfo, this.methods, false);
                ComMethodInformation methodInformation = (ComMethodInformation) Adapter.GetBestMethodAndArguments(this.Name, (MethodInformation[]) informationArray, arguments, out objArray);
                object obj2 = type.InvokeMember(this.Name, invokeAttr, null, method.baseObject, objArray, ComUtil.GetModifiers(methodInformation.parameters), CultureInfo.CurrentCulture, null);
                Adapter.SetReferences(objArray, methodInformation, arguments);
                if (methodInformation.ReturnType != typeof(void))
                {
                    return obj2;
                }
                return AutomationNull.Value;
            }
            catch (TargetInvocationException exception)
            {
                CommandProcessorBase.CheckForSevereException(exception.InnerException);
                COMException innerException = exception.InnerException as COMException;
                if ((innerException == null) || (innerException.ErrorCode != -2147352573))
                {
                    string str = (exception.InnerException == null) ? exception.Message : exception.InnerException.Message;
                    throw new MethodInvocationException("ComMethodTargetInvocation", exception, ExtendedTypeSystem.MethodInvocationException, new object[] { method.Name, arguments.Length, str });
                }
            }
            catch (COMException exception3)
            {
                if (exception3.ErrorCode != -2147352570)
                {
                    throw new MethodInvocationException("ComMethodCOMException", exception3, ExtendedTypeSystem.MethodInvocationException, new object[] { method.Name, arguments.Length, exception3.Message });
                }
            }
            return null;
        }

        internal Collection<string> MethodDefinitions()
        {
            Collection<string> collection = new Collection<string>();
            foreach (int num in this.methods)
            {
                IntPtr ptr;
                this.typeInfo.GetFuncDesc(num, out ptr);
                System.Runtime.InteropServices.ComTypes.FUNCDESC funcdesc = (System.Runtime.InteropServices.ComTypes.FUNCDESC) Marshal.PtrToStructure(ptr, typeof(System.Runtime.InteropServices.ComTypes.FUNCDESC));
                string item = ComUtil.GetMethodSignatureFromFuncDesc(this.typeInfo, funcdesc, false);
                collection.Add(item);
                this.typeInfo.ReleaseFuncDesc(ptr);
            }
            return collection;
        }

        internal string Name
        {
            get
            {
                return this.name;
            }
        }
    }
}

