namespace System.Data.Services.Parsing
{
    using System;
    using System.Collections.Generic;
    using System.Data.Services;
    using System.Data.Services.Providers;
    using System.Diagnostics;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Spatial;
    using System.Text;

    [DebuggerDisplay("FunctionDescription {name}")]
    internal class FunctionDescription
    {
        private Func<Expression, Expression[], Expression> conversionFunction;
        private const string FunctionNameCast = "cast";
        private const string FunctionNameIsOf = "isof";
        private readonly MemberInfo member;
        private readonly string name;
        private readonly Type[] parameterTypes;

        public FunctionDescription(MemberInfo member, Type[] parameterTypes) : this(member, parameterTypes, null, member.Name)
        {
        }

        public FunctionDescription(string name, Type[] parameterTypes, Func<Expression, Expression[], Expression> conversionFunction) : this(null, parameterTypes, conversionFunction, name)
        {
        }

        private FunctionDescription(MemberInfo member, Type[] parameterTypes, Func<Expression, Expression[], Expression> conversionFunction, string name)
        {
            this.member = member;
            this.parameterTypes = parameterTypes;
            this.conversionFunction = conversionFunction;
            this.name = name;
            this.IsTypeCast = name == "cast";
            this.IsTypeCheckOrCast = this.IsTypeCast || (name == "isof");
            this.RequiresNullPropagation = !this.IsTypeCheckOrCast;
        }

        internal static Expression BinaryCast(Expression target, Expression[] arguments)
        {
            ConstantExpression expression = (ConstantExpression) arguments[1];
            Type typeAllowingNull = (Type) expression.Value;
            if (ExpressionUtils.IsNullConstant(arguments[0]))
            {
                typeAllowingNull = WebUtil.GetTypeAllowingNull(typeAllowingNull);
                return Expression.Constant(null, typeAllowingNull);
            }
            return Expression.Convert(arguments[0], typeAllowingNull);
        }

        internal static Expression BinaryCastResourceType(Expression target, Expression[] arguments)
        {
            bool flag = (bool) ((ConstantExpression) arguments[2]).Value;
            return Expression.Call(null, flag ? OpenTypeMethods.ConvertMethodInfo : DataServiceProviderMethods.ConvertMethodInfo, arguments[0], arguments[1]);
        }

        internal static Expression BinaryIsOf(Expression target, Expression[] arguments)
        {
            ConstantExpression expression = (ConstantExpression) arguments[1];
            return Expression.TypeIs(arguments[0], (Type) expression.Value);
        }

        internal static Expression BinaryIsOfResourceType(Expression target, Expression[] arguments)
        {
            bool flag = (bool) ((ConstantExpression) arguments[2]).Value;
            return Expression.Call(null, flag ? OpenTypeMethods.TypeIsMethodInfo : DataServiceProviderMethods.TypeIsMethodInfo, arguments[0], arguments[1]);
        }

        internal static string BuildSignatureList(string name, IEnumerable<FunctionDescription> descriptions)
        {
            StringBuilder builder = new StringBuilder();
            string str = string.Empty;
            foreach (FunctionDescription description in descriptions)
            {
                builder.Append(str);
                str = "; ";
                string str2 = string.Empty;
                builder.Append(name);
                builder.Append('(');
                foreach (Type type in description.ParameterTypes)
                {
                    builder.Append(str2);
                    str2 = ", ";
                    Type underlyingType = Nullable.GetUnderlyingType(type);
                    if (underlyingType != null)
                    {
                        builder.Append(underlyingType.FullName);
                        builder.Append('?');
                    }
                    else
                    {
                        builder.Append(type.FullName);
                    }
                }
                builder.Append(')');
            }
            return builder.ToString();
        }

        private static FunctionDescription CreateFunctionDescription(Type targetType, bool instance, bool method, string name, params Type[] parameterTypes)
        {
            MemberInfo property;
            Type[] typeArray;
            BindingFlags bindingAttr = BindingFlags.Public | (instance ? BindingFlags.Instance : BindingFlags.Static);
            if (method)
            {
                property = targetType.GetMethod(name, bindingAttr, null, parameterTypes, null);
            }
            else
            {
                property = targetType.GetProperty(name, bindingAttr);
            }
            if (instance)
            {
                typeArray = new Type[parameterTypes.Length + 1];
                typeArray[0] = targetType;
                parameterTypes.CopyTo(typeArray, 1);
            }
            else
            {
                typeArray = parameterTypes;
            }
            FunctionDescription description = new FunctionDescription(property, typeArray);
            if (method)
            {
                if (instance)
                {
                    description.ConversionFunction = new Func<Expression, Expression[], Expression>(description.InstanceMethodConversionFunction);
                    return description;
                }
                description.ConversionFunction = new Func<Expression, Expression[], Expression>(description.StaticMethodConversionFunction);
                return description;
            }
            description.ConversionFunction = new Func<Expression, Expression[], Expression>(description.InstancePropertyConversionFunction);
            return description;
        }

