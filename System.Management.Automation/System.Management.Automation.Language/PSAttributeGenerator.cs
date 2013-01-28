namespace System.Management.Automation.Language
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Management.Automation;
    using System.Reflection;

    internal class PSAttributeGenerator : CreateInstanceBinder
    {
        private static readonly Dictionary<CallInfo, PSAttributeGenerator> _binderCache = new Dictionary<CallInfo, PSAttributeGenerator>();

        private PSAttributeGenerator(CallInfo callInfo) : base(callInfo)
        {
        }

        public override DynamicMetaObject FallbackCreateInstance(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion)
        {
            bool flag;
            Type type = (Type) target.Value;
            MethodInformation[] methodInformationArray = DotNetAdapter.GetMethodInformationArray(type.GetConstructors());
            target = new DynamicMetaObject(target.Expression, BindingRestrictions.GetInstanceRestriction(target.Expression, target.Value), target.Value);
            string errorId = null;
            string errorMsg = null;
            int count = base.CallInfo.ArgumentCount - base.CallInfo.ArgumentNames.Count;
            MethodInformation information = Adapter.FindBestMethod(methodInformationArray, null, (from arg in args.Take<DynamicMetaObject>(count) select arg.Value).ToArray<object>(), ref errorId, ref errorMsg, out flag);
            if (information == null)
            {
                if (errorSuggestion == null)
                {
                }
                return new DynamicMetaObject(Expression.Throw(Expression.New(CachedReflectionInfo.MethodException_ctor, new Expression[] { Expression.Constant(errorId), Expression.Constant(null, typeof(Exception)), Expression.Constant(errorMsg), Expression.NewArrayInit(typeof(object), new Expression[] { Expression.Constant(".ctor").Cast(typeof(object)), ExpressionCache.Constant(count).Cast(typeof(object)) }) }), this.ReturnType), target.CombineRestrictions(args));
            }
            ConstructorInfo method = (ConstructorInfo) information.method;
            ParameterInfo[] parameters = method.GetParameters();
            Expression[] arguments = new Expression[parameters.Length];
            int index = 0;
            while (index < parameters.Length)
            {
                bool flag3;
                Type parameterType = parameters[index].ParameterType;
                if (parameters[index].GetCustomAttributes(typeof(ParamArrayAttribute), true).Any<object>() && flag)
                {
                    Type elementType = parameters[index].ParameterType.GetElementType();
                    List<Expression> initializers = new List<Expression>();
                    int num3 = index;
                    int num4 = index;
                    while (num4 < count)
                    {
                        bool flag2;
                        LanguagePrimitives.ConversionData data = LanguagePrimitives.FigureConversion(args[index].Value, elementType, out flag2);
                        initializers.Add(PSConvertBinder.InvokeConverter(data, args[num4].Expression, elementType, flag2, ExpressionCache.InvariantCulture));
                        num4++;
                        index++;
                    }
                    arguments[num3] = Expression.NewArrayInit(elementType, initializers);
                    break;
                }
                LanguagePrimitives.ConversionData conversion = LanguagePrimitives.FigureConversion(args[index].Value, parameterType, out flag3);
                arguments[index] = PSConvertBinder.InvokeConverter(conversion, args[index].Expression, parameterType, flag3, ExpressionCache.InvariantCulture);
                index++;
            }
            Expression right = Expression.New(method, arguments);
            if (base.CallInfo.ArgumentNames.Any<string>())
            {
                ParameterExpression expr = Expression.Parameter(right.Type);
                List<Expression> expressions = new List<Expression>();
                foreach (string str3 in base.CallInfo.ArgumentNames)
                {
                    Expression expression3;
                    Type propertyType;
                    bool flag4;
                    MemberInfo[] infoArray3 = type.GetMember(str3, MemberTypes.Property | MemberTypes.Field, BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                    if ((infoArray3.Length != 1) || (!(infoArray3[0] is PropertyInfo) && !(infoArray3[0] is FieldInfo)))
                    {
                        return target.ThrowRuntimeError(args, BindingRestrictions.Empty, "PropertyNotFoundForType", ParserStrings.PropertyNotFoundForType, new Expression[] { Expression.Constant(str3), Expression.Constant(type) });
                    }
                    MemberInfo info2 = infoArray3[0];
                    if (info2 is PropertyInfo)
                    {
                        PropertyInfo property = (PropertyInfo) info2;
                        if (property.GetSetMethod() == null)
                        {
                            return target.ThrowRuntimeError(args, BindingRestrictions.Empty, "PropertyIsReadOnly", ParserStrings.PropertyIsReadOnly, new Expression[] { Expression.Constant(str3) });
                        }
                        propertyType = property.PropertyType;
                        expression3 = Expression.Property(expr.Cast(info2.DeclaringType), property);
                    }
                    else
                    {
                        propertyType = ((FieldInfo) info2).FieldType;
                        expression3 = Expression.Field(expr.Cast(info2.DeclaringType), (FieldInfo) info2);
                    }
                    LanguagePrimitives.ConversionData data3 = LanguagePrimitives.FigureConversion(args[index].Value, propertyType, out flag4);
                    if (data3.Rank == ConversionRank.None)
                    {
                        return PSConvertBinder.ThrowNoConversion(args[index], propertyType, this, -1, args.Except<DynamicMetaObject>(new DynamicMetaObject[] { args[index] }).Prepend<DynamicMetaObject>(target).ToArray<DynamicMetaObject>());
                    }
                    expressions.Add(Expression.Assign(expression3, PSConvertBinder.InvokeConverter(data3, args[index].Expression, propertyType, flag4, ExpressionCache.InvariantCulture)));
                    index++;
                }
                ParameterExpression variable = Expression.Parameter(typeof(Exception));
                BlockExpression expression5 = Expression.Block(Expression.Assign(expr, right), Expression.TryCatch(Expression.Block(typeof(void), expressions), new CatchBlock[] { Expression.Catch(variable, Expression.Block(Expression.Call(CachedReflectionInfo.CommandProcessorBase_CheckForSevereException, variable), Compiler.ThrowRuntimeErrorWithInnerException("PropertyAssignmentException", Expression.Property(variable, "Message"), variable, typeof(void), new Expression[0]))) }), expr);
                right = Expression.Block(new ParameterExpression[] { expr }, new Expression[] { expression5 });
            }
            return new DynamicMetaObject(right, target.CombineRestrictions(args));
        }

        internal static PSAttributeGenerator Get(CallInfo callInfo)
        {
            lock (_binderCache)
            {
                PSAttributeGenerator generator;
                if (!_binderCache.TryGetValue(callInfo, out generator))
                {
                    generator = new PSAttributeGenerator(callInfo);
                    _binderCache.Add(callInfo, generator);
                }
                return generator;
            }
        }
    }
}

