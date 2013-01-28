namespace System.Data.Services.Client
{
    using System;
    using System.Collections.Generic;
    using System.Data.Services.Client.Materialization;
    using System.Data.Services.Client.Metadata;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    internal class ProjectionPlanCompiler : ALinqExpressionVisitor
    {
        private readonly Dictionary<Expression, ExpressionAnnotation> annotations = new Dictionary<Expression, ExpressionAnnotation>(ReferenceEqualityComparer<Expression>.Instance);
        private static readonly DynamicProxyMethodGenerator dynamicProxyMethodGenerator = new DynamicProxyMethodGenerator();
        private int identifierId;
        private readonly ParameterExpression materializerExpression = Expression.Parameter(typeof(object), "mat");
        private readonly Dictionary<Expression, Expression> normalizerRewrites;
        private ProjectionPathBuilder pathBuilder;
        private bool topLevelProjectionFound;

        private ProjectionPlanCompiler(Dictionary<Expression, Expression> normalizerRewrites)
        {
            this.normalizerRewrites = normalizerRewrites;
            this.pathBuilder = new ProjectionPathBuilder();
        }

        private Expression CallCheckValueForPathIsNull(Expression entry, Expression entryType, ProjectionPath path)
        {
            Expression key = CallMaterializer("ProjectionCheckValueForPathIsNull", new Expression[] { entry, entryType, Expression.Constant(path, typeof(object)) });
            ExpressionAnnotation annotation = new ExpressionAnnotation {
                Segment = path[path.Count - 1]
            };
            this.annotations.Add(key, annotation);
            return key;
        }

        private static Expression CallMaterializer(string methodName, params Expression[] arguments)
        {
            return CallMaterializerWithType(methodName, null, arguments);
        }

        private static Expression CallMaterializerWithType(string methodName, Type[] typeArguments, params Expression[] arguments)
        {
            MethodInfo method = typeof(ODataEntityMaterializerInvoker).GetMethod(methodName, false, true);
            if (typeArguments != null)
            {
                method = method.MakeGenericMethod(typeArguments);
            }
            return dynamicProxyMethodGenerator.GetCallWrapper(method, arguments);
        }

        private Expression CallValueForPath(Expression entry, Expression entryType, ProjectionPath path)
        {
            Expression key = CallMaterializer("ProjectionValueForPath", new Expression[] { this.materializerExpression, entry, entryType, Expression.Constant(path, typeof(object)) });
            ExpressionAnnotation annotation = new ExpressionAnnotation {
                Segment = path[path.Count - 1]
            };
            this.annotations.Add(key, annotation);
            return key;
        }

        private Expression CallValueForPathWithType(Expression entry, Expression entryType, ProjectionPath path, Type type)
        {
            Expression key = Expression.Convert(this.CallValueForPath(entry, entryType, path), type);
            ExpressionAnnotation annotation = new ExpressionAnnotation {
                Segment = path[path.Count - 1]
            };
            this.annotations.Add(key, annotation);
            return key;
        }

        internal static ProjectionPlan CompilePlan(LambdaExpression projection, Dictionary<Expression, Expression> normalizerRewrites)
        {
            Expression expression = new ProjectionPlanCompiler(normalizerRewrites).Visit(projection);
            return new ProjectionPlan { Plan = (Func<object, object, Type, object>) ((LambdaExpression) expression).Compile(), ProjectedType = projection.Body.Type };
        }

        private Expression GetDeepestEntry(Expression[] path)
        {
            Expression expression = null;
            int index = 1;
            do
            {
                Expression[] arguments = new Expression[] { expression ?? this.pathBuilder.ParameterEntryInScope, Expression.Constant(((MemberExpression) path[index]).Member.Name, typeof(string)) };
                expression = CallMaterializer("ProjectionGetEntry", arguments);
                index++;
            }
            while (index < path.Length);
            return expression;
        }

        private Expression GetExpressionBeforeNormalization(Expression expression)
        {
            Expression expression2;
            if ((this.normalizerRewrites != null) && this.normalizerRewrites.TryGetValue(expression, out expression2))
            {
                expression = expression2;
            }
            return expression;
        }

        private Expression RebindConditionalNullCheck(ConditionalExpression conditional, ResourceBinder.PatternRules.MatchNullCheckResult nullCheck)
        {
            ExpressionAnnotation annotation;
            Expression key = this.Visit(nullCheck.TestToNullExpression);
            Expression expression2 = this.Visit(nullCheck.AssignExpression);
            if (!this.annotations.TryGetValue(key, out annotation))
            {
                return base.VisitConditional(conditional);
            }
            ProjectionPathSegment segment = annotation.Segment;
            Expression test = this.CallCheckValueForPathIsNull(segment.StartPath.RootEntry, segment.StartPath.ExpectedRootType, segment.StartPath);
            Expression ifTrue = Expression.Constant(null, expression2.Type);
            Expression ifFalse = expression2;
            return Expression.Condition(test, ifTrue, ifFalse);
        }

        private static Expression RebindConstructor(ConstructorInfo info, params Expression[] arguments)
        {
            return dynamicProxyMethodGenerator.GetCallWrapper(info, arguments);
        }

        private Expression RebindEntityMemberInit(MemberInitExpression init)
        {
            Expression[] expressionsToTargetEntity;
            Expression deepestEntry;
            Expression expectedParamTypeInScope;
            ParameterExpression expression5;
            ParameterExpression expression6;
            if (!this.pathBuilder.HasRewrites)
            {
                expressionsToTargetEntity = MemberAssignmentAnalysis.Analyze(this.pathBuilder.LambdaParameterInScope, ((MemberAssignment) init.Bindings[0]).Expression).GetExpressionsToTargetEntity();
            }
            else
            {
                expressionsToTargetEntity = MemberAssignmentAnalysis.EmptyExpressionArray;
            }
            Expression parameterEntryInScope = this.pathBuilder.ParameterEntryInScope;
            List<string> list = new List<string>();
            List<Func<object, object, Type, object>> list2 = new List<Func<object, object, Type, object>>();
            Type type = init.NewExpression.Type;
            Expression expression2 = Expression.Constant(type, typeof(Type));
            string[] names = (from e in expressionsToTargetEntity.Skip<Expression>(1) select ((MemberExpression) e).Member.Name).ToArray<string>();
            if (expressionsToTargetEntity.Length <= 1)
            {
                deepestEntry = this.pathBuilder.ParameterEntryInScope;
                expectedParamTypeInScope = this.pathBuilder.ExpectedParamTypeInScope;
                expression5 = (ParameterExpression) this.pathBuilder.ParameterEntryInScope;
                expression6 = (ParameterExpression) this.pathBuilder.ExpectedParamTypeInScope;
            }
            else
            {
                deepestEntry = this.GetDeepestEntry(expressionsToTargetEntity);
                expectedParamTypeInScope = expression2;
                expression5 = Expression.Parameter(typeof(object), "subentry" + this.identifierId++);
                expression6 = (ParameterExpression) this.pathBuilder.ExpectedParamTypeInScope;
                ProjectionPath path = new ProjectionPath((ParameterExpression) this.pathBuilder.LambdaParameterInScope, this.pathBuilder.ExpectedParamTypeInScope, this.pathBuilder.ParameterEntryInScope, expressionsToTargetEntity.Skip<Expression>(1));
                ExpressionAnnotation annotation = new ExpressionAnnotation {
                    Segment = path[path.Count - 1]
                };
                this.annotations.Add(deepestEntry, annotation);
                ExpressionAnnotation annotation2 = new ExpressionAnnotation {
                    Segment = path[path.Count - 1]
                };
                this.annotations.Add(expression5, annotation2);
                this.pathBuilder.RegisterRewrite(this.pathBuilder.LambdaParameterInScope, names, expression5);
            }
            for (int i = 0; i < init.Bindings.Count; i++)
            {
                LambdaExpression expression7;
                MemberAssignment assignment = (MemberAssignment) init.Bindings[i];
                list.Add(assignment.Member.Name);
                if (ClientTypeUtil.TypeOrElementTypeIsEntity(ClientTypeUtil.GetMemberType(assignment.Member)) && (assignment.Expression.NodeType == ExpressionType.MemberInit))
                {
                    ProjectionPath path2;
                    ExpressionAnnotation annotation3;
                    Expression expression8 = CallMaterializer("ProjectionGetEntry", new Expression[] { parameterEntryInScope, Expression.Constant(assignment.Member.Name, typeof(string)) });
                    ParameterExpression key = Expression.Parameter(typeof(object), "subentry" + this.identifierId++);
                    if (this.annotations.TryGetValue(this.pathBuilder.ParameterEntryInScope, out annotation3))
                    {
                        path2 = new ProjectionPath((ParameterExpression) this.pathBuilder.LambdaParameterInScope, this.pathBuilder.ExpectedParamTypeInScope, parameterEntryInScope);
                        path2.AddRange(annotation3.Segment.StartPath);
                    }
                    else
                    {
                        path2 = new ProjectionPath((ParameterExpression) this.pathBuilder.LambdaParameterInScope, this.pathBuilder.ExpectedParamTypeInScope, parameterEntryInScope, expressionsToTargetEntity.Skip<Expression>(1));
                    }
                    Type reflectedType = assignment.Member.ReflectedType;
                    ProjectionPathSegment item = new ProjectionPathSegment(path2, assignment.Member.Name, reflectedType);
                    path2.Add(item);
                    string[] strArray2 = (from m in path2
                        where m.Member != null
                        select m.Member).ToArray<string>();
                    ExpressionAnnotation annotation4 = new ExpressionAnnotation {
                        Segment = item
                    };
                    this.annotations.Add(key, annotation4);
                    this.pathBuilder.RegisterRewrite(this.pathBuilder.LambdaParameterInScope, strArray2, key);
                    Expression expression = this.Visit(assignment.Expression);
                    this.pathBuilder.RevokeRewrite(this.pathBuilder.LambdaParameterInScope, strArray2);
                    this.annotations.Remove(key);
                    expression = Expression.Convert(expression, typeof(object));
                    ParameterExpression[] parameters = new ParameterExpression[] { this.materializerExpression, key, expression6 };
                    expression7 = Expression.Lambda(expression, parameters);
                    Expression[] arguments = new Expression[] { this.materializerExpression, expression8, expression6 };
                    ParameterExpression[] expressionArray4 = new ParameterExpression[] { this.materializerExpression, (ParameterExpression) parameterEntryInScope, expression6 };
                    expression7 = Expression.Lambda(Expression.Invoke(expression7, arguments), expressionArray4);
                }
                else
                {
                    Expression body = Expression.Convert(this.Visit(assignment.Expression), typeof(object));
                    ParameterExpression[] expressionArray5 = new ParameterExpression[] { this.materializerExpression, expression5, expression6 };
                    expression7 = Expression.Lambda(body, expressionArray5);
                }
                list2.Add((Func<object, object, Type, object>) expression7.Compile());
            }
            for (int j = 1; j < expressionsToTargetEntity.Length; j++)
            {
                this.pathBuilder.RevokeRewrite(this.pathBuilder.LambdaParameterInScope, names);
                this.annotations.Remove(deepestEntry);
                this.annotations.Remove(expression5);
            }
            return Expression.Convert(CallMaterializer("ProjectionInitializeEntity", new Expression[] { this.materializerExpression, deepestEntry, expectedParamTypeInScope, expression2, Expression.Constant(list.ToArray()), Expression.Constant(list2.ToArray()) }), type);
        }

        private Expression RebindMemberAccess(MemberExpression m, ExpressionAnnotation baseAnnotation)
        {
            Expression expression = m.Expression;
            Expression rewrite = this.pathBuilder.GetRewrite(expression);
            if (rewrite != null)
            {
                Expression expectedRootType = Expression.Constant(expression.Type, typeof(Type));
                ProjectionPath startPath = new ProjectionPath(rewrite as ParameterExpression, expectedRootType, rewrite);
                ProjectionPathSegment segment2 = new ProjectionPathSegment(startPath, m);
                startPath.Add(segment2);
                return this.CallValueForPathWithType(rewrite, expectedRootType, startPath, m.Type);
            }
            ProjectionPathSegment item = new ProjectionPathSegment(baseAnnotation.Segment.StartPath, m);
            baseAnnotation.Segment.StartPath.Add(item);
            return this.CallValueForPathWithType(baseAnnotation.Segment.StartPath.RootEntry, baseAnnotation.Segment.StartPath.ExpectedRootType, baseAnnotation.Segment.StartPath, m.Type);
        }

        private Expression RebindMethodCallForMemberSelect(MethodCallExpression call)
        {
            Expression key = null;
            ExpressionAnnotation annotation;
            Expression expression2 = this.Visit(call.Arguments[0]);
            this.annotations.TryGetValue(expression2, out annotation);
            if (annotation != null)
            {
                LambdaExpression expression3 = call.Arguments[1] as LambdaExpression;
                ParameterExpression expression4 = expression3.Parameters.Last<ParameterExpression>();
                Expression expression5 = this.Visit(call.Arguments[1]);
                if (ClientTypeUtil.TypeOrElementTypeIsEntity(expression4.Type))
                {
                    Type type = call.Method.ReturnType.GetGenericArguments()[0];
                    key = CallMaterializer("ProjectionSelect", new Expression[] { this.materializerExpression, this.pathBuilder.ParameterEntryInScope, this.pathBuilder.ExpectedParamTypeInScope, Expression.Constant(type, typeof(Type)), Expression.Constant(annotation.Segment.StartPath, typeof(object)), expression5 });
                    this.annotations.Add(key, annotation);
                    key = CallMaterializerWithType("EnumerateAsElementType", new Type[] { type }, new Expression[] { key });
                    this.annotations.Add(key, annotation);
                }
                else
                {
                    key = Expression.Call(call.Method, expression2, expression5);
                    this.annotations.Add(key, annotation);
                }
            }
            if (key == null)
            {
                key = base.VisitMethodCall(call);
            }
            return key;
        }

        private Expression RebindMethodCallForMemberToList(MethodCallExpression call)
        {
            ExpressionAnnotation annotation;
            Expression key = this.Visit(call.Arguments[0]);
            if (this.annotations.TryGetValue(key, out annotation))
            {
                key = this.TypedEnumerableToList(key, call.Type);
                this.annotations.Add(key, annotation);
            }
            return key;
        }

        private Expression RebindMethodCallForNewSequence(MethodCallExpression call)
        {
            Expression key = null;
            if (call.Method.Name == "Select")
            {
                ExpressionAnnotation annotation;
                Expression expression2 = this.Visit(call.Arguments[0]);
                this.annotations.TryGetValue(expression2, out annotation);
                if (annotation != null)
                {
                    LambdaExpression expression3 = call.Arguments[1] as LambdaExpression;
                    ParameterExpression expression4 = expression3.Parameters.Last<ParameterExpression>();
                    Expression expression5 = this.Visit(call.Arguments[1]);
                    if (ClientTypeUtil.TypeOrElementTypeIsEntity(expression4.Type))
                    {
                        Type type = call.Method.ReturnType.GetGenericArguments()[0];
                        key = CallMaterializer("ProjectionSelect", new Expression[] { this.materializerExpression, this.pathBuilder.ParameterEntryInScope, this.pathBuilder.ExpectedParamTypeInScope, Expression.Constant(type, typeof(Type)), Expression.Constant(annotation.Segment.StartPath, typeof(object)), expression5 });
                        this.annotations.Add(key, annotation);
                        key = CallMaterializerWithType("EnumerateAsElementType", new Type[] { type }, new Expression[] { key });
                        this.annotations.Add(key, annotation);
                    }
                    else
                    {
                        key = Expression.Call(call.Method, expression2, expression5);
                        this.annotations.Add(key, annotation);
                    }
                }
            }
            else
            {
                ExpressionAnnotation annotation2;
                Expression expression6 = this.Visit(call.Arguments[0]);
                if (this.annotations.TryGetValue(expression6, out annotation2))
                {
                    key = this.TypedEnumerableToList(expression6, call.Type);
                    this.annotations.Add(key, annotation2);
                }
            }
            if (key == null)
            {
                key = base.VisitMethodCall(call);
            }
            return key;
        }

        private Expression RebindNewExpressionForDataServiceCollectionOfT(NewExpression nex)
        {
            NewExpression key = this.VisitNew(nex);
            Expression expression2 = null;
            ExpressionAnnotation annotation = null;
            if (key != null)
            {
                ConstructorInfo info = nex.Type.GetInstanceConstructors(false).First<ConstructorInfo>(c => (c.GetParameters().Length == 7) && (c.GetParameters()[0].ParameterType == typeof(object)));
                Type type = typeof(IEnumerable<>).MakeGenericType(new Type[] { nex.Type.GetGenericArguments()[0] });
                if (((key.Arguments.Count == 1) && (key.Constructor == nex.Type.GetInstanceConstructor(true, new Type[] { type }))) && this.annotations.TryGetValue(key.Arguments[0], out annotation))
                {
                    expression2 = RebindConstructor(info, new Expression[] { this.materializerExpression, Expression.Constant(null, typeof(DataServiceContext)), key.Arguments[0], Expression.Constant(TrackingMode.AutoChangeTracking, typeof(TrackingMode)), Expression.Constant(null, typeof(string)), Expression.Constant(null, typeof(Func<EntityChangedParams, bool>)), Expression.Constant(null, typeof(Func<EntityCollectionChangedParams, bool>)) });
                }
                else if ((key.Arguments.Count == 2) && this.annotations.TryGetValue(key.Arguments[0], out annotation))
                {
                    expression2 = RebindConstructor(info, new Expression[] { this.materializerExpression, Expression.Constant(null, typeof(DataServiceContext)), key.Arguments[0], key.Arguments[1], Expression.Constant(null, typeof(string)), Expression.Constant(null, typeof(Func<EntityChangedParams, bool>)), Expression.Constant(null, typeof(Func<EntityCollectionChangedParams, bool>)) });
                }
                else if ((key.Arguments.Count == 5) && this.annotations.TryGetValue(key.Arguments[0], out annotation))
                {
                    expression2 = RebindConstructor(info, new Expression[] { this.materializerExpression, Expression.Constant(null, typeof(DataServiceContext)), key.Arguments[0], key.Arguments[1], key.Arguments[2], key.Arguments[3], key.Arguments[4] });
                }
                else if (((key.Arguments.Count == 6) && typeof(DataServiceContext).IsAssignableFrom(key.Arguments[0].Type)) && this.annotations.TryGetValue(key.Arguments[1], out annotation))
                {
                    expression2 = RebindConstructor(info, new Expression[] { this.materializerExpression, key.Arguments[0], key.Arguments[1], key.Arguments[2], key.Arguments[3], key.Arguments[4], key.Arguments[5] });
                }
            }
            if (annotation != null)
            {
                this.annotations.Add(key, annotation);
            }
            return expression2;
        }

        private Expression RebindParameter(Expression expression, ExpressionAnnotation annotation)
        {
            Expression expression2 = this.CallValueForPathWithType(annotation.Segment.StartPath.RootEntry, annotation.Segment.StartPath.ExpectedRootType, annotation.Segment.StartPath, expression.Type);
            ProjectionPath startPath = new ProjectionPath(annotation.Segment.StartPath.Root, annotation.Segment.StartPath.ExpectedRootType, annotation.Segment.StartPath.RootEntry);
            ProjectionPathSegment item = new ProjectionPathSegment(startPath, null, null);
            startPath.Add(item);
            ExpressionAnnotation annotation2 = new ExpressionAnnotation {
                Segment = item
            };
            this.annotations[expression] = annotation2;
            return expression2;
        }

        private Expression TypedEnumerableToList(Expression source, Type targetType)
        {
            Type type = source.Type.GetGenericArguments()[0];
            Type type2 = targetType.GetGenericArguments()[0];
            return CallMaterializerWithType("ListAsElementType", new Type[] { type, type2 }, new Expression[] { this.materializerExpression, source });
        }

        internal override Expression Visit(Expression exp)
        {
            if (exp == null)
            {
                return exp;
            }
            if (exp.NodeType != ExpressionType.New)
            {
                return base.Visit(exp);
            }
            NewExpression nex = (NewExpression) exp;
            if (ResourceBinder.PatternRules.MatchNewDataServiceCollectionOfT(nex))
            {
                return this.RebindNewExpressionForDataServiceCollectionOfT(nex);
            }
            return this.VisitNew(nex);
        }

        internal override Expression VisitBinary(BinaryExpression b)
        {
            Expression expressionBeforeNormalization = this.GetExpressionBeforeNormalization(b);
            if (expressionBeforeNormalization == b)
            {
                return base.VisitBinary(b);
            }
            return this.Visit(expressionBeforeNormalization);
        }

        internal override Expression VisitConditional(ConditionalExpression conditional)
        {
            Expression expressionBeforeNormalization = this.GetExpressionBeforeNormalization(conditional);
            if (expressionBeforeNormalization != conditional)
            {
                return this.Visit(expressionBeforeNormalization);
            }
            ResourceBinder.PatternRules.MatchNullCheckResult nullCheck = ResourceBinder.PatternRules.MatchNullCheck(this.pathBuilder.LambdaParameterInScope, conditional);
            if (!nullCheck.Match || !ClientTypeUtil.TypeOrElementTypeIsEntity(ResourceBinder.StripConvertToAssignable(nullCheck.TestToNullExpression).Type))
            {
                Expression test = null;
                if (nullCheck.Match)
                {
                    Expression left = this.Visit(nullCheck.TestToNullExpression);
                    if (left.NodeType == ExpressionType.Convert)
                    {
                        left = ((UnaryExpression) left).Operand;
                    }
                    test = Expression.MakeBinary(ExpressionType.Equal, left, Expression.Constant(null));
                }
                if (test == null)
                {
                    test = this.Visit(conditional.Test);
                }
                Expression ifTrue = this.Visit(conditional.IfTrue);
                Expression ifFalse = this.Visit(conditional.IfFalse);
                if (((test != conditional.Test) || (ifTrue != conditional.IfTrue)) || (ifFalse != conditional.IfFalse))
                {
                    return Expression.Condition(test, ifTrue, ifFalse, ifTrue.Type.IsAssignableFrom(ifFalse.Type) ? ifTrue.Type : ifFalse.Type);
                }
            }
            return this.RebindConditionalNullCheck(conditional, nullCheck);
        }

        internal override Expression VisitLambda(LambdaExpression lambda)
        {
            if (!this.topLevelProjectionFound || ((lambda.Parameters.Count == 1) && ClientTypeUtil.TypeOrElementTypeIsEntity(lambda.Parameters[0].Type)))
            {
                this.topLevelProjectionFound = true;
                ParameterExpression expectedType = Expression.Parameter(typeof(Type), "type" + this.identifierId);
                ParameterExpression entry = Expression.Parameter(typeof(object), "entry" + this.identifierId);
                this.identifierId++;
                this.pathBuilder.EnterLambdaScope(lambda, entry, expectedType);
                ProjectionPath startPath = new ProjectionPath(lambda.Parameters[0], expectedType, entry);
                ProjectionPathSegment item = new ProjectionPathSegment(startPath, null, null);
                startPath.Add(item);
                ExpressionAnnotation annotation = new ExpressionAnnotation {
                    Segment = item
                };
                this.annotations[lambda.Parameters[0]] = annotation;
                Expression expression4 = this.Visit(lambda.Body);
                if (expression4.Type.IsValueType)
                {
                    expression4 = Expression.Convert(expression4, typeof(object));
                }
                Expression expression = Expression.Lambda<Func<object, object, Type, object>>(expression4, new ParameterExpression[] { this.materializerExpression, entry, expectedType });
                this.pathBuilder.LeaveLambdaScope();
                return expression;
            }
            return base.VisitLambda(lambda);
        }

        internal override Expression VisitMemberAccess(MemberExpression m)
        {
            ExpressionAnnotation annotation;
            Expression expression = m.Expression;
            if (PrimitiveType.IsKnownNullableType(expression.Type))
            {
                return base.VisitMemberAccess(m);
            }
            Expression key = this.Visit(expression);
            if (this.annotations.TryGetValue(key, out annotation))
            {
                return this.RebindMemberAccess(m, annotation);
            }
            return Expression.MakeMemberAccess(key, m.Member);
        }

        internal override Expression VisitMemberInit(MemberInitExpression init)
        {
            this.pathBuilder.EnterMemberInit(init);
            Expression expression = null;
            if (this.pathBuilder.CurrentIsEntity && (init.Bindings.Count > 0))
            {
                expression = this.RebindEntityMemberInit(init);
            }
            else
            {
                expression = base.VisitMemberInit(init);
            }
            this.pathBuilder.LeaveMemberInit();
            return expression;
        }

        internal override Expression VisitMethodCall(MethodCallExpression m)
        {
            Expression expressionBeforeNormalization = this.GetExpressionBeforeNormalization(m);
            if (expressionBeforeNormalization != m)
            {
                return this.Visit(expressionBeforeNormalization);
            }
            if (this.pathBuilder.CurrentIsEntity)
            {
                if (m.Method.Name == "Select")
                {
                    return this.RebindMethodCallForMemberSelect(m);
                }
                if (m.Method.Name == "ToList")
                {
                    return this.RebindMethodCallForMemberToList(m);
                }
                return base.VisitMethodCall(m);
            }
            if (ProjectionAnalyzer.IsMethodCallAllowedEntitySequence(m))
            {
                return this.RebindMethodCallForNewSequence(m);
            }
            return base.VisitMethodCall(m);
        }

        internal override Expression VisitParameter(ParameterExpression p)
        {
            ExpressionAnnotation annotation;
            if (this.annotations.TryGetValue(p, out annotation))
            {
                return this.RebindParameter(p, annotation);
            }
            return base.VisitParameter(p);
        }

        internal override Expression VisitUnary(UnaryExpression u)
        {
            Expression expressionBeforeNormalization = this.GetExpressionBeforeNormalization(u);
            if (expressionBeforeNormalization == u)
            {
                ExpressionAnnotation annotation;
                Expression expression2 = base.VisitUnary(u);
                UnaryExpression expression3 = expression2 as UnaryExpression;
                if ((expression3 != null) && this.annotations.TryGetValue(expression3.Operand, out annotation))
                {
                    this.annotations[expression2] = annotation;
                }
                return expression2;
            }
            return this.Visit(expressionBeforeNormalization);
        }

        internal class ExpressionAnnotation
        {
            internal ProjectionPathSegment Segment { get; set; }
        }
    }
}