        internal static Dictionary<string, FunctionDescription[]> CreateFunctions()
        {
            Dictionary<string, FunctionDescription[]> dictionary = new Dictionary<string, FunctionDescription[]>(StringComparer.Ordinal);
            dictionary.Add("endswith", new FunctionDescription[] { StringInstanceFunction("EndsWith", new Type[] { typeof(string) }) });
            dictionary.Add("indexof", new FunctionDescription[] { StringInstanceFunction("IndexOf", new Type[] { typeof(string) }) });
            dictionary.Add("replace", new FunctionDescription[] { StringInstanceFunction("Replace", new Type[] { typeof(string), typeof(string) }) });
            dictionary.Add("startswith", new FunctionDescription[] { StringInstanceFunction("StartsWith", new Type[] { typeof(string) }) });
            dictionary.Add("tolower", new FunctionDescription[] { StringInstanceFunction("ToLower", Type.EmptyTypes) });
            dictionary.Add("toupper", new FunctionDescription[] { StringInstanceFunction("ToUpper", Type.EmptyTypes) });
            dictionary.Add("trim", new FunctionDescription[] { StringInstanceFunction("Trim", Type.EmptyTypes) });
            FunctionDescription[] descriptionArray = new FunctionDescription[] { StringInstanceFunction("Substring", new Type[] { typeof(int) }), StringInstanceFunction("Substring", new Type[] { typeof(int), typeof(int) }) };
            dictionary.Add("substring", descriptionArray);
            descriptionArray = new FunctionDescription[] { new FunctionDescription("SubstringOf", new Type[] { typeof(string), typeof(string) }, new Func<Expression, Expression[], Expression>(FunctionDescription.SubstringOf)) };
            dictionary.Add("substringof", descriptionArray);
            descriptionArray = new FunctionDescription[] { CreateFunctionDescription(typeof(string), false, true, "Concat", new Type[] { typeof(string), typeof(string) }) };
            dictionary.Add("concat", descriptionArray);
            descriptionArray = new FunctionDescription[] { CreateFunctionDescription(typeof(string), true, false, "Length", Type.EmptyTypes) };
            dictionary.Add("length", descriptionArray);
            descriptionArray = new FunctionDescription[] { CreatePropertyBasedFunction(typeof(DateTime), "Year"), CreatePropertyBasedFunction(typeof(DateTimeOffset), "Year") };
            dictionary.Add("year", descriptionArray);
            descriptionArray = new FunctionDescription[] { CreatePropertyBasedFunction(typeof(DateTime), "Month"), CreatePropertyBasedFunction(typeof(DateTimeOffset), "Month") };
            dictionary.Add("month", descriptionArray);
            descriptionArray = new FunctionDescription[] { CreatePropertyBasedFunction(typeof(DateTime), "Day"), CreatePropertyBasedFunction(typeof(DateTimeOffset), "Day") };
            dictionary.Add("day", descriptionArray);
            descriptionArray = new FunctionDescription[] { CreatePropertyBasedFunction(typeof(DateTime), "Hour"), CreatePropertyBasedFunction(typeof(DateTimeOffset), "Hour"), CreatePropertyBasedFunction(typeof(TimeSpan), "Hours") };
            dictionary.Add("hour", descriptionArray);
            descriptionArray = new FunctionDescription[] { CreatePropertyBasedFunction(typeof(DateTime), "Minute"), CreatePropertyBasedFunction(typeof(DateTimeOffset), "Minute"), CreatePropertyBasedFunction(typeof(TimeSpan), "Minutes") };
            dictionary.Add("minute", descriptionArray);
            descriptionArray = new FunctionDescription[] { CreatePropertyBasedFunction(typeof(DateTime), "Second"), CreatePropertyBasedFunction(typeof(DateTimeOffset), "Second"), CreatePropertyBasedFunction(typeof(TimeSpan), "Seconds") };
            dictionary.Add("second", descriptionArray);
            dictionary.Add("round", MathFunctionArray("Round"));
            dictionary.Add("floor", MathFunctionArray("Floor"));
            dictionary.Add("ceiling", MathFunctionArray("Ceiling"));
            descriptionArray = new FunctionDescription[] { new FunctionDescription("isof", new Type[] { typeof(Type) }, new Func<Expression, Expression[], Expression>(FunctionDescription.UnaryIsOf)), new FunctionDescription("isof", new Type[] { typeof(object), typeof(Type) }, new Func<Expression, Expression[], Expression>(FunctionDescription.BinaryIsOf)), new FunctionDescription("isof", new Type[] { typeof(ResourceType) }, new Func<Expression, Expression[], Expression>(FunctionDescription.UnaryIsOfResourceType)), new FunctionDescription("isof", new Type[] { typeof(object), typeof(ResourceType) }, new Func<Expression, Expression[], Expression>(FunctionDescription.BinaryIsOfResourceType)) };
            dictionary.Add("isof", descriptionArray);
            ResourceType[] allPrimitives = ResourceType.PrimitiveResourceTypeMap.AllPrimitives;
            List<FunctionDescription> list = new List<FunctionDescription>(allPrimitives.Length + 4);
            for (int i = 0; i < allPrimitives.Length; i++)
            {
                list.Add(new FunctionDescription("cast", new Type[] { allPrimitives[i].InstanceType, typeof(Type) }, new Func<Expression, Expression[], Expression>(FunctionDescription.BinaryCast)));
            }
            list.Add(new FunctionDescription("cast", new Type[] { typeof(Type) }, new Func<Expression, Expression[], Expression>(FunctionDescription.UnaryCast)));
            list.Add(new FunctionDescription("cast", new Type[] { typeof(object), typeof(Type) }, new Func<Expression, Expression[], Expression>(FunctionDescription.BinaryCast)));
            list.Add(new FunctionDescription("cast", new Type[] { typeof(ResourceType) }, new Func<Expression, Expression[], Expression>(FunctionDescription.UnaryCastResourceType)));
            list.Add(new FunctionDescription("cast", new Type[] { typeof(object), typeof(ResourceType) }, new Func<Expression, Expression[], Expression>(FunctionDescription.BinaryCastResourceType)));
            dictionary.Add("cast", list.ToArray());
            List<FunctionDescription> list2 = new List<FunctionDescription> {
                CreateFunctionDescription(typeof(GeographyOperationsExtensions), false, true, "Distance", new Type[] { typeof(GeographyPoint), typeof(GeographyPoint) }),
                CreateFunctionDescription(typeof(GeometryOperationsExtensions), false, true, "Distance", new Type[] { typeof(GeometryPoint), typeof(GeometryPoint) })
            };
            foreach (FunctionDescription description in list2)
            {
                description.RequiresNullPropagation = false;
            }
            dictionary.Add("geo.distance", list2.ToArray());
            return dictionary;
        }

