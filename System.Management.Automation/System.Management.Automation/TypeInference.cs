namespace System.Management.Automation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    internal class TypeInference
    {
        [TraceSource("ETS", "Extended Type System")]
        private static readonly PSTraceSource tracer = PSTraceSource.GetTracer("ETS", "Extended Type System");
        private readonly HashSet<Type>[] typeParameterIndexToSetOfInferenceCandidates;

        internal TypeInference(ICollection<Type> typeParameters)
        {
            this.typeParameterIndexToSetOfInferenceCandidates = new HashSet<Type>[typeParameters.Count];
        }

        internal Type GetInferredType(Type typeParameter)
        {
            Func<Type, bool> predicate = null;
            ICollection<Type> inferenceCandidates = this.typeParameterIndexToSetOfInferenceCandidates[typeParameter.GenericParameterPosition];
            if ((inferenceCandidates != null) && inferenceCandidates.Any<Type>(t => t.Equals(typeof(LanguagePrimitives.Null))))
            {
                Type type = inferenceCandidates.FirstOrDefault<Type>(t => t.IsValueType);
                if (type != null)
                {
                    tracer.WriteLine("Cannot reconcile null and {0} (a value type)", new object[] { type });
                    inferenceCandidates = null;
                    this.typeParameterIndexToSetOfInferenceCandidates[typeParameter.GenericParameterPosition] = null;
                }
                else
                {
                    inferenceCandidates = (from t in inferenceCandidates
                        where !t.Equals(typeof(LanguagePrimitives.Null))
                        select t).ToList<Type>();
                    if (inferenceCandidates.Count == 0)
                    {
                        inferenceCandidates = null;
                        this.typeParameterIndexToSetOfInferenceCandidates[typeParameter.GenericParameterPosition] = null;
                    }
                }
            }
            if ((inferenceCandidates != null) && (inferenceCandidates.Count > 1))
            {
                if (predicate == null)
                {
                    predicate = potentiallyCommonBaseClass => inferenceCandidates.All<Type>(delegate (Type otherCandidate) {
                        if (!otherCandidate.Equals(potentiallyCommonBaseClass))
                        {
                            return potentiallyCommonBaseClass.IsAssignableFrom(otherCandidate);
                        }
                        return true;
                    });
                }
                Type item = inferenceCandidates.Where<Type>(predicate).FirstOrDefault<Type>();
                if (item != null)
                {
                    inferenceCandidates.Clear();
                    inferenceCandidates.Add(item);
                }
                else
                {
                    tracer.WriteLine("Multiple unreconcilable inferences for type parameter {0}", new object[] { typeParameter });
                    inferenceCandidates = null;
                    this.typeParameterIndexToSetOfInferenceCandidates[typeParameter.GenericParameterPosition] = null;
                }
            }
            if (inferenceCandidates == null)
            {
                tracer.WriteLine("Couldn't infer type parameter {0}", new object[] { typeParameter });
                return null;
            }
            return inferenceCandidates.Single<Type>();
        }

        internal static MethodInformation Infer(MethodInformation genericMethod, Type[] argumentTypes)
        {
            MethodInfo method = (MethodInfo) genericMethod.method;
            MethodInfo info2 = Infer(method, argumentTypes, genericMethod.hasVarArgs);
            if (info2 != null)
            {
                return new MethodInformation(info2, 0);
            }
            return null;
        }

        private static MethodInfo Infer(MethodInfo genericMethod, Type[] typesOfMethodArguments, bool hasVarArgs)
        {
            if (!genericMethod.ContainsGenericParameters)
            {
                return genericMethod;
            }
            Type[] genericArguments = genericMethod.GetGenericArguments();
            Type[] typesOfMethodParameters = (from p in genericMethod.GetParameters() select p.ParameterType).ToArray<Type>();
            MethodInfo info = Infer(genericMethod, genericArguments, typesOfMethodParameters, typesOfMethodArguments);
            if (((info == null) && hasVarArgs) && (typesOfMethodArguments.Length >= (typesOfMethodParameters.Length - 1)))
            {
                IEnumerable<Type> first = typesOfMethodParameters.Take<Type>(typesOfMethodParameters.Length - 1);
                IEnumerable<Type> second = Enumerable.Repeat<Type>(typesOfMethodParameters[typesOfMethodParameters.Length - 1].GetElementType(), (typesOfMethodArguments.Length - typesOfMethodParameters.Length) + 1);
                info = Infer(genericMethod, genericArguments, first.Concat<Type>(second), typesOfMethodArguments);
            }
            return info;
        }

        private static MethodInfo Infer(MethodInfo genericMethod, ICollection<Type> typeParameters, IEnumerable<Type> typesOfMethodParameters, IEnumerable<Type> typesOfMethodArguments)
        {
            MethodInfo info2;
            using (tracer.TraceScope("Inferring type parameters for the following method: {0}", new object[] { genericMethod }))
            {
                if (PSTraceSourceOptions.WriteLine == (tracer.Options & PSTraceSourceOptions.WriteLine))
                {
                    object[] args = new object[] { string.Join(", ", (from t in typesOfMethodArguments select t.ToString()).ToArray<string>()) };
                    tracer.WriteLine("Types of method arguments: {0}", args);
                }
                TypeInference inference = new TypeInference(typeParameters);
                if (!inference.UnifyMultipleTerms(typesOfMethodParameters, typesOfMethodArguments))
                {
                    return null;
                }
                IEnumerable<Type> source = typeParameters.Select<Type, Type>(new Func<Type, Type>(inference.GetInferredType));
                if (source.Any<Type>(inferredType => inferredType == null))
                {
                    info2 = null;
                }
                else
                {
                    try
                    {
                        MethodInfo info = genericMethod.MakeGenericMethod(source.ToArray<Type>());
                        tracer.WriteLine("Inference succesful: {0}", new object[] { info });
                        info2 = info;
                    }
                    catch (ArgumentException exception)
                    {
                        tracer.WriteLine("Inference failure: {0}", new object[] { exception.Message });
                        info2 = null;
                    }
                }
            }
            return info2;
        }

        private static bool IsEqualGenericTypeDefinition(Type parameterType, Type argumentType)
        {
            if (!argumentType.IsGenericType)
            {
                return false;
            }
            return parameterType.GetGenericTypeDefinition().Equals(argumentType.GetGenericTypeDefinition());
        }

        private bool Unify(Type parameterType, Type argumentType)
        {
            if (!parameterType.ContainsGenericParameters)
            {
                return true;
            }
            if (parameterType.IsGenericParameter)
            {
                HashSet<Type> set = this.typeParameterIndexToSetOfInferenceCandidates[parameterType.GenericParameterPosition];
                if (set == null)
                {
                    set = new HashSet<Type>();
                    this.typeParameterIndexToSetOfInferenceCandidates[parameterType.GenericParameterPosition] = set;
                }
                set.Add(argumentType);
                tracer.WriteLine("Inferred {0} => {1}", new object[] { parameterType, argumentType });
                return true;
            }
            if (parameterType.IsArray)
            {
                if (argumentType.Equals(typeof(LanguagePrimitives.Null)))
                {
                    return true;
                }
                if (argumentType.IsArray && (parameterType.GetArrayRank() == argumentType.GetArrayRank()))
                {
                    return this.Unify(parameterType.GetElementType(), argumentType.GetElementType());
                }
                tracer.WriteLine("Couldn't unify array {0} with {1}", new object[] { parameterType, argumentType });
                return false;
            }
            if (parameterType.IsByRef)
            {
                if (argumentType.IsGenericType && argumentType.GetGenericTypeDefinition().Equals(typeof(PSReference<>)))
                {
                    Type type = argumentType.GetGenericArguments()[0];
                    return (type.Equals(typeof(LanguagePrimitives.Null)) || this.Unify(parameterType.GetElementType(), type));
                }
                tracer.WriteLine("Couldn't unify reference type {0} with {1}", new object[] { parameterType, argumentType });
                return false;
            }
            if (parameterType.IsGenericType && parameterType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                return (argumentType.Equals(typeof(LanguagePrimitives.Null)) || this.Unify(parameterType.GetGenericArguments()[0], argumentType));
            }
            if (parameterType.IsGenericType)
            {
                return (argumentType.Equals(typeof(LanguagePrimitives.Null)) || this.UnifyConstructedType(parameterType, argumentType));
            }
            tracer.WriteLine("Unrecognized kind of type: {0}", new object[] { parameterType });
            return false;
        }

        private bool UnifyConstructedType(Type parameterType, Type argumentType)
        {
            if (IsEqualGenericTypeDefinition(parameterType, argumentType))
            {
                IEnumerable<Type> genericArguments = parameterType.GetGenericArguments();
                IEnumerable<Type> argumentTypes = argumentType.GetGenericArguments();
                return this.UnifyMultipleTerms(genericArguments, argumentTypes);
            }
            foreach (Type type in argumentType.GetInterfaces())
            {
                if (IsEqualGenericTypeDefinition(parameterType, type))
                {
                    return this.UnifyConstructedType(parameterType, type);
                }
            }
            for (Type type2 = argumentType.BaseType; type2 != null; type2 = type2.BaseType)
            {
                if (IsEqualGenericTypeDefinition(parameterType, type2))
                {
                    return this.UnifyConstructedType(parameterType, type2);
                }
            }
            tracer.WriteLine("Attempt to unify different constructed types: {0} and {1}", new object[] { parameterType, argumentType });
            return false;
        }

        internal bool UnifyMultipleTerms(IEnumerable<Type> parameterTypes, IEnumerable<Type> argumentTypes)
        {
            List<Type> list = parameterTypes.ToList<Type>();
            List<Type> list2 = argumentTypes.ToList<Type>();
            if (list.Count != list2.Count)
            {
                tracer.WriteLine("Mismatch in number of parameters and arguments", new object[0]);
                return false;
            }
            for (int i = 0; i < list.Count; i++)
            {
                if (!this.Unify(list[i], list2[i]))
                {
                    tracer.WriteLine("Couldn't unify {0} with {1}", new object[] { list[i], list2[i] });
                    return false;
                }
            }
            return true;
        }
    }
}

