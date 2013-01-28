namespace System.Management.Automation
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Management.Automation.Language;

    internal static class TypeOps
    {
        internal static object AsOperator(object left, Type type)
        {
            bool flag;
            if (type == null)
            {
                throw InterpreterError.NewInterpreterException(null, typeof(RuntimeException), null, "AsOperatorRequiresType", ParserStrings.AsOperatorRequiresType, new object[0]);
            }
            LanguagePrimitives.ConversionData data = LanguagePrimitives.FigureConversion(left, type, out flag);
            if (data.Rank == ConversionRank.None)
            {
                return null;
            }
            try
            {
                if (flag)
                {
                    return data.Invoke(PSObject.Base(left), type, false, (PSObject) left, NumberFormatInfo.InvariantInfo, null);
                }
                return data.Invoke(left, type, false, null, NumberFormatInfo.InvariantInfo, null);
            }
            catch (PSInvalidCastException)
            {
                return null;
            }
        }

        internal static bool IsInstance(object left, object right)
        {
            object o = PSObject.Base(left);
            object obj3 = PSObject.Base(right);
            Type type = obj3 as Type;
            if (type == null)
            {
                type = ParserOps.ConvertTo<Type>(obj3, null);
                if (type == null)
                {
                    throw InterpreterError.NewInterpreterException(obj3, typeof(RuntimeException), null, "IsOperatorRequiresType", ParserStrings.IsOperatorRequiresType, new object[0]);
                }
            }
            return (((type == typeof(PSCustomObject)) && (o is PSObject)) || ((type.Equals(typeof(PSObject)) && (left is PSObject)) || type.IsInstanceOfType(o)));
        }

        internal static Type ResolveTypeName(ITypeName typeName)
        {
            Type reflectionType = typeName.GetReflectionType();
            if (reflectionType != null)
            {
                return reflectionType;
            }
            GenericTypeName name = typeName as GenericTypeName;
            if (name != null)
            {
                Type genericType = name.GetGenericType(ResolveTypeName(name.TypeName));
                Type[] typeArguments = (from arg in name.GenericArguments select ResolveTypeName(arg)).ToArray<Type>();
                try
                {
                    if ((genericType != null) && genericType.ContainsGenericParameters)
                    {
                        genericType.MakeGenericType(typeArguments);
                    }
                }
                catch (Exception exception)
                {
                    CommandProcessorBase.CheckForSevereException(exception);
                    throw InterpreterError.NewInterpreterException(typeName, typeof(RuntimeException), null, "TypeNotFoundWithMessage", ParserStrings.TypeNotFoundWithMessage, new object[] { typeName.FullName, exception.Message });
                }
            }
            ArrayTypeName name2 = typeName as ArrayTypeName;
            if (name2 != null)
            {
                ResolveTypeName(name2.ElementType);
            }
            throw InterpreterError.NewInterpreterException(typeName, typeof(RuntimeException), null, "TypeNotFound", ParserStrings.TypeNotFound, new object[] { typeName.FullName });
        }
    }
}