        private static FunctionDescription CreatePropertyBasedFunction(Type type, string name)
        {
            return CreateFunctionDescription(type, true, false, name, Type.EmptyTypes);
        }

        public Expression InstanceMethodConversionFunction(Expression target, Expression[] arguments)
        {
            Expression instance = arguments[0];
            Expression[] destinationArray = new Expression[arguments.Length - 1];
            Array.Copy(arguments, 1, destinationArray, 0, arguments.Length - 1);
            return Expression.Call(instance, (MethodInfo) this.member, destinationArray);
        }

        public Expression InstancePropertyConversionFunction(Expression target, Expression[] arguments)
        {
            return Expression.Property(arguments[0], (PropertyInfo) this.member);
        }

        public Expression InvokeOpenTypeMethod(Expression[] arguments)
        {
            Type[] types = new Type[this.parameterTypes.Length];
            for (int i = 0; i < types.Length; i++)
            {
                types[i] = typeof(object);
            }
            MethodInfo method = typeof(OpenTypeMethods).GetMethod(this.name, BindingFlags.Public | BindingFlags.Static, null, types, null);
            return Expression.Call(null, method, arguments);
        }

        private static FunctionDescription[] MathFunctionArray(string name)
        {
            return new FunctionDescription[] { CreateFunctionDescription(typeof(Math), false, true, name, new Type[] { typeof(double) }), CreateFunctionDescription(typeof(Math), false, true, name, new Type[] { typeof(decimal) }) };
        }

        public Expression StaticMethodConversionFunction(Expression target, Expression[] arguments)
        {
            return Expression.Call((MethodInfo) this.member, arguments);
        }

        private static FunctionDescription StringInstanceFunction(string name, params Type[] parameterTypes)
        {
            return CreateFunctionDescription(typeof(string), true, true, name, parameterTypes);
        }

        internal static Expression SubstringOf(Expression target, Expression[] arguments)
        {
            Type[] types = new Type[] { typeof(string) };
            MethodInfo method = typeof(string).GetMethod("Contains", BindingFlags.Public | BindingFlags.Instance, null, types, null);
            return Expression.Call(arguments[1], method, new Expression[] { arguments[0] });
        }

        internal static Expression UnaryCast(Expression target, Expression[] arguments)
        {
            ConstantExpression expression = (ConstantExpression) arguments[0];
            return Expression.Convert(target, (Type) expression.Value);
        }

        internal static Expression UnaryCastResourceType(Expression target, Expression[] arguments)
        {
            return Expression.Call(null, DataServiceProviderMethods.ConvertMethodInfo, target, arguments[0]);
        }

        internal static Expression UnaryIsOf(Expression target, Expression[] arguments)
        {
            ConstantExpression expression = (ConstantExpression) arguments[0];
            return Expression.TypeIs(target, (Type) expression.Value);
        }

        internal static Expression UnaryIsOfResourceType(Expression target, Expression[] arguments)
        {
            return Expression.Call(null, DataServiceProviderMethods.TypeIsMethodInfo, target, arguments[0]);
        }

        public Func<Expression, Expression[], Expression> ConversionFunction
        {
            [DebuggerStepThrough]
            get
            {
                return this.conversionFunction;
            }
            [DebuggerStepThrough]
            set
            {
                this.conversionFunction = value;
            }
        }

        public bool IsTypeCast { get; private set; }

        public bool IsTypeCheckOrCast { get; private set; }

        public Type[] ParameterTypes
        {
            [DebuggerStepThrough]
            get
            {
                return this.parameterTypes;
            }
        }

        public bool RequiresNullPropagation { get; set; }
    }
}

