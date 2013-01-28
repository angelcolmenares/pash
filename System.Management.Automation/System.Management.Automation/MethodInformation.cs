namespace System.Management.Automation
{
    using System;
    using System.Diagnostics;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;

    [DebuggerDisplay("MethodInformation: {methodDefinition}")]
    internal class MethodInformation
    {
        private string _cachedMethodDefinition;
        private static OpCode[] _ldc = new OpCode[] { OpCodes.Ldc_I4_0, OpCodes.Ldc_I4_1, OpCodes.Ldc_I4_2, OpCodes.Ldc_I4_3, OpCodes.Ldc_I4_4, OpCodes.Ldc_I4_5, OpCodes.Ldc_I4_6, OpCodes.Ldc_I4_7, OpCodes.Ldc_I4_8 };
        internal bool hasOptional;
        internal bool hasVarArgs;
        internal bool isGeneric;
        internal MethodBase method;
        private MethodInvoker methodInvoker;
        internal ParameterInformation[] parameters;
        private bool useReflection;

        internal MethodInformation(MethodBase method, int parametersToIgnore)
        {
            this.method = method;
            this.isGeneric = method.IsGenericMethod;
            ParameterInfo[] parameters = method.GetParameters();
            int num = parameters.Length - parametersToIgnore;
            this.parameters = new ParameterInformation[num];
            for (int i = 0; i < num; i++)
            {
                this.parameters[i] = new ParameterInformation(parameters[i]);
                if (parameters[i].IsOptional)
                {
                    this.hasOptional = true;
                }
            }
            this.hasVarArgs = false;
            if (num > 0)
            {
                ParameterInfo info = parameters[num - 1];
                if ((!this.hasOptional && info.ParameterType.IsArray) && (info.GetCustomAttributes(typeof(ParamArrayAttribute), false).Length != 0))
                {
                    this.hasVarArgs = true;
                    this.parameters[num - 1].isParamArray = true;
                }
            }
        }

        internal MethodInformation(bool hasvarargs, bool hasoptional, ParameterInformation[] arguments)
        {
            this.hasVarArgs = hasvarargs;
            this.hasOptional = hasoptional;
            this.parameters = arguments;
        }

        private static bool CompareMethodParameters(MethodInfo method1, MethodInfo method2)
        {
            ParameterInfo[] parameters = method1.GetParameters();
            ParameterInfo[] infoArray2 = method2.GetParameters();
            if (parameters.Length != infoArray2.Length)
            {
                return false;
            }
            for (int i = 0; i < parameters.Length; i++)
            {
                if (!parameters[i].ParameterType.Equals(infoArray2[i].ParameterType))
                {
                    return false;
                }
            }
            return true;
        }

        private static void EmitLdc(ILGenerator emitter, int c)
        {
            if (c < _ldc.Length)
            {
                emitter.Emit(_ldc[c]);
            }
            else
            {
                emitter.Emit(OpCodes.Ldc_I4, c);
            }
        }

        private static Type FindInterfaceForMethod(MethodInfo method, out MethodInfo methodToCall)
        {
            methodToCall = null;
            foreach (Type type2 in method.DeclaringType.GetInterfaces())
            {
                MethodInfo info = type2.GetMethod(method.Name, BindingFlags.Instance);
                if ((info != null) && CompareMethodParameters(info, method))
                {
                    methodToCall = info;
                    return type2;
                }
            }
            return null;
        }

        private MethodInvoker GetMethodInvoker(MethodInfo method)
        {
            Type declaringType;
            int num2;
            bool flag = false;
            bool flag2 = false;
            bool flag3 = false;
            MethodInfo methodToCall = method;
            int index = 0;
            DynamicMethod method2 = new DynamicMethod(method.Name, typeof(object), new Type[] { typeof(object), typeof(object[]) }, typeof(Adapter).Module, true);
            ILGenerator iLGenerator = method2.GetILGenerator();
            ParameterInfo[] parameters = method.GetParameters();
            int num3 = 0;
            if ((!method.IsStatic && method.DeclaringType.IsValueType) && !method.IsVirtual)
            {
                flag = true;
                num3++;
            }
            foreach (ParameterInfo info2 in parameters)
            {
                if (info2.IsOut || info2.ParameterType.IsByRef)
                {
                    flag2 = true;
                    num3++;
                }
            }
            LocalBuilder[] builderArray = null;
            Type returnType = method.ReturnType;
            if (num3 > 0)
            {
                if (flag2 && (returnType != typeof(void)))
                {
                    num3++;
                    flag3 = true;
                }
                builderArray = new LocalBuilder[num3];
                index = 0;
                if (flag)
                {
                    declaringType = method.DeclaringType;
                    builderArray[index] = iLGenerator.DeclareLocal(declaringType);
                    iLGenerator.Emit(OpCodes.Ldarg_0);
                    iLGenerator.Emit(OpCodes.Unbox_Any, declaringType);
                    iLGenerator.Emit(OpCodes.Stloc, builderArray[index]);
                    index++;
                }
                for (num2 = 0; num2 < parameters.Length; num2++)
                {
                    declaringType = parameters[num2].ParameterType;
                    if (parameters[num2].IsOut || declaringType.IsByRef)
                    {
                        if (declaringType.IsByRef)
                        {
                            declaringType = declaringType.GetElementType();
                        }
                        builderArray[index] = iLGenerator.DeclareLocal(declaringType);
                        iLGenerator.Emit(OpCodes.Ldarg_1);
                        EmitLdc(iLGenerator, num2);
                        iLGenerator.Emit(OpCodes.Ldelem_Ref);
                        if (declaringType.IsValueType)
                        {
                            iLGenerator.Emit(OpCodes.Unbox_Any, declaringType);
                        }
                        else if (declaringType != typeof(object))
                        {
                            iLGenerator.Emit(OpCodes.Castclass, declaringType);
                        }
                        iLGenerator.Emit(OpCodes.Stloc, builderArray[index]);
                        index++;
                    }
                }
                if (flag3)
                {
                    builderArray[index] = iLGenerator.DeclareLocal(returnType);
                }
            }
            index = 0;
            if (!method.IsStatic)
            {
                if (method.DeclaringType.IsValueType)
                {
                    if (method.IsVirtual)
                    {
                        declaringType = FindInterfaceForMethod(method, out methodToCall);
                        if (declaringType == null)
                        {
                            this.useReflection = true;
                            return null;
                        }
                        iLGenerator.Emit(OpCodes.Ldarg_0);
                        iLGenerator.Emit(OpCodes.Castclass, declaringType);
                    }
                    else
                    {
                        iLGenerator.Emit(OpCodes.Ldloca, builderArray[index]);
                        index++;
                    }
                }
                else
                {
                    iLGenerator.Emit(OpCodes.Ldarg_0);
                }
            }
            num2 = 0;
            while (num2 < parameters.Length)
            {
                declaringType = parameters[num2].ParameterType;
                if (declaringType.IsByRef)
                {
                    iLGenerator.Emit(OpCodes.Ldloca, builderArray[index]);
                    index++;
                }
                else if (parameters[num2].IsOut)
                {
                    iLGenerator.Emit(OpCodes.Ldloc, builderArray[index]);
                    index++;
                }
                else
                {
                    iLGenerator.Emit(OpCodes.Ldarg_1);
                    EmitLdc(iLGenerator, num2);
                    iLGenerator.Emit(OpCodes.Ldelem_Ref);
                    if (declaringType.IsValueType)
                    {
                        iLGenerator.Emit(OpCodes.Unbox_Any, declaringType);
                    }
                    else if (declaringType != typeof(object))
                    {
                        iLGenerator.Emit(OpCodes.Castclass, declaringType);
                    }
                }
                num2++;
            }
            iLGenerator.Emit(method.IsStatic ? OpCodes.Call : OpCodes.Callvirt, methodToCall);
            if (flag3)
            {
                iLGenerator.Emit(OpCodes.Stloc, builderArray[builderArray.Length - 1]);
            }
            if (flag2)
            {
                index = flag ? 1 : 0;
                for (num2 = 0; num2 < parameters.Length; num2++)
                {
                    declaringType = parameters[num2].ParameterType;
                    if (parameters[num2].IsOut || declaringType.IsByRef)
                    {
                        if (declaringType.IsByRef)
                        {
                            declaringType = declaringType.GetElementType();
                        }
                        iLGenerator.Emit(OpCodes.Ldarg_1);
                        EmitLdc(iLGenerator, num2);
                        iLGenerator.Emit(OpCodes.Ldloc, builderArray[index]);
                        if (declaringType.IsValueType)
                        {
                            iLGenerator.Emit(OpCodes.Box, declaringType);
                        }
                        iLGenerator.Emit(OpCodes.Stelem_Ref);
                        index++;
                    }
                }
            }
            if (returnType == typeof(void))
            {
                iLGenerator.Emit(OpCodes.Ldnull);
            }
            else
            {
                if (flag3)
                {
                    iLGenerator.Emit(OpCodes.Ldloc, builderArray[builderArray.Length - 1]);
                }
                Adapter.DoBoxingIfNecessary(iLGenerator, returnType);
            }
            iLGenerator.Emit(OpCodes.Ret);
            return (MethodInvoker) method2.CreateDelegate(typeof(MethodInvoker));
        }

        internal object Invoke(object target, object[] arguments)
        {
            if ((target is PSObject) && !this.method.DeclaringType.IsAssignableFrom(target.GetType()))
            {
                target = PSObject.Base(target);
            }
            if (!this.useReflection)
            {
                if (this.methodInvoker == null)
                {
                    if (!(this.method is MethodInfo))
                    {
                        this.useReflection = true;
                    }
                    else
                    {
                        this.methodInvoker = this.GetMethodInvoker(this.method as MethodInfo);
                    }
                }
                if (this.methodInvoker != null)
                {
                    return this.methodInvoker(target, arguments);
                }
            }
            return this.method.Invoke(target, arguments);
        }

        internal string methodDefinition
        {
            get
            {
                if (this._cachedMethodDefinition == null)
                {
                    string str = DotNetAdapter.GetMethodInfoOverloadDefinition(this.method.Name, this.method, this.method.GetParameters().Length - this.parameters.Length);
                    Interlocked.CompareExchange<string>(ref this._cachedMethodDefinition, str, null);
                }
                return this._cachedMethodDefinition;
            }
        }

        private delegate object MethodInvoker(object target, object[] arguments);
    }
}

