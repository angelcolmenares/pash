namespace System.Data.Services.Client
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Security;
    using System.Security.Permissions;

    internal class DynamicProxyMethodGenerator
    {
        private static Dictionary<MethodBase, MethodInfo> dynamicProxyMethods = new Dictionary<MethodBase, MethodInfo>(EqualityComparer<MethodBase>.Default);

        [SecurityCritical, PermissionSet(SecurityAction.Assert, Unrestricted=true)]
        private static DynamicMethod CreateDynamicMethod(string name, Type returnType, Type[] parameterTypes)
        {
            return new DynamicMethod(name, returnType, parameterTypes, typeof(DynamicProxyMethodGenerator).Module, true);
        }

        internal Expression GetCallWrapper(MethodBase method, params Expression[] arguments)
        {
            if (!this.ThisAssemblyCanCreateHostedDynamicMethodsWithSkipVisibility())
            {
                return WrapOriginalMethodWithExpression(method, arguments);
            }
            return GetDynamicMethodCallWrapper(method, arguments);
        }

        [SecuritySafeCritical]
        private static Expression GetDynamicMethodCallWrapper(MethodBase method, params Expression[] arguments)
        {
            if ((method.DeclaringType == null) || (method.DeclaringType.Assembly != typeof(DynamicProxyMethodGenerator).Assembly))
            {
                return WrapOriginalMethodWithExpression(method, arguments);
            }
            string name = "_dynamic_" + method.ReflectedType.Name + "_" + method.Name;
            MethodInfo info = null;
            lock (dynamicProxyMethods)
            {
                dynamicProxyMethods.TryGetValue(method, out info);
            }
            if (info != null)
            {
                return Expression.Call(info, arguments);
            }
            Type[] parameterTypes = (from p in method.GetParameters() select p.ParameterType).ToArray<Type>();
            MethodInfo methodInfo = method as MethodInfo;
            DynamicMethod method2 = CreateDynamicMethod(name, (methodInfo == null) ? method.ReflectedType : methodInfo.ReturnType, parameterTypes);
            ILGenerator iLGenerator = method2.GetILGenerator();
            for (int i = 0; i < parameterTypes.Length; i++)
            {
                switch (i)
                {
                    case 0:
                        iLGenerator.Emit(OpCodes.Ldarg_0);
                        break;

                    case 1:
                        iLGenerator.Emit(OpCodes.Ldarg_1);
                        break;

                    case 2:
                        iLGenerator.Emit(OpCodes.Ldarg_2);
                        break;

                    case 3:
                        iLGenerator.Emit(OpCodes.Ldarg_3);
                        break;

                    default:
                        iLGenerator.Emit(OpCodes.Ldarg, i);
                        break;
                }
            }
            if (methodInfo == null)
            {
                iLGenerator.Emit(OpCodes.Newobj, (ConstructorInfo) method);
            }
            else
            {
                iLGenerator.EmitCall(OpCodes.Call, methodInfo, null);
            }
            iLGenerator.Emit(OpCodes.Ret);
            lock (dynamicProxyMethods)
            {
                if (!dynamicProxyMethods.ContainsKey(method))
                {
                    dynamicProxyMethods.Add(method, method2);
                }
            }
            return Expression.Call(method2, arguments);
        }

        protected virtual bool ThisAssemblyCanCreateHostedDynamicMethodsWithSkipVisibility()
        {
            return typeof(DynamicProxyMethodGenerator).Assembly.IsFullyTrusted;
        }

        private static Expression WrapOriginalMethodWithExpression(MethodBase method, Expression[] arguments)
        {
            MethodInfo info = method as MethodInfo;
            if (info != null)
            {
                return Expression.Call(info, arguments);
            }
            return Expression.New((ConstructorInfo) method, arguments);
        }
    }
}

